@echo off
echo Stopping CarRentalManagementSystem processes...
taskkill /f /im CarRentalManagementSystem.exe
taskkill /f /im dotnet.exe
echo Application stopped.
pause 