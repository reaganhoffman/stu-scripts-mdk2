using System;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript {
    partial class Program {
        class UnifiedHolo {

            const string MAIN_LOG_KEY = "MainLogDisplay";

            List<STUDisplay> _displays;
            public List<MainLogDisplay> _mainLogDisplays;

            public float ScreenOffset = 0.5f; // distance from the center of the display block to the screen portion of the block

            public UnifiedHolo(List<STUDisplay> displays, MyIni ini) {
                MyIniParseResult result;
                _displays = displays;
                _mainLogDisplays = new List<MainLogDisplay>();
                foreach (var display in _displays) {
                    if (!ini.TryParse(display.DisplayBlock.CustomData, out result)) {
                        throw new Exception($"Error parsing log configuration: {result}");
                    }
                    if (ini.Get("UnifiedHolo", MAIN_LOG_KEY).ToBoolean(false)) {
                        int displayIndex = ini.Get("UnifiedHolo", "DisplayIndex").ToInt32(0);
                        double fontSize = ini.Get("UnifiedHolo", "FontSize").ToDouble(1.0);
                        string font = ini.Get("UnifiedHolo", "Font").ToString("Monospace");
                        _mainLogDisplays.Add(new MainLogDisplay(display.DisplayBlock, displayIndex, font, (float)fontSize));
                    }
                }
            }

            public void Update() {
                WriteLogs();
            }

            public void CreateMainLog(string message, STULogType type) {
                STULog log = new STULog {
                    Sender = "PR",
                    Message = message,
                    Type = type
                };
                foreach (var display in _mainLogDisplays) {
                    display.Logs.Enqueue(log);
                }
            }

            void WriteLogs() {
                foreach (var display in _mainLogDisplays) {
                    display.Update();
                }
            }

        }
    }
}
