# Architecture Decisions & Explanations

This document tracks technical decisions, explanations of core technologies, and architectural changes for the Windows ARM64 port of OpenHPSDR-Thetis.

---

## 1. Fast Fourier Transform Library (FFTW3)

### What is FFTW3?
**FFTW (Fastest Fourier Transform in the West)** is a highly optimized C library for computing discrete Fourier transforms (DFTs). In the context of SDR (Software Defined Radio) applications like Thetis, FFT is the mathematical operation that converts raw radio samples in the time domain (I/Q data received from hardware like the Hermes-Lite 2) into the frequency domain. 

This conversion is required for:
1. **Panadapter / Waterfall Display**: Rendering the visual representation of radio signals across a frequency span.
2. **Filtering & Demodulation**: Many DSP filters (like bandpass filters, noise blankers, and spectral subtraction noise reduction) are implemented in the frequency domain because it is computationally faster and mathematically cleaner to multiply frequencies rather than convolving long time-domain filters.

### Decision: Native ARM64 Compilation via vcpkg
- **Problem**: The original repository comes with precompiled x64 and x86 binaries of FFTW3. To run natively on ARM64, we need native `libfftw3-3.dll` (double precision), `libfftw3f-3.dll` (single precision float), and `libfftw3l-3.dll` (long double precision) binaries.
- **Solution**: We will compile FFTW3 using the host's `vcpkg` package manager with the `arm64-windows` triplet. This ensures the library is built with optimization flags matching the host compiler (`cl.exe`) for ARM64.
- **Status**: Pending execution of compilation step.

---

## 2. Target Framework Upgrade (.NET Framework 4.8.1)

### Why is this upgrade required?
Thetis's GUI is a C# WinForms application targeting `.NET Framework 4.8`.
- While Windows on ARM can run `.NET Framework 4.8` applications using x86/x64 emulation, running under emulation forces the process to load x86 or x64 native DLLs.
- Since we want native ARM64 performance for the math-heavy DSP backend (`wdsp.dll`), the C# application must run as a native ARM64 process.
- **.NET Framework 4.8.1** is the first version of .NET Framework to introduce **native ARM64 support** for Windows Forms and WPF applications.
- By upgrading our target framework to `v4.8.1` and compiling the C# projects for `ARM64` (or `AnyCPU` running on an ARM64 system), the CLR will execute the application natively, allowing it to load our native ARM64 DSP DLLs directly.

### Decision: Upgrade C# targets to 4.8.1
- **Status**: Approved by User. Target framework version tags in C# project files will be updated in the next phase.

---

## 3. Noise Reduction (rnnoise & specbleach)

### What are these libraries?
- **rnnoise**: A recurrent neural network-based noise suppression library developed by the Xiph.Org Foundation. It combines classic DSP spectral subtraction with a deep learning recurrent neural network (GRU) to filter out background noise from speech.
- **specbleach**: A spectral subtraction denoiser designed specifically for cleanup of background noise in voice communications.

### Decision: Compile from source using CMake
- The project repository includes the full source code for both libraries in `Project Files/lib/NR_Algorithms_x64/src`.
- We will configure CMake to generate Visual Studio project files targeting ARM64 and build them using MSVC's ARM64 compilers.
- **Status**: Pending execution.

---

## 4. Fortran Dependencies Audit

