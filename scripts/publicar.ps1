$ErrorActionPreference = "Stop"

$raiz = Split-Path -Parent $PSScriptRoot
$saida = "$raiz\publish\app"

Write-Host "1/4 Compilando o frontend..."
Push-Location "$raiz\frontend"
try {
    npm ci
    npm run build
}
finally {
    Pop-Location
}

Write-Host "2/4 Copiando o frontend para o backend..."
$wwwroot = "$raiz\backend\src\Contratos.Api\wwwroot"
if (Test-Path $wwwroot) { Remove-Item $wwwroot -Recurse -Force }
Copy-Item "$raiz\frontend\dist" $wwwroot -Recurse

if (Test-Path $saida) { Remove-Item $saida -Recurse -Force }

Write-Host "3/4 Publicando a janela do sistema..."
dotnet publish "$raiz\desktop-host\ContratoAutomatizado.Desktop\ContratoAutomatizado.Desktop.csproj" `
    -c Release -r win-x64 --self-contained true -o $saida

Write-Host "4/4 Publicando o servidor..."
dotnet publish "$raiz\backend\src\Contratos.Api\Contratos.Api.csproj" `
    -c Release -r win-x64 --self-contained true -o "$saida\servidor"

Write-Host "Pronto. Abra: $saida\ContratoAutomatizado.exe"