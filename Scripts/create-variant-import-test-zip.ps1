# Genera un ZIP de prueba para POST /api/catalog/variant-images/import-zip
# Uso: .\Scripts\create-variant-import-test-zip.ps1 -Skus "MI-SKU-1","MI-SKU-2"
param(
    [Parameter(Mandatory = $true, HelpMessage = "SKUs exactos de variantes en tu negocio (como en Inventario).")]
    [string[]]$Skus,

    [string]$OutputZip = ""
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing

$root = Split-Path -Parent $PSScriptRoot
$outDir = Join-Path $root "Scripts\test-data\variant-image-import\_staging"
if (Test-Path $outDir) {
    Remove-Item $outDir -Recurse -Force
}
New-Item -ItemType Directory -Path $outDir | Out-Null

function New-TestProductJpeg {
    param(
        [string]$Sku,
        [int]$Slot,
        [string]$Path
    )

    $size = 900
    $bmp = New-Object System.Drawing.Bitmap $size, $size
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.Color]::FromArgb(255, 255, 255, 255))

    $productColors = @(
        [System.Drawing.Color]::FromArgb(255, 220, 80, 60),
        [System.Drawing.Color]::FromArgb(255, 70, 130, 180),
        [System.Drawing.Color]::FromArgb(255, 60, 160, 90)
    )
    $brushProduct = New-Object System.Drawing.SolidBrush $productColors[($Slot - 1) % 3]
    $g.FillEllipse($brushProduct, 180, 180, 540, 540)

    $fontSku = New-Object System.Drawing.Font("Segoe UI", 22, [System.Drawing.FontStyle]::Bold)
    $fontSlot = New-Object System.Drawing.Font("Segoe UI", 18, [System.Drawing.FontStyle]::Regular)
    $g.DrawString($Sku, $fontSku, [System.Drawing.Brushes]::Black, 24, 24)
    $g.DrawString("Foto $Slot (prueba import ZIP)", $fontSlot, [System.Drawing.Brushes]::DimGray, 24, 820)

    $bmp.Save($Path, [System.Drawing.Imaging.ImageFormat]::Jpeg)
    $g.Dispose()
    $bmp.Dispose()
    $brushProduct.Dispose()
    $fontSku.Dispose()
    $fontSlot.Dispose()
}

$filesCreated = 0
foreach ($sku in $Skus) {
    $clean = $sku.Trim()
    if ([string]::IsNullOrWhiteSpace($clean)) {
        continue
    }
    for ($slot = 1; $slot -le 2; $slot++) {
        $fileName = "{0}_{1}.jpg" -f $clean, $slot
        $filePath = Join-Path $outDir $fileName
        New-TestProductJpeg -Sku $clean -Slot $slot -Path $filePath
        $filesCreated++
        Write-Host "  + $fileName"
    }
}

if ($filesCreated -eq 0) {
    throw "No se creó ningún archivo. Pasá al menos un SKU con -Skus."
}

if ([string]::IsNullOrWhiteSpace($OutputZip)) {
    $skuLabel = ($Skus | ForEach-Object { $_.Trim() } | Where-Object { $_ } | Select-Object -First 1)
    $OutputZip = Join-Path $root "Scripts\test-data\variant-image-import\variant-images-test-$skuLabel.zip"
}

$zipPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputZip)
$zipDir = Split-Path $zipPath -Parent
if (-not (Test-Path $zipDir)) {
    New-Item -ItemType Directory -Path $zipDir | Out-Null
}
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Compress-Archive -Path (Join-Path $outDir "*") -DestinationPath $zipPath -CompressionLevel Optimal
Remove-Item $outDir -Recurse -Force

Write-Host ""
Write-Host "ZIP listo: $zipPath"
Write-Host "Archivos: $filesCreated"
Write-Host ""
Write-Host "Prueba en el FE: Inventario -> Importar fotos ZIP"
