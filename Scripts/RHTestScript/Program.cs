
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRageMath;


namespace IngameScript
{
    public partial class Program : MyGridProgram
    {
        public static LogScreen _LogScreen { get; set; }

        public Vector3D TestVector { get; set; }

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Once;
            _LogScreen = new LogScreen(Me, 0, 1f);
        }

        public void Main(string argument, UpdateType updateSource)
        {
            float newFontSize;
            float.TryParse(argument, out newFontSize);
            if (newFontSize == default(float)) {
                AddToLogQueue("new font size not determined from input; defaulting to 0.5");
                newFontSize = 0.5f; }
            _LogScreen.Surface.FontSize = newFontSize;
            AddToLogQueue($"this is font size {_LogScreen.Surface.FontSize}");

            _LogScreen.Refresh();
        }

        public void AddToLogQueue(string message)
        {
            _LogScreen.Logs.Enqueue(new STULog("me", message, STULogType.INFO));
        }

    }
}
