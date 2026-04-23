using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class LogScreen : STUDisplay
        {
            public Queue<STULog> Logs { get; set; }

            public LogScreen(IMyTerminalBlock block, int displayIndex, string font = "Monospace", float fontSize = 1) : base(block, displayIndex, font, fontSize)
            {
                Logs = new Queue<STULog>();
            }
        }
    }
}
