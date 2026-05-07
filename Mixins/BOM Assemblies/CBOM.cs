using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public class CBOM
        {
            const string mob = "MyObjectBuilder_";

            public enum Size
            {
                Large,
                Small
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

            static Dictionary<string, BOM> SmallBlockComponents { get; set; } = new Dictionary<string, BOM>()
            {
                {mob + "MyProgrammableBlock/SmallProgrammableBlock", new BOM(){SteelPlate = 2, Construction = 2, Motor = 1, Computer = 2, LargeTube = 2, Display = 1} },
                {mob + "BatteryBlock/SmallBlockSmallBatteryBlock", new BOM(){SteelPlate = 25, Construction = 5, Computer = 2, PowerCell = 20} }
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
