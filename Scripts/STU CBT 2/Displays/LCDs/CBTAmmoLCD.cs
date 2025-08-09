using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript {
    partial class Program {
        public partial class CBTAmmoLCD : STUDisplay {
            public static Action<string> echo;
            public enum TurretStatus
            {
                Off,
                Ready,
                Engaged,
                NoAmmo,
                Destroyed
            }
            public TurretStatus GatlingOverallStatus { get; set; }
            public TurretStatus AssaultCannonOverallStatus { get; set; }

            public enum FixedGunStatus
            {
                Off,
                Ready,
                Reloading,
                NoAmmo,
                Destroyed
            }
            public FixedGunStatus RailgunOverallStatus { get; set; }
            public FixedGunStatus ArtilleryOverallStatus { get; set; }

            public CBTAmmoLCD(Action<string> Echo, IMyTerminalBlock block, int displayIndex, string font = "Monospace", float fontSize = 1) : base(block, displayIndex, font, fontSize) {
                echo = Echo;
            }

            int GatlingAmmo;
            int AssaultAmmo;
            int RailgunAmmo;
            int ArtilleryAmmo;

            public void LoadAmmoData(int gatlingAmmo, int assaultAmmo, int railgunAmmo, int artilleryAmmo) {
                GatlingAmmo = gatlingAmmo;
                AssaultAmmo = assaultAmmo;
                RailgunAmmo = railgunAmmo;
                ArtilleryAmmo = artilleryAmmo;
            }

            public void BuildScreen(MySpriteDrawFrame frame, Vector2 centerPos, float scale = 1f) {
                MySprite background = new MySprite() {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Position = TopLeft + new Vector2(0, ScreenHeight / 2),
                    Size = new Vector2(ScreenWidth, ScreenHeight),
                    Color = new Color(0, 0, 0)
                };
                MySprite horizontal_line = new MySprite() {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Position = TopLeft + new Vector2(0, ScreenHeight / 2 + 1),
                    Size = new Vector2(ScreenWidth, 2),
                    Color = new Color(64, 64, 64)
                };
                MySprite vertical_line_1 = new MySprite() {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Position = TopLeft + new Vector2(ScreenWidth / 3 - 1, 0),
                    Size = new Vector2(2, ScreenHeight),
                    Color = new Color(64, 64, 64)
                };
                MySprite vertical_line_2 = new MySprite() {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Position = TopLeft + new Vector2(ScreenWidth / 3 * 2 - 1, 0),
                    Size = new Vector2(2, ScreenHeight),
                    Color = new Color(64, 64, 64)
                };
                MySprite vertical_line_3 = new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Position = TopLeft + new Vector2(ScreenWidth / 3 * 2 - 1, ScreenHeight / 4 * 3),
                    Size = new Vector2(2, ScreenHeight / 2),
                    Color = new Color(64, 64, 64)
                };
                MySprite GAT_text = new MySprite() {
                    Type = SpriteType.TEXT,
                    Data = "GAT",
                    Position = TopLeft + new Vector2(ScreenWidth / 6 - GetTextSpriteWidth("GAT") / 2, 1),
                    Color = GatlingColor(),
                    FontId = "Monospace",
                    RotationOrScale = 1
                };
                MySprite GAT_ammo_amount = new MySprite() {
                    Type = SpriteType.TEXT,
                    Data = GatlingAmmo.ToString(),
                    Position = TopLeft + new Vector2(ScreenWidth / 6 - GetTextSpriteWidth(GatlingAmmo.ToString()) / 2, ScreenHeight / 4 + 1),
                    Color = GatlingColor(),
                    FontId = "Monospace",
                    RotationOrScale = 1
                };
                MySprite AST_text = new MySprite() {
                    Type = SpriteType.TEXT,
                    Data = "AST",
                    Position = TopLeft + new Vector2(ScreenWidth / 2 - GetTextSpriteWidth("AST") / 2, 1),
                    Color = AssaultCannonColor(),
                    FontId = "Monospace",
                    RotationOrScale = 1
                };
                MySprite AST_ammo_amount = new MySprite() {
                    Type = SpriteType.TEXT,
                    Data = AssaultAmmo.ToString(),
                    Position = TopLeft + new Vector2(ScreenWidth / 2 - GetTextSpriteWidth(AssaultAmmo.ToString()) / 2, ScreenHeight / 4 + 1),
                    Color = AssaultCannonColor(),
                    FontId = "Monospace",
                    RotationOrScale = 1
                };
                MySprite RG_text = new MySprite() {
                    Type = SpriteType.TEXT,
                    Data = "RG",
                    Position = TopLeft + new Vector2(ScreenWidth / 6 * 5 - GetTextSpriteWidth("RG") / 2, 1),
                    Color = RailgunColor(),
                    FontId = "Monospace",
                    RotationOrScale = 1
                };
                MySprite RG_ammo_amount = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = RailgunAmmo.ToString(),
                    Position = TopLeft + new Vector2(ScreenWidth / 6 * 5 - GetTextSpriteWidth(RailgunAmmo.ToString()) / 2, ScreenHeight / 4 + 1),
                    Color = RailgunColor(),
                    FontId = "Monospace",
                    RotationOrScale = 1
                };
                MySprite ART_text = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = "ART",
                    Position = TopLeft + new Vector2(ScreenWidth / 6 * 5 - GetTextSpriteWidth("ART") / 2, ScreenHeight / 4 * 2 + 5),
                    Color = ArtilleryColor(),
                    FontId = "Monospace",
                    RotationOrScale = 1
                };
                MySprite ART_value = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = ArtilleryAmmo.ToString(),
                    Position = TopLeft + new Vector2(ScreenWidth / 6 * 5 - GetTextSpriteWidth(ArtilleryAmmo.ToString()) / 2, ScreenHeight / 4 * 3 + 1),
                    Color = ArtilleryColor(),
                    FontId = "Monospace",
                    RotationOrScale = 1
                };
                MySprite CC_text_and_value = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = $"CC {CBT.CruiseControlSpeed}",
                    Position = TopLeft + new Vector2(ScreenWidth / 6 - GetTextSpriteWidth("CC") / 2, ScreenHeight / 4 * 2 + 5),
                    Color = CruiseControlColor(),
                    FontId = "Monospace",
                    RotationOrScale = 1
                };
                MySprite ATT_text = new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = $"ATT {BoolConverter(CBT.AttitudeControlActivated)}",
                    Position = TopLeft + new Vector2(ScreenWidth / 6 - GetTextSpriteWidth("AT") / 2, ScreenHeight / 4 * 3 + 1),
                    Color = AttitudeControlColor(),
                    FontId = "Monospace",
                    RotationOrScale = 1
                };
                

                frame.Add(background);
                frame.Add(horizontal_line);
                frame.Add(vertical_line_1);
                frame.Add(vertical_line_2);
                frame.Add(vertical_line_3);
                frame.Add(GAT_text);
                frame.Add(GAT_ammo_amount);
                frame.Add(AST_text);
                frame.Add(AST_ammo_amount);
                frame.Add(RG_text);
                frame.Add(RG_ammo_amount);
                frame.Add(ART_text);
                frame.Add(ART_value);
                frame.Add(CC_text_and_value);
                frame.Add(ATT_text);

            }

            public static Color RailgunColor()
            {
                try // if weapons get destroyed, the reference to them will become null
                {
                    List<int> enabled = new List<int>();
                    List<float> rechargeTimes = new List<float>();
                    foreach (var rg in CBT.Railguns)
                    {
                        switch (rg.Enabled)
                        {
                            case true: enabled.Add(1); break;
                            case false: enabled.Add(0); break;
                        }
                        rechargeTimes.Add(CBT.GetRailgunRechargeTimeLeft(rg));
                    }

                    if (enabled.Sum() == 0) { return new Color(64, 64, 64); }
                    if (rechargeTimes.Min() != 0) { return Color.Blue; }
                }
                catch
                {
                    return Color.Red;
                }
                var inventory = CBT.InventoryEnumerator.MostRecentItemTotals;
                if (!inventory.ContainsKey("Large Railgun Sabot")) { return Color.Yellow; }
                else return Color.White;
            }

            public static Color ArtilleryColor()
            {
                try // if weapons get destroyed, the reference to them will become null
                {
                    List<int> enabled = new List<int>();
                    foreach (var artilleryCanon in CBT.ArtilleryCannons)
                    {
                        switch(artilleryCanon.Enabled)
                        {
                            case true: enabled.Add(1); break;
                            case false: enabled.Add(0); break;
                        }
                    }
                    if(enabled.Sum() == 0) { return new Color(64, 64, 64); }
                }
                catch
                {
                    return Color.Red;
                }
                var inventory = CBT.InventoryEnumerator.MostRecentItemTotals;
                if (!inventory.ContainsKey("Artillery Shell")) { return Color.Yellow; }
                else return Color.White;
            }

            public static Color AssaultCannonColor()
            {
                try
                {
                    foreach (var assaultCannon in CBT.AssaultCannons)
                    {
                        if (!assaultCannon.Enabled) return new Color(64, 64, 64);
                        else if (assaultCannon.IsShooting) return Color.Green;
                        continue;
                    }
                }
                catch
                {
                    return Color.Red;
                }
                var inventory = CBT.InventoryEnumerator.MostRecentItemTotals;
                if (!inventory.ContainsKey("Assault Cannon Shell")) return Color.Yellow;
                else return Color.White;
            }

            public static Color GatlingColor()
            {
                try
                {
                    foreach (var gatlingTurret in CBT.GatlingTurrets)
                    {
                        if (!gatlingTurret.Enabled) return new Color(64, 64, 64);
                        else if (gatlingTurret.IsShooting) return Color.Green;
                        continue;
                    }
                }
                catch {  return Color.Red; }
                var inventory = CBT.InventoryEnumerator.MostRecentItemTotals;
                if (!inventory.ContainsKey("Gatling Ammo Box")) return Color.Yellow;
                else return Color.White;
            }

            public static Color CruiseControlColor()
            {
                if (CBT.CruiseControlActivated) { return new Color(0, 255, 0); }
                else { return new Color(127, 127, 127); }
            }

            public static Color AttitudeControlColor()
            {
                if (CBT.AttitudeControlActivated) { return new Color(0, 255, 0); }
                else { return new Color(127, 127, 127); }
            }
        }
    }
}
