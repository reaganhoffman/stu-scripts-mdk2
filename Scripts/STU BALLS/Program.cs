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
        STUInventoryEnumerator InventoryEnumerator { get; set; }
        STUMasterLogBroadcaster Broadcaster { get; set; }
        STUMasterLogBroadcaster LIGMAUnicaster { get; set; }
        IMyBroadcastListener Listener { get; set; }
        MyCommandLine CommandLine { get; set; }
        MyCommandLine WirelessMessageCommandLine { get; set; }
        MyIni _ini { get; set; }
        string BALLS_STATION_NAME { get; set; }
        Queue<STUStateMachine> ManeuverQueue { get; set; }
        static STUStateMachine CurrentManeuver { get; set; }
        Dictionary<string, Dictionary<string, Action>> CommandIndex { get; set; }

        static BALLS _balls { get; set; }
        Dictionary<string, double> RequiredComponents { get; set; } = new Dictionary<string, double>();
        

        

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            if (_ini.TryParse(Me.CustomData))
            {
                try
                {
                    List<MyIniKey> keys = new List<MyIniKey>();
                    _ini.GetKeys(keys);
                    MyIniKey key = keys.Find(name => name.ToString() == "BALLS_STATION_NAME");
                    BALLS_STATION_NAME = _ini.Get(key).ToString();
                }
                catch (Exception e)
                {
                    Echo($"could not find BALLS_STATION_NAME in custom data");
                    BALLS_STATION_NAME = "";
                }
                
            }

            InventoryEnumerator = new STUInventoryEnumerator(GridTerminalSystem, Me);
            Broadcaster = new STUMasterLogBroadcaster(BALLS_STATION_NAME, IGC, TransmissionDistance.AntennaRelay);
            LIGMAUnicaster = new STUMasterLogBroadcaster("LIGMA-1", IGC, TransmissionDistance.AntennaRelay);
            Listener = IGC.RegisterBroadcastListener(BALLS_STATION_NAME);
            CommandLine = new MyCommandLine();
            WirelessMessageCommandLine = new MyCommandLine();
            _ini = new MyIni();
            _balls = new BALLS(GridTerminalSystem, Runtime, IGC, Me, "");

            RequiredComponents.Add("Steel Plates", 5);
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

            switch (BALLS.CurrentState)
            {
                case BALLS.State.Active:
                    CurrentManeuver.ExecuteStateMachine();
                    if (!CheckResources()) BALLS.CurrentState = BALLS.State.MissingResources;
                    break;
                case BALLS.State.Standby:
                    // wait to be reactivated
                    break;
                case BALLS.State.MissingResources:
                    Broadcaster.Log(new STULog(BALLS_STATION_NAME, "Not enough resources", STULogType.ERROR));
                    if (CheckResources()) BALLS.CurrentState = BALLS.State.Standby;
                    break;
            }
        }

        public void HandleCommand(string command)
        {
            if (CommandLine.TryParse(command))
            {
                switch (CommandLine.Argument(0).ToUpper())
                {
                    case "ACTIVATE": BALLS.CurrentState = BALLS.State.Active; break;
                    case "STANDBY": BALLS.CurrentState = BALLS.State.Standby; break;
                    case "TARGET": HandleArguments(CommandLine); break;
                }
            }
        }

        public void HandleArguments(MyCommandLine commandLine)
        {
            string parentCommand = commandLine.Argument(0).ToUpper();
            Dictionary<string, Action> subcommands;
            if ( CommandIndex.TryGetValue(parentCommand, out subcommands))
            {
                for (int i = 1; i < commandLine.ArgumentCount; i++)
                {
                    Action subcommand;
                    if (subcommands.TryGetValue(commandLine.Argument(i), out subcommand))
                    {
                        subcommand();
                    }
                }
            }
        }

        public bool CheckResources()
        {
            // check if we have enough resources for a missile
            foreach (var item in RequiredComponents)
            {
                double currentItemCount;
                if (InventoryEnumerator.MostRecentItemTotals.TryGetValue(item.Key, out currentItemCount) & currentItemCount < item.Value) return false;
            }
            return true;
        }
    }
}
