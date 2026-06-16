using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.Entities.Blocks;
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
using static IngameScript.Program;

namespace IngameScript
{
    public partial class Program : MyGridProgram
    {
        MyIni Ini { get; set; } = new MyIni();
        public string ELEVATOR_ID { get; set; } = string.Empty;
        public Elevator _elevator { get; set; }
        public List<Elevator.Floor> _floors { get; set; }
        List<IMyPistonBase> _pistons { get; set; } = new List<IMyPistonBase>();
        Dictionary<int, IMyDoor> _doors { get; set; } = new Dictionary<int, IMyDoor>();
        Dictionary<int, IMyButtonPanel> _upButtons { get; set; } = new Dictionary<int, IMyButtonPanel>();
        Dictionary<int, IMyButtonPanel> _downButtons { get; set; } = new Dictionary<int, IMyButtonPanel>();
        Dictionary<int, IMyTerminalBlock> _floorDisplays { get; set; } = new Dictionary<int, IMyTerminalBlock>();
        List<IMyButtonPanel> _cabButtonPanel { get; set; } = new List<IMyButtonPanel>();
        IMyTerminalBlock _cabScreen { get; set; }

        public class Request
        {
            public Elevator.Floor CallingFloor { get; set; }
            public Elevator.Direction Direction { get; set; }
        }
        public List<Elevator.Floor> RequestedFloors { get; set; } = new List<Elevator.Floor>();
        public Queue<Request> Requests { get; set; } = new Queue<Request>();
        public Request CurrentRequest { get; set; }

        public Program()
        {
            // open PB's custom data, find elevator ID and get all keys, which denote floors and their block height above basement
            // [ELEVATOR]
            // ID=ELEVATOR_NAME
            // 1=(block height above basement)
            // 2=(block height above basement)
            // ...
            Ini.Clear();
            if (!Ini.TryParse(Me.CustomData, "ELEVATOR")) throw new Exception(); // throws exception if tryparse returns false (fails)
            ELEVATOR_ID = Ini.Get("ELEVATOR", "ID").ToString("DEFAULT");

            // now we can find the pistons
            // [ELEVATOR]
            // ID=ELEVATOR_NAME
            List<IMyPistonBase> pistons = new List<IMyPistonBase>();
            Grid.GetBlocksOfType(pistons, piston => piston.CustomData.Contains("[ELEVATOR]"));
            foreach (var piston in pistons)
            {
                Ini.Clear();
                if (CheckID(piston))
                {
                    _pistons.Add(piston);
                }
                else continue;
            }

            // now we can instantiate the elevator object
            _elevator = new Elevator(ELEVATOR_ID, _pistons);

            // now we can iteratively create floors and add them to the elevator object
            List<MyIniKey> floorsFromConfig = new List<MyIniKey>();
            Ini.GetKeys(floorsFromConfig);
            int result; // not used for anything, just needs to exist so the following TryParse will work
            floorsFromConfig.RemoveAll(floor => !int.TryParse(floor.Name, out result)); // get rid of all keys that can't be parsed as an integer
            // we should be left with keys that can be parsed as integers... now we will loop through them and attempt to gather all hardware associated with that floor

            foreach (var floor in floorsFromConfig)
            {
                int floorNumFromConfig;
                if (!int.TryParse(floor.Name, out floorNumFromConfig)) continue; // skip for malformed config

                Elevator.Floor tempFloor = new Elevator.Floor();

                tempFloor.FloorNum = floorNumFromConfig;
                tempFloor.BlocksAboveBasement = Ini.Get(floor).ToInt16(Int16.MinValue);
                tempFloor.Display = GetFloorDisplay(ELEVATOR_ID, floorNumFromConfig);
                tempFloor.UpCallButton = 
            }

            LoadHardware();
        }

