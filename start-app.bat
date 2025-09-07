@echo off
echo ========================================
echo Car Rental Management System - Startup
echo ========================================

echo Stopping any existing processes...
taskkill /f /im CarRentalManagementSystem.exe >nul 2>&1
taskkill /f /im dotnet.exe >nul 2>&1

echo.
echo Cleaning project...
dotnet clean

echo.
echo Building project...
dotnet build

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Build failed! Please check the errors above.
    pause
    exit /b 1
)

echo.
echo Starting application...
echo Application will be available at: http://localhost:5162
echo.
echo Press Ctrl+C to stop the application
echo.

dotnet run

pause 