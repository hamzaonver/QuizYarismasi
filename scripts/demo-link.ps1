# Hafta 8 - Vize demosu için paylaşılabilir link
#
# Siteyi bu bilgisayarda açar ve ücretsiz bir tünelle internete bağlar.
# Önce Cloudflare'in hesapsız "quick tunnel" özelliği kullanılır (https://xxxx.trycloudflare.com):
# bu link pencere açık kaldığı sürece DEĞİŞMEZ. Olmazsa SSH tüneli (localhost.run) denenir. Bu linki hoca ve arkadaşlar
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

# 0b) Daha önce açık kalmış demo-link penceresi ve tünelleri varsa kapat
Get-CimInstance Win32_Process -ErrorAction SilentlyContinue |
    Where-Object { $_.ProcessId -ne $PID -and (
        ($_.Name -like "powershell*" -and $_.CommandLine -like "*demo-link.ps1*") -or
        ($_.Name -eq "ssh.exe" -and $_.CommandLine -like "*localhost:$port*") -or
        ($_.Name -eq "cloudflared.exe")) } |
    ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }

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

# 4) Demo sürerken bilgisayarın uykuya geçmesini engelle (uyursa link kapanır)
try {
    Add-Type -Namespace QuizArena -Name Uyku -MemberDefinition '[DllImport("kernel32.dll")] public static extern uint SetThreadExecutionState(uint esFlags);'
    $null = [QuizArena.Uyku]::SetThreadExecutionState([uint32]2147483651)   # CONTINUOUS | SYSTEM_REQUIRED | DISPLAY_REQUIRED
} catch { }

# 5) Tünel: önce localhost.run, olmazsa serveo.net
$cloudflared = Join-Path $env:LOCALAPPDATA "QuizArena\cloudflared.exe"
$tuneller = @(
    @{ Ad = "Cloudflare";    Exe = $cloudflared; Arg = "tunnel --no-autoupdate --url http://localhost:$port"; Desen = 'https://(?!api\.)[a-z0-9\-]+\.trycloudflare\.com' },
    @{ Ad = "localhost.run"; Arg = "-n -o StrictHostKeyChecking=accept-new -o ServerAliveInterval=30 -R 80:localhost:$port nokey@localhost.run"; Desen = 'https://[a-z0-9]+\.lhr\.life' },
    @{ Ad = "serveo.net";    Arg = "-n -o StrictHostKeyChecking=accept-new -o ServerAliveInterval=30 -R 80:localhost:$port serveo.net";          Desen = 'https://[a-z0-9\-]+\.serveousercontent\.com' }
)
$cikti = Join-Path $env:TEMP "quizarena-tunel.txt"

# Tüneli açar, linki ve ssh sürecini döndürür
function Tunel-Ac {
    foreach ($t in $tuneller) {
        Remove-Item $cikti, "$cikti.err" -ErrorAction SilentlyContinue
        $program = "ssh"
        if ($t.Exe) {
            # Cloudflare programı yoksa resmi GitHub sayfasından bir kere indir (~60 MB)
            if (-not (Test-Path $t.Exe)) {
                Yaz "   Cloudflare tünel programı indiriliyor (bir kere, ~60 MB)..." Yellow
                try {
                    New-Item -ItemType Directory -Force -Path (Split-Path $t.Exe) | Out-Null
                    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
                    $ProgressPreference = "SilentlyContinue"
                    Invoke-WebRequest "https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-windows-amd64.exe" -OutFile $t.Exe -UseBasicParsing
                } catch { Yaz "   İndirilemedi: $($_.Exception.Message)" Yellow; Remove-Item $t.Exe -ErrorAction SilentlyContinue; continue }
            }
            $program = $t.Exe
        }
        $p = Start-Process $program -ArgumentList $t.Arg -NoNewWindow -PassThru -RedirectStandardOutput $cikti -RedirectStandardError "$cikti.err"
        for ($i = 0; $i -lt 40; $i++) {
            Start-Sleep 1
            $metin = ""
            if (Test-Path $cikti) { $metin += Get-Content $cikti -Raw }
            if (Test-Path "$cikti.err") { $metin += Get-Content "$cikti.err" -Raw }
            if ($metin -match $t.Desen) { return @{ Ssh = $p; Link = $matches[0]; Desen = $t.Desen } }
            if ($p.HasExited) { break }
        }
        if (-not $p.HasExited) { Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue }
        Yaz "   $($t.Ad) olmadı, diğeri deneniyor..." Yellow
    }
    return @{ Ssh = $null; Link = ""; Desen = "" }
}

