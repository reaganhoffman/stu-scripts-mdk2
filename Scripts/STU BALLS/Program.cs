using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Utils;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        STUInventoryEnumerator InventoryEnumerator { get; set; }
        MyCommandLine CommandLine { get; set; }
        IMyBroadcastListener BALLSListener { get; set; }
        IMyBroadcastListener LIGMATelemetryListener { get; set; }
        IMyBroadcastListener LIGMALogListener { get; set; }
        IMyBroadcastListener FiringGroupLogListener { get; set; }
        IMyBroadcastListener FiringGroupTelemetryListener { get; set; }
        MyCommandLine WirelessMessageCommandLine { get; set; }
        MyIni _ini { get; set; } = new MyIni();
        string BALLS_STATION_NAME { get; set; }
        string FIRING_GROUP { get; set; }
        Queue<STUStateMachine> ManeuverQueue { get; set; }
        static STUStateMachine CurrentManeuver { get; set; }
        ConstructLIGMA ConstructionStateMachine { get; set; }

        BALLS _BALLS { get; set; }

        public static bool IGNORE_OUT_OF_RESOURCES { get; private set; } = false;
        

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            if (_ini.TryParse(Me.CustomData))
            {
                BALLS_STATION_NAME = _ini.Get("Configuration", "BALLSStationName").ToString("");
                FIRING_GROUP = _ini.Get("Configuration", "FiringGroup").ToString("");
            }
            else
            {
                Echo($"Malformed configuration in this PB's custom data.\n" +
                    $"BALLSStationName and FiringGroup *must* be defined under the [Configuration] section.\n" +
                    $"They can both be blank, but must be defined.\n" +
                    $"Terminating script.");
                Runtime.UpdateFrequency = UpdateFrequency.None;
                return;
            }

            _BALLS = new BALLS(GridTerminalSystem, Runtime, IGC, Me, BALLS_STATION_NAME, "");
            _BALLS.AddToLocalLogQueue("Initializing subsystems...");

            InventoryEnumerator = new STUInventoryEnumerator(GridTerminalSystem, Me);
            
            CommandLine = new MyCommandLine();
            BALLSListener = IGC.RegisterBroadcastListener(BALLS_STATION_NAME);
            LIGMATelemetryListener = IGC.RegisterBroadcastListener(LIGMA_VARIABLES.LIGMA_TELEMETRY_BROADCASTER);
            LIGMALogListener = IGC.RegisterBroadcastListener(LIGMA_VARIABLES.LIGMA_LOG_BROADCASTER);
            FiringGroupLogListener = IGC.RegisterBroadcastListener(LIGMA_VARIABLES.LIGMA_LOG_BROADCASTER + FIRING_GROUP);
            FiringGroupTelemetryListener = IGC.RegisterBroadcastListener(LIGMA_VARIABLES.LIGMA_TELEMETRY_BROADCASTER + FIRING_GROUP);
            WirelessMessageCommandLine = new MyCommandLine();
            ManeuverQueue = new Queue<STUStateMachine>();
            _BALLS.CurrentState = BALLS.State.Standby;

            _BALLS.AddToLocalLogQueue("Done.", STULogType.OK);

            ConstructionStateMachine = new ConstructLIGMA(_BALLS);
        }

        public void Save()
        {
            
        }

        public void Main(string argument, UpdateType updateSource)
        {
            InventoryEnumerator.EnumerateInventories();

            HandleCommand(argument);

            if (BALLSListener.HasPendingMessage) { HandleIncomingBALLSLog(BALLSListener.AcceptMessage()); }

            if (LIGMALogListener.HasPendingMessage) { HandleIncomingLIGMALog(LIGMALogListener.AcceptMessage()); }

            if (FiringGroupLogListener.HasPendingMessage) { HandleIncomingLIGMALog(FiringGroupLogListener.AcceptMessage()); }

            if (LIGMATelemetryListener.HasPendingMessage) { HandleIncomingLIGMATelemetry(LIGMATelemetryListener.AcceptMessage()); }

            if (FiringGroupTelemetryListener.HasPendingMessage) { HandleIncomingLIGMATelemetry(FiringGroupTelemetryListener.AcceptMessage()); }

            switch (_BALLS.CurrentState)
            {
                case BALLS.State.Active:
                    if (!HaveEnoughResources())
                    {
                        _BALLS.AddToLocalLogQueue("Out of resources; cannot construct a new LIGMA!", STULogType.WARNING);
                        _BALLS.BroadcasterQueue.Enqueue(new STULog(BALLS_STATION_NAME, "Not enough resources", STULogType.WARNING));
                        _BALLS.CurrentState = BALLS.State.MissingResources;
                    }

                    if (!_BALLS.MergeBlock.IsConnected)
                    {
                        ConstructionStateMachine = new ConstructLIGMA(_BALLS);
                        _BALLS.CurrentState = BALLS.State.Building;
                    }                    
                    break;
                case BALLS.State.Building:
                    if (ConstructionStateMachine.ExecuteStateMachine() && ConstructionStateMachine.CurrentInternalState == STUStateMachine.InternalStates.Done) _BALLS.CurrentState = BALLS.State.Active;
                    break;
                case BALLS.State.Standby:
                    ManeuverQueue.Clear();
                    // wait to be reactivated
                    break;
                case BALLS.State.MissingResources:
                    if (HaveEnoughResources()) _BALLS.CurrentState = BALLS.State.Standby;
                    break;
            }

            _BALLS.Update();
        }

        public void HandleCommand(string command)
        {
            if (CommandLine.TryParse(command))
            {
                switch (CommandLine.Argument(0).ToUpper())
                {
                    case "ACTIVATE":
                        ConstructionStateMachine.CurrentInternalState = STUStateMachine.InternalStates.Done;
                        _BALLS.CurrentState = BALLS.State.Active;
                        break;
                    case "STANDBY":
                        ConstructionStateMachine.CurrentInternalState = STUStateMachine.InternalStates.Done;
                        foreach (var welder in _BALLS.Welders) { welder.Enabled = false; }
                        _BALLS.CurrentState = BALLS.State.Standby; 
                        break;
                    case "IGNORE": 
                        _BALLS.AddToLocalLogQueue($"Setting creative mode to {!IGNORE_OUT_OF_RESOURCES}"); 
                        IGNORE_OUT_OF_RESOURCES = !IGNORE_OUT_OF_RESOURCES; 
                        break;
                    case "TEST": 
                        Test(); 
                        break;
                    default: break;
                }
            }
        }

        public void HandleIncomingBALLSLog(MyIGCMessage message)
        {
            try
            {
                STULog receivedLog = STULog.Deserialize(message.Data.ToString());
                _BALLS.AddToLocalLogQueue(receivedLog);
            }
            catch (Exception e)
            {
                _BALLS.AddToLocalLogQueue($"Failed to parse incoming message as STULog:\n'{message}'\n{e.Message}");
            }
        }

        public void HandleIncomingLIGMALog(MyIGCMessage message)
        {
            STULog incomingLog = STULog.Deserialize(message.Data.ToString());
            _BALLS.AddToLocalLogQueue(incomingLog);
        }

        public void HandleIncomingLIGMATelemetry(MyIGCMessage message)
        {
            try
            {
                STULog incomingLog = STULog.Deserialize(message.Data.ToString());
                Dictionary<string, string> incomingTelemetry = incomingLog.Metadata;
                _BALLS.AddToLocalLogQueue(incomingLog);
                string incomingFiringGroup;
                incomingTelemetry.TryGetValue("FiringGroup", out incomingFiringGroup);
                if (string.IsNullOrEmpty(incomingFiringGroup))
                {
                    string incomingId;
                    incomingTelemetry.TryGetValue("Id", out incomingId);
                    long ligma_id;
                    long.TryParse(incomingId, out ligma_id);
                    _BALLS.AddToLocalLogQueue($"Found virgin LIGMA with ID {ligma_id}.");
                    AssignVirginLIGMAToFiringGroup(ligma_id);
                }
            }
            catch (Exception e)
            {
                _BALLS.AddToLocalLogQueue($"Received malformed LIGMA Telemetry. Not processed.\n{e}", STULogType.ERROR);
            }
        }

        public void AssignVirginLIGMAToFiringGroup(long ligma_id)
        {
            IGC.SendUnicastMessage(ligma_id, FIRING_GROUP, new STULog()
            {
                Message = "SubscribeToFiringGroup",
                Sender = BALLS_STATION_NAME,
                Type = STULogType.INFO,
            });
        }

        public bool HaveEnoughResources()
        {
            if (IGNORE_OUT_OF_RESOURCES) return true;

            // check if we have enough resources for a missile
            foreach (var item in GetComponentsNeededForRemainingBlocks())
            {
                double currentItemCount;
                InventoryEnumerator.MostRecentItemTotals.TryGetValue(item.Key, out currentItemCount);
                if (currentItemCount < item.Value) return false;
            }
            return true;
        }

        void Test()
        {
            
        }



        public Dictionary<string, int> GetComponentsNeededForRemainingBlocks()
        {
            Dictionary<string, int> remainingComponents = new Dictionary<string, int>();
            foreach (var block in _BALLS.Projector.RemainingBlocksPerType) // loop through all blocks of the blueprint that have yet to be welded
            {
                Dictionary<string, int> thisBlockBOM = CBOM.GetPartBOM(block.ToString(), CBOM.Size.Small); // retrieve the BOM of each block (e.g. a PB requires some steel plates, some computers, whatever) in the form of a dictionary
                foreach (var component in thisBlockBOM) // loop through this dictionary, and add each of its component/count pairs to the running-total-return-value dictionary
                {
                    if (remainingComponents.ContainsKey(component.Key))
                    {
                        remainingComponents[component.ToString()] += component.Value;
                    }
                    else
                    {
                        remainingComponents.Add(component.Key, component.Value);
                    }
                }
            }
            return remainingComponents;
        }
    }
}
