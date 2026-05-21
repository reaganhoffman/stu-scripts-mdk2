using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public partial class CBT
        {
            public class AcquireTarget : STUStateMachine
            {
                public override string Name => "Reconnoiterer";
                static CBT ThisCBT { get; set; }
                Queue<STUStateMachine> ManeuverQueue { get; set; }
                bool InitialRaycastState { get; set; }
                public MyDetectedEntityInfo Raycast { get; set; }
                public Dictionary<string, string> SerializedHitInfo { get; set; }
                public bool PilotConfirmation { get; set; } = false;
                float TOLscreenRefreshTick { get; set; }


                public AcquireTarget(CBT thisCBT, Queue<STUStateMachine> thisManeuverQueue)
                {
                    ThisCBT = thisCBT;
                    ManeuverQueue = thisManeuverQueue;
                    PushLIGMAStatusToBottomCameraScreens("CHARGING\nRAYCAST");
                    InitialRaycastState = Camera.EnableRaycast;
                    Camera.EnableRaycast = true;
                }

                public override bool Init()
                {
                    if (Camera.CanScan(2000))
                    {
                        Raycast = Camera.Raycast(2000, 0, 0);
                        string s = $"Target acquired:\n" +
                            $"{Raycast.Type}\n" +
                            $"{Raycast.Name}\n" +
                            $"{Raycast.Relationship}";
                        AddToLogQueue(s);
                        PushLIGMAStatusToBottomCameraScreens(s);
                        SerializedHitInfo = STURaycaster.GetHitInfoDictionary(Raycast);
                        return true;
                    }
                    return false;
                }

                public override bool Run()
                {
                    PushTOLStatusToBottomCameraScreens("CONFIRM\nTARGET\nCOORDINATES?");
                    if (PilotConfirmation)
                    {
                        CBT.LIGMABroadcaster.Log(new STULog()
                        {
                            Message = "UpdateTargetData",
                            Sender = CBT_VARIABLES.CBT_VEHICLE_NAME,
                            Type = STULogType.INFO,
                            Metadata = SerializedHitInfo
                        });
                        PushTOLStatusToBottomCameraScreens("SENT");
                        TOLscreenRefreshTick = CBT.Runtime.LifetimeTicks + 6;
                        return true;
                    }
                    return false;
                }

                public override bool Closeout()
                {
                    if (CBT.Runtime.LifetimeTicks >= TOLscreenRefreshTick)
                    {
                        Camera.EnableRaycast = InitialRaycastState;
                        PushLIGMAStatusToBottomCameraScreens("");
                        PushTOLStatusToBottomCameraScreens("");
                        return true;
                    }
                    return false;
                }
            }
        }
    }
}
