# Hafta 7 entegrasyon testi
# Projeyi derler, siteyi arka planda açar ve canlı oturum akışını otomatik dener:
# öğretmen girişi -> oturum aç -> öğrenci girişi -> katıl -> öğretmen katılımcıyı görür -> oturumu iptal et.
# Test kendi açtığı oturumu sonunda iptal eder, veritabanında iz bırakmaz.
# Çalıştırmak için: scripts\hafta7-test.bat dosyasına çift tıkla.

$root = Split-Path $PSScriptRoot -Parent
$log  = Join-Path $PSScriptRoot "hafta7-test-sonuc.txt"
$url  = "http://localhost:5199"
Set-Content -Path $log -Value "Hafta 7 test - $(Get-Date)" -Encoding UTF8
$hata = 0

function Yaz($mesaj) { Write-Host $mesaj; Add-Content -Path $log -Value $mesaj -Encoding UTF8 }
function Kontrol($sart, $mesaj) {
    if ($sart) { Yaz "[OK]   $mesaj" } else { Yaz "[HATA] $mesaj"; $script:hata++ }
}
# Formdaki CSRF anahtarını sayfadan okur
function Anahtar($html) {
    if ($html -match 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"') { return $matches[1] }
    return ""
}
function Sayfa($adres, $oturum) { Invoke-WebRequest "$url$adres" -WebSession $oturum -UseBasicParsing }
function Gonder($adres, $form, $oturum) {
    Invoke-WebRequest "$url$adres" -Method Post -Body $form -WebSession $oturum -UseBasicParsing
}
function Giris($eposta, $parola) {
    $oturum = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    $s = Sayfa "/Account/Login" $oturum
    $null = Gonder "/Account/Login" @{ Email = $eposta; Password = $parola; __RequestVerificationToken = (Anahtar $s.Content) } $oturum
    return $oturum
}

# 1) Derleme
Yaz "`n--- 1) Derleme ---"
$cikti = & dotnet build "$root\QuizArena.sln" -nologo -v q 2>&1
$cikti | Select-Object -Last 6 | ForEach-Object { Yaz "       $_" }
Kontrol ($LASTEXITCODE -eq 0) "Proje hatasız derlendi"
if ($LASTEXITCODE -ne 0) {
    Yaz "Derleme olmadı. Visual Studio'da proje çalışıyorsa durdur (Shift+F5) ve testi tekrar çalıştır."
    exit 1
}

# 2) Siteyi arka planda aç
$env:ASPNETCORE_ENVIRONMENT = "Development"
Yaz "`n--- 2) Site açılıyor ---"
$siteLog = Join-Path $PSScriptRoot "hafta7-site-log.txt"
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
    Yaz "       Site kapandı mı: $($site.HasExited)"
    if (Test-Path $siteLog) { Get-Content $siteLog -Tail 15 | ForEach-Object { Yaz "       site> $_" } }
    if (Test-Path "$siteLog.err") { Get-Content "$siteLog.err" -Tail 15 | ForEach-Object { Yaz "       hata> $_" } }
}

