# Powershell script to checkpoint the current project state and push to Git.
# Run this script before taking a break or changing machines.

$repoRoot = Get-Location
$appDataBrain = "$HOME\.gemini\antigravity\brain"

if (-not (Test-Path $appDataBrain)) {
    Write-Warning "Antigravity AppData brain folder not found at $appDataBrain"
    exit 1
}

# Find the most recently modified conversation directory in AppData
$latestBrain = Get-ChildItem -Path $appDataBrain -Directory | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if ($null -eq $latestBrain) {
    Write-Warning "No active conversation brain directory found."
    exit 1
}

Write-Host "Found active conversation brain: $($latestBrain.Name)"

# Copy artifacts to the Git repository Documents/ folder
$docFolder = Join-Path $repoRoot "Documents"
if (-not (Test-Path $docFolder)) {
    New-Item -ItemType Directory -Path $docFolder | Out-Null
}

$filesToSync = @("task.md", "implementation_plan.md")
foreach ($file in $filesToSync) {
    $src = Join-Path $latestBrain.FullName $file
    $dest = Join-Path $docFolder $file
    if (Test-Path $src) {
        Copy-Item -Path $src -Destination $dest -Force
        Write-Host "Copied $file to Documents/"
    } else {
        Write-Warning "Source file $file not found in AppData."
    }
}

# Git operations
Write-Host "Staging files..."
git add .

$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$commitMsg = "checkpoint: ARM64 port state as of $timestamp"

Write-Host "Committing changes..."
git commit -m $commitMsg

Write-Host "Current branch:"
git branch --show-current

Write-Host "Pushing to remote..."
git push

Write-Host "Checkpoint complete! You can safely switch machines or take a break."
