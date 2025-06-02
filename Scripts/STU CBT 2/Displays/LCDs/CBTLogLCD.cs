using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;

namespace IngameScript {
    partial class Program {
        public partial class CBTLogLCD : STUDisplay {
            public Queue<STULog> FlightLogs;
            public static Action<string> echo;

            public CBTLogLCD(Action<string> Echo, IMyTerminalBlock block, int displayIndex, string font = "Monospace", float fontSize = 1) : base(block, displayIndex, font, fontSize) {
                echo = Echo;
                FlightLogs = new Queue<STULog>();
            }
        }
    }
}
