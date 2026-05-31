# Project Collaboration & Communication Rules

This document outlines the strict collaboration rules and guidelines that **Antigravity** (the AI Coding Assistant) must adhere to throughout this project.

## 1. No Assumptions Policy
- **Zero Assumptions**: Antigravity must never make assumptions regarding any design choices, architectural paths, library dependencies, compilers, tools, or target environments.
- **Clarification**: If any requirement or instruction is underspecified, Antigravity must stop and request clarification from the User before proceeding.
- **Explicit Listings**: When proposing any plan, all underlying assumptions must be clearly listed in a dedicated section so the User can review, verify, and approve/reject them.

## 2. Plan First, Execute Later
- **Strict Planning Requirement**: For all non-trivial changes (including project configuration changes, file creations, dependency updates, and compilation setup), an implementation plan (`implementation_plan.md`) must be written/updated first.
- **Explicit Approval**: No modifications to existing source files or project configuration files may be made until the User has explicitly reviewed and approved the implementation plan in the chat.
- **Scope Limitations**: Plans should be granular and broken down into logical phases. If a phase encounters unexpected complexity, execution must pause, and the plan must be revised and re-approved.

## 3. Communication Style & Transparency
- **Conciseness**: Keep chat responses focused, precise, and directly actionable, without unnecessary fluff.
- **Ongoing Findings**: Provide deep technical details of ongoing compiler findings, DLL linkages, and design decisions in every response.
- **Decision Logging**: Maintain and update all major design/porting decisions in [DECISIONS.md](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Documents/DECISIONS.md) to keep a persistent log for future contributors.
- **Risk Identification**: Highlight any potential regressions (e.g., build breakages, performance issues, compatibility) immediately.
- **File Links**: Always use clickable standard Markdown links for files and code elements using the `file://` scheme with forward slashes (e.g., `[Thetis_VS2026.sln](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Thetis_VS2026.sln)`).

## 4. Code & Document Integrity
- **Preserve Documentation**: Do not remove, modify, or truncate existing code comments, docstrings, or license headers unless explicitly requested by the User.
- **Testing & Verification**: Every implementation plan must include a clear verification plan detailing how the changes will be tested (both automated and manual steps).

## 5. Hermes Lite 2 Reference Sources
- **Official References**: All hardware integration, protocol changes, register manipulation, and DSP changes for the Hermes Lite 2 (HL2) model must refer to and be consistent with:
  - [Hermes Lite 2 Hardware Notes](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Documentation/HL2/Hermes%20Lite%202%20Hardware%20Notes.md)
  - The official [Hermes-Lite2 GitHub Repository](https://github.com/softerhardware/Hermes-Lite2)
- **Gateware & Protocol Compliance**: Any protocol-level changes must verify compatibility with HL2 gateware and FPGA register constraints as documented in these sources.

