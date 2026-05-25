using Sandbox.ModAPI.Ingame;
using System;

namespace IngameScript {
    partial class Program {
        public class STUDatabaseWriter {

            private long _databaseId;
            private string _entityId;
            private IMyIntergridCommunicationSystem _igc;

            public STUDatabaseWriter(long databaseId, IMyIntergridCommunicationSystem igc, long entityId) {
                _databaseId = databaseId;
                _entityId = entityId.ToString();
                _igc = igc;
            }

            public STUDatabaseWriter(string databaseId, IMyIntergridCommunicationSystem igc, long entityId) {
                long parsedDatabasedID;
                if (!long.TryParse(databaseId, out parsedDatabasedID)) {
                    throw new Exception("Error parsing database ID");
                }
                _databaseId = parsedDatabasedID;
                _entityId = entityId.ToString();
                _igc = igc;
            }

            /// <summary>
            /// Returns true if the log was successfully sent, not necessarily if it was successfully written to disk
            /// </summary>
            /// <param name="log"></param>
            /// <returns></returns>
            public bool TryWriteToRemote(STULog log) {
                return _igc.SendUnicastMessage(_databaseId, _entityId, log.Serialize());
            }

        }
    }
}
