using Sandbox.ModAPI.Ingame;
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
        public class CabButtonPanelDisplay : STUDisplay
        {
            public CabButtonPanelDisplay(IMyTerminalBlock block, int displayIndex, float fontSize = 1, string font = "Monospace") : base(block, displayIndex, fontSize, font)
            {
                MySprite downButton = new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Alignment = TextAlignment.CENTER,
                    Data = "Arrow",
                    Position = new Vector2(0f, 0f) * 1 + 0,
                    Size = new Vector2(100f, 100f) * 1,
                    Color = new Color(255, 255, 255, 255),
                    RotationOrScale = (float)Math.PI
                };

                MySprite upButton = new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Alignment = TextAlignment.CENTER,
                    Data = "Arrow",
                    Position = new Vector2(0f, 0f) * 1 + 0,
                    Size = new Vector2(100f, 100f) * 1,
                    Color = new Color(255, 255, 255, 255),
                    RotationOrScale = 0
                };

                MySprite okButton = new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Alignment = TextAlignment.CENTER,
                    Data = "Circle",
                    Position = new Vector2(0f, 0f) * 1 + 0,
                    Size = new Vector2(100f, 97f) * 1,
                    Color = new Color(0, 255, 0, 255),
                    RotationOrScale = 0f
                };

                StartFrame();
                switch (displayIndex)
                {
                    case 0: CurrentFrame.Add(downButton); break;
                    case 1: CurrentFrame.Add(upButton); break;
                    case 2: CurrentFrame.Add(okButton); break;
                    default: break;
                }
                EndAndPaintFrame();
            }

        }
    }
    
}
