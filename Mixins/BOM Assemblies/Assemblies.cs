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
    partial class Program
    {
        public class Assemblies
        {
            public static Dictionary<string, int> LIGMA_MK_1 { get; set; } = new Dictionary<string, int>()
            {
                // armor blocks: 67
                // large hydrogen thruster: 1
                // conveyor converter: 1
                // small conveyor: 9
                // small hydrogen tank: 12
                // small battery: 6
                // small reinforced conveyor tube T junction: 6
                // small reinforced curved conveyor tube: 8
                // small connector: 1
                // hydrogen thruster: 10
                // small reinforced conveyor tube: 4
                // letter l: 1
                // letter i: 2
                // letter g: 1
                // letter m: 1
                // letter a: 1
                // number 1: 1
                // antenna: 1
                // remote control: 1
                // gyroscope: 1
                // light armor panel: 177
                // top mounted camera: 1
                // small merge block: 1
                // automaton sensor: 1
                // programmable block: 1
                
                {$"{STUInventoryEnumerator.c}/SteelPlate", 67 + 30 + 3*12 + 4*6 + 2*6 + 2*8 + 7 + 7*10 + 2*8 + 7 + 2 + 25 + 177 + 2 + 2 + 2 + 2}, // armor blocks, lrg hydro thruster, sm hyd tank (12), sm bat(6), rein conv t(6), rein conv el(8), sm conn, thrust(10), rein conv tube (8), letters (1ea*7), antenna, gyro, light armor panels, camera, merge, sensor, pb
                {$"{STUInventoryEnumerator.c}/InteriorPlate", 6 + 4*9 + 2*6 + 1*8 + 1*8 + 5}, // conv converter, sml convr (9), rein conv t(6), rein conv el(8), rein conv tube (8), sensor
                {$"{STUInventoryEnumerator.c}/Construction", 30 + 8 + 4*9 + 2*12 + 2*6 + 2*6 + 1*8 + 4 + 15*10 + 1*8 + 1 + 5 + 3 + 5 + 2}, // large hyd thrstr, conv converter, sml convr (9), sm hyd tank (12), sm bat(6), rein conv t(6), rein conv el(8), sm conn, thrust(10), rein conv tube (8), antenna, gyro, merge, sensor, pb
                {$"{STUInventoryEnumerator.c}/MetalGrid", 30 + 4*10}, // lrg hyd thrst, thrust (10), 
                {$"{STUInventoryEnumerator.c}/Motor", 2 + 1*9 + 1*6 + 1*8 + 1 + 1*8 + 2 + 1 + 1}, // conv converter, sml convr (9), rein conv t(6), rein conv el(8), sm conn, rein conv tube (8), gyro, merge, pb
                {$"{STUInventoryEnumerator.c}/Computer", 4*12 + 2*6 + 1 + 1 + 3 + 3 + 1 + 6 + 2}, // sm hyd tank (12), sm bat(6), sm conn, antenna, gyro, camera, merge, sensor, pb
                {$"{STUInventoryEnumerator.c}/SmallTube", 6 + 2*12 + 1 + 1 + 1}, // conv converter, sm hyd tank(12), sm conn, antenna, merge
                {$"{STUInventoryEnumerator.c}/LargeTube", 30 + 1*12 + 2*15 + 1 + 2}, // lrg hyd thrst, sm hyd tank (12), thrust (10), gyro, pb
                {$"{STUInventoryEnumerator.c}/Display", 1}, // pb
                {$"{STUInventoryEnumerator.c}/PowerCell", 2*6}, // sm bat(6), 
                {$"{STUInventoryEnumerator.c}/RadioCommunication", 4 + 1}, // antenna, sensor
            };


        }

    }
}
