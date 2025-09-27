using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;

namespace IngameScript {
    partial class Program {
        public partial class MainLogDisplay : STUDisplay {

            public Queue<STULog> Logs { get; set; }

            public MainLogDisplay(IMyTerminalBlock block, int displayIndex, string font = "Monospace", float fontSize = 1) : base(block, displayIndex, font, fontSize) {
                Logs = new Queue<STULog>();
            }

            public void Update() {
                StartFrame();
                WriteWrappableLogs(Logs);
                EndAndPaintFrame();
            }

        }
    }
}
