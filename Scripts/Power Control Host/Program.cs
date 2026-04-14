using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript
{
    public partial class Program : MyGridProgram
    {
        MyIni _ini { get; set; }
        MyCommandLine CommandLineParser { get; set; } = new MyCommandLine();
        List<IMyFunctionalBlock> FunctionalBlocks { get; set; } = new List<IMyFunctionalBlock>();
        static PowerControlModule PCM { get; set; }

        public Program()
        {
            _ini = new MyIni();
            try
            {
                _ini.TryParse(Storage);   
            }
            catch (Exception e)
            {
                Echo($"{e.Message}\n{e.Source}\n{e.StackTrace}");
            }
            finally
            {
                GridTerminalSystem.GetBlocksOfType(FunctionalBlocks);
                PCM = new PowerControlModule(Echo, _ini.ToString()); // attempt to load from the Storage string.
                                                                     // The PowerControlModule constructor handles if the string is invalid
                                                                     // by way of PowerControlModule.LoadSaveState().
                PCM.RefreshGroupMembership(FunctionalBlocks);
            }
        }

        public void Save()
        {
            _ini.Clear();

            foreach (var powerGroup in PCM.PowerGroups)
            {
                _ini.Set("POWER", powerGroup.Name, powerGroup.Enabled);
            }

            Storage = _ini.ToString();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            try
            {
                CommandLineParser.TryParse(argument);
                if (CommandLineParser.Argument(0).ToUpper() != "POWER") return; // command has to start with POWER
                
                if (CommandLineParser.Switch("r") || CommandLineParser.Switch("R")) // check 'r' switch first, then continue
                {
                    FunctionalBlocks = new List<IMyFunctionalBlock>();
                    GridTerminalSystem.GetBlocksOfType(FunctionalBlocks);
                    PCM.RefreshGroupMembership(FunctionalBlocks);
                }

                if (CommandLineParser.ArgumentCount < 2) return; 

                if (CommandLineParser.Argument(1).ToUpper() == "LOW") { HandleLowPowerModeRequest(); return; }
                // if first argument is LOW, immediately process as a low power mode request and return.
                if (CommandLineParser.Argument(1).ToUpper() == "ON") { PCM.AllOn(); return; }

                PowerControlModule.PowerGroup userRequestedPowerGroup;
                if (PCM.TryGetPowerGroup(CommandLineParser.Argument(1), out userRequestedPowerGroup))
                {
                    if (Check("e"))
                    {
                        ProcessUserRequest(userRequestedPowerGroup, true);
                        return;
                    }
                    if (Check("d"))
                    {
                        ProcessUserRequest(userRequestedPowerGroup, false);
                        return;
                    }
                    if (CommandLineParser.Switches.Count == 0)
                    {
                        PCM.TogglePowerGroup(userRequestedPowerGroup);
                    }
                }
            }
            catch (Exception e)
            {
                Echo($"Invalid command: \n'{argument}'\n{e.Message}\n{e.StackTrace}\n{e.InnerException}");
            }
        }

        void ProcessUserRequest(PowerControlModule.PowerGroup powerGroup, bool desiredState = true)
        {
            switch (desiredState)
            {
                case true: PCM.EnablePowerGroup(powerGroup); return;
                case false: PCM.DisablePowerGroup(powerGroup); return;
            }
        }

        void HandleLowPowerModeRequest()
        {
            bool desiredLowPowerModeState = false;
            if (Check("e")) desiredLowPowerModeState = true;
            if (Check("d")) desiredLowPowerModeState = false;
            if (!(Check("e") | Check("d"))) desiredLowPowerModeState = !PCM.LowPowerModeActivated;
            Echo($"desiredLowPowerModestate: {desiredLowPowerModeState}");
            switch (desiredLowPowerModeState)
            {
                case true: PCM.GoToLowPowerMode(); break;
                case false: PCM.LoadSaveState(PCM.PowerGroupsSaveState); break;
            }
        }

        bool Check(string @switch)
        {
            return CommandLineParser.Switch(@switch.ToLower()) || CommandLineParser.Switch(@switch.ToUpper());
        }
    }
}
