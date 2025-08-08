using System;
using System.Collections.Generic;

namespace IngameScript {
    partial class Program {
        public partial class CBT {
            public class MoveHinge : STUStateMachine {
                public override string Name => "Move Hinge";
                public Queue<STUStateMachine> ManeuverQueue { get; set; }
                public Sandbox.ModAPI.Ingame.IMyMotorStator Hinge { get; set; }
                public float TargetAngle { get; set; }
                public MoveHinge(Queue<STUStateMachine> thisManeuverQueue, Sandbox.ModAPI.Ingame.IMyMotorStator thisHinge, float angle) {
                    Hinge = thisHinge;
                    TargetAngle = angle;
                    ManeuverQueue = thisManeuverQueue;
                }

                public override bool Init() {
                    Hinge.Torque = CBTRearDock.HINGE_TORQUE;
                    return true;
                }

                public override bool Run() {
                    Hinge.Torque = CBTRearDock.HINGE_TORQUE;
                    if (Math.Abs(Hinge.Angle - TargetAngle) < CBTRearDock.HINGE_ANGLE_TOLERANCE) {
                        Hinge.TargetVelocityRPM = 0;
                        return true;
                    } else if (Hinge.Angle < TargetAngle) {
                        Hinge.UpperLimitRad = TargetAngle;
                        Hinge.TargetVelocityRPM = CBTRearDock.HINGE_TARGET_VELOCITY;
                        return false;
                    } else if (Hinge.Angle > TargetAngle) {
                        Hinge.LowerLimitRad = TargetAngle;
                        Hinge.TargetVelocityRPM = -CBTRearDock.HINGE_TARGET_VELOCITY;
                        return false;
                    } else
                        return false;
                }

                public override bool Closeout() {
                    return true;
                }
            }
        }
    }
}
