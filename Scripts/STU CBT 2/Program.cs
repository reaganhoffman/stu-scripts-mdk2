using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;
using VRageRender.Messages;

namespace IngameScript {
    partial class Program : MyGridProgram {
        MyIni _ini = new MyIni();
        CBT CBTShip { get; set; }
        STUMasterLogBroadcaster Broadcaster { get; set; }
        IMyBroadcastListener Listener { get; set; }
        STUInventoryEnumerator InventoryEnumerator { get; set; }
        List<IMyTerminalBlock> AllTerminalBlocks { get; set; } = new List<IMyTerminalBlock>();
        List<IMyGasTank> AllTanks { get; set; } = new List<IMyGasTank>();
        List<IMyBatteryBlock> AllBatteries { get; set; } = new List<IMyBatteryBlock>();
        MyCommandLine CommandLineParser { get; set; } = new MyCommandLine();
        MyCommandLine WirelessMessageParser { get; set; } = new MyCommandLine();
        Queue<STUStateMachine> ManeuverQueue { get; set; } = new Queue<STUStateMachine>();
        static STUStateMachine CurrentManeuver { get; set; }
        public struct ManeuverQueueData {
            public string CurrentManeuverName;
            public bool CurrentManeuverInitStatus;
            public bool CurrentManeuverRunStatus;
            public bool CurrentManeuverCloseoutStatus;
            public string FirstManeuverName;
            public string SecondManeuverName;
            public string ThirdManeuverName;
            public string FourthManeuverName;
            public bool Continuation;
        }

