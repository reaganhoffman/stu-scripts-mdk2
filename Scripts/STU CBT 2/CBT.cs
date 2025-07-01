using EmptyKeys.UserInterface.Generated.WorkshopBrowserView_Bindings;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript {
    partial class Program {
        public partial class CBT {

            public static Action<string> echo;

            public const float TimeStep = 1.0f / 6.0f;
            public static Phase CurrentPhase { get; set; } = Phase.Idle;

            private static int _powerLevel { get; set; }
            public static int PowerLevel {
                get { return _powerLevel; }
                set {
                    _powerLevel = value;
                    SetPowerLevel(value);
                }
            }

            public static float UserInputForwardVelocity = 0;
            public static float UserInputRightVelocity = 0;
            public static float UserInputUpVelocity = 0;
            public static float UserInputRollVelocity = 0;
            public static float UserInputPitchVelocity = 0;
            public static float UserInputYawVelocity = 0;

            public static CBTGangway.GangwayStates UserInputGangwayState;
            public static int UserInputRearDockPosition;

            public bool CanDockWithCR = false;

            public static bool CruiseControlActivated { get; private set; } = false;
            public static float CruiseControlSpeed { get; private set; } = 0f;
            public static bool AttitudeControlActivated { get; private set; } = false;
            public static float AltitudeControlHeight { get; private set; } = 0f;

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
            public static List<CBTAutopilotLCD> AutopilotStatusChannel { get; set; } = new List<CBTAutopilotLCD>();
            public static List<CBTManeuverQueueLCD> ManeuverQueueChannel { get; set; } = new List<CBTManeuverQueueLCD>();
            public static List<CBTAmmoLCD> AmmoChannel { get; set; } = new List<CBTAmmoLCD>();
            public static List<CBTStatusLCD> StatusChannel { get; set; } = new List<CBTStatusLCD>();
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
            public static IMyBatteryBlock[] Batteries { get; set; } = LoadAllBlocksOfType<IMyBatteryBlock>();
            public static IMyButtonPanel[] ButtonPanels { get; set; } = LoadAllBlocksOfType<IMyButtonPanel>();
            public static IMyPowerProducer[] HydrogenEngines { get; set; } = LoadAllBlocksOfType<IMyPowerProducer>();
            public static IMyShipMergeBlock MergeBlock { get; set; } = LoadBlockByName<IMyShipMergeBlock>("CBT Merge Block");
            public static IMyGasTank[] HydrogenTanks { get; set; } = LoadAllBlocksOfType<IMyGasTank>("Hydrogen");
            public static IMyGasTank[] OxygenTanks { get; set; } = LoadAllBlocksOfType<IMyGasTank>("Oxygen");
            // power level 1:
            public static IMyRemoteControl RemoteControl { get; set; } = LoadBlockByName<IMyRemoteControl>("CBT Remote Control");
            public static IMyThrust[] Thrusters { get; set; } = LoadAllBlocksOfType<IMyThrust>();
            public static IMyShipConnector Connector { get; set; } // fix this later, Ethan said something about the LIGMA code assuming exactly one connector
            public static IMyMotorStator RearHinge1 { get; set; }
            public static IMyMotorStator RearHinge2 { get; set; }
            public static IMyPistonBase RearPiston { get; set; }
            public static IMyMotorStator GangwayHinge1 { get; set; }
            public static IMyMotorStator GangwayHinge2 { get; set; }
            public static IMyMotorStator HangarRotor { get; set; }
            public static IMyMotorStator CameraRotor { get; set; }
            public static IMyMotorStator CameraHinge { get; set; }
            public static IMyCameraBlock Camera { get; set; }
            
            public static IMyTerminalBlock FlightSeat { get; set; }

            public static IMyGridProgramRuntimeInfo Runtime { get; set; }

            
            public static IMyGyro[] Gyros { get; set; }

            public static IMyLandingGear[] LandingGear { get; set; }
            public static IMyCryoChamber[] CryoPods { get; set; }
            //public static IMyCargoContainer[] CargoContainers { get; set; }
            public static IMyMedicalRoom MedicalRoom { get; set; }
            public static IMyGasGenerator[] H2O2Generators { get; set; }
            public static IMyGravityGenerator GravityGenerator { get; set; }
            public static IMySensorBlock[] Sensors { get; set; }
            public static IMyInteriorLight[] InteriorLights { get; set; }
            public static IMyUserControllableGun[] GatlingTurrets { get; set; }
            public static IMyUserControllableGun[] AssaultCannons { get; set; }
            public static IMyUserControllableGun[] Railguns { get; set; }
            public static IMySmallMissileLauncher[] ArtilleryCannons { get; set; }
            
            public static IMyShipConnector[] HangarMagPlates { get; set; }
            public static IMyDoor[] Doors { get; set; }
            public static IMyRadioAntenna Antenna { get; set; }
            public static IMyCockpit[] FOControlSeats { get; set; }
            public static IMyAssembler Assembler { get; set; }
            public static IMyRefinery Refinery { get; set; }
            public static IMyReflectorLight[] Spotlights { get; set; }
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
            public CBT(Action<string> Echo, STUInventoryEnumerator inventoryEnumerator, STUMasterLogBroadcaster broadcaster, IMyGridTerminalSystem grid, IMyProgrammableBlock me, IMyGridProgramRuntimeInfo runtime) {
                Me = me;
                InventoryEnumerator = inventoryEnumerator;
                Broadcaster = broadcaster;
                Runtime = runtime;
                CBTGrid = grid;
                echo = Echo;

                AddToLogQueue("INITIALIZING...");
                // overhead
                AddLogSubscribers(grid);
                AddAutopilotIndicatorSubscribers(grid);
                AddManeuverQueueSubscribers(grid);
                AddAmmoScreens(grid);
                AddStatusScreens(grid);

                // zero power draw
                //LoadBatteries(grid);
                //LoadButtonPanels(grid);
                //LoadHydrogenEngines(grid);
                //LoadMergeBlock(grid);
                //LoadCargoContainers(grid);
                //LoadOxygenTanks(grid);
                //LoadHydrogenTanks(grid);

                // flight critical ("power level 0" / negligible or intermittent power draw)
                //LoadRemoteController(grid);
                LoadThrusters(grid);
                LoadGyros(grid);
                LoadFlightSeat(grid);
                LoadConnector(grid);
                LoadCryoPods(grid);
                LoadLandingGear(grid);
                LoadDoors(grid);
                LoadHangarMagPlates(grid);
                AddToLogQueue("FLIGHT-CRITICAL COMPONENTS ... DONE");

                // power level 1
                LoadGatlingGuns(grid);
                AddToLogQueue("POWER LEVEL 1 ... DONE");

                // power level 2
                LoadRearDockArm(grid);
                LoadGangwayActuators(grid);
                LoadHangarRotor(grid);
                AddToLogQueue("POWER LEVEL 2 ... DONE");

                // power level 3
                LoadInteriorLights(grid);
                LoadSpotlights(grid);
                LoadGravityGenerator(grid);
                LoadMedicalRoom(grid);
                LoadCamera(grid);
                AddToLogQueue("POWER LEVEL 3 ... DONE");

                // power level 4
                LoadAntenna(grid);
                LoadH2O2Generators(grid);
                LoadAirVents(grid);
                AddToLogQueue("POWER LEVEL 4 ... DONE");

                // power level 5
                LoadAssaultCannonTurrets(grid);
                LoadFOControlSeat(grid);
                LoadRailguns(grid);
                LoadArtilleryCannons(grid);
                AddToLogQueue("POWER LEVEL 5 ... DONE");

                // power level 6
                LoadRefinery(grid);
                LoadAssembler(grid);
                AddToLogQueue("POWER LEVEL 6 ... DONE");

                // power level 7
                LoadSensors(grid);
                LoadOreDetector(grid);
                AddToLogQueue("POWER LEVEL 7 ... DONE");

                AssignPowerClasses(grid);

                FlightController = new STUFlightController(grid, RemoteControl, me);

                DockingModule = new CBTDockingModule();
                ACM = new AirlockControlModule();
                ACM.LoadAirlocks(grid, me, runtime);

                AddToLogQueue("INITIALIZED", STULogType.OK);
            }

            #region High-Level Software Control Methods
            //public static void EchoPassthru(string text) {
            //    echo(text);
            //}

            // define the broadcaster method so that display messages can be sent throughout the world
            // (currently not implemented, just keeping this code here for future use)
            public static void CreateBroadcast(string message, bool encrypt = false, string type = STULogType.INFO) {
                string key = null;
                if (encrypt)
                    key = CBT_VARIABLES.TEA_KEY;

                Broadcaster.Log(new STULog {
                    Sender = CBT_VARIABLES.CBT_VEHICLE_NAME,
                    Message = message,
                    Type = type,
                }
                    );

                AddToLogQueue($"just now finished Create Broadcast with message: {message}, key: {key}");
            }

            // define the method to send CBT log messages to the queue of all the screens on the CBT that are subscribed to such messages
            // actually pulling those messages from the queue and displaying them is done in UpdateLogScreens()
            public static void AddToLogQueue(string message, string type = STULogType.INFO, string sender = CBT_VARIABLES.CBT_VEHICLE_NAME) {
                foreach (var screen in LogChannel) {
                    screen.FlightLogs.Enqueue(new STULog {
                        Sender = sender,
                        Message = message,
                        Type = type,
                    });
                }
            }

            private static void SetPowerLevel(int powerLevel) {
                for (int i = 0; i < PowerClasses.Count; i++) {
                    foreach (var item in PowerClasses[i]) {
                        if (i <= powerLevel) { item.Enabled = true; } else { item.Enabled = false; }
                    }
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
                var inventory = InventoryEnumerator.GetItemTotals();
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
                // logic here getting status updates of other systems

                foreach (var screen in StatusChannel)
                {
                    screen.StartFrame();
                    screen.BuildScreen(screen.CurrentFrame, screen.Center);
                    screen.EndAndPaintFrame();
                }
            }
            #endregion

            #region Hardware Initialization
            #region Overhead ()
            private static void AddLogSubscribers(IMyGridTerminalSystem grid) {
                grid.GetBlocks(AllTerminalBlocks);
                foreach (var block in AllTerminalBlocks) {
                    string CustomDataRawText = block.CustomData;
                    string[] CustomDataLines = CustomDataRawText.Split('\n');
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
                    string CustomDataRawText = block.CustomData;
                    string[] CustomDataLines = CustomDataRawText.Split('\n');
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
                    string CustomDataRawText = block.CustomData;
                    string[] CustomDataLines = CustomDataRawText.Split('\n');
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
                    string CustomDataRawText = block.CustomData;
                    string[] CustomDataLines = CustomDataRawText.Split('\n');
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
            #endregion

            private static T[] LoadAllBlocksOfType<T>() where T : class, IMyTerminalBlock
            {
                var intermediateList = new List<T>();
                CBTGrid.GetBlocksOfType(intermediateList, block => block.CubeGrid == Me.CubeGrid);

                if (intermediateList.Count == 0)
                {
                    AddToLogQueue($"No blocks of type '{typeof(T).Name}' found on the grid.", STULogType.ERROR);
                }

                return intermediateList.ToArray();
            }
            private static T[] LoadAllBlocksOfType<T>(string detailedInfo) where T : class, IMyTerminalBlock
            {
                var intermediateList = new List<T>();
                CBTGrid.GetBlocksOfType(intermediateList, block => block.CubeGrid == Me.CubeGrid && block.DetailedInfo.Contains(detailedInfo));

                if (intermediateList.Count == 0)
                {
                    AddToLogQueue($"No blocks of type '{typeof(T).Name}' found on the grid.", STULogType.ERROR);
                }

                return intermediateList.ToArray();
            }
            private static T LoadBlockByName<T>(string name) where T : class, IMyTerminalBlock
            {
                var block = CBTGrid.GetBlockWithName(name);
                if (block == null)
                {
                    AddToLogQueue($"Could not find block with name '{name}'. It it named correctly?", STULogType.ERROR);
                    return null;
                }
                else { return block as T; }
            }



            #region Zero Power Draw
            //private static void LoadBatteries(IMyGridTerminalSystem grid)
            //{
            //    List<IMyBatteryBlock> batteryBlocks = new List<IMyBatteryBlock>();
            //    grid.GetBlocksOfType<IMyBatteryBlock>(batteryBlocks, block => block.CubeGrid == Me.CubeGrid);
            //    if (batteryBlocks.Count == 0)
            //    {
            //        AddToLogQueue("No batteries found on the CBT", STULogType.ERROR);
            //        return;
            //    }

            //    Batteries = batteryBlocks.ToArray();
            //}
            //private static void LoadButtonPanels(IMyGridTerminalSystem grid)
            //{
            //    List<IMyButtonPanel> buttonPanelBlocks = new List<IMyButtonPanel>();
            //    grid.GetBlocksOfType<IMyButtonPanel>(buttonPanelBlocks, block => block.CubeGrid == Me.CubeGrid);
            //    if (buttonPanelBlocks.Count == 0)
            //    {
            //        AddToLogQueue("No button panels found on the CBT", STULogType.ERROR);
            //        return;
            //    }
            //    ButtonPanels = buttonPanelBlocks.ToArray();
            //}
            //private static void LoadHydrogenEngines(IMyGridTerminalSystem grid)
            //{
            //    List<IMyPowerProducer> hydrogenEngineBlocks = new List<IMyPowerProducer>();
            //    grid.GetBlocksOfType<IMyPowerProducer>(hydrogenEngineBlocks, block => block.CubeGrid == Me.CubeGrid);
            //    if (hydrogenEngineBlocks.Count == 0)
            //    {
            //        AddToLogQueue("No hydrogen engines found on the CBT", STULogType.ERROR);
            //        return;
            //    }

            //    HydrogenEngines = hydrogenEngineBlocks.ToArray();
            //}
            //private static void LoadMergeBlock(IMyGridTerminalSystem grid)
            //{
            //    var mergeBlock = grid.GetBlockWithName("CBT Merge Block");
            //    if (mergeBlock == null)
            //    {
            //        AddToLogQueue("Could not locate \"CBT Merge Block\"; ensure merge block is named appropriately", STULogType.ERROR);
            //        return;
            //    }
            //    MergeBlock = mergeBlock as IMyShipMergeBlock;
            //}
            //private static void LoadCargoContainers(IMyGridTerminalSystem grid)
            //{
            //    List<IMyCargoContainer> cargoContainerBlocks = new List<IMyCargoContainer>();
            //    grid.GetBlocksOfType<IMyCargoContainer>(cargoContainerBlocks, block => block.CubeGrid == Me.CubeGrid);
            //    if (cargoContainerBlocks.Count == 0)
            //    {
            //        AddToLogQueue("No cargo containers found on the CBT", STULogType.ERROR);
            //        return;
            //    }

            //    CargoContainers = cargoContainerBlocks.ToArray();
            //}
            //private static void LoadOxygenTanks(IMyGridTerminalSystem grid)
            //{
            //    List<IMyGasTank> gasTankBlocks = new List<IMyGasTank>();
            //    grid.GetBlocksOfType<IMyGasTank>(gasTankBlocks, block => block.CubeGrid == Me.CubeGrid && block.DetailedInfo.Contains("Oxygen"));
            //    if (gasTankBlocks.Count == 0)
            //    {
            //        AddToLogQueue("No oxygen tanks found on the CBT", STULogType.ERROR);
            //        return;
            //    }

            //    OxygenTanks = gasTankBlocks.ToArray();
            //}
            //private static void LoadHydrogenTanks(IMyGridTerminalSystem grid)
            //{
            //    List<IMyGasTank> gasTankBlocks = new List<IMyGasTank>();
            //    grid.GetBlocksOfType<IMyGasTank>(gasTankBlocks, block => block.CubeGrid == Me.CubeGrid && block.DetailedInfo.Contains("Hydrogen"));
            //    if (gasTankBlocks.Count == 0)
            //    {
            //        AddToLogQueue("No hydrogen tanks found on the CBT", STULogType.ERROR);
            //        return;
            //    }
            //    HydrogenTanks = gasTankBlocks.ToArray();
            //}
            #endregion

            #region Flight Critical (Power Level 0)
            //private static void LoadRemoteController(IMyGridTerminalSystem grid) {
            //    List<IMyTerminalBlock> remoteControlBlocks = new List<IMyTerminalBlock>();
            //    grid.GetBlocksOfType<IMyRemoteControl>(remoteControlBlocks, block => block.CubeGrid == Me.CubeGrid);
            //    if (remoteControlBlocks.Count == 0) {
            //        AddToLogQueue("No remote control blocks found on the CBT", STULogType.ERROR);
            //        return;
            //    }
            //    RemoteControl = remoteControlBlocks[0] as IMyRemoteControl;
            //}
            // load ALL thrusters of ALL types
            // in later versions, fix this to have a list of ALL thrusters, plus subdivided groups of JUST ions and JUST hydros. 
            // even more generalized version of a ship's class should allow for atmo, but the CBT doesn't have atmo.
            //private static void LoadThrusters(IMyGridTerminalSystem grid) {
            //    List<IMyTerminalBlock> thrusterBlocks = new List<IMyTerminalBlock>();
            //    grid.GetBlocksOfType<IMyThrust>(thrusterBlocks, block => block.CubeGrid == Me.CubeGrid);
            //    if (thrusterBlocks.Count == 0) {
            //        AddToLogQueue("No thrusters found on the CBT", STULogType.ERROR);
            //        return;
            //    }

            //    IMyThrust[] allThrusters = new IMyThrust[thrusterBlocks.Count];

            //    for (int i = 0; i < thrusterBlocks.Count; i++) {
            //        allThrusters[i] = thrusterBlocks[i] as IMyThrust;
            //    }

            //    Thrusters = allThrusters;
            //}
            private static void LoadGyros(IMyGridTerminalSystem grid) {
                List<IMyTerminalBlock> gyroBlocks = new List<IMyTerminalBlock>();
                grid.GetBlocksOfType<IMyGyro>(gyroBlocks, block => block.CubeGrid == Me.CubeGrid);
                if (gyroBlocks.Count == 0) {
                    AddToLogQueue("No gyros found on the CBT", STULogType.ERROR);
                    return;
                }

                IMyGyro[] gyros = new IMyGyro[gyroBlocks.Count];
                for (int i = 0; i < gyroBlocks.Count; i++) {
                    gyros[i] = gyroBlocks[i] as IMyGyro;
                }

                Gyros = gyros;
            }
            private static void LoadFlightSeat(IMyGridTerminalSystem grid)
            {
                FlightSeat = grid.GetBlockWithName("CBT Flight Seat") as IMyTerminalBlock;
                if (FlightSeat == null)
                {
                    AddToLogQueue("Could not locate \"CBT Flight Seat\"; ensure flight seat is named appropriately", STULogType.ERROR);
                    return;
                }
            }
            private static void LoadConnector(IMyGridTerminalSystem grid)
            {
                var connector = grid.GetBlockWithName("CBT Rear Connector");
                if (connector == null)
                {
                    AddToLogQueue("Could not locate \"CBT Rear Connector\"; ensure connector is named appropriately.", STULogType.ERROR);
                    return;
                }
                Connector = connector as IMyShipConnector;
                Connector.Enabled = true;
                Connector.IsParkingEnabled = false;
                Connector.PullStrength = 0;
            }
            private static void LoadCryoPods(IMyGridTerminalSystem grid)
            {
                List<IMyTerminalBlock> cryoPodBlocks = new List<IMyTerminalBlock>();
                grid.GetBlocksOfType<IMyCryoChamber>(cryoPodBlocks, block => block.CubeGrid == Me.CubeGrid);

                IMyCryoChamber[] cryoPods = new IMyCryoChamber[cryoPodBlocks.Count];
                for (int i = 0; i < cryoPodBlocks.Count; i++)
                {
                    cryoPods[i] = cryoPodBlocks[i] as IMyCryoChamber;
                }
            }
            private static void LoadLandingGear(IMyGridTerminalSystem grid)
            {
                List<IMyTerminalBlock> landingGearBlocks = new List<IMyTerminalBlock>();
                grid.GetBlocksOfType<IMyLandingGear>(landingGearBlocks, block => block.CubeGrid == Me.CubeGrid);
                if (landingGearBlocks.Count == 0)
                {
                    AddToLogQueue("No landing gear found on the CBT", STULogType.ERROR);
                    return;
                }

                IMyLandingGear[] landingGear = new IMyLandingGear[landingGearBlocks.Count];
                for (int i = 0; i < landingGearBlocks.Count; ++i)
                {
                    landingGear[i] = landingGearBlocks[i] as IMyLandingGear;
                }

                LandingGear = landingGear;
            }
            private static void LoadDoors(IMyGridTerminalSystem grid)
            {
                List<IMyTerminalBlock> doorBlocks = new List<IMyTerminalBlock>();
                grid.GetBlocksOfType<IMyDoor>(doorBlocks, block => block.CubeGrid == Me.CubeGrid);
                IMyDoor[] doors = new IMyDoor[doorBlocks.Count];
                for (int i = 0; i < doorBlocks.Count; i++)
                {
                    doors[i] = doorBlocks[i] as IMyDoor;
                }
                Doors = doors;
            }
            private static void LoadHangarMagPlates(IMyGridTerminalSystem grid)
            {
                List<IMyShipConnector> magPlateBlocks = new List<IMyShipConnector>();
                grid.GetBlocksOfType<IMyShipConnector>(magPlateBlocks, block => block.CubeGrid == Me.CubeGrid && !block.BlockDefinition.SubtypeName.Contains("Connector"));

                HangarMagPlates = magPlateBlocks.ToArray();
            }
            #endregion

            #region Power Level 1
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

            #region Power Level 2
            private static void LoadRearDockArm(IMyGridTerminalSystem grid)
            {
                var hinge1 = grid.GetBlockWithName("CBT Rear Hinge 1");
                var hinge2 = grid.GetBlockWithName("CBT Rear Hinge 2");
                var piston = grid.GetBlockWithName("CBT Rear Piston");
                if (hinge1 == null || hinge2 == null || piston == null)
                {
                    AddToLogQueue("Could not locate at least one stinger arm component; ensure all components are named appropriately", STULogType.ERROR);
                    return;
                }

                RearHinge1 = hinge1 as IMyMotorStator;
                RearHinge2 = hinge2 as IMyMotorStator;
                RearPiston = piston as IMyPistonBase;

                RearDock = new CBTRearDock(RearPiston, RearHinge1, RearHinge2, Connector);
            }
            private static void LoadGangwayActuators(IMyGridTerminalSystem grid)
            {
                var hinge1 = grid.GetBlockWithName("CBT Gangway Hinge 1");
                var hinge2 = grid.GetBlockWithName("CBT Gangway Hinge 2");
                if (hinge1 == null || hinge2 == null)
                {
                    AddToLogQueue("Could not locate at least one gangway actuator component; ensure all components are named appropriately", STULogType.ERROR);
                    return;
                }

                GangwayHinge1 = hinge1 as IMyMotorStator;
                GangwayHinge1.TargetVelocityRPM = 0;
                GangwayHinge2 = hinge2 as IMyMotorStator;
                GangwayHinge2.TargetVelocityRPM = 0;

                Gangway = new CBTGangway(GangwayHinge1, GangwayHinge2);
            }
            private static void LoadHangarRotor(IMyGridTerminalSystem grid)
            {
                var rotor = grid.GetBlockWithName("CBT Ramp Rotor");
                if (rotor == null)
                {
                    AddToLogQueue("Could not locate 'CBT Ramp Rotor' on the grid; ensure it exists and is named properly.", STULogType.ERROR);
                    return;
                }
                HangarRotor = rotor as IMyMotorStator;
            }
            #endregion

            #region Power Level 3
            private static void LoadInteriorLights(IMyGridTerminalSystem grid)
            {
                List<IMyInteriorLight> lightBlocks = new List<IMyInteriorLight>();
                grid.GetBlocksOfType<IMyInteriorLight>(lightBlocks, block => block.CubeGrid == Me.CubeGrid);

                InteriorLights = lightBlocks.ToArray();
            }
            private static void LoadSpotlights(IMyGridTerminalSystem grid)
            {
                List<IMyReflectorLight> spotlightBlocks = new List<IMyReflectorLight>();
                grid.GetBlocksOfType<IMyReflectorLight>(spotlightBlocks, block => block.CubeGrid == Me.CubeGrid);
                
                Spotlights = spotlightBlocks.ToArray();
            }
            private static void LoadGravityGenerator(IMyGridTerminalSystem grid)
            {
                var gravityGenerator = grid.GetBlockWithName("CBT Gravity Generator");
                if (gravityGenerator == null)
                {
                    AddToLogQueue("No gravity generator found on the CBT", STULogType.ERROR);
                    return;
                }

                GravityGenerator = gravityGenerator as IMyGravityGenerator;
            }
            private static void LoadMedicalRoom(IMyGridTerminalSystem grid)
            {
                MedicalRoom = grid.GetBlockWithName("CBT Medical Room") as IMyMedicalRoom;
                if (MedicalRoom == null)
                {
                    AddToLogQueue("Could not locate \"CBT Medical Room\"; ensure medical room is named appropriately", STULogType.ERROR);
                    return;
                }
            }
            private static void LoadCamera(IMyGridTerminalSystem grid)
            {
                var rotor = grid.GetBlockWithName("CBT Camera Rotor");
                var hinge = grid.GetBlockWithName("CBT Camera Hinge");
                var camera = grid.GetBlockWithName("CBT Bottom Camera");
                if (rotor == null || hinge == null || camera == null)
                {
                    AddToLogQueue("Could not locate at least one camera component; ensure all components are named appropriately", STULogType.ERROR);
                    return;
                }

                CameraRotor = rotor as IMyMotorStator;
                CameraHinge = hinge as IMyMotorStator;
                Camera = camera as IMyCameraBlock;
            }
            #endregion

            #region Power Level 4
            private static void LoadAntenna(IMyGridTerminalSystem grid)
            {
                try
                {
                    Antenna = grid.GetBlockWithName("CBT Antenna") as IMyRadioAntenna;
                }
                catch
                {
                    AddToLogQueue("Error trying to find \"CBT Antenna\". Not loaded.", STULogType.WARNING);
                    return;
                }
            }
            private static void LoadH2O2Generators(IMyGridTerminalSystem grid)
            {
                List<IMyTerminalBlock> generatorBlocks = new List<IMyTerminalBlock>();
                grid.GetBlocksOfType<IMyGasGenerator>(generatorBlocks, block => block.CubeGrid == Me.CubeGrid);
                if (generatorBlocks.Count == 0)
                {
                    AddToLogQueue("No H2O2 generators found on the CBT", STULogType.ERROR);
                    return;
                }

                IMyGasGenerator[] generators = new IMyGasGenerator[generatorBlocks.Count];
                for (int i = 0; i < generatorBlocks.Count; i++)
                {
                    generators[i] = generatorBlocks[i] as IMyGasGenerator;
                }

                H2O2Generators = generators;
            }
            private static void LoadAirVents(IMyGridTerminalSystem grid)
            {
                List<IMyAirVent> airVentBlocks = new List<IMyAirVent>();
                grid.GetBlocksOfType<IMyAirVent>(airVentBlocks, block => block.CubeGrid == Me.CubeGrid);
                if (airVentBlocks.Count == 0)
                {
                    AddToLogQueue("No air vents found on the CBT", STULogType.ERROR);
                    return;
                }
                
                AirVents = airVentBlocks.ToArray();
            }
            #endregion

            #region Power Level 5
            private static void LoadAssaultCannonTurrets(IMyGridTerminalSystem grid)
            {
                List<IMyUserControllableGun> assaultCannonBlocks = new List<IMyUserControllableGun>();
                grid.GetBlocksOfType<IMyUserControllableGun>(assaultCannonBlocks, block => block.CubeGrid == Me.CubeGrid && block.BlockDefinition.SubtypeId.Contains("LargeBlockMediumCalibreTurret"));

                if (assaultCannonBlocks.Count == 0) { 
                    AddToLogQueue("No assault cannons found on the CBT", STULogType.ERROR);
                    echo("No assault cannons found on the CBT");
                    return; 
                }
                
                AssaultCannons = assaultCannonBlocks.ToArray();
            }
            private static void LoadFOControlSeat(IMyGridTerminalSystem grid)
            {
                List<IMyCockpit> controlSeatBlocks = new List<IMyCockpit>();
                grid.GetBlocksOfType<IMyCockpit>(controlSeatBlocks, block => block.CubeGrid == Me.CubeGrid && !block.BlockDefinition.SubtypeName.Contains("LargeBlockCockpit"));

                if (controlSeatBlocks.Count == 0)
                {
                    AddToLogQueue("No control seats found on the CBT", STULogType.ERROR);
                    echo("No control seats found on the CBT");
                    return;
                }
                FOControlSeats = controlSeatBlocks.ToArray();
            }

            private static void LoadArtilleryCannons(IMyGridTerminalSystem grid)
            {
                List<IMySmallMissileLauncher> artilleryBlocks = new List<IMySmallMissileLauncher>();
                grid.GetBlocksOfType<IMySmallMissileLauncher>(artilleryBlocks, block => block.CubeGrid == Me.CubeGrid && block.BlockDefinition.SubtypeId.Contains("LargeBlockLargeCalibreGun"));

                if (artilleryBlocks.Count == 0)
                {
                    AddToLogQueue("No artillery guns found on the CBT", STULogType.ERROR);
                    echo("No artillery guns found on the CBT");
                    return;
                }

                ArtilleryCannons = artilleryBlocks.ToArray();
            }
            private static void LoadRailguns(IMyGridTerminalSystem grid)
            {
                List<IMyUserControllableGun> railgunBlocks = new List<IMyUserControllableGun>();
                grid.GetBlocksOfType<IMyUserControllableGun>(railgunBlocks, block => block.CubeGrid == Me.CubeGrid && block.BlockDefinition.SubtypeId.Contains("LargeRailgun"));

                if (railgunBlocks.Count == 0) { 
                    AddToLogQueue("No railguns found on the CBT", STULogType.ERROR);
                    echo("No railguns found on the CBT");
                    return; 
                }

                Railguns = railgunBlocks.ToArray();
            }
            #endregion

            #region Power Level 6
            private static void LoadAssembler(IMyGridTerminalSystem grid)
            {
                try
                {
                    Assembler = grid.GetBlockWithName("CBT Assembler") as IMyAssembler;
                }
                catch
                {
                    AddToLogQueue("Error finding \"CBT Assembler\". Not loaded", STULogType.WARNING);
                    return;
                }
            }
            private static void LoadRefinery(IMyGridTerminalSystem grid)
            {
                try
                {
                    Refinery = grid.GetBlockWithName("CBT Refinery") as IMyRefinery;
                }
                catch
                {
                    AddToLogQueue("Error finding \"CBT Refinery\". Not loaded", STULogType.WARNING);
                    return;
                }
            }
            #endregion

            #region Power Level 7
            private static void LoadSensors(IMyGridTerminalSystem grid)
            {
                List<IMySensorBlock> sensorBlocks = new List<IMySensorBlock>();
                grid.GetBlocksOfType<IMySensorBlock>(sensorBlocks, block => block.CubeGrid == Me.CubeGrid);
                
                Sensors = sensorBlocks.ToArray();
            }
            private static void LoadOreDetector(IMyGridTerminalSystem grid)
            {
                try
                {
                    OreDetector = grid.GetBlockWithName("CBT Ore Detector") as IMyOreDetector;
                }
                catch
                {
                    AddToLogQueue("Error finding \"CBT Ore Detector\". Not loaded.", STULogType.WARNING);
                    return;
                }
            }
            #endregion

            public static void AssignPowerClasses(IMyGridTerminalSystem grid) {
                try
                {
                    CBT.AddToLogQueue("Instantiating power classes...", STULogType.INFO);
                    /// level 1 power class:
                    /// Gatling guns
                    List<IMyFunctionalBlock> level1blocks = new List<IMyFunctionalBlock>();
                    foreach (var item in GatlingTurrets)
                    {
                        level1blocks.Add(item);
                    }
                    PowerClasses[1] = level1blocks;

                    /// level 2 power class:
                    /// rear dock arm assembly
                    /// gangway assembly
                    List<IMyFunctionalBlock> level2blocks = new List<IMyFunctionalBlock>();
                    level2blocks.Add(RearHinge1);
                    level2blocks.Add(RearHinge2);
                    level2blocks.Add(RearPiston);
                    level2blocks.Add(GangwayHinge1);
                    level2blocks.Add(GangwayHinge2);
                    PowerClasses[2] = level2blocks;

                    /// level 3 power class:
                    /// interior lights
                    /// exterior lights (spotlights)
                    /// gravity generator
                    /// med bay
                    /// camera
                    List<IMyFunctionalBlock> level3blocks = new List<IMyFunctionalBlock>();
                    foreach (var item in InteriorLights)
                    {
                        level3blocks.Add(item);
                    }
                    foreach (var item in Spotlights)
                    {
                        level3blocks.Add(item);
                    }
                    level3blocks.Add(GravityGenerator);
                    level3blocks.Add(MedicalRoom);
                    level3blocks.Add(Camera);
                    List<IMyTerminalBlock> allLCDs = new List<IMyTerminalBlock>();
                    grid.GetBlocksOfType<IMyTextPanel>(allLCDs, block => block.CubeGrid == Me.CubeGrid);
                    foreach (var item in allLCDs)
                    {
                        level3blocks.Add(item as IMyFunctionalBlock);
                    }
                    PowerClasses[3] = level3blocks;

                    /// level 4 power class:
                    /// Antenna
                    /// h2/o2 generators
                    /// Air vents
                    List<IMyFunctionalBlock> level4blocks = new List<IMyFunctionalBlock>();
                    level4blocks.Add(Antenna);
                    foreach (var item in H2O2Generators)
                    {
                        level4blocks.Add(item);
                    }
                    foreach (var item in AirVents)
                    {
                        level4blocks.Add(item);
                    }
                    PowerClasses[4] = level4blocks;

                    /// level 5 power class:
                    /// Assault Cannons
                    /// Lower deck control seats
                    /// railguns
                    List<IMyFunctionalBlock> level5blocks = new List<IMyFunctionalBlock>();
                    foreach (var item in AssaultCannons)
                    {
                        level5blocks.Add(item);
                    }
                    foreach (var item in FOControlSeats)
                    {
                        level5blocks.Add(item as IMyFunctionalBlock);
                    }
                    foreach (var item in Railguns)
                    {
                        level5blocks.Add(item);
                    }
                    PowerClasses[5] = level5blocks;

                    /// level 6 power class:
                    /// Assembler
                    /// Refinery
                    List<IMyFunctionalBlock> level6blocks = new List<IMyFunctionalBlock>();
                    level6blocks.Add(Assembler);
                    level6blocks.Add(Refinery);
                    PowerClasses[6] = level6blocks;

                    /// level 7 power class:
                    /// sensors
                    /// ore detector
                    List<IMyFunctionalBlock> level7blocks = new List<IMyFunctionalBlock>();
                    foreach (var item in Sensors)
                    {
                        level7blocks.Add(item);
                    }
                    level7blocks.Add(OreDetector);
                    PowerClasses[7] = level7blocks;
                }
                catch (Exception ex)
                {
                    echo($"Something failed in CBT.AssignPowerClasses().\n{ex.Message}");
                    AddToLogQueue($"Something failed in CBT.AssignPowerClasses().\n{ex.Message}");
                }
            }
            #endregion Hardware Initialization

            #region Inventory reports
            public static int GetHydrogenPercentFilled()
            {
                var inventory = InventoryEnumerator.GetItemTotals();
                float h2RawAmount = inventory.ContainsKey("Hydrogen") ? (float)inventory["Hydrogen"] : 0;
                try
                {
                    return (int)(h2RawAmount / CBT.InventoryEnumerator.HydrogenCapacity) * 100;
                }
                catch
                {
                    return 0; // save for potentially dividing by zero
                }
            }
            public static int GetOxygenPercentFilled()
            {
                var inventory = InventoryEnumerator.GetItemTotals();
                float o2RawAmount = inventory.ContainsKey("Oxygen") ? (float)inventory["Oxygen"] : 0;
                try
                {
                    return (int)(o2RawAmount / CBT.InventoryEnumerator.OxygenCapacity) * 100;
                }
                catch
                {
                    return 0; // save from potentially dividing by zero
                }
            }
            public static int GetPowerPercent()
            {
                var inventory = InventoryEnumerator.GetItemTotals();
                float kWrunningTotal = 0;
                foreach (var battery in Batteries)
                {
                    kWrunningTotal += battery.CurrentStoredPower;
                }
                try
                {
                    return (int)(kWrunningTotal / InventoryEnumerator.PowerCapacity) * 100;
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
            public static void LevelToHorizon()
            {
                if (RemoteControl.GetNaturalGravity() == null) 
                { 
                    AddToLogQueue("No gravity detected, cannot set attitude control.", STULogType.WARNING);
                    CancelAttitudeControl();
                    return; 
                }
                
                if (!AttitudeControlActivated) { SetAutopilotControl(FlightController.HasThrusterControl, true, RemoteControl.DampenersOverride); }
                // point belly towards the ground...
                FlightController.AlignShipToTarget(RemoteControl.GetNaturalGravity(), MergeBlock, "right");
                
                AttitudeControlActivated = true;
            }

            public static void CancelAttitudeControl()
            {
                SetAutopilotControl(FlightController.HasThrusterControl, false, RemoteControl.DampenersOverride);
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
                float chargeLevel = 0;
                foreach (var line in lines)
                {
                    if (line.StartsWith("Fully recharged in:", StringComparison.OrdinalIgnoreCase))
                    {
                        Match match = Regex.Match(line, @"\d+(\.\d+)?");
                        if (match.Success)
                        {
                            chargeLevel = float.Parse(match.Value);
                        }
                        else { return 0; }
                    }
                    else { continue; }
                }
                AddToLogQueue($"Railgun recharge level: {chargeLevel}");
                return chargeLevel;
            }

            public static float GetArtilleryReloadStatus(IMyUserControllableGun artilleryGun)
            {
                string details = artilleryGun.DetailedInfo;
                string[] lines = details.Split('\n');
                float reloadLevel = 0;
                foreach (var line in lines)
                {
                    if (line.StartsWith("Reloading:", StringComparison.OrdinalIgnoreCase))
                    {
                        string status = line.Substring("Reloading:".Length).Trim();
                        if (status.Equals("No", StringComparison.OrdinalIgnoreCase))
                        {
                            reloadLevel++;
                        }
                    }
                }
                AddToLogQueue($"Artillery reload level: {reloadLevel}");
                return reloadLevel;
            }

            #endregion Weapons
            #endregion

            public static bool AngleCloseEnoughDegrees(float angle, float target, float tolerance = 0.01f)
            {
                return Math.Abs(angle - target) <= tolerance;
            }

            public static bool AngleRangeCloseEnoughDegrees(float angle, float lowerBound, float upperBound, float tolerance = 0.01f)
            {
                return (angle >= Math.Abs(angle - lowerBound) && angle <= Math.Abs(angle - upperBound));
            }

            public static float DegToRad(float angle)
            {
                return (angle / 180 * (float)Math.PI);
            }

            public static float RadToDeg(float angle)
            {
                return (angle / (float)Math.PI * 180);
            }

        }
    }
}
