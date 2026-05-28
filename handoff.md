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

## 2. Uncommitted Files on this Machine
Run `git status` to see:
- Modified C++ projects and C# `.csproj` files.
- Untracked directories:
  - `.vscode/` (Debug launcher configuration)
  - `Project Files/lib/fftw_arm64/` (Native FFTW3 ARM64 libs)
  - `Project Files/lib/NR_Algorithms_arm64/` (Native NR ARM64 libs)

---

## 3. How to Resume Work

### Step A: Push Current Workspace State to Git
1. Create and checkout a new branch (e.g., `feature/arm64-port`):
   ```powershell
   git checkout -b feature/arm64-port
   ```
2. Stage and commit all changes, including untracked libraries:
   ```powershell
   git add .
   git commit -m "checkpoint: arm64 compilation successful, ready for pinvoke corrections"
   ```
3. Push to your repository:
   ```powershell
   git push origin feature/arm64-port
   ```

### Step B: Pull on the New Machine
1. Clone/pull the branch:
   ```powershell
   git clone <repo-url>
   git checkout feature/arm64-port
   ```

### Step C: Direct the AI Assistant to Resume
Paste this instruction to start the next session:
> "We are porting OpenHPSDR-Thetis to Windows ARM64. Refer to `handoff.md` in the repository root for the current progress. We have defined the P/Invoke corrections plan in our previous session. Let's start by implementing the proposed changes in `handoff.md` section 1 under Phase 3 (updating `win32.cs`, `console.cs`, `frmNotchPopup.cs`, `WinMM.cs`, and `MidiDevice.cs` signatures)."
