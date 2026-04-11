using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;

namespace IngameScript
{
    public partial class Program
    {
        public class PowerControlModule
        {
            Action<string> Echo { get; set; }
            public PowerControlModule(Action<string> echo)
            {
                Echo = echo;
            }
            
            public class PowerGroup
            {
                public string Name;
                public bool Enabled;
                public List<IMyFunctionalBlock> Blocks;
            }


            public PowerGroup[] PowerGroups { get; private set; } = new PowerGroup[] {

                new PowerGroup{Name = "FLIGHT", Enabled = true, Blocks = new List<IMyFunctionalBlock>()},
                new PowerGroup{Name = "LIFE-SUPPORT", Enabled = true, Blocks = new List<IMyFunctionalBlock>()},
                new PowerGroup{Name = "WEAPONS", Enabled = true, Blocks = new List<IMyFunctionalBlock>()},
                new PowerGroup{Name = "EGRESS", Enabled = true, Blocks = new List<IMyFunctionalBlock>()},
                new PowerGroup{Name = "PRODUCTION", Enabled = true, Blocks = new List<IMyFunctionalBlock>()},
                new PowerGroup{Name = "COMFORT", Enabled = true, Blocks = new List<IMyFunctionalBlock>()},
                new PowerGroup{Name = "DECORATIVE", Enabled = true, Blocks = new List<IMyFunctionalBlock>()},
                new PowerGroup{Name = "MISC", Enabled = true, Blocks = new List<IMyFunctionalBlock>()},
                new PowerGroup{Name = "LOW", Enabled = false, Blocks = new List<IMyFunctionalBlock>()}
            };

            public List<PowerGroup> PowerGroupsSaveState { get; set; } = new List<PowerGroup>();

            public void RefreshGroupMembership(List<IMyFunctionalBlock> blocks)
            {
                // clear existing lists
                foreach (var group in PowerGroups)
                {
                    group.Blocks.Clear();
                }
                
                // loop through all blocks, read custom data, assign block to each power group found in the [POWER] section of the ini result
                foreach (var block in blocks)
                {
                    foreach (var group in PowerGroups)
                    {
                        if (IsPartOfPowerGroup(block, group.Name))
                        {
                            group.Blocks.Add(block);
                        }
                    }
                }
            }

            public bool TryGetPowerGroup(string name, out PowerGroup powerGroup)
            {
                foreach (var item in PowerGroups)
                {
                    if (string.Equals(name, item.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        powerGroup = item;
                        return true;
                    }
                }
                powerGroup = default(PowerGroup);
                return false;
            }

            public void TogglePowerGroup(PowerGroup powerGroup)
            {
                bool newState = !powerGroup.Enabled;
                if (powerGroup.Name == "LOW") newState = powerGroup.Enabled; // low power mode exception (it's backwards)
                switch (newState)
                {
                    case true: EnablePowerGroup(powerGroup); return;
                    case false: DisablePowerGroup(powerGroup); return;
                }
            }

            public void EnablePowerGroup(PowerGroup powerGroup)
            {
                if (powerGroup.Name == "LOW") { GoToLowPowerMode(); return; } // treating "enabling" low power mode as GTLPM
                foreach (var block in powerGroup.Blocks)
                {
                    block.Enabled = true; // turn the block on 
                }
                Echo($"current power group: {powerGroup.Name}");
                Echo($"current power group state: {powerGroup.Enabled}");
                powerGroup.Enabled = true; // change the state of the PowerClass in memory
                Echo($"power group state after enabling: {powerGroup.Enabled}");
            }

            public void DisablePowerGroup(PowerGroup powerGroup)
            {
                if (powerGroup.Name == "LOW") { RestoreFromSaveState(); return; } // treating "disabling" low power mode as RFSS
                foreach (var block in powerGroup.Blocks)
                {
                    block.Enabled = false; // turn the block off
                }
                Echo($"current power group: {powerGroup.Name}");
                Echo($"current power group state: {powerGroup.Enabled}");
                powerGroup.Enabled = false; // change the state of the PowerClass in memory
                Echo($"power group state after disabling: {powerGroup.Enabled}");
            }

            public void GoToLowPowerMode()
            {
                PowerGroupsSaveState.Clear();
                foreach (var powerGroup in PowerGroups)
                {
                    if (powerGroup.Name == "LOW") continue; // low power mode is backwards compared to all the other power modes
                    PowerGroupsSaveState.Add(new PowerGroup
                    {
                        Name = powerGroup.Name,
                        Enabled = powerGroup.Enabled,
                        Blocks = powerGroup.Blocks
                    });

                    DisablePowerGroup(powerGroup);
                }
                PowerGroups[PowerGroups.Count()].Enabled = true; // and thus want to toggle low power mode 'on' when we go to low power mode
                Echo($"save state after GTLPM: {PowerGroupsSaveState}");
            }

            public void RestoreFromSaveState()
            {
                Echo($"save state before RFSS: {PowerGroupsSaveState}");
                if (PowerGroupsSaveState.Count == 0) return;
                foreach (var powerGroup in PowerGroupsSaveState)
                {
                    if (powerGroup.Name == "LOW") continue; // low power mode is backwards compared to all the other power modes
                    if (powerGroup.Enabled) EnablePowerGroup(powerGroup);
                }
                PowerGroupsSaveState.Clear();
                PowerGroups[PowerGroups.Count()].Enabled = false; // and thus want to toggle low power mode 'off' when we restore from save state
            }

            bool IsPartOfPowerGroup(IMyFunctionalBlock block, string group)
            {
                MyIni ini = new MyIni();

                MyIniParseResult result;
                if (!ini.TryParse(block.CustomData, out result))
                {
                    // what is the behavior when TryParse fails? throws an exception? returns null? returns false?
                    return false;
                }

                return ini.Get("POWER", $"{group}").ToBoolean();
            }

        }
    }
}
