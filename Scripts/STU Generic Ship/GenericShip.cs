
using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript {
    partial class Program {
        class GenericShip {

            IMyGridTerminalSystem _grid;
            IMyProgrammableBlock _me;
            MyIni _ini;
            List<LogLCD> _logDisplays;

            STUFlightController _flightController;

            // temp variables for efficiency
            STULog t_log;

            STUInventoryEnumerator _inventoryEnumerator;
            Dictionary<string, double> _inventory = new Dictionary<string, double>();

            public GenericShip(IMyGridTerminalSystem grid, IMyProgrammableBlock me) {

                // Baseline grid access
                _grid = grid;
                _me = me;

                // Validate custom data configuration is valid Imi
                _ini = new MyIni();
                MyIniParseResult result;
                if (!_ini.TryParse(me.CustomData, out result)) {
                    throw new Exception($"Error parsing ship configuration: {result}");
                }

                _logDisplays = DiscoverLogSubscribers();
                _flightController = new STUFlightController(grid, DiscoverFlightControllerRemoteControl(), me);
                _inventoryEnumerator = new STUInventoryEnumerator(grid, me);

                CreateMasterLog("All systems nominal.", STULogType.OK);

            }

            public void Update() {
                _flightController.UpdateState();
                _inventoryEnumerator.EnumerateInventories();
                _inventory = _inventoryEnumerator.MostRecentItemTotals;
            }

            IMyRemoteControl DiscoverFlightControllerRemoteControl() {
                string tag = GenericShipVariables.ConfigurationTags.FlightControllerRemoteControl.ToString();
                List<IMyRemoteControl> remoteControls = new List<IMyRemoteControl>();
                _grid.GetBlocksOfType(
                    remoteControls,
                    block => MyIni.HasSection(block.CustomData, tag)
                    && block.CubeGrid == _me.CubeGrid);
                if (remoteControls.Count == 0) {
                    throw new Exception($"No remote controller for flight controller found; insert '[{GenericShipVariables.ConfigurationTags.FlightControllerRemoteControl.ToString()}]' into the Custom Data field of a RC");
                }
                if (remoteControls.Count > 1) {
                    throw new Exception("Multiple flight controllers found in the grid. Please ensure only one is present.");
                }
                return remoteControls[0];
            }

            List<LogLCD> DiscoverLogSubscribers() {
                string tag = GenericShipVariables.ConfigurationTags.LogLCD.ToString();
                List<IMyTextPanel> logPanels = new List<IMyTextPanel>();
                _grid.GetBlocksOfType(
                    logPanels,
                    block => MyIni.HasSection(block.CustomData, tag)
                    && block.CubeGrid == _me.CubeGrid);
                return new List<LogLCD>(logPanels.ConvertAll(panel => {
                    MyIniParseResult result;
                    if (!_ini.TryParse(panel.CustomData, out result)) {
                        throw new Exception($"Error parsing log configuration: {result}");
                    }
                    int displayIndex = _ini.Get(tag, "DisplayIndex").ToInt32(0);
                    double fontSize = _ini.Get(tag, "FontSize").ToDouble(1.0);
                    string font = _ini.Get(tag, "Font").ToString("Monospace");
                    return new LogLCD(panel, displayIndex, font, (float)fontSize);
                }));
            }

            public void CreateMasterLog(string message, STULogType type) {
                t_log = new STULog {
                    Sender = GenericShipVariables.LoggerName,
                    Message = message,
                    Type = type,
                };
                foreach (var subscriber in _logDisplays) {
                    subscriber.Logs.Enqueue(t_log);
                }
            }

            public void UpdateLogDisplays() {
                foreach (var subscriber in _logDisplays) {
                    subscriber.WriteLogs();
                }
            }


        }
    }
}
