using VRageMath;

namespace IngameScript {
    partial class Program {
        public partial class LIGMA {
            public class PlanetToSpaceTerminalPlan : ITerminalPlan {

                public override int TERMINAL_VELOCITY => 250;

                public override bool Run() {
                    FirstRunTasks();
                    FlightController.SetV_WorldFrame(Base6Directions.Direction.Forward, TERMINAL_VELOCITY);
                    FlightController.AlignShipToTarget(TargetData.Position);
                    FlightController.OptimizeShipRoll(TargetData.Position);
                    return false;
                }

            }
        }
    }
}
