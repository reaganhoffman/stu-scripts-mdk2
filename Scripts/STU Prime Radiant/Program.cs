using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;

namespace IngameScript {
    public partial class Program : MyGridProgram {

        const string UNIFIED_HOLO_DISPLAY_TAG = "UnifiedHolo";
        const string PRIME_RADIANT_RAYCASTER_TAG = "PrimeRadiantRaycaster";
        const string PRIME_RADIANT_MAIN_RAYCASTER_TAG = "PrimeRadiantMainRaycaster";
        const string OBSERVER_TAG = "PrimeRadiantObserver";
        const string OBSERVER_SENSOR_TAG = "PrimeRadiantObserverSensor";

        MyIni _ini = new MyIni();
        UnifiedHolo _unifiedHolo;

        List<Vector3> _tempTargets = new List<Vector3> {
            new Vector3(-36135.47,-35919.1,-37193.12),
            new Vector3(-36133.7,-35925.19,-37188.17),
            new Vector3(-36143.61,-35916.06,-37192.36),
            new Vector3(-36140.23,-35923.19,-37189.45),
            new Vector3(-36142.61,-35920.08,-37190.05),
            new Vector3(-36133.92,-35926.6,-37194.5)
        };

        Dictionary<long, MyDetectedEntityInfo> _targets = new Dictionary<long, MyDetectedEntityInfo>();
        IMyCameraBlock _mainCamera;
        List<IMyCameraBlock> _cameras = new List<IMyCameraBlock>();

        Dictionary<long, float> _timeSinceLastUpdate = new Dictionary<long, float>();
        Dictionary<IMyCameraBlock, double> _requiredScanDistances = new Dictionary<IMyCameraBlock, double>();
        Dictionary<IMyCameraBlock, List<MyDetectedEntityInfo>> _cameraTargets = new Dictionary<IMyCameraBlock, List<MyDetectedEntityInfo>>();

        public Program() {
            _unifiedHolo = new UnifiedHolo(DiscoverUnifiedHoloDisplays(), InitializeObserverSensor(), Echo);
            _cameras = InitializeCameras();
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Main(string arg) {
            if (arg == "scan") {
                // scan 2000 meters ahead with the main camera
                var info = _mainCamera.Raycast(2000);
                if (!info.IsEmpty()) {
                    Echo("hit");
                    _targets[info.EntityId] = info;
                    _timeSinceLastUpdate[info.EntityId] = 0; // reset time since last update for this target
                    Echo(_targets.Count.ToString());
                } else {
                    Echo("Main camera raycast failed.");
                }
            }
            UpdateTargets();
            _unifiedHolo.Update(_targets);
        }

        public void UpdateTargets() {

            // Clear previous targets
            _cameraTargets.Clear();

            // Stage 1: Find which cameras can see which targets
            foreach (var target in _targets) {
                _timeSinceLastUpdate[target.Key] += (float)Runtime.TimeSinceLastRun.TotalSeconds;
                Vector3D estimatedPosition = target.Value.Position + target.Value.Velocity * _timeSinceLastUpdate[target.Key];
                foreach (var camera in _cameras) {
                    if (camera.CanScan(estimatedPosition)) {
                        if (!_cameraTargets.ContainsKey(camera)) {
                            _cameraTargets[camera] = new List<MyDetectedEntityInfo>();
                        }
                        _cameraTargets[camera].Add(target.Value);
                    }
                }
            }

            // Stage 2: For each camera, determine the required scan distance
            foreach (var camera in _cameras) {
                if (_cameraTargets.ContainsKey(camera)) {
                    double requiredDistance = 0;
                    foreach (var target in _cameraTargets[camera]) {
                        double distance = Vector3D.Distance(camera.GetPosition(), target.Position);
                        requiredDistance += distance;
                    }
                    _requiredScanDistances[camera] = requiredDistance;
                } else {
                    _requiredScanDistances[camera] = 0; // No targets for this camera
                }
            }

            // Stage 3: Update only those cameras which can update all of their targets
            foreach (var camera in _cameras) {
                if (_cameraTargets.ContainsKey(camera)) {
                    double requiredDistance = _requiredScanDistances[camera];
                    if (camera.AvailableScanRange >= requiredDistance) {
                        _cameraTargets[camera].ForEach(target => {
                            MyDetectedEntityInfo info = camera.Raycast(target.Position);
                            if (!info.IsEmpty()) {
                                _targets[target.EntityId] = info;
                            } else {
                                // TODO: Handle case where raycast fails
                            }
                        });
                    }
                }
            }

        }

        List<STUDisplay> DiscoverUnifiedHoloDisplays() {
            List<IMyTextPanel> logPanels = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType(
                logPanels,
                block => MyIni.HasSection(block.CustomData, UNIFIED_HOLO_DISPLAY_TAG)
                && block.CubeGrid == Me.CubeGrid);
            if (logPanels.Count == 0) {
                throw new Exception($"No Unified Holo displays found with tag '{UNIFIED_HOLO_DISPLAY_TAG}'");
            }
            return new List<STUDisplay>(logPanels.ConvertAll(panel => {
                MyIniParseResult result;
                if (!_ini.TryParse(panel.CustomData, out result)) {
                    throw new Exception($"Error parsing log configuration: {result}");
                }
                return new STUDisplay(panel, 0);
            }));
        }

        List<IMyCameraBlock> InitializeCameras() {
            List<IMyCameraBlock> mainCamera = new List<IMyCameraBlock>();
            GridTerminalSystem.GetBlocksOfType(
                mainCamera,
                block => MyIni.HasSection(block.CustomData, PRIME_RADIANT_MAIN_RAYCASTER_TAG)
                && block.CubeGrid == Me.CubeGrid);
            if (mainCamera.Count > 1) {
                throw new Exception($"Multiple main raycaster cameras found with tag '{PRIME_RADIANT_MAIN_RAYCASTER_TAG}'");
            } else if (mainCamera.Count == 0) {
                throw new Exception($"No main raycaster camera found with tag '{PRIME_RADIANT_MAIN_RAYCASTER_TAG}'");
            }
            _mainCamera = mainCamera[0];

            List<IMyCameraBlock> allCameras = new List<IMyCameraBlock>();
            GridTerminalSystem.GetBlocksOfType(
                allCameras,
                block => MyIni.HasSection(block.CustomData, PRIME_RADIANT_RAYCASTER_TAG)
                && block.CubeGrid == Me.CubeGrid);
            allCameras = allCameras.Append(_mainCamera).ToList();
            allCameras.ForEach(camera => {
                camera.Enabled = true;
                camera.EnableRaycast = true;
            });
            if (allCameras.Count == 0) {
                throw new Exception($"No cameras found with tag '{PRIME_RADIANT_RAYCASTER_TAG}'");
            }
            return allCameras;
        }

        IMySensorBlock InitializeObserverSensor() {
            // find block with tag "PrimeRadiantObserverSensor"
            List<IMySensorBlock> sensors = new List<IMySensorBlock>();
            GridTerminalSystem.GetBlocksOfType(sensors, block => MyIni.HasSection(block.CustomData, OBSERVER_SENSOR_TAG) && block.CubeGrid == Me.CubeGrid);
            if (sensors.Count == 0) {
                throw new Exception($"No observer sensor found with tag '{OBSERVER_SENSOR_TAG}'");
            }
            if (sensors.Count > 1) {
                throw new Exception($"Multiple observer sensors found with tag '{OBSERVER_SENSOR_TAG}'");
            }
            return sensors[0];
        }

        public void echo(string s) {
            Echo(s);
        }

    }
}