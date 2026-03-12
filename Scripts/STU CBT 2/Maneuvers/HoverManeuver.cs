
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
                    bool stableVelocity = FlightController.SetStableForwardVelocity(0);
                    FlightController.SetVr(0);
                    FlightController.SetVp(0);
                    FlightController.SetVw(0);
                    return stableVelocity;
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
