using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.WorldEnvironment.Modules;
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
        MyCommandLine CommandLineParser { get; set; } = new MyCommandLine();
        public struct trit
        {
            private sbyte value__;

            private trit(sbyte value = 0)
            {
                value__ = value;
            }

            public static trit down = new trit(-1);
            public static trit middle = new trit(0);
            public static trit up = new trit(1);

            public static implicit operator sbyte(trit t) => t.value__;
        }

        public Program()
        {
            
        }


        public void Main(string argument, UpdateType updateSource)
        {
            try
            {
                CommandLineParser.TryParse(argument);
                if (CommandLineParser.Argument(0).ToUpper() != "POWER") return;
                string userRequestPowerGroup = CommandLineParser.Argument(1);
                trit userRequestedState = CommandLineParser.Switch("e") | CommandLineParser.Switch("E");
                
            }
            catch
            {
                Echo("something went wrong");
            }
        }
    }
}
