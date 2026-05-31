# Comparative Analysis: mi0bot/Gi8TME vs. Pre-Port and Future Architectures

This document compares the characteristics, design, and performance properties of the **mi0bot/Gi8TME** (Reid Campbell) Hermes-Lite 2 optimized fork against the upstream Apache Labs repository, the current native **.NET 8 ARM64 port**, and the planned **ground-up C# / Modern C++ Windows ARM64 client**.

---

## 1. Architectural Comparison Matrix

| Feature / Dimension | Upstream (Apache Labs) | mi0bot/Gi8TME (HL2 Fork) | Current Port (WinForms on .NET 8 ARM64) | Ground-up HL2 Client (Windows ARM64 Native) |
| :--- | :--- | :--- | :--- | :--- |
| **Target Runtime** | .NET Framework 4.8 (x86/x64) | .NET Framework 4.8 (x86/x64) | **.NET 8.0-windows** (Native ARM64) | **C# (.NET 9+) & Modern C++** (Native Windows ARM64) |
| **Execution on ARM64**| Heavy Emulation (x86/x64) | Heavy Emulation (x86/x64) | **100% Native ARM64 JIT** | **100% Native ARM64 Execution** |
| **HL2 Hardware Support**| Generic HPSDR mapping | Dedicated HL2 settings (31-step LNA, 16-stage TX, N2ADR) | **Fully integrated & native** (preserves HL2 custom features) | **Direct hardware register control** (no translation layers) |
| **Network Protocol** | Protocol 1 (max 100 Mbps) | Protocol 1 (max 100 Mbps) | **Protocol 1** (optimized network socket pipeline) | **Protocol 2** (utilizes 1 Gbps, higher bandwidth) |
| **GUI Framework** | Legacy WinForms | Legacy WinForms | **WinForms on .NET 8** (improved DPI scaling) | **Modern Windows UI** (WinUI 3 / WPF) |
| **Database System** | Shared XML Profile | Separate DB profiles | **Refactored .NET 8 profiles** (concurrency-safe) | **Independent SQLite/JSON config** (no ANAN clashes) |
| **Legacy Code Bloat** | High (50k+ line files) | High (inherited from PowerSDR) | High (with ARM64 P/Invoke bugfixes) | **Zero** (clean C# GUI + modular Modern C++ backend) |

---

## 2. Review of Previous Recommendations vs. Current State

During the initial scoping phases, two main recommendations were analyzed:

1. **Previous Recommendation (Option C): Upgrade C# frontend to target `.NET Framework 4.8.1`**
   - *Rationale*: Minimal porting risk. All legacy COM controls, WinForms skins, and third-party packages would run natively on ARM64 out-of-the-box without code rewrites.
   - *Alternative Chosen (Option A)*: We instead bypassed 4.8.1 and migrated straight to **.NET 8.0-windows**.
   - *Evaluation*: Migrating to .NET 8 was highly successful for long-term project viability, but it introduced several runtime incompatibilities that needed to be patched (and were not present in mi0bot's legacy .NET 4.8 codebase):
     - **WinForms Control Strictness**: `.NET 8` throws strict `ArgumentOutOfRangeException` exceptions when `NumericUpDown` bounds are modified out of order. This caused hangs when selecting the "Hermes Lite" radio model in `setup.cs` and `console.cs`, which we fixed with a custom `SafeSetBounds` method.
     - **Thread Abortions**: Modern .NET throws a `PlatformNotSupportedException` when calling `Thread.Abort()`. The original `mi0bot/Gi8TME` code used thread abortion for cleanup, which required try-catch guards.
     - **Serialization Changes**: `BinaryFormatter` is deprecated and disabled by default in `.NET 8`. Custom serialization layouts (e.g. `ConcurrentDictionary` in `MeterManager.cs`) had to be converted to standard dictionaries during profile saves.

2. **Integration of mi0bot/Gi8TME Features**
   - We preserved all of Reid Campbell's custom HL2 hardware integrations, such as the **31-Step Attenuation Loop** and **16-Stage TX Drive Control**.
   - However, our P/Invoke audit revealed that some original signatures (e.g. MIDI handlers in `Midi2Cat.IO` and window positioning handlers in `win32.cs`) truncated 64-bit pointers when running under a native ARM64 process. We successfully upgraded these parameters from `int` to `IntPtr` and corrected the pointer arithmetic.

---

## 3. Findings & Recommendations for a Future Ground-up Client

If a dedicated SDR client was written strictly for the Hermes-Lite 2 targeting native Windows ARM64 (unshackled from legacy code and cross-platform overhead):

1. **Protocol 2 Adoption**
   - The HL2 contains a gigabit Ethernet interface. While `mi0bot/Gi8TME` is bound to Protocol 1 (limiting sample rates to 384 kHz), a new client could leverage Protocol 2. This allows multiple receivers to have independent clock rates and opens the door for much higher bandwidth spectral captures.
2. **Native Windows ARM64 Optimization & Low-Latency Audio**
   - Free from cross-platform constraints, the client can be tailored specifically to Windows APIs. We can write a high-performance C++ backend that interfaces directly with **Windows WASAPI** (Windows Audio Session API) to achieve ultra-low latency audio processing (vital for fast CW/Morse keying).
3. **Modern C# / C++ Hybrid Architecture**
   - Build a clean C# frontend (using WinUI 3 or WPF) to deliver a modern, high-DPI Windows desktop layout, and back it with a native, thread-safe C++20/C++23 DLL using the MSVC compiler with full `/analyze` static checking and AddressSanitizer support.
4. **Database Simplicity**
   - The configuration files could drop the complex, multi-receiver configuration mappings required for ANAN's 7000/8000 series radios, using a simple local configuration file (JSON or SQLite) instead.
