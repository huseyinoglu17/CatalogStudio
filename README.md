# Barış Kerem Hüseyinoğlu — Çocuk Giyim Katalog Atölyesi

Tek .NET 10 LTS / ASP.NET Core MVC projesi. Razor, yerel Bootstrap, EF Core SQLite ve int anahtarlı ASP.NET Core Identity kullanır. React veya ayrı Web API yoktur. İkinci gereksinim belgesindeki üyelik ve admin paneli ilk belgedeki anonim kullanıcı yaklaşımının yerini alır.


## Railway Deployment

### Kurulum

1. `CatalogStudio` kaynak klasörünü GitHub repository içine push edin. Yerel App_Data, veritabanı, yüklenen görseller ve secret dosyalarını eklemeyin.
2. Railway üzerinde **New Project → Deploy from GitHub Repo** ile repository seçin.
3. Service **Root Directory** değerini `CatalogStudio.csproj` ve `Dockerfile` dosyalarını içeren klasöre ayarlayın. Repository doğrudan bu klasörün içeriğiyse kök dizini kullanın; teslim ZIP yapısını koruduysanız `/CatalogStudio` kullanın.
4. **Variables** alanına aşağıdaki değerleri ekleyin:

| Variable | Değer |
| --- | --- |
| ASPNETCORE_ENVIRONMENT | `Production` |
| OPENAI_API_KEY | Kendi OpenAI API anahtarınız; Railway secret olarak saklayın |
| ADMIN_EMAIL | Oluşturulacak yönetici hesabının e-posta adresi |
| ADMIN_PASSWORD | En az 8 karakter, büyük/küçük harf ve rakam içeren güçlü parola |
| APP_DATA_PATH | `/app/App_Data` |
| ConnectionStrings__DefaultConnection | İsteğe bağlı: `Data Source=/app/App_Data/catalog.db` |

`PORT` değerini elle eklemeyin; Railway tarafından sağlanır. Uygulama bu portta `0.0.0.0` üzerinde dinler. API anahtarı için öncelik `OPENAI_API_KEY`, ardından `OpenAI:ApiKey` yapılandırmasıdır. Development user-secrets production container'a taşınmaz.

5. Service'e **Volume** ekleyin; **Mount Path** tam olarak `/app/App_Data` olsun.
6. Dockerfile builder kullanın. Özel build/start/pre-deploy komutu girmeyin. Migration, volume bağlandıktan sonra uygulama startup sırasında çalışır.
7. **Settings → Networking → Generate Domain** ile HTTPS adresi oluşturun.
8. **Healthcheck Path**: `/health`. Dahil edilen `railway.json` Dockerfile, 120 saniye healthcheck bekleme süresi ve tek replica ayarını içerir.
9. Deploy edin. Logs ekranında storage yolu, migration tamamlanması ve uygulamanın dinlediği portu kontrol edin.
10. HTTPS siteyi açıp admin hesabıyla giriş yapın; kullanıcı kaydı, logo yükleme, katalog üretme/indirme ve tekrar üretme akışlarını kontrol edin. Gerçek katalog testi API kullanımına tabidir.
11. Redeploy sonrası katalogların ve oturumun korunduğunu kontrol edin.

### Kalıcı veriler ve işletim

SQLite, dosyalar ve Data Protection anahtarları merkezi StoragePathService üzerinden volume altında tutulur. Dosyalar `App_Data/files`, oturum anahtarları `App_Data/DataProtection-Keys` altındadır. Kullanıcı dosyaları yetki kontrolü yapan mevcut controller endpointlerinden sunulur; App_Data public değildir. Tekrar üretim için gereken orijinal girdiler kalıcı, AI ara çıktıları işletim sistemi temp klasöründedir ve işlem temizliğinde kaldırılır.

**Volume olmadan redeploy sonrasında veritabanı, kataloglar ve oturum anahtarları kaybolabilir.** Volume yedeğini düzenli alın. Mevcut yerel verileri taşımak istiyorsanız uygulama durmuşken tutarlı bir App_Data yedeğini volume'a aktarın; bu paket yerel müşteri verilerini içermez.

Docker giriş betiği volume kökünün sahipliğini ayarlamak için başlar, ardından `gosu app` ile uygulamayı UID 1654 altında çalıştırır. Dosyalar yalnızca uygulama kullanıcısına açık izinlerle oluşturulur; chmod 777 kullanılmaz. Başka sistemden aktarılan dosyaların sahipliği de uygulama kullanıcısına uygun olmalıdır. Data Protection anahtarları bu Linux kurulumunda ayrıca şifrelenmez; volume erişimini ve yedeklerini koruyun. Aynı volume ve sabit `CatalogGenerator` application name ile oturum cookie'leri restart sonrası çözülebilir.

