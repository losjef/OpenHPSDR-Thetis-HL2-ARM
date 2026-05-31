# OpenHPSDR-Thetis Windows ARM64 Port - Project Task List

This document tracks all tasks, milestones, and progress for the porting effort.

## Current Progress Status
- `[ ]` Not Started
- `[/]` In Progress
- `[x]` Completed

---

## Phase 1: Environment Audit & Native Dependencies
- [x] Establish environment capability audit
  - [x] Confirm MSVC ARM64 toolset functionality on host
  - [x] Confirm .NET SDK versions on host
- [x] Compile FFTW3 library for Windows ARM64
  - [x] Run `vcpkg install fftw3[double-precision,float-precision,long-double-precision]:arm64-windows`
  - [x] Copy built `fftw3` DLLs and LIBs to `Project Files/lib/fftw_arm64`
- [x] Compile Noise Reduction libraries for Windows ARM64
  - [x] Configure `rnnoise` with CMake and compile ARM64 release binaries
  - [x] Configure `libspecbleach` with CMake and compile ARM64 release binaries
  - [x] Copy built noise reduction DLLs and LIBs to `Project Files/lib/NR_Algorithms_arm64`
- [x] Verify ARM64 compilation of FFTW3 and noise reduction binaries
  - [x] Run PE header check script on all compiled binaries to ensure machine type is `ARM64` (value `0xAA64`)

## Phase 2: Solution Configurations & Project Upgrades
- [x] Add ARM64 Build Configurations to Visual Studio Solution
  - [x] Add `ARM64` platform configuration to [Thetis_VS2026.sln](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Thetis_VS2026.sln)
  - [x] Add `ARM64` target configuration to native C++ projects:
    - [x] `wdsp.vcxproj`
    - [x] `ChannelMaster.vcxproj`
    - [x] `cmASIO.vcxproj`
    - [x] `portaudio.vcxproj`
- [x] Configure native projects to link against ARM64 dependencies
  - [x] Update `wdsp.vcxproj` library paths to use `Project Files/lib/fftw_arm64` and `Project Files/lib/NR_Algorithms_arm64` when building under the ARM64 configuration
- [x] Migrate C# Projects to target modern .NET 8.0-windows
  - [x] Backup legacy project files (`.csproj`)
  - [x] Migrate [Thetis.csproj](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Console/Thetis.csproj) to SDK-style targeting `net8.0-windows`
  - [x] Migrate [Midi2Cat.csproj](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Midi2Cat/Midi2Cat.csproj) to SDK-style targeting `net8.0-windows`
  - [x] Migrate [RawInput.csproj](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/RawInput/RawInput.csproj) to SDK-style targeting `net8.0-windows`
  - [x] Map modern project build outputs in Configuration Manager for ARM64

## Phase 3: Compilation & Native Code Execution
- [x] Compile PortAudio for ARM64
- [x] Compile `ChannelMaster.dll` and `cmASIO.dll` for ARM64
- [x] Compile `wdsp.dll` for ARM64
- [x] Compile C# projects (Thetis, Midi2Cat, RawInput) for ARM64
- [x] Investigate and resolve runtime DLL loading (P/Invoke) issues
  - [x] Audit P/Invoke signatures in [portaudio.cs](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Console/portaudio.cs) and other source files to ensure compatibility with native 64-bit ARM DLLs under .NET 8
  - [x] Fix VS Code Debug run failures by implementing a smart fallback to copy Release DLLs if Debug native DLLs are not built


## Phase 4: Unit Testing & Verification
- [x] Write Unit/Integration Tests
  - [x] Create a C++ verification helper to verify `wdsp.dll` and `ChannelMaster.dll` ARM64 math functions produce correct outputs (via C# P/Invoke Unit Tests)
  - [x] Create a C# unit test project to verify basic console startup, database handling, and parameter settings under native ARM64 execution
- [x] Verify full application execution
  - [x] Run runtime checks on target ARM64 device (unit tests executed natively on ARM64 host)
  - [ ] Test audio capture/playback via PortAudio ARM64 (manual)
  - [ ] Test spectral display rendering (manual)

## Phase 5: Packaging & Installer Updates
- [x] Upgrade [Thetis-Installer.wixproj](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Thetis-Installer/Thetis-Installer.wixproj) to build an ARM64 package
- [ ] Package and verify the final installer on Windows ARM64

## Phase 6: Run-time Asset Handling (Skins)
- [x] Set up automated skins extraction and copying in [Thetis.csproj](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Console/Thetis.csproj)
- [x] Verify that skins are extracted to AppData and copied to output directory during build
- [x] Verify the application launches without the missing skins warning

## Phase 7: Hermes Lite Model Selection Hang & .NET 8 Thread Safety
- [x] Add InternalsVisibleTo to AssemblyInfo.cs
- [x] Implement safe NumericUpDown range limits in setup.cs (SelectedIndexChanged)
- [x] Implement safe, exception-free value assignment in FixedTunePower setter
- [x] Wrap Thread.Abort() calls in try-catch blocks to handle PlatformNotSupportedException under .NET 8
- [x] Add unit/integration tests to verify model selection safety and CM router loading
- [x] Build solution and run test suite to verify success




