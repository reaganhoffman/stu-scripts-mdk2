using Sandbox.ModAPI.Ingame;

namespace IngameScript {
    partial class Program {
        public class STUDatabaseWriter {

            private long _databaseId;
            private string _entityId;
            private IMyIntergridCommunicationSystem _igc;

            public STUDatabaseWriter(long databaseId, IMyIntergridCommunicationSystem igc, string entityId) {
                _databaseId = databaseId;
            }

            public bool TryWriteToRemote(STULog log) {
                _igc.SendUnicastMessage(_databaseId, _entityId, log.Serialize());
                return false;
            }

        }
    }
}
