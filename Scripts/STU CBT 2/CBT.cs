using EmptyKeys.UserInterface.Generated.WorkshopBrowserView_Bindings;
using Sandbox.Game.Screens;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Management.Instrumentation;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.VisualScripting;
using VRage.Game.VisualScripting;
using VRageMath;

namespace IngameScript {
    partial class Program {
        public partial class CBT {

            public static Action<string> echo;

            public static CBT ThisCBT { get; set; }

            public const float TimeStep = 1.0f / 6.0f;
            public static Phase CurrentPhase { get; set; } = Phase.Idle;

            public static int PowerLevel { get; private set; }

            public struct BlockInfo
            {
                public long Id;
                public string Name;
                public int PowerLevel;
            }

            public static List<BlockInfo> CBTBlocks { get; private set; } = new List<BlockInfo>();

            public static float UserInputForwardVelocity { get; set; } = 0;
            public static float UserInputRightVelocity { get; set; } = 0;
            public static float UserInputUpVelocity { get; set; } = 0;
            public static float UserInputRollVelocity { get; set; } = 0;
            public static float UserInputPitchVelocity { get; set; } = 0;
            public static float UserInputYawVelocity { get; set; } = 0;

            public static CBTGangway.GangwayStates UserInputGangwayState { get; set; }

            public bool CanDockWithCR { get; set; } = false;

            public static bool CruiseControlActivated { get; private set; } = false;
            public static float CruiseControlSpeed { get; private set; } = 0f;
            public static bool AttitudeControlActivated { get; private set; } = false;
            public static bool AltitudeControlActivated { get; private set; } = false;
            public static float AltitudeControlHeight { get; private set; } = 0f;
            public static bool ShipIsLevel { get; private set; } = false;
            public static bool ShipIsHovering { get; private set; } = false;

            // prepare the program by declaring all the different blocks we are going to use
            public static IMyGridTerminalSystem CBTGrid { get; set; }
            public static IMyTerminalBlock[] AllTerminalBlocks { get; set; }
            public static IMyFunctionalBlock[] AllFunctionalBlocks { get; set; }
            public static List<CBTLogLCD> LogChannel { get; set; } = new List<CBTLogLCD>();
            public static Queue<string> LogChannelMessageBuffer { get; set; } = new Queue<string>();
            public static List<CBTAutopilotLCD> AutopilotStatusChannel { get; set; } = new List<CBTAutopilotLCD>();
            public static List<CBTManeuverQueueLCD> ManeuverQueueChannel { get; set; } = new List<CBTManeuverQueueLCD>();
            public static Queue<STUStateMachine> ManeuverQueue { get; set; }
            public static List<CBTAmmoLCD> AmmoChannel { get; set; } = new List<CBTAmmoLCD>();
            public static List<CBTStatusLCD> StatusChannel { get; set; } = new List<CBTStatusLCD>();
            public static List<CBTBottomCameraLCD> BottomCameraChannel { get; set; } = new List<CBTBottomCameraLCD>();
            public static List<CBTConfirmationTerminal> ConfirmationTerminalChannel { get; set; } = new List<CBTConfirmationTerminal>();
            public static STUFlightController FlightController { get; set; }
            public static CBTDockingModule DockingModule { get; set; }
            public static AirlockControlModule ACM { get; set; }
            public static PowerControlModule PCM { get; set; }
            public static CBTGangway Gangway { get; set; }
            public static IMyProgrammableBlock Me { get; set; }
            public static STUMasterLogBroadcaster Broadcaster { get; set; }
            public static STUInventoryEnumerator InventoryEnumerator { get; set; }
            #region Hardware
            // power level 0:
            public static IMyBatteryBlock[] Batteries { get; set; } 
            public static IMyButtonPanel[] ButtonPanels { get; set; } 
            public static IMyPowerProducer[] HydrogenEngines { get; set; }
            public static IMyShipMergeBlock MergeBlock { get; set; }
            public static IMyGasTank[] HydrogenTanks { get; set; }
            public static IMyGasTank[] OxygenTanks { get; set; }
            // power level 1:
            public static IMyRemoteControl RemoteControl { get; set; }
            public static IMyThrust[] Thrusters { get; set; }
            public static IMyGyro[] Gyros { get; set; }
            public static IMyTerminalBlock FlightSeat { get; set; }
            public static IMyShipConnector Connector { get; set; }
            public static IMyCryoChamber[] CryoPods { get; set; }
            public static IMyLandingGear[] LandingGear { get; set; }
            public static IMyDoor[] Doors { get; set; }
            public static IMyMotorStator RearHinge1 { get; set; }
            public static IMyMotorStator RearHinge2 { get; set; }
            public static IMyPistonBase RearPiston { get; set; }
            public static IMyMotorStator GangwayHinge1 { get; set; }
            public static IMyMotorStator GangwayHinge2 { get; set; }
            public static IMyMotorStator HangarRotor { get; set; }
            public static IMyMotorStator CameraRotor { get; set; }
            public static IMyMotorStator CameraHinge { get; set; }
            public static IMyCameraBlock Camera { get; set; }
            

