using System;
using System.Collections;
using System.Collections.Generic;
namespace IngameScript {
    partial class Program {
        public partial class CBT {
            public class MovePiston : STUStateMachine {
                public override string Name => "Move Piston";
                public Queue<STUStateMachine> ManeuverQueue { get; set; }
                public Sandbox.ModAPI.Ingame.IMyPistonBase RearDockPiston { get; set; }
                public float TargetDistance { get; set; }
                public MovePiston(Queue<STUStateMachine> thisManeuverQueue, Sandbox.ModAPI.Ingame.IMyPistonBase piston, float targetDistance) {
                    RearDockPiston = piston;
                    TargetDistance = targetDistance;
                    ManeuverQueue = thisManeuverQueue;
                }

                public override bool Init() {
                    RearDockPiston.Enabled = true;
                    return true;
                }

                public override bool Run() {
                    if (Math.Abs(RearDockPiston.CurrentPosition - TargetDistance) < CBT_VARIABLES.PISTON_POSITION_TOLERANCE) {
                        RearDockPiston.Velocity = 0;
                        return true;
                    } else if (RearDockPiston.CurrentPosition < TargetDistance) {
                        RearDockPiston.MaxLimit = TargetDistance;
                        RearDockPiston.Velocity = CBT_VARIABLES.PISTON_TARGET_VELOCITY;
                        return false;
                    } else if (RearDockPiston.CurrentPosition > TargetDistance) {
                        RearDockPiston.MinLimit = TargetDistance;
                        RearDockPiston.Velocity = -CBT_VARIABLES.PISTON_TARGET_VELOCITY;
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
