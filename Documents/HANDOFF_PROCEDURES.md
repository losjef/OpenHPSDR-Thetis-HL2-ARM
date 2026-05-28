# Windows ARM64 Port - Handoff Procedures

Follow these steps to pause work on one machine and resume it on another.

---

## 1. Before Taking a Break (Source Machine)

1. Save all open files.
2. Build the project (either in the terminal or by selecting the **`Build & Run Handoff (Release - win-arm64)`** option in VS Code's Run and Debug panel). This automatically compiles the Release binary and synchronizes the latest planning files (`task.md` and `implementation_plan.md`) from the AI's internal directory to `Documents/`:
   ```powershell
   dotnet build "Project Files/Source/Console/Thetis.csproj" -c Release -r win-arm64 --no-self-contained
   ```
3. Open the **VS Code Source Control panel**, click the Spark icon to generate an AI commit message, commit, and push your changes to GitHub.

---

## 2. Resuming Work (Target Machine)

1. Fetch and checkout the correct branch on the new machine:
   ```powershell
   git checkout <branch-name>
   git pull origin <branch-name>
   ```
2. Open the project in your editor and **start a new conversation session with the AI assistant**.
3. Run the restore script in the PowerShell terminal:
   ```powershell
   .\resume.ps1
   ```
   *This copies the project planning files from `Documents/` back into the newly initialized AI session folder.*
4. Instruct the AI assistant to read the active plan using this prompt:
   > "We are porting OpenHPSDR-Thetis to Windows ARM64. Refer to `handoff.md` and `Documents/HANDOFF_PROCEDURES.md` in the repository root for current progress and instructions. We have defined the P/Invoke corrections plan. Read `implementation_plan.md` and `task.md` and begin implementing the proposed changes."
