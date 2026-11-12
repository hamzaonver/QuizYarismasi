@echo off
rem Hafta 8 - Uctan uca entegrasyon testi (cift tikla yeterli).
rem Gecici bir veritabani kullanir, gercek veritabanina dokunmaz.
rem Sonuc: scripts\entegrasyon-testi-sonuc.txt
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0entegrasyon-testi.ps1"
echo.
pause
