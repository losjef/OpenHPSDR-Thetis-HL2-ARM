# Windows ARM64 Port - Handoff Procedures

Follow these steps to pause work on one machine and resume it on another.

---

## 1. Before Taking a Break (Source Machine)

1. Save all open files in your editor.
2. Open a PowerShell terminal at the repository root.
3. Run the checkpoint script:
   ```powershell
   .\checkpoint.ps1
   ```
   *This script will copy the current planning files (`task.md` and `implementation_plan.md`) from the AI's internal directory to `Documents/`, stage all changes, commit them with a timestamp, and push them to the current git branch.*

---

## 2. Resuming Work (Target Machine)

1. Fetch and check out the correct branch on the new machine:
   ```powershell
   git pull origin feature/arm64-port
   git checkout feature/arm64-port
   ```
2. Open the project in your editor and **start a new conversation session with the AI assistant**.
3. Run the restore script in the PowerShell terminal:
   ```powershell
   .\resume.ps1
   ```
   *This copies the project planning files from `Documents/` back into the newly initialized AI session folder.*
4. Instruct the AI assistant to read the active plan using this prompt:
   > "We are porting OpenHPSDR-Thetis to Windows ARM64. Refer to `handoff.md` and `Documents/HANDOFF_PROCEDURES.md` in the repository root for current progress and instructions. We have defined the P/Invoke corrections plan. Read `implementation_plan.md` and `task.md` and begin implementing the proposed changes."
