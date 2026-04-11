using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
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
            
            public struct PowerGroup
            {
                public string Name;
                public bool Enabled;
                public List<IMyFunctionalBlock> Blocks;
            }


            public PowerGroup[] PowerGroups { get; private set; } = new PowerGroup[] {

                new PowerGroup{Name = "FLIGHT", Enabled = true, Blocks = new List<IMyFunctionalBlock>()},
                new PowerGroup{Name = "LIFE SUPPORT", Enabled = true, Blocks = new List<IMyFunctionalBlock>()},
                new PowerGroup{Name = "WEAPONS", Enabled = true, Blocks = new List<IMyFunctionalBlock>()},
                new PowerGroup{Name = "EGRESS", Enabled = true, Blocks = new List<IMyFunctionalBlock>()},
                new PowerGroup{Name = "PRODUCTION", Enabled = true, Blocks = new List<IMyFunctionalBlock>()},
                new PowerGroup{Name = "COMFORT", Enabled = true, Blocks = new List<IMyFunctionalBlock>()},
                new PowerGroup{Name = "DECORATIVE", Enabled = true, Blocks = new List<IMyFunctionalBlock>()},
                new PowerGroup{Name = "MISC", Enabled = true, Blocks = new List<IMyFunctionalBlock>()},
                new PowerGroup{Name = "LOW", Enabled = true, Blocks = new List<IMyFunctionalBlock>()}
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
                    if (string.Equals(name, item.Name, System.StringComparison.OrdinalIgnoreCase))
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
                switch (newState)
                {
                    case true: EnablePowerGroup(powerGroup); return;
                    case false: DisablePowerGroup(powerGroup); return;
                }
            }

            public void EnablePowerGroup(PowerGroup powerGroup)
            {
                foreach (var block in powerGroup.Blocks)
                {
                    block.Enabled = true; // turn the block on 
                }
                powerGroup.Enabled = true; // change the state of the PowerClass in memory
            }

            public void DisablePowerGroup(PowerGroup powerGroup)
            {
                if (powerGroup.Name == "LOW") return; // disallow blocks designated Low Power Mode to be turned off in software.
                foreach (var block in powerGroup.Blocks)
                {
                    block.Enabled = false; // turn the block off
                } 
                powerGroup.Enabled = false; // change the state of the PowerClass in memory
            }

            public void GoToLowPowerMode()
            {
                PowerGroupsSaveState.Clear();
                foreach (var powerGroup in PowerGroups)
                {
                    PowerGroupsSaveState.Add(powerGroup);
                    DisablePowerGroup(powerGroup);
                }
            }

            public void RestoreFromSaveState()
            {
                foreach (var powerGroup in PowerGroupsSaveState)
                {
                    if (powerGroup.Enabled)
                        EnablePowerGroup(powerGroup);
                }
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