MVP'yi **tek replica** ile çalıştırın. SQLite ve yarım kalan üretim/token iade kurtarması buna göre düzenlenmiştir. Volume bağlı redeploy kısa kesinti oluşturabilir. Üretim sırasında deploy başlatmamayı tercih edin. Uzun AI işlemlerinin gerçek Railway bağlantı süresini ve kaynak kullanımını kendi maksimum renk sayınızla doğrulayın.

Production admin değişkenleri eksikse warning yazılır, uygulama ve health çalışır; Development varsayılan hesabı oluşturulmaz. Mevcut adminin parolası redeploy sırasında sıfırlanmaz. API anahtarı eksikse üretim ekranında “OpenAI API yapılandırması bulunamadı.” mesajı gösterilir. Healthcheck OpenAI erişimini veya kotasını ölçmez; SQLite erişimini kontrol eder.

Production forwarded headers ayarı Railway proxy'sine güvenir; container'a doğrudan genel ağ erişimi açmayın. Identity cookie Secure, HttpOnly ve SameSite=Lax kullanır. HTTPS yönlendirmesi öncesinde forwarded headers işlenir; health HTTP üzerinden de çalışır. İsteğe bağlı `AllowedHosts` kısıtlamasına kendi domainlerinizin yanında `healthcheck.railway.app` ekleyin (noktalı virgülle ayrılır). Varsayılan değer Railway domain oluşturma akışı için `*` şeklindedir.

Upload toplam sınırı 215 MB; her görsel 10 MB ve 24 megapiksel ile sınırlıdır. MIME, uzantı ve görsel decode kontrolü yapılır. Production logları console üzerinden izlenir; secret, parola ve cookie değerleri uygulama loglarına yazılmaz.

### Custom domain

**Settings → Networking → Custom Domain** üzerinden alan adınızı ekleyin. DNS sağlayıcınızda Railway ekranının verdiği kayıt türünü ve hedef değerini aynen kullanın. DNS doğrulaması ve TLS tamamlandıktan sonra HTTPS erişimini ve giriş akışını kontrol edin.

### Yerel production doğrulaması

Proje klasöründe:

```powershell
dotnet restore
dotnet build -c Release
dotnet test -c Release
docker build -t catalog-app .
docker run -d --name catalog-local -e ASPNETCORE_ENVIRONMENT=Production -e PORT=8080 -e APP_DATA_PATH=/app/App_Data -p 127.0.0.1:8080:8080 -v catalog-local-data:/app/App_Data catalog-app
Invoke-WebRequest http://localhost:8080/health
```

Bu komutta API/admin secret verilmeden health 200 dönmelidir. Production giriş testi HTTPS reverse proxy üzerinden yapılmalıdır. İşiniz bittiğinde `docker stop catalog-local` ve `docker rm catalog-local` ile test container'ını kaldırabilirsiniz; named volume kalır.

Teslimdeki `Verification/Run-SmokeTests.ps1` console tabanlı entegrasyon doğrulamasını çalıştırır; ana projede ayrı test SDK projesi olmadığından `dotnet test` tek başına bu kontrolleri çalıştırmaz. 74 kontrol; üyelik, yetkiler, logo, katalog, token, yeniden üretim, production admin, secure cookie, forwarded HTTPS ve restart sonrası oturum korumasını kapsar. OpenAI çağrıları testte taklit edilir; bu deployment doğrulamasında ücretli gerçek üretim yapılmadı. Release build 0 uyarı/0 hata, Docker build başarılı; gerçek Linux container health 200 ve kalıcı volume restart testi başarılı.

