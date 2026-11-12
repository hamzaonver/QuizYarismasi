# Vize Demo Senaryosu — QuizArena v0.8.0

**Süre:** 7–8 dakika · **Gösterilecek:** Kullanıcı + veritabanı + çekirdek modüller uçtan uca çalışıyor (Hafta 8 PDF)

## Demo linki (hoca link isterse)

İki link var:

| Link | Ne işe yarar | Nasıl alınır |
|---|---|---|
| **Kod linki** (sabit) | Projenin kodu, belgeler, sürüm geçmişi | `https://github.com/hamzaonver/QuizYarismasi` |
| **Canlı demo linki** (her seferinde değişir) | Sitenin kendisi; hoca ve arkadaşlar kendi telefonundan/bilgisayarından açar, öğrenciler telefondan oturuma katılabilir | `scripts\demo-link.bat` → çift tıkla |

**Canlı demo linki nasıl açılır:**
1. Visual Studio'da site çalışıyorsa durdur (Shift+F5).
2. `scripts\demo-link.bat` dosyasına çift tıkla, ~30 saniye bekle.
3. Pencerede yeşil yazıyla **`DEMO LİNKİ: https://xxxxxxxx.lhr.life`** çıkar, üstünde telefonla okutulabilen bir **QR kod** vardır. Link panoya kopyalanır ve tarayıcıda kendiliğinden açılır. "Kontrol: link internetten açılıyor (OK)" yazısını gör.
4. Linki hocaya / sınıf grubuna gönder (Ctrl+V). Arkadaşların telefondan açıp öğrenci olarak kayıt olup oturuma katılabilir.
5. Demo bitince siyah pencerede **Enter**'a bas → site ve link kapanır.

**Bilmen gerekenler:**
- Link **sadece o pencere açıkken** ve bilgisayar internete bağlıyken çalışır. Pencereyi kapatma. (Betik açıkken bilgisayar kendiliğinden uykuya geçmez.)
- Ücretsiz tünel adresi ara sıra **kendiliğinden değişebilir** ya da bağlantı kopabilir. Betik bunu fark edip yeniden bağlanır ve sarı yazıyla **"DİKKAT: ... yeni linki paylaş!"** der; o zaman yeni linki tekrar paylaş. Eski link genelde bir süre daha çalışmaya devam eder.
- Linke girenler ana sayfada **"Nasıl denenir?"** kutusunu, giriş ve kayıt sayfalarında demo hesaplarını ve öğretmen davet kodunu (`OGRETMEN-2026`) görür. Yani hoca kendi başına kayıt olup deneyebilir.
- Link **her çalıştırmada değişir** → sunumdan hemen önce aç, o anki linki ver.
- Hesap gerekmiyor; Windows'taki hazır `ssh` programıyla ücretsiz tünel (localhost.run) kullanılıyor. O olmazsa betik kendiliğinden serveo.net'i dener.
- Okul ağı engellerse: telefonundan **hotspot** açıp bilgisayarı ona bağla, betiği tekrar çalıştır.
- Link açıkken siteye herkes demo hesaplarıyla girebilir; demo bitince kapat.
- Hoca "neden link değişiyor?" derse: *"Ücretsiz geçici tünel kullanıyorum. Kalıcı yayın (dağıtım) PDF'e göre 14. haftanın konusu."*

## Sunumdan önce (5 dakika, evde ya da sınıfa girmeden)

1. Visual Studio → `QuizArena.sln` → başlangıç projesi **QuizArena.Web** → **F5**. Site açılmalı.
2. Öğretmen olarak gir → **Oturumlarım** → "Devam ediyor" ya da "Bekleme salonu" yazan eski bir oturum varsa **Aç** → **Oturumu erken bitir** / **İptal et**. (Demo temiz başlasın.)
3. Tarayıcıda **iki pencere** hazırla, yan yana koy:
   - Normal pencere → **öğretmen**
   - Gizli pencere (Ctrl+Shift+N) → **öğrenci** (aynı adresi yapıştır)
4. Visual Studio'da şu sekmeleri açık bırak: `SessionService.cs`, `QuizService.cs`, `docs/mimari.md`
5. GitHub'da depo sayfası açık olsun: `https://github.com/hamzaonver/QuizYarismasi` → sağda **Releases → v0.8.0**
6. (İsteğe bağlı) `scripts\entegrasyon-testi.bat` → çift tıkla → "TÜM KONTROLLER BAŞARILI (38 kontrol)" ekranını hazır tut

| Rol | E-posta | Parola |
|---|---|---|
| Öğretmen | `ogretmen@quizarena.test` | `Ogretmen123!` |
| Öğrenci | `ogrenci1@quizarena.test` | `Ogrenci123!` |
| Öğretmen davet kodu | `OGRETMEN-2026` | |

---

## 1. Giriş (30 sn)

> "QuizArena, öğretmenin quiz hazırlayıp sınıfta canlı oturum açtığı, öğrencilerin katılım koduyla bağlandığı bir web uygulaması. C# ASP.NET Core MVC, Entity Framework Core ve SQLite kullandım. Bugün **v0.8.0 ara sürümünü** gösteriyorum: kullanıcı yönetimi, veritabanı, quiz oluşturma ve canlı oturum modülleri birlikte çalışıyor."

Sayfanın en altındaki **v0.8.0** yazısını göster.

## 2. Kullanıcı modülü — Hafta 5 (1 dk)

