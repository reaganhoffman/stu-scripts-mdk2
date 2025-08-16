using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public static class CBT_VARIABLES
        {
            public const string CBT_VEHICLE_NAME = "CBT";
            public const string CBT_BROADCAST_CHANNEL = "CBT";
            public const string TEA_KEY = "SaladTossersUnited";

            public const float HINGE_ANGLE_TOLERANCE = 0.0077f;
            public const float HINGE_TARGET_VELOCITY = 4f;
            public const float HINGE_TORQUE = 7000000;
            public const float PISTON_POSITION_TOLERANCE = 0.01f;
            public const float PISTON_TARGET_VELOCITY = 1.7f;
            public const float PISTON_NEUTRAL_DISTANCE = 4f;

            public const double PLANETARY_DETECTION_BUFFER = 2000;
            //public const string PRIVATE_KEY = "";
            //public const string PUBLIC_KEY = "";
        }
    }
}
