using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript {
    partial class Program {
        public partial class CBTStatusLCD : STUDisplay {
            public static Action<string> echo;

            public float CharWidth { get; set; }
            public float CharHeight { get; set; }
            public float FontSize { get; set; }
            
            public CBTStatusLCD(Action<string> Echo, IMyTerminalBlock block, int displayIndex, string font = "Monospace", float fontSize = 1) : base(block, displayIndex, font, fontSize) {
                echo = Echo;
                CharWidth = GetTextSpriteWidth("A") * fontSize;
                CharHeight = GetTextSpriteHeight("A") * fontSize;
                FontSize = fontSize;
            }

            public Color GetGearStatusColor()
            {
                // grey: disconnected, not in range
                // yellow: disconnected, in range
                // green: connected
                // red: landing gear damaged
                // black: default color, if the primary landing gear is somehow not in one of the three Keen-defined states

                // this loop finds the landing gear with the most advanced state, and uses that to represent the state of them all. 
                // e.g. if just one landing gear is in range, the display will show yellow. similarly, if just one of the landing gears is locked, the display will show green.
                LandingGearMode landingGearMode = LandingGearMode.Unlocked;
                for (int i = 0; i < CBT.LandingGear.Length; i++)
                {
                    if (!CBT.LandingGear[i].IsFunctional) return Color.Red; // not quite sure if IsFunctional is the right property, too lazy to ask GPT rn...
                    else if (CBT.LandingGear[i].LockMode == LandingGearMode.Locked) { landingGearMode = LandingGearMode.Locked; continue; }
                    else if (CBT.LandingGear[i].LockMode == LandingGearMode.ReadyToLock) { landingGearMode = LandingGearMode.ReadyToLock; continue; }
                }
                switch (landingGearMode)
                {
                    case LandingGearMode.Unlocked: return new Color(64, 64, 64);
                    case LandingGearMode.ReadyToLock: return Color.Yellow;
                    case LandingGearMode.Locked: return Color.Green;
                    default: return Color.Black;
                }
            }

            public Color GetConnectorStatusColor()
            {
                // grey: disconnected, not in range
                // yellow: disconnected, in range
                // green: connected
                // red: damaged
                // black: default if somehow the checks fail
                if (!CBT.Connector.IsFunctional) return Color.Red;
                switch (CBT.Connector.Status)
                {
                    case MyShipConnectorStatus.Unconnected: return new Color(64, 64, 64);
                    case MyShipConnectorStatus.Connectable: return Color.Yellow;
                    case MyShipConnectorStatus.Connected: return Color.Green;
                    default: return Color.Black;
                }
            }

            public Color GetGangwayStatusColor()
            {
                switch (CBT.Gangway.CurrentGangwayState)
                {
                    case CBTGangway.GangwayStates.Unknown: return Color.Red;
                    case CBTGangway.GangwayStates.Frozen: return Color.Red;
                    case CBTGangway.GangwayStates.Resetting: return Color.Cyan;
                    case CBTGangway.GangwayStates.Extended: return Color.Green;
                    case CBTGangway.GangwayStates.Retracted: return new Color(64, 64, 64) ;
                    case CBTGangway.GangwayStates.Extending: return Color.Yellow;
                    case CBTGangway.GangwayStates.Retracting: return Color.Yellow;
                    default: return Color.Black;
                }
            }
            public string GetGangwayStatusString()
            {
                switch (CBT.Gangway.CurrentGangwayState)
                {
                    case CBTGangway.GangwayStates.Unknown: return "U";
                    case CBTGangway.GangwayStates.Frozen: return "F";
                    case CBTGangway.GangwayStates.Resetting: return "R";
                    case CBTGangway.GangwayStates.Extended: return "O";
                    case CBTGangway.GangwayStates.Retracted: return "C";
                    case CBTGangway.GangwayStates.Extending: return "M";
                    case CBTGangway.GangwayStates.Retracting: return "M";
                    default: return "X";
                }
            }

            public Color GetRampStatusColor()
            {
                float angle = CBT.RadToDeg(CBT.HangarRotor.Angle);

                if (CBT.AngleCloseEnoughDegrees(angle, 0)) return new Color(64, 64, 64) ;
                else if (CBT.AngleRangeCloseEnoughDegrees(angle, 90, 110)) return Color.Green;
                else if (CBT.AngleRangeCloseEnoughDegrees(angle, 0, 89)) return Color.Yellow;
                else return Color.Red;
            }
            public string GetRampStatusString()
            {
                float angle = CBT.RadToDeg(CBT.HangarRotor.Angle);
                
                if (CBT.AngleCloseEnoughDegrees(angle, 0)) return "C";
                else if (CBT.AngleRangeCloseEnoughDegrees(angle, 90, 110)) return "O";
                else if (CBT.AngleRangeCloseEnoughDegrees(angle, 0, 90)) return "M";
                else return "X";
            }

            public Color GetGravityGeneratorStatusColor()
            {
                switch (CBT.GravityGenerator.IsWorking)
                {
                    case true: return Color.Green;
                    case false: return new Color(64, 64, 64);
                    default: return Color.Black;
                }
            }

            public Color GetMergeBlockStatusColor()
            {
                switch (CBT.MergeBlock.State)
                {
                    case MergeState.None: return new Color(64, 64, 64);
                    case MergeState.Working: return Color.Yellow;
                    case MergeState.Locked: return Color.Green;
                    default: return Color.Black;
                }
            }

            public Color GetRadioTransmissionStatusColor()
            {
                switch (CBT.Antenna.IsWorking)
                {
                    case true: return Color.Green;
                    case false: return new Color(64, 64, 64);
                    default: return Color.Black;
                }
            }

            public Color GetVentsStatusColor()
            {
                // see limitations described above in the comments of GetGearStatusColor()
                VentStatus ventStatus = VentStatus.Depressurized;
                for (int i = 0; i < CBT.AirVents.Length; i++)
                {
                    if (!CBT.AirVents[i].IsWorking) return new Color(64, 64, 64);
                    if (CBT.AirVents[i].Status == VentStatus.Pressurized || CBT.AirVents[i].GetOxygenLevel() > 0.99) { ventStatus = VentStatus.Pressurized; continue; }
                    else if (CBT.AirVents[i].Status == VentStatus.Pressurizing) { ventStatus = VentStatus.Pressurizing; continue; }
                    else if (CBT.AirVents[i].Status == VentStatus.Depressurizing) { ventStatus = VentStatus.Depressurizing; continue; }
                    else if (CBT.AirVents[i].Status == VentStatus.Depressurized) { ventStatus = VentStatus.Depressurized; continue; }
                }
                switch (ventStatus)
                {
                    case VentStatus.Pressurized: return Color.Green;
                    case VentStatus.Pressurizing: return Color.Yellow;
                    case VentStatus.Depressurized: return Color.Red;
                    case VentStatus.Depressurizing: return Color.Blue;
                    default: return Color.Black;
                }
            }

            public Color GetHydrogenStatusColor()
            {
                int hydrogen = CBT.GetHydrogenPercentFilled();
                if (hydrogen > 33) { return Color.White; }
                else if (hydrogen > 0 && hydrogen <= 33) { return Color.Yellow; }
                else if (hydrogen == 0) { return Color.Red; }
                else return Color.Black;
            }
            public string GetHydrogenStatusString()
            {
                int hydrogen = CBT.GetHydrogenPercentFilled();
                if (hydrogen == 100) { return "F"; }
                else { return hydrogen.ToString() + "%"; }
            }

            public Color GetHydrogenEngineStatusColor()
            {
                for (int i = 0; i < CBT.HydrogenEngines.Length; i++)
                {
                    if (CBT.HydrogenEngines[i].Enabled == true) { return Color.Green; }
                }
                return new Color(64, 64, 64);
            }

            public Color GetOxygenStatusColor()
            {
                int oxygen = CBT.GetOxygenPercentFilled();
                if (oxygen > 33) { return Color.White; }
                else if (oxygen > 0 && oxygen <= 33) { return Color.Yellow; }
                else if (oxygen == 0) { return Color.Red; }
                else return Color.Black;
            }
            public string GetOxygenStatusString()
            {
                int oxygen = CBT.GetOxygenPercentFilled();
                if (oxygen == 100) { return "F"; }
                else { return oxygen.ToString() + "%"; }
            }

            public Color GetMedBayStatusColor()
            {
                if (CBT.MedicalRoom.Enabled) return Color.Green;
                else return new Color(64, 64, 64);
            }

            public Color GetBatteryStatusColor()
            {
                // if any of the batteries are in recharge mode, change the color to cyan
                for (int i = 0; i < CBT.Batteries.Length; i++)
                {
                    if (CBT.Batteries[i].ChargeMode == ChargeMode.Recharge) { return Color.Cyan; }
                }
                int powerLevel = CBT.GetPowerPercent();
                if (powerLevel > 33) return Color.White;
                else if (powerLevel > 0 && powerLevel <= 33) return Color.Yellow;
                else if (powerLevel == 0) return Color.Red;
                else return Color.Black;
            }
            public string GetBatteryStatusString()
            {
                int powerLevel = CBT.GetPowerPercent();
                if (powerLevel == 100) return "F";
                else return powerLevel.ToString() + "%";
            }


            public void BuildScreen(MySpriteDrawFrame frame, Vector2 centerPos, float scale = 1f) {
                MySprite background = new MySprite() {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Position = TopLeft + new Vector2(0, ScreenHeight / 2),
                    Size = new Vector2(ScreenWidth, ScreenHeight),
                    Color = new Color(0, 0, 0)
                };
                MySprite GEAR = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = "GEAR",
                    Position = TopLeft + new Vector2(0, 0),
                    Color = GetGearStatusColor(),
                    FontId = "Monospace",
                    RotationOrScale = FontSize
                };
                MySprite CONNECTOR = new MySprite
                {
                    Type = SpriteType.TEXT,
                    Data = "CONN",
                    Position = TopLeft + new Vector2(ScreenWidth / 2 + 1, 0),
                    Color = GetConnectorStatusColor(),
                    FontId = "Monospace",
                    RotationOrScale = FontSize
                };
                MySprite GANGWAY = new MySprite
                {
                    Type = SpriteType.TEXT,
                    Data = $"GWAY:{GetGangwayStatusString()}",
                    Position = TopLeft + new Vector2(0, CharHeight),
                    Color = GetGangwayStatusColor(),
                    FontId = "Monospace",
                    RotationOrScale = FontSize
                };
                MySprite RAMP = new MySprite
                {
                    Type = SpriteType.TEXT,
                    Data = $"RAMP:{GetRampStatusString()}",
                    Position = TopLeft + new Vector2(ScreenWidth / 2 + 1, CharHeight),
                    Color = GetRampStatusColor(),
                    FontId = "Monospace",
                    RotationOrScale = FontSize
                };
                MySprite GRAVITY_GENERATOR = new MySprite
                {
                    Type = SpriteType.TEXT,
                    Data = "GGEN",
                    Position = TopLeft + new Vector2(0, CharHeight * 2),
                    Color = GetGravityGeneratorStatusColor(),
                    FontId = "Monospace",
                    RotationOrScale = FontSize
                };
                MySprite MERGE = new MySprite
                {
                    Type = SpriteType.TEXT,
                    Data = "MERGE",
                    Position = TopLeft + new Vector2(ScreenWidth / 2 + 1, CharHeight * 2),
                    Color = GetMergeBlockStatusColor(),
                    FontId = "Monospace",
                    RotationOrScale = FontSize
                };
                MySprite ANTENNA = new MySprite
                {
                    Type = SpriteType.TEXT,
                    Data = "TX/RX",
                    Position = TopLeft + new Vector2(0, CharHeight * 3),
                    Color = GetRadioTransmissionStatusColor(),
                    FontId = "Monospace",
                    RotationOrScale = FontSize
                };
                MySprite VENTS = new MySprite
                {
                    Type = SpriteType.TEXT,
                    Data = "VENTS",
                    Position = TopLeft + new Vector2(ScreenWidth / 2 + 1, CharHeight * 3),
                    Color = GetVentsStatusColor(),
                    FontId = "Monospace",
                    RotationOrScale = FontSize
                };
                MySprite FUEL = new MySprite
                {
                    Type = SpriteType.TEXT,
                    Data = $"H2:{GetHydrogenStatusString()}",
                    Position = TopLeft + new Vector2(0, CharHeight * 4),
                    Color = GetHydrogenStatusColor(),
                    FontId = "Monospace",
                    RotationOrScale = FontSize
                };
                MySprite ENGINES = new MySprite
                {
                    Type = SpriteType.TEXT,
                    Data = "PGEN",
                    Position = TopLeft + new Vector2(ScreenWidth / 2 + 1, CharHeight * 4),
                    Color = GetHydrogenEngineStatusColor(),
                    FontId = "Monospace",
                    RotationOrScale = FontSize
                };
                MySprite OXYGEN = new MySprite
                {
                    Type = SpriteType.TEXT,
                    Data = $"O2:{GetOxygenStatusString()}",
                    Position = TopLeft + new Vector2(0, CharHeight * 5),
                    Color = GetOxygenStatusColor(),
                    FontId = "Monospace",
                    RotationOrScale = FontSize
                };
                MySprite MED = new MySprite
                {
                    Type = SpriteType.TEXT,
                    Data = "MED",
                    Position = TopLeft + new Vector2(ScreenWidth / 2 + 1, CharHeight * 5),
                    Color = GetMedBayStatusColor(),
                    FontId = "Monospace",
                    RotationOrScale = FontSize
                };
                MySprite ELECTRICITY = new MySprite
                {
                    Type = SpriteType.TEXT,
                    Data = $"kW:{GetBatteryStatusString()}",
                    Position = TopLeft + new Vector2(0, CharHeight * 6),
                    Color = GetBatteryStatusColor(),
                    FontId = "Monospace",
                    RotationOrScale = FontSize
                };
                MySprite POWER_LEVEL = new MySprite
                {
                    Type = SpriteType.TEXT,
                    Data = $"PL:{CBT.PowerLevel}",
                    Position = TopLeft + new Vector2(ScreenWidth / 2 + 1, CharHeight * 6),
                    Color = Color.White,
                    FontId = "Monospace",
                    RotationOrScale = FontSize
                };


                frame.Add(background);
                frame.Add(GEAR);
                frame.Add(CONNECTOR);
                frame.Add(GANGWAY);
                frame.Add(RAMP);
                frame.Add(GRAVITY_GENERATOR);
                frame.Add(MERGE);
                frame.Add(ANTENNA);
                frame.Add(VENTS);
                frame.Add(FUEL);
                frame.Add(ENGINES);
                frame.Add(OXYGEN);
                frame.Add(MED);
                frame.Add(ELECTRICITY);
                frame.Add(POWER_LEVEL);

            }

        }
    }
}
