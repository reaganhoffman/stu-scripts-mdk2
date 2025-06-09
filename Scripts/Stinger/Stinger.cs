using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    partial class Program
    {
        public partial class Stinger
        {
            public static Action<string> Echo { get; set; }
            public static IMyProgrammableBlock Me { get; set; }
            public static IMyGridProgramRuntimeInfo Runtime { get; set; }
            public static IMyGridTerminalSystem StingerGrid { get; set; }
            public static List<StingerLogLCD> LogChannel { get; set; } = new List<StingerLogLCD>();
            public static List<StingerAutopilotLCD> AutopilotStatusChannel { get; set; } = new List<StingerAutopilotLCD>();
            public static STUFlightController FlightController { get; set; }
            public static STUMasterLogBroadcaster Broadcaster { get; set; }
            public static STUInventoryEnumerator InventoryEnumerator { get; set; }
            
            public const float TimeStep = 1.0f / 6.0f;
            public static float Timestamp { get; set; }
            public enum Phase
            {
                Idle,
                Executing,
            }
            public static Phase CurrentPhase { get; set; }

            public static bool CruiseControlActivated { get; set; }
            public static float CruiseControlSpeed { get; set; }
            public static bool AltitudeControlActivated { get; set; }
            public static float AltitudeControlHeight { get; set; }
            public static float UserInputForwardVelocity { get; set; }
            public static float UserInputRightVelocity { get; set; }
            public static float UserInputUpVelocity { get; set; }
            public static float UserInputRollVelocity { get; set; }
            public static float UserInputPitchVelocity { get; set; }
            public static float UserInputYawVelocity { get; set; }

            public static IMyMotorStator CameraRotor { get; set; }
            public static IMyMotorStator CameraHinge { get; set; }
            public static IMyCameraBlock Camera { get; set; }
            public static IMyRemoteControl RemoteControl { get; set; }
            public static IMyTerminalBlock FlightSeat { get; set; }
            public static IMyThrust[] Thrusters { get; set; }
            public static IMyGyro[] Gyros { get; set; }
            public static IMyBatteryBlock[] Batteries { get; set; }
            public static IMyGasTank[] HydrogenTanks { get; set; }
            public static IMyCargoContainer[] CargoContainers { get; set; }
            public static IMyInteriorLight[] InteriorLights { get; set; }
            public static IMyReflectorLight[] Spotlights { get; set; }
            public static IMyRadioAntenna Antenna { get; set; }
            public static IMyUserControllableGun[] GatlingGuns { get; set; }
            public static IMyUserControllableGun[] AssaultCannons { get; set; }
            public static IMyLandingGear[] LandingGears { get; set; }

            public Stinger(Action<string> echo, STUMasterLogBroadcaster broadcaster, STUInventoryEnumerator inventoryEnumerator, IMyGridTerminalSystem grid, IMyProgrammableBlock me, IMyGridProgramRuntimeInfo runtime)
            {
                Me = me;
                Broadcaster = broadcaster;
                InventoryEnumerator = inventoryEnumerator;
                Runtime = runtime;
                StingerGrid = grid;
                Echo = echo;
                Timestamp = 0f;
                CurrentPhase = Phase.Idle;
                CruiseControlActivated = false;
                CruiseControlSpeed = 0f;
                AltitudeControlActivated = false;
                AltitudeControlHeight = 0f;
                UserInputForwardVelocity = 0f;
                UserInputRightVelocity = 0f;
                UserInputUpVelocity = 0f;
                UserInputRollVelocity = 0f;
                UserInputPitchVelocity = 0f;
                UserInputYawVelocity = 0f;

                // hardware initialization code goes here
            }

            public static void EchoPassthru(string text)
            {
                Echo(text);
            }

            public static void CreateBroadcast(string message, string type = STULogType.INFO)
            {
                Broadcaster.Log(new STULog
                {
                    Sender = "STR",
                    Message = message,
                    Type = type
                });

                
            }

            public static void AddToLogQueue(string message, string type = STULogType.INFO, string sender = "STR")
            {
                foreach (var screen in LogChannel)
                {
                    screen.FlightLogs.Enqueue(new STULog
                    {
                        Sender = sender,
                        Message = message,
                        Type = type,
                    });
                }
            }

            public static void ResetUserInputVelocities()
            {
                UserInputForwardVelocity = 0f;
                UserInputRightVelocity = 0f;
                UserInputUpVelocity = 0f;
                UserInputRollVelocity = 0f;
                UserInputPitchVelocity = 0f;
                UserInputYawVelocity = 0f;
            }

            public static void SetAutopilotControl(bool thrusters, bool gyroscopes, bool dampeners)
            {
                if (thrusters) { FlightController.ReinstateThrusterControl(); } else { FlightController.RelinquishThrusterControl(); }
                if (gyroscopes) { FlightController.ReinstateGyroControl(); } else { FlightController.RelinquishGyroControl(); }
                RemoteControl.DampenersOverride = dampeners;
            }
        }
    }
}
