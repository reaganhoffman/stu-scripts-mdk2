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
            
            public override bool Init()
            {
                return true;
            }

            public override bool Run()
            {
                return true;
            }

            public override bool Closeout()
            {
                return true;
            }
        }
    }
}
