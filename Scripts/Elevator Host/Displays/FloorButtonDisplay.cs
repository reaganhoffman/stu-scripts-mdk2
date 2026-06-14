using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    partial class Program
    {
        public class FloorButtonDisplay : STUDisplay
        {
            public FloorButtonDisplay(IMyButtonPanel panel, Elevator.Direction direction) : base(panel as IMyTerminalBlock, 0, "Monospace", 1f)
            {

            }
        }
    }
    
}
