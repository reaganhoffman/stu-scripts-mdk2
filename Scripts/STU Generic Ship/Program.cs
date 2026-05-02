using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame.Utilities;

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
