using Sandbox.ModAPI.Ingame;
using System;
using System.Text;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript {
    public partial class Program : MyGridProgram {

        IMyUnicastListener _broadcastListener;
        MyIGCMessage _incomingMessage;
        STULog _log;
        StringBuilder _logs;
        MyIni _ini;

        string _failedLogs;

        public Program() {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            _ini = new MyIni();
            _broadcastListener = IGC.UnicastListener;
            _logs = new StringBuilder();
            if (!String.IsNullOrEmpty(Storage)) {
                _logs = new StringBuilder(Storage);
            }

            // initialize configuration
            MyIniParseResult parseResult = new MyIniParseResult();
            if (_ini.TryParse(Me.CustomData, out parseResult)) {
                // fill out pre-existing values
            } else {
                // initialize values
            }

            // always set the database ID
            _ini.Set("Configuration", "DatabaseID", Me.EntityId.ToString());
            Me.CustomData = _ini.ToString();
        }

        public void Save() {
            Storage = _logs.ToString();
        }

        public void Main() {
            Echo(_broadcastListener.HasPendingMessage.ToString());
            while (_broadcastListener.HasPendingMessage) {
                try {
                    _incomingMessage = _broadcastListener.AcceptMessage();
                    _log = STULog.Deserialize(_incomingMessage.Data.ToString());
                    _logs.Append(_log.ToPSV(_incomingMessage.Tag));
                } catch (ArgumentException ex) {
                    // figure out later     
                    Echo(ex.Message);
                }
            }
            Save();
        }
    }
}
