using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;

namespace IngameScript {
    partial class Program {
        public class LogScreen : STUDisplay {
            public Queue<STULog> Logs { get; set; }

            public LogScreen(IMyTerminalBlock block, int displayIndex, float fontSize, string font = "Monospace") : base(block, displayIndex, fontSize, font) {
                Logs = new Queue<STULog>();
            }

            public void Refresh() {
                StartFrame();
                WriteWrappableLogs(Logs);
                EndAndPaintFrame();
            }
        }
    }
}
