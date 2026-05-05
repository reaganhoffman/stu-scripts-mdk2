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

            BALLS _balls { get; set; }

            public ConstructLIGMA(BALLS balls)
            {
                _balls = balls;
            }
            
            public override bool Init()
            {
                _balls.Projector.Enabled = true;
                _balls.MergeBlock.Enabled = true;
                _balls.Connector.Enabled = true;
                foreach (var welder in _balls.Welders) { welder.Enabled = true; }
                return true;
            }

            public override bool Run()
            {
                if (_balls.Projector.BuildableBlocksCount == 0)
                {
                    foreach (var welder in _balls.Welders) { welder.Enabled = false; }
                    return true;
                }
                else return false;
            }

            public override bool Closeout()
            {
                _balls.Connector.Connect();
                _balls.TryGetLIGMAFuelTanks();
                bool atLeastOneNotFull = false;
                foreach (var tank in BALLS.LIGMA_FuelTanks)
                {
                    if (tank.FilledRatio < 1) atLeastOneNotFull = true;
                }
                if (atLeastOneNotFull) return false;
                else if (_balls.MergeBlock.IsConnected == false) return true;
                else return false;
            }
        }
    }
}
