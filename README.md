<div align="center">

> **A [CaYaDev](https://github.com/CaYatur) product** — *CaYaSafe* serisinden

</div>

# CaYaSafeLock — Full Screen PC Kilit Sistemi

CaYaSafeLock, Windows tabanlı bilgisayarları kilitlemek, izlemek ve yalnızca yetkili kullanıcıların (QR kod veya PIN ile) açmasına izin vermek için geliştirilmiş çok bileşenli bir güvenlik sistemidir. Eğitim kurumları, kütüphaneler veya halka açık bilgisayar salonları için tasarlanmıştır.

---

## Sistem Mimarisi

```
┌─────────────────────────────────────────────────────────┐
│                     Kullanıcı Makinesi                  │
│                                                         │
│  ┌─────────────┐   ┌──────────────┐  ┌───────────────┐ │
│  │  PCDisableCY │   │    CYSADS    │  │CaYaControlSys │ │
│  │ (Kilit Ekranı│   │  (Servis 1)  │  │  (Servis 2)   │ │
│  │  Uygulaması) │   │  Dosya İzle  │  │  Servis İzle  │ │
│  └──────┬──────┘   └──────┬───────┘  └───────┬───────┘ │
│         │                 └─────────┬─────────┘         │
│  ┌──────┴──────┐              ┌─────┴──────┐            │
│  │    CYSL     │              │CYStartupCtrl│            │
│  │(Servis Başl.)│             │(Başlangıç) │            │
│  └─────────────┘              └────────────┘            │
└───────────────────────┬─────────────────────────────────┘
                        │ HTTPS (QR Doğrulama)
                        ▼
         ┌──────────────────────────┐
         │     CaYaSafeQR Server    │
         │    (Node.js / Express)   │
         │  QR Kayıt & Doğrulama   │
         └──────────────────────────┘
```

---

## Bileşenler

### 1. `PCDisableCY` — Ana Kilit Ekranı Uygulaması
Ana uygulama. Windows başladığında CYSL servisi tarafından otomatik olarak başlatılır.

**Özellikler:**
- Tüm monitörleri kaplayan tam ekran kilit ekranı
- Klavye kısayollarını engelleme (Alt+F4, Win tuşu, Ctrl+Alt+Del vb.)
- **QR Kod ile açma:** Yetkili kullanıcı, web uygulamasından (CaYaSafeQR) QR kod tarar → kilit sunucu üzerinden doğrular → açılır
- **PIN kodu ile açma:** 6 haneli şifreli PIN girişi
- **Otomatik kilit zamanlayıcısı:** Belirtilen süre sonra ekranı otomatik kilitler
- **USB engelleme:** USB takıldığında algılar ve çıkarma işlemi uygular
- Saat/tarih gösterimi
- Birden fazla tema (Siyah, Beyaz, Gökkuşağı)
- **Oylama sistemi:** Sınıf ortamında anket/oylama yapılabilir (VoteSystem)
- Kurulum kaldırma işlemini algılama ve güvenli kapatma

**Ekranlar / Formlar:**
| Form | Görev |
|------|-------|
| `LockScreen` | Ana kilit ekranı, QR okuma, PIN girişi |
| `Kilit` | Alt kilit bileşeni |
| `LockSystem` | Sistem bilgisi gösterimi (sağ alt köşe) |
| `settings` | Zamanlayıcı ve kilit ayarları |
| `VoteSystem` | Anlık oylama sistemi |
| `USBblock` | USB cihaz izleme ve engelleme |
| `WatcherANT` | Servis izleme ve watchdog |
| `TimeOut` | Otomatik kilit geri sayım ekranı |
| `SafeModWarn` | Güvenli mod uyarı ekranı |
| `cancel` | İptal/durdurma ekranı |

---

### 2. `CYSADS` — Dosya İzleme Servisi
Windows arka plan servisi. Sistem günlüğe kayıt dosyasını izler.

**Görevler:**
- `C:\ProgramData\CaYaProtection\CaYaSafe\LockSC\RLTMC.cysf` dosyasını sürekli izler
- Dosya içeriği yetkisiz değiştirilirse → `shutdown /s /f /t 0` (bilgisayarı zorla kapatır)
- `CaYaControlSystem` servisinin çalışıp çalışmadığını kontrol eder
- Servis beklenmedik şekilde durduğunda bilgisayarı kapatır

---

### 3. `CaYaControlSystem` — Servis Watchdog
Windows arka plan servisi. CYSADS servisini izler.

**Görevler:**
- CYSADS servisinin çalışıp çalışmadığını döngüsel olarak kontrol eder
- CYSADS durduğunda ve RLTMC.cysf dosyası geçersizse → bilgisayarı zorla kapatır
- İki servis birbirini izleyerek bypass girişimlerini engeller

> **Not:** CYSADS ve CaYaControlSystem birbirini izleyen çift watchdog sistemi oluşturur. İkisi de çalışmadığında sistem kapatılır.

---

### 4. `CYSL` — Servis Başlatıcı
Windows servisi. Oturum açan kullanıcının masaüstünde PCDisableCY uygulamasını başlatır.

**Görevler:**
- WTS (Windows Terminal Services) API kullanarak aktif kullanıcı oturumunu tespit eder
- `CreateProcessAsUser` ile PCDisableCY'yi kullanıcı oturumunda çalıştırır
- Servis olarak çalıştığı için kullanıcı masaüstünde görünür kılar

---

### 5. `CYStartupControl` — Başlangıç Kontrolcüsü
Windows başlangıcında çalışır. Güvenli Mod gibi bypass yöntemlerini engeller.

**Görevler:**
- Güvenli Mod'u devre dışı bırakır (Registry üzerinden)
- Yönetici yetkisi yoksa `StartupScreenBlock` ekranı gösterir
- Başka bir örnek zaten çalışıyorsa kendini sonlandırır

---

### 6. `CaYaSafeQR` — QR Kod Sunucusu (Node.js)
Web tabanlı QR kod kimlik doğrulama ve yönetim sunucusu.

**Teknolojiler:** Node.js, Express, Socket.IO, CryptoJS, dotenv

**API Endpointleri:**
| Endpoint | Metod | Açıklama |
|----------|-------|----------|
| `/` | GET | QR tarayıcı arayüzü |
| `/:key` | GET | 8 haneli anahtarla kişisel arayüz |
| `/kaydet` | POST | Taranan QR kodu sunucuya kaydet |
| `/sil/*` | GET | Kaydedilen QR kodu sil |
| `/kontrol/*` | GET | QR kodun geçerli olup olmadığını kontrol et |
| `/connect` | POST | Kullanıcı bağlantısını kaydet |
| `/connected-users` | GET | Bağlı kullanıcıları listele |

**`server2.js` — Oylama Sunucusu:**
| Endpoint | Açıklama |
|----------|----------|
| `/anket-olustur` | Yeni anket oluştur |
| `/anket-bitir/:kod` | Anketi kapat (60 sn sonra sil) |
| `/anket-sonuc/:kod` | Anlık oylama sonuçları |
| Socket: `oy-ver` | Oy kullan |
| Socket: `oy-guncelle` | Anlık oy güncellemesi |

---

### 7. `CaYaSafeLockMainSetup` — İlk Kurulum Aracı
Sistemi ilk kez yapılandırmak için kullanılır.

**Görevler:**
- Yönetici şifresini ve cihaz anahtarını (8 haneli PIN) alır
- Her ikisini AES-256 ile şifreleyerek `CtL.cy` ve `log.cdat` dosyalarına yazar
- Bu dosyalar kurulum sırasında hedef makineye kopyalanır

---

### 8. `CaYaSafeLockSetup` — Kurulum / Kaldırma Uygulaması
Sistemi hedef bilgisayara kurar veya kaldırır.

**Kurulum adımları:**
1. `CYSYSTEM` klasörünü `C:\Users\Default\AppData\Roaming\CYSYSTEM` konumuna kopyalar
2. `CaYaProtection` klasörünü `C:\ProgramData\CaYaProtection` konumuna kopyalar
3. Windows Registry'yi günceller
4. Görev zamanlayıcıya başlangıç görevi ekler
5. CYSADS ve CaYaControlSystem servislerini yükler ve başlatır

**Kaldırma adımları:**
1. Servisleri durdurur ve kaldırır
2. Görev zamanlayıcıdaki görevi siler
3. Kopyalanan dosyaları ve klasörleri temizler
4. Registry kayıtlarını siler

---

## Açılış Akışı

```
Windows Boot
    └─► CYStartupControl (Güvenli Mod engelle)
    └─► CYSADS Servisi başlar (dosya izlemeye başlar)
    └─► CaYaControlSystem Servisi başlar (CYSADS izler)
    └─► CYSL Servisi başlar
            └─► PCDisableCY'yi kullanıcı oturumunda başlatır
                    └─► LockScreen gösterilir
                    └─► USB izleme başlar
                    └─► QR sunucu sorgulaması başlar (her 1 sn)
                    └─► Zamanlayıcı başlar
```

---

## Kilit Açma Yöntemi

### QR Kod ile:
1. Kullanıcı, `CaYaSafeQR` web arayüzünü açar (kamera ile)
2. Yetkili kişinin QR kodunu tarar
3. QR kod AES-256 ile şifreli → sunucuya gönderilir
4. PCDisableCY her saniye sunucuyu sorgular
5. Eşleşme bulunursa kayıt silinir ve kilit açılır

### PIN Kodu ile:
1. Kilit ekranında 6 haneli PIN girilir
2. Girilen değer AES-256 ile şifrelenip `log.cdat` ile karşılaştırılır
3. Eşleşirse kilit açılır

---

## Kurulum

### Gereksinimler
- Windows 10/11
- .NET Framework 4.8
- Node.js 18+
- Yönetici (Administrator) yetkisi

### Node.js Sunucusu
```bash
cd CaYaSafeQR
cp .env.example .env
# .env dosyasını düzenleyip AES_KEY ve AES_IV değerlerini girin
npm install
npm start
```

### C# Projeleri
1. `CaYaSafeLockMainSetup` ile şifreli config dosyalarını oluşturun
2. `CaYaSafeLockSetup` ile hedef bilgisayara kurun

### Konfigürasyon (`.env`)
```env
PORT=8080
AES_KEY=<32 karakterlik şifreleme anahtarınız>
AES_IV=<16 karakterlik IV değeriniz>
```

### Konfigürasyon (`App.config`)
`PCDisableCY` ve `CaYaSafeLockMainSetup` projeleri için `App.config.example` dosyasını `App.config` olarak kopyalayıp anahtarları doldurun:
```xml
<add key="AES_KEY" value="..." />
<add key="AES_IV" value="..." />
<add key="QR_SERVER_URL" value="https://your-server-url/" />
<add key="VOTE_SERVER_URL" value="https://your-vote-server-url" />
```

---

## Proje Yapısı

```
CaYaSafeLock-FullScreen/
├── CaYaSafeQR/                  # Node.js QR sunucusu
│   ├── server.js                # Ana sunucu (QR doğrulama)
│   ├── server2.js               # Oylama sunucusu (Socket.IO)
│   ├── index.html               # QR tarayıcı arayüzü
│   ├── public/
│   │   └── anket.html           # Oylama arayüzü
│   ├── .env.example             # Ortam değişkeni şablonu
│   └── package.json
│
├── CaYaSafeYazılım/             # Derlenmiş binary'ler (kurulum paketi)
│   ├── CaYaSafeLockSetup.exe
│   ├── CaYaSafeLockMainSetup.exe
│   └── CaYaProtection/          # Servis ve uygulama dosyaları
│
└── Visual Studio/               # C# kaynak kodları
    ├── PCDisableCY/             # Ana kilit ekranı uygulaması
    ├── CYSADS/                  # Dosya izleme servisi
    ├── CaYaControlSystem/       # Servis watchdog
    ├── CYSL/                    # Servis başlatıcı
    ├── CYStartupControl/        # Başlangıç kontrolcüsü
    ├── CaYaSafeLockSetup/       # Kurulum/kaldırma
    └── CaYaSafeLockMainSetup/   # İlk kurulum aracı
```

---

## Güvenlik Notları

- AES şifreleme anahtarları **asla** kaynak koda yazılmamalıdır. `App.config` ve `.env` dosyalarını kullanın.
- `userLoginData.CY`, `CtL.cy`, `log.cdat` dosyaları şifreli runtime verisi içerir — `.gitignore`'a eklenmiştir.
- QR kod sistemi replay saldırısına karşı korumalıdır: her QR başarılı doğrulamadan sonra sunucudan silinir.

---

## Lisans

Bu proje **MIT License with Ethical Use Restriction** kapsamında lisanslanmıştır.

Özgürce kullanabilir, değiştirebilir ve dağıtabilirsiniz. Ancak aşağıdaki kullanımlar **kesinlikle yasaktır:**

- Habersiz/onaysız başkalarının bilgisayarını kilitlemek
- Fidye yazılımı, şantaj veya zorlama amacıyla kullanmak
- Yetkisiz erişim sağlamak veya kişisel veri toplamak
- Herhangi bir suç faaliyetinde araç olarak kullanmak

Ayrıntılar için [LICENSE](LICENSE) dosyasına bakın.

---

<div align="center">

Made with ❤️ by **[CaYaDev](https://github.com/CaYatur)** · *CaYaSafe* ürün serisi

© 2024-2026 ÇAĞAN TURGUT

</div>
