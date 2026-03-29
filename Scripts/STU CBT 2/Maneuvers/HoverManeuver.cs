
namespace IngameScript {
    partial class Program {
        public partial class CBT {
            public class HoverManeuver : STUStateMachine {
                public override string Name => "Hover";
                public HoverManeuver() {

                }

                public override bool Init() {
                    // ensure we have access to the thrusters, gyros, and dampeners are on
                    SetAutopilotControl(true, true, true);
                    ResetUserInputVelocities();
                    return true;
                }

                public override bool Run() {
                    FlightController.SetV_WorldFrame(FlightController.CurrentPosition, 0);
                    FlightController.SetAxialVelocity(new VRageMath.Vector3D(0, 0, 0));
                    return FlightController.VelocityMagnitude <= 0.01;
                }

                public override bool Closeout() {
                    // stabilize gyros
                    foreach (var gyro in CBT.FlightController.AllGyroscopes)
                    {
                        gyro.Pitch = 0;
                        gyro.Yaw = 0;
                        gyro.Roll = 0;
                    }
                    // relinquish control of the thrusters and gyros, keep dampeners on 
                    SetAutopilotControl(false, false, true);

                    CBT.ResetUserInputVelocities();
                    return true;
                }
            }
        }
    }
}
