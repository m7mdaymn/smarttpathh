@echo off
REM Digital Pass Backend Startup Script
REM This script will build, migrate, and run the backend

echo.
echo ╔════════════════════════════════════════════════════╗
echo ║   Digital Pass Backend - Setup & Run Script       ║
echo ╚════════════════════════════════════════════════════╝
echo.

REM Set directory
cd /d "c:\DATA\projects(freelance)\DigitalPass\DigitalpassFrontt\backend"

echo [1/5] Installing dependencies...
call dotnet restore

if %errorlevel% neq 0 (
    echo ❌ Restore failed
    exit /b 1
)

echo [2/5] Building project...
call dotnet build

if %errorlevel% neq 0 (
    echo ❌ Build failed
    exit /b 1
)

echo [3/5] Applying database migrations...
call dotnet ef database update

if %errorlevel% neq 0 (
    echo ⚠️  Migration warning - Database may already exist
)

echo [4/5] Starting backend server...
echo.
echo ╔════════════════════════════════════════════════════════════════╗
echo ║                                                                ║
echo ║  ✅ Backend is ready!                                         ║
echo ║                                                                ║
echo ║  📍 API URL:      http://localhost:5000/api                   ║
echo ║  📚 Swagger UI:   http://localhost:5000/swagger               ║
echo ║  📖 OpenAPI:      http://localhost:5000/openapi               ║
echo ║                                                                ║
echo ║  Press Ctrl+C to stop the server                              ║
echo ║                                                                ║
echo ╚════════════════════════════════════════════════════════════════╝
echo.

call dotnet run

if %errorlevel% neq 0 (
    echo ❌ Application failed to run
    exit /b 1
)

pause
