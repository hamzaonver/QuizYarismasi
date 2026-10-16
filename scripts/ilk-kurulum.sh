#!/usr/bin/env bash
# QuizArena - ilk kurulum (macOS / Linux)
# Kullanım: proje kök klasöründe ->  bash scripts/ilk-kurulum.sh
set -e
cd "$(dirname "$0")/.."

echo "1) NuGet paketleri indiriliyor..."
dotnet restore

echo "2) dotnet-ef aracı kontrol ediliyor..."
if ! dotnet tool list --global | grep -q dotnet-ef; then
  dotnet tool install --global dotnet-ef --version 8.0.11
  export PATH="$PATH:$HOME/.dotnet/tools"
fi

echo "3) İlk migration oluşturuluyor (InitialCreate)..."
if [ ! -d "src/QuizArena.Data/Migrations" ]; then
  dotnet ef migrations add InitialCreate --project src/QuizArena.Data --startup-project src/QuizArena.Web
else
  echo "   Migrations klasörü zaten var, atlandı."
fi

echo "4) Migration'ın SQL betiği docs/database/migration.sql dosyasına yazılıyor..."
dotnet ef migrations script --project src/QuizArena.Data --startup-project src/QuizArena.Web -o docs/database/migration.sql

echo "5) Derleniyor..."
dotnet build

echo
echo "Hazır! Çalıştırmak için:  dotnet run --project src/QuizArena.Web"
