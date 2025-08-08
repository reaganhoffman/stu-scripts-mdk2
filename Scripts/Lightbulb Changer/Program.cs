using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    public partial class Program : MyGridProgram
    {

        public List<IMyLightingBlock> AllLights { get; set; } = new List<IMyLightingBlock>();
        public struct LightbulbColor
        {
            public int R;
            public int G;
            public int B;

            public LightbulbColor(int r, int g, int b)
            {
                R = r;
                G = g;
                B = b;
            }
        }
        public Dictionary<string, LightbulbColor> LightbulbColors { get; set; } = new Dictionary<string, LightbulbColor>();

        public Program()
        {
            LightbulbColors.Add("HPS", new LightbulbColor(255,183,76));
            LightbulbColors.Add("LPS", new LightbulbColor(255, 209, 178));
            LightbulbColors.Add("CFL", new LightbulbColor(255, 244, 229));
            LightbulbColors.Add("MVL", new LightbulbColor(216, 247, 255));
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // get a list of all lights on the grid
            GridTerminalSystem.GetBlocksOfType<IMyLightingBlock>(AllLights);
            foreach (var light in AllLights)
            {
                // read the light's custom data, then change its color
                string[] customData = light.CustomData.Split(new[] { '\n' });
                foreach (var item in customData)
                {
                    string line = item.Trim().ToUpper();
                    if (LightbulbColors.ContainsKey(line))
                    {
                        SetLightbulbColor(light, LightbulbColors[line]);
                    }
                    // will necessarily get the last valid light code in the entire custom data field
                }
            }
        }

        public void SetLightbulbColor(IMyLightingBlock lightbulb, LightbulbColor color)
        {
            lightbulb.Color = new Color(color.R, color.G, color.B); // implicitly casts ints to floats
        }
    }
}
