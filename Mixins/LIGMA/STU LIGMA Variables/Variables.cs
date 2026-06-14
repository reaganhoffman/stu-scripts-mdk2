namespace IngameScript {
    partial class Program {
        public static class LIGMA_VARIABLES {

            public const string LIGMA_VEHICLE_NAME = "LIGMA-I";
            public const string LIGMA_RECONNOITERER_NAME = "SDC-3";

            public const string LIGMA_TELEMETRY_CHANNEL = "LIGMA_TELEMETRY_BROADCASTER"; // + firingGroup; blank at first, then attempts to self-assign, remains blank on fail
            public const string LIGMA_LOG_CHANNEL = "LIGMA_LOG_BROADCASTER"; // + firingGroup; blank at first, then attempts to self-assign, remains blank on fail
            public const string LIGMA_GOOCH_TARGET_BROADCASTER = "LIGMA_GOOCH_TARGET_BROADCASTER";
            public const string LIGMA_GOOCH_LOG_BROADCASTER = "LIGMA_GOOCH_LOG_BROADCASTER";

            public const string LIGMA_HQ_LOG_SUBSCRIBER_TAG = "LIGMA_HQ_LOG_SUBSCRIBER";
            public const string LIGMA_HQ_TELEMETRY_BROADCASTER = "LIGMA_HQ_TELEMETRY_BROADCASTER";
            public const string LIGMA_HQ_MAIN_SUBSCRIBER_TAG = "LIGMA_HQ_MAIN_SUBSCRIBER";
            public const string LIGMA_HQ_NAME = "LIGMA_HQ";

            public const string BALLS_ANNOUNCEMENT_CHANNEL = "BALLS_ANNOUNCEMENT_CHANNEL";
            public const string BALLS_STATION_NAME = "BALLS-1";
            public const string VIRGIN_LIGMA_RESPONSE_CHANNEL = "VIRGIN_LIGMA_RESPONSE_CHANNEL";

            public const double PLANETARY_DETECTION_BUFFER = 2000;

            public static class COMMANDS {
                public const string Launch = "Launch";
                public const string Detonate = "Detonate";
                public const string UpdateTargetData = "UpdateTargetData";
                public const string Test = "Test";
                public const string SendGoodbye = "SendGoodbye";
                public const string SubscribeToFiringGroup = "SubscribeToFiringGroup";
            }

        }
    }
}
