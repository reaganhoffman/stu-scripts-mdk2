using Sandbox.Game.Entities.Cube;
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
        
        public class AirlockControlModule
        {
            
            public struct Airlock
            {
                public IMyDoor SideA { get; set; }
                public IMyDoor SideB { get; set; }
                public AirlockStateMachine StateMachine { get; set; }
            }
            public struct SoloAirlock
            {
                public IMyDoor Door { get; set; }
                public SoloAirlockStateMachine StateMachine { get; set; }
            }
            private List<Airlock> Airlocks { get; set; } = new List<Airlock>();
            private List<SoloAirlock> SoloAirlocks { get; set; } = new List<SoloAirlock>();

            public bool SoloEnabled { get; private set; } = true;
            public bool AirlockEnabled { get; private set; } = true;

            /// <summary>
            /// Searches through the grid, finds solo doors and airlock pairs and loads them into the script.
            /// </summary>
            /// <param name="grid"></param>
            /// <param name="programmableBlock"></param>
            /// <param name="runtime"></param>
            public void LoadAirlocks(IMyGridTerminalSystem grid, IMyProgrammableBlock programmableBlock, IMyGridProgramRuntimeInfo runtime)
            {
                // create a temporary list of all the doors on the grid
                List<IMyTerminalBlock> allDoors = new List<IMyTerminalBlock>();
                grid.GetBlocksOfType<IMyDoor>(allDoors, airlock => airlock is IMyDoor && airlock.CubeGrid == programmableBlock.CubeGrid);
                // extract all the solo airlocks from the temp list by checking whether their custom data contains "solo"
                for (int i = 0; i < allDoors.Count; i++)
                {
                    IMyDoor door = (IMyDoor)allDoors[i];
                    if (door.CustomData.ToUpper().Contains("SOLO"))
                    {
                        SoloAirlock soloAirlock = new SoloAirlock();
                        soloAirlock.Door = door;
                        soloAirlock.StateMachine = new SoloAirlockStateMachine(door, runtime);
                        SoloAirlocks.Add(soloAirlock);
                        allDoors.Remove(door);
                        i--;
                    }
                }
                // loop through the temp list, // repeat until the temp list is empty.
                while (allDoors.Count > 0)
                {
                    IMyDoor door = (IMyDoor)allDoors[0];
                    // get the first line of its custom data.
                    string[] customDataRaw = door.CustomData.Split('\n');
                    string listedPartner = customDataRaw[0];
                    // if it has a partner, add both doors to a new Airlock struct and add that to the Airlocks list
                    if (listedPartner != "")
                    {
                        IMyDoor partner = (IMyDoor)grid.GetBlockWithName(listedPartner);
                        if (partner != null)
                        {
                            Airlock airlock = new Airlock();
                            airlock.SideA = door;
                            airlock.SideB = partner;
                            airlock.StateMachine = new AirlockStateMachine(door, partner, runtime);
                            Airlocks.Add(airlock);
                        }
                    }
                    // remove both doors from the temp list
                    allDoors.Remove(door);
                    allDoors.Remove((IMyTerminalBlock)grid.GetBlockWithName(listedPartner));
                }
            }

            /// <summary>
            /// Enumerates through the stored list of airlocks known to the program and prints them to the output.
            /// </summary>
            /// <returns>
            /// A string of all the airlock pairs found on the grid.
            /// </returns>
            public string GetAirlocks()
            {
                string result = "Airlocks:\n\n";
                foreach (Airlock airlock in Airlocks)
                {
                    result += $"{airlock.SideA.CustomName} <-> {airlock.SideB.CustomName}\n";
                }
                foreach (SoloAirlock soloAirlock in SoloAirlocks)
                {
                    result += $"Solo: {soloAirlock.Door.CustomName}\n";
                }
                return result;
            }

            /// <summary>
            /// The main method to update the state machines of the airlock objects. Must be called every program execution cycle. This is where the 'enabled' state variables for the airlocks comes into play.
            /// </summary>
            public void UpdateAirlocks()
            {
                if (AirlockEnabled)
                {
                    foreach (Airlock airlock in Airlocks)
                    {
                        airlock.StateMachine.Update();
                    }
                }
                
                if (SoloEnabled)
                {
                    foreach (SoloAirlock soloAirlock in SoloAirlocks)
                    {
                        soloAirlock.StateMachine.Update();
                    }
                }
            }

            /// <summary>
            /// Opens all airlocks (double-doors) that are currently held in memory. Only works if automatic control is disabled.
            /// </summary>
            public void OpenAirlocks(bool overrideControl = false)
            {
                if (!overrideControl && !AirlockEnabled) return;
                foreach (var a in Airlocks)
                {
                    a.SideA.OpenDoor();
                    a.SideB.OpenDoor();
                }
            }

            /// <summary>
            /// Closes all airlocks (double-doors) that are currently held in memory.
            /// </summary>
            public void CloseAirlocks()
            {
                foreach (var a in Airlocks)
                {
                    a.SideA.CloseDoor();
                    a.SideB.CloseDoor();
                }
            }

            /// <summary>
            /// Opens all solo doors that are currently held in memory. Only works if automatic control is disabled.
            /// </summary>
            public void OpenSoloDoors(bool overrideControl = false)
            {
                if (!overrideControl && !SoloEnabled) return;
                foreach (var a in SoloAirlocks)
                {
                    a.Door.OpenDoor();
                }
            }

            /// <summary>
            /// Closes all solo doors that are currently held in memory.
            /// </summary>
            public void CloseSoloDoors()
            {
                foreach (var a in SoloAirlocks)
                {
                    a.Door.CloseDoor();
                }
            }

            /// <summary>
            /// Sets the enabled state of each class of airlock. Defaults to 'true' for both classes.
            /// </summary>
            /// <param name="solo"></param>
            /// <param name="airlock"></param>
            public void ChangeAutomaticControl(bool solo, bool airlock)
            {
                SoloEnabled = solo;
                AirlockEnabled = airlock;
            }

            /// <summary>
            /// Changes the duration that (all) the airlocks are open for.
            /// </summary>
            /// <param name="timeBufferMS"></param>
            public void ChangeDuration(double timeBufferMS = 750)
            {
                foreach (Airlock airlock in Airlocks)
                {
                    airlock.StateMachine.TimeBufferMS = timeBufferMS;
                }
                foreach (SoloAirlock soloAirlock in SoloAirlocks)
                {
                    soloAirlock.StateMachine.TimeBufferMS = timeBufferMS;
                }
            }
        }
    }
}
