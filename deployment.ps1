# deploy.ps1
$ErrorActionPreference = 'Stop'

# Settings
$ServiceName = "SonicInflator"
$TargetDir = "C:\Services\SonicInflator"
$PublishDir = "$PSScriptRoot\publish"

Write-Host "📁 Ensuring target directory exists..."
if (!(Test-Path $TargetDir)) {
    New-Item -ItemType Directory -Path $TargetDir -Force | Out-Null
}

Write-Host "📤 Copying published files to target directory..."
Copy-Item "$PublishDir\*" -Destination $TargetDir -Recurse -Force

# Service registration
if (Get-Service -Name $ServiceName -ErrorAction SilentlyContinue) {
    Write-Host "🛑 Stopping existing service..."
    sc.exe stop $ServiceName | Out-Null

    Write-Host "🗑️ Deleting existing service..."
    sc.exe delete $ServiceName | Out-Null
}

Write-Host "🛠️ Creating new Windows service..."
sc.exe create $ServiceName binPath= "$TargetDir\SonicInflatorService.exe" start= auto

Write-Host "🚀 Starting service..."
sc.exe start $ServiceName

Write-Host "✅ Deployment complete."
