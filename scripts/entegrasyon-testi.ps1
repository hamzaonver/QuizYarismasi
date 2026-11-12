# Hafta 8 - Uçtan uca entegrasyon testi (ara sürüm v0.8.0)
#
# Kullanıcı + veritabanı + çekirdek modüllerin birlikte çalıştığını otomatik dener.
# GERÇEK veritabanına (quizarena.db) DOKUNMAZ: test için geçici, boş bir veritabanı açar,
# sonunda onu siler. Bu yüzden istediğin kadar çalıştırabilirsin.
#
# Akış:  Hafta 4 veritabanı -> Hafta 5 kayıt/giriş/rol -> Hafta 6 quiz oluşturma
#        -> Hafta 7 canlı oturum -> Hafta 8 hata düzeltmeleri ve regresyon kontrolleri
#
# Çalıştırmak için: scripts\entegrasyon-testi.bat dosyasına çift tıkla.

$root   = Split-Path $PSScriptRoot -Parent
$log    = Join-Path $PSScriptRoot "entegrasyon-testi-sonuc.txt"
$url    = "http://localhost:5198"
$testDb = Join-Path $env:TEMP ("quizarena-entegrasyon-" + [guid]::NewGuid().ToString("N") + ".db")
Set-Content -Path $log -Value "QuizArena entegrasyon testi - $(Get-Date)" -Encoding UTF8
$script:hata = 0
$script:basarili = 0

