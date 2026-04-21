using Sandbox.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    partial class Program
    {
        class ConstructLIGMA : STUStateMachine
        {
            public override string Name => "Construct LIGMA";

            public long LIGMA_EntityID;
            
            public override bool Init()
            {
                BALLS.Projector.Enabled = true;
                BALLS.MergeBlock.Enabled = true;
                BALLS.Connector.Enabled = true;
                BALLS.Connector.Connect();
                foreach (var welder in BALLS.Welders) { welder.Enabled = true; }
                return true;
            }

            public override bool Run()
            {
                if (BALLS.Projector.BuildableBlocksCount == 0)
                {
                    foreach (var welder in BALLS.Welders) { welder.Enabled = false; }
                    return true;
                }
                else return false;
            }

            public override bool Closeout()
            {
                BALLS.TryGetLIGMAFuelTanks();
                bool atLeastOneNotFull = false;
                foreach (var tank in BALLS.LIGMA_FuelTanks)
                {
                    if (tank.FilledRatio < 1) atLeastOneNotFull = true;
                }
                if (!atLeastOneNotFull)
                {
                    // send launch command to ligma
                    
                    return true;
                }

                return false;
            }
        }
    }
}
