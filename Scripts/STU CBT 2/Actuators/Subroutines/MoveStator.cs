using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;

namespace IngameScript {
    partial class Program {
        public partial class CBT {
            public class MoveStator : STUStateMachine {
                public override string Name => "Move Stator";
                public UpdateFrequency UpdateFreq { get; set; }
                public Queue<STUStateMachine> ManeuverQueue { get; set; }
                public IMyMotorStator Stator { get; set; }
                public float TargetRadians { get; set; }
                int _ticksElapsedSinceInitialCall { get; set; } = 0;
                int TicksElapsedSinceInitialCall
                {
                    get
                    {
                        return _ticksElapsedSinceInitialCall;
                    }
                    set
                    {
                        int factor;
                        switch (UpdateFreq)
                        {
                            case UpdateFrequency.Update1: factor = 1; break;
                            case UpdateFrequency.Update10: factor = 10; break;
                            case UpdateFrequency.Update100: factor = 100; break;
                            default: factor = 10; break;
                        }
                        _ticksElapsedSinceInitialCall = value += factor;
                    }
                }
                double CalculatedSecondsToRun { get; set; } = 0;
                float TimeoutAllowance { get; set; } = 0;
                /// <summary>
                /// Describes a generic stator movement action.
                /// </summary>
                /// <param name="updateFrequency">The runtime of the calling program. Referenced when a timeout is used.</param>
                /// <param name="thisManeuverQueue">The maneuver queue this maneuver will be scheduled into.</param>
                /// <param name="stator">The stator on which to act.</param>
                /// <param name="targetRadians">Target angle in radians.</param>
                /// <param name="timeoutAllowance">To use the timeout feature, set this to a positive number which represents the number of seconds past the calculated runtime to keep the maneuver in the Run state. The expected time to complete the Run state is calculated when this maneuver is instantiated. During the Run state, if this amount of time has passed, the manuever will shortcut to the Closeout state. Otherwise (if this value is not set), the Run state will only be exitable by the stator achieving its target angle.</param>
                public MoveStator(UpdateFrequency updateFrequency, Queue<STUStateMachine> thisManeuverQueue, IMyMotorStator stator, float targetRadians, float timeoutAllowance = 0) {
                    UpdateFreq = updateFrequency;
                    Stator = stator;
                    TargetRadians = targetRadians;
                    ManeuverQueue = thisManeuverQueue;
                    TimeoutAllowance = Math.Max(0, timeoutAllowance);
                    CalculatedSecondsToRun = (Math.Abs(Math.Max(stator.Angle, TargetRadians) - Math.Min(stator.Angle, TargetRadians)) / (Math.PI * 2)) / (CBT_VARIABLES.HINGE_TARGET_VELOCITY / 60);
                }

                public override bool Init() {
                    Stator.Torque = CBT_VARIABLES.HINGE_TORQUE;
                    Stator.Enabled = true;
                    return true;
                }

                public override bool Run() {
                    TicksElapsedSinceInitialCall++;
                    Stator.Torque = CBT_VARIABLES.HINGE_TORQUE;
                    if (Math.Abs(Stator.Angle - TargetRadians) < CBT_VARIABLES.HINGE_ANGLE_TOLERANCE || ShouldBeDone() ) {
                        Stator.TargetVelocityRPM = 0;
                        return true;
                    } else if (Stator.Angle < TargetRadians) {
                        Stator.UpperLimitRad = TargetRadians;
                        Stator.TargetVelocityRPM = CBT_VARIABLES.HINGE_TARGET_VELOCITY;
                        return false;
                    } else if (Stator.Angle > TargetRadians) {
                        Stator.LowerLimitRad = TargetRadians;
                        Stator.TargetVelocityRPM = -CBT_VARIABLES.HINGE_TARGET_VELOCITY;
                        return false;
                    } else
                        return false;
                }

                public override bool Closeout() {
                    Stator.BrakingTorque = CBT_VARIABLES.HINGE_TORQUE;
                    Stator.Enabled = false;
                    return true;
                }

                bool ShouldBeDone()
                {
                    if (TimeoutAllowance == 0f) return false;
                    return (TicksElapsedSinceInitialCall / 60) > CalculatedSecondsToRun + TimeoutAllowance;
                }
            }
        }
    }
}
