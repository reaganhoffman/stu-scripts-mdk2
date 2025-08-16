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

            const double DEFAULT_TIME_BUFFER = 1000f;
            public struct Airlock
            {
                public IMyDoor SideA { get; set; }
                public IMyDoor SideB { get; set; }
                public double TimeBufferMS { get; set; }
                public AirlockStateMachine StateMachine { get; set; }
            }
            public struct SoloAirlock
            {
                public IMyDoor Door { get; set; }
                public double TimeBufferMS { get; set; }
                public SoloAirlockStateMachine StateMachine { get; set; }
            }
            List<Airlock> Airlocks { get; set; } = new List<Airlock>();
            List<SoloAirlock> SoloAirlocks { get; set; } = new List<SoloAirlock>();

            public bool SoloEnabled { get; private set; } = true;
            public bool AirlockEnabled { get; private set; } = true;

            /// <summary>
            /// Searches through the grid, finds solo doors and airlock pairs and loads them into the script.
            /// </summary>
            /// <param name="doors"></param>
            /// <param name="runtime"></param>
            public void LoadAirlocks(List<IMyDoor> doors, IMyGridProgramRuntimeInfo runtime)
            {
                // create temp dictionary of door name and the door object for assigning airlock pairs later
                Dictionary<string, IMyDoor> doorDictionary = new Dictionary<string, IMyDoor>();
                foreach (var door in doors)
                {
                    CBT.echo($"Adding door {door.CustomName} to internal dictionary");
                    doorDictionary.Add(door.CustomName.Trim().ToUpper(), door);
                }

                List<long> doorsAlreadyAdded = new List<long>();
                
                foreach (var door in doors)
                {
                    if (doorsAlreadyAdded.Contains(door.EntityId))
                        continue; // skip this door if we've already made an airlock object out of it
                    MyIni ini = new MyIni();

                    MyIniParseResult result;
                    if (!ini.TryParse(door.CustomData, out result))
                        continue; // this should skip the current block if attempting to parse its custom data fails


                    string partner = "SOLO";
                    double timeBuffer = DEFAULT_TIME_BUFFER;
                    ini.Get("AIRLOCK", "PARTNER").TryGetString(out partner);
                    if (ini.ContainsKey("AIRLOCK", "TIME_BUFFER"))
                        ini.Get("AIRLOCK", "TIME_BUFFER").TryGetDouble(out timeBuffer);

                    CBT.echo($"partner: {partner}");
                    CBT.echo($"time buffer: {timeBuffer}");

                    if (partner.ToUpper() == "SOLO")
                    {
                        SoloAirlock soloAirlock = new SoloAirlock();
                        soloAirlock.Door = door;
                        soloAirlock.TimeBufferMS = timeBuffer;
                        soloAirlock.StateMachine = new SoloAirlockStateMachine(door, runtime, timeBuffer);
                        SoloAirlocks.Add(soloAirlock);
                        doorsAlreadyAdded.Add(door.EntityId);
                    }
                    else if (doorDictionary.ContainsKey(partner.Trim().ToUpper()))
                    {
                        Airlock airlock = new Airlock();
                        airlock.SideA = door;
                        IMyDoor partnerDoor;
                        if (!doorDictionary.TryGetValue(partner.Trim().ToUpper(), out partnerDoor))
                            throw new Exception();
                        airlock.SideB = partnerDoor;
                        airlock.TimeBufferMS = timeBuffer;
                        airlock.StateMachine = new AirlockStateMachine(door, partnerDoor, runtime, timeBuffer);
                        Airlocks.Add(airlock);

                        doorsAlreadyAdded.Add(door.EntityId);
                        doorsAlreadyAdded.Add(partnerDoor.EntityId);

                    }

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
