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
- [ ] Add ARM64 Build Configurations to Visual Studio Solution
  - [ ] Add `ARM64` platform configuration to [Thetis_VS2026.sln](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Thetis_VS2026.sln)
  - [ ] Add `ARM64` target configuration to native C++ projects:
    - [ ] `wdsp.vcxproj`
    - [ ] `ChannelMaster.vcxproj`
    - [ ] `cmASIO.vcxproj`
    - [ ] `portaudio.vcxproj`
- [ ] Configure native projects to link against ARM64 dependencies
  - [ ] Update `wdsp.vcxproj` library paths to use `Project Files/lib/fftw_arm64` and `Project Files/lib/NR_Algorithms_arm64` when building under the ARM64 configuration
- [ ] Migrate C# Projects to target modern .NET 8.0-windows
  - [ ] Backup legacy project files (`.csproj`)
  - [ ] Migrate [Thetis.csproj](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Console/Thetis.csproj) to SDK-style targeting `net8.0-windows`
  - [ ] Migrate [Midi2Cat.csproj](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Midi2Cat/Midi2Cat.csproj) to SDK-style targeting `net8.0-windows`
  - [ ] Migrate [RawInput.csproj](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/RawInput/RawInput.csproj) to SDK-style targeting `net8.0-windows`
  - [ ] Map modern project build outputs in Configuration Manager for ARM64

## Phase 3: Compilation & Native Code Execution
- [ ] Compile PortAudio for ARM64
- [ ] Compile `ChannelMaster.dll` and `cmASIO.dll` for ARM64
- [ ] Compile `wdsp.dll` for ARM64
- [ ] Compile C# projects (Thetis, Midi2Cat, RawInput) for ARM64
- [ ] Investigate and resolve runtime DLL loading (P/Invoke) issues
  - [ ] Audit P/Invoke signatures in [portaudio.cs](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Console/portaudio.cs) and other source files to ensure compatibility with native 64-bit ARM DLLs under .NET 8

## Phase 4: Unit Testing & Verification
- [ ] Write Unit/Integration Tests
  - [ ] Create a C++ test runner project to verify `wdsp.dll` ARM64 math functions (FFT, filter coefficients) produce correct outputs
  - [ ] Create a C# unit test project to verify basic console startup, database handling, and parameter settings under native ARM64 execution
- [ ] Verify full application execution
  - [ ] Run runtime checks on target ARM64 device or emulator
  - [ ] Test audio capture/playback via PortAudio ARM64
  - [ ] Test spectral display rendering

## Phase 5: Packaging & Installer Updates
- [ ] Upgrade [Thetis-Installer.wixproj](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Thetis-Installer/Thetis-Installer.wixproj) to build an ARM64 package
- [ ] Package and verify the final installer on Windows ARM64
