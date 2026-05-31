# **Hermes Lite 2 Hardware Notes**

This directory serves as a central knowledge base compiling key technical documentation, hardware schematics, installation guides, and community discussions for the Hermes Lite 2 (HL2) Software Defined Radio. It is structured to help software tools and engineers understand the hardware architecture and requirements, particularly for building or porting compatible software like Thetis.

## **Essential Documentation & Community Resources**

| Resource Name | Description / Purpose | Source Link   |
| :---- | :---- | :---- |
| **Hermes-Lite Project Homepage** | The official starting point for the project, hosting general overview information and portals to the documentation ecosystem. | [hermeslite.com](http://hermeslite.com/) |
| **Hermes Lite 2 & Thetis Installation Guide** | Comprehensive PDF covering the configuration steps, driver installations, and setup requirements for pairing the HL2 with Thetis and third-party applications. | [Installation PDF](https://raw.githubusercontent.com/wiki/softerhardware/Hermes-Lite2/docs/Hermes_Lite_2_Thetis_Installation_and_3rd_Party_Apps.pdf) |
| **Thetis Manuals and Links** | Resource page dedicated to Thetis software user manuals, software downloads, and hardware-specific configurations. | [Thetis Manuals](https://www.hermeslite2plus.com/p/manuals.html) |

## **Hardware Specifications & Schematics**

| Resource Name | Description / Purpose | Source Link   |
| :---- | :---- | :---- |
| **Hermes-Lite 2 Schematic (GitHub)** | The official electrical schematic diagram (PDF) of the core Hermes-Lite 2 hardware repository, detailing component connections, FPGA interfacing, and RF paths. | [HL2 Schematic PDF](https://github.com/softerhardware/Hermes-Lite2/blob/master/hardware/hl/hermeslite.pdf) |
| **Schematics and BOM Group Discussion** | Community technical thread focusing on hardware revisions, Bill of Materials (BOM) nuances, components, and assembly specifications. | [Google Groups Thread](https://groups.google.com/g/hermes-lite/c/9Hi9P-o4FAs) |
| **Getting Started Group Archive** | Core community onboarding discussions detailing initial configuration, hardware validation, and foundational integration steps. | [Google Groups Thread](https://groups.google.com/g/hermes-lite/c/OBz7BMpvCdw) |

## **Architectural Summary for Software Porting**

When provisioning an AI system or software build pipeline with this context, prioritize the following elements derived from these sources:

* **Network Protocol:** The HL2 operates over Ethernet using the openHPSDR network protocol. The software interface must manage UDP packet traffic smoothly for both I/Q data and control commands.  
* **FPGA registers:** Refer to the repository schematics to verify how control signals map from the FPGA to physical components like the frontend filters and the AD9866 frontend transceiver.  
* **Windows Dependency Stack:** The installation guide highlights specific driver requirements (such as WinPcap or Npcap) necessary for low-level network access, which will be a key focal point when recompiling or modifying dependencies for Windows on ARM.