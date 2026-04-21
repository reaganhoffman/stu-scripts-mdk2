using Sandbox.Game.Replication;
using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public partial class CBT
        {
            public class AcquireTarget : STUStateMachine
            {
                public override string Name => "Xmit Target";
                static CBT ThisCBT { get; set; }
                Queue<STUStateMachine> ManeuverQueue { get; set; }
                private bool InitialRaycastState { get; set; }
                public MyDetectedEntityInfo Raycast { get; set; }
                public Dictionary<string, string> SerializedHitInfo { get; set; }


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
                    return false;
                }

                public override bool Closeout()
                {
                    Camera.EnableRaycast = InitialRaycastState;
                    PushLIGMAStatusToBottomCameraScreens("");
                    return true;
                }
            }
        }
    }
}