            public static IMyGridProgramRuntimeInfo Runtime { get; set; }

            

            public static IMyCargoContainer[] CargoContainers { get; set; }
            public static IMyMedicalRoom MedicalRoom { get; set; }
            public static IMyGasGenerator[] H2O2Generators { get; set; }
            public static IMyGravityGenerator GravityGenerator { get; set; }
            public static IMySensorBlock[] Sensors { get; set; }
            public static IMyInteriorLight[] LandingLights { get; set; }
            public static IMyUserControllableGun[] GatlingTurrets { get; set; }
            public static IMyUserControllableGun[] AssaultCannons { get; set; }
            public static IMyUserControllableGun[] Railguns { get; set; }
            public static IMySmallMissileLauncher[] ArtilleryCannons { get; set; }
            
            public static IMyLandingGear[] HangarMagPlate { get; set; }
            public static IMyRadioAntenna Antenna { get; set; }
            public static IMyCockpit[] OfficerControlSeats { get; set; }
            public static IMyAssembler Assembler { get; set; }
            public static IMyRefinery Refinery { get; set; }
            public static IMyReflectorLight[] DownwardSpotlights { get; set; }
            public static IMyReflectorLight[] Headlights { get; set; }
            public static IMyOreDetector OreDetector { get; set; }
            public static IMyAirVent[] AirVents { get; set; }
            #endregion

            public static bool LandingGearState { get; set; }

            public enum Phase {
                Idle,
                Executing,
            }

