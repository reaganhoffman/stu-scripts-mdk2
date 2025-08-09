using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    public partial class Program : MyGridProgram
    {
        Rover ThisRover { get; set; }
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            ThisRover = new Rover(GridTerminalSystem, Me, Echo);
        }

        public void Main(string argument, UpdateType updateSource)
        {
            switch (argument.Trim().ToUpper())
            {
                case "TEST":
                    Rover.Test();
                    break;
                case "UP":
                    Rover.AirUp();
                    break;
                case "DOWN":
                    Rover.AirDown();
                    break;
            }
        }
    }
}
