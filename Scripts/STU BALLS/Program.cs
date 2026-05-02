using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRage.Game;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        STUInventoryEnumerator InventoryEnumerator { get; set; }
        STUMasterLogBroadcaster Broadcaster { get; set; }
        STUMasterLogBroadcaster LIGMAUnicaster { get; set; }
        IMyBroadcastListener Listener { get; set; }
        IMyBroadcastListener TelemetryListener { get; set; }
        IMyBroadcastListener LIGMALogListener { get; set; }
        MyCommandLine CommandLine { get; set; }
        MyCommandLine WirelessMessageCommandLine { get; set; }
        MyIni _ini { get; set; } = new MyIni();
        string BALLS_STATION_NAME { get; set; }
        Queue<STUStateMachine> ManeuverQueue { get; set; }
        static STUStateMachine CurrentManeuver { get; set; }

        BALLS _BALLS { get; set; }
        Dictionary<string, double> RequiredComponents { get; set; } = new Dictionary<string, double>();
        

        

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

            _BALLS = new BALLS(GridTerminalSystem, Runtime, IGC, Me, "");
            _BALLS.AddToLogQueue("Initializing subsystems...");

            InventoryEnumerator = new STUInventoryEnumerator(GridTerminalSystem, Me);
            Broadcaster = new STUMasterLogBroadcaster(BALLS_STATION_NAME, IGC, TransmissionDistance.AntennaRelay);
            LIGMAUnicaster = new STUMasterLogBroadcaster("LIGMA-1", IGC, TransmissionDistance.AntennaRelay);
            Listener = IGC.RegisterBroadcastListener(BALLS_STATION_NAME);
            TelemetryListener = IGC.RegisterBroadcastListener(LIGMA_VARIABLES.LIGMA_TELEMETRY_BROADCASTER);
            LIGMALogListener = IGC.RegisterBroadcastListener(LIGMA_VARIABLES.LIGMA_LOG_BROADCASTER);
            CommandLine = new MyCommandLine();
            WirelessMessageCommandLine = new MyCommandLine();
            ManeuverQueue = new Queue<STUStateMachine>();
            _BALLS.CurrentState = BALLS.State.Standby;

            _BALLS.AddToLogQueue("Done.", STULogType.OK);

            RequiredComponents.Add("Steel Plate", 5);
            RequiredComponents.Add("Gyroscope", 5);
        }

        public void Save()
        {
            
        }

        public void Main(string argument, UpdateType updateSource)
        {
            InventoryEnumerator.EnumerateInventories();

            HandleCommand(argument);

            if (Listener.HasPendingMessage) { HandleCommand(Listener.AcceptMessage().ToString()); }

            if (LIGMALogListener.HasPendingMessage)
            {
                _BALLS.AddToLogQueue(STULog.Deserialize(LIGMALogListener.AcceptMessage().Data.ToString()));
            }

            if (ManeuverQueue.Count > 0) { CurrentManeuver = ManeuverQueue.Dequeue(); }

            switch (_BALLS.CurrentState)
            {
                case BALLS.State.Active:
                    if (ManeuverQueue.Count > 0)
                    {
                        CurrentManeuver = ManeuverQueue.Dequeue();
                        CurrentManeuver.ExecuteStateMachine();
                    }
                    //if (!HaveEnoughResources()) BALLS.CurrentState = BALLS.State.MissingResources;
                    break;
                case BALLS.State.Standby:
                    // wait to be reactivated
                    break;
                case BALLS.State.MissingResources:
                    Broadcaster.Log(new STULog(BALLS_STATION_NAME, "Not enough resources", STULogType.ERROR));
                    //if (HaveEnoughResources()) BALLS.CurrentState = BALLS.State.Standby;
                    break;
            }

            BALLS.Update(_BALLS);
        }

        public void HandleCommand(string command)
        {
            if (CommandLine.TryParse(command))
            {
                switch (CommandLine.Argument(0).ToUpper())
                {
                    case "ACTIVATE": _BALLS.CurrentState = BALLS.State.Active; break;
                    case "STANDBY": _BALLS.CurrentState = BALLS.State.Standby; break;
                    default: break;
                }
            }
        }

        public bool HaveEnoughResources()
        {
            // check if we have enough resources for a missile
            Dictionary<MyDefinitionBase, int> remainingBlocks;
            remainingBlocks = _BALLS.Projector.RemainingBlocksPerType;
            foreach (var item in RequiredComponents)
            {
                double currentItemCount;
                InventoryEnumerator.MostRecentItemTotals.TryGetValue(item.Key, out currentItemCount);
                if (currentItemCount < item.Value) return false;
            }
            return true;
        }
    }
}
