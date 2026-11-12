# Hafta 8 - Vize demosu için paylaşılabilir link
#
# Siteyi bu bilgisayarda açar ve ücretsiz bir SSH tüneli (localhost.run) ile internete bağlar.
# Sonunda https://xxxxx.lhr.life gibi bir link verir. Bu linki hoca ve arkadaşlar
# kendi telefonundan / bilgisayarından açabilir (aynı Wi-Fi'da olmaları gerekmez).
#
# Önemli:
#  - Link sadece bu pencere açıkken ve bilgisayar açıkken çalışır.
#  - Her çalıştırmada link DEĞİŞİR (ücretsiz tünel). Sunumdan hemen önce çalıştır.
#  - Hesap veya kurulum gerekmez; Windows'taki hazır ssh programını kullanır.
#  - Site senin gerçek veritabanını (quizarena.db) kullanır; demo hesaplarıyla girilir.
#
# Çalıştırmak için: scripts\demo-link.bat dosyasına çift tıkla.

param([int]$Sure = 0)   # Sadece test için: 0'dan büyükse o kadar saniye sonra kendiliğinden kapanır

$root = Split-Path $PSScriptRoot -Parent
$port = 5300
$yerel = "http://localhost:$port"
$linkDosyasi = Join-Path $PSScriptRoot "demo-link.txt"

function Yaz($m, $renk = "Gray") { Write-Host $m -ForegroundColor $renk }

# 0) ssh var mı?
if (-not (Get-Command ssh -ErrorAction SilentlyContinue)) {
    Yaz "ssh bulunamadı. Ayarlar > Sistem > İsteğe bağlı özellikler > 'OpenSSH İstemcisi' ekle." Red
    exit 1
}

# 1) Bu portta eskiden kalmış bir site varsa kapat
Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue |
    ForEach-Object { Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue }

# 2) Derle
Yaz "1/3 Proje derleniyor..."
$null = & dotnet build "$root\QuizArena.sln" -nologo -v q 2>&1
if ($LASTEXITCODE -ne 0) {
    Yaz "Derleme olmadı. Visual Studio'da site çalışıyorsa durdur (Shift+F5) ve tekrar dene." Red
    exit 1
}

# 3) Siteyi arka planda aç
Yaz "2/3 Site açılıyor..."
$env:ASPNETCORE_ENVIRONMENT = "Development"
$site = Start-Process dotnet -ArgumentList "run --no-build --no-launch-profile --urls $yerel" `
    -WorkingDirectory (Join-Path $root "src\QuizArena.Web") -PassThru -WindowStyle Hidden
$acik = $false
for ($i = 0; $i -lt 90 -and -not $acik; $i++) {
    Start-Sleep 1
    try { $null = Invoke-WebRequest "$yerel/" -UseBasicParsing -TimeoutSec 5; $acik = $true } catch { }
}
if (-not $acik) {
    Yaz "Site açılmadı. 'Uygulama Denetimi' hatası varsa: Windows Güvenliği > Akıllı Uygulama Denetimi > Kapalı" Red
    if ($site) { & taskkill /PID $site.Id /T /F 2>&1 | Out-Null }
    exit 1
}

# 4) Tüneli aç (önce localhost.run, olmazsa serveo.net)
Yaz "3/3 İnternet linki alınıyor..."
$tuneller = @(
    @{ Ad = "localhost.run"; Arg = "-n -o StrictHostKeyChecking=accept-new -o ServerAliveInterval=30 -R 80:localhost:$port nokey@localhost.run"; Desen = 'https://[a-z0-9]+\.lhr\.life' },
    @{ Ad = "serveo.net";    Arg = "-n -o StrictHostKeyChecking=accept-new -o ServerAliveInterval=30 -R 80:localhost:$port serveo.net";          Desen = 'https://[a-z0-9\-]+\.serveousercontent\.com' }
)
$ssh = $null; $link = ""
foreach ($t in $tuneller) {
    $cikti = Join-Path $env:TEMP "quizarena-tunel.txt"
    Remove-Item $cikti, "$cikti.err" -ErrorAction SilentlyContinue
    $ssh = Start-Process ssh -ArgumentList $t.Arg -NoNewWindow -PassThru -RedirectStandardOutput $cikti -RedirectStandardError "$cikti.err"
    for ($i = 0; $i -lt 25 -and -not $link; $i++) {
        Start-Sleep 1
        $metin = ""
        if (Test-Path $cikti) { $metin += Get-Content $cikti -Raw }
        if (Test-Path "$cikti.err") { $metin += Get-Content "$cikti.err" -Raw }
        if ($metin -match $t.Desen) { $link = $matches[0] }
        if ($ssh.HasExited) { break }
    }
    if ($link) { break }
    if (-not $ssh.HasExited) { Stop-Process -Id $ssh.Id -Force -ErrorAction SilentlyContinue }
    Yaz "   $($t.Ad) olmadı, diğeri deneniyor..." Yellow
}

try {
    if (-not $link) {
        Yaz "Link alınamadı. İnternet bağlantını kontrol et (okul ağı ssh'ı engelliyor olabilir; telefondan hotspot dene)." Red
        Yaz "Site yine de bu bilgisayarda açık: $yerel" Yellow
    }
    else {
        # Link dışarıdan gerçekten açılıyor mu? (internet üzerinden giriş sayfasını iste)
        $calisiyor = $false
        for ($i = 0; $i -lt 5 -and -not $calisiyor; $i++) {
            try { $r = Invoke-WebRequest "$link/Account/Login" -UseBasicParsing -TimeoutSec 15; $calisiyor = ($r.Content -match "QuizArena") }
            catch { Start-Sleep 2 }
        }
        Set-Content -Path $linkDosyasi -Value @($link, "kontrol: $calisiyor") -Encoding UTF8
        try { Set-Clipboard -Value $link } catch { }
        Yaz ""
        Yaz "==============================================================" Green
        Yaz "  DEMO LİNKİ:  $link" Green
        Yaz "==============================================================" Green
        if ($calisiyor) { Yaz "  Kontrol: link internetten açılıyor (OK)" Green } else { Yaz "  Kontrol: link henüz cevap vermedi, birkaç saniye sonra tarayıcıda dene" Yellow }
        Yaz "  Link panoya kopyalandı (Ctrl+V ile yapıştırabilirsin)."
        Yaz "  Öğretmen: ogretmen@quizarena.test / Ogretmen123!"
        Yaz "  Öğrenci : ogrenci1@quizarena.test / Ogrenci123!  (ya da Kayıt ol)"
        Yaz "  Link sadece bu pencere açıkken çalışır."
        Yaz ""
        Start-Process $link   # linki varsayılan tarayıcıda aç
    }

    if ($Sure -gt 0) { Start-Sleep -Seconds $Sure }
    else { $null = Read-Host "Demo bitince kapatmak için Enter'a bas" }
}
finally {
    if ($ssh -and -not $ssh.HasExited) { Stop-Process -Id $ssh.Id -Force -ErrorAction SilentlyContinue }
    if ($site) { & taskkill /PID $site.Id /T /F 2>&1 | Out-Null }
    Yaz "Site ve link kapatıldı."
}
