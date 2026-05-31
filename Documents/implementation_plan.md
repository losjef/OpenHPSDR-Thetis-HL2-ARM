# Implementation Plan: Fix Hermes Lite Model Selection Hang & Step Attenuator Range Crash

This plan addresses the crash/hang occurring when selecting the "Hermes Lite" radio model. The issue is caused by C# Windows Forms `NumericUpDown` controls throwing an unhandled `ArgumentOutOfRangeException` when their bounds (`Minimum` / `Maximum`) are modified while their current `.Value` property lies outside the target bounds. This happens during model transitions for step attenuator controls (`udRX1StepAttData`, `udRX2StepAttData`, `udTXStepAttData`) on the main console form, and the band-gain controls (`nud160M` to `nud10M`) on the setup form.

Additionally, we fix a typo in the `validateTXStepAttData` validation logic.

## Proposed Changes

### Component 1: Console / Main Form Attenuator Controls
#### [MODIFY] [console.cs](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Console/console.cs)
1. Add a `SafeSetBounds` helper method to `Console` that safely updates the bounds of a `NumericUpDown` (or `NumericUpDownTS`) control without throwing exceptions:
   ```csharp
   private void SafeSetBounds(System.Windows.Forms.NumericUpDown ud, decimal min, decimal max, int decimalPlaces = 0, decimal increment = 1)
   {
       if (ud == null) return;
       decimal val = ud.Value;
       if (val < min) val = min;
       if (val > max) val = max;
       if (min < ud.Minimum) ud.Minimum = min;
       if (max > ud.Maximum) ud.Maximum = max;
       ud.Value = val;
       ud.Minimum = min;
       ud.Maximum = max;
       ud.DecimalPlaces = decimalPlaces;
       ud.Increment = increment;
   }
   ```
2. In `SetupForHPSDRModel()`, safely initialize the limits of `udRX1StepAttData`, `udRX2StepAttData`, and `udTXStepAttData` using `SafeSetBounds`.
   - For `HERMESLITE`: set range to `[-28, 31]` with 0 decimal places and 1 increment.
   - For other models: reset range to `[0, 61]` (if `alexpresent` and conditions met) or `[0, 31]` for RX, and `[0, 31]` for TX.
3. In `RX1AttenuatorData` and `RX2AttenuatorData` property setters, replace direct assignments to `.Maximum` and `.Minimum` on step attenuator controls with `SafeSetBounds`.
4. In `validateTXStepAttData(int att)`, fix the typo:
   - Change: `if (att < udRX1StepAttData.Minimum) att = (int)udTXStepAttData.Minimum;`
   - To: `if (att < udTXStepAttData.Minimum) att = (int)udTXStepAttData.Minimum;`

### Component 2: Setup Form Band Gain Controls
#### [MODIFY] [setup.cs](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Console/setup.cs)
1. Add a null check to `SafeSetBounds` in `setup.cs`.
2. In `comboRadioModel_SelectedIndexChanged`:
   - At the beginning, reset the range of `nud160M` through `nud10M` to the default `[38.8, 100]` (with 1 decimal place and 0.1 increment) using `SafeSetBounds`.
   - In case `HPSDRModel.HERMESLITE:`, change the range of `nud160M` through `nud10M` to `[0, 100]` (with 0 decimal places and 1 increment) using `SafeSetBounds`. This replaces the direct, unsafe property assignments in lines 20408 to 20439.

### Component 3: Unit Tests
#### [MODIFY] [WdspTests.cs](file:///c:/Users/jeffl/Source/repos/github/OpenHPSDR-Thetis-HL2-ARM/Project%20Files/Source/Thetis.Tests/WdspTests.cs)
1. Add a unit test `TestSetupRadioModelTransition` that instantiates a `Setup` form, mocks basic fields, and simulates transitions between `HPSDRModel.HERMES` and `HPSDRModel.HERMESLITE` back and forth.
2. Verify that all setup controls and attenuator controls update their ranges correctly and no `ArgumentOutOfRangeException` or other range exception is thrown during transitions.

---

## Verification Plan

### Automated Tests
- Run the MSTest suite to verify compilation and execution:
  ```powershell
  dotnet test "Project Files/Source/Thetis.Tests/Thetis.Tests.csproj" -c Debug -r win-arm64
  ```

### Manual Verification
- Open the application and navigate to `Setup -> H/W Select -> Radio Model`.
- Switch the Radio Model from "Hermes" (or other models) to "Hermes Lite", back to "Hermes", and then back to "Hermes Lite" again.
- Verify that the transitions complete instantly and without any crash or UI freeze.
