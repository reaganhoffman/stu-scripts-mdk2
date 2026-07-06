
using System;
using System.Collections.Generic;
using System.Text;
using VRageMath;

namespace IngameScript {
    partial class Program {

        public static class AUTO_MINER_VARIABLES {

            public const string AUTO_MINER_RECON_NAME = "AM-R";
            public const string AUTO_MINER_RECON_LOG_SUBSCRIBER_TAG = "AUTO_MINER_RECON_LOG_SUBSCRIBER";

            public const string AUTO_MINER_HQ_RECON_JOB_LISTENER = "AUTO_MINER_HQ_RECON_JOB_LISTENER";

            public const string AUTO_MINER_HQ_DRONE_TELEMETRY_CHANNEL = "AUTO_MINER_HQ_DRONE_TELEMETRY_CHANNEL";
            public const string AUTO_MINER_HQ_DRONE_LOG_CHANNEL = "AUTO_MINER_HQ_DRONE_LOG_CHANNEL";

            public const string AUTO_MINER_DRONE_COMMAND_CHANNEL = "AUTO_MINER_DRONE_COMMAND_CHANNEL";

            public const string AUTO_MINER_HQ_COMMAND_CHANNEL = "AUTO_MINER_HQ_COMMAND_CHANNEL";
            public const string AUTO_MINER_HQ_NAME = "AM-HQ";

            public const string AUTO_MINER_LOG_SUBSCRIBER_TAG = "AUTO_MINER_LOG_SUBSCRIBER";
            public const string AUTO_MINER_HQ_MAIN_SUBSCRIBER_TAG = "AUTO_MINER_MAIN_SUBSCRIBER";

        }

        public class MiningDroneData {

            public string Id { get; set; }
            public string Name { get; set; }
            public string State { get; set; }
            public Vector3D WorldPosition { get; set; }
            public Vector3D WorldVelocity { get; set; }
            public Vector3D WorldAcceleration { get; set; }
            public double HydrogenLiters { get; set; }
            public double HydrogenCapacity { get; set; }
            public double PowerMegawatts { get; set; }
            public double PowerCapacity { get; set; }
            public double CargoLevel { get; set; }
            public double CargoCapacity { get; set; }
            public PlaneD JobPlane { get; set; }
            public Vector3D JobSite { get; set; }
            public int JobDepth { get; set; }

            public static MiningDroneData Deserialize(Dictionary<string, string> droneData) {
                var drone = new MiningDroneData();
                foreach (var key in droneData.Keys) {
                    switch (key) {
                        case "Id":
                            drone.Id = droneData[key];
                            break;
                        case "Name":
                            drone.Name = droneData[key];
                            break;
                        case "State":
                            drone.State = droneData[key];
                            break;
                        case "WorldPosition":
                            drone.WorldPosition = DeserializeVector3D(droneData[key]);
                            break;
                        case "WorldVelocity":
                            drone.WorldVelocity = DeserializeVector3D(droneData[key]);
                            break;
                        case "WorldAcceleration":
                            drone.WorldAcceleration = DeserializeVector3D(droneData[key]);
                            break;
                        case "HydrogenLiters":
                            drone.HydrogenLiters = double.Parse(droneData[key]);
                            break;
                        case "HydrogenCapacity":
                            drone.HydrogenCapacity = double.Parse(droneData[key]);
                            break;
                        case "PowerMegawatts":
                            drone.PowerMegawatts = double.Parse(droneData[key]);
                            break;
                        case "PowerCapacity":
                            drone.PowerCapacity = double.Parse(droneData[key]);
                            break;
                        case "CargoLevel":
                            drone.CargoLevel = double.Parse(droneData[key]);
                            break;
                        case "CargoCapacity":
                            drone.CargoCapacity = double.Parse(droneData[key]);
                            break;
                        case "JobPlane":
                            drone.JobPlane = DeserializePlaneD(droneData[key]);
                            break;
                        case "JobSite":
                            drone.JobSite = DeserializeVector3D(droneData[key]);
                            break;
                        case "JobDepth":
                            drone.JobDepth = int.Parse(droneData[key]);
                            break;
                    }
                }

                return drone;
            }

            public static string FormatVector3D(Vector3D v) {
                return $"({v.X}, {v.Y}, {v.Z})";
            }