Kaynaklar: [Railway volumes](https://docs.railway.com/volumes), [healthchecks](https://docs.railway.com/deployments/healthchecks), [domain yönetimi](https://docs.railway.com/networking/domains/working-with-domains).

## Gereksinimler

- .NET 10 SDK (geliştirmede 10.0.302 ile doğrulandı).
- NuGet erişimi.
- Gerçek katalog üretimi için ilgili modellere erişebilen OpenAI API hesabı ve anahtarı.
- ImageSharp metin çizimi için işletim sisteminde Arial veya başka bir font. Linux'ta bir TrueType font yükleyin veya `Catalog__FontPath` ortam değişkenine fontun mutlak yolunu verin.
- SixLabors paketlerinin lisans koşullarını kullanımınıza göre değerlendirin: https://sixlabors.com/pricing/

## Çalıştırma

Komutları `CatalogStudio.csproj` dosyasının bulunduğu klasörde çalıştırın:

```powershell
dotnet restore
dotnet tool restore
dotnet ef database update
dotnet run --launch-profile http --urls http://localhost:5187
```

Tarayıcıda http://localhost:5187 adresini açın. Launch profile Development ortamını kullanır. Kök klasördeki `CatalogStudio.sln` Visual Studio ile açılabilir.

### API anahtarı

Anahtarı repoya veya appsettings.json'a yazmayın. Development için:

```powershell
dotnet user-secrets set "OpenAI:ApiKey" "BURAYA_API_ANAHTARI"
```

Alternatif olarak yalnızca mevcut PowerShell oturumu için:

```powershell
$env:OPENAI_API_KEY = "BURAYA_API_ANAHTARI"
dotnet run --launch-profile http --urls http://localhost:5187
```

`OPENAI_API_KEY` varsa user-secrets değerinden önceliklidir. Anahtarı sohbet mesajına yapıştırmanız gerekmez.

## Kullanım

1. Hesap oluşturun; ardından giriş yapın.
2. Marka adı ve logo girin. Daha önce logo yüklediyseniz her yeni katalogda tekrar kullanmayı açıkça seçin veya yeni logo yükleyin.
3. Pozitif tam sayı ürün kodunu ve yaş aralığını girin.
4. Model türünü ve toplam renk sayısını (1–10) seçin; istenen ürün fotoğraflarını yükleyin.
5. Standart veya özel fotoğraf boyutunu seçip üretimi başlatın. Tamamlanana kadar sayfayı açık tutun.
6. Sonuç ekranından markalı kataloğu ve her rengin ayrı beyaz fonlu PNG fotoğrafını indirin. Yalnızca son katalog paketi saklanır.

PNG, JPEG ve WebP; dosya başına 10 MB, görsel başına 24 megapiksel sınırı uygulanır. Görseller sunucuda yeniden PNG olarak kodlanır; EXIF kaldırılır. Tarayıcı önizlemeleri ve sürükle-bırak desteklenir. Hatalı gönderimden sonra dosyaları tekrar seçmek gerekir.

## Görsel üretimi

- Ayrı metin analizi yoktur; ürün fotoğrafı doğrudan görsel düzenleme isteğine gönderilir.
- Referansla görsel düzenleme: istenen `gpt-image-2`, Images Edits API.
- Görsel modeli OpenAI:ImageModel ayarıyla değiştirilebilir; mevcut model gpt-image-2 olarak korunmuştur.
- GPT Image 2 için `input_fidelity` gönderilmez; `output_format=png`, `size=1024x1536`, `quality=medium` kullanılır.
- Her ana/renk görseli kendi referans fotoğrafıyla doğrudan düzenlenir. Logo AI'a gönderilmez.
- Final canvas, gerçek logo, kod ve İngilizce yaş metni C# / ImageSharp ile birleştirilir.
- Her API isteğinin 5 dakika zaman aşımı vardır; iptal belirteci aktarılır. Yalnızca açık 429 cevapları en fazla iki kez yeniden denenir. Belirsiz hatalarda çift ücret riskini önlemek için otomatik üretim tekrarı yapılmaz.
- Başarısız işlemin dosyaları ve başarılı işlemin ara görselleri temizlenir; orijinal ana ürün, son PNG ve kayıtlı logo tutulur.
- N renk için N ana/ürün düzenlemesi ve N beyaz model düzenlemesi, toplam 2N AI isteği yapılır. En fazla üç istek paralel çalışır.
- AI ürün ayrıntılarını değiştirebilir. Yayın öncesinde renk, baskı, kesim ve dikişleri orijinal ürünle karşılaştırın. Piksel düzeyinde aynı ürün garantisi verilmez.

Resmî kaynaklar (10 Eylül 2026):
- https://developers.openai.com/api/docs/models/gpt-image-2
- https://developers.openai.com/api/docs/guides/image-generation
- https://developers.openai.com/api/docs/guides/images-vision

## Identity ve admin

Development seed:
- E-posta: `admin@catalog.local`
- Şifre: `Admin1234!`
- Yönetim: `/Admin`

Register yalnızca User rolünü verir. Admin rolü sunucu seed'i veya mevcut admin tarafından verilir. Şifreler Identity ile hashlenir; en az 8 karakter, büyük/küçük harf ve rakam gerekir. Beş başarısız giriş kilitleme uygular.

Admin panelinde kullanıcı durumu/rolleri/silme, marka listesi, katalog önizleme ve silme, toplamlar ve son kayıtlar vardır. Admin kendi hesabını silemez, pasifleştiremez veya kendi Admin rolünü kaldıramaz. Rol değişiklikleri güvenlik damgasını yeniler; her istekte kontrol edilir.

Şifremi unuttum ekranı temel yapı olarak vardır. Bu sürüm e-posta göndermez ve sıfırlama bağlantısı üretmez; ekranda bu durum açıkça belirtilir. Gerçek e-posta göndericisi ve token tabanlı reset akışı ileride tamamlanmalıdır.

### Production

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ADMIN_EMAIL = "yonetici@example.com"
$env:ADMIN_PASSWORD = "GUCLU_BENZERSIZ_SIFRE"
$env:AllowedHosts = "katalog.example.com"
dotnet run --no-launch-profile
```

Production'da admin ortam değişkenleri yoksa varsayılan hesap oluşturulmaz ve warning yazılır. Mevcut hesabın parolası veya rolü başlangıçta tekrar değiştirilmez. Production için **yeni bir veritabanı** kullanın; Development veritabanını bilinen seed hesabıyla taşımayın. HTTPS ve kalıcı Data Protection anahtar deposu yapılandırın. Reverse proxy'nin upload ve yanıt zaman aşımı limitlerini uzun üretim isteklerine göre ayarlayın.

## Veritabanı ve depolama

`Migrations/` klasöründe InitialIdentityCatalog migration'ı ve snapshot bulunur. Uygulama başlangıcında bekleyen migration'lar uygulanır; `EnsureCreated` kullanılmaz. Roller ve ilk admin idempotent oluşturulur.

Varsayılan SQLite: `App_Data/catalog.db`.
Özel dosyalar: `App_Data/files/`.
Bağlantı değişkeni: ConnectionStrings__DefaultConnection; eski ConnectionStrings__Default da desteklenir.
Dosya deposu değişkeni: `Storage__Root`.

Dosyalar wwwroot dışında tutulur; yalnızca yetkili controller üzerinden sunulur. Normal kullanıcı başka kullanıcının kayıtlarına, katalog PNG'sine veya logosuna ulaşamaz. Admin yönetim yetkisiyle logo ve katalogları görüntüler. Değişiklik yapan POST işlemlerinde global anti-forgery doğrulaması vardır. Veritabanı ve özel dosyaları birlikte yedekleyin.

## Dosya yapısı

```
Areas/Admin/Controllers/   Dashboard, kullanıcı, marka ve katalog yönetimi
Areas/Admin/Views/         Admin Razor ekranları
Controllers/              Home, Account, Catalog
Data/AppDbContext.cs      Identity + uygulama tabloları
Models/Domain.cs          AppUser, UserPreference, Catalog, CatalogRequest, AgeRange, AgeUnit
ViewModels/               Giriş, kayıt ve profil form modelleri
Services/                 OpenAI, kompozisyon, yaş, prompt, dosya, marka, seed
Migrations/               EF Core migration ve snapshot
Views/                    Türkçe Razor ekranları ve iki ayrı layout
wwwroot/                  Yerel Bootstrap, CSS, JavaScript
App_Data/                 Yerel veritabanı ve özel dosyalar (pakete dahil edilmez)
```


## Çıktılar, boyut ve fiyatlandırma

Önü veya arkası baskılı model türünü seçtikten sonra toplam renk sayısı (1–10) sorulur. İlk/manekenin giyeceği renk bu sayıya dahildir. Önü baskılı akışta ilk renk için ana ürün fotoğrafı, kalan renklerin her biri için bir fotoğraf yüklenir. Tek renk için başka renk fotoğrafı gerekmez. Arkası baskılı akışta her rengin ön fotoğrafı ve yalnızca ilk rengin arka fotoğrafı yüklenir.

- Birden fazla renk: markalı ve ürün kodlu toplu katalog + her renk için ayrı beyaz fonlu manken fotoğrafı.
- Tek renk: güzel arka planlı, markalı ve ürün kodlu tam alan manken fotoğrafı + beyaz fonda marka/kod eklenmeyen tek manken fotoğrafı.
- Arkası baskılı ana katalogda iki manken önden/arkadan görünür. İlk renk sağdaki ürünlerde tekrarlanmaz. Ayrı beyaz fonlu fotoğraflar renklerin önden mankenli görünüşlerini gösterir.
- Standart boyut tüm çıktılar için 1400 × 1100 pikseldir. Özel boyutta genişlik/yükseklik ayrı girilir: her biri 256–4096 piksel. Çıktılar PNG olarak tam bu ölçülerde hazırlanır; oran bozulmaması için gerektiğinde düz fonla boşluk eklenir. Bu ayar nihai dosya boyutudur, AI'ın yerel üretim çözünürlüğünü artırmaz.
- Yeni üretim: toplam renk sayısı × 75 token. Örnek: 1 renk 75, 4 renk 300, 10 renk 750 token.
- Yeniden üretim: renk sayısından bağımsız sabit 25 token; ana katalog ve tüm beyaz fonlu görseller yeniden hazırlanır, aynı ölçüler korunur.
- Sonuç ekranında tüm fotoğraflar ayrı önizlenir ve indirilir. WhiteImage endpointi yalnızca sahibi veya Admin için açıktır.

Başlangıç bakiyesi 200 token, adminin kullanıcı adı/e-posta ile token ekleme/çıkarma/ayarlama işlevleri korunur. Krediler uygulama içi birimlerdir; OpenAI API token ücretlerinden bağımsızdır. Kesinti koşullu atomik SQL güncellemesiyle yapılır. Katalog ve tüm beyaz çıktılar başarıyla kaydedilmeden işlem tamamlanmış sayılmaz. Başarısız üretimde rezervasyon bir kez iade edilir; önceki katalog korunur.

## Katalog saklama ve temizleme

Her kullanıcı için yalnızca son başarılı katalog paketi tutulur. Yeni üretim veya yeniden üretim tamamlandığında önceki ana çıktı, beyaz fonlu fotoğraflar ve kaynak görseller silinir. Kayıtlı marka logosu korunur. Startup sırasında da birikmiş eski kataloglar temizlenir. Katalog silmek token iadesi yapmaz. Yedekler bu temizlikten bağımsızdır.

Ana fotoğraf, renk kaynakları, logo kopyası, marka/cep/cinsiyet/yaş/seri ve nihai ölçüler tekrar üretim için saklanır. Eski kataloglarda beyaz çıktı listesi boş, boyut 1400 × 1100 olarak migrate edilir; tekrar üretim yeni çıktıları oluşturur. Kaynakları geçmişte silinmiş kataloglar tekrar üretilemez.

Tek instance kullanın. Startup'ta Pending rezervasyonlar iade edilir; çok sunuculu iş sahipliği ve kalıcı iş kuyruğu bu sürümde yoktur. İşlem sırasında süreç zorla kapatılırsa dosya bakımı gerekebilir.

## Görsel kuralları ve mobil arayüz

Cepsiz seçimi referanstaki cep koruma talimatının önüne geçer; ön/yan/arka ceplerin kaldırılması ve ellerin görünür olması istenir. Tek parça ürün sade beyaz tamamlayıcı kıyafetle giydirilir. Logo AI'a gönderilmez; katalog kompozisyonunda arka planı yerel olarak kaldırılır. Beyaz model fotoğrafında ek logo, ürün kodu, yaş, seri metni ve dekor bulunmaz; ürünün orijinal baskısı korunur.

Telefonda sabit alt menü, üstte bakiye, büyük form alanları ve ayrı fotoğraf indirme kartları vardır. 390 piksel mobil ve 1440 piksel masaüstünde yeni formlar incelendi; özel boyut alanları ve renk ücretleri kontrol edildi.

## Doğrulama

Release build 0 hata / 0 uyarı ile başarılı. 127 entegrasyon kontrolü geçti: üyelik, admin, yetkiler, logo, renk başına ücret, sabit yeniden üretim, tek renk ön/arka akış, her rengin beyaz çıktısı, boyutlar, yetkili indirme, başarısız üretimde iade ve dosya temizliği, kalıcı cookie ve production health.

Verification/Run-SmokeTests.ps1, console test projesini kendi work klasöründe oluşturur. Testler MVC, Identity, SQLite, ImageSharp ve gerçek API istemcisi üzerinden çalışır; yalnızca OpenAI HTTP yanıtları taklit edilir. Ücretli gerçek görsel üretimi bu güncellemede yapılmadı; beyaz fon, ürün benzerliği ve cep kuralının görsel doğruluğu gerçek örneklerle ayrıca değerlendirilmelidir.

OpenAI Docs: [görsel üretimi için talimatlar](https://developers.openai.com/api/docs/guides/image-prompting).
