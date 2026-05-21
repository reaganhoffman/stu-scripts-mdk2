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
                Building,
                Standby,
                MissingResources
            }

            public State CurrentState { get; set; }
            
            public IMyGridTerminalSystem Grid { get; set; }
            public IMyGridProgramRuntimeInfo RuntimeInfo { get; set; }
            public IMyIntergridCommunicationSystem IGC { get; set; }
            public IMyProgrammableBlock PB { get; set; }

            static HWLoader HWLoader { get; set; }

            STUMasterLogBroadcaster Broadcaster { get; set; }
            public Queue<STULog> BroadcasterQueue { get; set; } = new Queue<STULog>();
            STUMasterLogBroadcaster LIGMAUnicaster { get; set; }
            public Queue<STULog> LIGMAUnicasterQueue { get; set; } = new Queue<STULog>();
            public string BALLS_STATION_NAME { get; private set; }

            public AirlockControlModule ACM { get; private set; }
            public PowerControlModule PCM { get; private set; }
            public List<STULog> Logs { get; private set; }
            public LogScreen MainScreen { get; private set; }
            public StatusScreen SmallScreen { get; private set; }
            public Queue<STULog> LogQueue { get; private set; }

            public static List<IMyTerminalBlock> AllTerminalBlocks { get; private set; }
            public IMyRadioAntenna Antenna { get; private set; }
            public IMyMotorStator GantryStator { get; private set; }
            public IMyShipMergeBlock MergeBlock { get; private set; }
            public IMyShipConnector Connector { get; private set; }
            public IMyProjector Projector { get; private set; }
            public List<IMyShipWelder> Welders { get; private set; }

            public long LIGMA_EntityID { get; set; }
            public bool LIGMA_Ready { get; private set; }
            public static IMyGasTank[] LIGMA_FuelTanks { get; private set; }
            


            public BALLS(
                IMyGridTerminalSystem grid, 
                IMyGridProgramRuntimeInfo runtime, 
                IMyIntergridCommunicationSystem igc, 
                IMyProgrammableBlock pb, 
                string balls_station_name,
                string pcmSaveState)
            {
                Grid = grid;
                RuntimeInfo = runtime;
                IGC = igc;
                PB = pb;
                BALLS_STATION_NAME = balls_station_name;

                CurrentState = State.Standby;

                HWLoader = new HWLoader(Grid, PB);

                ACM = new AirlockControlModule();
                PCM = new PowerControlModule(pcmSaveState);

                MainScreen = new LogScreen(PB, 0, 0.75f);
                SmallScreen = new StatusScreen(this, PB, 1, "Monospace", 3);

                Antenna = HWLoader.LoadBlockByName<IMyRadioAntenna>("Antenna");
                GantryStator = HWLoader.LoadBlockByName<IMyMotorStator>("LIGMA Gantry Rotor");
                MergeBlock = HWLoader.LoadBlockByName<IMyShipMergeBlock>("Small Merge Block");
                Connector = HWLoader.LoadBlockByName<IMyShipConnector>("Small Connector");
                Projector = HWLoader.LoadBlockByName<IMyProjector>("Projector");
                Welders = HWLoader.LoadAllBlocksOfType<IMyShipWelder>().ToList();

                Broadcaster = new STUMasterLogBroadcaster(BALLS_STATION_NAME, IGC, TransmissionDistance.AntennaRelay);
                LIGMAUnicaster = new STUMasterLogBroadcaster("LIGMA-1", IGC, TransmissionDistance.AntennaRelay);
            }


            public void Update()
            {
                MainScreen.Refresh();
                SmallScreen.Refresh();
                try
                {
                    Broadcaster.Log(BroadcasterQueue.Dequeue());
                }
                catch { }
                try
                {
                    LIGMAUnicaster.Log(LIGMAUnicasterQueue.Dequeue());
                }
                catch { }
            }

            public void AddToLogQueue(string message, STULogType logType = STULogType.INFO)
            {
                MainScreen.Logs.Enqueue(new STULog(BALLS_STATION_NAME, message, logType));
            }

            public void AddToLogQueue(STULog log)
            {
                MainScreen.Logs.Enqueue(log);
            }


            public void GetLIGMAEntityID()
            {

            }

            public bool TryGetLIGMAFuelTanks()
            {
                IMyGasTank[] _tanks = HWLoader.LoadAllBlocksOfTypeWithSubtypeId<IMyGasTank>("SmallHydrogenTankSmall");
                if (_tanks.Length == 0) return false;
                else LIGMA_FuelTanks = _tanks; return true;
            }

        }
    }
}