            public static Vector3D DeserializeVector3D(string s) {
                // Remove parentheses and split by comma
                s = s.Trim('(', ')');
                var parts = s.Split(',');
                if (parts.Length != 3)
                    throw new FormatException($"Invalid Vector3D format: {s}");

                var x = double.Parse(parts[0]);
                var y = double.Parse(parts[1]);
                var z = double.Parse(parts[2]);

                return new Vector3D(x, y, z);
            }

            public static PlaneD DeserializePlaneD(string s) {
                // Remove parentheses and split by comma
                s = s.Trim('(', ')');
                var parts = s.Split(',');
                if (parts.Length != 4)
                    throw new FormatException("Invalid PlaneD format");

                var x = double.Parse(parts[0]);
                var y = double.Parse(parts[1]);
                var z = double.Parse(parts[2]);
                var d = double.Parse(parts[3]);

                return new PlaneD(x, y, z, d);
            }

            public string Serialize() {
                var sb = new StringBuilder();
                sb.Append($"Id: {Id}; ");
                sb.Append($"Name: {Name}; ");
                sb.Append($"State: {State}; ");
                sb.Append($"WorldPosition: {FormatVector3D(WorldPosition)}; ");
                sb.Append($"WorldVelocity: {FormatVector3D(WorldVelocity)}; ");
                sb.Append($"WorldAcceleration: {FormatVector3D(WorldAcceleration)}; ");
                sb.Append($"HydrogenLiters: {HydrogenLiters}; ");
                sb.Append($"PowerMegawatts: {PowerMegawatts}; ");
                sb.Append($"HydrogenCapacity: {HydrogenCapacity}; ");
                sb.Append($"PowerCapacity: {PowerCapacity}; ");
                sb.Append($"CargoLevel: {CargoLevel}; ");
                sb.Append($"JobSite: {FormatVector3D(JobSite)}; ");
                sb.Append($"JobPlane: {FormatPlaneD(JobPlane)}; ");
                sb.Append($"JobDepth: {JobDepth}; ");
                sb.Append($"CargoCapacity: {CargoCapacity}");
                return sb.ToString();
            }

            public Dictionary<string, string> SerialDictionary() {
                Dictionary<string, string> dict = new Dictionary<string, string> {
                    ["Id"] = Id,
                    ["Name"] = Name,
                    ["State"] = State,
                    ["WorldPosition"] = FormatVector3D(WorldPosition),
                    ["WorldVelocity"] = FormatVector3D(WorldVelocity),
                    ["WorldAcceleration"] = FormatVector3D(WorldAcceleration),
                    ["HydrogenLiters"] = HydrogenLiters.ToString(),
                    ["PowerMegawatts"] = PowerMegawatts.ToString(),
                    ["HydrogenCapacity"] = PowerCapacity.ToString(),
                    ["PowerCapacity"] = PowerCapacity.ToString(),
                    ["CargoLevel"] = CargoLevel.ToString(),
                    ["JobSite"] = FormatVector3D(JobSite),
                    ["JobPlane"] = FormatPlaneD(JobPlane)
                };
                return dict;
            }

            public static string FormatPlaneD(PlaneD p) {
                return $"({p.Normal.X}, {p.Normal.Y}, {p.Normal.Z}, {p.D})";
            }

        }

        public class MinerState {
            public const string INITIALIZE = "INITIALIZE";
            public const string IDLE = "IDLE";
            public const string FLY_TO_JOB_SITE = "FLY_TO_JOB_SITE";
            public const string FLY_TO_HOME_BASE = "FLY_TO_HOME_BASE";
            public const string REFUELING = "REFUELING";
            public const string RECHARGING = "RECHARGING";
            public const string MINING = "MINING";
            public const string RTB = "RTB";
            public const string HARD_FAILURE = "HARD_FAILURE";
            public const string MISSING = "MISSING";
            public const string ALIGN_WITH_BASE_CONNECTOR = "ALIGN_WITH_BASE_CONNECTOR";
            public const string DOCKING = "DOCKING";
            public const string DEPOSIT_ORES = "DEPOSIT_ORES";
            public const string DEPOSIT_IDLE = "DEPOSIT_IDLE";
            public const string ERROR = "ERROR";
            public const string EMERGENCY_LANDING = "EMERGENCY_LANDING";
        }

    }
}
