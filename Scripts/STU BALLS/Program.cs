using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI.Ingame;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        STUInventoryEnumerator InventoryEnumerator { get; set; }
        MyCommandLine CommandLine { get; set; }
        IMyBroadcastListener Listener { get; set; }
        IMyBroadcastListener TelemetryListener { get; set; }
        IMyBroadcastListener LIGMALogListener { get; set; }
        MyCommandLine WirelessMessageCommandLine { get; set; }
        MyIni _ini { get; set; } = new MyIni();
        string BALLS_STATION_NAME { get; set; }
        Queue<STUStateMachine> ManeuverQueue { get; set; }
        static STUStateMachine CurrentManeuver { get; set; }
        ConstructLIGMA ConstructionStateMachine { get; set; }

        BALLS _BALLS { get; set; }

        bool IGNORE_OUT_OF_RESOURCES { get; set; } = false;
        

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            if (_ini.TryParse(Me.CustomData))
            {
                BALLS_STATION_NAME = _ini.Get("CONFIG", "BALLS_STATION_NAME").ToString("");
            }
            else
            {
                Echo($"Malformed configuration in this PB's custom data. Terminating script.");
                Runtime.UpdateFrequency = UpdateFrequency.None;
                return;
            }

            _BALLS = new BALLS(GridTerminalSystem, Runtime, IGC, Me, BALLS_STATION_NAME, "");
            _BALLS.AddToLogQueue("Initializing subsystems...");

            InventoryEnumerator = new STUInventoryEnumerator(GridTerminalSystem, Me);
            
            CommandLine = new MyCommandLine();
            Listener = IGC.RegisterBroadcastListener(BALLS_STATION_NAME);
            TelemetryListener = IGC.RegisterBroadcastListener(LIGMA_VARIABLES.LIGMA_TELEMETRY_BROADCASTER);
            LIGMALogListener = IGC.RegisterBroadcastListener(LIGMA_VARIABLES.LIGMA_LOG_BROADCASTER);
            WirelessMessageCommandLine = new MyCommandLine();
            ManeuverQueue = new Queue<STUStateMachine>();
            _BALLS.CurrentState = BALLS.State.Standby;

            _BALLS.AddToLogQueue("Done.", STULogType.OK);

            ConstructionStateMachine = new ConstructLIGMA(_BALLS);
        }

        public void Save()
        {
            
        }

        public void Main(string argument, UpdateType updateSource)
        {
            InventoryEnumerator.EnumerateInventories();

            HandleCommand(argument);

            if (Listener.HasPendingMessage) { HandleIncomingLog(Listener.AcceptMessage()); }

            if (LIGMALogListener.HasPendingMessage)
            {
                _BALLS.AddToLogQueue(STULog.Deserialize(LIGMALogListener.AcceptMessage().Data.ToString()));
            }

            switch (_BALLS.CurrentState)
            {
                case BALLS.State.Active:
                    if (!HaveEnoughResources())
                    {
                        _BALLS.AddToLogQueue("Out of resources; cannot construct a new LIGMA!", STULogType.WARNING);
                        _BALLS.BroadcasterQueue.Enqueue(new STULog(BALLS_STATION_NAME, "Not enough resources", STULogType.WARNING));
                        _BALLS.CurrentState = BALLS.State.MissingResources;
                    }

                    if (ConstructionStateMachine.CurrentInternalState == STUStateMachine.InternalStates.Done)
                    {
                        ConstructionStateMachine = new ConstructLIGMA(_BALLS);
                        _BALLS.CurrentState = BALLS.State.Building;
                    }                    
                    break;
                case BALLS.State.Building:
                    if (ConstructionStateMachine.ExecuteStateMachine()) _BALLS.CurrentState = BALLS.State.Active;
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
                        _BALLS.AddToLogQueue($"Setting creative mode to {!IGNORE_OUT_OF_RESOURCES}"); 
                        IGNORE_OUT_OF_RESOURCES = !IGNORE_OUT_OF_RESOURCES; 
                        break;
                    case "TEST": 
                        Test(); 
                        break;
                    case "LAUNCH":
                        Launch();
                        break;
                    default: break;
                }
            }
        }

        public void HandleIncomingLog(MyIGCMessage message)
        {
            try
            {
                STULog receivedLog = STULog.Deserialize(message.Data.ToString());
                switch (receivedLog.Message)
                {
                    case "UpdateTargetData":
                        _BALLS.LIGMAUnicasterQueue.Enqueue(receivedLog);
                        _BALLS.BroadcasterQueue.Enqueue(new STULog(BALLS_STATION_NAME, "Target data received.", STULogType.OK));
                        _BALLS.LIGMAUnicasterQueue.Enqueue(new STULog(BALLS_STATION_NAME, "Launch", receivedLog.Type, receivedLog.Metadata));
                        _BALLS.BroadcasterQueue.Enqueue(new STULog(BALLS_STATION_NAME, "Bombs away!", STULogType.OK));
                        break;
                }
            }
            catch (Exception e)
            {
                _BALLS.AddToLogQueue($"Failed to parse incoming message as STULog:\n'{message}'\n{e.Message}");
            }
            finally
            {
                _BALLS.AddToLogQueue($"New wireless message:\n{message.Data}");
            }
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

        void Launch()
        {
            _BALLS.BroadcasterQueue.Enqueue(new STULog() { Message = "Launch" });
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
