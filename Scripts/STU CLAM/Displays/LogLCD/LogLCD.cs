using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public partial class LogLCD : STUDisplay
        {

            public Queue<STULog> Logs { get; set; }

            public LogLCD(IMyTerminalBlock block, int displayIndex, string font = "Monospace", float fontSize = 1) : base(block, displayIndex, fontSize, font)
            {
                Logs = new Queue<STULog>();
            }

        }
    }
}
