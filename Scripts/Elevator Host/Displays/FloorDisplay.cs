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
        public class FloorDisplay : STUDisplay
        {
            public FloorDisplay(IMyTerminalBlock block) : base(block, 0, 1f, "Monospace")
            {

            }
        }
    }
    
}
