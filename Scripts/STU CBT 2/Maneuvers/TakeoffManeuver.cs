using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    partial class Program
    {
        public partial class CBT
        {
            public class TakeoffManeuver : STUStateMachine
            {
                public override string Name => "Takeoff";
                static CBT ThisCBT { get; set; }
                Queue<STUStateMachine> ManeuverQueue { get; set; }
                CBTGangway CBTGangway { get; set; }
                public enum TakeoffPhases
                {
                    HardwareActuatorPrep,
                    AscendToTakeoffHeight,
                    HandoffToPilot
                }
                public TakeoffPhases InternalState { get; set; }
                public double InitialAltitude { get; set; }
                public bool ReadyForHandoff { get; private set; } = false;
                private bool AskedForConfirmationAlready { get; set; } = false;
                private bool _pilotConfirmation { get; set; } = false;
                public bool PilotConfirmation
                {
                    get { return _pilotConfirmation; }
                    set
                    {
                        if (value == true)
                        {
                            if (ReadyForHandoff || CurrentInternalState == InternalStates.Init)
                            {
                                _pilotConfirmation = true;
                            }
                            else
                            {
                                AddToLogQueue("Not done taking off, cannot hand over manual control yet.", STULogType.WARNING);
                            }
                        }
                        else
                        {
                            _pilotConfirmation = false;
                        }
                    }
                }

                public TakeoffManeuver(CBT thisCBT, Queue<STUStateMachine> thisManeuverQueue, CBTGangway cBTGangway)
                {
                    ThisCBT = thisCBT;
                    ManeuverQueue = thisManeuverQueue;
                    CBTGangway = cBTGangway;
                }

                public override bool Init()
                {
                    InitialAltitude = FlightController.GetCurrentSurfaceAltitude();
                    foreach (var spotlight in DownwardSpotlights) { spotlight.Enabled = true; }
                    foreach (var light in LandingLights) { light.Enabled = true; }
                    foreach (var spotlight in Headlights) { spotlight.Enabled = true; }
                    if (!AskedForConfirmationAlready) AddToLogQueue("Enter 'CONFIRM' to proceed with takeoff sequence.", STULogType.WARNING);
                    AskedForConfirmationAlready = true;
                    ThisCBT.PushTOLStatusToBottomCameraScreens("CONFIRM TAKEOFF");
                    if (Math.Abs(FlightController.VelocityMagnitude) < 0.1 && PilotConfirmation) // only continue if we're stationary and pilot confirms takeoff
                    {
                        // ensure we have access to the thrusters, gyros, and dampeners are on
                        SetAutopilotControl(true, true, true);
                        ResetUserInputVelocities();
                        CancelCruiseControl();
                        
                        foreach (var mp in HangarMagPlates) { mp.Enabled = true; } // lock hangar mag plates
                        foreach (var bat in Batteries) { bat.ChargeMode = ChargeMode.Auto; } // set batteries to auto
                        foreach (var tank in HydrogenTanks) { tank.Stockpile = false; } // disable stockpiling for hydro tanks
                        foreach (var thruster in Thrusters) { thruster.Enabled = true; } // turn on thrusters
                        foreach (var gyro in Gyros) { gyro.Enabled = true; } // turn on gyros
                        PilotConfirmation = false;
                        return true;
                    }
                    else return false;
                }

                public override bool Run()
                {
                    switch (InternalState)
                    {
                        case TakeoffPhases.HardwareActuatorPrep:
                            Connector.Disconnect(); // disconnect rear connector
                            Gangway.ResetGangway(); // retract gangway
                            SetLandingGear(false); // disengage landing gear
                            HangarRotor.TargetVelocityRPM = Math.Abs(HangarRotor.TargetVelocityRPM) * -1; // close hangar ramp, ensuring its velocity is negative
                            UserInputRearDockPosition = 0; // tell the stinger actuator state machine to stow the stinger

                            StingerLock.Lock();
                            if (StingerLock.IsLocked) InternalState = TakeoffPhases.AscendToTakeoffHeight; // only proceed once the stinger is locked in place

                            break;
                        case TakeoffPhases.AscendToTakeoffHeight:
                            ThisCBT.PushTOLStatusToBottomCameraScreens("TAKING OFF...");
                            double x = FlightController.GetCurrentSurfaceAltitude() - InitialAltitude;
                            double ascendVelocity = Math.Min(10, Math.Max(2,(Math.Pow(x,2)+100*x)/100));
                            if (FlightController.MaintainSurfaceAltitude(InitialAltitude + 50, ascendVelocity, 1)) { InternalState = TakeoffPhases.HandoffToPilot; }
                            break;
                        case TakeoffPhases.HandoffToPilot:
                            LevelToHorizon();
                            StingerLock.Lock();
                            if (StingerLock.IsLocked) // hand off control once the stinger is actually locked
                            {
                                ReadyForHandoff = true;
                                ThisCBT.PushTOLStatusToBottomCameraScreens("CONFIRM FLIGHT");
                                AddToLogQueue("Takeoff sequence Complete. Ready to fly.", STULogType.OK);
                                return true;
                            }
                            return false;
                            
                    }
                    return false;
                }

                public override bool Closeout()
                {
                    ThisCBT.PushTOLStatusToBottomCameraScreens("");
                    foreach (var spotlight in DownwardSpotlights) { spotlight.Enabled = false; }
                    foreach (var light in LandingLights) { light.Enabled = false; }
                    CBT.CancelAttitudeControl();
                    CBT.SetAutopilotControl(false, false, true);
                    if (ReadyForHandoff) ManeuverQueue.Enqueue(new CBT.HoverManeuver()); // only queue a hover maneuver if we got this far naturally-
                    // in the case that I "cancel" the takeoff maneuver halfway through by running AP RESET, for example, then ReadyForTakeoff would not have been hit
                    return true;
                }
            }
        }
    }
}
