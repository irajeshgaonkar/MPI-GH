$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

$repoRoot = Split-Path -Parent $PSScriptRoot
$distRoot = Join-Path $repoRoot 'dist\hca-poc'
$zipPath = Join-Path $repoRoot 'dist\hca-poc.zip'

if (-not (Test-Path $distRoot)) {
    throw "Build output not found: $distRoot"
}

if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

$files = Get-ChildItem -Path $distRoot -Recurse -File -Force

if (-not $files) {
    throw "No files found to package in $distRoot"
}

$zipFileStream = [System.IO.File]::Open($zipPath, [System.IO.FileMode]::CreateNew)

try {
    $archive = New-Object System.IO.Compression.ZipArchive($zipFileStream, [System.IO.Compression.ZipArchiveMode]::Create, $false)

    try {
        foreach ($file in $files) {
            $relativePath = $file.FullName.Substring($distRoot.Length).TrimStart('\').Replace('\', '/')
            $entry = $archive.CreateEntry($relativePath, [System.IO.Compression.CompressionLevel]::Optimal)

            $entryStream = $entry.Open()
            $sourceStream = [System.IO.File]::OpenRead($file.FullName)

            try {
                $sourceStream.CopyTo($entryStream)
            }
            finally {
                $sourceStream.Dispose()
                $entryStream.Dispose()
            }
        }
    }
    finally {
        $archive.Dispose()
    }
}
finally {
    $zipFileStream.Dispose()
}

Write-Host "Created package: $zipPath"