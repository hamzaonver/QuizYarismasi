# QuizArena - ilk kurulum (Windows PowerShell)
# Kullanım: proje kök klasöründe ->  powershell -ExecutionPolicy Bypass -File scripts\ilk-kurulum.ps1
$ErrorActionPreference = "Stop"
Set-Location (Split-Path $PSScriptRoot -Parent)

Write-Host "1) NuGet paketleri indiriliyor..." -ForegroundColor Cyan
dotnet restore

Write-Host "2) dotnet-ef aracı kontrol ediliyor..." -ForegroundColor Cyan
if (-not (dotnet tool list --global | Select-String "dotnet-ef")) {
    dotnet tool install --global dotnet-ef --version 8.0.11
}

Write-Host "3) İlk migration oluşturuluyor (InitialCreate)..." -ForegroundColor Cyan
if (-not (Test-Path "src\QuizArena.Data\Migrations")) {
    dotnet ef migrations add InitialCreate --project src\QuizArena.Data --startup-project src\QuizArena.Web
} else {
    Write-Host "   Migrations klasörü zaten var, atlandı."
}

Write-Host "4) Migration'ın SQL betiği docs\database\migration.sql dosyasına yazılıyor..." -ForegroundColor Cyan
dotnet ef migrations script --project src\QuizArena.Data --startup-project src\QuizArena.Web -o docs\database\migration.sql

Write-Host "5) Derleniyor..." -ForegroundColor Cyan
dotnet build

Write-Host "`nHazır! Çalıştırmak için:  dotnet run --project src\QuizArena.Web" -ForegroundColor Green
