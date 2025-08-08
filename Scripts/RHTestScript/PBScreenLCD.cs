using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public partial class PBScreenLCD : STUDisplay
        {
            public Queue<STULog> Logs;
            public static Action<string> echo;

            public PBScreenLCD(IMyTerminalBlock block, int displayIndex, string font = "Monospace", float fontSize = 1) : base(block, displayIndex, font, fontSize)
            {
                Logs = new Queue<STULog>();
            }
        }
    }
}