using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    partial class Program
    {
        public partial class Rover
        {
            public const float WHEEL_MIN_HEIGHT = -0.5f;
            public const float WHEEL_MAX_HEIGHT = 0.35f;
            public const float WHEEL_INC_HEIGHT = 0.1f;
            public static IMyGridTerminalSystem RoverGrid { get; set; }
            public static IMyProgrammableBlock Me { get; set; }
            public static Action<string> Echo { get; set; }

            public static IMyMotorSuspension[] Wheels { get; set; }
            public static float WheelHeight { get; set; }
            public static Suspension ThisSuspension { get; set; }
            public static Suspension.States UserInputSuspensionState { get; set; }

            public Rover(IMyGridTerminalSystem grid, IMyProgrammableBlock programmableBlock, Action<string> echo)
            {
                RoverGrid = grid;
                Me = programmableBlock;
                Echo = echo;

                Wheels = LoadAllBlocksOfType<IMyMotorSuspension>();
                
                float cumulativeWheelHeight = 0;
                int numWheels = 0;
                foreach (var wheel in Wheels)
                {
                    cumulativeWheelHeight += wheel.Height;
                    numWheels++;
                }
                WheelHeight = cumulativeWheelHeight / numWheels;

                ThisSuspension = new Suspension(Wheels);
            }

            public static T[] LoadAllBlocksOfType<T>() where T : class, IMyTerminalBlock
            {
                var intermediateList = new List<T>();
                RoverGrid.GetBlocksOfType(intermediateList, block => block.CubeGrid == Me.CubeGrid);
                if (intermediateList.Count == 0) { Echo($"No blocks of type '{typeof(T).Name}' found on the grid."); }
                return intermediateList.ToArray();
            }

            public static void AirUp()
            {
                foreach (var wheel in Wheels)
                {
                    wheel.Height += WHEEL_INC_HEIGHT;
                }
            }

            public static void AirDown()
            {
                foreach (var wheel in Wheels)
                {
                    wheel.Height -= WHEEL_INC_HEIGHT;
                }
            }

            public static void Test()
            {
                string heights = "";
                foreach (var wheel in Wheels)
                {
                    heights += $"{wheel.Height}\n";
                }
                Echo($"{heights}");
            }
        }
    }
    
}
