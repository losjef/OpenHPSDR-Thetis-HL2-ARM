# Project Handoff: Windows ARM64 Port for OpenHPSDR-Thetis (HL2)

Use this document to resume the porting work on any machine.

---

## 1. Current Progress Summary

### Native Dependencies (Phase 1) - 100% Complete
- **FFTW3**: Double/float precision libraries compiled and placed in `Project Files/lib/fftw_arm64/`.
- **Noise Reduction**: `rnnoise.dll` and `specbleach.dll` compiled for ARM64 using CMake/MSVC and placed in `Project Files/lib/NR_Algorithms_arm64/`.
- **PE Verification**: All native DLLs verified as native `ARM64` (`0xAA64` machine architecture).

### Project Upgrade (Phase 2) - 100% Complete
- **C# Migration**: Converted C# projects (`Thetis`, `Midi2Cat`, `RawInput`) to SDK-style targeting `.net8.0-windows` (WinForms).
- **Configurations**: Added native `ARM64` configuration to `Thetis_VS2026.sln` duplicating `x64` settings.
- **Orphaned Files**: Excluded orphaned files (`rxaControls.cs`, `cwkeyer.cs`, `CAT\JustinIO.cs`) in `Thetis.csproj` to fix compilation.
- **ARM64 Intrinsics**: Patched Intel SSE-specific code (`_MM_SET_FLUSH_ZERO_MODE`) in `wdsp/channel.c` using ARM64 FPCR registers.
- **Result**: The entire solution builds under MSVC/MSBuild ARM64 configuration with **0 errors**.

### P/Invoke Audit (Phase 3) - 75% Complete
- Audited P/Invoke declarations in `portaudio.cs`, `dsp.cs`, `specHPSDR.cs`, and `RawInput`. They are verified 64-bit safe.
- Identified 32-bit truncation bugs in window and MIDI handles:
  - `win32.cs`: `SetWindowPos` uses `int` instead of `IntPtr` for `hwnd`/`hWndInsertAfter`.
  - `console.cs` and `frmNotchPopup.cs`: Call sites cast window handles using `Handle.ToInt32()`.
  - `Midi2Cat.IO/WinMM.cs`: `MidiInUnprepareHeader` and `MidiInCallback` delegate use `int` for handles.
  - `MidiDevice.cs`: Signature mismatch for long message / callback handles.
- **Approved Plan**: The implementation plan is defined to fix these signatures and call sites.

---

## 2. Dynamic Planning Synchronization
We have automated the copy of your AI planning state (`task.md` and `implementation_plan.md`) directly into `Thetis.csproj` post-build. 
- Building the project automatically synchronizes the active planning files to the repository `Documents/` folder.
- There is **no need** to run custom checkpoint scripts. Just build/compile and commit your repository changes using standard Git or VS Code Source Control (with AI-generated commit messages).

---

## 3. How to Resume Work

### Step A: Push Current Workspace State to Git
1. Build the project (either in the terminal or by selecting the **`Build & Run Handoff (Release - win-arm64)`** option in VS Code's Run and Debug panel) to make sure the latest planning files are copied:
   ```powershell
   dotnet build "Project Files/Source/Console/Thetis.csproj" -c Release -r win-arm64 --no-self-contained
   ```
2. Open the **VS Code Source Control panel**, click the Spark icon next to the commit input to **generate an AI commit message**, and commit.
3. Push the active branch to remote `origin`.

### Step B: Pull on the New Machine
1. Clone/pull the branch:
   ```powershell
   git clone <repo-url>
   git checkout <active-branch>
   ```

### Step C: Direct the AI Assistant to Resume
1. Open the project in your editor and start the new AI assistant session.
2. In the powershell terminal, run:
   ```powershell
   .\resume.ps1
   ```
   *This copies the planning files from `Documents/` back into the newly initialized session.*
3. Start the session with:
   > "We are porting OpenHPSDR-Thetis to Windows ARM64. Refer to `handoff.md` and `Documents/HANDOFF_PROCEDURES.md` in the repository root for current progress and instructions. We have defined the P/Invoke corrections plan. Read `implementation_plan.md` and `task.md` and begin implementing the proposed changes."