            public CBT(Queue<STUStateMachine> thisManeuverQueue, Action<string> Echo, STUInventoryEnumerator inventoryEnumerator, STUMasterLogBroadcaster broadcaster, IMyGridTerminalSystem grid, IMyProgrammableBlock me, IMyGridProgramRuntimeInfo runtime) {
                ManeuverQueue = thisManeuverQueue;
                Me = me;
                InventoryEnumerator = inventoryEnumerator;
                Broadcaster = broadcaster;
                Runtime = runtime;
                CBTGrid = grid;
                echo = Echo;
                ThisCBT = this;


                AllTerminalBlocks = LoadAllBlocksOfType<IMyTerminalBlock>();
                AllFunctionalBlocks = LoadAllBlocksOfType<IMyFunctionalBlock>();

                
                PopulateScreenSubscribers();
                AddToLogQueue("INITIALIZING...");


                Batteries = LoadAllBlocksOfType<IMyBatteryBlock>();
                HydrogenEngines = LoadAllBlocksOfTypeWithSubtypeId<IMyPowerProducer>("HydrogenEngine");
                MergeBlock = LoadBlockByName<IMyShipMergeBlock>("CBT Merge Block");
                CargoContainers = LoadAllBlocksOfType<IMyCargoContainer>();
                OxygenTanks = LoadAllBlocksOfTypeWithDetailedInfo<IMyGasTank>("Oxygen");
                Thrusters = LoadAllBlocksOfType<IMyThrust>();
                Gyros = LoadAllBlocksOfType<IMyGyro>();
                Connector = LoadBlockByName<IMyShipConnector>("CBT Rear Connector");
                    Connector.Enabled = true;
                    Connector.IsParkingEnabled = false;
                    Connector.PullStrength = 0;
                CryoPods = LoadAllBlocksOfType<IMyCryoChamber>();
                LandingGear = LoadAllBlocksOfTypeWithSubtypeId<IMyLandingGear>("LandingGear");
                HangarMagPlate = LoadAllBlocksOfTypeWithSubtypeId<IMyLandingGear>("MagneticPlate");

                
                LoadGatlingGuns(grid); // keeping this one as is...
                OfficerControlSeats = LoadAllBlocksOfTypeWithSubtypeId<IMyCockpit>("Module");
                HydrogenTanks = LoadAllBlocksOfTypeWithDetailedInfo<IMyGasTank>("Hydrogen");
                RemoteControl = LoadBlockByName<IMyRemoteControl>("CBT Remote Control");
                FlightSeat = LoadBlockByName<IMyTerminalBlock>("CBT Flight Seat");
                ButtonPanels = LoadAllBlocksOfType<IMyButtonPanel>();

                
                GangwayHinge1 = LoadBlockByName<IMyMotorStator>("CBT Gangway Hinge 1");
                GangwayHinge2 = LoadBlockByName<IMyMotorStator>("CBT Gangway Hinge 2");
                    GangwayHinge1.TargetVelocityRPM = 0;
                    GangwayHinge2.TargetVelocityRPM = 0;
                    Gangway = new CBTGangway(GangwayHinge1, GangwayHinge2);
                HangarRotor = LoadBlockByName<IMyMotorStator>("CBT Ramp Rotor");
                Doors = LoadAllBlocksOfType<IMyDoor>();

                
                LandingLights = LoadAllBlocksOfTypeWithCustomData<IMyInteriorLight>("LandingLight");
                DownwardSpotlights = LoadAllBlocksOfTypeWithCustomData<IMyReflectorLight>("DownwardSpotlight");
                Headlights = LoadAllBlocksOfTypeWithCustomData<IMyReflectorLight>("Headlight");
                GravityGenerator = LoadBlockByName<IMyGravityGenerator>("CBT Gravity Generator");
                MedicalRoom = LoadBlockByName<IMyMedicalRoom>("CBT Medical Room");
                CameraRotor = LoadBlockByName<IMyMotorStator>("CBT Camera Rotor");
                CameraHinge = LoadBlockByName<IMyMotorStator>("CBT Camera Hinge");
                Camera = LoadBlockByName<IMyCameraBlock>("CBT Bottom Camera");

                
                Antenna = LoadBlockByName<IMyRadioAntenna>("CBT Antenna");
                H2O2Generators = LoadAllBlocksOfType<IMyGasGenerator>();
                AirVents = LoadAllBlocksOfType<IMyAirVent>();

                
                AssaultCannons = LoadAllBlocksOfTypeWithSubtypeId<IMyUserControllableGun>("LargeBlockMediumCalibreTurret");
                Railguns = LoadAllBlocksOfTypeWithSubtypeId<IMyUserControllableGun>("LargeRailgun");
                ArtilleryCannons = LoadAllBlocksOfTypeWithSubtypeId<IMySmallMissileLauncher>("LargeBlockLargeCalibreGun");

                
                Refinery = LoadBlockByName<IMyRefinery>("CBT Refinery");
                Assembler = LoadBlockByName<IMyAssembler>("CBT Assembler");

                
                Sensors = LoadAllBlocksOfType<IMySensorBlock>();
                OreDetector = LoadBlockByName<IMyOreDetector>("CBT Ore Detector");

                // instantiate flight controller
                AddToLogQueue("Initializing FC");
                FlightController = new STUFlightController(grid, RemoteControl, me);
                AddToLogQueue("FC Initialized", STULogType.OK);

                // instantiate docking module
                AddToLogQueue("Initializing Docking Module");
                DockingModule = new CBTDockingModule();
                AddToLogQueue("Docking Module Initialized", STULogType.OK);

                // instantiate airlock control module
                AddToLogQueue("Initializing ACM");
                ACM = new AirlockControlModule();
                ACM.LoadAirlocks(Doors.ToList(), runtime);
                AddToLogQueue("ACM Initialized", STULogType.OK);

                // instantiate power control module
                AddToLogQueue("Initializing PCM");
                PCM = new PowerControlModule(AllFunctionalBlocks.ToList());
                AddToLogQueue("PCM Initialized", STULogType.OK);

                // ensure initial state of certain blocks
                Connector.Enabled = true;
                Connector.IsParkingEnabled = false;
                Connector.PullStrength = 0;

                AddToLogQueue("INITIALIZED", STULogType.OK);
            }

