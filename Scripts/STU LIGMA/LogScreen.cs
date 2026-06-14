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

            public LogScreen(IMyTerminalBlock block, int displayIndex, float fontSize, string font = "Monospace") : base(block, displayIndex, fontSize, font)
            {
                Logs = new Queue<STULog>();
            }

            public void Refresh()
            {
                StartFrame();
                LIGMA.CreateOkBroadcast($"{this.Surface.FontSize}");
                WriteWrappableLogs(Logs);
                EndAndPaintFrame();
            }
        }
    }
}