try {
    if ($acik) {
        # 3) Öğretmen
        Yaz "`n--- 3) Öğretmen ---"
        $ogretmen = Giris "ogretmen@quizarena.test" "Ogretmen123!"
        $liste = Sayfa "/Session" $ogretmen
        Kontrol ($liste.Content -match "Oturumlar") "Öğretmen girişi ve 'Oturumlarım' sayfası"

        if ($liste.Content -match "Bekleme salonu|Devam ediyor") {
            Yaz "[BİLGİ] Açık bir oturumun var, test ona dokunmamak için oturum adımlarını atlıyor."
            Yaz "        Oturumlarım sayfasından o oturumu bitir/iptal et, sonra testi tekrar çalıştır."
        }
        else {
            # Yayındaki ve sorusu olan bir quiz bul ("Canlı oturum başlat" butonu olan)
            $quizler = Sayfa "/Quiz" $ogretmen
            $idler = [regex]::Matches($quizler.Content, '/Quiz/Details/(\d+)') | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique
            $quizId = $null; $detay = $null
            foreach ($id in $idler) {
                $d = Sayfa "/Quiz/Details/$id" $ogretmen
                if ($d.Content -match "Session/Create\?quizId=$id") { $quizId = $id; $detay = $d; break }
            }
            Kontrol ($quizId -ne $null) "Yayında bir quiz var (Canlı oturum başlat butonu görünüyor)"

            if ($quizId) {
                $host1 = Gonder "/Session/Create?quizId=$quizId" @{ __RequestVerificationToken = (Anahtar $detay.Content) } $ogretmen
                $sessionId = $host1.BaseResponse.ResponseUri.AbsolutePath.Split("/")[-1]
                $kod = ""
                if ($host1.Content -match 'id="join-code">([A-Z0-9]{6})<') { $kod = $matches[1] }
                Kontrol ($kod -ne "") "Oturum açıldı, katılım kodu: $kod"

                $bos = Gonder "/Session/Start/$sessionId" @{ __RequestVerificationToken = (Anahtar $host1.Content) } $ogretmen
                Kontrol ($bos.Content -match "En az bir") "Kural: katılımcı yokken oturum başlamıyor"

                # 4) Öğrenci
                Yaz "`n--- 4) Öğrenci ---"
                $ogrenci = Giris "ogrenci1@quizarena.test" "Ogrenci123!"
                $panel = Sayfa "/Student" $ogrenci
                Kontrol ($panel.Content -match "Oturuma katıl") "Öğrenci girişi ve panel"

                $yanlis = Gonder "/Student/Join" @{ "Join.JoinCode" = "ZZZZZZ"; "Join.Nickname" = "TestAli"; __RequestVerificationToken = (Anahtar $panel.Content) } $ogrenci
                Kontrol ($yanlis.Content -match "bulunamad") "Kural: yanlış kodla katılınamıyor"

                $panel = Sayfa "/Student" $ogrenci
                $oyun = Gonder "/Student/Join" @{ "Join.JoinCode" = $kod.ToLower(); "Join.Nickname" = "TestAli"; __RequestVerificationToken = (Anahtar $panel.Content) } $ogrenci
                Kontrol ($oyun.Content -match "Bekleme salonundas") "Küçük harfle yazılan kodla katıldı, bekleme salonunda"

                $durum = (Sayfa "/Student/PlayState/$sessionId" $ogrenci).Content | ConvertFrom-Json
                Kontrol ($durum.participantCount -ge 1) "Öğrenci durum bilgisi (PlayState) çalışıyor"

                # 5) Öğretmen katılımcıyı görüyor mu?
                Yaz "`n--- 5) Entegrasyon ---"
                $state = (Sayfa "/Session/State/$sessionId" $ogretmen).Content | ConvertFrom-Json
                Kontrol ($state.participants -contains "TestAli") "Öğretmen ekranı katılımcıyı görüyor (State)"

                $yasak = Sayfa "/Session/Host/$sessionId" $ogrenci
                Kontrol ($yasak.Content -notmatch 'id="join-code"') "Öğrenci öğretmen ekranına giremiyor"

                # 6) Temizlik: test oturumunu iptal et
                Yaz "`n--- 6) Temizlik ---"
                $h = Sayfa "/Session/Host/$sessionId" $ogretmen
                $null = Gonder "/Session/Cancel/$sessionId" @{ __RequestVerificationToken = (Anahtar $h.Content) } $ogretmen
                $son = Sayfa "/Session" $ogretmen
                Kontrol ($son.Content -notmatch "Bekleme salonu") "Test oturumu iptal edildi (veritabanı temiz)"
            }
        }
    }
}
catch {
    Yaz "[HATA] Beklenmeyen hata: $($_.Exception.Message)"
    $hata++
}
finally {
    # Siteyi kapat
    if ($site) { & taskkill /PID $site.Id /T /F 2>&1 | Out-Null }
}

Yaz ""
if ($hata -eq 0) { Yaz "SONUÇ: TÜM KONTROLLER BAŞARILI" } else { Yaz "SONUÇ: $hata kontrol başarısız" }