### Current Status
- An initial recursive search of the workspace for Fortran files (`*.f`, `*.f90`, `*.for`, etc.) indicates that there are **no active Fortran source files** compiled by the projects in this solution.
- The only Fortran-related files are interface/binding headers (`fftw3.f`, `fftw3.f03`, etc.) distributed as part of the third-party FFTW3 library. The active projects (`wdsp`, `ChannelMaster`, `cmASIO`, `portaudio`, `Thetis` C# frontend) are written entirely in C, C++, and C#.

### Decision: Risk Mitigation and Ongoing Research
- If any legacy Fortran routines are discovered during deep compilation audits of `wdsp` or other modules, we will:
  1. Identify their mathematical purpose.
  2. Research compilation options (e.g., MinGW/GFortran cross-compilers or Intel oneAPI Fortran).
  3. Discuss and draft a porting plan to translate them to C/C++ or replace them with modern equivalent C/C++ libraries before making any changes.

---

## 5. Device Connectivity & USB Drivers

### Context
- The target hardware for this port is the **Hermes-Lite 2 (HL2)** SDR transceiver.
- The HL2 connects to the host PC strictly via **Ethernet (UDP/IP)** rather than USB.

### Decision: Bypassing USB Driver Porting
- Since the HL2 does not use USB for connection, any USB-specific driver DLLs (such as `FTDI2XX.dll` or similar FTDI drivers) are **not required** to be ported natively to ARM64.
- If the application references FTDI packages (e.g. NuGet packages in the C# project), we can leave them as AnyCPU/x86/x64 assemblies. As long as the runtime execution path for HL2 does not call or initialize the native FTDI drivers, the application will run fine without native ARM64 FTDI DLLs.
- This reduces porting complexity and risk, allowing us to focus entirely on the core DSP libraries (`wdsp`) and the network sockets/audio interfaces.

---

## 6. Hermes-Lite 2 Exclusive Client Optimization Analysis

### Context
During Phase 8 (HL2 Capabilities & Review), we analyzed the architecture of Thetis in comparison to a dedicated SDR client built solely and purely for the Hermes-Lite 2 (HL2).

### Decision: Documenting Community Fork & Ground-Up Optimization Opportunities
While the Windows ARM64 port successfully executes the C# WinForms frontend and native DSP/network libraries, future development efforts or alternative client projects can target the following architectural optimization vectors:

1. **Existing mi0bot Fork (Reid Campbell) Optimizations** (Reference: [mi0bot/Gi8TME](https://github.com/mi0bot/Gi8TME)):
   - **31-Step Attenuation Loop**: Repurpose control data bits to supply 31 physical steps over the HL2's internal LNA gain.
   - **16-Stage TX Drive Control**: Rescale software transmit sliders to map smoothly onto the 16 hardware power amplifier stages of the HL2.
   - **N2ADR Integration**: Map specific hardware/UI control buttons to automatically configure companion filter boards (e.g. N2ADR companion board) on band change.
   - **PureSignal Corrections**: Correct/adapt the feedback calibration algorithm to handle HL2 feedback loop responses.

2. **Ground-Up Exclusive HL2 SDR Client Opportunities**:
   - **Stripping Legacy Code & UI Bloat**: Thetis is based on a legacy PowerSDR codebase with files up to 50k lines of code. Rebuilding a dedicated client enables a modern, lightweight, responsive UI with native cross-platform support (Linux, macOS, Windows) without emulation/Wine.
   - **Unleashing the Network Protocol**: Bypass legacy HPSDR Protocol 1 (100 Mbps, 384 kHz sample rate limit) and leverage Protocol 2 alongside custom HL2 gateware. This utilizes the full physical 1 Gbps interface for independent receiver clocking and higher bandwidths.
   - **Database Independence**: Avoid parallel profile configuration directories and XML mapping workarounds by separating the client configuration completely from legacy ANAN/Apache Labs databases.

---

## 7. Programming Language Decision for Ground-Up Client (Rust vs. C/C++)

### Context
A ground-up Hermes-Lite 2 client requires a robust architectural foundation handling real-time UDP packet processing, multi-threaded DSP pipelines, low-latency audio rendering, and a high-performance visual display.

> [!IMPORTANT]
> **Target Platform Constraints**
> This project is designed strictly and exclusively for native Windows ARM64 execution. There are no cross-platform requirements (no Linux or macOS support is needed), nor are there legacy x86/x64 compatibility requirements. The entire backend and frontend architecture is optimized for a single platform: Windows ARM64.

### Reference Document
- For a detailed, point-by-point technical comparison, see [rust_vs_cpp_comparison.md](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Documents/rust_vs_cpp_comparison.md).

### Summary Analysis
- **Rust** provides a compile-time guarantee of memory safety and thread safety ("Fearless Concurrency"), preventing common SDR bugs like data races, pointer corruption, and heap memory leaks without GC pauses. It is highly suited for the network and DSP pipelines.
- **C/C++** remains the industry standard for traditional DSP toolkits (Liquid-SDR, VOLK) and has qualitative tooling for native GUI layouts (Qt6) and seamless integration with Windows APIs.
- **Architectural Approaches**:
  1. *Pure Rust Backend + Frontend (egui/Slint)*: Excellent for safety, dependency management (`cargo`), and direct compilation to Windows ARM64.
  2. *Hybrid Approach (Rust DSP Core + C++ Qt GUI)*: Offers the best of both worlds by wrapping Rust's thread-safe network and DSP logic in a DLL with a C ABI, consumed by a native, feature-rich C++ Qt user interface targeting Windows ARM64.

### Final Decision: C / C++ / C# with Compile-Time and AI Guardrails
- **Selection**: We will stay with the C/C++/C# technology stack for the ground-up Windows ARM64 client.
  - **Frontend**: C# (utilizing modern .NET 8.0/10.0 and Windows-native UI frameworks like WPF or WinUI 3) to provide a rich, polished desktop GUI.
  - **Backend**: Modern C++ (C++20/C++23) for low-latency network I/O, audio streaming (WASAPI), and DSP pipelines.
- **Safety Enforcement**:
  - Memory and concurrency safety will be enforced via strict compiler flags (`/W4`, `/WX`, `/analyze`), AddressSanitizer (`/fsanitize=address`), C++ Core Guidelines checkers in MSVC, and modern C# safety features (nullable types, Roslyn analyzers, `Span<T>`).
  - An AI coding guidelines rulebook has been established in the repository at [AI_RULES.md](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Documents/AI_RULES.md) to ensure any assistant-driven code conforms to this strict modern safety model.






