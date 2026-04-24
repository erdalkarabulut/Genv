using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentModbus;

namespace PlcModbusWorker;

internal static class Program
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };

    public static async Task<int> Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        WorkerConfig cfg = LoadConfig(args);
        if (string.IsNullOrWhiteSpace(cfg.IndustrialApiKey))
        {
            Console.Error.WriteLine("IndustrialApiKey boş. appsettings.json veya --api-key ile verin.");
            return 2;
        }

        var handler = new HttpClientHandler();
        if (cfg.AcceptAnyTlsCertificate)
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        using var http = new HttpClient(handler)
        {
            BaseAddress = new Uri(cfg.ApiBaseUrl.TrimEnd('/') + "/", UriKind.Absolute),
            Timeout = TimeSpan.FromSeconds(60),
        };
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        http.DefaultRequestHeaders.Add("X-Industrial-ApiKey", cfg.IndustrialApiKey);

        Console.WriteLine($"API: {cfg.ApiBaseUrl} | yeniden senkron: her {cfg.ConfigRefreshSeconds}s");

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                using CancellationTokenSource wave = CancellationTokenSource.CreateLinkedTokenSource(cts.Token);

                List<PlcSensorSyncDto> points;
                try
                {
                    points = await FetchSyncAsync(http, wave.Token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[sync] {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(10), cts.Token).ConfigureAwait(false);
                    continue;
                }

                if (points.Count == 0)
                {
                    Console.WriteLine("[sync] sensör tanımı yok; bekleniyor...");
                    await Task.Delay(TimeSpan.FromSeconds(Math.Min(30, cfg.ConfigRefreshSeconds)), cts.Token).ConfigureAwait(false);
                    continue;
                }

                Console.WriteLine($"[sync] {points.Count} sensör yüklendi.");

                Task[] loops = points.Select(p => RunSensorLoopAsync(http, p, wave.Token)).ToArray();
                Task refreshWait = Task.Delay(TimeSpan.FromSeconds(cfg.ConfigRefreshSeconds), cts.Token);

                Task winner = await Task.WhenAny(Task.WhenAll(loops), refreshWait).ConfigureAwait(false);
                await wave.CancelAsync().ConfigureAwait(false);

                try
                {
                    await Task.WhenAll(loops).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // dalga iptali — beklenen
                }

                if (winner == refreshWait && !cts.Token.IsCancellationRequested)
                    continue;

                if (cts.Token.IsCancellationRequested)
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            // Ctrl+C
        }

        return 0;
    }

    private static async Task RunSensorLoopAsync(HttpClient http, PlcSensorSyncDto point, CancellationToken ct)
    {
        int delayMs = Math.Clamp(point.PollIntervalSeconds * 1000, 500, 86_400_000);
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await PollOnceAsync(http, point, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{point.SensorCode}] {ex.Message}");
            }

            try
            {
                await Task.Delay(delayMs, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private static async Task PollOnceAsync(HttpClient http, PlcSensorSyncDto point, CancellationToken ct)
    {
        if (!IPAddress.TryParse(point.ModbusHost, out IPAddress? ip))
            ip = (await Dns.GetHostAddressesAsync(point.ModbusHost, ct).ConfigureAwait(false)).FirstOrDefault()
                ?? throw new InvalidOperationException($"Çözülemeyen host: {point.ModbusHost}");

        var endpoint = new IPEndPoint(ip, point.ModbusPort);

        using var client = new ModbusTcpClient();
        await Task.Run(() => client.Connect(endpoint, ModbusEndianness.BigEndian), ct).ConfigureAwait(false);

        Memory<ushort> buffer = await client
            .ReadHoldingRegistersAsync<ushort>(point.SlaveId, point.RegisterAddress, point.RegisterLength, ct)
            .ConfigureAwait(false);

        ushort[] regs = buffer.ToArray();
        int raw = CombineRegisters(regs);
        double divisor = point.ScaleDivisor == 0 ? 1.0 : point.ScaleDivisor;
        double value = raw / divisor;

        var payload = new TelemetryPostBody
        {
            Items =
            [
                new TelemetryItem
                {
                    SensorCode = point.SensorCode,
                    Value = value,
                    ReadAtUtc = DateTime.UtcNow,
                    RawRegisterValue = raw,
                },
            ],
        };

        using var resp = await http
            .PostAsJsonAsync("api/plc-integration/telemetry", payload, JsonOpts, ct)
            .ConfigureAwait(false);

        if (!resp.IsSuccessStatusCode)
        {
            string body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            throw new InvalidOperationException($"telemetry HTTP {(int)resp.StatusCode}: {body}");
        }
    }

    /// <summary>Çok kayıtlı ham değer (PLC’ye göre sıra değişebilir; gerekirse worker’da özelleştirin).</summary>
    private static int CombineRegisters(ushort[] regs)
    {
        return regs.Length switch
        {
            0 => 0,
            1 => regs[0],
            _ => (regs[0] << 16) | regs[1],
        };
    }

    private static async Task<List<PlcSensorSyncDto>> FetchSyncAsync(HttpClient http, CancellationToken ct)
    {
        using HttpResponseMessage resp = await http.GetAsync("api/plc-integration/sync", ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        await using Stream stream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        List<PlcSensorSyncDto>? list = await JsonSerializer.DeserializeAsync<List<PlcSensorSyncDto>>(stream, JsonOpts, ct).ConfigureAwait(false);
        return list ?? new List<PlcSensorSyncDto>();
    }

    private static WorkerConfig LoadConfig(string[] args)
    {
        var cfg = new WorkerConfig();
        string baseDir = AppContext.BaseDirectory;
        string path = Path.Combine(baseDir, "appsettings.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            WorkerConfig? fileCfg = JsonSerializer.Deserialize<WorkerConfig>(json, JsonOpts);
            if (fileCfg is not null)
                cfg = fileCfg;
        }

        foreach (string a in args)
        {
            if (a.StartsWith("--api=", StringComparison.OrdinalIgnoreCase))
                cfg.ApiBaseUrl = a["--api=".Length..];
            else if (a.StartsWith("--key=", StringComparison.OrdinalIgnoreCase))
                cfg.IndustrialApiKey = a["--key=".Length..];
            else if (a.StartsWith("--refresh-sec=", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(a["--refresh-sec=".Length..], out int r))
                cfg.ConfigRefreshSeconds = Math.Max(30, r);
            else if (string.Equals(a, "--insecure-tls", StringComparison.OrdinalIgnoreCase))
                cfg.AcceptAnyTlsCertificate = true;
        }

        if (string.IsNullOrWhiteSpace(cfg.ApiBaseUrl))
            cfg.ApiBaseUrl = Environment.GetEnvironmentVariable("PLC_API_BASE_URL") ?? "http://localhost:5274";

        if (string.IsNullOrWhiteSpace(cfg.IndustrialApiKey))
            cfg.IndustrialApiKey = Environment.GetEnvironmentVariable("PLC_INDUSTRIAL_API_KEY") ?? "";

        string? refreshEnv = Environment.GetEnvironmentVariable("PLC_CONFIG_REFRESH_SEC");
        if (!string.IsNullOrEmpty(refreshEnv) && int.TryParse(refreshEnv, out int rs))
            cfg.ConfigRefreshSeconds = Math.Max(30, rs);

        return cfg;
    }
}

internal sealed class WorkerConfig
{
    public string ApiBaseUrl { get; set; } = "";
    public string IndustrialApiKey { get; set; } = "";
    public int ConfigRefreshSeconds { get; set; } = 300;
    public bool AcceptAnyTlsCertificate { get; set; }
}

internal sealed class PlcSensorSyncDto
{
    public Guid Id { get; set; }
    public string SensorCode { get; set; } = "";
    public string DeviceName { get; set; } = "";
    public string DevicePrefix { get; set; } = "";
    public string DataLabel { get; set; } = "";
    public string ModbusHost { get; set; } = "";
    public int ModbusPort { get; set; }
    public int SlaveId { get; set; }
    public int RegisterAddress { get; set; }
    public int RegisterLength { get; set; }
    public double ScaleDivisor { get; set; }
    public int PollIntervalSeconds { get; set; }
}

internal sealed class TelemetryPostBody
{
    public List<TelemetryItem> Items { get; set; } = new();
}

internal sealed class TelemetryItem
{
    public string SensorCode { get; set; } = "";
    public double Value { get; set; }
    public DateTime ReadAtUtc { get; set; }
    public int? RawRegisterValue { get; set; }
}
