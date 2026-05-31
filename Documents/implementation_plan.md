# Implementation Plan: Fix Hermes Lite Model Selection Hang and Thread.Abort Exceptions

This plan addresses the application hang/crash when selecting "Hermes Lite" from the hardware setup model dropdown. It also handles unsafe `Thread.Abort()` calls under .NET 8.

## Proposed Changes

### Component 1: Internals Visibility
#### [MODIFY] [AssemblyInfo.cs](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Console/AssemblyInfo.cs)
- Add `[assembly: InternalsVisibleTo("Thetis.Tests")]` to allow unit testing of internal classes like `cmaster` and `HardwareSpecific`.

### Component 2: Tune Power and Attenuator Range Constraints
#### [MODIFY] [setup.cs](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Console/setup.cs)
- In `comboRadioModel_SelectedIndexChanged`, ensure that:
  - If selecting `HERMESLITE`, the limits of `udTXTunePower` are safely set to `[-16.5, 0]` (setting `Maximum = 0` first, then `Minimum = -16.5` to prevent out-of-range exceptions).
  - If selecting any other model, reset `udTXTunePower` limits back to `[0, 100]` (setting `Maximum = 100` first, then `Minimum = 0`). Also reset `udATTOnTX.Minimum` back to `0`.
- In `FixedTunePower` property setter, clamp the assigned value to the control's current `Minimum` and `Maximum` bounds before assigning it to `udTXTunePower.Value`. This guarantees `ArgumentOutOfRangeException` will never be thrown by the WinForms control.

### Component 3: Thread.Abort() Safety under .NET 8
#### [MODIFY] [console.cs](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Console/console.cs)
- Wrap all occurrences of `Thread.Abort()` in try-catch blocks to catch and ignore `PlatformNotSupportedException` (which is always thrown under .NET 8). This prevents crash/hang issues during thread shutdowns when a thread takes longer than 500ms to join.
#### [MODIFY] [PSForm.cs](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Console/PSForm.cs)
- Wrap `ampvThread.Abort()` in a try-catch block for `PlatformNotSupportedException`.
#### [MODIFY] [TCPIPcatServer.cs](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Console/CAT/TCPIPcatServer.cs)
- Wrap thread abort calls in try-catch blocks.
#### [MODIFY] [TCIServer.cs](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Console/TCIServer.cs)
- Wrap thread abort calls in try-catch blocks.

### Component 4: Verification Suite
#### [MODIFY] [WdspTests.cs](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Thetis.Tests/WdspTests.cs)
- Add a test verifying `cmaster.CMLoadRouterAll` can be called safely for `HPSDRModel.HERMESLITE` and other models.
- Add a test simulating model changes and checking that no exception is thrown when assigning `FixedTunePower` values outside the Hermes Lite limits.

---

## Verification Plan

### Automated Tests
- Build and run unit tests targeting native ARM64:
  ```powershell
  dotnet test "Project Files/Source/Thetis.Tests/Thetis.Tests.csproj" -c Debug -r win-arm64
  ```

### Manual Verification
- Launch the application and select "Hermes Lite" from `Setup -> H/W Select -> Radio Model`.
- Confirm that the dropdown updates instantly without freezing, hanging, or throwing unhandled exceptions.
- Select another model (e.g. "Hermes") and verify the tune power limits return to normal (`[0, 100]`).
