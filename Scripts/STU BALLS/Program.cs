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

            string currentState = "";
            try
            {
                currentState = CurrentManeuver.CurrentInternalState.ToString();
            }
            catch
            {
                Echo("cannot get current maneuver's current internal state.");
            }
            finally
            {
                _BALLS.AddToLogQueue($"current internal state: {currentState}");
            }

            switch (_BALLS.CurrentState)
            {
                case BALLS.State.Active:
                    if (ManeuverQueue.Count > 0)
                    {
                        CurrentManeuver = ManeuverQueue.Dequeue();
                        if (CurrentManeuver.ExecuteStateMachine()) ManeuverQueue.Enqueue(new ConstructLIGMA(_BALLS));
                    }
                    if (!HaveEnoughResources())
                    {
                        _BALLS.AddToLogQueue("Out of resources; cannot construct a new LIGMA!", STULogType.WARNING);
                        Broadcaster.Log(new STULog(BALLS_STATION_NAME, "Not enough resources", STULogType.ERROR));
                        _BALLS.CurrentState = BALLS.State.MissingResources;
                    }
                    break;
                case BALLS.State.Standby:
                    ManeuverQueue.Clear();
                    // wait to be reactivated
                    break;
                case BALLS.State.MissingResources:
                    if (HaveEnoughResources()) _BALLS.CurrentState = BALLS.State.Standby;
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
                    case "ACTIVATE": ManeuverQueue.Enqueue(new ConstructLIGMA(_BALLS)); _BALLS.CurrentState = BALLS.State.Active; break;
                    case "STANDBY": _BALLS.CurrentState = BALLS.State.Standby; break;
                    case "IGNORE": _BALLS.AddToLogQueue($"Setting creative mode to {!IGNORE_OUT_OF_RESOURCES}"); IGNORE_OUT_OF_RESOURCES = !IGNORE_OUT_OF_RESOURCES; break;
                    default: break;
                }
            }
        }

        public bool HaveEnoughResources()
        {
            if (IGNORE_OUT_OF_RESOURCES) return true;
            // check if we have enough resources for a missile
            foreach (var itemRequiredForLIGMA in Assmeblies.LIGMA_MK_1)
            {
                double currentItemCount;
                InventoryEnumerator.MostRecentItemTotals.TryGetValue(itemRequiredForLIGMA.Key, out currentItemCount);
                if (currentItemCount < itemRequiredForLIGMA.Value) return false;
            }
            return true;
        }
    }
}
