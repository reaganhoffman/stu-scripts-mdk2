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

            public LogScreen(IMyTerminalBlock block, int displayIndex, float fontSize = 1, string font = "Monospace") : base(block, displayIndex, font, fontSize)
            {
                Logs = new Queue<STULog>();
            }

            public void Refresh()
            {
                StartFrame();
                WriteWrappableLogs(Logs);
                EndAndPaintFrame();
            }
        }
    }
}
