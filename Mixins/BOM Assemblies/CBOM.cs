#define LIGMA1

using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public class CBOM
        {
            public const string mob = "MyObjectBuilder_";

            public enum Size
            {
                Large,
                Small
            }

            public enum Assembly
            {
                LIGMA1
            }

            public class BOM
            {
                public int SteelPlate;
                public int InteriorPlate;
                public int Construction;
                public int MetalGrid;
                public int Motor;
                public int Computer;
                public int SmallTube;
                public int LargeTube;
                public int Display;
                public int Girder;
                public int BulletproofGlass;
                public int PowerCell;
                public int Detector;
                public int RadioCommunication;
                public int Thrust;
                public int Reactor;
                public int GravityGenerator;
                public int Medical;
                public int Superconductor;
                public int SolarCell;
                public int Explosives;

                public Dictionary<string, int> Components = new Dictionary<string, int>();

                public BOM()
                {
                    Components.Add("SteelPlate", SteelPlate);
                    Components.Add("InteriorPlate", InteriorPlate);
                    Components.Add("Construction", Construction);
                    Components.Add("MetalGrid", MetalGrid);
                    Components.Add("Motor", Motor);
                    Components.Add("Computer", Computer);
                    Components.Add("SmallTube", SmallTube);
                    Components.Add("LargeTube", LargeTube);
                    Components.Add("Display", Display);
                    Components.Add("Girder", Girder);
                    Components.Add("BulletproofGlass", BulletproofGlass);
                    Components.Add("PowerCell", PowerCell);
                    Components.Add("Detector", Detector);
                    Components.Add("RadioCommunication", RadioCommunication);
                    Components.Add("Thrust", Thrust);
                    Components.Add("Reactor", Reactor);
                    Components.Add("GravityGenerator", GravityGenerator);
                    Components.Add("Medical", Medical);
                    Components.Add("Superconductor", Superconductor);
                    Components.Add("SolarCell", SolarCell);
                    Components.Add("Explosives", Explosives);
                }
            }


            static Dictionary<string, BOM> LargeBlockComponents { get; set; } = new Dictionary<string, BOM>()
            {
                {mob + "MyProgrammableBlock/LargeProgrammableBlock", new BOM(){SteelPlate = 21, Construction = 4, Motor = 1, Computer = 2, LargeTube = 2, Display = 1} },
                {mob + "BatteryBlock/LargeBlockBatteryBlock", new BOM(){SteelPlate = 80, Construction = 30, Computer = 25, PowerCell = 80, } }
            };

            public static Dictionary<string, BOM> SmallBlockComponents { get; set; } = new Dictionary<string, BOM>()
            {
                #if LIGMA1
                {mob + "MyProgrammableBlock/SmallProgrammableBlock", new BOM(){SteelPlate = 2, Construction = 2, Motor = 1, Computer = 2, LargeTube = 2, Display = 1} },
                {mob + "BatteryBlock/SmallBlockSmallBatteryBlock", new BOM(){SteelPlate = 25, Construction = 5, Computer = 2, PowerCell = 20} },
                {mob + "CubeBlock/", new BOM(){SteelPlate = 1} },
                {mob + "Thrust/SmallBlockLargeThrust", new BOM(){SteelPlate = 5, Construction = 2, LargeTube = 5} },
                {mob + "Conveyor/SmallBlockConveyorConverter", new BOM(){InteriorPlate = 6, Construction = 8, Motor = 2, SmallTube = 6} },
                {mob + "Conveyor/SmallBlockConveyor", new BOM(){InteriorPlate = 4, Construction = 4, Motor = 1 } },
                {mob + "OxygenTank/SmallHydrogenTank", new BOM(){SteelPlate = 40, Construction = 20, Computer = 4, SmallTube = 30, LargeTube = 20 } },
                {mob + "BatteryBlock/SmallBlockSmallBatteryBlock", new BOM(){SteelPlate = 4, Construction = 2, Computer = 2, PowerCell = 2} },
                {mob + "Conveyor/ConveyorTubeDuctSmallT", new BOM(){SteelPlate = 2, InteriorPlate = 2, Construction = 2, Motor = 1, } },
                {mob + "ConveyorConnector/ConveyorTubeDuctSmallCurved", new BOM(){SteelPlate = 2, InteriorPlate = 1, Construction = 1, Motor = 1} },
                {mob + "ShipConnector/ConnectorSmall", new BOM(){SteelPlate = 7, Construction = 4, Motor = 1, Computer = 4, SmallTube = 2} },
                {mob + "Thrust/SmallBlockSmallHydrogenThrust", new BOM(){SteelPlate = 7, Construction = 15, MetalGrid = 4, LargeTube = 2, } },
                {mob + "ConveyorConnector/ConveyorTubeDuctSmall", new BOM(){SteelPlate = 2, InteriorPlate = 1, Construction = 1, Motor = 1} },
                {mob + "CubeBlock/SmallSymbolL", new BOM(){SteelPlate = 1} },
                {mob + "CubeBlock/SmallSymbolI", new BOM(){SteelPlate = 1} },
                {mob + "CubeBlock/SmallSymbolG", new BOM(){SteelPlate = 1} },
                {mob + "CubeBlock/SmallSymbolM", new BOM(){SteelPlate = 1} },
                {mob + "CubeBlock/SmallSymbolA", new BOM(){SteelPlate = 1} },
                {mob + "CubeBlock/SmallSymbolHyphen", new BOM(){SteelPlate = 1} },
                {mob + "CubeBlock/SmallSymbol1", new BOM(){SteelPlate = 1} },
                {mob + "RadioAntenna/SmallBlockRadioAntenna", new BOM(){SteelPlate = 2, Construction = 1, Computer = 1, SmallTube = 1, RadioCommunication = 4 } },
                {mob + "RemoteControl/SmallBlockRemoteControl", new BOM(){InteriorPlate = 2, Construction = 1, Motor = 1, Computer = 1, } },
                {mob + "Gyro/SmallBlockGyro", new BOM(){SteelPlate = 25, Construction = 5, Motor = 2, Computer = 3, LargeTube = 1} },
                {mob + "CubeBlock/SmallArmorPanelLight", new BOM(){SteelPlate = 1} },
                {mob + "CameraBlock/SmallCameraTopMounted", new BOM(){SteelPlate = 2, Computer = 3, } },
                {mob + "MergeBlock/SmallShipSmallMergeBlock", new BOM(){SteelPlate = 2, Construction = 3, Motor = 1, Computer = 1, SmallTube = 1, } },
                {mob + "SensorBlock/SmallBlockSensorReskin", new BOM(){SteelPlate = 2, InteriorPlate = 5, Computer = 6, Detector = 6, RadioCommunication = 1, } },
                #endif

            };

            public static Dictionary<string, int> GetPartBOM(string block, Size size)
            {
                BOM bom = new BOM();
                switch (size)
                {
                    case Size.Large:
                        LargeBlockComponents.TryGetValue(block, out bom);
                        break;
                    case Size.Small:
                        SmallBlockComponents.TryGetValue(block, out bom);
                        break;
                }

                return bom.Components;
            }

        }

    }
}
