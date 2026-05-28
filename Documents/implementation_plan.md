# Implementation Plan: Windows ARM64 Port - Phase 3: P/Invoke Signature Audit & Corrections

This plan addresses P/Invoke signature corrections across the C# projects to ensure 64-bit compatibility under Windows ARM64.

---

## User Review Required

Please review the proposed P/Invoke adjustments:

> [!IMPORTANT]
> **Handle Type Corrections (`IntPtr` vs `int`)**
> - Window handles (`HWND`) and MIDI device/stream handles must be represented as `IntPtr` (64-bit on ARM64) rather than `int` (32-bit).
> - Casting these handles using `.ToInt32()` will lead to `OverflowException` or memory truncation, resulting in silent failures or `AccessViolationException` crashes at runtime.
> - We will update the signatures in [win32.cs](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Console/win32.cs) and [WinMM.cs](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Midi2Cat/Midi2Cat.IO/WinMM.cs), along with their corresponding call sites.

---

## Proposed Changes

### Component 1: Console / Frontend P/Invokes

#### [MODIFY] [win32.cs](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Console/win32.cs)
Change `SetWindowPos` signature to accept `IntPtr` for `hwnd` and `hWndInsertAfter`:
```csharp
[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
public static extern int SetWindowPos(IntPtr hwnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);
```

#### [MODIFY] [console.cs](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Console/console.cs)
Modify calls to `Win32.SetWindowPos` to pass `Handle` directly (as `IntPtr`) and cast constant window ordering targets:
- Line 13369:
  ```csharp
  Win32.SetWindowPos(this.Handle, (IntPtr)(-1), this.Left, this.Top, this.Width, this.Height, 0);
  ```
- Line 13374:
  ```csharp
  Win32.SetWindowPos(this.Handle, (IntPtr)(-2), this.Left, this.Top, this.Width, this.Height, 0);
  ```

#### [MODIFY] [frmNotchPopup.cs](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Console/frmNotchPopup.cs)
Modify calls to `Win32.SetWindowPos`:
- Line 85:
  ```csharp
  Win32.SetWindowPos(this.Handle, (IntPtr)(-1), this.Left, this.Top, this.Width, this.Height, 0);
  ```
- Line 90:
  ```csharp
  Win32.SetWindowPos(this.Handle, (IntPtr)(-2), this.Left, this.Top, this.Width, this.Height, 0);
  ```

---

### Component 2: Midi2Cat Library P/Invokes

#### [MODIFY] [WinMM.cs](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Midi2Cat/Midi2Cat.IO/WinMM.cs)
- Change `MidiInUnprepareHeader` signature to use `IntPtr hMidiIn`.
- Change `MidiInCallback` delegate to represent pointer-sized fields (`dwInstance`, `dwParam1`, `dwParam2`) as `IntPtr`:
  ```csharp
  unsafe public delegate int MidiInCallback(IntPtr hMidiIn, int wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2);
  ```
- Change `midiOutLongMsg`, `midiOutPrepareHeader`, and `midiOutUnprepareHeader` signatures to use `IntPtr handle`.

#### [MODIFY] [MidiDevice.cs](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Midi2Cat/Midi2Cat.IO/MidiDevice.cs)
- Update signature of `InCallback` method:
  ```csharp
  private int InCallback(IntPtr hMidiIn, int wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2)
  ```
- Retrieve parameters by calling `.ToInt64()` on `dwParam1`:
  ```csharp
  long val1 = dwParam1.ToInt64();
  Command cmd = (Command)((byte)val1);
  byte controlId = (byte)(val1 >> 8);
  byte data = (byte)(val1 >> 16);
  byte status = (byte)(val1 & 0xFF);
  ```
- Update `SendLongMessage` and static `SendMsg` signature to take `IntPtr handle`.
- Update `SendMsg` call inside `SendLongMessage`:
  ```csharp
  result = WinMM.MidiOutPrepareHeader(handle, ptr, size);
  if (result == 0) result = WinMM.MidiOutLongMessage(handle, ptr, size);
  if (result == 0) result = WinMM.MidiOutUnprepareHeader(handle, ptr, size);
  ```

---

## Verification Plan

### Automated Tests
- **Compilation Check**: Run `dotnet build` on the solution `Thetis_VS2026.sln` targeting `Release` and `ARM64`. Verify that all C# compiler errors are resolved.

### Manual Verification
- **PE Check**: Verify that DLLs are generated correctly in `Project Files/bin/ARM64/Release/`.
