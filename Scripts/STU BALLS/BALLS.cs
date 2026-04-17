using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    public partial class Program
    {
        public class BALLS
        {
            public enum State
            {
                Idle,
                Executing
            }

            public static State CurrentState { get; set; } = State.Idle;
            
            public static IMyGridTerminalSystem Grid { get; set; }
            public static IMyGridProgramRuntimeInfo RuntimeInfo { get; set; }
            public static IMyIntergridCommunicationSystem IGC { get; set; }
            public static IMyProgrammableBlock PB { get; set; }

            static HWLoader HWLoader { get; set; }

            public static AirlockControlModule ACM { get; private set; }
            public static PowerControlModule PCM { get; private set; }
            public static List<STULog> Logs { get; private set; }

            public static List<IMyTerminalBlock> AllTerminalBlocks { get; private set; }
            public static IMyRadioAntenna Antenna { get; private set; }
            public static IMyMotorStator GantryBase { get; private set; }
            public static IMyProjector Projector { get; private set; }
            public static List<IMyShipWelder> Welders { get; private set; }
            


            public BALLS(
                IMyGridTerminalSystem grid, 
                IMyGridProgramRuntimeInfo runtime, 
                IMyIntergridCommunicationSystem igc, 
                IMyProgrammableBlock pb, 
                string pcmSaveState)
            {
                Grid = grid;
                RuntimeInfo = runtime;
                IGC = igc;
                PB = pb;

                HWLoader = new HWLoader(Grid, PB);

                ACM = new AirlockControlModule();
                PCM = new PowerControlModule(pcmSaveState);

                Antenna = HWLoader.LoadBlockByName<IMyRadioAntenna>("Antenna");
                GantryBase = HWLoader.LoadBlockByName<IMyMotorStator>("LIGMA Gantry Rotor");
                Projector = HWLoader.LoadBlockByName<IMyProjector>("Projector");
                Welders = HWLoader.LoadAllBlocksOfType<IMyShipWelder>().ToList();
            }

            
        }
    }
}
