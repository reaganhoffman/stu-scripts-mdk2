namespace IngameScript {
    partial class Program {
        public partial class LIGMA {
            public class SpaceToSpaceTerminalPlan : ITerminalPlan {

                public override int TERMINAL_VELOCITY => 200;

                public override bool Run() {
                    FirstRunTasks();
                    FlightController.SetV_WorldFrame(Base6Directions.Direction.Forward, TERMINAL_VELOCITY);
                    FlightController.AlignShipToTarget(TargetData.Position);
                    return false;
                }

            }
        }
    }
}