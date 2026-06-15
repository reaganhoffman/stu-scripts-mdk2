
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRageMath;


namespace IngameScript
{
    public partial class Program : MyGridProgram
    {
        public IMyBroadcastListener Listener { get; set; }
        public List<IMyBroadcastListener> FoundListeners { get; set; } = new List<IMyBroadcastListener>();

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            Listener = IGC.RegisterBroadcastListener("BALLS_DISCOVERY_CHANNEL");
        }

        public void Main(string argument, UpdateType updateSource)
        {
            IGC.GetBroadcastListeners(FoundListeners);
            Echo($"GetBroadcastListeners: \n{FoundListeners}\nCount: {FoundListeners.Count}\nTag of 0th index: {FoundListeners[0].Tag.ToString()}");
            FoundListeners.Clear();
        }

        //public void AddToLogQueue(string message)
        //{
        //    _LogScreen.Logs.Enqueue(new STULog("me", message, STULogType.INFO));
        //}

    }
}
