# Stem Cell & Cryo Management — Demo Rehberi

## 1. Servisleri ayağa kaldır

### Backend (API + PostgreSQL)

```bash
docker compose up -d postgres
dotnet run --project src/genVApi/WebAPI
```

ya da tamamen container içinde:

```bash
docker compose up --build
```

Swagger UI: http://localhost:5278/swagger (veya Docker'da http://localhost:8080/swagger)

### Frontend (Vite + React)

```bash
cd frontend
npm install
npm run dev
```

Panel: http://localhost:5173

Dev sunucusu `/api` ve `/hubs` isteklerini `http://localhost:5278` adresindeki .NET
API'ye proxy'ler. `Development` ortamında API, demo akışlarının kimlik doğrulama
olmadan çalışabilmesi için otomatik olarak bir `Admin` principal enjekte eder.

Uygulama açılışta:
- EF Core migration'ı çalıştırır.
- Tam demo veri setini seed eder (ayrıntı için bkz. [§ 2. Seed verisi](#2-seed-verisi)).

## 2. Seed verisi

> Veritabanı boşken uygulama kalkışında otomatik eklenir (`Tanks` tablosu boş
> ise çalışır; aksi halde atlanır). Tüm klinik senaryo ve operasyon durumları
> tek şemada kapsanır.
>
> **Yeniden seed etmek için:**
> ```bash
> docker compose down -v        # db volume'ünü siler
> docker compose up --build     # migration + seed tekrar çalışır
> ```

### Tanklar ve slotlar

| Tank  | Rack      | Box       | Slot sayısı           |
|-------|-----------|-----------|-----------------------|
| TankA | R1, R2    | B1, B2    | 4 × 9 = **36 slot**   |
| TankB | R1        | B1        | 1 × 9 = **9 slot**    |

Seed sonrası Cryo Grid'de 4 torba Stored (P1·P4·P5 cryo bagleri + P5'in
taşınmış hali) ile birkaç slot dolu, kalanlar boş görünür.

### Donorlar

| Ad                    | Kilo | Kan | Yakınlık  |
|-----------------------|------|-----|-----------|
| Murat Kara (Kardeş)   | 72   | A+  | Sibling   |
| Serap Tunç (Anne)     | 66   | 0-  | Parent    |
| MUD Donor 2026-17     | 70   | B+  | MUD       |

### Hastalar — tüm klinik durumları kapsayan 6 senaryo

| Kod  | Ad              | Kilo | Tip          | Tanı                | Protokol     | Gün(ler)      | Kümülatif CD34/kg | Kümülatif CD3/kg | Klinik durum                  | Torba durumu                                             |
|------|-----------------|------|--------------|---------------------|--------------|---------------|-------------------|------------------|-------------------------------|----------------------------------------------------------|
| P1   | John Doe        | 70   | Otolog       | Multiple Myeloma    | PR-2026-001  | 1             | **5.8** (Optimal) | 5.1              | **Optimal**                   | 4 torba (1 Stored/Cryo + 3 Reserved)                     |
| P2   | Fatma Kara      | 65   | Otolog       | NH Lymphoma         | PR-2026-002  | 1 → 2         | 3.3 (Sınırda)     | 4.4              | **Sınırda** — split açık       | Henüz bölünmedi (demo: `POST /api/Bags/split` denenebilir) |
| P3   | Ali Veli        | 80   | Otolog       | AML (induction fail)| PR-2026-003  | 1 → 2 → 3 → 4 | 1.6 (< target 2)  | 3.2              | **Yetersiz** · max gün doldu  | Torba yok — klinik değerlendirme gerekir                 |
| P4   | Mehmet Solmaz   | 68   | Allogeneik   | CML blast faz       | PR-2026-004  | 1             | 6.2 (Optimal)     | **12.5**         | **GVHD riski** (CD3 > 10)     | 4 torba (1 Stored/Cryo + 3 Reserved)                     |
| P5   | Ayşe Tunç       | 60   | Allogeneik   | AML MRD+            | PR-2026-005  | 1 → 2         | 5.1 (Optimal)     | 5.3              | **Optimal**                   | 4 torba · Cryo bir kez **Move**'landı · Infüzyon **Used** |
| P6   | Hakan Dalgıç    | 75   | Allogeneik   | ALL (Ph+)           | PR-2026-006  | 1 → 2         | 5.1 (Optimal)     | **1.6**          | **Düşük bağışıklık** (CD3 < 2)| Henüz bölünmedi                                          |

### Bag movement log

Seed'in oluşturduğu `BagMovement` kayıtları:

- `Split-Store (Cryo)` — P1, P4, P5 için cryo torbası ilk boş slota yerleştirildi.
- `Move` — P5'in cryo torbası bir sonraki slota taşındı (demo amacıyla).
- `Use` — P5'in Infusion torbası infüzyon akışında kullanıldı.

### Ekranlarda ne görürsün

- **Dashboard**: 6 hasta, 10 aferez seansı, 12 torba (1 Used + 11 aktif),
  doluluk oranı ~%9, kümülatif CD34 toplamı ~29, CD3 toplamı ~33, risk
  bileşeni "Optimal" (toplam sınırları aştığı için).
- **Hastalar**: 6 kart; otolog/allogeneik rozetleri karışık.
- **Hasta detayı**: Her senaryo için farklı öneri banner'ı (Optimal / Sınırda /
  Yetersiz / GVHD riski / Düşük bağışıklık / Max gün aşıldı). Timeline'da her
  güne ait PK (WBC pre, HGB, HCT, PLT) ve ÜRÜN (Hacim, WBC, %CD34/45/3,
  %Lenfosit, MHS) değerleri açılabilir.