**Gizli pencere (öğrenci):**
1. **Kayıt ol** → Ad: *Vize Öğrenci*, e-posta: `vize@ornek.test`, parola: `abc` → **"parola kuralı" hatası**
   > "Parola en az 8 karakter, büyük-küçük harf ve rakam içermeli; veritabanına düz metin değil BCrypt özeti kaydediliyor."
2. Parola: `Vize1234` (iki kutuya da) → Rol: Öğrenci → **Kayıt ol** → öğrenci paneline düştü
3. Adres çubuğuna `/Quiz` yaz → **"Erişim engellendi"**
   > "Rol kontrolü: öğrenci öğretmen sayfalarına giremiyor."

## 3. Quiz modülü — Hafta 6 (1,5 dk)

**Normal pencere (öğretmen):** `ogretmen@quizarena.test` ile giriş
1. **+ Yeni quiz** → Başlık: *Vize Demo Quizi* → **Oluştur ve soru ekle**
2. **Yayınla**'ya bas → **"en az bir soru olmalıdır"**
3. **+ Soru ekle** → Soru: *Bilgisayarın beyni hangisidir?* → şıklar: RAM / **İşlemci (CPU)** / Klavye / Monitör → doğru şık: İşlemci → **Soruyu ekle**
   > "Her soruda 2–6 şık ve tek doğru cevap var, süre ve puan ayarlanabiliyor. Doğru şık sadece öğretmen ekranında işaretli."
4. **Yayınla** → "Quiz yayınlandı"

## 4. Canlı oturum — Hafta 7 (2 dk)

1. **Öğretmen:** ▶ **Canlı oturum başlat** → büyük katılım kodu
2. **Öğrenci:** Panelim → kodu **küçük harfle** yaz, takma ad *Ali* → **Oturuma katıl** → bekleme salonu
3. **Öğretmen ekranına bak** (dokunma) → 3 saniye içinde *Ali* listeye geldi
   > "Sayfa her 3 saniyede bir sunucuya durumu soruyor. 9. haftada bunu WebSocket ile anlık yapacağım."
4. **Öğretmen:** **Oturumu başlat** → öğrenci ekranı kendiliğinden soruya geçti
   > "Öğrenciye doğru cevap bilgisi hiç gönderilmiyor, sayfa kaynağında da yok."
5. **Öğretmen:** **Son soru · Oturumu bitir** → öğrenci "Oturum sona erdi" görüyor

## 5. Hafta 8 — Entegrasyon, hata düzeltme, sürüm (1,5 dk)

1. **Hata düzeltmesi göster:** Öğretmen → başka bir yayındaki quizden (ör. *Bilgisayar Donanımı Temelleri*) oturum aç → quizin sayfasına dön → **Yayından kaldır** → **"Bu quizin açık bir oturumu var. Önce oturumu bitirin."**
   > "Bu bir entegrasyon hatasıydı: oturum açıkken quiz taslağa alınabiliyordu. `QuizService` artık önce açık oturum var mı diye bakıyor." → Oturumu iptal et.
2. **Entegrasyon testi:** `scripts\entegrasyon-testi.bat` ekranını göster (ya da çift tıkla, ~1 dk)
   > "Bu test kayıt olmaktan oturum bitirmeye kadar her şeyi otomatik deniyor, 38 kontrol. Gerçek veritabanına dokunmuyor, kendine geçici bir veritabanı açıp sonra siliyor. Eski haftaların bozulmadığını da kontrol ediyor; buna regresyon testi deniyor."
3. **GitHub:** Releases → **v0.8.0** ve `CHANGELOG.md`
   > "Ara sürümü Git tag ile etiketledim. Semantic versioning kullanıyorum: 0 = geliştirme aşaması, 8 = sekizinci haftanın özellikleri, son 0 = henüz yama yok."

## 6. Kapanış (20 sn)

> "Şu an kullanıcı, quiz ve oturum modülleri uçtan uca çalışıyor. Sonraki adım 9. hafta: WebSocket ile gerçek zamanlı yarışma ve öğrencinin cevap vermesi. Ardından yapay zekâyla soru üretimi, puan ve lider tablosu gelecek."

---

## Bir şey ters giderse

| Durum | Ne yaparsın |
|---|---|
| "Canlı oturum başlat" çıkmıyor | Quiz taslakta ya da sorusu yok → soru ekle, **Yayınla** |
| Oturum açınca eski bir oturum açıldı | O quizin bitmemiş oturumu var (kural) → bitir/iptal et, yenisini aç |
| Öğretmen ile öğrenci girişi karıştı | Öğrenciyi **gizli pencerede** aç |
| F5'te "Uygulama Denetimi ilkesi ... engelledi" | Windows Güvenliği → Uygulama ve tarayıcı denetimi → Akıllı Uygulama Denetimi → Kapalı |
| Entegrasyon testi "Derleme olmadı" diyor | Visual Studio'da site çalışıyorsa durdur (Shift+F5), testi tekrar çalıştır |
| Demo linki çıkmadı ("Link alınamadı") | Okul ağı ssh'ı engelliyor olabilir → telefondan hotspot aç, `demo-link.bat`'ı tekrar çalıştır. Olmazsa demoyu kendi ekranından göster, kod linkini (GitHub) ver |
| Kayıtta "Bu e-posta adresiyle zaten bir hesap var" | Prova sırasında o e-postayla kayıt oldun → farklı bir e-posta yaz (ör. `vize2@ornek.test`) |
