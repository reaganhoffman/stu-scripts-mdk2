using System;
using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game;

namespace IngameScript
{
    public partial class Program
    {
        public class PowerControlModule
        {
            public struct PowerClass
            {
                public string Class;
                public bool Enabled;
            }

            List<IMyFunctionalBlock> AllFunctionalBlocks { get; set; }

            public PowerControlModule(List<IMyFunctionalBlock> allFunctionalBlocks)
            {
                AllFunctionalBlocks = allFunctionalBlocks;
            }

            public static PowerClass[] PowerClasses { get; private set; } = new PowerClass[] { 
                
                new PowerClass{Class = "FLIGHT", Enabled = true},
                new PowerClass{Class = "LIFE SUPPORT", Enabled = true},
                new PowerClass{Class = "WEAPONS", Enabled = true },
                new PowerClass{Class = "EGRESS", Enabled = true },
                new PowerClass{Class = "PRODUCTION", Enabled = true},
                new PowerClass{Class = "COMFORT", Enabled = true},
                new PowerClass{Class = "DECORATIVE", Enabled = true},
                new PowerClass{Class = "MISC", Enabled = true},
                new PowerClass{Class = "LOW", Enabled = true}
            };

            static List<PowerClass> PowerClassesBeforeLowPowerMode { get; set; }

            public void UpdateBlocks(List<IMyFunctionalBlock> blocks)
            {
                AllFunctionalBlocks = blocks;
            }

            public static PowerClass GetPowerClassByName(string name)
            {
                foreach (var item in PowerClasses)
                {
                    if (item.Class == name.ToUpper())
                        return item;
                }
                return new PowerClass();
            }

            public void TogglePowerClass(PowerClass powerClass)
            {
                powerClass.Enabled = !powerClass.Enabled; // toggle the state of the power class in memory

                foreach (var block in AllFunctionalBlocks)
                {
                    if (IsPartOfClass(block, powerClass.Class)) 
                        block.Enabled = powerClass.Enabled; // set the block's enabled property to whatever the power class in memory says it should be
                }
            }

            public void EnablePowerClass(PowerClass powerClass)
            {
                foreach (var block in AllFunctionalBlocks)
                {
                    if (IsPartOfClass(block, powerClass.Class))
                        block.Enabled = true; // turn the block on 
                }
                powerClass.Enabled = true; // change the state of the PowerClass in memory
            }

            public void DisablePowerClass(PowerClass powerClass)
            {
                foreach (var block in AllFunctionalBlocks)
                {
                    if (IsPartOfClass(block, powerClass.Class))
                        block.Enabled = false; // turn the block off
                }
                powerClass.Enabled = false; // change the state of the PowerClass in memory
            }

            public void GoToLowPowerMode()
            {
                foreach (var @class in PowerClasses)
                {
                    PowerClassesBeforeLowPowerMode.Add(@class);
                    DisablePowerClass(@class);
                }
            }

            public void RestoreFromLowPowerMode(List<IMyFunctionalBlock> blocks)
            {
                foreach (var @class in PowerClassesBeforeLowPowerMode)
                {
                    if (@class.Enabled)
                        EnablePowerClass(@class);

                    PowerClassesBeforeLowPowerMode.Remove(@class);
                }
            }

            bool IsPartOfClass(IMyFunctionalBlock block, string @class)
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

                return ini.Get("POWER", $"{@class}").ToBoolean();
            }
        }
    }
}
