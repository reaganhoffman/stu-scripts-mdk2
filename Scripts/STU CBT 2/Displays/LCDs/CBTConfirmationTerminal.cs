using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public partial class CBTConfirmationTerminal : STUDisplay
        {
            public static Action<string> echo;

            string CurrentManeuverName;
            STUStateMachine.InternalStates CurrentManeuverPhase;
            float FontSize;
            StringAndColor MultilineMessage;

            struct StringAndColor
            {
                public string Text;
                public Color Color;

                public StringAndColor(string text, Color color)
                {
                    Text = text;
                    Color = color;
                }

            }


            public CBTConfirmationTerminal(Action<string> Echo, IMyTerminalBlock block, int displayIndex, string font = "Monospace", float fontSize = 1) : base(block, displayIndex, font, fontSize)
            {
                echo = Echo;
                FontSize = fontSize;
            }

            public void LoadManeuverData(ManeuverQueueData data)
            {
                CurrentManeuverName = data.CurrentManeuverName;
                if (data.CurrentManeuverInitStatus) CurrentManeuverPhase = STUStateMachine.InternalStates.Init;
                else if (data.CurrentManeuverRunStatus) CurrentManeuverPhase = STUStateMachine.InternalStates.Run;
                else if (data.CurrentManeuverCloseoutStatus) CurrentManeuverPhase = STUStateMachine.InternalStates.Closeout;
                else CurrentManeuverPhase = STUStateMachine.InternalStates.Done;
            }

            MySprite DrawBackground()
            {
                Color bgColor = Color.DarkCyan;
                // determine background color
                if (CurrentManeuverName == "Takeoff" || CurrentManeuverName == "Landing") // not applicable when we're not taking off or landing
                {
                    switch (CurrentManeuverPhase)
                    {
                        case STUStateMachine.InternalStates.Init:
                            bgColor = Color.Yellow;
                            break;
                        case STUStateMachine.InternalStates.Run:
                            bgColor = Color.Black;
                            break;
                        case STUStateMachine.InternalStates.Closeout:
                            bgColor = Color.Yellow;
                            break;
                        default:
                            // no need to define a case for the 'Done' phase, since I already assign a value to bgColor at the start of this block
                            break;
                    }
                }
                
                return new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Position = TopLeft + new Vector2(0, ScreenHeight / 2),
                    Size = new Vector2(ScreenWidth, ScreenHeight),
                    Color = bgColor
                };
            }

            StringAndColor BuildMultilineMessage()
            {
                string statusMessage = "";
                Color textColor = Color.White;
                // determine status message and color
                switch (CurrentManeuverName)
                {
                    case "Takeoff":
                        switch (CurrentManeuverPhase)
                        {
                            case STUStateMachine.InternalStates.Init:
                                statusMessage = "\nCONFIRM\nTAKEOFF?";
                                textColor = Color.Black;
                                break;
                            case STUStateMachine.InternalStates.Run:
                                statusMessage = "\nTAKEOFF\nSEQUENCE\nIN\nPROGRESS";
                                textColor = Color.Yellow;
                                break;
                            case STUStateMachine.InternalStates.Closeout:
                                statusMessage = "\nREADY FOR\nPILOT\nCONTROL\n\nCONFIRM?";
                                textColor = Color.Black;
                                break;
                            default:
                                // no need to define a case for the 'Done' phase, since I already assigned default values at the start of this block
                                break;
                        }
                        break;
                    case "Landing":
                        switch (CurrentManeuverPhase)
                        {
                            case STUStateMachine.InternalStates.Init:
                                statusMessage = "\n\nCONFIRM\nLANDING?";
                                textColor = Color.Black;
                                break;
                            case STUStateMachine.InternalStates.Run:
                                statusMessage = "\nLANDING\nSEQUENCE\nIN\nPROGRESS";
                                textColor = Color.Yellow;
                                break;
                            case STUStateMachine.InternalStates.Closeout:
                                statusMessage = "\nREADY\nTO\nLAND\n\nCONFIRM?";
                                textColor = Color.Black;
                                break;
                            default:
                                // no need to define a case for the 'Done' phase, since I already assigned default values at the start of this block
                                break;
                        }
                        break;
                    default:
                        break;
                }
                return new StringAndColor(statusMessage, textColor);
            }


            public void BuildScreen(MySpriteDrawFrame frame, Vector2 centerPos, float scale = 1f)
            {
                MultilineMessage = BuildMultilineMessage();
                frame.Add(DrawBackground());
                DrawCenteredMultilineString(MultilineMessage.Text, MultilineMessage.Color, frame);
            }
        }
    }
}