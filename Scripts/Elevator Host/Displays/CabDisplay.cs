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
        public class CabDisplay : STUDisplay
        {
            public CabDisplay(IMyTerminalBlock block, int displayIndex, float fontSize = 1f, string font = "Monospace" ) : base(block, displayIndex, fontSize, font)
            {
                
            }

            public void Refresh(int currentFloor, int desiredFloor, Elevator.State state)
            {

            }
        }
    }
    
}
