using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class StatusScreen : STUDisplay
        {
            float FontSize { get; set; }

            public StatusScreen(IMyTerminalBlock block, int displayIndex, string font = "Monospace", float fontSize = 1) : base (block, displayIndex, font, fontSize)
            {
                FontSize = fontSize;
            }

            public void Refresh()
            {
                StartFrame();
                MySprite status = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = BALLS.CurrentState.ToString().ToUpper(),
                    Color = VRageMath.Color.White,
                    Position = TopLeft + new Vector2((ScreenWidth - GetTextSpriteWidth(BALLS.CurrentState.ToString())) / 2, (ScreenHeight - GetTextSpriteHeight(BALLS.CurrentState.ToString())) / 2),
                    FontId = "Monospace",
                    RotationOrScale = FontSize
                };

                MySprite background = new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Position = TopLeft + new Vector2(0, ScreenHeight / 2),
                    Size = new Vector2(ScreenWidth, ScreenHeight),
                    Color = new Color(0, 0, 0)
                };

                AlignCenterWithinParent(background, ref status);

                CurrentFrame.Add(status);
                EndAndPaintFrame();
            }
        }
    }
}
