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
        public partial class StingerAutopilotLCD : STUDisplay
        {
            public StingerAutopilotLCD(IMyTerminalBlock block, int displayIndex, string font = "Monospace", float fontSize = 1) : base(block, displayIndex, font, fontSize)
            {

            }
        }
    }
}
