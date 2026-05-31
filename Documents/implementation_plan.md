# Implementation Plan: Soft Dedicate OpenHPSDR-Thetis to Hermes-Lite 2

This plan outlines the changes to soft-dedicate the Thetis application to the Hermes-Lite 2 (HL2) SDR. Instead of risky physical removal of legacy radio configuration files, we will restrict the configuration paths, dropdown options, and default fallback models specifically to the HL2.

---

## User Review Required

Please review the proposed architectural locks:

> [!IMPORTANT]
> **Dropdown Selection Restrictions**
> - The radio model select dropdown (`comboRadioModel`) will have all other models (ANAN series, Red Pitaya, etc.) removed, leaving `"HERMES LITE"` as the sole option.
> - Any legacy database profile containing a different radio model will automatically trigger a fallback reset to `"HERMES LITE"` instead of the upstream default `"HERMES"`.

---

## Proposed Changes

### Component 1: Setup GUI & Form Behavior

#### [MODIFY] [setup.designer.cs](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Console/setup.designer.cs)
Remove all entries in the `comboRadioModel` item array except for `"HERMES LITE"`:
```diff
             this.comboRadioModel.Items.AddRange(new object[] {
-            "HERMES",
-            "HERMES LITE",
-            "ANAN-10",
-            "ANAN-10E",
-            "ANAN-100",
-            "ANAN-100B",
-            "ANAN-100D",
-            "ANAN-200D",
-            "ANAN-7000DLE",
-            "ANAN-8000DLE",
-            "ANAN-G1",
-            "ANAN-G2",
-            "ANAN-G2-1K",
-            "ANVELINA-PRO3",
-            "RED-PITAYA"});
+            "HERMES LITE"});
```

#### [MODIFY] [setup.cs](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Console/setup.cs)
1. In `AfterConstructor()`, change default fallback text from `"HERMES"` to `"HERMES LITE"`:
   ```diff
-  if (comboRadioModel.Text == "") comboRadioModel.Text = "HERMES";
+  if (comboRadioModel.Text == "") comboRadioModel.Text = "HERMES LITE";
   ```
2. In `getOptions()`, change the database validation fallback from `HPSDRModel.HERMES` to `HPSDRModel.HERMESLITE`:
   ```diff
-  a["comboRadioModel"] = HPSDRModel.HERMES.ToString();
+  a["comboRadioModel"] = HPSDRModel.HERMESLITE.ToString();
   ```
3. In `getModelFromDB()`, default to `HPSDRModel.HERMESLITE` if not found in the database:
   ```diff
-  else
-      return HPSDRModel.FIRST;
+  else
+      return HPSDRModel.HERMESLITE;
   ```

---

### Component 2: Model & Hardware Mappings

#### [MODIFY] [clsHardwareSpecific.cs](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Console/clsHardwareSpecific.cs)
1. Change default case in `StringModelToEnum`:
   ```diff
   default:
-      return HPSDRModel.HERMES;
+      return HPSDRModel.HERMESLITE;
   ```
2. Change default case in `EnumModelToString`:
   ```diff
   default:
-      return "HERMES";
+      return "HERMES-LITE";
   ```

---

### Component 3: Database Manager Defaults

#### [MODIFY] [clsDBMan.cs](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Console/clsDBMan.cs)
1. Update `JsonConverter` default attribute for the database model:
   ```diff
-  [JsonConverter(typeof(DatabaseInfoDefaultStringEnumConverter), HPSDRModel.HERMES)]
+  [JsonConverter(typeof(DatabaseInfoDefaultStringEnumConverter), HPSDRModel.HERMESLITE)]
   ```
2. Update default property values in `DatabaseInfo()` constructor:
   ```diff
-  Model = HPSDRModel.HERMES;
+  Model = HPSDRModel.HERMESLITE;
   ```
3. Update fallback checks in `DBWritten()` (line 541), `LoadDB` (line 1458, 1468), and backup imports (line 1808, 1831) to default to `HPSDRModel.HERMESLITE`.

---

## Verification Plan

### Automated Tests
- Run unit/integration tests to ensure no model-selection or database loading regressions exist:
  ```powershell
  dotnet test "Project Files/Source/Thetis.Tests/Thetis.Tests.csproj" -c Release -r win-arm64
  ```

### Manual Verification
- Launch Thetis on the ARM64 device or build environment.
- Verify that "Radio Model" in Setup defaults automatically to `"HERMES LITE"`.
- Click the dropdown to verify that no other radio models are displayed or selectable.