            #region High-Level Software Control Methods
            public static void CreateBroadcast(string message, bool encrypt = false, STULogType type = STULogType.INFO) {
                Broadcaster.Log(new STULog {
                    Sender = CBT_VARIABLES.CBT_VEHICLE_NAME,
                    Message = message,
                    Type = type,
                }
                    );
            }

            // define the method to send CBT log messages to the queue of all the screens on the CBT that are subscribed to such messages
            // actually pulling those messages from the queue and displaying them is done in UpdateLogScreens()
            public static void AddToLogQueue(string message, STULogType type = STULogType.INFO, string sender = CBT_VARIABLES.CBT_VEHICLE_NAME) {
                foreach (var screen in LogChannel) {
                    screen.FlightLogs.Enqueue(new STULog {
                        Sender = sender,
                        Message = message,
                        Type = type,
                    });
                }
            }


            #endregion

            #region Screen Update Methods
            public static void UpdateLogScreens() {
                // get any logs generated by the flight controller and add them to the queue
                while (STUFlightController.FlightLogs.Count > 0) {
                    STULog log = STUFlightController.FlightLogs.Dequeue();
                    AddToLogQueue(log.Message, log.Type, log.Sender);
                }

                // update all the screens that are subscribed to the flight log, which each have their own queue of logs
                foreach (var screen in LogChannel) {
                    screen.StartFrame();
                    screen.WriteWrappableLogs(screen.FlightLogs);
                    screen.EndAndPaintFrame();
                }
            }

            public static void UpdateAutopilotScreens() {
                foreach (var screen in AutopilotStatusChannel) {
                    screen.StartFrame();
                    screen.DrawAutopilotStatus(screen.CurrentFrame, screen.Center);
                    screen.EndAndPaintFrame();
                }
            }

            public static void UpdateManeuverQueueScreens(ManeuverQueueData maneuverQueueData) {
                foreach (var screen in ManeuverQueueChannel) {
                    screen.StartFrame();
                    screen.LoadManeuverQueueData(maneuverQueueData);
                    screen.BuildManeuverQueueScreen(screen.CurrentFrame, screen.Center);
                    screen.EndAndPaintFrame();
                }
            }

            public static void UpdateAmmoScreens() {
                var inventory = InventoryEnumerator.MostRecentItemTotals;
                foreach (var screen in AmmoChannel) {
                    screen.StartFrame();
                    screen.LoadAmmoData(
                        inventory.ContainsKey("Gatling Ammo Box") ? (int)inventory["Gatling Ammo Box"] : 0,
                        inventory.ContainsKey("Assault Cannon Shell") ? (int)inventory["Assault Cannon Shell"] : 0,
                        inventory.ContainsKey("Large Railgun Sabot") ? (int)inventory["Large Railgun Sabot"] : 0,
                        inventory.ContainsKey("Artillery Shell") ? (int)inventory["Artillery Shell"] : 0
                        );
                    screen.BuildScreen(screen.CurrentFrame, screen.Center);
                    screen.EndAndPaintFrame();
                }
            }

            
            public static void UpdateStatusScreens()
            {
                // logic for handling the gathering of subsystem statuses is handled within the CBTStatusLCD class... probably not a good idea for porting to GSC...

                foreach (var screen in StatusChannel)
                {
                    screen.StartFrame();
                    screen.BuildScreen(screen.CurrentFrame, screen.Center);
                    screen.EndAndPaintFrame();
                }
            }

            public static void UpdateBottomCameraScreens()
            {
                foreach (var screen in BottomCameraChannel)
                {
                    screen.StartFrame();
                    screen.BuildScreen(screen.CurrentFrame, screen.Center);
                    screen.EndAndPaintFrame();
                }
            }

            public static void UpdateConfirmationTerminals(ManeuverQueueData data)
            {
                foreach (var terminal in ConfirmationTerminalChannel)
                {
                    terminal.LoadManeuverData(data);
                    terminal.StartFrame();
                    terminal.BuildScreen(terminal.CurrentFrame, terminal.Center);
                    terminal.EndAndPaintFrame();
                }
            }
            #endregion

