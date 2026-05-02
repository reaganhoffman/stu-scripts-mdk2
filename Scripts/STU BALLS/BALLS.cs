using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
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
                Active,
                Standby,
                MissingResources
            }

            public static State CurrentState { get; set; } = State.Active;
            
            public static IMyGridTerminalSystem Grid { get; set; }
            public static IMyGridProgramRuntimeInfo RuntimeInfo { get; set; }
            public static IMyIntergridCommunicationSystem IGC { get; set; }
            public static IMyProgrammableBlock PB { get; set; }

            static HWLoader HWLoader { get; set; }

            public static AirlockControlModule ACM { get; private set; }
            public static PowerControlModule PCM { get; private set; }
            public static List<STULog> Logs { get; private set; }
            public static LogScreen MainScreen { get; private set; }
            public static StatusScreen SmallScreen { get; private set; }
            public static Queue<STULog> LogQueue { get; private set; }

            public static List<IMyTerminalBlock> AllTerminalBlocks { get; private set; }
            public static IMyRadioAntenna Antenna { get; private set; }
            public static IMyMotorStator GantryStator { get; private set; }
            public static IMyShipMergeBlock MergeBlock { get; private set; }
            public static IMyShipConnector Connector { get; private set; }
            public static IMyProjector Projector { get; private set; }
            public static List<IMyShipWelder> Welders { get; private set; }

            public static long LIGMA_EntityID { get; set; }
            public static bool LIGMA_Ready { get; private set; }
            public static IMyGasTank[] LIGMA_FuelTanks { get; private set; }
            


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

                MainScreen = new LogScreen(PB, 0);
                SmallScreen = new StatusScreen(PB, 1, "Monospace", 3);

                Antenna = HWLoader.LoadBlockByName<IMyRadioAntenna>("Antenna");
                GantryStator = HWLoader.LoadBlockByName<IMyMotorStator>("LIGMA Gantry Rotor");
                MergeBlock = HWLoader.LoadBlockByName<IMyShipMergeBlock>("Small Merge Block");
                Connector = HWLoader.LoadBlockByName<IMyShipConnector>("Small Connector");
                Projector = HWLoader.LoadBlockByName<IMyProjector>("Projector");
                Welders = HWLoader.LoadAllBlocksOfType<IMyShipWelder>().ToList();
            }

            public static void AddToLogQueue(string message, STULogType logType = STULogType.INFO)
            {
                MainScreen.Logs.Enqueue(new STULog("BALLS", message, logType));
            }


            public static void GetLIGMAEntityID()
            {

            }

            public static bool TryGetLIGMAFuelTanks()
            {
                IMyGasTank[] _tanks = HWLoader.LoadAllBlocksOfTypeWithSubtypeId<IMyGasTank>("SmallHydrogenTankSmall");
                if (_tanks.Length == 0) return false;
                else LIGMA_FuelTanks = _tanks; return true;
            }

            
        }
    }
}
