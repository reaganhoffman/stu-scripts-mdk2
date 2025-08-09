using System;
using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;

namespace IngameScript
{
    partial class Program
    {
        public class Suspension
        {
            public IMyMotorSuspension[] Wheels { get; set; }
            public enum States
            {
                Up,
                Down,
                Initial
            }
            public States DesiredState { get; set; }
            
            public Suspension(IMyMotorSuspension[] wheels)
            {
                Wheels = wheels;
                DesiredState = States.Initial;
            }

            public void Raise()
            {
                float lowestWheel = Rover.WHEEL_MIN_HEIGHT; // min height refers to the suspension itself, not the effective height of the rover. they're backwards.
                foreach (var wheel in Wheels)
                {
                    wheel.Height -= Rover.WHEEL_INC_HEIGHT;
                }
            }

            public void Lower()
            {

            }
        }
    }
}
