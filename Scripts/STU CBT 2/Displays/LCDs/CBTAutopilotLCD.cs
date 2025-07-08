using Sandbox.ModAPI.Ingame;
using System;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript {
    partial class Program {
        public partial class CBTAutopilotLCD : STUDisplay {
            public static Action<string> echo;

            public CBTAutopilotLCD(Action<string> Echo, IMyTerminalBlock block, int displayIndex, string font = "Monospace", float fontSize = 1) : base(block, displayIndex, font, fontSize) {
                echo = Echo;
            }

            public Color GetThrustersStatusColorBG()
            {
                Color color = new Color();
                switch (CBT.FlightController.HasThrusterControl)
                {
                    case true: color = Color.White; break;
                    case false: color = Color.Black; break;
                }
                foreach (var thruster in CBT.Thrusters)
                {
                    if (!thruster.Enabled)
                    {
                        color = Color.Red; break;
                    }
                }
                return color;
            }
            public Color GetThrustersStatusColorText()
            {
                Color color = new Color();
                switch (CBT.FlightController.HasThrusterControl)
                {
                    case true: color = Color.Black; break;
                    case false: color = Color.White; break;
                }
                foreach (var thruster in CBT.Thrusters)
                {
                    if (!thruster.Enabled)
                    {
                        color = Color.Black; break;
                    }
                }
                return color;
            }

            public string GetThrustersStatus()
            {
                string state = "";
                switch (CBT.FlightController.HasThrusterControl)
                {
                    case true: state = "AUTO"; break;
                    case false: state = "MANU"; break;
                }
                foreach (var thruster in CBT.Thrusters)
                {
                    if (!thruster.Enabled)
                    {
                        state = "OFF";
                        break;
                    }
                }
                return state;
            }
            public Color GetGyrosStatusColorText()
            {
                Color color = new Color();
                switch (CBT.FlightController.HasGyroControl)
                {
                    case true: color = Color.Black; break;
                    case false: color = Color.White; break;
                }
                foreach (var gyro in CBT.Gyros)
                {
                    if (!gyro.Enabled)
                    {
                        color = Color.Black;
                        break;
                    }
                }
                return color;
            }
            public Color GetGyrosStatusColorBG()
            {
                Color color = new Color();
                switch(CBT.FlightController.HasGyroControl)
                {
                    case true: color = Color.White; break;
                    case false: color = Color.Black; break;
                }
                foreach (var gyro in CBT.Gyros)
                {
                    if (!gyro.Enabled)
                    {
                        color = Color.Red; 
                        break;
                    }
                }
                return color;
            }
            public string GetGyrosStatus()
            {
                string state = "";
                switch (CBT.FlightController.HasGyroControl)
                {
                    case true: state = "AUTO"; break;
                    case false: state = "MANU"; break;
                }
                foreach (var gyro in CBT.Gyros)
                {
                    if (!gyro.Enabled)
                    {
                        state = "OFF";
                        break;
                    }
                }
                return state;
            }

            public Color GetDampenersStatusColor()
            {
                if (CBT.FlightController.RemoteControl.DampenersOverride) return Color.Green;
                else return Color.Red;
            }

            public string GetDampenersStatus()
            {
                return BoolConverter(CBT.FlightController.RemoteControl.DampenersOverride);
            }

            public void DrawAutopilotStatus(MySpriteDrawFrame frame, Vector2 centerPos, float scale = 1f) {
                
                MySprite background_sprite = new MySprite() {
                    Type = SpriteType.TEXTURE,
                    Alignment = TextAlignment.CENTER,
                    Data = "SquareSimple",
                    Position = centerPos,
                    Size = new Vector2(ScreenWidth, ScreenHeight) * scale,
                    Color = Color.Black,
                    RotationOrScale = 0f
                };
                MySprite AUTOPILOT = new MySprite() {
                    Type = SpriteType.TEXT,
                    Alignment = TextAlignment.CENTER,
                    Data = "AUTOPILOT",
                    Position = new Vector2(0f, -105f) * scale + centerPos,
                    Color = new Color(255, 255, 255, 255),
                    FontId = "Debug",
                    RotationOrScale = scale
                };
                MySprite THRUSTERS = new MySprite() {
                    Type = SpriteType.TEXT,
                    Alignment = TextAlignment.CENTER,
                    Data = $"Thrusters {GetThrustersStatus()}",
                    Position = new Vector2(0f, -51f) * scale + centerPos,
                    Color = GetThrustersStatusColorText(),
                    FontId = "Debug",
                    RotationOrScale = scale
                };
                MySprite thrustersBG = new MySprite() {
                    Type = SpriteType.TEXTURE,
                    Alignment = TextAlignment.CENTER,
                    Data = "SquareSimple",
                    Position = new Vector2(0f, -36f) * scale + centerPos,
                    Size = new Vector2(ScreenWidth, 31f) * scale,
                    Color = GetThrustersStatusColorBG(),
                    RotationOrScale = 0f
                }; // Thrusters BG
                MySprite GYROS = new MySprite() {
                    Type = SpriteType.TEXT,
                    Alignment = TextAlignment.CENTER,
                    Data = $"Gyros {GetGyrosStatus()}",
                    Position = new Vector2(0f, -18f) * scale + centerPos,
                    Color = GetGyrosStatusColorText(),
                    FontId = "Debug",
                    RotationOrScale = scale
                };
                MySprite gyroBG = new MySprite() {
                    Type = SpriteType.TEXTURE,
                    Alignment = TextAlignment.CENTER,
                    Data = "SquareSimple",
                    Position = new Vector2(0f, 0f) * scale + centerPos,
                    Size = new Vector2(ScreenWidth, 31f) * scale,
                    Color = GetGyrosStatusColorBG(),
                    RotationOrScale = 0f
                }; // Gyro BG
                MySprite DAMPENERS = new MySprite() {
                    Type = SpriteType.TEXT,
                    Alignment = TextAlignment.CENTER,
                    Data = $"Dampeners {GetDampenersStatus()}",
                    Position = new Vector2(0f, 18f) * scale + centerPos,
                    Color = Color.Black,
                    FontId = "Debug",
                    RotationOrScale = scale
                };
                MySprite dampenersBG = new MySprite() {
                    Type = SpriteType.TEXTURE,
                    Alignment = TextAlignment.CENTER,
                    Data = "SquareSimple",
                    Position = new Vector2(0f, 36f) * scale + centerPos,
                    Size = new Vector2(ScreenWidth, 31f) * scale,
                    Color = GetDampenersStatusColor(),
                    RotationOrScale = 0f
                }; // Dampeners BG

                frame.Add(background_sprite);
                frame.Add(AUTOPILOT);
                frame.Add(thrustersBG);
                frame.Add(THRUSTERS);
                frame.Add(gyroBG);
                frame.Add(GYROS);
                frame.Add(dampenersBG);
                frame.Add(DAMPENERS);
            }
        }
    }
}