            #region Hardware Initialization
            #region Screens
            private static void PopulateScreenSubscribers()
            {
                foreach (var block in AllTerminalBlocks)
                {
                    MyIni ini = new MyIni();
                    MyIniParseResult result;
                    if (!ini.TryParse(block.CustomData, out result))
                    {
                        // if there is no text in the custom data, the TryParse will return false (?), so skip to the next block
                        continue;
                    }
                    List<string> sections = new List<string>();
                    ini.GetSections(sections);
                    foreach(var section in sections)
                    {
                        if (section == "SCREEN") {
                            string screenConfig;
                            string channel;
                            float fontSize;
                            List<MyIniKey> keys = new List<MyIniKey>();
                            ini.GetKeys(section, keys);
                            foreach (var key in keys)
                            {
                                try
                                {
                                    screenConfig = ini.Get(key).ToString();
                                    // fill in default font size if not provided
                                    if (!screenConfig.Contains(","))
                                        screenConfig += ",1";
                                    channel = screenConfig.Substring(0, screenConfig.IndexOf(','));
                                    fontSize = 1;
                                    float.TryParse(screenConfig.Substring(screenConfig.IndexOf(',') + 1), out fontSize);
                                    int screenIndex = 0;
                                    int.TryParse(key.ToString().Substring(key.ToString().IndexOf('/') + 1), out screenIndex);
                                    AssignScreenToChannel(screenIndex, channel, fontSize, block);
                                }
                                catch (Exception e)
                                {
                                    echo($"parsing screenConfig or AssignScreenToChannel() failed on block {block.CustomName}:\n{e.Message}");
                                    continue;
                                }
                            }
                        }
                    }
                }
            }

            private static void AssignScreenToChannel(int screenIndex, string channel, float fontSize, IMyTerminalBlock block)
            {
                //echo($"attempting to add block {block.CustomName} to channel {channel} with font size {fontSize}. custom data:\n{block.CustomData}");
                switch (channel)
                {
                    case "LOG":
                        LogChannel.Add(new CBTLogLCD(echo, block, screenIndex, "Monospace", fontSize));
                        break;
                    case "AUTOPILOT":
                        AutopilotStatusChannel.Add(new CBTAutopilotLCD(echo, block, screenIndex, "Monospace", fontSize));
                        break;
                    case "MANEUVER":
                        ManeuverQueueChannel.Add(new CBTManeuverQueueLCD(echo, block, screenIndex, "Monospace", fontSize));
                        break;
                    case "AMMO":
                        AmmoChannel.Add(new CBTAmmoLCD(echo, block, screenIndex, "Monospace", fontSize));
                        break;
                    case "STATUS":
                        StatusChannel.Add(new CBTStatusLCD(echo, block, screenIndex, "Monospace", fontSize));
                        break;
                    case "BOTTOM":
                        BottomCameraChannel.Add(new CBTBottomCameraLCD(ThisCBT, echo, block, screenIndex, "Monospace", fontSize));
                        break;
                    case "CONFIRMATION":
                        ConfirmationTerminalChannel.Add(new CBTConfirmationTerminal(echo, block, screenIndex, "Monospace", fontSize));
                        break;
                    default:
                        return;
                }
            }
            
            public static void PushTOLStatusToBottomCameraScreens(string status)
            {
                foreach (var item in BottomCameraChannel)
                {
                    item.TOLStatus = status;
                }
            }
            #endregion

