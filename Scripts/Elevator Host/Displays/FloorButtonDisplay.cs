using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class FloorButtonDisplay : STUDisplay
        {
            Elevator.Direction Direction { get; set; }
            
            public FloorButtonDisplay(IMyButtonPanel panel, Elevator.Direction direction) : base(panel as IMyTerminalBlock, 0, 1f, "Monospace")
            {
                Direction = direction;
            }

            public void Refresh(bool callReceived)
            {
                MySprite background = new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Alignment = TextAlignment.CENTER,
                    Data = "Square",
                    Position = new Vector2(0f, 0f) * 1 + 0,
                    Size = new Vector2(100f, 100f) * 1,
                    Color = callReceived ? new Color(0, 255, 0, 255) : Color.Black,
                    RotationOrScale = 0
                };
                
                MySprite upArrow = new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Alignment = TextAlignment.CENTER,
                    Data = "Arrow",
                    Position = new Vector2(0f, 0f) * 1 + 0,
                    Size = new Vector2(100f, 100f) * 1,
                    Color = callReceived? Color.Black : new Color(0, 255, 0, 255),
                    RotationOrScale = 0
                };

                MySprite downArrow = new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Alignment = TextAlignment.CENTER,
                    Data = "Arrow",
                    Position = new Vector2(0f, 0f) * 1 + 0,
                    Size = new Vector2(100f, 100f) * 1,
                    Color = callReceived ? Color.Black : new Color(0, 255, 0, 255),
                    RotationOrScale = (float)Math.PI
                };
                
                StartFrame();
                CurrentFrame.Add(background);
                switch (Direction)
                {
                    case Elevator.Direction.Up:
                        CurrentFrame.Add(upArrow);
                        break;
                    case Elevator.Direction.Down:
                        CurrentFrame.Add(downArrow);
                        break;
                }
                EndAndPaintFrame();
            }
        }
    }
    
}