- **Cryo Grid**: TankA R1 B1 alt bölümünde Stored slotlar, TankB ile
  karşılaştırmalı doluluk rozetleri.
- **Bags**: Frozen / Stored / Reserved / Used / Discarded dağılımı gerçek
  veriyle filtrelenebilir; P5'in Used Infusion torbası ve Cryo'nun Move geçmişi
  görünür.

## 3. Demo akışı (1-7)

### 1) Hasta oluştur
`POST /api/Patients`
```json
{
  "fullName": "John Doe",
  "weightKg": 75,
  "bloodGroup": "A+",
  "transplantType": "Autologous",
  "diagnosis": "Multiple Myeloma",
  "protocolNo": "PR-2026-42",
  "birthDate": "1985-05-17"
}
```

### 2) Session (Aferez) gir
`POST /api/CollectionSessions`
```json
{
  "patientId": "<hasta-id>",
  "day": 1,
  "date": "2026-04-20",
  "volumeMl": 250,
  "wbc": 180,
  "cd34Percent": 1.2,
  "cd45Percent": 80,
  "cd3Percent": 35,
  "cd34PerKg": 0,
  "cd3PerKg": 0
}
```

### 3) CD34 / CD3 hesapla
`POST /api/CollectionSessions/{id}/calculate`

Response örneği:
```json
{ "sessionId": "…", "cd34PerKg": 5.76, "cd3PerKg": 16.8, "patientWeightKg": 75 }
```

### 4) Bag oluştur
`POST /api/Bags`
```json
{
  "sessionId": "<session-id>",
  "bagNumber": 1,
  "volumeMl": 120,
  "sourceVolumeMl": 250,
  "cd34PerKg": 5.76,
  "cd3PerKg": 16.8,
  "status": "Frozen"
}
```

### 5) Bag'i slota koy
`POST /api/Bags/store`
```json
{ "bagId": "<bag-id>", "slotId": "<slot-id>" }
```

Bu anda tüm bağlı SignalR clientlarına `BagStored` ve `DashboardUpdated` event'leri yayınlanır.

### 5.b) Kümülatif yeterli olduğunda — 4 torbaya böl + 1'ini cryo'ya yerleştir

Kümülatif CD34/kg hedefe ulaştığında ürün **4 torbaya bölünür**, bunlardan **1 tanesi cryo amacıyla tanka** konur. Tek bir istek bu işlemin tamamını yapar:

`POST /api/Bags/split`
```json
{
  "sessionId": "<son-session-id>",
  "bagCount": 4,
  "autoPlaceCryo": true,
  "requireCumulativeSufficient": true
}
```

- `bagCount` (default 4) → ürün kaç torbaya bölünecek.
- 1. torba `Cryo`, 2. `Infusion`, 3. `Backup`, 4. `QualityControl` amacıyla oluşturulur.
- `autoPlaceCryo=true` iken sistem **ilk boş slotu** bulup Cryo torbasını oraya yerleştirir ve `BagStored` event'i yayınlar. Manuel slot vermek için `cryoSlotId` gönderilebilir.
- `requireCumulativeSufficient=true` iken hastanın kümülatif CD34/kg hedef altındaysa işlem reddedilir.
- Her torba aynı `splitBatchId`'yi taşır → grup olarak izlenebilir.

Response örneği:

```json
{
  "sessionId": "…",
  "patientId": "…",
  "splitBatchId": "…",
  "bagCount": 4,
  "perBagVolumeMl": 62.5,
  "perBagCd34PerKg": 1.44,
  "perBagCd3PerKg": 4.20,
  "cryoBagId": "…",
  "cryoSlotId": "…",
  "bags": [
    { "bagNumber": 1, "purpose": "Cryo",           "status": "Stored",   "slotId": "…" },
    { "bagNumber": 2, "purpose": "Infusion",       "status": "Reserved", "slotId": null },
    { "bagNumber": 3, "purpose": "Backup",         "status": "Reserved", "slotId": null },
    { "bagNumber": 4, "purpose": "QualityControl", "status": "Reserved", "slotId": null }
  ]
}
```

### 6) Başka kullanıcıda anında görün (SignalR)

JS istemci örneği:

```javascript
const conn = new signalR.HubConnectionBuilder()
  .withUrl("/hubs/cryo")
  .build();

conn.on("BagStored",  p => console.log("stored", p));
conn.on("BagMoved",   p => console.log("moved", p));
conn.on("BagUsed",    p => console.log("used", p));
conn.on("DashboardUpdated", () => refreshDashboard());

await conn.start();
await conn.invoke("JoinTenant", "default");
```

### 7) Bag kullan → slot boşalsın
`POST /api/Bags/use`
```json
{ "bagId": "<bag-id>" }
```

Bag `Used` olur, slot boşalır, `BagUsed` event'i yayınlanır.

## 4. İlave uç noktalar

| Endpoint | Açıklama |
| --- | --- |
| `GET  /api/Dashboard` | Toplam CD34/CD3, risk durumu, bag/slot sayıları |
| `GET  /api/Dashboard/cryo-grid` | Tank → Rack → Box → Slot hiyerarşisi (UI grid için) |
| `GET  /api/ApheresisPlans/{patientId}` | Transplant tipine göre aferez planı, kümülatif CD34, sonraki gün önerisi |
| `POST /api/Bags/move` | Bir bag'i slot A'dan slot B'ye taşır |
| `POST /api/Bags/split` | Aferez ürününü N torbaya böler, 1'ini Cryo olarak işaretler ve isteğe bağlı oto-yerleştirir |
| `POST /api/CollectionSessions/{id}/calculate` | CD34 / CD3 formüllerini hesaplar |

### Aferez Planlama

`GET /api/ApheresisPlans/{patientId}` her istekte şu bilgiyi döner:

```json
{
  "patientId": "…",
  "transplantType": "Autologous",
  "isAutologous": true,
  "maxCollectionDays": 4,
  "targetCd34PerKg": 2.0,
  "idealCd34PerKg": 4.0,
  "completedDays": 1,
  "remainingDays": 3,
  "cumulativeCd34PerKg": 1.82,
  "cumulativeCd3PerKg": 5.4,
  "isSufficient": false,
  "isOptimal": false,
  "maxDaysReached": false,
  "shouldContinue": true,
  "nextDay": 2,
  "nextPlannedDate": "2026-04-21T00:00:00Z",
  "status": "Yetersiz",
  "recommendation": "Gün 1 tamamlandı, kümülatif CD34 (1.82) hedefin (2.00) altında. Gün 2 ile devam edilmelidir.",
  "completedSessions": [ … ],
  "forecastPlan":      [ … ]
}
```

## 5. Hesaplama formülleri

```
CD34/kg = (Volume × WBC × %CD45 × %CD34) / 10000 / WeightKg
CD3/kg  = (Volume × WBC × %CD3) / 10000 / WeightKg
```

Klinik değerlendirme (yalnızca UI mesajı):

| Durum | Koşul |
| --- | --- |
| Optimal           | CD34 ≥ 4 ve CD3 ∈ [3,8]      |
| GVHD Riski        | CD34 ≥ 4 ve CD3 > 10         |
| Düşük Bağışıklık  | CD34 ≥ 4 ve CD3 < 2          |
| Yetersiz          | CD34 < 2                     |

## 6. Aferez günü kuralları

| Transplant | Maksimum gün | Ardışık olmak zorunda | Minimum CD34/kg | İdeal CD34/kg |
| --- | --- | --- | --- | --- |
| **Autologous** (hasta kendi)   | 4 | Hayır | 2 | 4 |
| **Allogeneic** (donör / akraba) | 2 | Evet  | 4 | 5 |

Sunucu tarafı kontrolleri (`CreateCollectionSessionCommand` sırasında):

- `Day > maxDays` → BusinessException ("Maksimum N gün aferez yapılabilir").
- Aynı hasta için aynı gün iki kez açılamaz.
- Allogeneik protokolde gün 2 açılmadan önce gün 1 olmak zorundadır.
- Session oluşturulduğunda eğer `Cd34PerKg`/`Cd3PerKg` sıfır bırakılmışsa otomatik hesaplanır.

> Bu sistem medikal karar vermez, sadece veri ve hesaplama sunar.