            #region Block Loading
            public static T[] LoadAllBlocksOfType<T>() where T : class, IMyTerminalBlock
            {
                var intermediateList = new List<T>();
                CBTGrid.GetBlocksOfType(intermediateList, block => block.IsSameConstructAs(Me)); // this should extend to the ends of pistons and rotors and hinges, but not things connected with connectors
                if (intermediateList.Count == 0) { AddToLogQueue($"No blocks of type '{typeof(T).Name}' found on the grid or any connected subgrids.", STULogType.ERROR); }
                return intermediateList.ToArray();
            }
            public static T[] LoadAllBlocksOfTypeWithCustomData<T>(string customData) where T : class, IMyTerminalBlock
            {
                var intermediateList = new List<T>();
                CBTGrid.GetBlocksOfType(intermediateList, block => block.IsSameConstructAs(Me) && block.CustomData.Contains(customData)); // instead of block.CubeGrid == Me.CubeGrid
                if (intermediateList.Count == 0) { AddToLogQueue($"No blocks of type '{typeof(T).Name}' found on the grid whose custom data contains '{customData}'.", STULogType.ERROR); }
                return intermediateList.ToArray();
            }
            public static T[] LoadAllBlocksOfTypeWithDetailedInfo<T>(string detailedInfo) where T : class, IMyTerminalBlock
            {
                var intermediateList = new List<T>();
                CBTGrid.GetBlocksOfType(intermediateList, block => block.IsSameConstructAs(Me) && block.DetailedInfo.Contains(detailedInfo));
                if (intermediateList.Count == 0) { AddToLogQueue($"No blocks of type '{typeof(T).Name}' found on the grid whose detailed info contains '{detailedInfo}'.", STULogType.ERROR); }
                return intermediateList.ToArray();
            }
            public static T[] LoadAllBlocksOfTypeWithSubtypeId<T>(string subtype) where T : class, IMyTerminalBlock
            {
                var intermediateList = new List<T>();
                CBTGrid.GetBlocksOfType(intermediateList, block => block.IsSameConstructAs(Me) && block.BlockDefinition.SubtypeId.Contains(subtype));
                if (intermediateList.Count == 0) { AddToLogQueue($"No blocks of type '{typeof(T).Name}' found on the grid whose subtype ID contains '{subtype}'.", STULogType.ERROR); }
                return intermediateList.ToArray();
            }
            public static T LoadBlockByName<T>(string name) where T : class, IMyTerminalBlock
            {
                var block = CBTGrid.GetBlockWithName(name);
                if (block == null)
                {
                    AddToLogQueue($"Could not find block with name '{name}'.", STULogType.ERROR);
                    return null;
                }
                else { return block as T; }
            }
            #endregion

            private static void LoadGatlingGuns(IMyGridTerminalSystem grid)
            {
                List<IMyUserControllableGun> gatlingGunBlocks = new List<IMyUserControllableGun>();
                grid.GetBlocksOfType<IMyUserControllableGun>(gatlingGunBlocks, block => block.IsSameConstructAs(Me) && 
                    !block.BlockDefinition.SubtypeName.Contains("LargeBlockMediumCalibreTurret") && // not assault turrets
                    !block.BlockDefinition.SubtypeName.Contains("LargeBlockLargeCalibreGun") && // not artillery
                    !block.BlockDefinition.SubtypeName.Contains("LargeRailgun")); // not railguns

                if (gatlingGunBlocks.Count == 0) { AddToLogQueue("No gatling guns found.", STULogType.ERROR); return; }

                GatlingTurrets = gatlingGunBlocks.ToArray();
            }

            #endregion

            #region Inventory reports
            public static int GetHydrogenPercentFilled()
            {
                var inventory = InventoryEnumerator.MostRecentItemTotals;
                double h2RawAmount = inventory.ContainsKey("Hydrogen") ? inventory["Hydrogen"] : 0;
                try
                {
                    return (int)((h2RawAmount / CBT.InventoryEnumerator.HydrogenCapacity) * 100);
                }
                catch
                {
                    return 0; // save for potentially dividing by zero
                }
            }
            public static int GetOxygenPercentFilled()
            {
                var inventory = InventoryEnumerator.MostRecentItemTotals;
                float o2RawAmount = inventory.ContainsKey("Oxygen") ? (float)inventory["Oxygen"] : 0;
                try
                {
                    return (int)((o2RawAmount / CBT.InventoryEnumerator.OxygenCapacity) * 100);
                }
                catch
                {
                    return 0; // save from potentially dividing by zero
                }
            }
            public static int GetPowerPercent()
            {
                var inventory = InventoryEnumerator.MostRecentItemTotals;
                float kWrunningTotal = 0;
                foreach (var battery in Batteries)
                {
                    kWrunningTotal += battery.CurrentStoredPower;
                }
                try
                {
                    return (int)((kWrunningTotal / InventoryEnumerator.PowerCapacity) * 100);
                }
                catch
                {
                    return 0; // save from potentially dividing by zero
                }
            }
            #endregion

