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
        MyIni _ini { get; set; } = new MyIni();
        MyCommandLine CommandLineParser { get; set; } = new MyCommandLine();
        List<IMyTerminalBlock> TerminalBlocks { get; set; }
        List<IMyFunctionalBlock> FunctionalBlocks { get; set; }
        static PowerControlModule PCM { get; set; }

        public Program()
        {
            try
            {
                _ini.TryParse(Storage);
                GridTerminalSystem.GetBlocksOfType(FunctionalBlocks);
                Echo($"test: {FunctionalBlocks.Count}");
                GridTerminalSystem.GetBlocks(TerminalBlocks);
                Echo($"terminal blocks: {TerminalBlocks.Count}");
                PCM = new PowerControlModule();
                //PCM.RefreshGroupMembership(FunctionalBlocks);
            }
            catch (Exception e)
            {
                //Echo($"{e.Message}\n{e.Source}\n{e.StackTrace}");
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
            Echo($"test: {FunctionalBlocks.Count}\n" +
                $"terminal blocks: {TerminalBlocks.Count}");
            try
            {
                CommandLineParser.TryParse(argument);
                if (CommandLineParser.Argument(0).ToUpper() != "POWER") return;
                if (CommandLineParser.Switch("r") | CommandLineParser.Switch("R"))
                {
                    GridTerminalSystem.GetBlocksOfType<IMyFunctionalBlock>(FunctionalBlocks);
                    PCM.RefreshGroupMembership(FunctionalBlocks);
                    return;
                }
                PowerControlModule.PowerGroup userRequestedPowerGroup;
                if (PCM.TryGetPowerGroup(CommandLineParser.Argument(1), out userRequestedPowerGroup))
                {
                    bool userRequestedState;
                    if (CommandLineParser.Switches.Contains("e") | CommandLineParser.Switches.Contains("E"))
                    {
                        userRequestedState = CommandLineParser.Switch("e") | CommandLineParser.Switch("E");
                        ProcessUserRequest(userRequestedPowerGroup, userRequestedState);
                        return;
                    }
                    if (CommandLineParser.Switches.Contains("d") | CommandLineParser.Switches.Contains("D"))
                    {
                        userRequestedState = CommandLineParser.Switch("d") | CommandLineParser.Switch("D");
                        userRequestedState = !userRequestedState;
                        ProcessUserRequest(userRequestedPowerGroup, userRequestedState);
                        return;
                    }
                    if (CommandLineParser.Switches.DefaultIfEmpty<string>().Equals(null))
                    {

                        bool currentState = userRequestedPowerGroup.Enabled;
                        ProcessUserRequest(userRequestedPowerGroup, !currentState);
                    }
                }
                else throw new Exception();
            }
            catch
            {
                Echo($"Invalid command: \n'{argument}'");
            }
        }

        void ProcessUserRequest(PowerControlModule.PowerGroup powerGroup, bool desiredState = true)
        {
            if (powerGroup.Name == "LOW")
            {
                switch (powerGroup.Enabled)
                {
                    case true: PCM.RestoreFromSaveState(); return;
                    case false: PCM.GoToLowPowerMode(); return;
                }
            }
            switch (desiredState)
            {
                case true: PCM.EnablePowerGroup(powerGroup); return;
                case false: PCM.DisablePowerGroup(powerGroup); return;
            }
        }
    }
}
