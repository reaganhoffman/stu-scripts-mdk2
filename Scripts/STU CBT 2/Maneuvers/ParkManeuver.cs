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
                static CBT ThisCBT { get; set; }
                Queue<STUStateMachine> ManeuverQueue { get; set; }
                CBTGangway CBTGangway { get; set; }
                public enum LandingPhases
                {
                    InitialDescent,
                    FinalApproach
                }
                public LandingPhases InternalState { get; set; }
                private bool InitialRaycastState { get; set; }
                public double? AltitudeAboveLZ { get; set; }
                public Vector3D LZVector { get; set; }
                Vector3D InitialPositionBeforeDescent { get; set; }
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

                bool ZeroG { get; set; } = false; // assume gravity exists unless proven otherwise
                
                public ParkManeuver(CBT thisCBT, Queue<STUStateMachine> thisManeuverQueue, CBTGangway cBTGangway) {
                    ThisCBT = thisCBT;
                    ManeuverQueue = thisManeuverQueue;
                    CBTGangway = cBTGangway;
                    if (FlightController.ShipController.GetNaturalGravity() == Vector3D.Zero) { ZeroG = true; }
                    CBT.PushTOLStatusToBottomCameraScreens("Acquiring LZ Distance");
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
                        CameraHinge.TargetVelocityRPM = Math.Abs(CameraHinge.TargetVelocityRPM) * -1; // point camera downwards

                        if (!ZeroG)
                        {
                            LevelToHorizon();
                        }
                        if (
                            !AskedForConfirmationAlready
                            && AngleCloseEnoughDegrees(CameraHinge.Angle, 0)
                            && ZeroG ? true : CBT.ShipIsLevel // only check whether the ship is level if we're in gravity
                            && Camera.CanScan(500)) // only do the raycast if the camera is pointed downards and we're level with the horizon
                        {
                            Raycast = Camera.Raycast(500, 0, 0); // limit detection to 500 meters
                            LZVector = Camera.GetPosition() - Raycast.HitPosition.Value;

                            AltitudeAboveLZ = LZVector.Length() + 21.2; // empirical testing -> describes the flight seat's altitude above LZ
                            LandingZonePlatformElevation = FlightController.GetCurrentSurfaceAltitude() - AltitudeAboveLZ;

                            AskForConfirmation();
                        }

                        if (PilotConfirmation)
                        {
                            PilotConfirmation = false;
                            CBTGangway.ToggleGangway(1); // extend gangway
                            InitialPositionBeforeDescent = FlightController.CurrentPosition;
                            return true;
                        }
                    }
                    return false;
                }

                public override bool Run() {
                    switch (InternalState)
                    {
                        case LandingPhases.InitialDescent:
                            CBT.PushTOLStatusToBottomCameraScreens("DESCENDING...");
                            double descendVelocity = Math.Max((FlightController.GetCurrentSurfaceAltitude() - Math.Max(0,LandingZonePlatformElevation ?? 0 ))/ 10, 4);
                            FlightController.SetV_WorldFrame(Base6Directions.Direction.Down, descendVelocity, null, STUFlightController.STUVelocityController.OverrideMode.ADDITIVE);
                            CBT.AddToLogQueue($"FC.SufAlt - LZPlatElev: {FlightController.GetCurrentSurfaceAltitude() - LandingZonePlatformElevation}");
                            if (!ZeroG && FlightController.GetCurrentSurfaceAltitude() - LandingZonePlatformElevation < 21.2 + 15) // I want this to engage when I'm 15 meters from impact (wrt the landing gears), so this factor is necessary based on where the flight seat is, offset from the landing gears 
                            { 
                                InternalState = LandingPhases.FinalApproach; 
                            }
                            else if (ZeroG && (Camera.GetPosition() - Raycast.HitPosition.Value).Length() < 10)
                            {
                                InternalState = LandingPhases.FinalApproach;
                            }
                                break;
                        case LandingPhases.FinalApproach:
                            CBT.PushTOLStatusToBottomCameraScreens("FINAL \nAPPROACH");
                            CBT.CameraHinge.TargetVelocityRad = Math.Abs(CBT.CameraHinge.TargetVelocityRad); // point the camera 'level' with the horizon
                            CancelAttitudeControl();
                            foreach (var light in LandingLights) { light.Enabled = true; }
                            FlightController.SetV_WorldFrame(Base6Directions.Direction.Down, 1, null, STUFlightController.STUVelocityController.OverrideMode.ADDITIVE);
                            if (FlightController.VelocityMagnitude <= 0.1) 
                            {
                                Landed = true;
                                AddToLogQueue("Enter 'CONFIRM' to complete landing sequence.", STULogType.WARNING);
                                return true;
                            }
                            break;
                    }
                    return false;
                }

                public override bool Closeout() {
                    Camera.EnableRaycast = InitialRaycastState; // return the camera's raycast state to whatever it was before
                    CBT.PushTOLStatusToBottomCameraScreens("CONFIRM \nPARK");
                    
                    if (PilotConfirmation)
                    {
                        CBT.PushTOLStatusToBottomCameraScreens("");
                        SetLandingGear(true);
                        CancelAttitudeControl();
                        SetAutopilotControl(false, false, true);
                        foreach (var thruster in Thrusters) { thruster.Enabled = false; }
                        foreach (var spotlight in DownwardSpotlights) { spotlight.Enabled = false; }
                        foreach (var spotlight in Headlights) { spotlight.Enabled = false; }
                        foreach (var landingGear in LandingGear)
                        {
                            if (landingGear.IsLocked) // final check to make sure landing gear actually locked
                            {
                                AddToLogQueue("Landing sequence complete.", STULogType.OK);
                                return true;
                            }
                        }
                        
                        
                    }
                    return false;
                    
                }

                void AskForConfirmation()
                {
                    CBT.PushTOLStatusToBottomCameraScreens("CONFIRM LAND");
                    AddToLogQueue($"Current altitude above LZ: {AltitudeAboveLZ}");
                    AddToLogQueue("Enter 'CONFIRM' to proceed with landing sequence.", STULogType.WARNING);
                    AskedForConfirmationAlready = true;
                }
            }
        }
    }
}
