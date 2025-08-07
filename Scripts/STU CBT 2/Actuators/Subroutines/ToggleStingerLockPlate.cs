using System;
using System.Collections;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public partial class CBT
        {
            public class ToggleStingerLockPlate : STUStateMachine
            {
                public override string Name => "Lock Stinger";
                public Queue<STUStateMachine> ManeuverQueue { get; set; }
                public bool State;

                public ToggleStingerLockPlate(Queue<STUStateMachine> thisManeuverQueue, bool state)
                {
                    ManeuverQueue = thisManeuverQueue;
                    State = state;
                }

                public override bool Init()
                {
                    return true;
                }

                public override bool Run()
                {
                    if (State) 
                    { 
                        StingerLock.Lock();
                        return StingerLock.IsLocked;
                    }
                    else
                    {
                        StingerLock.Unlock();
                        return !StingerLock.IsLocked;
                    }
                }

                public override bool Closeout()
                {
                    return true;
                }
            }
        }
        
    }
    
}
