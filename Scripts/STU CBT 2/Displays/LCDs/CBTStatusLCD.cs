using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript {
    partial class Program {
        public partial class CBTStatusLCD : STUDisplay {
            static Action<string> Echo { get; set; }

            public float CharWidth { get; set; }
            public float CharHeight { get; set; }
            public float FontSize { get; set; }
            
            public CBTStatusLCD(Action<string> echo, IMyTerminalBlock block, int displayIndex, string font = "Monospace", float fontSize = 1) : base(block, displayIndex, font, fontSize) {
                Echo = echo;
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
                if (!CBT.GangwayHinge1.Enabled || !CBT.GangwayHinge2.Enabled) return new Color(64, 64, 64);

                switch (CBT.Gangway.CurrentGangwayState)
                {
                    case CBTGangway.GangwayStates.Unknown: return Color.Red;
                    case CBTGangway.GangwayStates.Frozen: return Color.Red;
                    case CBTGangway.GangwayStates.Resetting: return Color.Cyan;
                    case CBTGangway.GangwayStates.Extended: return Color.Green;
                    case CBTGangway.GangwayStates.Retracted: return Color.Green;
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

                frame.Add(background);
                frame.Add(BuildTextSprite("GEAR", 0,0, GetGearStatusColor()));
                frame.Add(BuildTextSprite("CONN", ScreenWidth / 2 + 1, 0, GetConnectorStatusColor()));
                frame.Add(BuildTextSprite($"GWAY:{GetGangwayStatusString()}", 0, CharHeight, GetGangwayStatusColor()));
                frame.Add(BuildTextSprite($"RAMP:{GetRampStatusString()}", ScreenWidth / 2 + 1, CharHeight, GetRampStatusColor()));
                frame.Add(BuildTextSprite("GGEN", 0, CharHeight * 2, GetGravityGeneratorStatusColor()));
                frame.Add(BuildTextSprite("TX/RX", 0, CharHeight * 3, GetRadioTransmissionStatusColor()));
                frame.Add(BuildTextSprite("VENTS", ScreenWidth / 2 + 1, CharHeight * 3, GetVentsStatusColor()));
                frame.Add(BuildTextSprite($"H2:{GetHydrogenStatusString()}", 0, CharHeight * 4, GetHydrogenStatusColor()));
                frame.Add(BuildTextSprite("PGEN", ScreenWidth / 2 + 1, CharHeight * 4, GetHydrogenEngineStatusColor()));
                frame.Add(BuildTextSprite($"O2:{GetOxygenStatusString()}", 0, CharHeight * 5, GetOxygenStatusColor()));
                frame.Add(BuildTextSprite("MED", ScreenWidth / 2 + 1, CharHeight * 5, GetMedBayStatusColor()));
                frame.Add(BuildTextSprite($"kW:{GetBatteryStatusString()}", 0, CharHeight * 6, GetBatteryStatusColor()));
                frame.Add(BuildTextSprite($"PL:{CBT.PowerLevel}", ScreenWidth / 2 + 1, CharHeight * 6, Color.White));

            }

        }
    }
}