function Yaz($mesaj) { Write-Host $mesaj; Add-Content -Path $log -Value $mesaj -Encoding UTF8 }
function Baslik($mesaj) { Yaz ""; Yaz "--- $mesaj ---" }
function Kontrol($sart, $mesaj) {
    if ($sart) { Yaz "[OK]   $mesaj"; $script:basarili++ } else { Yaz "[HATA] $mesaj"; $script:hata++ }
}
# Formdaki CSRF anahtarını sayfadan okur
function Anahtar($html) {
    if ($html -match 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"') { return $matches[1] }
    return ""
}
# Yönlendirmelerden sonra en son açılan sayfanın adresi
# (Windows PowerShell 5.1'de ResponseUri, PowerShell 7'de RequestMessage.RequestUri)
function Son-Adres($r) {
    $u = $r.BaseResponse.ResponseUri
    if (-not $u) { $u = $r.BaseResponse.RequestMessage.RequestUri }
    if ($u) { return $u.AbsolutePath }
    return ""
}
# Sunucunun cevabını sadeleştirir: sayfa metni (Türkçe karakterler &#x...; olarak gelir, çözülür),
# en son açılan adres ve durum kodu
function Cevap($r) {
    return [pscustomobject]@{
        Content    = [System.Net.WebUtility]::HtmlDecode($r.Content)
        Url        = Son-Adres $r
        StatusCode = [int]$r.StatusCode
    }
}
# Sayfa açar. 404 gibi hatalarda durmaz, durum kodunu döndürür.
function Sayfa($adres, $oturum) {
    try { return Cevap (Invoke-WebRequest "$url$adres" -WebSession $oturum -UseBasicParsing) }
    catch {
        $kod = 0
        if ($_.Exception.Response) { $kod = [int]$_.Exception.Response.StatusCode }
        return [pscustomobject]@{ StatusCode = $kod; Content = ""; Url = "" }
    }
}
function Gonder($adres, $form, $oturum) {
    return Cevap (Invoke-WebRequest "$url$adres" -Method Post -Body $form -WebSession $oturum -UseBasicParsing)
}
function YeniOturum { return New-Object Microsoft.PowerShell.Commands.WebRequestSession }
function Giris($eposta, $parola) {
    $oturum = YeniOturum
    $s = Sayfa "/Account/Login" $oturum
    $null = Gonder "/Account/Login" @{ Email = $eposta; Password = $parola; __RequestVerificationToken = (Anahtar $s.Content) } $oturum
    return $oturum
}
function Kayit($oturum, $ad, $eposta, $parola, $rol, $davet) {
    $s = Sayfa "/Account/Register" $oturum
    $form = @{ FullName = $ad; Email = $eposta; Password = $parola; ConfirmPassword = $parola; Role = $rol
               TeacherInviteCode = $davet; __RequestVerificationToken = (Anahtar $s.Content) }
    return Gonder "/Account/Register" $form $oturum
}
function Adres($cevap) { return $cevap.Url }

# ---------------------------------------------------------------
Baslik "1) Derleme"
$cikti = & dotnet build "$root\QuizArena.sln" -nologo -v q 2>&1
$cikti | Select-Object -Last 4 | ForEach-Object { Yaz "       $_" }
Kontrol ($LASTEXITCODE -eq 0) "Proje hatasız derlendi"
if ($LASTEXITCODE -ne 0) {
    Yaz "Derleme olmadı. Visual Studio'da proje çalışıyorsa durdur (Shift+F5) ve testi tekrar çalıştır."
    exit 1
}

# ---------------------------------------------------------------
Baslik "2) Site geçici veritabanıyla açılıyor"
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__Default = "Data Source=$testDb"
$siteLog = Join-Path $PSScriptRoot "entegrasyon-site-log.txt"
$web = Join-Path $root "src\QuizArena.Web"
$site = Start-Process dotnet -ArgumentList "run --no-build --no-launch-profile --urls $url" -WorkingDirectory $web `
    -PassThru -WindowStyle Hidden -RedirectStandardOutput $siteLog -RedirectStandardError "$siteLog.err"
$acik = $false; $sonHata = ""
for ($i = 0; $i -lt 90 -and -not $acik; $i++) {
    Start-Sleep -Seconds 1
    try { $null = Invoke-WebRequest "$url/" -UseBasicParsing -TimeoutSec 5; $acik = $true }
    catch { $sonHata = $_.Exception.Message }
    if ($site.HasExited) { break }
}
Kontrol $acik "Site açıldı ($url)"
if (-not $acik) {
    Yaz "       Son hata: $sonHata"
    if (Test-Path "$siteLog.err") { Get-Content "$siteLog.err" -Tail 10 | ForEach-Object { Yaz "       hata> $_" } }
    Yaz "       'Uygulama Denetimi ilkesi' yazıyorsa: Windows Güvenliği > Uygulama ve tarayıcı denetimi > Akıllı Uygulama Denetimi > Kapalı"
}

try {
    if ($acik) {
        # -------------------------------------------------------
        Baslik "3) Hafta 4 - Veritabanı"
        Kontrol (Test-Path $testDb) "Boş veritabanı migration ile oluşturuldu"
        $ogretmen = Giris "ogretmen@quizarena.test" "Ogretmen123!"
        $quizler = Sayfa "/Quiz" $ogretmen
        Kontrol ($quizler.Content -match "Bilgisayar Donan") "Sentetik (örnek) veri yüklendi, demo öğretmenin quizleri görünüyor"

        # -------------------------------------------------------
        Baslik "4) Hafta 5 - Kullanıcı yönetimi"
        $ogrenciA = YeniOturum
        $k = Kayit $ogrenciA "Test Ogrenci" "test.ogrenci@quizarena.test" "Test1234" "Student" ""
        Kontrol ((Adres $k) -eq "/Student") "Yeni öğrenci kayıt oldu ve öğrenci paneline yönlendirildi"

        $tekrar = Kayit (YeniOturum) "Test Ogrenci" "test.ogrenci@quizarena.test" "Test1234" "Student" ""
        Kontrol ($tekrar.Content -match "zaten bir hesap var") "Aynı e-postayla ikinci kayıt engellendi"

        $zayif = Kayit (YeniOturum) "Zayif Parola" "zayif@quizarena.test" "abc" "Student" ""
        Kontrol ((Adres $zayif) -eq "/Account/Register") "Zayıf parolayla kayıt engellendi"

        $davetsiz = Kayit (YeniOturum) "Sahte Ogretmen" "sahte@quizarena.test" "Test1234" "Teacher" "YANLIS-KOD"
        Kontrol ($davetsiz.Content -match "davet kodu ge") "Yanlış davet koduyla öğretmen olunamıyor"

        $ogretmenT = YeniOturum
        $k2 = Kayit $ogretmenT "Test Ogretmen" "test.ogretmen@quizarena.test" "Test1234" "Teacher" "OGRETMEN-2026"
        Kontrol ((Adres $k2) -eq "/Quiz") "Doğru davet koduyla öğretmen kayıt oldu"

        $yanlisGiris = YeniOturum
        $s = Sayfa "/Account/Login" $yanlisGiris
        $g = Gonder "/Account/Login" @{ Email = "test.ogrenci@quizarena.test"; Password = "Yanlis999"; __RequestVerificationToken = (Anahtar $s.Content) } $yanlisGiris
        Kontrol ($g.Content -match "parola hatal") "Yanlış parolayla giriş yapılamıyor"

        $rolKontrol = Sayfa "/Quiz" $ogrenciA
        Kontrol ((Adres $rolKontrol) -like "/Account/AccessDenied*") "Öğrenci öğretmen sayfasına (Quizlerim) giremiyor"

        $anonim = Sayfa "/Session" (YeniOturum)
        Kontrol ((Adres $anonim) -like "/Account/Login*") "Giriş yapmayan kullanıcı giriş sayfasına yönlendiriliyor"

        # -------------------------------------------------------
        Baslik "5) Hafta 6 - Quiz oluşturma"
        $f = Sayfa "/Quiz/Create" $ogretmenT
        $yeni = Gonder "/Quiz/Create" @{ Title = "Entegrasyon Testi Quizi"; Description = "Otomatik test"; DefaultTimeLimitSeconds = 20
                                         __RequestVerificationToken = (Anahtar $f.Content) } $ogretmenT
        $quizId = (Adres $yeni).Split("/")[-1]
        Kontrol ((Adres $yeni) -like "/Quiz/Details/*") "Öğretmen yeni quiz oluşturdu (Id: $quizId)"

        $bosYayin = Gonder "/Quiz/Publish/$quizId`?publish=true" @{ __RequestVerificationToken = (Anahtar $yeni.Content) } $ogretmenT
        Kontrol ($bosYayin.Content -match "en az bir soru") "Sorusu olmayan quiz yayınlanamıyor"

        $tekSik = Sayfa "/Quiz/AddQuestion?quizId=$quizId" $ogretmenT
        $hatali = Gonder "/Quiz/AddQuestion?quizId=$quizId" @{ Text = "Tek şıklı soru"; TimeLimitSeconds = 20; Points = 1000
                    "Options[0]" = "Sadece bir şık"; CorrectIndex = 0; __RequestVerificationToken = (Anahtar $tekSik.Content) } $ogretmenT
        Kontrol ((Adres $hatali) -like "/Quiz/AddQuestion*") "Tek şıklı soru kabul edilmedi (en az 2 şık kuralı)"

        foreach ($no in 1..2) {
            $sf = Sayfa "/Quiz/AddQuestion?quizId=$quizId" $ogretmenT
            $null = Gonder "/Quiz/AddQuestion?quizId=$quizId" @{ Text = "Test sorusu $no"; TimeLimitSeconds = 20; Points = 1000
                        "Options[0]" = "A şıkkı"; "Options[1]" = "B şıkkı"; "Options[2]" = "C şıkkı"; "Options[3]" = "D şıkkı"
                        CorrectIndex = 1; __RequestVerificationToken = (Anahtar $sf.Content) } $ogretmenT
        }
        $detay = Sayfa "/Quiz/Details/$quizId" $ogretmenT
        Kontrol (($detay.Content -match "Test sorusu 1") -and ($detay.Content -match "Test sorusu 2")) "2 soru eklendi"

        $yayin = Gonder "/Quiz/Publish/$quizId`?publish=true" @{ __RequestVerificationToken = (Anahtar $detay.Content) } $ogretmenT
        Kontrol ($yayin.Content -match "Quiz yay.nland") "Quiz yayınlandı"

        $baskasi = Sayfa "/Quiz/Details/$quizId" $ogretmen
        Kontrol ((Adres $baskasi) -like "/Account/AccessDenied*" -or $baskasi.StatusCode -eq 404) "Başka öğretmen bu quizi göremiyor (sahiplik kontrolü)"

        # -------------------------------------------------------
        Baslik "6) Hafta 7 - Canlı oturum ve katılım"
        $host1 = Gonder "/Session/Create?quizId=$quizId" @{ __RequestVerificationToken = (Anahtar $yayin.Content) } $ogretmenT
        $sessionId = (Adres $host1).Split("/")[-1]
        $kod = ""
        if ($host1.Content -match 'id="join-code">([A-Z0-9]{6})<') { $kod = $matches[1] }
        Kontrol ($kod -ne "") "Oturum açıldı, katılım kodu üretildi: $kod"

        $bos = Gonder "/Session/Start/$sessionId" @{ __RequestVerificationToken = (Anahtar $host1.Content) } $ogretmenT
        Kontrol ($bos.Content -match "En az bir") "Katılımcı yokken oturum başlamıyor"

        $panel = Sayfa "/Student" $ogrenciA
        $yanlis = Gonder "/Student/Join" @{ "Join.JoinCode" = "ZZZZZZ"; "Join.Nickname" = "Ali"; __RequestVerificationToken = (Anahtar $panel.Content) } $ogrenciA
        Kontrol ($yanlis.Content -match "bulunamad") "Yanlış kodla katılınamıyor"

        $panel = Sayfa "/Student" $ogrenciA
        $oyun = Gonder "/Student/Join" @{ "Join.JoinCode" = $kod.ToLower(); "Join.Nickname" = "Ali"; __RequestVerificationToken = (Anahtar $panel.Content) } $ogrenciA
        Kontrol ($oyun.Content -match "Bekleme salonundas") "Kayıt olan öğrenci küçük harfli kodla katıldı, bekleme salonunda"

        $ogrenciB = Giris "ogrenci1@quizarena.test" "Ogrenci123!"
        $panelB = Sayfa "/Student" $ogrenciB
        $ayniAd = Gonder "/Student/Join" @{ "Join.JoinCode" = $kod; "Join.Nickname" = "ali"; __RequestVerificationToken = (Anahtar $panelB.Content) } $ogrenciB
        Kontrol ($ayniAd.Content -match "kullan.l.yor") "Aynı takma ad ikinci kez kullanılamıyor"
        $panelB = Sayfa "/Student" $ogrenciB
        $null = Gonder "/Student/Join" @{ "Join.JoinCode" = $kod; "Join.Nickname" = "Ayse"; __RequestVerificationToken = (Anahtar $panelB.Content) } $ogrenciB

        $state = (Sayfa "/Session/State/$sessionId" $ogretmenT).Content | ConvertFrom-Json
        Kontrol (($state.participants -contains "Ali") -and ($state.participants -contains "Ayse")) "Öğretmen ekranı iki katılımcıyı da görüyor"

        $yasak = Sayfa "/Session/Host/$sessionId" $ogrenciA
        Kontrol ($yasak.Content -notmatch 'id="join-code"') "Öğrenci öğretmen ekranına giremiyor"

        $digerOgretmen = Sayfa "/Session/Host/$sessionId" $ogretmen
        Kontrol ($digerOgretmen.Content -notmatch 'id="join-code"') "Başka öğretmen bu oturumu yönetemiyor"

        $h = Sayfa "/Session/Host/$sessionId" $ogretmenT
        $basla = Gonder "/Session/Start/$sessionId" @{ __RequestVerificationToken = (Anahtar $h.Content) } $ogretmenT
        Kontrol ($basla.Content -match "Soru 1 / 2") "Oturum başladı, 1. soru gösteriliyor"

        $soru = Sayfa "/Student/Play/$sessionId" $ogrenciA
        Kontrol ($soru.Content -match "Test sorusu 1") "Öğrenci ekranında 1. soru görünüyor"
        Kontrol ($soru.Content -notmatch "correct") "Öğrenci sayfasında doğru cevap bilgisi yok"

        $ogrenciC = Giris "ogrenci2@quizarena.test" "Ogrenci123!"
        $panelC = Sayfa "/Student" $ogrenciC
        $gec = Gonder "/Student/Join" @{ "Join.JoinCode" = $kod; "Join.Nickname" = "Mehmet"; __RequestVerificationToken = (Anahtar $panelC.Content) } $ogrenciC
        Kontrol ($gec.Content -match "ba.lad") "Başlamış oturuma sonradan katılınamıyor"

        $sonraki = Gonder "/Session/Next/$sessionId" @{ __RequestVerificationToken = (Anahtar $basla.Content) } $ogretmenT
        $durum = (Sayfa "/Student/PlayState/$sessionId" $ogrenciA).Content | ConvertFrom-Json
        Kontrol ($durum.currentQuestionIndex -eq 1) "Sonraki soruya geçildi, öğrencinin durumu da güncellendi"

        $bitir = Gonder "/Session/Next/$sessionId" @{ __RequestVerificationToken = (Anahtar $sonraki.Content) } $ogretmenT
        $durum = (Sayfa "/Student/PlayState/$sessionId" $ogrenciA).Content | ConvertFrom-Json
        Kontrol ($durum.status -eq "Finished") "Son sorudan sonra oturum bitti, öğrenci de 'bitti' durumunu görüyor"

        # -------------------------------------------------------
        Baslik "7) Hafta 8 - Hata düzeltmeleri ve regresyon"
        # Yeni bir oturum açıp, açık oturum varken quizi yayından kaldırmayı dene
        $d = Sayfa "/Quiz/Details/$quizId" $ogretmenT
        $host2 = Gonder "/Session/Create?quizId=$quizId" @{ __RequestVerificationToken = (Anahtar $d.Content) } $ogretmenT
        $session2 = (Adres $host2).Split("/")[-1]
        $d = Sayfa "/Quiz/Details/$quizId" $ogretmenT
        $kaldir = Gonder "/Quiz/Publish/$quizId`?publish=false" @{ __RequestVerificationToken = (Anahtar $d.Content) } $ogretmenT
        Kontrol ($kaldir.Content -match "a.ık bir oturumu var") "Düzeltme: açık oturumu olan quiz yayından kaldırılamıyor"

        $h2 = Sayfa "/Session/Host/$session2" $ogretmenT
        $null = Gonder "/Session/Cancel/$session2" @{ __RequestVerificationToken = (Anahtar $h2.Content) } $ogretmenT

        $ekle = Sayfa "/Quiz/AddQuestion?quizId=$quizId" $ogretmenT
        Kontrol ($ekle.Content -match "soruları değiştirilemez") "Regresyon (Hafta 6): oturumu olan quizin soruları kilitli"

        $d = Sayfa "/Quiz/Details/$quizId" $ogretmenT
        $sil = Gonder "/Quiz/Delete/$quizId" @{ __RequestVerificationToken = (Anahtar $d.Content) } $ogretmenT
        Kontrol ($sil.Content -match "silinemez") "Regresyon (Hafta 4/6): oturumu olan quiz silinemiyor, sonuçlar korunuyor"

        $liste = Sayfa "/Session" $ogretmenT
        Kontrol ($liste.Content -match "Bitti") "Oturumlarım listesinde biten oturum görünüyor"

        $gecmis = Sayfa "/Student" $ogrenciA
        Kontrol ($gecmis.Content -match "Entegrasyon Testi Quizi") "Öğrencinin 'Katıldığım oturumlar' listesinde oturum görünüyor"

        $ana = Sayfa "/" (YeniOturum)
        Kontrol ($ana.Content -match "v0\.8\.0") "Sayfanın altında sürüm numarası görünüyor (v0.8.0)"
    }
}
catch {
    Yaz "[HATA] Beklenmeyen hata: $($_.Exception.Message)"
    $script:hata++
}
finally {
    # Siteyi kapat ve geçici veritabanını sil
    if ($site) { & taskkill /PID $site.Id /T /F 2>&1 | Out-Null }
    Start-Sleep -Seconds 1
    Remove-Item "$testDb*" -ErrorAction SilentlyContinue
    $env:ConnectionStrings__Default = $null
}

Yaz ""
Yaz "Geçici test veritabanı silindi; gerçek veritabanına dokunulmadı."
if ($script:hata -eq 0) { Yaz "SONUÇ: TÜM KONTROLLER BAŞARILI ($($script:basarili) kontrol)" }
else { Yaz "SONUÇ: $($script:hata) kontrol başarısız, $($script:basarili) kontrol başarılı" }
