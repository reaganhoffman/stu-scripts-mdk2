using Sandbox.ModAPI.Ingame;
using System;

namespace IngameScript {
    partial class Program {
        public partial class CBTGangway {
            private const float HINGE_ANGLE_TOLERANCE = 0.0071f;
            private const float HINGE_TARGET_VELOCITY_RPM = 3f;
            private const float HINGE_TORQUE = 7000000;
            public static IMyMotorStator GangwayHinge1 { get; set; }
            public static IMyMotorStator GangwayHinge2 { get; set; }
            public enum GangwayStates {
                Unknown,
                Retracting,
                Retracted,
                Extending,
                Extended,
                Resetting,
                ResettingHinge1,
                ResettingHinge2,
                Frozen, // currently not used
            }
            public GangwayStates CurrentGangwayState { get; private set; }
            private GangwayStates LastUserInputGangwayState { get; set; }

            public CBTGangway(IMyMotorStator hinge1, IMyMotorStator hinge2) {
                // constructor

                GangwayHinge1 = hinge1;
                GangwayHinge2 = hinge2;
                CurrentGangwayState = GangwayStates.Unknown;
                LastUserInputGangwayState = CBT.UserInputGangwayState;

                switch (TryDetermineState()) {
                    case GangwayStates.Extended:
                        CurrentGangwayState = GangwayStates.Extended;
                        break;
                    case GangwayStates.Retracted:
                        CurrentGangwayState = GangwayStates.Retracted;
                        break;
                    default:
                        CurrentGangwayState = GangwayStates.Unknown;
                        break;
                }
            }

            // state machine
            public void UpdateGangway(GangwayStates desiredState) {
                if (desiredState != CurrentGangwayState && desiredState != LastUserInputGangwayState) {
                    if (CanGoToRequestedState(desiredState)) {
                        CurrentGangwayState = desiredState;
                        LastUserInputGangwayState = desiredState;
                    } else {
                        CBT.AddToLogQueue($"Cannot go to requested state {desiredState}", STULogType.ERROR);
                    }
                }
                switch (CurrentGangwayState) {
                    case GangwayStates.Unknown:
                        // DO NOTHING
                        break;

                    case GangwayStates.Resetting:
                        if (ResetGangwayActuators()) { CurrentGangwayState = GangwayStates.ResettingHinge1; }
                        break;

                    case GangwayStates.Retracting:
                        GangwayHinge1.RotorLock = false;
                        GangwayHinge2.RotorLock = false;
                        if (RetractGangway()) { CurrentGangwayState = GangwayStates.Retracted; }
                        break;

                    case GangwayStates.Retracted:
                        // DO NOTHING
                        break;

                    case GangwayStates.Extending:
                        GangwayHinge1.RotorLock = false;
                        GangwayHinge2.RotorLock = false;
                        if (ExtendGangway()) { CurrentGangwayState = GangwayStates.Extended; }
                        break;

                    case GangwayStates.Extended:
                        // DO NOTHING
                        break;

                    case GangwayStates.ResettingHinge1:
                        if (ResetHinge1()) { CurrentGangwayState = GangwayStates.ResettingHinge2; }
                        break;

                    case GangwayStates.ResettingHinge2:
                        if (ResetHinge2()) { CurrentGangwayState = GangwayStates.Retracted; }
                        break;

                    case GangwayStates.Frozen: // currently not used / inaccessable
                        CBT.AddToLogQueue($"Halting Gangway actuators.", STULogType.INFO);
                        GangwayHinge1.TargetVelocityRad = 0;
                        GangwayHinge2.TargetVelocityRad = 0;
                        break;
                }
            }

            // methods
            public GangwayStates TryDetermineState() {
                if (Math.Abs(GangwayHinge1.Angle) < HINGE_ANGLE_TOLERANCE && Math.Abs(GangwayHinge2.Angle - (Math.PI / 2)) < HINGE_ANGLE_TOLERANCE) {
                    return GangwayStates.Extended;
                } else if (GangwayHinge1.Angle < -1.56 && GangwayHinge2.Angle < -1.52) {
                    return GangwayStates.Retracted;
                } else { return GangwayStates.Unknown; }
            }

            public bool CanGoToRequestedState(GangwayStates requestedState) {
                switch (requestedState) {
                    // (case) HowDoIGetToThisState?
                    // (return) you can only get to this state if these || conditions !& are && met
                    // i.e. you can get to Unknown from anywhere, but you can only get to ResettingHinge1 from ResettingHinge2
                    case GangwayStates.Unknown:
                        return true;
                    case GangwayStates.Resetting:
                        return true;
                    case GangwayStates.Retracting:
                        return CurrentGangwayState == GangwayStates.Extended || CurrentGangwayState == GangwayStates.Retracted;
                    case GangwayStates.Retracted:
                        return CurrentGangwayState == GangwayStates.Retracting;
                    case GangwayStates.Extending:
                        return CurrentGangwayState == GangwayStates.Retracted || CurrentGangwayState == GangwayStates.Extended;
                    case GangwayStates.Extended:
                        return CurrentGangwayState == GangwayStates.Extending;
                    case GangwayStates.ResettingHinge1:
                        return CurrentGangwayState == GangwayStates.ResettingHinge2;
                    case GangwayStates.ResettingHinge2:
                        return CurrentGangwayState == GangwayStates.Resetting;
                    case GangwayStates.Frozen:
                        return true;
                    default:
                        return false;
                }
            }


