# AI Coding & Safety Guidelines for Hermes-Lite 2 Client

This document defines the strict programming rules, safety models, and style requirements for AI-assisted coding in this repository. All code generated for the C/C++ backend and C# frontend must adhere to these guidelines to ensure native stability on Windows ARM64 without memory safety crashes.

---

## 1. C++ Backend Safety (Target: Modern C++20 / C++23)

To match the safety profile of Rust while utilizing the performance of C++, code generation must adhere to the following rules:

### A. Memory & Resource Management
* **Zero Manual Allocations**: Raw `new` and `delete` operators are strictly forbidden. Use `std::make_unique` for single-owner resources and `std::make_shared` for shared resources.
* **RAII Enforcement**: All system resources (file handles, sockets, mutex locks) must be managed by RAII objects.
* **Locking Safety**: Use `std::scoped_lock` (C++17) or `std::unique_lock` instead of manual mutex locking/unlocking.

### B. Safe Containers & Arrays
* **No C-Style Arrays**: Never declare raw arrays (e.g., `float buffers[1024]`). Use `std::array<float, 1024>` or `std::vector<float>`.
* **Bounds Safety**: For non-performance-critical accesses, use `.at()` to throw exceptions on out-of-bounds access. In hot DSP loops, use range-based `for` loops or validate indices explicitly before accessing.
* **Non-owning Views**: Use `std::span` (C++20) and `std::string_view` (C++17) to pass slices of data without copying, but **always** document the lifetime of the underlying owner to ensure no dangling reference occurs.

### C. Concurrency & Threading
* **Thread Lifetimes**: Use `std::jthread` (C++20) instead of `std::thread`. `std::jthread` automatically signals cooperative cancellation (`std::stop_token`) and joins upon destruction, preventing thread leaks and crashes on shutdown.
* **Data Races**: Ensure all shared data accessed by multiple threads is protected by a mutex, read-write lock (`std::shared_mutex`), or wrapped in `std::atomic` values.

### D. Modern API Paradigms
* **Monadic Error Handling**: Prefer returning `std::optional` (C++17) or `std::expected` (C++23) for functions that can fail, rather than using error return-codes or output pointer arguments (e.g. `int* err`).
* **Auto-Vectorization**: Write clean, branch-free loops in math-heavy sections to allow the MSVC compiler to auto-vectorize loops into native ARM64 NEON instructions.

---

## 2. C# Frontend Safety (Target: .NET 8.0+)

### A. Reference Types & Safety
* **Nullable Reference Types**: Ensure `<Nullable>enable</Nullable>` is enabled in all `.csproj` files. Resolve all compiler warnings regarding potential null dereferences.
* **Pattern Matching & Guards**: Use modern C# pattern matching and `throw new ArgumentNullException(nameof(param))` guards at public API entry points.

### B. Zero-Allocation Buffering
* **Spans and Memory**: For receiving and parsing network packets (UDP) or audio buffers, use `Span<T>`, `ReadOnlySpan<T>`, and `Memory<T>` to slice buffers without creating new garbage-collected objects.
* **Avoid Unsafe Blocks**: The `unsafe` keyword is forbidden unless explicitly required for direct memory pinning during P/Invoke calls to the C++ DSP backend. Where used, the pinned scopes must be kept as small as possible.

### C. Interop (P/Invoke)
* **Direct Blitting**: Ensure that structures passed between C# and C++ are blittable (i.e. have identical memory representations in both managed and unmanaged memory) using `[StructLayout(LayoutKind.Sequential)]` to prevent layout corruption.

---

## 3. Project Configuration & Tooling Enforcement

### A. MSVC Compiler Flags (Windows ARM64)
* **High Warning Levels**: Enable `/W4` (Warning level 4) and `/WX` (Treat warnings as errors).
* **Static Analysis**: Enable `/analyze` during build pipelines to catch potential bugs before compilation.
* **AddressSanitizer**: Enable `/fsanitize=address` in Debug/Testing builds. Any out-of-bounds array access or use-after-free will crash immediately with a descriptive memory map.
