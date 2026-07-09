using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
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
        static IMyGridTerminalSystem Grid { get; set; }
        static IMyProgrammableBlock PB { get; set; }

        public static class HWLoader
        {
            //public HWLoader(IMyGridTerminalSystem grid, IMyProgrammableBlock pb)
            //{
            //    Grid = grid;
            //    PB = pb;
            //}

            public static T[] LoadAllBlocksOfType<T>() where T : class, IMyTerminalBlock
            {
                var intermediateList = new List<T>();
                Grid.GetBlocksOfType(intermediateList, block => block.IsSameConstructAs(PB));
                return intermediateList.ToArray();
            }
            public static T[] LoadAllBlocksOfTypeWithCustomData<T>(string customData) where T : class, IMyTerminalBlock
            {
                var intermediateList = new List<T>();
                Grid.GetBlocksOfType(intermediateList, block => block.IsSameConstructAs(PB) && block.CustomData.Contains(customData));
                return intermediateList.ToArray();
            }
            public static T[] LoadAllBlocksOfTypeWithDetailedInfo<T>(string detailedInfo) where T : class, IMyTerminalBlock
            {
                var intermediateList = new List<T>();
                Grid.GetBlocksOfType(intermediateList, block => block.IsSameConstructAs(PB) && block.DetailedInfo.Contains(detailedInfo));
                return intermediateList.ToArray();
            }
            public static T[] LoadAllBlocksOfTypeWithSubtypeId<T>(string subtype) where T : class, IMyTerminalBlock
            {
                var intermediateList = new List<T>();
                Grid.GetBlocksOfType(intermediateList, block => block.IsSameConstructAs(PB) && block.BlockDefinition.SubtypeId.Contains(subtype));
                return intermediateList.ToArray();
            }
            public static T LoadBlockByName<T>(string name) where T : class, IMyTerminalBlock
            {
                var block = Grid.GetBlockWithName(name);
                if (block == null) return null;
                else return block as T;
            }

            private static IMyLargeGatlingTurret[] LoadGatlingGuns()
            {
                List<IMyLargeGatlingTurret> gatlingGunBlocks = new List<IMyLargeGatlingTurret>();
                Grid.GetBlocksOfType<IMyLargeGatlingTurret>(gatlingGunBlocks, block => block.IsSameConstructAs(PB) &&
                    !block.BlockDefinition.SubtypeName.Contains("LargeBlockMediumCalibreTurret") && // not assault turrets
                    !block.BlockDefinition.SubtypeName.Contains("LargeBlockLargeCalibreGun") && // not artillery
                    !block.BlockDefinition.SubtypeName.Contains("LargeRailgun")); // not railguns

                return gatlingGunBlocks.ToArray();
            }
        }
    }
}
