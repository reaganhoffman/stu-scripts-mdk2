using EmptyKeys.UserInterface.Generated.WorkshopBrowserView_Bindings;
using Sandbox.Game.Screens;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using VRage.Game.ModAPI.Ingame;
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
            public static int UserInputRearDockPosition { get; set; }

            public bool CanDockWithCR { get; set; } = false;

            public static bool CruiseControlActivated { get; private set; } = false;
            public static float CruiseControlSpeed { get; private set; } = 0f;
            public static bool AttitudeControlActivated { get; private set; } = false;
            public static float AltitudeControlHeight { get; private set; } = 0f;
            public static bool ShipIsLevel { get; private set; } = false;

            //public static Vector3D NextWaypoint;

            /// <summary>
            ///  prepare the program by declaring all the different blocks we are going to use
            /// </summary>
            // this may be potentially confusing, but "GridTerminalSystem" as it is commonly used in Program.cs to get blocks from the grid
            // does not exist in this namespace. Therefore, we are creating a new GridTerminalSystem object here to use in this class.
            // I could have named it whatever, e.g. "CBTGrid" but I don't want to have too many different names for the same thing.
            // just understand that when I reference the GridTerminalSystem property of the CBT class, I am referring to this object and NOT the one in Program.cs
            public static IMyGridTerminalSystem CBTGrid { get; set; }
            public static List<IMyTerminalBlock> AllTerminalBlocks { get; set; } = new List<IMyTerminalBlock>();
            public static List<CBTLogLCD> LogChannel { get; set; } = new List<CBTLogLCD>();
            public static Queue<string> LogChannelMessageBuffer { get; set; } = new Queue<string>();
            public static List<CBTAutopilotLCD> AutopilotStatusChannel { get; set; } = new List<CBTAutopilotLCD>();
            public static List<CBTManeuverQueueLCD> ManeuverQueueChannel { get; set; } = new List<CBTManeuverQueueLCD>();
            public static Queue<STUStateMachine> ManeuverQueue { get; set; }
            public static List<CBTAmmoLCD> AmmoChannel { get; set; } = new List<CBTAmmoLCD>();
            public static List<CBTStatusLCD> StatusChannel { get; set; } = new List<CBTStatusLCD>();
            public static List<CBTBottomCameraLCD> BottomCameraChannel { get; set; } = new List<CBTBottomCameraLCD>();
            public static STUFlightController FlightController { get; set; }
            public static CBTDockingModule DockingModule { get; set; }
            public static AirlockControlModule ACM { get; set; }
            public static CBTGangway Gangway { get; set; }
            public static CBTRearDock RearDock { get; set; }
            public static IMyProgrammableBlock Me { get; set; }
            public static STUMasterLogBroadcaster Broadcaster { get; set; }
            public static STUInventoryEnumerator InventoryEnumerator { get; set; }
            #region Hardware
            // power level 0:
            public static IMyBatteryBlock[] Batteries { get; set; } 
            public static IMyButtonPanel[] ButtonPanels { get; set; } 
            public static IMyPowerProducer[] HydrogenEngines { get; set; } //= LoadAllBlocksOfType<IMyPowerProducer>();
            public static IMyShipMergeBlock MergeBlock { get; set; } //= LoadBlockByName<IMyShipMergeBlock>("CBT Merge Block");
            public static IMyGasTank[] HydrogenTanks { get; set; } //= LoadAllBlocksOfType<IMyGasTank>("Hydrogen");
            public static IMyGasTank[] OxygenTanks { get; set; } //= LoadAllBlocksOfType<IMyGasTank>("Oxygen");
            // power level 1:
            public static IMyRemoteControl RemoteControl { get; set; } //= LoadBlockByName<IMyRemoteControl>("CBT Remote Control");
            public static IMyThrust[] Thrusters { get; set; } //= LoadAllBlocksOfType<IMyThrust>();
            public static IMyGyro[] Gyros { get; set; } //= LoadAllBlocksOfType<IMyGyro>();
            public static IMyTerminalBlock FlightSeat { get; set; } //= LoadBlockByName<IMyTerminalBlock>("CBT Flight Seat");
            public static IMyShipConnector Connector { get; set; } //= LoadBlockByName<IMyShipConnector>("CBT Rear Connector");
            public static IMyCryoChamber[] CryoPods { get; set; } //= LoadAllBlocksOfType<IMyCryoChamber>();
            public static IMyLandingGear[] LandingGear { get; set; } //= LoadAllBlocksOfType<IMyLandingGear>();
            public static IMyDoor[] Doors { get; set; } //= LoadAllBlocksOfType<IMyDoor>();
            public static IMyMotorStator RearHinge1 { get; set; }
            public static IMyMotorStator RearHinge2 { get; set; }
            public static IMyPistonBase RearPiston { get; set; }
            public static IMyMotorStator GangwayHinge1 { get; set; }
            public static IMyMotorStator GangwayHinge2 { get; set; }
            public static IMyMotorStator HangarRotor { get; set; }
            public static IMyMotorStator CameraRotor { get; set; }
            public static IMyMotorStator CameraHinge { get; set; }
            public static IMyCameraBlock Camera { get; set; }
            public static IMyLandingGear StingerLock { get; set; }
            public static IMyMotorStator StingerLockRotor { get; set; }
            

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
            
            public static IMyLandingGear[] HangarMagPlates { get; set; }
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


            // define phases for the main state machine
            // the one that will be used in conjunction with the ManeuverQueue
            public enum Phase {
                Idle,
                Executing,
            }

            public static Dictionary<int, List<IMyFunctionalBlock>> PowerClasses = new Dictionary<int, List<IMyFunctionalBlock>>()
            {
                // { 0, new List<IMyFunctionalBlock> { } },
                { 1, new List<IMyFunctionalBlock> { } },
                { 2, new List<IMyFunctionalBlock> { } },
                { 3, new List<IMyFunctionalBlock> { } },
                { 4, new List<IMyFunctionalBlock> { } },
                { 5, new List<IMyFunctionalBlock> { } },
                { 6, new List<IMyFunctionalBlock> { } },
                { 7, new List<IMyFunctionalBlock> { } },
            };

            // CBT object constructor
            public CBT(Queue<STUStateMachine> thisManeuverQueue, Action<string> Echo, STUInventoryEnumerator inventoryEnumerator, STUMasterLogBroadcaster broadcaster, IMyGridTerminalSystem grid, IMyProgrammableBlock me, IMyGridProgramRuntimeInfo runtime) {
                ManeuverQueue = thisManeuverQueue;
                Me = me;
                InventoryEnumerator = inventoryEnumerator;
                Broadcaster = broadcaster;
                Runtime = runtime;
                CBTGrid = grid;
                echo = Echo;
                ThisCBT = this;
                

                AddToLogQueue("INITIALIZING...");
                // overhead
                AddLogSubscribers(grid);
                AddAutopilotIndicatorSubscribers(grid);
                AddManeuverQueueSubscribers(grid);
                AddAmmoScreens(grid);
                AddStatusScreens(grid);
                AddBottomCameraScreens(grid);

                // power level 0 (flight critical, negligible or intermittent power draw)
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
                LandingGear = LoadAllBlocksOfType<IMyLandingGear>();
                HangarMagPlates = LoadAllBlocksOfTypeWithSubtypeId<IMyLandingGear>("MagneticPlate");
                StingerLockRotor = LoadBlockByName<IMyMotorStator>("Stinger Lock Rotor");

                // power level 1
                LoadGatlingGuns(grid); // keeping this one as is...
                OfficerControlSeats = LoadAllBlocksOfTypeWithSubtypeId<IMyCockpit>("Module");
                StingerLock = LoadBlockByName<IMyLandingGear>("Stinger Lock");
                HydrogenTanks = LoadAllBlocksOfTypeWithDetailedInfo<IMyGasTank>("Hydrogen");
                RemoteControl = LoadBlockByName<IMyRemoteControl>("CBT Remote Control");
                FlightSeat = LoadBlockByName<IMyTerminalBlock>("CBT Flight Seat");
                ButtonPanels = LoadAllBlocksOfType<IMyButtonPanel>();

                // power level 2
                RearHinge1 = LoadBlockByName<IMyMotorStator>("CBT Rear Hinge 1");
                RearHinge2 = LoadBlockByName<IMyMotorStator>("CBT Rear Hinge 2");
                RearPiston = LoadBlockByName<IMyPistonBase>("CBT Rear Piston");
                    RearDock = new CBTRearDock(ManeuverQueue, RearPiston, RearHinge1, RearHinge2, Connector);
                GangwayHinge1 = LoadBlockByName<IMyMotorStator>("CBT Gangway Hinge 1");
                GangwayHinge2 = LoadBlockByName<IMyMotorStator>("CBT Gangway Hinge 2");
                    GangwayHinge1.TargetVelocityRPM = 0;
                    GangwayHinge2.TargetVelocityRPM = 0;
                    Gangway = new CBTGangway(GangwayHinge1, GangwayHinge2);
                HangarRotor = LoadBlockByName<IMyMotorStator>("CBT Ramp Rotor");
                Doors = LoadAllBlocksOfType<IMyDoor>();

                // power level 3
                LandingLights = LoadAllBlocksOfTypeWithCustomData<IMyInteriorLight>("LandingLight");
                DownwardSpotlights = LoadAllBlocksOfTypeWithCustomData<IMyReflectorLight>("DownwardSpotlight");
                Headlights = LoadAllBlocksOfTypeWithCustomData<IMyReflectorLight>("Headlight");
                GravityGenerator = LoadBlockByName<IMyGravityGenerator>("CBT Gravity Generator");
                MedicalRoom = LoadBlockByName<IMyMedicalRoom>("CBT Medical Room");
                CameraRotor = LoadBlockByName<IMyMotorStator>("CBT Camera Rotor");
                CameraHinge = LoadBlockByName<IMyMotorStator>("CBT Camera Hinge");
                Camera = LoadBlockByName<IMyCameraBlock>("CBT Bottom Camera");

                // power level 4
                Antenna = LoadBlockByName<IMyRadioAntenna>("CBT Antenna");
                H2O2Generators = LoadAllBlocksOfType<IMyGasGenerator>();
                AirVents = LoadAllBlocksOfType<IMyAirVent>();

                // power level 5
                AssaultCannons = LoadAllBlocksOfTypeWithSubtypeId<IMyUserControllableGun>("LargeBlockMediumCalibreTurret");
                Railguns = LoadAllBlocksOfTypeWithSubtypeId<IMyUserControllableGun>("LargeRailgun");
                ArtilleryCannons = LoadAllBlocksOfTypeWithSubtypeId<IMySmallMissileLauncher>("LargeBlockLargeCalibreGun");

                // power level 6
                Refinery = LoadBlockByName<IMyRefinery>("CBT Refinery");
                Assembler = LoadBlockByName<IMyAssembler>("CBT Assembler");

                // power level 7
                Sensors = LoadAllBlocksOfType<IMySensorBlock>();
                OreDetector = LoadBlockByName<IMyOreDetector>("CBT Ore Detector");

                SetPowerLevel(7);

                FlightController = new STUFlightController(grid, RemoteControl, me);

                DockingModule = new CBTDockingModule();
                ACM = new AirlockControlModule();
                ACM.LoadAirlocks(grid, me, runtime);

                Connector.Enabled = true;
                Connector.IsParkingEnabled = false;
                Connector.PullStrength = 0;

                AddToLogQueue("INITIALIZED", STULogType.OK);
            }

            #region High-Level Software Control Methods
            //public static void EchoPassthru(string text) {
            //    echo(text);
            //}

            // define the broadcaster method so that display messages can be sent throughout the world
            // (currently not implemented, just keeping this code here for future use)
            public static void CreateBroadcast(string message, bool encrypt = false, STULogType type = STULogType.INFO) {
                // had some abandoned encryption logic here

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

            public static void SetPowerLevel(int powerLevel) {
                AddToLogQueue($"Setting Power Level to PL-{powerLevel}");
                PowerLevel = powerLevel;
                List<IMyFunctionalBlock> allFunctionalBlocks = new List<IMyFunctionalBlock>();
                CBTGrid.GetBlocksOfType<IMyFunctionalBlock>(allFunctionalBlocks);
                foreach (var block in allFunctionalBlocks)
                {
                    int powerClassOfBlock = GetPowerClassOfBlock(block);
                    if (powerClassOfBlock < 0 ) continue; // don't interact at all with blocks that don't have a defined power class
                    else if (powerClassOfBlock <= powerLevel) 
                    {
                        block.Enabled = true; 
                    }
                    else 
                    { 
                        if (powerLevel < 1 && (FlightController.HasGyroControl || FlightController.HasThrusterControl))
                        {
                            AddToLogQueue("FLIGHT CONTROLLER IS ACTIVE! Refusing to go below power level 1.", STULogType.WARNING);
                            return;
                        }
                        block.Enabled = false; 
                    }
                }
            }

            public static int GetPowerClassOfBlock(IMyTerminalBlock block)
            {
                int powerLevel;
                string[] customDataRawLines = block.CustomData.Split('\n');
                foreach (var line in customDataRawLines)
                {
                    if (line.Equals("")) { continue; }
                    else if (line.Contains("PL"))
                    {
                        if (int.TryParse(line.Substring(2), out powerLevel)) { return powerLevel; }
                        else continue;
                    }
                }
                return -1;
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
            #endregion

            #region Hardware Initialization
            #region Screens
            private static void AddLogSubscribers(IMyGridTerminalSystem grid) {
                grid.GetBlocks(AllTerminalBlocks);
                foreach (var block in AllTerminalBlocks) {
                    string[] CustomDataLines = block.CustomData.Split('\n');
                    foreach (var line in CustomDataLines) {
                        if (line.Contains("CBT_LOG")) {
                            string[] kvp = line.Split(':');
                            // adjust font size based on what screen we're trying to initalize
                            float fontSize;
                            try {
                                fontSize = float.Parse(kvp[2]);
                                if (fontSize < 0.1f || fontSize > 10f) {
                                    throw new Exception("Invalid font size");
                                }
                            } catch (Exception e) {
                                echo("caught exception in CBT.AddLogSubscribers():");
                                echo(e.Message);
                                fontSize = 0.5f;
                            }
                            CBTLogLCD screen = new CBTLogLCD(echo, block, int.Parse(kvp[1]), "Monospace", fontSize);
                            LogChannel.Add(screen);
                        }
                    }
                }
            }
            private static void AddAutopilotIndicatorSubscribers(IMyGridTerminalSystem grid) {
                grid.GetBlocks(AllTerminalBlocks);
                foreach (var block in AllTerminalBlocks) {
                    string[] CustomDataLines = block.CustomData.Split('\n');
                    foreach (var line in CustomDataLines) {
                        if (line.Contains("CBT_AUTOPILOT")) {
                            string[] kvp = line.Split(':');
                            CBTAutopilotLCD screen = new CBTAutopilotLCD(echo, block, int.Parse(kvp[1]));
                            AutopilotStatusChannel.Add(screen);
                        }
                    }
                }
            }
            private static void AddManeuverQueueSubscribers(IMyGridTerminalSystem grid) {
                grid.GetBlocks(AllTerminalBlocks);
                foreach (var block in AllTerminalBlocks) {
                    string[] CustomDataLines = block.CustomData.Split('\n');
                    foreach (var line in CustomDataLines) {
                        if (line.Contains("CBT_MANEUVER_QUEUE")) {
                            string[] kvp = line.Split(':');
                            CBTManeuverQueueLCD screen = new CBTManeuverQueueLCD(echo, block, int.Parse(kvp[1]));
                            ManeuverQueueChannel.Add(screen);
                        }
                    }
                }
            }
            private static void AddAmmoScreens(IMyGridTerminalSystem grid) {
                grid.GetBlocks(AllTerminalBlocks);
                foreach (var block in AllTerminalBlocks) {
                    string[] CustomDataLines = block.CustomData.Split('\n');
                    foreach (var line in CustomDataLines) {
                        if (line.Contains("CBT_AMMO")) {
                            string[] kvp = line.Split(':');
                            CBTAmmoLCD screen = new CBTAmmoLCD(echo, block, int.Parse(kvp[1]));
                            AmmoChannel.Add(screen);
                        }
                    }
                }
            }
            private static void AddStatusScreens(IMyGridTerminalSystem grid)
            {
                grid.GetBlocks(AllTerminalBlocks);
                foreach (var block in AllTerminalBlocks)
                {
                    string CustomDataRawText = block.CustomData;
                    string[] CustomDataLines = CustomDataRawText.Split('\n');
                    foreach (var line in CustomDataLines)
                    {
                        if (line.Contains("CBT_STATUS"))
                        {
                            string[] kvp = line.Split(':');
                            float fontSize;
                            try
                            {
                                fontSize = float.Parse(kvp[2]);
                                if (fontSize < 0.1f || fontSize > 10f)
                                {
                                    throw new Exception("Invalid font size");
                                }
                            }
                            catch (Exception e)
                            {
                                echo("caught exception in CBT.AddStatusScreens():");
                                echo(e.Message);
                                fontSize = 0.5f;
                            }
                            CBTStatusLCD screen = new CBTStatusLCD(echo, block, int.Parse(kvp[1]), "Monospace", fontSize);
                            StatusChannel.Add(screen);
                        }
                    }
                }
            }

            private static void AddBottomCameraScreens(IMyGridTerminalSystem grid)
            {
                grid.GetBlocks(AllTerminalBlocks);
                foreach (var block in AllTerminalBlocks)
                {
                    string CustomDataRawText = block.CustomData;
                    string[] CustomDataLines = CustomDataRawText.Split('\n');
                    foreach (var line in CustomDataLines)
                    {
                        if (line.Contains("CBT_BOTTOM_CAMERA"))
                        {
                            string[] kvp = line.Split(':');
                            float fontSize;
                            try
                            {
                                fontSize = float.Parse(kvp[2]);
                                if (fontSize < 0.1f || fontSize > 10f)
                                {
                                    throw new Exception("Invalid font size");
                                }
                            }
                            catch (Exception e)
                            {
                                echo("caught exception in CBT.AddBottomCameraScreens():");
                                echo(e.Message);
                                fontSize = 0.5f;
                            }
                            CBTBottomCameraLCD screen = new CBTBottomCameraLCD(ThisCBT, echo, block, int.Parse(kvp[1]), "Monospace", fontSize);
                            BottomCameraChannel.Add(screen);
                        }
                    }
                }
            }

            public void PushTOLStatusToBottomCameraScreens(string status)
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
                CBTGrid.GetBlocksOfType(intermediateList, block => block.CubeGrid == Me.CubeGrid);
                if (intermediateList.Count == 0) { AddToLogQueue($"No blocks of type '{typeof(T).Name}' found on the grid.", STULogType.ERROR); }
                return intermediateList.ToArray();
            }
            public static T[] LoadAllBlocksOfTypeWithCustomData<T>(string customData) where T : class, IMyTerminalBlock
            {
                var intermediateList = new List<T>();
                CBTGrid.GetBlocksOfType(intermediateList, block => block.CubeGrid == Me.CubeGrid && block.CustomData.Contains(customData));
                if (intermediateList.Count == 0) { AddToLogQueue($"No blocks of type '{typeof(T).Name}' found on the grid whose custom data contains '{customData}'.", STULogType.ERROR); }
                return intermediateList.ToArray();
            }
            public static T[] LoadAllBlocksOfTypeWithDetailedInfo<T>(string detailedInfo) where T : class, IMyTerminalBlock
            {
                var intermediateList = new List<T>();
                CBTGrid.GetBlocksOfType(intermediateList, block => block.CubeGrid == Me.CubeGrid && block.DetailedInfo.Contains(detailedInfo));
                if (intermediateList.Count == 0) { AddToLogQueue($"No blocks of type '{typeof(T).Name}' found on the grid whose detailed info contains '{detailedInfo}'.", STULogType.ERROR); }
                return intermediateList.ToArray();
            }
            public static T[] LoadAllBlocksOfTypeWithSubtypeId<T>(string subtype) where T : class, IMyTerminalBlock
            {
                var intermediateList = new List<T>();
                CBTGrid.GetBlocksOfType(intermediateList, block => block.CubeGrid == Me.CubeGrid && block.BlockDefinition.SubtypeId.Contains(subtype));
                if (intermediateList.Count == 0) { AddToLogQueue($"No blocks of type '{typeof(T).Name}' found on the grid whose subtype ID contains '{subtype}'.", STULogType.ERROR); }
                return intermediateList.ToArray();
            }
            public static T LoadBlockByName<T>(string name) where T : class, IMyTerminalBlock
            {
                var block = CBTGrid.GetBlockWithName(name);
                if (block == null)
                {
                    AddToLogQueue($"Could not find block with name '{name}'. Is it named correctly?", STULogType.ERROR);
                    return null;
                }
                else { return block as T; }
            }
            #endregion

            private static void LoadGatlingGuns(IMyGridTerminalSystem grid)
            {
                List<IMyUserControllableGun> gatlingGunBlocks = new List<IMyUserControllableGun>();
                grid.GetBlocksOfType<IMyUserControllableGun>(gatlingGunBlocks, block => block.CubeGrid == Me.CubeGrid && 
                    !block.BlockDefinition.SubtypeName.Contains("LargeBlockMediumCalibreTurret") && // not assault turrets
                    !block.BlockDefinition.SubtypeName.Contains("LargeBlockLargeCalibreGun") && // not artillery
                    !block.BlockDefinition.SubtypeName.Contains("LargeRailgun")); // not railguns

                if (gatlingGunBlocks.Count == 0) { AddToLogQueue("No gatling guns found on the CBT", STULogType.ERROR); return; }

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

            public static void SetAutopilotControl(bool thrusters, bool gyroscopes, bool dampeners) {
                if (thrusters) { FlightController.ReinstateThrusterControl(); } else { FlightController.RelinquishThrusterControl(); }
                if (gyroscopes) { FlightController.ReinstateGyroControl(); } else { FlightController.RelinquishGyroControl(); }
                RemoteControl.DampenersOverride = dampeners;
            }

            public static void ResetUserInputVelocities() {
                UserInputForwardVelocity = 0;
                UserInputRightVelocity = 0;
                UserInputUpVelocity = 0;
                UserInputRollVelocity = 0;
                UserInputPitchVelocity = 0;
                UserInputYawVelocity = 0;
            }
            public static bool LevelToHorizon()
            {
                if (RemoteControl.GetNaturalGravity() == null) 
                { 
                    AddToLogQueue("No gravity detected, cannot set attitude control.", STULogType.WARNING);
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

            public static float DegToRad(float angle)
            {
                return (angle * (float)Math.PI / 180);
            }

            public static float RadToDeg(float angle)
            {
                return (angle * (180/(float)Math.PI));
            }

            public static void FlushLogChannelMessageBuffer()
            {
                if (LogChannelMessageBuffer.Count <= 0)
                {
                    AddToLogQueue("--END OF MESSAGE BUFFER--", STULogType.WARNING);
                    return;
                }
                for (int i = 0; i < Math.Min(10, LogChannelMessageBuffer.Count); i++)
                {
                    AddToLogQueue(LogChannelMessageBuffer.Dequeue(), STULogType.INFO);
                }
                if (LogChannelMessageBuffer.Count > 0) { AddToLogQueue("REPORT CONTINUES...", STULogType.OK); }
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
                        PowerLevel = GetPowerClassOfBlock(block),
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
