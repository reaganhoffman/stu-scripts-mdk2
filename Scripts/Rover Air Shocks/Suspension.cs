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
                Raising,
                Raised,
                Lowering,
                Lowered,
                Unknown
            }
            public States CurrentState { get; private set; }
            States LastUserInputState { get; set; }
            public States DesiredState { get; set; }
            
            public Suspension(IMyMotorSuspension[] wheels)
            {
                Wheels = wheels;
                CurrentState = TryDetermineState();
            }

            public void Set(States desiredState)
            {
                if (desiredState != CurrentState && desiredState != LastUserInputState)
                {
                    if (CanGoToRequestedState(desiredState))
                    {
                        CurrentState = desiredState;
                        LastUserInputState = desiredState;
                    }
                    else
                    {
                        Rover.Echo($"Cannot go to desired state {desiredState}");
                    }
                }
            }

            States TryDetermineState()
            {
                if (AllWheelsLowered()) return States.Lowered;
                if (AllWheelsRaised()) return States.Raised;
                return States.Unknown;
            }

            public void UpdateState()
            {
                switch (CurrentState)
                {
                    case States.Raised: break;
                    case States.Lowered: break;
                    case States.Raising: if (!AllWheelsRaised()) { Raise(); } else CurrentState = States.Raised; break;
                    case States.Lowering: if (!AllWheelsLowered()) { Lower(); } else CurrentState = States.Lowered; break;
                    case States.Unknown: break;
                }
            }

            bool CanGoToRequestedState(States requestedState)
            {
                switch (requestedState)
                {
                    // (case) HowDoIGetToThisState?
                    // (return) you can only get to this state if these || conditions !& are && met
                    case States.Raised: return AllWheelsRaised();
                    case States.Lowered: return AllWheelsLowered();
                    case States.Raising: return true;
                    case States.Lowering: return true;
                    case States.Unknown: return false;
                    default: return false;
                }
            }

                bool AllWheelsRaised()
            {
                float sum = 0;
                foreach (var wheel in Wheels)
                {
                    wheel.Height += sum;
                }
                return sum / Wheels.Length == Rover.WHEEL_MAX_HEIGHT;
            }

            bool AllWheelsLowered()
            {
                float sum = 0;
                foreach (var wheel in Wheels)
                {
                    wheel.Height += sum;
                }
                return sum / Wheels.Length == Rover.WHEEL_MIN_HEIGHT;
            }

            public void Raise()
            {
                foreach (var wheel in Wheels)
                {
                    wheel.Height -= Rover.WHEEL_INC_HEIGHT;
                }
            }

            public void Lower()
            {
                foreach (var wheel in Wheels)
                {
                    wheel.Height += Rover.WHEEL_INC_HEIGHT;
                }
            }
        }
    }
}
