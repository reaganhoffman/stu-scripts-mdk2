using Sandbox.ModAPI.Ingame;

namespace IngameScript {
    public partial class Program : MyGridProgram {
        public Program() {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Save() {
        }

        public void Main() {
            long databaseId;
            long.TryParse(Me.CustomData, out databaseId);
            STUDatabaseWriter writer = new STUDatabaseWriter(databaseId, IGC, Me.EntityId);
            STULog log = new STULog();
            log.Sender = "Tester";
            log.Type = STULogType.INFO;
            log.Message = "Hi";
            writer.TryWriteToRemote(log);
        }
    }
}
