using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class CBTBottomCameraLCD : STUDisplay
        {
            CBT ThisCBT { get; set; }
            Action<string> Echo { get; set; }

            public float CharWidth { get; set; }
            public float CharHeight { get; set; }
            public float FontSize { get; set; }

            double Vt { get; set; }
            double Vx { get; set; }
            double Vy { get; set; }
            double Vz { get; set; }
            double ALT { get; set; }
            public string TOLStatus { get; set; }
            string GangwayStatus { get; set; }
            string RampStatus { get; set; }
            string StingerStatus { get; set; }
            string CCStatus { get; set; }
            string ATTStatus { get; set; }
            public CBTBottomCameraLCD(CBT cbtObject, Action<string> echo, IMyTerminalBlock block, int displayIndex, string font = "Monospace", float fontSize = 1) : base(block, displayIndex, font, fontSize)
            {
                ThisCBT = cbtObject;
                Echo = echo;
                CharWidth = GetTextSpriteWidth("A") * fontSize;
                CharHeight = GetTextSpriteHeight("A") * fontSize;
                FontSize = fontSize;
            }

            void UpdateValues()
            {
                Vt = Math.Round( CBT.FlightController.VelocityMagnitude, 0);
                Vx = Math.Round( CBT.FlightController.CurrentVelocity_LocalFrame.X, 0);
                Vy = Math.Round( CBT.FlightController.CurrentVelocity_LocalFrame.Y, 0);
                Vz = Math.Round( CBT.FlightController.CurrentVelocity_LocalFrame.Z, 0);
                ALT = Math.Round( CBT.FlightController.GetCurrentSurfaceAltitude(), 0);
                // TOL status is pushed from the landing / takeoff maneuvers and elsewhere in the code... not a great way to do it, but I think it stems from the CBT constructor not being static.
                switch (CBT.Gangway.CurrentGangwayState) // can not invoke CBT.Gangway.CurrentGangwayState.ToString() because of the minifier...
                {
                    case CBTGangway.GangwayStates.Unknown: GangwayStatus = "UNKNOWN"; break;
                    case CBTGangway.GangwayStates.Retracting: GangwayStatus = "RETRACTING"; break;
                    case CBTGangway.GangwayStates.Retracted: GangwayStatus = "RETRACTED"; break;
                    case CBTGangway.GangwayStates.Extending: GangwayStatus = "EXTENDING"; break;
                    case CBTGangway.GangwayStates.Extended: GangwayStatus = "EXTENDED"; break;
                }
                RampStatus = "MOVING";
                if (CBT.AngleCloseEnoughDegrees(CBT.RadToDeg(CBT.HangarRotor.Angle), 0))
                {
                    RampStatus = "CLOSED";
                }
                else if (CBT.AngleRangeCloseEnoughDegrees(CBT.RadToDeg(CBT.HangarRotor.Angle), 90, 110))
                {
                    RampStatus = "OPEN";
                }
                StingerStatus = CBTRearDock.KnownPorts[CBTRearDock.DesiredPosition].Name.ToString();
                CCStatus = CBT.CruiseControlSpeed.ToString();
                if (!CBT.CruiseControlActivated) CCStatus = "OFF";
                ATTStatus = BoolConverter(CBT.AttitudeControlActivated);
            }

            public void BuildScreen(MySpriteDrawFrame frame, Vector2 centerPos, float scale = 1f)
            {
                UpdateValues();
                MySprite background = new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Position = TopLeft + new Vector2(0, ScreenHeight / 2),
                    Size = new Vector2(ScreenWidth, ScreenHeight),
                    Color = new Color(0, 0, 0, 0) // fourth argument makes the screen clear... I think
                };
                MySprite VT = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = $"Vt " + $"{this.Vt,5}",
                    Position = TopLeft + new Vector2(ScreenWidth - GetTextSpriteWidth($"Vt {this.Vt,5}"), 0),
                    Color = Color.Cyan,
                    FontId = "Monospace",
                    RotationOrScale = FontSize
                };
                MySprite VX = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = $"Vx " + $"{this.Vx,5}",
                    Position = TopLeft + new Vector2(ScreenWidth - GetTextSpriteWidth($"Vx {this.Vx,5}"), GetTextSpriteHeight("A")),
                    Color = Color.Cyan,
                    FontId = "Monospace",
                    RotationOrScale = FontSize
                };
                MySprite VY = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = $"Vy " + $"{this.Vy,5}",
                    Position = TopLeft + new Vector2(ScreenWidth - GetTextSpriteWidth($"Vy {this.Vy,5}"), GetTextSpriteHeight("A") * 2),
                    Color = Color.Cyan,
                    FontId = "Monospace",
                    RotationOrScale = FontSize
                };
                MySprite VZ = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = $"Vz " + $"{this.Vz,5}",
                    Position = TopLeft + new Vector2(ScreenWidth - GetTextSpriteWidth($"Vz {this.Vz,5}"), GetTextSpriteHeight("A") * 3),
                    Color = Color.Cyan,
                    FontId = "Monospace",
                    RotationOrScale = FontSize
                };
                MySprite ALT = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = $"ALT " + $"{this.ALT,5}",
                    Position = TopLeft + new Vector2(ScreenWidth - GetTextSpriteWidth($"ALT {this.ALT,5}"), GetTextSpriteHeight("A") * 4),
                    Color = Color.Cyan,
                    FontId = "Monospace",
                    RotationOrScale = FontSize
                };
                
                MySprite GangwayStatus = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = $"GANGWAY: {this.GangwayStatus}",
                    Position = TopLeft + new Vector2(0, 0),
                    Color = Color.Cyan,
                    FontId = "Monospace",
                    RotationOrScale = FontSize
                };
                MySprite RampStatus = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = $"RAMP: {this.RampStatus}",
                    Position = TopLeft + new Vector2(0, GetTextSpriteHeight("A")),
                    Color = Color.Cyan,
                    FontId = "Monospace",
                    RotationOrScale = FontSize
                };
                MySprite StingerStatus = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = $"STINGER: {this.StingerStatus}",
                    Position = TopLeft + new Vector2(0, GetTextSpriteHeight("A") * 2),
                    Color = Color.Cyan,
                    FontId = "Monospace",
                    RotationOrScale = FontSize
                };
                MySprite CCStatus = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = $"CC: {this.CCStatus}",
                    Position = TopLeft + new Vector2(0, GetTextSpriteHeight("A") * 4),
                    Color = Color.Cyan,
                    FontId = "Monospace",
                    RotationOrScale = FontSize
                };
                MySprite ATTStatus = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = $"ATT: {this.ATTStatus}",
                    Position = TopLeft + new Vector2(0, GetTextSpriteHeight("A") * 5),
                    Color = Color.Cyan,
                    FontId = "Monospace",
                    RotationOrScale = FontSize
                };
                MySprite TOLStatus = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = $"{this.TOLStatus}",
                    Position = TopLeft + new Vector2(0, GetTextSpriteHeight("A") * 7),
                    Color = Color.Cyan,
                    FontId = "Monospace",
                    RotationOrScale = FontSize
                };

                frame.Add(background);
                frame.Add(VT);
                frame.Add(VX);
                frame.Add(VY);
                frame.Add(VZ);
                frame.Add(ALT);
                frame.Add(TOLStatus);
                frame.Add(GangwayStatus);
                frame.Add(RampStatus);
                frame.Add(StingerStatus);
                frame.Add(CCStatus);
                frame.Add(ATTStatus);
            }
        }
    }
}
