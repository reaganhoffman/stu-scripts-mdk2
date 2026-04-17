using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
        STUInventoryEnumerator InventoryEnumerator { get; set; }
        STUMasterLogBroadcaster Broadcaster { get; set; }
        MyCommandLine CommandLine { get; set; }
        MyCommandLine WirelessMessageCommandLine { get; set; }
        MyIni _ini { get; set; }
        Queue<STUStateMachine> ManeuverQueue { get; set; }
        static STUStateMachine CurrentManeuver { get; set; }

        static BALLS _balls { get; set; }

        

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            InventoryEnumerator = new STUInventoryEnumerator(GridTerminalSystem, Me);
            Broadcaster = new STUMasterLogBroadcaster("test channel", IGC, TransmissionDistance.AntennaRelay);
            CommandLine = new MyCommandLine();
            WirelessMessageCommandLine = new MyCommandLine();
            _ini = new MyIni();
            _balls = new BALLS(GridTerminalSystem, Runtime, IGC, Me, "");
        }

        public void Save()
        {
            
        }

        public void Main(string argument, UpdateType updateSource)
        {
            InventoryEnumerator.EnumerateInventories();

            // handle command line

            // handle wireless message

            switch (BALLS.CurrentState)
            {
                case BALLS.State.Idle:
                    // check if any targets came in
                    break;
                case BALLS.State.Executing:
                    // do ya thing
                    break;
            }
        }
    }
}
