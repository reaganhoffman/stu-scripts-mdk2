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

            public float CharHeight { get; set; }
            public float FontSize { get; set; }
            
            readonly Color cl = Color.Cyan;

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
                RampStatus = "NOT CLOSED";
                if (CBT.AngleCloseEnoughDegrees(CBT.RadToDeg(CBT.HangarRotor.Angle), 0))
                {
                    RampStatus = "CLOSED";
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


                frame.Add(background);
                frame.Add(BuildTextSprite($"Vt " + $"{Vt,5}", ScreenWidth - GetTextSpriteWidth($"Vt {Vt,5}"), 0, cl));
                frame.Add(BuildTextSprite($"Vx " + $"{Vx,5}", ScreenWidth - GetTextSpriteWidth($"Vx {Vx,5}"), CharHeight, cl));
                frame.Add(BuildTextSprite($"Vy " + $"{Vy,5}", ScreenWidth - GetTextSpriteWidth($"Vy {Vy,5}"), CharHeight * 2, cl));
                frame.Add(BuildTextSprite($"Vz " + $"{Vz,5}", ScreenWidth - GetTextSpriteWidth($"Vz {Vz,5}"), CharHeight * 3, cl));
                frame.Add(BuildTextSprite($"ALT " + $"{ALT,5}", ScreenWidth - GetTextSpriteWidth($"ALT {ALT,5}"), CharHeight * 4, cl));
                frame.Add(BuildTextSprite($"{TOLStatus}", 0, CharHeight * 7, cl));
                frame.Add(BuildTextSprite($"GANGWAY: {GangwayStatus}", 0, 0, cl));
                frame.Add(BuildTextSprite($"RAMP: {RampStatus}", 0, CharHeight, cl));
                frame.Add(BuildTextSprite($"STINGER: {StingerStatus}", 0, CharHeight * 2, cl));
                frame.Add(BuildTextSprite($"CC: {CCStatus}", 0, CharHeight * 4, cl));
                frame.Add(BuildTextSprite($"ATT: {ATTStatus}", 0, CharHeight * 5, cl));
            }
        }
    }
}