        public Program() {
            // read from Storage
            _ini.TryParse(Storage);
            PowerControlModule.PowerGroupsSaveState.Clear();
            foreach (var group in PowerControlModule.PowerGroups)
            {
                PowerControlModule.PowerGroupsSaveState.Add(new PowerControlModule.PowerGroup { Name = group.Name, Blocks = group.Blocks, Enabled = _ini.Get("POWER", group.Name).ToBoolean() } );
            }
            PowerControlModule.RestoreFromSaveState();

            
            Broadcaster = new STUMasterLogBroadcaster(CBT_VARIABLES.CBT_BROADCAST_CHANNEL, IGC, TransmissionDistance.AntennaRelay);
            Listener = IGC.RegisterBroadcastListener(CBT_VARIABLES.CBT_BROADCAST_CHANNEL);
            GridTerminalSystem.GetBlocks(AllTerminalBlocks);
            GridTerminalSystem.GetBlocksOfType<IMyGasTank>(AllTanks);
            GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(AllBatteries);
            InventoryEnumerator = new STUInventoryEnumerator(GridTerminalSystem, Me);
            CBTShip = new CBT(ManeuverQueue, Echo, InventoryEnumerator, Broadcaster, GridTerminalSystem, Me, Runtime);
            CBT.SetAutopilotControl(true, true, false);

            CBT.ResetAutopilot();

            // at compile time, Runtime.UpdateFrequency needs to be set to update every 10 ticks. 
            // I'm pretty sure the user input buffer is empty as far as the program is concerned whenever you hit recompile, even if there is text in the box.
            // i.e. it's only when you hit "run" does the program pull whatever is in the user input buffer and run it.
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Save()
        {
            _ini.Clear();

            foreach (var group in PowerControlModule.PowerGroups)
            {
                _ini.Set("POWER", group.Name, group.Enabled);
            }
            
            Storage = _ini.ToString();
        }

        /// <summary>
        /// This is the program's main method. It gets called by Space Engineers, it runs, then MUST return within 1/6 of a second
        /// (the amound of time Space Engineers allots for scripts to run).
        /// The paradigm of this script revolves around this constraint, and makes heavy use of state machines and enumerators and the like.
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="updateSource"></param>
        public void Main(string argument, UpdateType updateSource) {
            try {
                try
                {
                    InventoryEnumerator.EnumerateInventories();
                }
                catch (Exception ex)
                {
                    Echo("Could not instantiate inventory enumerator");
                    CBT.AddToLogQueue("Something is wrong with the inventory enumerator...", STULogType.WARNING);
                    CBT.AddToLogQueue($"{ex.Message}");
                }

                HandleWirelessMessages();

                argument = argument.Trim().ToUpper();

                // parse the passed phrase as "[SUBJECT] [PREDICATE]"
                if (argument != "") {
                    if (!ParseCommand(argument)) {
                        CBT.AddToLogQueue($"cbt machine broke", STULogType.ERROR);
                    }
                }

                /// main state machine:
                /// Idle phase: look for work, so to speak. If there's something in the queue, pull it out and start executing it.
                /// Executing Phase: ultimately defers to the current maneuver's internal state machine.
                ///     if that internal state machine ever gets to the "Done" state, this "main" state machine will return to the Idle phase.
                switch (CBT.CurrentPhase) {
                    case CBT.Phase.Idle:
                        if (ManeuverQueue.Count > 0) {
                            try {
                                CurrentManeuver = ManeuverQueue.Dequeue();
                                CBT.AddToLogQueue($"Executing {CurrentManeuver.Name} maneuver...", STULogType.INFO);
                                CBT.CurrentPhase = CBT.Phase.Executing;
                            } catch {
                                CBT.AddToLogQueue("Could not pull maneuver from queue, despite the queue's count being greater than zero. Something is wrong, halting program...", STULogType.ERROR);
                                Runtime.UpdateFrequency = UpdateFrequency.None;
                            }
                        }
                        break;

                    case CBT.Phase.Executing:
                        if (CurrentManeuver.ExecuteStateMachine()) {
                            CurrentManeuver = null;
                            CBT.CurrentPhase = CBT.Phase.Idle;
                        }
                        break;
                }

                // update various subsystems that are independent of the maneuver queue
                CBT.FlightController.UpdateState();
                if (CBT.CruiseControlActivated) { CBT.SetCruiseControl(CBT.CruiseControlSpeed); } // set cruise control only if cruise control is activated.
                if (CBT.AttitudeControlActivated) { CBT.LevelToHorizon(); } // attempt to level with the horizon only if attitude control is activated.
                if (CBT.HeadingControlActivated) { CBT.SetHeadingControl(); } // continuously call SetHeadingControl if heading control is activated.
                CBT.Gangway.UpdateGangway(CBT.UserInputGangwayState);
                CBT.UpdateAutopilotScreens();
                CBT.UpdateLogScreens();
                CBT.UpdateManeuverQueueScreens(GatherManeuverQueueData());
                CBT.UpdateAmmoScreens();
                CBT.UpdateStatusScreens();
                CBT.UpdateBottomCameraScreens();
                CBT.UpdateConfirmationTerminals(GatherManeuverQueueData());
                CBT.ACM.UpdateAirlocks();
                CBT.DockingModule.UpdateDockingModule();
                if (CBT.DockingModule.CurrentDockingModuleState == CBTDockingModule.DockingModuleStates.QueueManeuvers) {
                    try {
                        // set up auxiliary hardware
                        CBT.Gangway.ToggleGangway(1);
                        CBT.MergeBlock.Enabled = true;
                        // go to point in space behind the CR
                        ManeuverQueue.Enqueue(new STUFlightController.GotoAndStop(CBT.FlightController, CBT.DockingModule.LineUpPosition, 10, CBT.MergeBlock));
                        // align ship to point at CR docking position
                        ManeuverQueue.Enqueue(new STUFlightController.PointAtTarget(CBT.FlightController, CBT.DockingModule.DockingPosition, CBT.MergeBlock));
                        // ManeuverQueue.Enqueue(new STUFlightController.PointAtTarget(CBT.FlightController, CBT.DockingModule.RollReference, CBT.MergeBlock, "down"));
                        // move again to line up with CR since the merge block is not at the center of mass
                        ManeuverQueue.Enqueue(new STUFlightController.GotoAndStop(CBT.FlightController, CBT.DockingModule.LineUpPosition, 2, CBT.MergeBlock));
                        // align again to point at CR docking position
                        ManeuverQueue.Enqueue(new STUFlightController.PointAtTarget(CBT.FlightController, CBT.DockingModule.DockingPosition, CBT.MergeBlock));
                        // ManeuverQueue.Enqueue(new STUFlightController.PointAtTarget(CBT.FlightController, CBT.DockingModule.RollReference, CBT.MergeBlock, "down"));
                        // move into docking position
                        ManeuverQueue.Enqueue(new STUFlightController.GotoAndStop(CBT.FlightController, CBT.DockingModule.DockingPosition, 7, CBT.MergeBlock));
                        // hover
                        ManeuverQueue.Enqueue(new CBT.HoverManeuver());
                        // ensure that this block is only hit once
                        CBT.DockingModule.CurrentDockingModuleState = CBTDockingModule.DockingModuleStates.Docking;
                    } catch (Exception e) {
                        CBT.AddToLogQueue($"Docking sequence failed: {e.Message}", STULogType.ERROR);
                    }
                }

                //Echo($"Physical Mass: {CBT.FlightController.RemoteControl.CalculateShipMass().PhysicalMass}\n" +
                //    $"Total Mass: {CBT.FlightController.RemoteControl.CalculateShipMass().TotalMass}\n" +
                //    $"Center of Mass: \n{CBT.RemoteControl.CenterOfMass}");

                // use this line to print random shit to the screen during testing
                // CBT.AddToLogQueue($"dot(CVWF, Gravity): {Vector3D.Dot(CBT.FlightController.CurrentVelocity_WorldFrame, CBT.FlightController.ShipController.GetNaturalGravity())}");


            } catch (Exception e) {
                Echo($"Program.cs: Caught exception: {e}");
                CBT.AddToLogQueue($"Program.cs: Caught exception: {e}", STULogType.WARNING);
                CBT.AddToLogQueue("");
                CBT.AddToLogQueue("");
                CBT.AddToLogQueue("HALTING PROGRAM EXECUTION!", STULogType.ERROR);
                CBT.UpdateLogScreens();
                Runtime.UpdateFrequency = UpdateFrequency.None;
            }
        }

        public ManeuverQueueData GatherManeuverQueueData() {
            ManeuverQueueData data = new ManeuverQueueData();
            data.CurrentManeuverName = CurrentManeuver?.Name;
            data.CurrentManeuverInitStatus = CurrentManeuver?.CurrentInternalState == STUStateMachine.InternalStates.Init;
            data.CurrentManeuverRunStatus = CurrentManeuver?.CurrentInternalState == STUStateMachine.InternalStates.Run;
            data.CurrentManeuverCloseoutStatus = CurrentManeuver?.CurrentInternalState == STUStateMachine.InternalStates.Closeout;
            data.FirstManeuverName = ManeuverQueue.Count > 0 ? ManeuverQueue.ElementAt(0).Name : null;
            data.SecondManeuverName = ManeuverQueue.Count > 1 ? ManeuverQueue.ElementAt(1).Name : null;
            data.ThirdManeuverName = ManeuverQueue.Count > 2 ? ManeuverQueue.ElementAt(2).Name : null;
            data.FourthManeuverName = ManeuverQueue.Count > 3 ? ManeuverQueue.ElementAt(3).Name : null;
            data.Continuation = ManeuverQueue.Count > 4;
            return data;
        }

        //public void ResetAutopilot() {
        //    ManeuverQueue.Clear();
        //    CBTShip.PushTOLStatusToBottomCameraScreens("");
        //    CBT.CancelAttitudeControl();
        //    CBT.CancelCruiseControl();
        //    CBT.ResetUserInputVelocities();
        //    foreach (var gyro in CBT.FlightController.AllGyroscopes) {
        //        gyro.Pitch = 0;
        //        gyro.Yaw = 0;
        //        gyro.Roll = 0;
        //    }
        //    CurrentManeuver = null;
        //    CBT.SetAutopilotControl(false, false, true);
        //    CBT.CurrentPhase = CBT.Phase.Idle;
        //    CBT.FlightController.UpdateShipMass();
        //    CBT.FlightController.UpdateThrustersAfterGridChange(CBT.Thrusters);
        //}

        public void HandleWirelessMessages() {
            if (Listener.HasPendingMessage) {
                var rawMessage = Listener.AcceptMessage();
                string message = rawMessage.Data.ToString();
                STULog incomingLog = STULog.Deserialize(message);
                // string decryptedMessage = Modem.Decrypt(incomingLog.Message, CBT_VARIABLES.TEA_KEY);

                if (WirelessMessageParser.TryParse(incomingLog.Message.ToUpper())) {
                    switch (WirelessMessageParser.Argument(0)) {
                        case "PING":
                            CBT.CreateBroadcast("PONG");
                            break;
                        case "POSITION":
                            CBT.AddToLogQueue($"Received position message: {incomingLog.Message}", STULogType.INFO);
                            try {
                                double a = double.Parse(WirelessMessageParser.Argument(1).Trim());
                                double b = double.Parse(WirelessMessageParser.Argument(2).Trim());
                                double c = double.Parse(WirelessMessageParser.Argument(3).Trim());
                                double d = double.Parse(WirelessMessageParser.Argument(4).Trim());
                                double e = double.Parse(WirelessMessageParser.Argument(5).Trim());
                                double f = double.Parse(WirelessMessageParser.Argument(6).Trim());
                                double x = double.Parse(WirelessMessageParser.Argument(7).Trim());
                                double y = double.Parse(WirelessMessageParser.Argument(8).Trim());
                                double z = double.Parse(WirelessMessageParser.Argument(9).Trim());
                                CBT.DockingModule.LineUpPosition = new Vector3D(a, b, c);
                                CBT.DockingModule.RollReference = new Vector3D(d, e, f);
                                CBT.DockingModule.DockingPosition = new Vector3D(x, y, z);
                            } catch (Exception e) {
                                CBT.AddToLogQueue($"Failed to parse position message: {e.Message}", STULogType.ERROR);
                                Echo($"Failed to parse position message: {e.Message}");
                            }
                            break;
                        case "READY":
                            CBT.AddToLogQueue("Received READY message from Hyperdrive Ring.", STULogType.INFO);
                            CBT.DockingModule.CRReadyFlag = true;
                            break;
                        default:
                            CBT.AddToLogQueue($"Received message: {incomingLog.Message}", STULogType.INFO);
                            break;
                    }
                }
            }
        }


        public bool ParseCommand(string arg) {
            // commands generally follow the structure "[SUBJECT] [PREDICATE]" OR be special, single-word commands.
            if (CommandLineParser.TryParse(arg)) {
                if (CommandLineParser.ArgumentCount % 2 != 0 && CommandLineParser.ArgumentCount > 1) // check parity of argument count, but ignore single-word commands.
                {
                    CBT.AddToLogQueue($"Command string '{arg}' does not have an even number of arguments! Refusing to parse.", STULogType.ERROR);
                    return false;
                }
                for (int i = 0; i < CommandLineParser.ArgumentCount; i = i + 2) {
                    string subject = CommandLineParser.Argument(i);
                    subject = subject.ToUpper();

                    string predicate;
                    try
                    {
                        predicate = CommandLineParser.Argument(i + 1);
                        predicate = predicate.ToUpper();
                    }
                    catch
                    {
                        predicate = "";
                    }
                        
                    float predicateAsFloat;
                    switch (subject) {
                        case "TEST":
                            CBT.AddToLogQueue($"{CBT.ACM.GetAirlocks()}");
                            break;
                        case "HELP": // prints a help message to the screen
                            switch (predicate)
                            {
                                case "":
                                    CBT.AddToLogQueue("Enter 'HELP [page number]' to view detailed help by category.", STULogType.OK);
                                    CBT.AddToLogQueue("1 - Software Commands.", STULogType.INFO);
                                    CBT.AddToLogQueue("2 - Hardware Blocks", STULogType.INFO);
                                    CBT.AddToLogQueue("3 - Actuator Groups", STULogType.INFO);
                                    CBT.AddToLogQueue("4 - Locomotion", STULogType.INFO);
                                    CBT.AddToLogQueue("5 - Weapons", STULogType.INFO);
                                    CBT.AddToLogQueue("6 - Networking", STULogType.INFO);
                                    break;
                                case "1":
                                    CBT.AddToLogQueue("'CLOSEOUT': instantly moves the current maneuver's state to the Closeout phase, if there is a currently executing maneuver.");
                                    CBT.AddToLogQueue("'POSITION': prints the current world position of the remote control block to the terminal.");
                                    CBT.AddToLogQueue("'CLEAR': crudely clears the terminal by adding 20 lines of blank logs to the log queue.");
                                    break;
                                case "2":
                                    CBT.AddToLogQueue("'POWER [<int>]': sets the power level of the CBT. Refer to CBT manual for description of power levels.");
                                    CBT.AddToLogQueue("'AIRLOCK [ <float> | ENABLE | DISABLE ]': interact with the airlock control module.");
                                    CBT.AddToLogQueue("'GEAR [ LOCK | UNLOCK ]': locks / unlocks the landing gear programmatically.");
                                    CBT.AddToLogQueue("'ENGINES [ ON | OFF ]': turns on / off hydrogen-powered generators programmatically.");
                                    CBT.AddToLogQueue("'DOORS CLOSE': closes all doors on the CBT.");
                                    break;
                                case "3":
                                    CBT.AddToLogQueue("'GANGWAY [ EXTEND | RETRACT | TOGGLE | RESET ]': interact with the gangway.");
                                    CBT.AddToLogQueue("'STINGER [ RESET | STOW | LHQ | HEROBRINE | CLAM ]': interact with the rear connector arm.");
                                    break;
                                case "4":
                                    CBT.AddToLogQueue("'AP [ RESET | THRUSTERS | GYROS | DAMPENERS ]': interact with the autopilot (toggling whether the Flight Controller has control of various flight hardware.");
                                    CBT.AddToLogQueue("'AUTOPILOT [ <float> | CANCEL | SET | INC | DEC ]': interact with the altitude controller.");
                                    CBT.AddToLogQueue("'CRUISE [ <float> | CANCEL | SET | INC | DEC ]': interact with the cruise controller.");
                                    CBT.AddToLogQueue("'HOVER': executes a hover maneuver.");
                                    CBT.AddToLogQueue("'FASTSTOP': executes a fast stop maneuver.");
                                    break;
                                case "5":
                                    break;
                                case "6":
                                default:
                                    CBT.AddToLogQueue("Page number out of range (probably not implemented yet).");
                                    break;
                            }
                            break;

                        #region CBT Software Commands
                        case "CLOSEOUT": // instantly moves the current maneuver's state to the Closeout phase, if there is a currently executing maneuver.
                            CBT.AddToLogQueue($"Cancelling maneuver '{CurrentManeuver.Name}'...", STULogType.INFO);
                            CBT.PushTOLStatusToBottomCameraScreens("");
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

                        case "TOGGLE":
                            if (PowerControlModule.GetPowerGroupByName(predicate).Name != null)
                                PowerControlModule.TogglePowerGroup(PowerControlModule.GetPowerGroupByName(predicate));
                            switch (predicate)
                            {
                                case "AIRLOCK":
                                    CBT.ACM.ChangeAutomaticControl(!CBT.ACM.SoloEnabled, !CBT.ACM.AirlockEnabled);
                                    CBT.AddToLogQueue($"Automatic Doors (solos) {BoolConverter(CBT.ACM.SoloEnabled)}");
                                    CBT.AddToLogQueue($"Automatic Airlocks {BoolConverter(CBT.ACM.AirlockEnabled)}", STULogType.INFO);
                                    break;
                            }
                            break;
                        case "ENABLE":
                            if (PowerControlModule.GetPowerGroupByName(predicate).Name != null)
                                PowerControlModule.EnablePowerGroup(PowerControlModule.GetPowerGroupByName(predicate));
                            switch (predicate)
                            {
                                case "AIRLOCK":
                                    CBT.ACM.ChangeAutomaticControl(true, true);
                                    CBT.ACM.CloseAirlocks();
                                    CBT.ACM.CloseSoloDoors();
                                    CBT.AddToLogQueue("Automatic Airlocks ENABLED", STULogType.OK);
                                    break;
                                case "ENGINES":
                                    foreach (var engine in CBT.HydrogenEngines) { engine.Enabled = true; }
                                    break;
                            }
                            break;
                        case "DISABLE":
                            if (PowerControlModule.GetPowerGroupByName(predicate).Name != null) // if 'predicate' cannot be found, GetPowerClassByName returns a new PowerClass, which has null values
                                PowerControlModule.DisablePowerGroup(PowerControlModule.GetPowerGroupByName(predicate)); 
                            switch (predicate)
                            {
                                case "AIRLOCK":
                                    CBT.ACM.ChangeAutomaticControl(false, false);
                                    CBT.AddToLogQueue("Automatic Airlocks DISABLED", STULogType.WARNING);
                                    break;
                                case "ENGINES":
                                    foreach (var engine in CBT.HydrogenEngines) { engine.Enabled = false; }
                                    break;
                                default:
                                    PrintParseError(subject, predicate);
                                    break;
                            }
                            break;

                        case "GPS":
                            switch (predicate)
                            {
                                case "REFRESH":
                                    CBT.RefreshSavedWaypoints();
                                    break;
                                default:
                                    foreach (var savedWaypoint in CBT.SavedWaypoints)
                                    {
                                        if (savedWaypoint.Name == predicate)
                                            CBT.QueueGotoAndStopManeuver(savedWaypoint.Location);
                                    }
                                    break;
                            }
                            break;

                        case "POWER":
                            switch (predicate)
                            {
                                case "LOW":
                                    PowerControlModule.GoToLowPowerMode();
                                    break;
                                case "RESTORE":
                                    PowerControlModule.RestoreFromSaveState();
                                    break;
                                case "ALL":
                                    foreach (var powerClass in PowerControlModule.PowerGroups)
                                    {
                                        PowerControlModule.EnablePowerGroup(powerClass);
                                    }
                                    break;
                                case "REFRESH":
                                    PowerControlModule.RefreshGroupMembership(CBT.AllFunctionalBlocks.ToList());
                                    break;
                                default: // "POWER L" toggles the state of the Life support power class. "POWER L1" explicitly turns it on, "POWER L0" explicitly off.
                                    bool foundOne = false;
                                    foreach (var powerClass in PowerControlModule.PowerGroups)
                                    {
                                        if (powerClass.Name.Substring(0,1) == predicate.Substring(0,1)) 
                                        {
                                            string modifier = "";
                                            try
                                            {
                                                modifier = predicate.Substring(1, 1);
                                            }
                                            catch
                                            {
                                                modifier = "-";
                                            }
                                            switch (modifier)
                                            {
                                                case "0":
                                                    PowerControlModule.DisablePowerGroup(powerClass);
                                                    break;
                                                case "1":
                                                    PowerControlModule.EnablePowerGroup(powerClass);
                                                    break;
                                                default:
                                                    PowerControlModule.TogglePowerGroup(powerClass);
                                                    break;
                                            }

                                            foundOne = true;
                                        }
                                    }
                                    if (!foundOne) PrintParseError(subject, predicate);
                                    break;
                            }
                            break;

                        // commenting for space saving reasons
                        //case "REPORT":
                        //    switch (predicate)
                        //    {
                        //        case "NEXT":
                        //            CBT.FlushLogChannelMessageBuffer();
                        //            break;
                        //        case "CLEAR":
                        //            CBT.LogChannelMessageBuffer.Clear();
                        //            CBT.AddToLogQueue("Report Message Buffer cleared.", STULogType.INFO);
                        //            break;
                        //        case "POWERLEVELS":
                        //            CBT.PopulatePowerLevelReport();
                        //            CBT.AddToLogQueue("Finished gathering power level data. Run 'REPORT NEXT' to view output.", STULogType.OK);
                        //            break;
                        //        default:
                        //            PrintParseError(subject, predicate);
                        //            break;
                            //}
                            //break;
                        case "FC":
                            switch (predicate)
                            {
                                case "MASS":
                                    CBT.FlightController.UpdateShipMass();
                                    CBT.FlightController.UpdateThrustersAfterGridChange(CBT.Thrusters);
                                    CBT.FlightController.UpdateGyrosAfterGridChange(CBT.Gyros);
                                    break;
                                default:
                                    PrintParseError(subject, predicate);
                                    break;
                            }
                            break;

                        #endregion

                        #region CBT Hardware Commands
                        case "AIRLOCK":
                            if (float.TryParse(predicate, out predicateAsFloat))
                            {
                                CBT.ACM.ChangeDuration(predicateAsFloat);
                                CBT.AddToLogQueue($"Airlock duration: {predicateAsFloat}ms", STULogType.OK);
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
                                    PrintParseError(subject, predicate);
                                    break;
                            }
                            break;

                        case "DOORS":
                            switch (predicate)
                            {
                                case "CLOSE":
                                    CBT.ACM.CloseAirlocks();
                                    CBT.ACM.CloseSoloDoors();
                                    break;
                                case "OPEN":
                                    CBT.ACM.CloseAirlocks();
                                    CBT.ACM.CloseSoloDoors();
                                    break;
                                default:
                                    PrintParseError(subject, predicate);
                                    break;
                            }
                            break;

                        case "ATMO":
                            switch (predicate)
                            {
                                case "HOSPITABLE":
                                    CBT.ACM.ChangeAutomaticControl(false, false);
                                    CBT.AddToLogQueue($"Airlocks open and vents depressurizing.", STULogType.OK);
                                    CBT.ACM.OpenAirlocks(true);
                                    CBT.ACM.OpenSoloDoors(true);
                                    foreach (var vent in CBT.AirVents) { vent.Depressurize = true; }
                                    break;
                                case "INHOSPITABLE":
                                    CBT.ACM.CloseSoloDoors();
                                    CBT.ACM.CloseAirlocks();
                                    CBT.ACM.ChangeAutomaticControl(true, true);
                                    CBT.AddToLogQueue($"Airlocks closed and vents pressurizing.", STULogType.OK);
                                    foreach (var vent in CBT.AirVents) { vent.Depressurize = false; }
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
                                case "RELAX":
                                    CBT.UserInputGangwayState = CBTGangway.GangwayStates.Unknown;
                                    CBTGangway.GangwayHinge1.Torque = 0;
                                    CBTGangway.GangwayHinge2.Torque = 0;
                                    break;
                                default:
                                    PrintParseError(subject, predicate);
                                    break;
                            }
                            break;
                        case "RAMP":
                            switch (predicate)
                            {
                                case "OPEN":
                                    CBT.ManeuverQueue.Enqueue(new CBT.MoveStator(Runtime.UpdateFrequency, ManeuverQueue, CBT.HangarRotor,CBT_VARIABLES.RAMP_HINGE_ANGLE_OPEN, 1));
                                    break;
                                case "CLOSE":
                                    CBT.ManeuverQueue.Enqueue(new CBT.MoveStator(Runtime.UpdateFrequency, ManeuverQueue, CBT.HangarRotor, CBT_VARIABLES.RAMP_HINGE_ANGLE_CLOSED));
                                    break;
                                case "TOGGLE":
                                    if (CBT.RampShouldBeClosed) CBT.ManeuverQueue.Enqueue(new CBT.MoveStator(Runtime.UpdateFrequency, ManeuverQueue, CBT.HangarRotor, CBT_VARIABLES.RAMP_HINGE_ANGLE_OPEN, 1));
                                    else CBT.ManeuverQueue.Enqueue(new CBT.MoveStator(Runtime.UpdateFrequency, ManeuverQueue, CBT.HangarRotor, CBT_VARIABLES.RAMP_HINGE_ANGLE_CLOSED));
                                    break;
                                default:
                                    PrintParseError(subject, predicate);
                                    break;
                            }
                            break;
                        #endregion

                        #region Locomotion
                        case "AP":
                            switch (predicate)
                            {
                                case "RESET":
                                    CBT.AddToLogQueue("Resetting autopilot...", STULogType.INFO);
                                    CBT.PushTOLStatusToBottomCameraScreens("");
                                    CBT.ResetAutopilot();
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
                                    PrintParseError(subject, predicate);
                                    break;
                            }
                            break;
                        case "ATT":
                            switch (predicate)
                            {
                                case "OFF":
                                    CBT.CancelAttitudeControl();
                                    CBT.AddToLogQueue("Attitude Control canceled.", STULogType.WARNING);
                                    break;
                                case "ON":
                                    if (CBT.RemoteControl.GetNaturalGravity() == Vector3D.Zero)
                                    {
                                        CBT.AddToLogQueue("No gravity detected, cannot enable Attitude Control.", STULogType.WARNING);
                                    }
                                    else
                                    {
                                        CBT.LevelToHorizon();
                                        CBT.AddToLogQueue($"Attitude Control enabled.", STULogType.OK);
                                    }
                                    break;
                                case "TOGGLE":
                                    if (CBT.AttitudeControlActivated)
                                    {
                                        CBT.CancelAttitudeControl();
                                        CBT.AddToLogQueue("Attitude Control canceled.", STULogType.WARNING);
                                    }
                                    else
                                    {
                                        if (CBT.RemoteControl.GetNaturalGravity() == Vector3D.Zero)
                                        {
                                            CBT.AddToLogQueue("No gravity detected, cannot enable Attitude Control.", STULogType.WARNING);
                                        }
                                        else
                                        {
                                            CBT.LevelToHorizon();
                                            CBT.AddToLogQueue($"Attitude Control enabled.", STULogType.OK);
                                        }
                                    }
                                    break;
                                default: PrintParseError(subject, predicate); break;
                            }
                            break;
                        case "HEAD":
                            switch (predicate)
                            {
                                case "OFF":
                                    CBT.CancelHeadingControl();
                                    CBT.AddToLogQueue("Heading Control canceled.", STULogType.WARNING);
                                    break;
                                case "ON":
                                    CBT.SetHeadingControl();
                                    CBT.AddToLogQueue("Heading Control enabled.", STULogType.OK);
                                    break;
                                case "TOGGLE":
                                    if (CBT.HeadingControlActivated)
                                    {
                                        CBT.CancelHeadingControl();
                                        CBT.AddToLogQueue("Heading Control canceled.", STULogType.WARNING);
                                    }
                                    else
                                    {
                                        CBT.SetHeadingControl();
                                        CBT.AddToLogQueue("Heading Control enabled.", STULogType.OK);
                                    }
                                    break;
                                default: PrintParseError(subject, predicate); break;
                            }
                            break;

                        case "CRUISE":
                            if (float.TryParse(predicate, out predicateAsFloat))
                            {
                                if (predicateAsFloat > 0 && predicateAsFloat <= 5000)
                                {
                                    CBT.AddToLogQueue($"Setting cruising speed to {predicateAsFloat}m/s", STULogType.INFO);
                                    CBT.SetCruiseControl(predicateAsFloat);
                                }
                                else { CBT.AddToLogQueue("Cruising speed must be between 0 and 5,000. Skipping...", STULogType.ERROR); }
                            }
                            else if (predicate == "CANCEL")
                            {
                                CBT.CancelCruiseControl();
                                CBT.AddToLogQueue("Cruise Control Cancelled.", STULogType.WARNING);
                            }
                            else if (predicate == "SET")
                            {
                                if (CBT.FlightController.VelocityMagnitude < 1 || CBT.FlightController.VelocityMagnitude > 5000) { CBT.AddToLogQueue("Cannot set cruise control at current speed", STULogType.WARNING); break; }
                                CBT.SetCruiseControl((float)CBT.FlightController.VelocityMagnitude);
                                CBT.AddToLogQueue($"Cruise Control: {CBT.CruiseControlSpeed}m/s", STULogType.INFO);
                            }
                            else if (predicate == "INC") // set the cruising speed to the next highest number in the cubic series (f(x)=x^3)
                            {
                                float desiredSpeed = 1;
                                switch (CBT.CruiseControlActivated)
                                {
                                    case true:
                                        if (CBT.CruiseControlSpeed <= 0) { desiredSpeed = NextHighestCubicNumber(0); }
                                        else { desiredSpeed = Math.Min(5000, NextHighestCubicNumber(CBT.CruiseControlSpeed)); }
                                        break;
                                    case false:
                                        if (CBT.FlightController.VelocityMagnitude <= 0) { desiredSpeed = NextHighestCubicNumber(0); }
                                        else { desiredSpeed = Math.Min(5000, NextHighestCubicNumber((float)CBT.FlightController.VelocityMagnitude)); }
                                        break;
                                }
                                CBT.SetCruiseControl(desiredSpeed);
                                if (CBT.CruiseControlSpeed >= 5000) { CBT.AddToLogQueue("Maximum cruise control speed reached (5000)", STULogType.WARNING); }
                                break;
                            }
                            else if (predicate == "DEC") // set the cruising speed to the next lowest number in the cubic series (f(x)=x^3)
                            {
                                float desiredSpeed = 1;
                                switch (CBT.CruiseControlActivated)
                                {
                                    case true:
                                        if (CBT.CruiseControlSpeed < 1) { desiredSpeed = 0; }
                                        else { desiredSpeed = Math.Max(0, NextLowestCubicNumber(CBT.CruiseControlSpeed)); }
                                        break;
                                    case false:
                                        if (CBT.FlightController.VelocityMagnitude < 1) { desiredSpeed = 0; }
                                        else { desiredSpeed = Math.Max(0, NextLowestCubicNumber((float)CBT.FlightController.VelocityMagnitude)); }
                                        break;
                                }
                                CBT.SetCruiseControl(desiredSpeed);
                                if (CBT.CruiseControlSpeed <= 0) { CBT.CancelCruiseControl(); CBT.ResetAutopilot(); CBT.AddToLogQueue("Cruise Control Cancelled (low speed)", STULogType.WARNING); }
                            }
                            else
                            {
                                PrintParseError(subject, predicate);
                            }
                            break;

                        case "LAND":
                            ManeuverQueue.Enqueue(new CBT.ParkManeuver(CBTShip, ManeuverQueue, CBT.Gangway));
                            break;

                        case "CONFIRM":
                            if (CurrentManeuver is CBT.ParkManeuver)
                            {
                                var _currentManeuver = (CBT.ParkManeuver)CurrentManeuver;
                                try
                                {
                                    _currentManeuver.PilotConfirmation = true;
                                }
                                catch (InvalidOperationException ex)
                                {
                                    Echo("Tried to change PilotConfirmation to TRUE when the current maneuver does not contain such a property: " + ex.Message);
                                }
                                break;
                            }
                            else if (CurrentManeuver is CBT.TakeoffManeuver)
                            {
                                var _currentManeuver = (CBT.TakeoffManeuver)CurrentManeuver;
                                try
                                {
                                    _currentManeuver.PilotConfirmation = true;
                                }
                                catch (InvalidOperationException ex)
                                {
                                    Echo("Tried to change PilotConfirmation to TRUE when the current maneuver does not contain such a property: " + ex.Message);
                                }
                                break;
                            }
                            else break;

                        case "TAKEOFF":
                            if (CBT.RemoteControl.GetNaturalGravity() == new Vector3D(0, 0, 0))
                            {
                                CBT.AddToLogQueue("Not in gravity. Aborting takeoff sequence.", STULogType.WARNING);
                                break;
                            }
                            ManeuverQueue.Enqueue(new CBT.TakeoffManeuver(CBTShip, ManeuverQueue, CBT.Gangway));
                            break;

                        case "HOVER": // queues a hover maneuver
                            CBT.AddToLogQueue("Queueing a Hover maneuver", STULogType.INFO);
                            CBT.CancelCruiseControl();
                            CBT.CancelAttitudeControl();
                            ManeuverQueue.Enqueue(new CBT.HoverManeuver());
                            break;

                        case "FASTSTOP": // queues a fast stop maneuver
                            CBT.AddToLogQueue("Queueing a fast stop maneuver", STULogType.INFO);
                            CBT.CancelCruiseControl();
                            CBT.CancelAttitudeControl();
                            ManeuverQueue.Enqueue(new STUFlightController.HardStop(CBT.FlightController));
                            break;
                        #endregion

                        #region Weapons
                        case "GAT":
                            switch (predicate)
                            {
                                case "POWER":
                                    foreach (var gat in CBT.GatlingTurrets) { gat.SetTargetingGroup("BatteryBlock"); }
                                    break;
                                case "RESET":
                                    foreach (var gat in CBT.GatlingTurrets) { gat.ResetTargetingToDefault(); }
                                    break;
                                default: PrintParseError(subject, predicate); break;
                            }
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
                                    PrintParseError(subject, predicate);
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
            } else {
                CBT.AddToLogQueue($"damn this shit broken fr fr", STULogType.ERROR);
                return false;
            }
        }

        public static string BoolConverter(bool value) {
            if (value) { return "ON"; }
            else { return "OFF"; }
        }

        public static float UpdateFrequencyAsFractionOfSecond()
        {
            switch (CBT.Runtime.UpdateFrequency)
            {
                case UpdateFrequency.Update1: return 1 / 60;
                case UpdateFrequency.Update10: return 10 / 60;
                case UpdateFrequency.Update100: return 100 / 60;
                default: return float.NaN;
            }
        }

        public void HelloWorld()
        {
            CBT.AddToLogQueue("hello world");
        }

        public float NextHighestCubicNumber(float value) {
            double index;
            if (value < 0 ) { value = 0; } // avoid attempting to cube a negative number
            index = Math.Ceiling(Math.Pow(value + 0.1, 1.0 / 3.0));
            return (float)Math.Pow(index, 3);
        }

        public float NextLowestCubicNumber(float value)
        {
            double index;
            if (value < 1) { value = 1; } // avoid attempting to cube a negative number
            index = Math.Floor(Math.Pow(value - 0.1, 1.0 / 3.0));
            return (float)Math.Pow(index, 3);
        }

        public void PrintParseError(string subject, string predicate)
        {
            CBT.AddToLogQueue($"Could not parse predicate {predicate} on subject {subject}.", STULogType.WARNING);
        }
    }
}
