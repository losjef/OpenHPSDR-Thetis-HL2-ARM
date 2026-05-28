# Powershell script to restore the planning state from git Documents/ to the active Antigravity brain.
# Run this on the new machine after checking out the branch and starting the Antigravity session.

$repoRoot = Get-Location
$appDataBrain = "$HOME\.gemini\antigravity\brain"

if (-not (Test-Path $appDataBrain)) {
    Write-Warning "Antigravity AppData brain folder not found at $appDataBrain. Make sure the AI agent is running or has started a session."
    exit 1
}

# Find the most recently modified conversation directory in AppData (which represents the active session on this machine)
$latestBrain = Get-ChildItem -Path $appDataBrain -Directory | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if ($null -eq $latestBrain) {
    Write-Warning "No active conversation brain directory found. Please start a session first."
    exit 1
}

Write-Host "Restoring planning artifacts to session: $($latestBrain.Name)"

$docFolder = Join-Path $repoRoot "Documents"
$filesToSync = @("task.md", "implementation_plan.md")

foreach ($file in $filesToSync) {
    $src = Join-Path $docFolder $file
    $dest = Join-Path $latestBrain.FullName $file
    if (Test-Path $src) {
        Copy-Item -Path $src -Destination $dest -Force
        Write-Host "Restored $file to AppData brain."
    } else {
        Write-Warning "Source file $src not found in Git repository."
    }
}

Write-Host "Planning artifacts successfully restored! Ask the AI assistant to read the active implementation_plan.md and task.md to continue."
