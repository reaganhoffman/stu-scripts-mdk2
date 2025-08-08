using Sandbox.ModAPI.Ingame;

namespace IngameScript {
    public partial class Program : MyGridProgram {

        GenericShip Ship;

        public Program() {
            Ship = new GenericShip(GridTerminalSystem, Me);
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Save() { }

        public void Main(string argument, UpdateType updateSource) {
            try {

            } catch {

            } finally {
                Ship.UpdateLogDisplays();
            }
        }

    }
}
