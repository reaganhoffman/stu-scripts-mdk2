using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
    public partial class Program : MyGridProgram
    {
        //static HWLoader Loader { get; set; }
        public const float FAST = 2;
        public const float MEDIUM = 1f;
        public const float SLOW = 0.1f;
        public const float PISTON_MIN_POSITION = 0.001f;
        public const float MAIN_PISTON_MAX_POSITION = 10f;
        public const float PISTON_TOLERANCE = 0.1f;
        public const float PISTON_FORCE = 5e5f;
        IMyPistonBase MP1 { get; set; }
        IMyPistonBase MP2 { get; set; }
        IMyPistonBase MP3 { get; set; }
        IMyMotorStator H { get; set; }
        IMyPistonBase RP1 { get; set; }
        IMyPistonBase RP2 { get; set; }
        IMyMotorStator RR { get; set; }
        IMyPistonBase LP1 { get; set; }
        IMyPistonBase LP2 { get; set; }
        IMyMotorStator LR { get; set; } 
        IMyPistonBase BP1 { get; set; }
        IMyPistonBase BP2 { get; set; }

        public enum Phase
        {
            Closed,
            RetractBP2,
            RetractBP1,
            SlightUpMP3,
            HingeUp,
            RetractMainPistons,
            RotateDeploySideRotors,
            ExtendSidePiston1s,
            ExtendSidePiston2s,
            Opened,
            RetractSidePiston2s,
            RetractSidePiston1s,
            RotateStoreSideRotors,
            ExtendMainPistons,
            HingeDown,
            SlightDownMP3,
            ExtendBP1,
            ExtendBP2
        }
        public Phase CurrentPhase { get; set; }

        public enum State
        {
            Open,
            Opening,
            Closed,
            Closing
        }
        public State CurrentState { get; set; }

        public Program()
        {
            //Loader = new HWLoader(Grid, Me);
            LoadHardware();
            CurrentState = TryDetermineState();
        }

        public void Save()
        {
            
        }

        public void LoadHardware()
        {
            MP1 = HWLoader.LoadBlockByName<IMyPistonBase>("Big Door Main Piston 1");
            MP2 = HWLoader.LoadBlockByName<IMyPistonBase>("Big Door Main Piston 2");
            MP3 = HWLoader.LoadBlockByName<IMyPistonBase>("Big Door Main Piston 3");
            H = HWLoader.LoadBlockByName<IMyMotorStator>("Big Door Hinge");
            RP1 = HWLoader.LoadBlockByName<IMyPistonBase>("Big Door Right Piston 1");
            RP2 = HWLoader.LoadBlockByName<IMyPistonBase>("Big Door Right Piston 2");
            RR = HWLoader.LoadBlockByName<IMyMotorStator>("Big Door Right Rotor");
            LP1 = HWLoader.LoadBlockByName<IMyPistonBase>("Big Door Left Piston 1");
            LP2 = HWLoader.LoadBlockByName<IMyPistonBase>("Big Door Left Piston 2");
            LR = HWLoader.LoadBlockByName<IMyMotorStator>("Big Door Left Rotor");
            BP1 = HWLoader.LoadBlockByName<IMyPistonBase>("Big Door Back Piston 1");
            BP2 = HWLoader.LoadBlockByName<IMyPistonBase>("Big Door Back Piston 2");
        }

        public void Main(string argument, UpdateType updateSource)
        {
            Run();

            switch (argument.Trim().ToUpper())
            {
                case "OPEN": Open(); break;
                case "CLOSE": Close(); break;
                case "TOGGLE": Toggle(); break;
                case "STOP": Stop(); break;
                default: break;
            }
        }

        public State TryDetermineState()
        {
            if (MP1.CurrentPosition <= PISTON_MIN_POSITION &&
                MP2.CurrentPosition <= PISTON_MIN_POSITION &&
                MP3.CurrentPosition <= PISTON_MIN_POSITION
                )
            {
                CurrentPhase = Phase.Opened;
                return State.Open;
            }

            CurrentPhase = Phase.Closed;
            return State.Closed;
            // very lazy state determination here
        }

        public void Run()
        {
            switch (CurrentPhase)
            {
                case Phase.Closed: break;
                case Phase.RetractBP2:
                    CurrentState = State.Opening;
                    BP2.Velocity = -FAST;
                    if (BP2.CurrentPosition <= PISTON_MIN_POSITION) CurrentPhase = Phase.RetractBP1;
                    break;
                case Phase.RetractBP1:
                    BP1.Velocity = -FAST;
                    if (BP1.CurrentPosition <= PISTON_MIN_POSITION) CurrentPhase = Phase.SlightUpMP3;
                    break;
                case Phase.SlightUpMP3:
                    MP3.Velocity = MEDIUM;
                    if (Math.Abs(MP3.CurrentPosition - 10) <= PISTON_TOLERANCE) CurrentPhase = Phase.HingeUp;
                    break;
                case Phase.HingeUp:
                    H.TargetVelocityRPM = -MEDIUM;
                    if (Math.Abs(H.Angle / Math.PI * 180) <= -2) CurrentPhase = Phase.RetractMainPistons;
                    break;
                case Phase.RetractMainPistons:
                    MP3.MinLimit = 0f;
                    MP1.Velocity = -FAST;
                    MP2.Velocity = -FAST;
                    MP3.Velocity = -FAST;
                    if (MP1.CurrentPosition <= PISTON_MIN_POSITION &&
                        MP2.CurrentPosition <= PISTON_MIN_POSITION &&
                        MP3.CurrentPosition <= PISTON_MIN_POSITION) CurrentPhase = Phase.RotateDeploySideRotors;
                    break;
                case Phase.RotateDeploySideRotors:
                    RR.TargetVelocityRPM = FAST;
                    LR.TargetVelocityRPM = -FAST;
                    if (Math.Abs(RR.Angle) <= PISTON_TOLERANCE &&
                        Math.Abs(LR.Angle) <= PISTON_TOLERANCE) CurrentPhase = Phase.ExtendSidePiston1s;
                    break;
                case Phase.ExtendSidePiston1s:
                    RP1.Velocity = FAST;
                    LP1.Velocity = FAST;
                    if (RP1.CurrentPosition >= 7 && LP1.CurrentPosition >= 7) CurrentPhase = Phase.ExtendSidePiston2s;
                    break;
                case Phase.ExtendSidePiston2s:
                    RP2.Velocity = FAST;
                    LP2.Velocity = FAST;
                    if (RP2.CurrentPosition >= 2 && LP2.CurrentPosition >= 2) CurrentPhase = Phase.Opened; CurrentState = State.Open;
                    break;
                case Phase.Opened: break;
                case Phase.RetractSidePiston2s:
                    CurrentState = State.Closing;
                    RP2.Velocity = -FAST;
                    LP2.Velocity = -FAST;
                    if (RP2.CurrentPosition <= PISTON_MIN_POSITION && LP2.CurrentPosition <= PISTON_MIN_POSITION) CurrentPhase = Phase.RetractSidePiston1s;
                    break;
                case Phase.RetractSidePiston1s:
                    RP1.Velocity = -FAST;
                    LP1.Velocity = -FAST;
                    if (RP1.CurrentPosition <= PISTON_MIN_POSITION && LP1.CurrentPosition <= PISTON_MIN_POSITION) CurrentPhase = Phase.RotateStoreSideRotors;
                    break;
                case Phase.RotateStoreSideRotors:
                    RR.TargetVelocityRPM = -FAST;
                    LR.TargetVelocityRPM = FAST;
                    if (Math.Abs(RR.Angle / Math.PI) - 0.5 <= PISTON_TOLERANCE &&
                        Math.Abs(LR.Angle / Math.PI) - 0.5 <= PISTON_TOLERANCE) CurrentPhase = Phase.ExtendMainPistons;
                    break;
                case Phase.ExtendMainPistons:
                    MP1.Velocity = FAST;
                    MP2.Velocity = FAST;
                    MP3.Velocity = FAST;
                    if (Math.Abs(MP1.CurrentPosition) - 10 <= 0 &&
                        Math.Abs(MP2.CurrentPosition) - 10 <= 0 &&
                        Math.Abs(MP3.CurrentPosition) - 10 <= 0) CurrentPhase = Phase.HingeDown;
                    break;
                case Phase.HingeDown:
                    H.TargetVelocityRPM = MEDIUM;
                    if (Math.Abs(H.Angle) <= PISTON_TOLERANCE) CurrentPhase = Phase.SlightDownMP3;
                    break;
                case Phase.SlightDownMP3:
                    MP3.MinLimit = 9.5f;
                    MP3.Velocity = -SLOW;
                    if (Math.Abs(MP3.CurrentPosition) - 9.5f <= 0) CurrentPhase = Phase.Closed; CurrentState = State.Closed;
                    break;
            }
        }

        public void Open()
        {
            CurrentState = State.Opening;
            CurrentPhase = Phase.RetractBP2;
        }

        public void Close()
        {
            CurrentState = State.Closing;
            CurrentPhase = Phase.RetractSidePiston2s;
        }

        public void Toggle()
        {
            switch (CurrentState)
            {
                case State.Open: Close(); break;
                case State.Closed: Open(); break;
                default: break;
            }
        }

        public void Stop()
        {

        }
    }
}
