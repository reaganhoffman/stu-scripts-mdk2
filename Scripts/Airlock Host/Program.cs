#define EXTENDED
using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    public partial class Program : MyGridProgram
    {
        public static AirlockControlModule ACM { get; set; }
        
        public Program()
        {
            ACM = new AirlockControlModule();
            ACM.LoadAirlocks(GridTerminalSystem, Me, Runtime);
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            argument = argument.ToUpper();
            double time;

            switch (argument)
            {
                default:
                    if ( double.TryParse(argument, out time)) { ACM.ChangeDuration(time); }
                    break;
                case "INFO": 
                    Echo(ACM.GetAirlocks()); 
                    break;
                case "HELP":
                    Echo($"Commands:\n" +
                        "'OPEN' - Opens all doors (only if automatic airlock control is disabled).\n" +
                        "'CLOSE' - Closes all doors.\n" +
                        "'ENABLE' - Enables automatic airlock control for all doors.\n" +
                        "'DISABLE' - Disables automatic airlock control for all doors.\n" +
                        "'OPEN SOLO' - Opens only 'solo' doors (only if automatic control is disabled).\n" +
                        "'CLOSE SOLO' - Closes only 'solo' doors.\n" + 
                        "'ENABLE SOLO' - Enables automatic airlock control for only solo doors.\n" +
                        "'DISABLE SOLO' - Disables automatic airlock control for only solo doors.\n" +
                        "'OPEN DISABLE' - Opens all doors and disables automatic airlock control.\n" +
                        "'CLOSE ENABLE' - Closes all doors and enables automatic airlock control.\n");
                    break;
                case "OPEN":
                    ACM.OpenAirlocks();
                    ACM.OpenSoloDoors();
                    break;
                case "CLOSE":
                    ACM.CloseAirlocks();
                    ACM.CloseSoloDoors();
                    Echo("All doors CLOSED");
                    break;
                case "ENABLE":
                    ACM.ChangeAutomaticControl(true, true);
                    Echo("Automatic control ENABLED");
                    break;
                case "DISABLE":
                    ACM.ChangeAutomaticControl(false, false);
                    Echo("Automatic control DISABLED");
                    break;
                case "OPEN SOLO":
                    ACM.OpenSoloDoors();
                    Echo("All solo doors OPENED");
                    break;
                case "CLOSE SOLO":
                    ACM.CloseSoloDoors();
                    Echo("All solo doors CLOSED");
                    break;
                case "ENABLE SOLO":
                    ACM.ChangeAutomaticControl(true, ACM.AirlockEnabled);
                    Echo("Automatic control for solo doors ENABLED");
                    break;
                case "DISABLE SOLO":
                    ACM.ChangeAutomaticControl(false, ACM.AirlockEnabled);
                    Echo("Automatic control for solo doors DISABLED");
                    break;
                case "OPEN DISABLE":
                    ACM.OpenAirlocks();
                    ACM.OpenSoloDoors();
                    ACM.ChangeAutomaticControl(false, false);
                    Echo("All doors OPENED and automatic control DISABLED");
                    break;
                case "CLOSE ENABLE":
                    ACM.CloseAirlocks();
                    ACM.CloseSoloDoors();
                    ACM.ChangeAutomaticControl(true, true);
                    Echo("All doors CLOSED and automatic control ENABLED");
                    break;
            }

            ACM.UpdateAirlocks();
        }
    }
}
