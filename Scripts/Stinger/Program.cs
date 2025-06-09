using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    public partial class Program : MyGridProgram
    {
        Stinger StingerShip { get; set; }
        STUMasterLogBroadcaster Broadcaster { get; set; }
        IMyBroadcastListener Listener { get; set; }
        STUInventoryEnumerator InventoryEnumerator { get; set; }
        MyCommandLine CommandLineParser { get; set; } = new MyCommandLine();
        MyCommandLine WirelessMessageParser { get; set; } = new MyCommandLine();
        Queue<STUStateMachine> ManeuverQueue { get; set; } = new Queue<STUStateMachine>();
        STUStateMachine CurrentManeuver { get; set; }
        

        public Program()
        {
            Broadcaster = new STUMasterLogBroadcaster("STR", IGC, TransmissionDistance.AntennaRelay);
            Listener = IGC.RegisterBroadcastListener("STR");
            InventoryEnumerator = new STUInventoryEnumerator(GridTerminalSystem, Me);
            StingerShip = new Stinger(Echo, Broadcaster, InventoryEnumerator, GridTerminalSystem, Me, Runtime);

            

            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            
        }

        public void ResetAutopilot()
        {
            ManeuverQueue.Clear();
            Stinger.CruiseControlActivated = false;
            Stinger.AltitudeControlActivated = false;
            Stinger.ResetUserInputVelocities();
            foreach (var gyro in Stinger.FlightController.AllGyroscopes)
            {
                gyro.Pitch = 0;
                gyro.Yaw = 0;
                gyro.Roll = 0;
            }
            CurrentManeuver = null;
            Stinger.SetAutopilotControl(false, false, true);
            Stinger.CurrentPhase = Stinger.Phase.Idle;
        }

        public void HandleWirelessMessages()
        {

        }
        public bool ParseCommand(string arg)
        {
            // commands MUST follow the structure "[SUBJECT] [PREDICATE]".
            if (CommandLineParser.TryParse(arg))
            {
                if (CommandLineParser.ArgumentCount % 2 != 0 && CommandLineParser.ArgumentCount > 1) // check parity of argument count, but ignore single-word commands.
                {
                    Stinger.AddToLogQueue($"Command string '{arg}' does not have an even number of arguments! Refusing to parse.", STULogType.ERROR);
                    return false;
                }
                for (int i = 0; i < CommandLineParser.ArgumentCount; i = i + 2)
                {
                    string subject = CommandLineParser.Argument(i);
                    subject = subject.ToUpper();
                    if (subject.Length < 2)
                    {
                        CBT.AddToLogQueue($"Command '{subject}' is too short to be valid. Skipping...", STULogType.WARNING);
                        continue;
                    }

                    string predicate;
                    try
                    {
                        predicate = CommandLineParser.Argument(i + 1);
                        predicate = predicate.ToUpper();
                    }
                    catch
                    {
                        CBT.AddToLogQueue($"Could not parse predicate. Defaulting to a blank string.", STULogType.WARNING);
                        predicate = "";
                    }

                    float predicateAsFloat;
                    switch (subject)
                    {
                        case "HELP": // prints a help message to the screen
                            switch (predicate)
                            {
                                case "":
                                    CBT.AddToLogQueue("Enter a page number [1 - 3] to view detailed help. Commands are organized alphabetically.", STULogType.OK);
                                    CBT.AddToLogQueue("CLOSEOUT {NOW} - Immediately goes to the 'closeout' state of the current maneuver.", STULogType.OK);
                                    CBT.AddToLogQueue("HALT {NOW} - Executes a hover maneuver.", STULogType.OK);
                                    CBT.AddToLogQueue("STOP {NOW} - Same as HALT, but changes the ship's orientation before firing thrusters to best counterract the current trajectory.", STULogType.OK);
                                    CBT.AddToLogQueue("AP RESET - Resets the autopilot 'manually' (outisde of the maneuver queue) and clears the maneuver queue.", STULogType.OK);
                                    CBT.AddToLogQueue("TEST - Executes hard-coded maneuver parameters. FOR TESTING PURPOSES ONLY.", STULogType.OK);
                                    break;
                                default:
                                    CBT.AddToLogQueue("Page number out of range (probably not implemented yet).");
                                    break;
                            }
                            break;

                        #region CBT Software Commands
                        case "CLOSEOUT": // instantly moves the current maneuver's state to the Closeout phase, if there is a currently executing maneuver.
                            CBT.AddToLogQueue($"Cancelling maneuver '{CurrentManeuver.Name}'...", STULogType.INFO);
                            if (CurrentManeuver != null)
                            {
                                CurrentManeuver.CurrentInternalState = STUStateMachine.InternalStates.Closeout;
                            }
                            break;

                        case "POSITION": // prints the current world position of the remote control to the terminal
                            CBT.AddToLogQueue($"Current position:", STULogType.INFO);
                            CBT.AddToLogQueue($"X: {CBT.FlightController.CurrentPosition.X}", STULogType.INFO);
                            CBT.AddToLogQueue($"Y: {CBT.FlightController.CurrentPosition.Y}", STULogType.INFO);
                            CBT.AddToLogQueue($"Z: {CBT.FlightController.CurrentPosition.Z}", STULogType.INFO);
                            break;

                        case "CLEAR": // blanks all of the log screens by adding 20 lines of blank strings to the log queue.
                            for (i = 0; i < 20; i++)
                            {
                                CBT.AddToLogQueue("");
                            }
                            break;
                        #endregion

                        #region CBT Hardware Commands
                        case "POWER": // turns on/off various blocks on the grid according to their power level. Each successive level includes all the blocks in lower levels.
                            int powerLevel;
                            if (int.TryParse(predicate, out powerLevel))
                            {
                                if (0 <= powerLevel && powerLevel <= 7)
                                {
                                    CBT.PowerLevel = powerLevel;
                                    break;
                                }
                                else
                                {
                                    CBT.AddToLogQueue($"Power level {powerLevel} is out of range. Must be between 1 and 7 (inclusive).");
                                    break;
                                }
                            }
                            else
                            {
                                CBT.AddToLogQueue($"Failed to parse predicate {predicate} on subject {subject}. Skipping...");
                            }
                            break;
                        case "AIRLOCK":
                            switch (predicate)
                            {
                                case "ENABLE":
                                    CBT.ACM.EnableAutomaticControl();
                                    break;
                                case "DISABLE":
                                    CBT.ACM.DisableAuomaticControl();
                                    break;
                                default:
                                    CBT.AddToLogQueue($"Airlock command '{predicate}' is not valid. Try ENABLE or DISABLE. Skipping...", STULogType.WARNING);
                                    break;
                            }
                            break;

                        case "GEAR":
                            switch (predicate)
                            {
                                case "LOCK":
                                    CBT.SetLandingGear(true);
                                    break;
                                case "UNLOCK":
                                    CBT.SetLandingGear(false);
                                    break;
                                default:
                                    CBT.AddToLogQueue($"{subject} command '{predicate}' is not valid. Try LOCK or UNLOCK. Skipping...", STULogType.WARNING);
                                    break;
                            }
                            break;

                        case "ENGINES":
                            switch (predicate)
                            {
                                case "ON":
                                    foreach (var engine in CBT.HydrogenEngines) { engine.Enabled = true; }
                                    break;
                                case "OFF":
                                    foreach (var engine in CBT.HydrogenEngines) { engine.Enabled = false; }
                                    break;
                                default:
                                    CBT.AddToLogQueue($"{subject} command '{predicate}' is not valid. Try ON or OFF. Skipping...", STULogType.WARNING);
                                    break;
                            }
                            break;
                        #endregion

                        #region Actuator Groups
                        case "GANGWAY":
                            switch (predicate)
                            {
                                case "EXTEND":
                                    CBT.Gangway.ToggleGangway(1);
                                    break;
                                case "RETRACT":
                                    CBT.Gangway.ToggleGangway(0);
                                    break;
                                case "TOGGLE":
                                    CBT.Gangway.ToggleGangway();
                                    break;
                                case "RESET":
                                    CBT.UserInputGangwayState = CBTGangway.GangwayStates.Resetting;
                                    break;
                                default:
                                    CBT.AddToLogQueue($"Gangway command '{predicate}' is not valid. Try EXTEND, RETRACT, TOGGLE, or RESET. Skipping...", STULogType.WARNING);
                                    break;
                            }
                            break;
                        case "STINGER":
                            switch (predicate)
                            {
                                case "RESET":
                                    CBT.UserInputRearDockPosition = 1;
                                    break;
                                case "STOW":
                                    CBT.UserInputRearDockPosition = 0;
                                    break;
                                case "LHQ":
                                    CBT.UserInputRearDockPosition = 2;
                                    break;
                                case "HEROBRINE":
                                    CBT.UserInputRearDockPosition = 3;
                                    break;
                                case "CLAM":
                                    CBT.UserInputRearDockPosition = 4;
                                    break;
                                default:
                                    CBT.AddToLogQueue($"Stinger command '{predicate}' is not valid. Try RESET, STOW, LHQ, HEROBRINE, or CLAM. Skipping...", STULogType.WARNING);
                                    break;
                            }
                            break;
                        #endregion

                        #region Simple Locomotion
                        case "FORWARD":
                            if (float.TryParse(predicate, out predicateAsFloat))
                            {
                                CBT.UserInputForwardVelocity = predicateAsFloat;
                                ManeuverQueue.Enqueue(new CBT.GenericManeuver(
                                    CBT.UserInputForwardVelocity,
                                    CBT.UserInputRightVelocity,
                                    CBT.UserInputUpVelocity,
                                    CBT.UserInputRollVelocity,
                                    CBT.UserInputPitchVelocity,
                                    CBT.UserInputYawVelocity));
                            }
                            else
                            {
                                CBT.AddToLogQueue($"Failed to parse predicate {predicate} on subject {subject}. Skipping...");
                            }
                            break;

                        case "BACKWARD":
                            if (float.TryParse(predicate, out predicateAsFloat))
                            {
                                CBT.UserInputForwardVelocity = Math.Abs(predicateAsFloat) * -1;
                                ManeuverQueue.Enqueue(new CBT.GenericManeuver(
                                    CBT.UserInputForwardVelocity,
                                    CBT.UserInputRightVelocity,
                                    CBT.UserInputUpVelocity,
                                    CBT.UserInputRollVelocity,
                                    CBT.UserInputPitchVelocity,
                                    CBT.UserInputYawVelocity));
                            }
                            else
                            {
                                CBT.AddToLogQueue($"Failed to parse predicate {predicate} on subject {subject}. Skipping...");
                            }
                            break;

                        case "UP":
                            if (float.TryParse(predicate, out predicateAsFloat))
                            {
                                CBT.UserInputUpVelocity = predicateAsFloat;
                                ManeuverQueue.Enqueue(new CBT.GenericManeuver(
                                    CBT.UserInputForwardVelocity,
                                    CBT.UserInputRightVelocity,
                                    CBT.UserInputUpVelocity,
                                    CBT.UserInputRollVelocity,
                                    CBT.UserInputPitchVelocity,
                                    CBT.UserInputYawVelocity));
                            }
                            else
                            {
                                CBT.AddToLogQueue($"Failed to parse predicate {predicate} on subject {subject}. Skipping...");
                            }
                            break;

                        case "DOWN":
                            if (float.TryParse(predicate, out predicateAsFloat))
                            {
                                CBT.UserInputUpVelocity = (predicateAsFloat) * -1;
                                ManeuverQueue.Enqueue(new CBT.GenericManeuver(
                                    CBT.UserInputForwardVelocity,
                                    CBT.UserInputRightVelocity,
                                    CBT.UserInputUpVelocity,
                                    CBT.UserInputRollVelocity,
                                    CBT.UserInputPitchVelocity,
                                    CBT.UserInputYawVelocity));
                            }
                            else
                            {
                                CBT.AddToLogQueue($"Failed to parse predicate {predicate} on subject {subject}. Skipping...");
                            }
                            break;

                        case "RIGHT":
                            if (float.TryParse(predicate, out predicateAsFloat))
                            {
                                CBT.UserInputRightVelocity = predicateAsFloat;
                                ManeuverQueue.Enqueue(new CBT.GenericManeuver(
                                    CBT.UserInputForwardVelocity,
                                    CBT.UserInputRightVelocity,
                                    CBT.UserInputUpVelocity,
                                    CBT.UserInputRollVelocity,
                                    CBT.UserInputPitchVelocity,
                                    CBT.UserInputYawVelocity));
                            }
                            else
                            {
                                CBT.AddToLogQueue($"Failed to parse predicate {predicate} on subject {subject}. Skipping...");
                            }
                            break;

                        case "LEFT":
                            if (float.TryParse(predicate, out predicateAsFloat))
                            {
                                CBT.UserInputRightVelocity = (predicateAsFloat) * -1;
                                ManeuverQueue.Enqueue(new CBT.GenericManeuver(
                                    CBT.UserInputForwardVelocity,
                                    CBT.UserInputRightVelocity,
                                    CBT.UserInputUpVelocity,
                                    CBT.UserInputRollVelocity,
                                    CBT.UserInputPitchVelocity,
                                    CBT.UserInputYawVelocity));
                            }
                            else
                            {
                                CBT.AddToLogQueue($"Failed to parse predicate {predicate} on subject {subject}. Skipping...");
                            }
                            break;

                        case "PITCH":
                            if (float.TryParse(predicate, out predicateAsFloat))
                            {
                                if (-1 <= predicateAsFloat && predicateAsFloat <= 1)
                                {
                                    CBT.UserInputPitchVelocity = predicateAsFloat * 3.14f;
                                    ManeuverQueue.Enqueue(new CBT.GenericManeuver(
                                        CBT.UserInputForwardVelocity,
                                        CBT.UserInputRightVelocity,
                                        CBT.UserInputUpVelocity,
                                        CBT.UserInputRollVelocity,
                                        CBT.UserInputPitchVelocity,
                                        CBT.UserInputYawVelocity));
                                }
                                else { CBT.AddToLogQueue($"Pitch value '{predicateAsFloat}' is out of range. Must be between -1 and +1. Skipping...", STULogType.WARNING); }
                            }
                            else
                            {
                                CBT.AddToLogQueue($"Failed to parse predicate {predicate} on subject {subject}. Skipping...");
                            }
                            break;

                        case "ROLL":
                            if (float.TryParse(predicate, out predicateAsFloat))
                            {
                                if (-1 <= predicateAsFloat && predicateAsFloat <= 1)
                                {
                                    CBT.UserInputRollVelocity = predicateAsFloat * 3.14f;
                                    ManeuverQueue.Enqueue(new CBT.GenericManeuver(
                                        CBT.UserInputForwardVelocity,
                                        CBT.UserInputRightVelocity,
                                        CBT.UserInputUpVelocity,
                                        CBT.UserInputRollVelocity,
                                        CBT.UserInputPitchVelocity,
                                        CBT.UserInputYawVelocity));
                                }
                                else { CBT.AddToLogQueue($"Roll value '{predicateAsFloat}' is out of range. Must be between -1 and +1. Skipping...", STULogType.WARNING); }
                            }
                            else
                            {
                                CBT.AddToLogQueue($"Failed to parse predicate {predicate} on subject {subject}. Skipping...");
                            }
                            break;

                        case "YAW":
                            if (float.TryParse(predicate, out predicateAsFloat))
                            {
                                if (-1 <= predicateAsFloat && predicateAsFloat <= 1)
                                {
                                    CBT.UserInputYawVelocity = predicateAsFloat * 3.14f;
                                    ManeuverQueue.Enqueue(new CBT.GenericManeuver(
                                        CBT.UserInputForwardVelocity,
                                        CBT.UserInputRightVelocity,
                                        CBT.UserInputUpVelocity,
                                        CBT.UserInputRollVelocity,
                                        CBT.UserInputPitchVelocity,
                                        CBT.UserInputYawVelocity));
                                }
                                else { CBT.AddToLogQueue($"Yaw value '{predicateAsFloat}' is out of range. Must be between -1 and +1. Skipping...", STULogType.WARNING); }
                            }
                            else
                            {
                                CBT.AddToLogQueue($"Failed to parse predicate {predicate} on subject {subject}. Skipping...");
                            }
                            break;
                        #endregion

                        #region Complex Locomotion
                        case "AP":
                            switch (predicate)
                            {
                                case "RESET":
                                    CBT.AddToLogQueue("Resetting autopilot...", STULogType.INFO);
                                    ResetAutopilot();
                                    break;
                                case "THRUSTERS":
                                    CBT.AddToLogQueue($"Toggling thruster control {BoolConverter(!CBT.FlightController.HasThrusterControl)}");
                                    CBT.SetAutopilotControl(!CBT.FlightController.HasThrusterControl, CBT.FlightController.HasGyroControl, CBT.RemoteControl.DampenersOverride);
                                    break;
                                case "GYROS":
                                    CBT.AddToLogQueue($"Toggling gyro control {BoolConverter(!CBT.FlightController.HasGyroControl)}");
                                    CBT.SetAutopilotControl(CBT.FlightController.HasThrusterControl, !CBT.FlightController.HasGyroControl, CBT.RemoteControl.DampenersOverride);
                                    break;
                                case "DAMPENERS":
                                    CBT.AddToLogQueue($"Toggling dampener control {BoolConverter(!CBT.RemoteControl.DampenersOverride)}");
                                    CBT.SetAutopilotControl(CBT.FlightController.HasThrusterControl, CBT.FlightController.HasGyroControl, !CBT.RemoteControl.DampenersOverride);
                                    break;
                                default:
                                    CBT.AddToLogQueue($"Autopilot command '{predicate}' is not valid. Try RESET or DISABLE. Skipping...", STULogType.WARNING);
                                    break;
                            }
                            break;
                        case "ALTITUDE":
                            if (float.TryParse(predicate, out predicateAsFloat))
                            {
                                if (predicateAsFloat >= 50)
                                {
                                    CBT.AddToLogQueue($"Setting cruising altitude to {predicateAsFloat}m", STULogType.INFO);
                                    CBT.SetCruisingAltitude(predicateAsFloat);
                                }
                                else { CBT.AddToLogQueue($"Cruising altitude cannot be set lower than 50 for safety. Skipping...", STULogType.WARNING); }
                            }
                            else if (predicate == "CANCEL")
                            {
                                CBT.AltitudeControlActivated = false;
                            }
                            else
                            {
                                CBT.AddToLogQueue($"Failed to parse predicate {predicate} on subject {subject}. Skipping...");
                            }
                            break;

                        case "CRUISE":
                            if (float.TryParse(predicate, out predicateAsFloat))
                            {
                                if (predicateAsFloat > 0)
                                {
                                    CBT.AddToLogQueue($"Setting cruising speed to {predicateAsFloat}m/s", STULogType.INFO);
                                    CBT.CruiseControlActivated = true;
                                    CBT.CruiseControlSpeed = predicateAsFloat;
                                }
                                else { CBT.AddToLogQueue("Cruising speed must be a positive number. Skipping...", STULogType.WARNING); }
                            }
                            else if (predicate == "CANCEL")
                            {
                                CBT.CruiseControlActivated = false;
                            }
                            else
                            {
                                CBT.AddToLogQueue($"Failed to parse predicate {predicate} on subject {subject}. Skipping...");
                            }
                            break;

                        case "HOVER": // queues a hover maneuver
                            CBT.AddToLogQueue("Queueing a Hover maneuver", STULogType.INFO);
                            CBT.CruiseControlActivated = false;
                            CBT.AltitudeControlActivated = false;
                            ManeuverQueue.Enqueue(new CBT.HoverManeuver());
                            break;

                        case "FASTSTOP": // queues a fast stop maneuver
                            CBT.AddToLogQueue("Queueing a fast stop maneuver", STULogType.INFO);
                            CBT.CruiseControlActivated = false;
                            CBT.AltitudeControlActivated = false;
                            ManeuverQueue.Enqueue(new STUFlightController.HardStop(CBT.FlightController));
                            break;
                        #endregion

                        #region Networking
                        case "PING": // predicate is ignored, but generally must still have a value. Otherwise, it would be caught at the top of this method.
                            CBT.AddToLogQueue("Broadcasting PING", STULogType.INFO);
                            CBT.CreateBroadcast("PING", false, STULogType.INFO);
                            break;
                        case "DOCK":
                            switch (predicate)
                            {
                                case "REQUEST":
                                    CBT.AddToLogQueue("Sending dock request to Hyperdrive Ring...", STULogType.INFO);
                                    CBT.DockingModule.SendDockRequestFlag = true;
                                    break;
                                case "CONTINUE":
                                    CBT.DockingModule.PilotConfirmation = true;
                                    break;
                                case "CANCEL":
                                    if (CBT.DockingModule.CurrentDockingModuleState == CBTDockingModule.DockingModuleStates.ConfirmWithPilot)
                                    {
                                        CBT.AddToLogQueue("Docking sequence cancelled. Returning docking module state to idle...", STULogType.WARNING);
                                        CBT.CreateBroadcast("CANCEL");
                                        CBT.DockingModule.CurrentDockingModuleState = CBTDockingModule.DockingModuleStates.Idle;
                                    }
                                    else
                                    {
                                        CBT.AddToLogQueue("Didn't find anything to cancel. Skipping...", STULogType.WARNING);
                                    }
                                    break;
                                default:
                                    CBT.AddToLogQueue($"Docking command '{predicate}' is not valid. Try REQUEST, CONFIRM or CANCEL. Skipping...", STULogType.WARNING);
                                    break;
                            }
                            break;
                        #endregion

                        default:
                            CBT.AddToLogQueue($"Unrecognized subject '{subject}'. Skipping...", STULogType.WARNING);
                            break;
                    }
                }
                return true;
            }
            else
            {
                CBT.AddToLogQueue($"damn this shit broken fr fr", STULogType.ERROR);
                return false;
            }
        }

        public string BoolConverter(bool value)
        {
            if (value) { return "ON"; }
            else { return "OFF"; }
        }
    }
}
