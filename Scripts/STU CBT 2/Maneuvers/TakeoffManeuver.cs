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
                Queue<STUStateMachine> ManeuverQueue { get; set; }
                CBTGangway CBTGangway { get; set; }
                public enum TakeoffPhases
                {
                    HardwareActuatorPrep,
                    AscendToTakeoffHeight,
                    HandoffToPilot
                }
                public TakeoffPhases InternalState { get; set; }
                public bool ReadyForHandoff { get; private set; } = false;
                private bool AskedForConfirmationAlready = false;
                private bool _pilotConfirmation = false;
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

                public TakeoffManeuver(Queue<STUStateMachine> thisManeuverQueue, CBTGangway cBTGangway)
                {
                    ManeuverQueue = thisManeuverQueue;
                    CBTGangway = cBTGangway;
                }

                public override bool Init()
                {
                    foreach (var spotlight in DownwardSpotlights) { spotlight.Enabled = true; }
                    foreach (var light in LandingLights) { light.Enabled = true; }
                    foreach (var spotlight in Headlights) { spotlight.Enabled = true; }
                    if (!AskedForConfirmationAlready) AddToLogQueue("Enter 'CONFIRM' to proceed with takeoff sequence.", STULogType.WARNING);
                    AskedForConfirmationAlready = true;
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
                        Connector.Disconnect(); // disconnect rear connector
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
                            SetLandingGear(false); // disengage landing gear
                            if (!CBTGangway.ToggleGangway(0)) { UserInputGangwayState = CBTGangway.GangwayStates.Resetting; } // retract gangway
                            HangarRotor.TargetVelocityRPM = Math.Abs(HangarRotor.TargetVelocityRPM) * -1; // close hangar ramp, ensuring its velocity is negative
                            UserInputRearDockPosition = 0; // tell the stinger actuator state machine to stow the stinger

                            InternalState = TakeoffPhases.AscendToTakeoffHeight;
                            break;
                        case TakeoffPhases.AscendToTakeoffHeight:
                            double ascendVelocity = Math.Min(10, Math.Max(2,FlightController.GetCurrentSurfaceAltitude()-20));
                            if (FlightController.MaintainSurfaceAltitude(50,ascendVelocity,1)) { InternalState = TakeoffPhases.HandoffToPilot; }
                            break;
                        case TakeoffPhases.HandoffToPilot:
                            LevelToHorizon();
                            Connector.Connect();
                            if (Connector.IsConnected) // now that the stinger should be stowed, connect the connector to its 'locked' position
                            {
                                ReadyForHandoff = true;
                                AddToLogQueue("Takeoff sequence Complete. Ready to fly.", STULogType.OK);
                                return true;
                            }
                            return false;
                            
                    }
                    return false;
                }

                public override bool Closeout()
                {
                    FlightController.MaintainSurfaceAltitude(50, 1, 1);
                    if (PilotConfirmation)
                    {
                        foreach (var spotlight in DownwardSpotlights) { spotlight.Enabled = false; }
                        foreach (var light in LandingLights) { light.Enabled = false; }
                        CBT.CancelAttitudeControl();
                        CBT.SetAutopilotControl(false, false, true);
                        return true;
                    }
                    return false;

                }
            }
        }
    }
}
