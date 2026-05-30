# Implementation Plan: Windows ARM64 Port - Phase 4: Unit Testing & Verification

This plan addresses Phase 4: Unit Testing & Verification. We will create a unit test project to verify both C++ math DLL functions (`wdsp.dll`, `ChannelMaster.dll`) and C# database/startup logic.

---

## User Review Required

> [!NOTE]
> **Cross-Architecture Verification Strategy**
> - The host build machine is an `AMD64` (x64) Windows machine, meaning native `ARM64` binaries cannot be executed directly on it.
> - To verify the math logic and code compatibility, we will design the unit tests to support both `x64` and `ARM64` configurations.
> - We will run the tests locally targeting `x64` to verify that the logic is correct.
> - The same test suite will be ready to execute on native `ARM64` hardware or a WoA environment.

---

## Proposed Changes

### Component 1: Unit Test Project Creation

#### [NEW] [Thetis.Tests.csproj](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Thetis.Tests/Thetis.Tests.csproj)
Create a new C# MSTest unit test project targeting `net8.0-windows7.0` (matching the main projects).
- Add project references to:
  - `Thetis.csproj`
  - `Midi2Cat.csproj`
- Add build targets to copy native DLLs (`wdsp.dll`, `ChannelMaster.dll`, `fftw3` DLLs, etc.) from `Project Files/lib/` and the native project output paths to the test runner directory so P/Invokes work seamlessly at test time.

#### [MODIFY] [Thetis_VS2026.sln](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Thetis_VS2026.sln)
Integrate the new `Thetis.Tests` project into the solution configurations for all platforms (`x64` and `ARM64`) and configurations (`Debug` and `Release`).

---

### Component 2: Unit Test Implementations

#### [NEW] [WdspTests.cs](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Thetis.Tests/WdspTests.cs)
Create tests for native `wdsp.dll` and `ChannelMaster.dll` via P/Invoke.
- Test `GetWDSPVersion()` to ensure the DLL loads and returns the expected version (`129`).
- Test `GetCMVersion()` to ensure `ChannelMaster.dll` loads and returns the version correctly.
- Test basic DSP/filter function invocation if feasible.

#### [NEW] [DatabaseTests.cs](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Thetis.Tests/DatabaseTests.cs)
Create database serialization tests:
- Initialize the database class `Thetis.DB`.
- Test XML load/save behavior to verify settings storage is fully functional on .NET 8.

---

## Verification Plan

### Automated Tests
- Build the entire solution targeting `Release` and `x64`.
- Execute tests via:
  ```powershell
  dotnet test "Project Files/Source/Thetis.Tests/Thetis.Tests.csproj" -c Release -r win-x64 --no-build
  ```
  Verify all tests pass on the host machine.
