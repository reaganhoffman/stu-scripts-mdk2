using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    partial class Program
    {
        public class ElevatorPistons
        {
            List<IMyPistonBase> Pistons { get; set; }

            public ElevatorPistons(List<IMyPistonBase> pistons)
            {
                Pistons = pistons;
            }

            public void GoToFloor(Elevator.Floor floor)
            {

            }
        }
    }
}