# Linki ekrana yazar (telefon için QR kodu da gösterir)
function Link-Goster($link) {
    $calisiyor = $false
    for ($i = 0; $i -lt 5 -and -not $calisiyor; $i++) {
        try { $r = Invoke-WebRequest "$link/Account/Login" -UseBasicParsing -TimeoutSec 15; $calisiyor = ($r.Content -match "QuizArena") }
        catch { Start-Sleep 2 }
    }
    Set-Content -Path $linkDosyasi -Value @($link, "kontrol: $calisiyor", (Get-Date)) -Encoding UTF8
    try { Set-Clipboard -Value $link } catch { }

    # localhost.run çıktısındaki QR kodu (telefon kamerasıyla okutulabilir)
    if (Test-Path $cikti) {
        $satirlar = Get-Content $cikti
        $qrBasla = ($satirlar | Select-String -Pattern "QR" -SimpleMatch | Select-Object -First 1).LineNumber
        if ($qrBasla) { $satirlar | Select-Object -Skip $qrBasla | ForEach-Object { Write-Host $_ } }
    }
    Yaz ""
    Yaz "==============================================================" Green
    Yaz "  DEMO LİNKİ:  $link" Green
    Yaz "==============================================================" Green
    if ($calisiyor) { Yaz "  Kontrol: link internetten açılıyor (OK)" Green } else { Yaz "  Kontrol: link henüz cevap vermedi, birkaç saniye sonra tarayıcıda dene" Yellow }
    Yaz "  Link panoya kopyalandı (Ctrl+V ile yapıştırabilirsin)."
    Yaz "  Öğretmen: ogretmen@quizarena.test / Ogretmen123!   (öğretmen kaydı davet kodu: OGRETMEN-2026)"
    Yaz "  Öğrenci : ogrenci1@quizarena.test / Ogrenci123!   (ya da Kayıt ol)"
    Yaz "  Link sadece bu pencere açıkken çalışır. KAPATMAK İÇİN: Enter" Cyan
    Yaz ""
}

Yaz "3/3 İnternet linki alınıyor..."
$tunel = Tunel-Ac
try {
    if (-not $tunel.Link) {
        Yaz "Link alınamadı. İnternet bağlantını kontrol et (okul ağı ssh'ı engelliyor olabilir; telefondan hotspot dene)." Red
        Yaz "Site yine de bu bilgisayarda açık: $yerel" Yellow
        Start-Process $yerel
    }
    else {
        Link-Goster $tunel.Link
        Start-Process $tunel.Link   # linki varsayılan tarayıcıda aç
    }

    # Bekle: Enter'a basılınca çık. Tünel koparsa kendiliğinden yeniden bağlan.
    $baslangic = Get-Date
    while ($true) {
        Start-Sleep -Milliseconds 500
        if ($Sure -gt 0 -and ((Get-Date) - $baslangic).TotalSeconds -ge $Sure) { break }
        if ($Sure -le 0) {
            try { if ([Console]::KeyAvailable -and [Console]::ReadKey($true).Key -eq "Enter") { break } } catch { }
        }
        # Ücretsiz tünel adresi ara sıra değişebilir: çıktıda yeni bir link görünürse ekrana yaz
        if ($tunel.Link -and ((Get-Date).Second % 5 -eq 0) -and (Test-Path $cikti)) {
            $tumu = [regex]::Matches((Get-Content $cikti -Raw), $tunel.Desen)
            if ($tumu.Count -gt 0 -and $tumu[$tumu.Count - 1].Value -ne $tunel.Link) {
                $tunel.Link = $tumu[$tumu.Count - 1].Value
                Yaz "DİKKAT: Tünel adresi değişti, yeni linki paylaş!" Yellow
                Link-Goster $tunel.Link
            }
        }
        if ($tunel.Ssh -and $tunel.Ssh.HasExited) {
            Yaz "Bağlantı koptu, yeniden bağlanılıyor..." Yellow
            $tunel = Tunel-Ac
            if ($tunel.Link) { Yaz "DİKKAT: Link değişti, yeni linki paylaş!" Yellow; Link-Goster $tunel.Link }
            else { Start-Sleep 10 }
        }
    }
}
finally {
    if ($tunel.Ssh -and -not $tunel.Ssh.HasExited) { Stop-Process -Id $tunel.Ssh.Id -Force -ErrorAction SilentlyContinue }
    if ($site) { & taskkill /PID $site.Id /T /F 2>&1 | Out-Null }
    try { $null = [QuizArena.Uyku]::SetThreadExecutionState([uint32]2147483648) } catch { }
    Yaz "Site ve link kapatıldı."
}
