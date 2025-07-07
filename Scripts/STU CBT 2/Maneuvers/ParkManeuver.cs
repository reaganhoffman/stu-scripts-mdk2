using Sandbox.Game.Replication;
using System;
using System.Collections.Generic;
using VRageMath;

namespace IngameScript {
    partial class Program {
        public partial class CBT {
            public class ParkManeuver : STUStateMachine {
                public override string Name => "Landing";
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
                private bool AskedForConfirmationAlready = false;
                private bool _pilotConfirmation = false;
                public bool PilotConfirmation 
                {
                    get { return _pilotConfirmation; }
                    set
                    {
                        if (value == true)
                        {
                            if (Landed || CurrentInternalState == InternalStates.Init)
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
                    if (!AskedForConfirmationAlready) AddToLogQueue("Enter 'CONFIRM' to proceed with landing sequence.", STULogType.WARNING);
                    AskedForConfirmationAlready = true;
                    if (Math.Abs(FlightController.VelocityMagnitude) < 0.1 && PilotConfirmation)
                    {
                        // ensure we have access to the thrusters, gyros, and dampeners are on
                        SetAutopilotControl(true, true, true);
                        ResetUserInputVelocities();
                        CancelCruiseControl();
                        LevelToHorizon();
                        foreach (var spotlight in Spotlights)
                        {
                            spotlight.Enabled = true;
                        }
                        if (!CBTGangway.ToggleGangway(1))
                        {
                            UserInputGangwayState = CBTGangway.GangwayStates.Resetting;
                        }
                        PilotConfirmation = false;
                        return true;
                    }
                    else return false;
                }

                public override bool Run() {
                    switch (InternalState)
                    {
                        case LandingPhases.InitialDescent:
                            double descendVelocity = Math.Max(FlightController.GetCurrentSurfaceAltitude() / 10, 4);
                            
                            if (FlightController.MaintainSurfaceAltitude(30, 10, descendVelocity) && CBTGangway.CurrentGangwayState == CBTGangway.GangwayStates.Extended) { InternalState = LandingPhases.FinalApproach; }
                            break;
                        case LandingPhases.FinalApproach:
                            CancelAttitudeControl();
                            foreach (var light in InteriorLights) { light.Enabled = true; }
                            if (FlightController.MaintainSurfaceAltitude(1, 1, 1) || FlightController.VelocityMagnitude <= 0.1) { InternalState = LandingPhases.Touchdown; }
                            break;
                        case LandingPhases.Touchdown:
                            FlightController.MaintainSurfaceAltitude(1, 1, 1);
                            if (FlightController.VelocityMagnitude < 0.1)
                            {
                                Landed = true;
                                AddToLogQueue("Landing sequence complete. Ready to park.", STULogType.OK);
                                return true;
                            }
                            break;
                    }
                    return false;
                }

                public override bool Closeout() {
                    if (PilotConfirmation)
                    {
                        SetLandingGear(true);
                        CancelAttitudeControl();
                        SetAutopilotControl(false, false, true);
                        foreach (var thruster in Thrusters) { thruster.Enabled = false; }
                        foreach (var spotlight in Spotlights) { spotlight.Enabled = false; }
                        return true;
                        
                    }
                    return false;
                    
                }
            }
        }
    }
}
