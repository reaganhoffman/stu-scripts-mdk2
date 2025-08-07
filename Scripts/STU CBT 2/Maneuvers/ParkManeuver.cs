using Sandbox.Game.Replication;
using Sandbox.ModAPI.Ingame;
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
                private bool InitialRaycastState { get; set; }
                public double? AltitudeAboveLZ { get; set; }
                public Vector3D InitialLandingZoneVector { get; set; }
                public MyDetectedEntityInfo Raycast { get; set; }
                public double? LandingZonePlatformElevation { get; set; }
                public bool Landed { get; private set; } = false;
                private bool AskedForConfirmationAlready { get; set; } = false;
                private bool _pilotConfirmation { get; set; } = false;
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
                    InitialRaycastState = Camera.EnableRaycast; // get the initial raycast state, so we can reset the camera to this state whenever we're done taking this raycast.
                    // cameras do consume a bit more power when raycasting is enabled, so this is done to not harm other areas of the code when it comes to power management.
                    Camera.EnableRaycast = true;
                    foreach (var spotlight in DownwardSpotlights) { spotlight.Enabled = true; }
                    foreach (var light in LandingLights) { light.Enabled = true; }
                    
                    if (Math.Abs(FlightController.VelocityMagnitude) < 0.1)
                    {
                        // ensure we have access to the thrusters, gyros, and dampeners are on
                        SetAutopilotControl(true, true, true);
                        ResetUserInputVelocities();
                        CancelCruiseControl();

                        LevelToHorizon();

                        // get elevation offset by pointing camera downward and taking a raycast
                        CameraHinge.TargetVelocityRPM = Math.Abs(CameraHinge.TargetVelocityRPM) * -1; // point camera downwards
                        if (!AskedForConfirmationAlready && AngleCloseEnoughDegrees(CameraHinge.Angle, 0) && CBT.ShipIsLevel && Camera.CanScan(500)) // only do the raycast if the camera is pointed downards and we're level with the horizon
                        {
                            Raycast = Camera.Raycast(500,0,0); // limit detection to 500 meters
                            InitialLandingZoneVector = Camera.GetPosition() - Raycast.HitPosition.Value;

                            AltitudeAboveLZ = InitialLandingZoneVector.Length() + 10; // empirical testing requires this 10m offset be added to get what the in-game HUD (maybe)
                            LandingZonePlatformElevation = FlightController.GetCurrentSurfaceAltitude() - AltitudeAboveLZ;

                            AddToLogQueue($"Current altitude above LZ: {AltitudeAboveLZ}");
                            AddToLogQueue("Enter 'CONFIRM' to proceed with landing sequence.", STULogType.WARNING);
                            AskedForConfirmationAlready = true;
                        }

                        if (PilotConfirmation)
                        {
                            PilotConfirmation = false;
                            CBTGangway.ToggleGangway(1); // extend gangway
                            return true;
                        }
                    }
                    return false;
                }

                public override bool Run() {
                    switch (InternalState)
                    {
                        case LandingPhases.InitialDescent:
                            double descendVelocity = Math.Max((FlightController.GetCurrentSurfaceAltitude() - Math.Max(0,LandingZonePlatformElevation ?? 0 ))/ 10, 4);
                            if (
                                FlightController.MaintainSurfaceAltitude(30 + LandingZonePlatformElevation ?? 0, 10, descendVelocity) && 
                                CBTGangway.CurrentGangwayState == CBTGangway.GangwayStates.Extended
                                ) 
                            { 
                                InternalState = LandingPhases.FinalApproach; 
                            }
                            break;
                        case LandingPhases.FinalApproach:
                            CancelAttitudeControl();
                            foreach (var light in LandingLights) { light.Enabled = true; }
                            if (FlightController.MaintainSurfaceAltitude(1, 1, 1) || FlightController.VelocityMagnitude <= 0.1) 
                            { 
                                InternalState = LandingPhases.Touchdown; 
                            }
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
                    Camera.EnableRaycast = InitialRaycastState; // return the camera's raycast state to whatever it was before

                    if (PilotConfirmation)
                    {
                        SetLandingGear(true);
                        CancelAttitudeControl();
                        SetAutopilotControl(false, false, true);
                        foreach (var thruster in Thrusters) { thruster.Enabled = false; }
                        foreach (var spotlight in DownwardSpotlights) { spotlight.Enabled = false; }
                        foreach (var spotlight in Headlights) { spotlight.Enabled = false; };
                        return true;
                        
                    }
                    return false;
                    
                }
            }
        }
    }
}