            #region CBT Helper Functions
            #region Autopilot
            public static void ResetAutopilot()
            {
                ManeuverQueue.Clear();
                PushTOLStatusToBottomCameraScreens("");
                CancelAttitudeControl();
                CancelCruiseControl();
                ResetUserInputVelocities();
                foreach (var gyro in CBT.FlightController.AllGyroscopes)
                {
                    gyro.Pitch = 0;
                    gyro.Yaw = 0;
                    gyro.Roll = 0;
                }
                CurrentManeuver = null;
                CurrentPhase = CBT.Phase.Idle;
                FlightController.UpdateShipMass();
                Thrusters = LoadAllBlocksOfType<IMyThrust>();
                FlightController.UpdateThrustersAfterGridChange(Thrusters);
                Gyros = LoadAllBlocksOfType<IMyGyro>();
                FlightController.UpdateGyrosAfterGridChange(Gyros);


                SetAutopilotControl(false, false, true);
            }
            
            public static int GetAutopilotState() {
                int autopilotState = 0;
                if (FlightController.HasThrusterControl) { autopilotState += 1; }
                if (FlightController.HasGyroControl) { autopilotState += 2; }
                if (!RemoteControl.DampenersOverride) { autopilotState += 4; }
                // 0 = no autopilot
                // 1 = thrusters only
                // 2 = gyros only
                // 3 = thrusters and gyros
                // 4 = dampeners only
                // 5 = thrusters and dampeners
                // 6 = gyros and dampeners
                // 7 = all three
                return autopilotState;
            }

            public static void SetAutopilotControl(bool thrusters, bool gyroscopes, bool dampeners_enabled) {
                if (thrusters) { FlightController.ReinstateThrusterControl(); } else { FlightController.RelinquishThrusterControl(); }
                if (gyroscopes) { FlightController.ReinstateGyroControl(); } else { FlightController.RelinquishGyroControl(); }
                RemoteControl.DampenersOverride = dampeners_enabled;
            }

            public static void ResetUserInputVelocities() {
                UserInputForwardVelocity = 0;
                UserInputRightVelocity = 0;
                UserInputUpVelocity = 0;
                UserInputRollVelocity = 0;
                UserInputPitchVelocity = 0;
                UserInputYawVelocity = 0;
            }

            /// <summary>
            /// Levels the ship to the horizon by defining a point 1000 meters in front of the ship and then using the Flight Controller's AlignShipToTarget method to point the ship at that point.
            /// Designed to be called continuously when attitude control is activated.
            /// Use the getter of ShipIsLevel to determine whether the ship is actually level.
            /// </summary>
            /// <returns></returns>
            public static bool LevelToHorizon()
            {
                if (RemoteControl.GetNaturalGravity() == null) 
                { 
                    AddToLogQueue("No gravity detected, aborting.", STULogType.WARNING);
                    CancelAttitudeControl();
                    return false; 
                }
                
                if (!AttitudeControlActivated) { SetAutopilotControl(FlightController.HasThrusterControl, true, RemoteControl.DampenersOverride); }
                // point belly towards the ground...
                Vector3 currentRemoteControlPosition = RemoteControl.GetPosition();
                Vector3 targetPosition = currentRemoteControlPosition + 10000 * RemoteControl.GetNaturalGravity();
                ShipIsLevel = FlightController.AlignShipToTarget(targetPosition, MergeBlock, "right");
                AttitudeControlActivated = true; return true;
            }

            /// <summary>
            /// Gracefully disengages attitude control.
            /// </summary>
            public static void CancelAttitudeControl()
            {
                SetAutopilotControl(FlightController.HasThrusterControl, false, RemoteControl.DampenersOverride);
                ShipIsLevel = false;
                AttitudeControlActivated = false;
            }

            public static void SetCruiseControl(float velocity)
            {
                if (!CruiseControlActivated) { SetAutopilotControl(true, FlightController.HasGyroControl, RemoteControl.DampenersOverride); }
                CruiseControlActivated = true;
                CruiseControlSpeed = velocity;
                FlightController.SetStableForwardVelocity(velocity);
            }

            public static void CancelCruiseControl()
            {
                CruiseControlActivated = false;
                SetAutopilotControl(false, false, true);
            }
            #endregion

            public static void SetLandingGear(bool @lock)
            {
                foreach (var gear in LandingGear)
                {
                    gear.Enabled = true; // ensure gears are 'on' for interacting with them
                    if (@lock)
                        gear.Lock();
                    else
                        gear.Unlock();
                }
                LandingGearState = @lock;
            }