            private bool ResetGangwayActuators() {
                GangwayHinge2.TargetVelocityRPM = 0;
                GangwayHinge2.Torque = 0;
                GangwayHinge2.BrakingTorque = 0;
                GangwayHinge2.RotorLock = false;
                GangwayHinge2.UpperLimitDeg = 90;
                GangwayHinge2.LowerLimitDeg = -90;
                GangwayHinge2.Enabled = false;

                GangwayHinge1.TargetVelocityRPM = 0;
                GangwayHinge1.Torque = 0;
                GangwayHinge1.BrakingTorque = 0;
                GangwayHinge1.RotorLock = false;
                GangwayHinge1.UpperLimitDeg = 90;
                GangwayHinge1.LowerLimitDeg = -90;
                GangwayHinge1.Enabled = false;

                return true;
            }

            private bool ResetHinge1() // hinge 1 should be the first to reset (close / retract)
            {
                // keeping the hinge's limits wide open, set it on a course to close (-90 degrees)
                GangwayHinge1.TargetVelocityRPM = -HINGE_TARGET_VELOCITY_RPM;
                GangwayHinge1.Torque = HINGE_TORQUE;
                GangwayHinge1.Enabled = true;

                if (CBT.RadToDeg(GangwayHinge1.Angle) < -85) // if it's past -85 degrees, fully define its limits (may cause slight snapping) and then set braking torque
                {
                    GangwayHinge1.UpperLimitDeg = 0;
                    GangwayHinge1.LowerLimitDeg = -90;
                    GangwayHinge1.BrakingTorque = HINGE_TORQUE;
                    return true;
                }
                return false;
            }

            private bool ResetHinge2() // hinge 2 should be reset AFTER hinge 1 is reset (closed / retracted)
            {
                // keeping the hinge's limits wide open, set it on a course to close (-90 degrees)
                GangwayHinge2.TargetVelocityRPM = -HINGE_TARGET_VELOCITY_RPM;
                GangwayHinge2.Torque = HINGE_TORQUE;
                GangwayHinge2.Enabled = true;

                if (CBT.RadToDeg(GangwayHinge2.Angle) < -85) { // once the angle gets less than -85 degrees, it's well on its way. now we set limits and braking torque
                    GangwayHinge2.UpperLimitDeg = 90;
                    GangwayHinge2.LowerLimitDeg = -90;
                    GangwayHinge2.BrakingTorque = HINGE_TORQUE;
                    return true;
                }
                return false;
            }

            private bool ExtendGangway() {
                // initially set hinge 2 to start extending, then once it gets greater than -50 degrees (so that it clears the ground), begin moving hinge 1
                GangwayHinge1.TargetVelocityRPM = 0;
                GangwayHinge2.TargetVelocityRPM = HINGE_TARGET_VELOCITY_RPM * 2;
                if (CBT.RadToDeg(GangwayHinge2.Angle) > -50) { GangwayHinge1.TargetVelocityRPM = HINGE_TARGET_VELOCITY_RPM; }
                if (Math.Abs(GangwayHinge1.Angle) < HINGE_ANGLE_TOLERANCE && Math.Abs(GangwayHinge2.Angle - (Math.PI / 2)) < HINGE_ANGLE_TOLERANCE) // if hinge1 close enough to 0, and hinge2 close enough to +90, call it extended.
                {
                    CBT.AddToLogQueue("Gangway Extended.", STULogType.OK);
                    return true;
                }
                return false;
            }

            private bool RetractGangway() {
                GangwayHinge1.TargetVelocityRPM = -HINGE_TARGET_VELOCITY_RPM;
                GangwayHinge2.TargetVelocityRPM = -HINGE_TARGET_VELOCITY_RPM * 2;
                if (GangwayHinge1.Angle < -1.56 && GangwayHinge2.Angle < -1.52) // are both hinges close enough to -90, expressed in radians?
                {
                    CBT.AddToLogQueue("Gangway Retracted.", STULogType.OK);
                    return true;
                } else { return false; }
            }

            public void ResetGangway()
            {
                CurrentGangwayState = GangwayStates.Resetting;
            }

            public bool ToggleGangway(float desiredState = float.NaN) {
                if (CurrentGangwayState == GangwayStates.Unknown) {
                    CBT.AddToLogQueue($"Gangway state unknown; to automatically reset, enter 'gangwayreset' in the prompt.", STULogType.WARNING);
                    return false;
                }

                if (desiredState == 1 && CanGoToRequestedState(GangwayStates.Extending)) {
                    CurrentGangwayState = GangwayStates.Extending;
                } else if (desiredState == 0 && CanGoToRequestedState(GangwayStates.Retracting)) {
                    CurrentGangwayState = GangwayStates.Retracting;
                } else {
                    if (CurrentGangwayState == GangwayStates.Retracted || CurrentGangwayState == GangwayStates.Retracting)
                        CurrentGangwayState = GangwayStates.Extending;
                    else if (CurrentGangwayState == GangwayStates.Extended || CurrentGangwayState == GangwayStates.Extending)
                        CurrentGangwayState = GangwayStates.Retracting;
                }
                return true;
            }
        }
    }
}
