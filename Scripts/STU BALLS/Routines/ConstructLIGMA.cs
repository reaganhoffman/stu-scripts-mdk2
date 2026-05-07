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
                _balls.AddToLogQueue("top of init");
                _balls.Projector.Enabled = true;
                _balls.MergeBlock.Enabled = true;
                _balls.Connector.Enabled = true;
                foreach (var welder in _balls.Welders) { welder.Enabled = true; }
                _balls.AddToLogQueue("after all the init stuff");
                return true;
            }

            public override bool Run()
            {
                _balls.AddToLogQueue("top of run");
                if (_balls.Projector.BuildableBlocksCount == 0)
                {
                    foreach (var welder in _balls.Welders) { welder.Enabled = false; }
                    _balls.AddToLogQueue("hit return true");
                    return true;
                }
                else _balls.AddToLogQueue("hit return false"); return false;
            }

            public override bool Closeout()
            {
                _balls.AddToLogQueue("top of closeout");
                _balls.Connector.Connect();
                _balls.TryGetLIGMAFuelTanks();
                bool atLeastOneNotFull = false;
                foreach (var tank in BALLS.LIGMA_FuelTanks)
                {
                    if (tank.FilledRatio < 1) atLeastOneNotFull = true;
                }
                if (atLeastOneNotFull)
                {
                    _balls.AddToLogQueue("hit return false");
                    return false;
                }
                else if (_balls.MergeBlock.IsConnected == false)
                {
                    _balls.AddToLogQueue("hit return true");
                    return true;
                }
                else _balls.AddToLogQueue("hit return false"); return false;
            }
        }
    }
}
