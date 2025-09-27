using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript {
    public partial class Program : MyGridProgram {

        const string UNIFIED_HOLO_DISPLAY_TAG = "UnifiedHolo";

        MyIni _ini = new MyIni();
        UnifiedHolo _unifiedHolo;

        public Program() {
            _unifiedHolo = new UnifiedHolo(DiscoverUnifiedHoloDisplays(), _ini);
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Main(string arg) {
            _unifiedHolo.Update();
            _unifiedHolo.CreateMainLog($"UHD alive at {DateTime.Now}", STULogType.INFO);
            echo(_unifiedHolo._mainLogDisplays.Count.ToString());
        }

        List<STUDisplay> DiscoverUnifiedHoloDisplays() {
            List<IMyTextPanel> logPanels = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType(
                logPanels,
                block => MyIni.HasSection(block.CustomData, UNIFIED_HOLO_DISPLAY_TAG)
                && block.CubeGrid == Me.CubeGrid);
            if (logPanels.Count == 0) {
                throw new Exception($"No Unified Holo displays found with tag '{UNIFIED_HOLO_DISPLAY_TAG}'");
            }
            return new List<STUDisplay>(logPanels.ConvertAll(panel => {
                MyIniParseResult result;
                if (!_ini.TryParse(panel.CustomData, out result)) {
                    throw new Exception($"Error parsing log configuration: {result}");
                }
                return new STUDisplay(panel, 0);
            }));
        }

        public void echo(string s) {
            Echo(s);
        }

    }
}