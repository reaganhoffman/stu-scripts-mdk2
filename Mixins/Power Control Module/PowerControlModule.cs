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
            MyIni _ini = new MyIni();
            public bool LowPowerModeActivated { get; private set; } = false;
            public string PowerGroupsSaveState { get; private set; } = "";

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
            };

            public PowerControlModule(Action<string> echo, string saveState)
            {
                Echo = echo;
                LoadSaveState(saveState);
                SaveCurrentState();
            }

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
                Echo($"current power group: {powerGroup.Name}");
                Echo($"current power group state: {powerGroup.Enabled}");
                powerGroup.Enabled = true; // change the state of the PowerClass in memory
                Echo($"power group state after enabling: {powerGroup.Enabled}");
            }

            public void DisablePowerGroup(PowerGroup powerGroup)
            {
                foreach (var block in powerGroup.Blocks)
                {
                    block.Enabled = false; // turn the block off
                }
                Echo($"current power group: {powerGroup.Name}");
                Echo($"current power group state: {powerGroup.Enabled}");
                powerGroup.Enabled = false; // change the state of the PowerClass in memory
                Echo($"power group state after disabling: {powerGroup.Enabled}");
            }

            public void AllOn()
            {
                foreach (var group in PowerGroups)
                {
                    EnablePowerGroup(group);
                }
                SaveCurrentState();
            }

            public void GoToLowPowerMode()
            {
                SaveCurrentState();
                foreach (var powerGroup in PowerGroups) DisablePowerGroup(powerGroup);
                LowPowerModeActivated = true;
                Echo($"save state after GTLPM: {PowerGroupsSaveState}");
            }

            public void SaveCurrentState()
            {
                _ini.Clear();
                PowerGroupsSaveState = "";
                foreach (var powerGroup in PowerGroups)
                {
                    _ini.Set("POWER", powerGroup.Name, powerGroup.Enabled);
                }
                PowerGroupsSaveState = _ini.ToString();
                Echo($"PowerGroupsSaveState:\n{PowerGroupsSaveState}");
            }
                        
            public void LoadSaveState(string saveState)
            {
                _ini.Clear();
                MyIniParseResult result;
                _ini.TryParse(saveState, out result); // not checking whether this fails (yet)
                Echo($"LoadSaveState result: {result.ToString()}");

                List<MyIniKey> keys = new List<MyIniKey>();
                _ini.GetKeys("POWER", keys); // still holding off on error checking
                foreach (var key in keys)
                {
                    PowerGroup powerGroup;
                    if (!TryGetPowerGroup(key.Name, out powerGroup))
                    {
                        AllOn();
                        Echo($"Error getting value in LoadSaveState. turning all blocks on.");
                        return; // fail secure in case the ini wasn't written properly
                    }
                    ;
                    bool savedState = _ini.Get(key).ToBoolean();
                    switch (savedState)
                    {
                        case true: EnablePowerGroup(powerGroup); break;
                        case false: DisablePowerGroup(powerGroup); break;
                    }
                }

                LowPowerModeActivated = false;
            }

            bool IsPartOfPowerGroup(IMyFunctionalBlock block, string group)
            {
                _ini.Clear();
                MyIniParseResult result;
                if (!_ini.TryParse(block.CustomData, out result)) return false;

                return _ini.Get("POWER", $"{group}").ToBoolean();
            }

        }
    }
}
