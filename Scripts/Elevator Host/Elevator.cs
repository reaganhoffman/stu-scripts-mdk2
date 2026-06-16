using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    partial class Program
    {
        public class Elevator
        {
            public const float BlockLength = 2.5f;
            
            public enum Direction
            {
                Up,
                Down
            }
            public enum State
            {
                GoingUp,
                GoingDown,
                Stopped
            }
            public enum DisplayType
            {
                Floor,
                Cab
            }
            public class FloorButton
            {
                public IMyButtonPanel ButtonPanel { get; private set; }
                public FloorButtonDisplay Display { get; private set; }
            }
            public class Floor
            {
                public int FloorNum { get; set; }
                public int BlocksAboveBasement { get; set; }
                public IMyDoor Door { get; set; }
                public FloorButton UpCallButton { get; set; }
                public FloorButton DownCallButton { get; set; }
                public FloorDisplay Display { get; set; }
            }

            public string ID { get; set; } = string.Empty;
            public List<IMyPistonBase> Pistons { get; set; } = new List<IMyPistonBase>();
            public float MaxElevationPossible { get; private set; }
            public float MinElevationPossible { get; private set; }
            public List<Floor> Floors { get; set; }
            public IMyButtonPanel CabButtonPanel { get; set; }
            public CabButtonPanelDisplay CabButtonDisplay_Up { get; set; }
            public CabButtonPanelDisplay CabButtonDisplay_Down { get; set; }
            public CabButtonPanelDisplay CabButtonDisplay_Enter { get; set; }
            public CabButtonPanelDisplay CabButtonDisplay_Back { get; set; }
            public Elevator(string id, List<IMyPistonBase> pistons)
            {
                ID = id;
                Pistons = pistons;
            }

            float FloorHeightBlockCountToElevation(int blockCount)
            {
                return blockCount * BlockLength;
            }

            float EachPistonDistanceToHitElevation(float elevation)
            {
                
            }
        }
    }
    
}