        FloorDisplay GetFloorDisplay(string id, int floorNum)
        {
            // loop through displays:
            // [ELEVATOR]
            // ID=ELEVATOR_NAME
            // TYPE={FLOOR | CAB}
            // FLOOR=#
            List<IMyTerminalBlock> screens = new List<IMyTerminalBlock>();
            Grid.GetBlocksOfType(screens, screen => screen.CustomData.Contains("[ELEVATOR]"));

            IMyTerminalBlock candidate = screens.First(); // this may cause debugging headaches later...
            foreach (var screen in screens)
            {
                MyIni _ini = new MyIni();
                if (!_ini.TryParse(screen.CustomData)) continue;
                CheckID(screen, id);
                int floor = _ini.Get("ELEVATOR", "FLOOR").ToInt16(Int16.MinValue);
                if (floor == Int16.MinValue || floor != floorNum) continue;
                string type = _ini.Get("ELEVATOR", "TYPE").ToString(string.Empty);
                if (type == string.Empty || type == "CAB") continue;

                candidate = screen; break;
            }
            return new FloorDisplay(candidate);
        }

        CabDisplay GetCabDisplay(string id)
        {
            // loop through displays:
            // [ELEVATOR]
            // ID=ELEVATOR_NAME
            // TYPE={FLOOR | CAB}
            // FLOOR=#
            List<IMyTerminalBlock> screens = new List<IMyTerminalBlock>();
            Grid.GetBlocksOfType(screens, screen => screen.CustomData.Contains("[ELEVATOR]"));

            IMyTerminalBlock candidate = screens.First(); // this may cause debugging headaches later...
            foreach (var screen in screens)
            {
                MyIni _ini = new MyIni();
                if (!_ini.TryParse(screen.CustomData)) continue;
                CheckID(screen, id);
                string type = _ini.Get("ELEVATOR", "TYPE").ToString(string.Empty);
                if (type == string.Empty || type == "CAB") continue;

                candidate = screen; break;
            }
            return new CabDisplay(candidate);
        }

        Elevator.FloorButton GetFloorButton(string id, Elevator.Direction direction)
        {

        }

        IMyButtonPanel GetCabButtonPanel(string id)
        {

        }

        void LoadHardware()
        {
            

            // loop through all doors that have the [ELEVATOR] section in their custom data
            // [ELEVATOR]
            // ID=ELEVATOR_NAME
            // FLOOR=#
            List<IMyDoor> doors = new List<IMyDoor>();
            Grid.GetBlocksOfType(doors, door => door.CustomData.Contains("[ELEVATOR]"));
            foreach (var door in doors)
            {
                Ini.Clear();
                if (CheckID(door))
                {
                    int floorNum = Ini.Get("ELEVATOR", "FLOOR").ToInt16(Int16.MinValue);
                    if (floorNum == Int16.MinValue) continue; // skip this door for malformed unparsable floor number

                    _doors.Add(floorNum, door);
                }
            }

            // loop through buttonpanels:
            // [ELEVATOR]
            // ID=ELEVATOR_NAME
            // TYPE={FLOOR | CAB}
            // FLOOR=#
            // DIRECTION={UP | DOWN}
            List<IMyButtonPanel> buttonPanels = new List<IMyButtonPanel>();
            Grid.GetBlocksOfType(buttonPanels, panel => panel.CustomData.Contains("[ELEVATOR]"));
            foreach (var buttonPanel in buttonPanels)
            {
                Ini.Clear();
                if (CheckID(buttonPanel))
                {
                    string type = Ini.Get("ELEVATOR", "TYPE").ToString(string.Empty);
                    int floorNum = Ini.Get("ELEVATOR", "FLOOR").ToInt16(Int16.MinValue);
                    if (floorNum == Int16.MinValue) continue; // abort for malformed config
                    string direction = Ini.Get("ELEVATOR", "DIRECTION").ToString(string.Empty);

                    switch (type)
                    {
                        case "FLOOR":
                            switch (direction)
                            {
                                case "UP": _upButtons.Add(floorNum, buttonPanel); break;
                                case "DOWN": _downButtons.Add(floorNum, buttonPanel); break;
                                default: continue; // abort for malformed config
                            }
                            break;
                        case "CAB": _cabButtonPanel.Add(buttonPanel); break;
                        default: continue; // abort for malformed config
                    }
                }
            }

            

            

        }

        bool CheckID(IMyTerminalBlock block, string id)
        {
            bool pass;
            Ini.Clear();
            pass = Ini.TryParse(block.CustomData, "ELEVATOR") & Ini.Get("ELEVATOR", "ID").ToString() == id;
            Ini.Clear();
            return pass;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // calling blocks have to send some kind of argument that can be used to identify themselves
            // no way to glean that from the ether, or as an additional argument of Main, etc
        }
    }
}
