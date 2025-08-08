
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;


namespace IngameScript
{
    public partial class Program : MyGridProgram
    {
        public static List<IMyTerminalBlock> AllTerminalBlocks { get; set; } = new List<IMyTerminalBlock>();
        public static List<IMyGasTank> AllGasTanks { get; set; } = new List<IMyGasTank>();
        public static PBScreenLCD PBScreen { get; set; }

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            GridTerminalSystem.GetBlocksOfType<IMyGasTank>(AllGasTanks);
            PBScreen = new PBScreenLCD(Me, 0, "Monospace", 0.5f);
        }

        public void Main(string argument, UpdateType updateSource)
        {
            foreach (var item in AllGasTanks)
            {
                Echo($"{item.CustomName}: {item.BlockDefinition.SubtypeId}");
                AddToLogQueue($"{item.CustomName}: {item.BlockDefinition.SubtypeId}");
            }

            UpdateLogScreens();
        }

        public void AddToLogQueue(string message)
        {
            PBScreen.Logs.Enqueue(new STULog("me", message, "INFO"));
        }

        public static void UpdateLogScreens()
        {
            PBScreen.StartFrame();
            PBScreen.WriteWrappableLogs(PBScreen.Logs);
            PBScreen.EndAndPaintFrame();
        }
    }
}
