using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    partial class Program
    {
        public partial class StingerLogLCD : STUDisplay
        {
            public Queue<STULog> FlightLogs;

            public StingerLogLCD(IMyTerminalBlock block, int displayIndex, string font = "Monospace", float fontSize = 1) : base(block, displayIndex, font, fontSize)
            {
                FlightLogs = new Queue<STULog>();
            }
        }
    }
}