            #region Weapons
            public static float GetRailgunRechargeTimeLeft(IMyUserControllableGun railgun)
            {
                string details = railgun.DetailedInfo;
                string[] lines = details.Split('\n');
                float result = 0f;

                foreach (var line in lines)
                {
                    if (line.StartsWith("Fully recharged in:", StringComparison.OrdinalIgnoreCase))
                    {
                        // Remove the label
                        string valuePart = line.Substring("Fully recharged in:".Length).Trim();

                        // Strip any units (like "s")
                        int spaceIndex = valuePart.IndexOf(' ');
                        if (spaceIndex > 0)
                        {
                            valuePart = valuePart.Substring(0, spaceIndex);
                        }

                        float.TryParse(valuePart, out result);
                    }
                }

                return result;
            }

            // this does not work, because DetailedInfo doesn't actually return anything. GPT made an honest mistake.
            // I'm keeping the orphaned code here so I might come back to it someday and implement a timer, where I can use the time between fire events to determine if it's reloading.
            //public static float GetArtilleryReloadStatus(IMyUserControllableGun artilleryGun)
            //{
            //    string details = artilleryGun.DetailedInfo;
            //    echo(artilleryGun.CustomName);
            //    echo(details);
            //    string[] lines = details.Split('\n');
            //    float reloadLevel = 0;
            //    foreach (var line in lines)
            //    {
            //        if (line.StartsWith("Reloading:", StringComparison.OrdinalIgnoreCase))
            //        {
            //            string status = line.Substring("Reloading:".Length).Trim();
            //            if (status.Equals("No", StringComparison.OrdinalIgnoreCase))
            //            {
            //                reloadLevel++;
            //            }
            //        }
            //    }
            //    return reloadLevel;
            //}

            #endregion Weapons
            #endregion

            public static bool AngleCloseEnoughDegrees(float angle, float target, float tolerance = 0.01f)
            {
                return Math.Abs(angle - target) <= tolerance;
            }

            public static bool AngleRangeCloseEnoughDegrees(float angle, float lowerBound, float upperBound, float tolerance = 0.5f)
            {
                return (angle >= lowerBound - tolerance && angle <= upperBound + tolerance);
            }

            // this method did not have any references, so I commented it for character-limit reasons.
            //public static float DegToRad(float angle)
            //{
            //    return (angle * (float)Math.PI / 180);
            //}

            public static float RadToDeg(float angle)
            {
                return (angle * (180/(float)Math.PI));
            }

            public static void FlushLogChannelMessageBuffer()
            {
                if (LogChannelMessageBuffer.Count <= 0)
                {
                    AddToLogQueue("--END--", STULogType.WARNING);
                    return;
                }
                for (int i = 0; i < Math.Min(10, LogChannelMessageBuffer.Count); i++)
                {
                    AddToLogQueue(LogChannelMessageBuffer.Dequeue(), STULogType.INFO);
                }
                if (LogChannelMessageBuffer.Count > 0) { AddToLogQueue("CONTINUES...", STULogType.OK); }
            }

            public static void PopulatePowerLevelReport()
            {
                RefreshBlockInfo(CBTBlocks);
                foreach (var item in CBTBlocks)
                {
                    if (item.PowerLevel > -1) // GetPowerClassOfBlock() returns -1 if it can't find a PL in the custom data, don't care about those blocks here.
                    {
                        LogChannelMessageBuffer.Enqueue($"PL{item.PowerLevel} | {item.Name}");
                    }
                }
            }
            public static void RefreshBlockInfo(List<BlockInfo> list)
            {
                var tmp = new List<IMyTerminalBlock>();
                CBTGrid.GetBlocks(tmp);

                if (list.Count > 0) list.Clear();
                foreach (var block in tmp)
                {
                    list.Add(new BlockInfo
                    {
                        Id = block.EntityId,
                        Name = block.CustomName,
                    });
                }

                list.Sort((a, b) =>
                {
                    int cmp = a.PowerLevel.CompareTo(b.PowerLevel);
                    return cmp != 0 ? cmp : a.Id.CompareTo(b.Id);
                });
            }

        }
    }
}
