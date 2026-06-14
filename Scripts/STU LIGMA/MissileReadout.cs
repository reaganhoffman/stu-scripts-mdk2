using Sandbox.ModAPI.Ingame;

namespace IngameScript {
    partial class Program {
        public class MissileReadout : STUDisplay {

            public LIGMA Missile { get; set; }

            public MissileReadout(IMyTerminalBlock block, int displayIndex, LIGMA missile) : base(block, displayIndex, 1f, "Monospace") {
                Missile = missile;
            }

        }
    }
}
