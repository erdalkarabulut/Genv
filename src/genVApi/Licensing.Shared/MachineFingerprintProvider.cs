using System.Security.Cryptography;
using System.Text;

namespace Licensing.Shared;

public static class MachineFingerprintProvider
{
    public static string GetFingerprintSha256Hex()
    {
        string raw = GetRawMaterial();
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string GetRawMaterial()
    {
        if (OperatingSystem.IsWindows())
            return "win:" + (ReadWindowsMachineGuid() ?? Environment.MachineName);
        if (OperatingSystem.IsLinux())
            return "linux:" + (ReadFirstLineIfExists("/etc/machine-id")
                ?? ReadFirstLineIfExists("/var/lib/dbus/machine-id")
                ?? Environment.MachineName);
        if (OperatingSystem.IsMacOS())
            return "mac:" + (ReadMacUuid() ?? Environment.MachineName);

        return "other:" + Environment.MachineName;
    }

    private static string? ReadWindowsMachineGuid()
    {
        try
        {
            if (!OperatingSystem.IsWindows()) return null;
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
            return key?.GetValue("MachineGuid") as string;
        }
        catch
        {
            return null;
        }
    }

    private static string? ReadFirstLineIfExists(string path)
    {
        try
        {
            if (!File.Exists(path)) return null;
            string? line = File.ReadAllText(path).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
            return string.IsNullOrWhiteSpace(line) ? null : line.Trim();
        }
        catch
        {
            return null;
        }
    }

    private static string? ReadMacUuid()
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "/usr/sbin/ioreg",
                Arguments = "-rd1 -c IOPlatformExpertDevice",
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };
            using var p = System.Diagnostics.Process.Start(psi);
            if (p is null) return null;
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit(5000);
            const string marker = "\"IOPlatformUUID\" = \"";
            int i = output.IndexOf(marker, StringComparison.Ordinal);
            if (i < 0) return null;
            i += marker.Length;
            int j = output.IndexOf('"', i);
            if (j < 0) return null;
            return output[i..j];
        }
        catch
        {
            return null;
        }
    }
}
