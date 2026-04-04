using Sandbox.Game.Entities.Cube;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.WorldEnvironment.Modules;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    public partial class Program
    {
        public static class PowerControlModule
        {
            public struct PowerGroup
            {
                public string Name;
                public bool Enabled;
                public List<IMyFunctionalBlock> Blocks;
            }


            public static PowerGroup[] PowerGroups { get; private set; } = new PowerGroup[] {

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

            static List<PowerGroup> PowerGroupsBeforeLowPowerMode { get; set; } = new List<PowerGroup>();

            public static void RefreshGroupMembership(List<IMyFunctionalBlock> blocks)
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

            public static PowerGroup GetPowerGroupByName(string name)
            {
                foreach (var item in PowerGroups)
                {
                    if (item.Name == name.ToUpper())
                        return item;
                }
                return new PowerGroup();
            }

            static public void TogglePowerGroup(PowerGroup powerGroup)
            {
                bool newState = !powerGroup.Enabled;

                foreach (var block in powerGroup.Blocks)
                {
                    if (IsPartOfPowerGroup(block, powerGroup.Name))
                    {
                        CBT.AddToLogQueue($"setting block {block.CustomName} {BoolConverter(newState)}");
                        block.Enabled = newState; // set the block's enabled property to the opposite of the state of the power class in memory
                    }
                }

                CBT.AddToLogQueue($"power class is currently {BoolConverter(powerGroup.Enabled)}");
                powerGroup.Enabled = newState; // toggle the state of the power class in memory
                CBT.AddToLogQueue($"just set power class to {BoolConverter(powerGroup.Enabled)} in memory");
            }

            static public void EnablePowerGroup(PowerGroup powerGroup)
            {
                if (powerGroup.Name == "LOW") return; // disallow Low Power Mode to be invoked this way; use GoToLowPowerMode / RestoreFromLowPowerMode
                foreach (var block in powerGroup.Blocks)
                {
                    block.Enabled = true; // turn the block on 
                }
                powerGroup.Enabled = true; // change the state of the PowerClass in memory
            }

            static public void DisablePowerGroup(PowerGroup powerGroup)
            {
                if (powerGroup.Name == "LOW") return; // disallow Low Power Mode to be invoked this way; use GoToLowPowerMode / RestoreFromLowPowerMode
                foreach (var block in powerGroup.Blocks)
                {
                    block.Enabled = false; // turn the block off
                } 
                powerGroup.Enabled = false; // change the state of the PowerClass in memory
            }

            static public void GoToLowPowerMode()
            {
                CBT.AddToLogQueue($"going to low power mode");
                foreach (var powerGroup in PowerGroups)
                {
                    CBT.AddToLogQueue($"PG {powerGroup.Name} is currently {BoolConverter(powerGroup.Enabled)}");
                    if (powerGroup.Enabled)
                    {
                        PowerGroupsBeforeLowPowerMode.Add(powerGroup);
                    }
                    DisablePowerGroup(powerGroup);
                }
            }

            static public void RestoreFromLowPowerMode()
            {
                CBT.AddToLogQueue("restoring from low power mode");
                foreach (var powerGroup in PowerGroupsBeforeLowPowerMode)
                {
                    CBT.AddToLogQueue($"found group {powerGroup.Name} in PGBLPM");
                    if (powerGroup.Enabled)
                        EnablePowerGroup(powerGroup);
                }
                PowerGroupsBeforeLowPowerMode.Clear();
            }

            static bool IsPartOfPowerGroup(IMyFunctionalBlock block, string group)
            {
                MyIni ini = new MyIni();

                MyIniParseResult result;
                if (!ini.TryParse(block.CustomData, out result))
                {
                    // this will throw an exception if there is an error parsing the custom data
                    // this is expected behavior (?) in the case that there is no text in the custom data
                    // throw new Exception(result.ToString());
                    return false;
                }

                return ini.Get("POWER", $"{group}").ToBoolean();
            }

        }
    }
}
