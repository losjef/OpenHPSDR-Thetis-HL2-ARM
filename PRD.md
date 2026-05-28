# Product Requirements Document (PRD): OpenHPSDR-Thetis Windows ARM64 Port

## 1. Overview & Objectives
The purpose of this project is to port the **OpenHPSDR-Thetis** Software Defined Radio (SDR) application (specifically the Hermes-Lite 2 optimized fork) to run on **Windows ARM64** platforms (e.g., Snapdragon X Elite/Plus, Surface Pro ARM). 

### Goals:
- Build and run OpenHPSDR-Thetis on Windows ARM64.
- Maximize performance by compiling the computationally intensive DSP and driver wrapper DLLs to run natively on ARM64.
- Deliver a working build pipeline and configuration setup in Visual Studio.
- Create an installer or deployment package suitable for Windows ARM64.

---

## 2. Architecture & Components
Thetis is a hybrid application consisting of a C# WinForms frontend and several performance-critical C/C++ libraries:

1. **C# Frontend & GUI**:
   - [Thetis (Console)](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Console/Thetis.csproj): Main WinForms application, currently targeting .NET Framework 4.8.
   - [Midi2Cat](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Midi2Cat/Midi2Cat.csproj): MIDI controller interface.
   - [RawInput](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/RawInput/RawInput.csproj): Keyboard/mouse raw input handler.

2. **Native C/C++ Libraries**:
   - [wdsp](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/wdsp/wdsp.vcxproj): The core digital signal processing library written in C.
   - [ChannelMaster](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/ChannelMaster/ChannelMaster.vcxproj): Hardware control interface.
   - [cmASIO](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/cmASIO/cmASIO.vcxproj): ASIO audio interface wrapper.
   - **PortAudio**: Audio library built from source within the solution.

3. **External Precompiled Dependencies**:
   - **FFTW3** (`fftw3-3.dll`): Fast Fourier Transform library.
   - **Noise Reduction Algorithms** (`rnnoise.dll`, `specbleach.dll`): DSP speech enhancement.
   - **Managed libraries**: SkiaSharp, SharpDX (DirectX wrapper), Svg, NAudio.

---

## 3. Porting Challenges & Requirements

### A. Solution Configuration
- The Visual Studio solution [Thetis_VS2026.sln](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Thetis_VS2026.sln) currently lacks an `ARM64` platform config.
- **Requirement**: Add `ARM64` configurations to all Visual Studio projects (`wdsp`, `ChannelMaster`, `cmASIO`, `portaudio`, `Thetis`, etc.) and map them correctly in the solution configuration manager.

### B. Native Dependencies Compilation
- **PortAudio**: Must be compiled for ARM64 using the solution's PortAudio project.
- **FFTW3**: The project references precompiled x64/x86 FFTW libraries. We must obtain or compile an ARM64 version of `libfftw3-3.dll` and its import library.
- **Noise Reduction (rnnoise & specbleach)**: These are currently linked as precompiled static/import libraries (`rnnoise.lib`, `specbleach.lib`). We need to compile these for ARM64.

### C. C# Project Runtime Targets
- **Option A (Native ARM64 Execution)**: Upgrade the projects from .NET Framework 4.8 to **.NET Framework 4.8.1** (which introduces native ARM64 support). This allows the frontend to run as a native ARM64 process and load ARM64 native DLLs.
- **Option B (Hybrid/Emulation Execution)**: Run the C# frontend under x64 emulation while loading x64 native DLLs, or run the C# frontend under ARM64EC. 
- *Note*: Option A is the standard path for native Windows ARM64 applications.

### D. Installer and Packaging
- Update [Thetis-Installer.wixproj](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Thetis-Installer/Thetis-Installer.wixproj) to support packaging the ARM64 binaries and targeting ARM64 Windows installations.

---

## 4. Key Open Questions & Discussion Topics

Before proceeding with any implementation plans, the following topics must be discussed and agreed upon:

> [!IMPORTANT]
> 1. **Target Framework for C# Frontend**: Do we upgrade the C# projects to target **.NET Framework 4.8.1** to get native ARM64 execution, or keep them on .NET Framework 4.8? Upgrading to 4.8.1 requires Visual Studio 2022 (version 17.3 or later) and Windows 11 on the build/run environment.
> 2. **Sourcing FFTW3 & Noise Reduction ARM64 Binaries**: How should we source the ARM64 binaries for FFTW3, rnnoise, and specbleach? Options include compiling them from source within this workspace, using vcpkg/NuGet, or retrieving them from trusted pre-built sources.
> 3. **Build Environment Capabilities**: What version of Visual Studio and MSVC compiler toolsets are installed on the current host machine? (ARM64 compilation requires the ARM64 build tools component to be installed in Visual Studio).
> 4. **Testing Environment**: Do you have a physical Windows ARM64 device available for testing the compiled binaries, or will we rely on emulation/virtualization for runtime validation?
