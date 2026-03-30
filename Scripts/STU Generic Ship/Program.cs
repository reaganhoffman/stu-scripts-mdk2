using Sandbox.Game;
using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;
using VRageRender.Messages;

namespace IngameScript {
    public partial class Program : MyGridProgram {

        GenericShip Ship { get; set; }
        Queue<STUStateMachine> ManeuverQueue { get; set; }
        static STUStateMachine CurrentManeuver { get; set; }
        STUInventoryEnumerator InventoryEnumerator { get; set; }
        STUMasterLogBroadcaster Broadcaster { get; set; }
        IMyBroadcastListener Listener { get; set; }
        MyCommandLine CommandLineParser { get; set; } = new MyCommandLine();
        MyCommandLine WirelessMessageParser { get; set; } = new MyCommandLine();


        public Program() {
            
            
            
            Ship = new GenericShip(GridTerminalSystem, Me);
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }


        public void Main(string argument, UpdateType updateSource) {
            try {

            } catch {

            } finally {
                Ship.UpdateLogDisplays();
            }
        }

    }
}
