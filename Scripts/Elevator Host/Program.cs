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

namespace IngameScript
{
    public partial class Program : MyGridProgram
    {
        MyIni _ini { get; set; } = new MyIni();
        public string ELEVATOR_ID { get; set; } = string.Empty;
        public Elevator _elevator { get; set; }
        
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
            List<IMyPistonBase> _pistons = new List<IMyPistonBase>();
            Dictionary<int, IMyDoor> _doors = new Dictionary<int, IMyDoor>();
            Dictionary<int, IMyButtonPanel> _upButtons = new Dictionary<int, IMyButtonPanel>();
            Dictionary<int, IMyButtonPanel> _downButtons = new Dictionary<int, IMyButtonPanel>();
            Dictionary<int, IMyTerminalBlock> _floorDisplays = new Dictionary<int, IMyTerminalBlock>();
            List<IMyButtonPanel> _cabButtonPanel = new List<IMyButtonPanel>();
            IMyTerminalBlock _cabScreen;
            
            try
            {
                // open PB's custom data, find out elevator ID
                // [ELEVATOR]
                // ID=ELEVATOR_NAME
                _ini.Clear();
                if (!_ini.TryParse(Me.CustomData, "ELEVATOR")) throw new Exception(); // throws exception if tryparse returns false (fails)
                ELEVATOR_ID = _ini.Get("ELEVATOR", "ID").ToString("DEFAULT");

                // search through all blocks, find common elevator components, assign those to elevator objects

                // loop through pistons:
                // [ELEVATOR]
                // ID=ELEVATOR_NAME
                List<IMyPistonBase> pistons = new List<IMyPistonBase>();
                Grid.GetBlocksOfType(pistons);
                foreach (var piston in pistons)
                {
                    _ini.Clear();
                    if (CheckID(piston))
                    {
                        _pistons.Add(piston);
                    }
                    else continue;
                }

                // loop through doors, since a door is what defines a floor
                // [ELEVATOR]
                // ID=ELEVATOR_NAME
                // FLOOR=#
                List<IMyDoor> doors = new List<IMyDoor>();
                Grid.GetBlocksOfType(doors);
                foreach (var door in doors)
                {
                    _ini.Clear();
                    if (CheckID(door))
                    {
                        int floorNum = _ini.Get("ELEVATOR", "FLOOR").ToInt32(0);
                        if (floorNum == 0) continue; // skip this door for malformed unparsable floor number

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
                Grid.GetBlocksOfType(buttonPanels);
                foreach (var buttonPanel in buttonPanels)
                {
                    _ini.Clear();
                    if (CheckID(buttonPanel))
                    {
                        string type = _ini.Get("ELEVATOR", "TYPE").ToString(string.Empty);
                        int floorNum = _ini.Get("ELEVATOR", "FLOOR").ToInt16(0);
                        string direction = _ini.Get("ELEVATOR", "DIRECTION").ToString(string.Empty);

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

                // loop through displays:
                // [ELEVATOR]
                // ID=ELEVATOR_NAME
                // TYPE={FLOOR | CAB}
                // FLOOR=#
                List<IMyTerminalBlock> screens = new List<IMyTerminalBlock>();
                List<IMyTextSurfaceProvider> blocksWithScreens = new List<IMyTextSurfaceProvider>();
                Grid.GetBlocksOfType(blocksWithScreens);
                foreach (IMyTerminalBlock blockWithScreen in blocksWithScreens)
                {
                    if (!blockWithScreen.GetType().ToString().ToUpper().Contains("BUTTON"))
                    {
                        screens.Add(blockWithScreen);
                    }
                }
                foreach (var screen in screens)
                {
                    _ini.Clear();
                    if (CheckID(screen))
                    {
                        string type = _ini.Get("ELEVATOR", "TYPE").ToString(string.Empty);
                        int floorNum = _ini.Get("ELEVATOR", "FLOOR").ToInt32(0);
                        if (floorNum == 0) continue; // abort for malformed config
                        switch (type)
                        {
                            case "FLOOR": _floorDisplays.Add(floorNum, screen); break;
                            case "CAB": _cabScreen = screen; break;
                            default: continue; // abort for malformed config
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Echo($"failure in parsing configs:\n{e.Message}\n{e.StackTrace}");
            }

            try
            {
                List<Elevator.Floor> _floors = new List<Elevator.Floor>();
                // compile list objects into floor objects
                for (int i = 1; i < _doors.Count; i++)
                {
                    _floors.Add(new Elevator.Floor(
                        i,
                        _doors[i],
                        new Elevator.FloorButton(_upButtons[i], Elevator.Direction.Up),
                        new Elevator.FloorButton(_downButtons[i], Elevator.Direction.Down),
                        new FloorDisplay(_floorDisplays[i])
                        ));
                }

                _elevator = new Elevator(ELEVATOR_ID, _pistons, _floors, _cabButtonPanel.First());
            }
            catch (Exception e)
            {
                Echo($"Error creating the elevator:\n{e.Message}\n{e.StackTrace}");
            }


        }

        bool CheckID(IMyTerminalBlock block)
        {
            bool pass;
            _ini.Clear();
            pass = _ini.TryParse(block.CustomData, "ELEVATOR") & _ini.Get("ELEVATOR", "ID").ToString() == ELEVATOR_ID;
            _ini.Clear();
            return pass;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // main loop that handles requests and moves pistons, opens doors, updates screens, etc
        }
    }
}
