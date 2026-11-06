@echo off
rem Hafta 7 testini calistirir (cift tikla yeterli). Sonuc: scripts\hafta7-test-sonuc.txt
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0hafta7-test.ps1"
echo.
pause
