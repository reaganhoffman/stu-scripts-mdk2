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
            public enum Direction
            {
                Up,
                Down
            }
            public class FloorButton
            {
                public IMyButtonPanel ButtonPanel { get; private set; }
                public FloorButtonDisplay Display { get; private set; }
                public FloorButton(IMyButtonPanel panel, Direction direction)
                {
                    ButtonPanel = panel;
                    Display = new FloorButtonDisplay(panel, direction);
                }
            }
            public class Floor
            {
                public int FloorNum { get; set; }
                public IMyDoor Door { get; set; }
                public FloorButton UpButton { get; set; }
                public FloorButton DownButton { get; set; }
                public FloorDisplay Display { get; set; }

                public Floor(int floorNum, IMyDoor door, FloorButton upButton, FloorButton downButton, FloorDisplay display)
                {
                    FloorNum = floorNum;
                    Door = door;
                    UpButton = upButton;
                    DownButton = downButton;
                    Display = display;
                }
            }

            public string ID { get; set; } = string.Empty;
            public List<IMyPistonBase> Pistons { get; set; } = new List<IMyPistonBase>();
            public Floor[] Floors { get; set; }
            public CabButtonPanelDisplay CabButtonPanelDisplay { get; set; } = new CabButtonPanelDisplay();
            public IMyButtonPanel ButtonPanel { get; set; }

            public Elevator(string id, List<IMyPistonBase> pistons, List<Floor> floors, IMyButtonPanel buttonPanel)
            {
                ID = id;
                Pistons = pistons;
                Floors = new Floor[floors.Count + 1];
                foreach (var floor in floors)
                {
                    Floors[floor.FloorNum] = floor; // ensures the floors are in the right order
                }
                ButtonPanel = buttonPanel;
            }

            int ElevationToFloorHeightBlockCount(float elevation)
            {

            }

            float FloorHeightBlockCountToElevation(int blockCount)
            {

            }

        }
    }
    
}
