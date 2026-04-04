using System;
using System.Collections.Generic;

namespace IngameScript {
    partial class Program {
        public partial class CBT {
            public class MoveStator : STUStateMachine {
                public override string Name => "Move Stator";
                public Queue<STUStateMachine> ManeuverQueue { get; set; }
                public Sandbox.ModAPI.Ingame.IMyMotorStator Stator { get; set; }
                public float TargetAngle { get; set; }
                public MoveStator(Queue<STUStateMachine> thisManeuverQueue, Sandbox.ModAPI.Ingame.IMyMotorStator stator, float angle) {
                    Stator = stator;
                    TargetAngle = angle;
                    ManeuverQueue = thisManeuverQueue;
                }

                public override bool Init() {
                    Stator.Torque = CBT_VARIABLES.HINGE_TORQUE;
                    Stator.Enabled = true;
                    return true;
                }

                public override bool Run() {
                    Stator.Torque = CBT_VARIABLES.HINGE_TORQUE;
                    if (Math.Abs(Stator.Angle - TargetAngle) < CBT_VARIABLES.HINGE_ANGLE_TOLERANCE) {
                        Stator.TargetVelocityRPM = 0;
                        return true;
                    } else if (Stator.Angle < TargetAngle) {
                        Stator.UpperLimitRad = TargetAngle;
                        Stator.TargetVelocityRPM = CBT_VARIABLES.HINGE_TARGET_VELOCITY;
                        return false;
                    } else if (Stator.Angle > TargetAngle) {
                        Stator.LowerLimitRad = TargetAngle;
                        Stator.TargetVelocityRPM = -CBT_VARIABLES.HINGE_TARGET_VELOCITY;
                        return false;
                    } else
                        return false;
                }

                public override bool Closeout() {
                    Stator.Enabled = false;
                    return true;
                }
            }
        }
    }
}
