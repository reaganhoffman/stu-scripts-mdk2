using Sandbox.Game.Replication;
using System;
using System.Collections.Generic;
using VRageMath;

namespace IngameScript {
    partial class Program {
        public partial class CBT {
            public class ParkManeuver : STUStateMachine {
                public override string Name => "Park";
                Queue<STUStateMachine> ManeuverQueue { get; set; }
                CBTGangway CBTGangway { get; set; }
                public enum LandingPhases
                {
                    InitialDescent,
                    FinalApproach,
                    Touchdown
                }
                public LandingPhases InternalState { get; set; }
                public bool Landed { get; private set; } = false;
                private bool _pilotConfirmation = false;
                public bool PilotConfirmation 
                {
                    get { return _pilotConfirmation; }
                    set
                    {
                        if (value == true)
                        {
                            if (Landed)
                            {
                                _pilotConfirmation = true;
                            }
                            else
                            {
                                AddToLogQueue("Not landed, cannot park yet.", STULogType.WARNING);
                            }
                        }
                        else
                        {
                            _pilotConfirmation = false;
                        }
                    } 
                }
                
                public ParkManeuver(Queue<STUStateMachine> thisManeuverQueue, CBTGangway cBTGangway) {
                    ManeuverQueue = thisManeuverQueue;
                    CBTGangway = cBTGangway;
                }

                public override bool Init() {
                    // ensure we have access to the thrusters, gyros, and dampeners are on
                    SetAutopilotControl(true, true, true);
                    ResetUserInputVelocities();
                    CancelCruiseControl();
                    LevelToHorizon();
                    foreach (var spotlight in CBT.Spotlights)
                    {
                        spotlight.Enabled = true;
                    }
                    if (!CBTGangway.ToggleGangway(1))
                    {
                        CBT.UserInputGangwayState = CBTGangway.GangwayStates.Resetting;
                    }
                    return Math.Abs(FlightController.VelocityMagnitude) < 0.1;
                }

                public override bool Run() {
                    switch (InternalState)
                    {
                        case LandingPhases.InitialDescent:
                            double descendVelocity = Math.Max(CBT.FlightController.GetCurrentSurfaceAltitude() / 10, 4);
                            
                            if (FlightController.MaintainSurfaceAltitude(30, 10, descendVelocity) && CBTGangway.CurrentGangwayState == CBTGangway.GangwayStates.Extended) { InternalState = LandingPhases.FinalApproach; }
                            break;
                        case LandingPhases.FinalApproach:
                            CBT.CancelAttitudeControl();
                            foreach (var light in CBT.InteriorLights) { light.Enabled = true; }
                            if (FlightController.MaintainSurfaceAltitude(1, 1, 1) || FlightController.VelocityMagnitude <= 0.1) { InternalState = LandingPhases.Touchdown; }
                            break;
                        case LandingPhases.Touchdown:
                            Landed = true;
                            AddToLogQueue("Landing sequence touched down. Ready to park.", STULogType.OK);
                            return true;
                    }
                    return false;
                }

                public override bool Closeout() {
                    FlightController.MaintainSurfaceAltitude(1, 1, 1);
                    if (PilotConfirmation)
                    {
                        CBT.SetLandingGear(true);
                        CBT.CancelAttitudeControl();
                        CBT.SetAutopilotControl(false, false, true);
                        foreach (var thruster in CBT.Thrusters) { thruster.Enabled = false; }
                        foreach (var spotlight in CBT.Spotlights) { spotlight.Enabled = false; }
                        return true;
                        
                    }
                    return false;
                    
                }
            }
        }
    }
}
