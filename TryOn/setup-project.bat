@echo off
echo ========================================
echo    CONFIGURACION INICIAL TRYON
echo ========================================

echo.
echo 1. Creando estructura de directorios...
if not exist "tryon-frontend" mkdir tryon-frontend
if not exist "TryOn.API\wwwroot" mkdir TryOn.API\wwwroot

echo.
echo 2. Navegando al directorio del frontend...
cd tryon-frontend

echo.
echo 3. Inicializando proyecto npm...
if not exist "package.json" (
    npm init -y
)

echo.
echo 4. Instalando dependencias...
npm install vite @snap/camera-kit fs-extra --save-dev

echo.
echo 5. Construyendo frontend...
npm run build

echo.
echo 6. Copiando al backend...
npm run build-and-copy

echo.
echo 7. Navegando al backend...
cd ../TryOn.API

echo.
echo 8. Restaurando paquetes NuGet...
dotnet restore

echo.
echo ========================================
echo    CONFIGURACION COMPLETADA
echo ========================================
echo.
echo PASOS SIGUIENTES:
echo.
echo 1. Ejecutar el backend:
echo    cd TryOn.API
echo    dotnet run
echo.
echo 2. Abrir navegador en: https://localhost:7295
echo.
echo 3. Para desarrollo del frontend:
echo    cd tryon-frontend
echo    npm run dev
echo.
pause
