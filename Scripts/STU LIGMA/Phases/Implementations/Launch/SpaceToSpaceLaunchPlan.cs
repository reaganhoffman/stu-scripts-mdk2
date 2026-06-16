using VRageMath;

namespace IngameScript {
    partial class Program {
        public partial class LIGMA {
            public class SpaceToSpaceLaunchPlan : ILaunchPlan {

                private int LAUNCH_VELOCITY = 15;
                private int LAUNCH_DISTANCE = 30;

                public override bool Run() {
                    FirstRunTasks();

                    FlightController.SetStableForwardVelocity(LAUNCH_VELOCITY);
                    if (Vector3D.Distance(FlightController.CurrentPosition, LaunchCoordinates) >= LAUNCH_DISTANCE) {
                        return true;
                    }

                    return false;
                }

            }
        }
    }
}
