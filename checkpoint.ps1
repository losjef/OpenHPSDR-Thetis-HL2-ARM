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

# Dynamically fetch current branch name to push to remote
$currentBranch = (git branch --show-current).Trim()
Write-Host "Current active branch: $currentBranch"

Write-Host "Pushing to remote origin..."
git push -u origin $currentBranch

Write-Host "`n" -NoNewline
Write-Host "==========================================================" -ForegroundColor Green
Write-Host "Checkpoint complete! You can safely switch machines." -ForegroundColor Green
Write-Host "To resume on the new machine, run these commands:" -ForegroundColor Green
Write-Host "  git checkout $currentBranch" -ForegroundColor Yellow
Write-Host "  git pull origin $currentBranch" -ForegroundColor Yellow
Write-Host "  .\resume.ps1" -ForegroundColor Yellow
Write-Host "==========================================================" -ForegroundColor Green
