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
                    _balls.Connector.Connect();
                    return true;
                }
                return false;
            }

            public override bool Closeout()
            {
                _balls.TryGetLIGMAFuelTanks();
                bool tanksFull = true;
                foreach (var tank in _balls.LIGMA_FuelTanks)
                {
                    if (tank.FilledRatio < 1) tanksFull = false;
                }
                if (tanksFull || IGNORE_OUT_OF_RESOURCES)
                {
                    _balls.Connector.Disconnect();
                    _balls.Connector.Enabled = false;
                    return true;
                }
                return false;
            }
        }
    }
}
