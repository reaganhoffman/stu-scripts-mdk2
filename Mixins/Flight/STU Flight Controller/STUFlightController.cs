using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using VRageMath;

namespace IngameScript {
    partial class Program {

        public partial class STUFlightController {

            // Maneuver classes
            public HardStop HardStopManeuver { get; private set; }
            public GotoAndStop GotoAndStopManeuver { get; set; }
            public NavigateOverPlanetSurface NavigateOverPlanetSurfaceManeuver { get; set; }

            public static Queue<STULog> FlightLogs = new Queue<STULog>();
            private const string FLIGHT_CONTROLLER_LOG_NAME = "STU-FC";
            private const string FLIGHT_CONTROLLER_STANDARD_OUTPUT_TAG = "FLIGHT_CONTROLLER_STANDARD_OUTPUT";

            StandardOutput[] _standardOutputDisplays { get; set; }

            public IMyShipController ShipController { get; protected set; }

            public bool HasGyroControl { get; private set; }
            public bool HasThrusterControl { get; private set; }

            public double TargetVelocity { get; protected set; }
            public double VelocityMagnitude { get; protected set; }
            public Vector3D CurrentVelocity_LocalFrame { get; protected set; }
            public Vector3D CurrentVelocity_WorldFrame { get; protected set; }
            public Vector3D AccelerationComponents { get; protected set; }

            public Vector3D CurrentPosition { get; protected set; }

            public MatrixD CurrentWorldMatrix { get; protected set; }
            public MatrixD PreviousWorldMatrix { get; protected set; }

            public IMyThrust[] ActiveThrusters { get; protected set; }
            public IMyGyro[] AllGyroscopes { get; protected set; }

            STUVelocityController _velocityController { get; set; }
            STUOrientationController _orientationController { get; set; }
            STUAltitudeController _altitudeController { get; set; }
            STUPointOrbitController _pointOrbitController { get; set; }
            STUPlanetOrbitController _planetOrbitController { get; set; }

            /// <summary>
            /// Flight utility class that handles velocity control and orientation control. Requires exactly one Remote Control block to function.
            /// Be sure to orient the Remote Control block so that its forward direction is the direction you want to be considered the "forward" direction of your ship.
            /// Also orient the Remote Control block so that its up direction is the direction you want to be considered the "up" direction of your ship.
            /// </summary>
            public STUFlightController(IMyGridTerminalSystem grid, IMyShipController shipController, IMyTerminalBlock me) {
                ShipController = shipController;
                ActiveThrusters = FindThrusters(grid, me);
                AllGyroscopes = FindGyros(grid, me);
                TargetVelocity = 0;
                _velocityController = new STUVelocityController(ShipController, ActiveThrusters);
                _orientationController = new STUOrientationController(ShipController, AllGyroscopes);
                _altitudeController = new STUAltitudeController(this, ShipController);
                _pointOrbitController = new STUPointOrbitController(this, ShipController);
                _planetOrbitController = new STUPlanetOrbitController(this);
                HasGyroControl = true;
                _standardOutputDisplays = FindStandardOutputDisplays(grid, me);
                HardStopManeuver = new HardStop(this);
                UpdateState();
                CreateOkFlightLog("Flight controller initialized.");
            }

            public void UpdateThrustersAfterGridChange(IMyThrust[] newActiveThrusters) {
                ActiveThrusters = newActiveThrusters;
                _velocityController = new STUVelocityController(ShipController, newActiveThrusters);
                _altitudeController = new STUAltitudeController(this, ShipController);
            }

            public void UpdateGyrosAfterGridChange(IMyGyro[] newActiveGyros) {
                AllGyroscopes = newActiveGyros;
                _orientationController = new STUOrientationController(ShipController, newActiveGyros);
            }

            public void MeasureCurrentVelocity() {
                CurrentVelocity_WorldFrame = ShipController.GetShipVelocities().LinearVelocity;
                CurrentVelocity_LocalFrame = STUTransformationUtils.WorldDirectionToLocalDirection(ShipController, CurrentVelocity_WorldFrame);
                VelocityMagnitude = CurrentVelocity_LocalFrame.Length();
            }

            public void MeasureCurrentAcceleration() {
                AccelerationComponents = _velocityController.CalculateAccelerationVectors();
            }

            public void MeasureCurrentPositionAndOrientation() {
                CurrentWorldMatrix = ShipController.WorldMatrix;
                CurrentPosition = ShipController.GetPosition();
            }

            public double GetCurrentSurfaceAltitude() {
                return _altitudeController.CurrentSurfaceAltitude;
            }

            public double GetCurrentSeaLevelAltitude() {
                return _altitudeController.CurrentSeaLevelAltitude;
            }

            public void ToggleThrusters(bool on) {
                foreach (var thruster in ActiveThrusters) {
                    thruster.Enabled = on;
                }
            }

            public double CalculateStoppingDistance() {
                double maxReverseAcceleration = _velocityController.CalculateMaxAcceleration_WorldFrame(CurrentVelocity_WorldFrame);
                return CalculateStoppingDistance(maxReverseAcceleration, CurrentVelocity_WorldFrame.Length());
            }

            public double CalculateStoppingDistance(double acceleration, double velocity) {
                double dx = (velocity * velocity) / (2 * acceleration); // kinematic equation for "how far would I travel if I slammed on the brakes right now?"
                return dx;
            }

            public bool DetectCollisionCourseWithPlanet()
            {
                return Vector3D.Dot(CurrentVelocity_WorldFrame, ShipController.GetNaturalGravity()) > GetCurrentSurfaceAltitude();
            }

            /// <summary>
            /// Updates various aspects of the ship's state, including velocity, acceleration, position, and orientation.
            /// This must be called on every tick to ensure that the ship's state is up-to-date!
            /// </summary>
            public void UpdateState() {
                _velocityController.UpdateState();
                _altitudeController.UpdateState();
                MeasureCurrentPositionAndOrientation();
                MeasureCurrentVelocity();
                MeasureCurrentAcceleration();
                DrawToStandardOutputs();
            }

            private void DrawToStandardOutputs() {
                for (int i = 0; i < _standardOutputDisplays.Length; i++) {
                    _standardOutputDisplays[i].DrawTelemetry();
                }
            }


            /// <summary>
            /// Attempts to reach desiredVelocity in the world frame. Pass in a reference position to align the ship with that position. Returns true if the ship's velocity is stable.
            /// </summary>
            /// <param name="targetPos"></param>
            /// <param name="desiredVelocity"></param>
            /// <param name="referencePos"></param>
            /// <returns></returns>
            public bool SetV_WorldFrame(
                Vector3D targetPos, 
                double desiredVelocity, 
                Vector3D referencePos, 
                STUVelocityController.OverrideMode overrideMode = STUVelocityController.OverrideMode.IGNORE_PLAYER_INPUT) 
            {
                return _velocityController.SetV_WorldFrame(targetPos, CurrentVelocity_WorldFrame, referencePos, desiredVelocity, overrideMode);
            }

            /// <summary>
            /// Attempts to reach desiredVelocity in the world frame. Pass in a reference block to align that block with targetPos. Returns true if the ship's velocity is stable.
            /// </summary>
            /// <param name="targetPos"></param>
            /// <param name="desiredVelocity"></param>
            /// <param name="reference"></param>
            /// <returns></returns>
            public bool SetV_WorldFrame(
                Vector3D targetPos, 
                double desiredVelocity, 
                IMyTerminalBlock reference, 
                STUVelocityController.OverrideMode overrideMode = STUVelocityController.OverrideMode.IGNORE_PLAYER_INPUT) 
            {
                return _velocityController.SetV_WorldFrame(targetPos, CurrentVelocity_WorldFrame, reference.GetPosition(), desiredVelocity, overrideMode);
            }

            /// <summary>
            /// Attempts to reach desiredVelocity in the world frame. Returns true if the ship's velocity is stable.
            /// </summary>
            /// <param name="targetPos"></param>
            /// <param name="desiredVelocity"></param>
            /// <returns></returns>
            public bool SetV_WorldFrame(
                Vector3D targetPos, 
                double desiredVelocity, 
                STUVelocityController.OverrideMode overrideMode = STUVelocityController.OverrideMode.IGNORE_PLAYER_INPUT) 
            {
                return _velocityController.SetV_WorldFrame(targetPos, CurrentVelocity_WorldFrame, CurrentPosition, desiredVelocity, overrideMode);
            }

            public bool SetV_WorldFrame(
                Base6Directions.Direction desiredDirection, 
                double desiredVelocity, 
                IMyTerminalBlock reference = null, 
                STUVelocityController.OverrideMode overrideMode = STUVelocityController.OverrideMode.IGNORE_PLAYER_INPUT) 
            {
                if (reference == null) {
                    reference = ShipController;
                }
                Vector3D direction = GetWorldDirection(reference, desiredDirection);
                Vector3D targetPos = reference.GetPosition() + direction * 1000;
                return _velocityController.SetV_WorldFrame(targetPos, CurrentVelocity_WorldFrame, CurrentPosition, desiredVelocity, overrideMode);
            }

            public void HardStopGyros() { _orientationController.HardStopGyros(); }
            public void SetVr(double rollSpeed) { _orientationController.SetVr(rollSpeed); }
            public void SetVp(double pitchSpeed) { _orientationController.SetVp(pitchSpeed); }
            public void SetVw(double yawSpeed) { _orientationController.SetVw(yawSpeed); }

            Vector3D GetWorldDirection(IMyTerminalBlock block, Base6Directions.Direction dir) {
                switch (dir) {
                    case Base6Directions.Direction.Forward:
                        return block.WorldMatrix.Forward;
                    case Base6Directions.Direction.Backward:
                        return block.WorldMatrix.Backward;
                    case Base6Directions.Direction.Left:
                        return block.WorldMatrix.Left;
                    case Base6Directions.Direction.Right:
                        return block.WorldMatrix.Right;
                    case Base6Directions.Direction.Up:
                        return block.WorldMatrix.Up;
                    case Base6Directions.Direction.Down:
                        return block.WorldMatrix.Down;
                    default:
                        return Vector3D.Zero;
                }
            }

            public bool Hover()
            {
                return _velocityController.SetV_WorldFrame(CurrentPosition, CurrentVelocity_WorldFrame, CurrentPosition, 0);
            }

            /// <summary>
            /// Sets the ship's axial velocities.
            /// </summary>
            /// <param name="axialVelocities">This vector's components correlate to the same axes as its planar counterpart; this variable's 'X' component should describe a desired rotation about the X axis, i.e. pitch. Similarly, this variable's 'Y' component should describe a desired yaw, and Z should describe roll.</param>
            public void SetAxialVelocity(Vector3D axialVelocities)
            {
                _orientationController.SetVp(axialVelocities.X);
                _orientationController.SetVw(axialVelocities.Y);
                _orientationController.SetVr(axialVelocities.Z);
            }

            /// <summary>
            /// Aligns the ship's forward direction with the target position. Returns true if the ship is aligned.
            /// </summary>
            /// <param name="targetPos"></param>
            /// <returns></returns>
            public bool AlignShipToTarget(Vector3D targetPos, IMyTerminalBlock referenceBlock = null, string referenceBlockFace = null, Vector3 humanInput = new Vector3()) {
                ReinstateGyroControl();
                return _orientationController.AlignShipToTarget(targetPos, referenceBlock, referenceBlockFace, humanInput);
            }

            /// <summary>
            /// Rolls the ship to optimize the ship's inertia vector for a given target position. Effectively allows the ship to turn faster, at the cost of more fuel.
            /// </summary>
            /// <param name="targetPos"></param>
            public void OptimizeShipRoll(Vector3D targetPos) {
                Vector3D inertiaHeadingNormal = GetInertiaHeadingNormal(targetPos);
                if (inertiaHeadingNormal == Vector3D.Zero) { return; }

                // Get the current orientation of the ship
                MatrixD currentOrientation = CurrentWorldMatrix.GetOrientation();

                // Define the normals for two perpendicular lateral faces in the ship's local space
                Vector3D[] lateralFaceNormals = {
                    new Vector3D(0, 1, 0),  // Up
                    new Vector3D(1, 0, 0)   // Right
                };

                double smallestAngle = double.PositiveInfinity;
                double rollAdjustment = 0;

                // Find the lateral face normal with the smallest needed roll adjustment
                foreach (var lateralNormal in lateralFaceNormals) {
                    Vector3D worldNormal = Vector3D.TransformNormal(lateralNormal, currentOrientation);
                    double angle = CalculateRollAngle(inertiaHeadingNormal, worldNormal);

                    if (Math.Abs(angle) < Math.Abs(smallestAngle)) {
                        smallestAngle = angle;
                        rollAdjustment = smallestAngle;
                    }
                }

                // If close enough, stop the roll
                if (Math.Abs(rollAdjustment) < 0.003) {
                    _orientationController.SetVr(0);
                } else {
                    _orientationController.SetVr(rollAdjustment);
                }
            }

            /// <summary>
            /// Calculates a normal vector for the plane containing the ship's inertia vector and the vector from the ship to the target.
            /// </summary>
            /// <param name="targetPos"></param>
            /// <returns></returns>
            private Vector3D GetInertiaHeadingNormal(Vector3D targetPos) {
                // ship inertia vector
                Vector3D worldVelocity = Vector3D.Normalize(Vector3D.TransformNormal(CurrentVelocity_LocalFrame, CurrentWorldMatrix));
                // ship-to-target vector
                Vector3D ST = Vector3D.Normalize(targetPos - CurrentPosition);
                // normal vector of plane containing SI and ST
                Vector3D crossProduct = Vector3D.Cross(worldVelocity, ST);

                // Cross products approach zero as the two vectors approach parallel
                // In other words, the ship is moving directly towards the target, so no need to roll
                if (Math.Abs(crossProduct.Length()) < 0.01) {
                    return Vector3D.Zero;
                }

                return Vector3D.Normalize(crossProduct);
            }

            /// <summary>
            /// Calculate the roll angle needed to offset the ship's local lateral face normal 45 degrees from velocity-heading normal.
            /// </summary>
            /// <param name="normalVector"></param>
            /// <param name="lateralFaceNormal"></param>
            /// <returns></returns>
            private double CalculateRollAngle(Vector3D normalVector, Vector3D lateralFaceNormal) {
                var dotProduct = Vector3D.Dot(normalVector, lateralFaceNormal);
                var angle = Math.Acos(dotProduct);
                return angle - Math.PI / 4;
            }

            public Vector3D GetAltitudeVelocityChangeForceVector(double desiredVelocity, double altitudeVelocity) {
                // Note that LocalGravityVector has already been transformed by the ship's orientation
                Vector3D localGravityVector = _velocityController.LocalGravityVector;

                // Calculate the magnitude of the gravitational force
                double gravityForceMagnitude = localGravityVector.Length();

                if (gravityForceMagnitude == 0) {
                    return Vector3D.Zero;
                }

                // Total mass of the ship
                double mass = STUVelocityController.ShipMass;

                // Total force needed: F = ma; a acts as basic proportional controlller here
                double totalForceNeeded = mass * (gravityForceMagnitude + desiredVelocity - altitudeVelocity);

                // Normalize the gravity vector to get the direction
                Vector3D unitGravityVector = localGravityVector / gravityForceMagnitude;

                // Calculate the force vector needed (opposite to gravity and scaled by totalForceNeeded)
                Vector3D outputForce = -unitGravityVector * totalForceNeeded;

                return outputForce;
            }

            public void OrbitPoint(Vector3D targetPos) {
                _pointOrbitController.Run(targetPos);
            }

            public void OrbitPlanet() {
                _planetOrbitController.Run();
            }

            /// <summary>
            /// Sets the ship's altitude to the target altitude. Returns true if the ship's altitude is stable.
            /// Pass in desiredAltitudeVelocity if you want the ship to reach the target altitude at a certain speed.
            /// </summary>
            /// <param name="targetAltitude"></param>
            /// <param name="desiredAltitudeVelocity"></param>
            /// <returns></returns>
            public bool MaintainSurfaceAltitude(double targetAltitude, double ascendVelocity, double descendVelocity) {
                return _altitudeController.MaintainSurfaceAltitude(targetAltitude, ascendVelocity, descendVelocity);
            }

            /// <summary>
            /// Sets the ship's altitude to the target altitude. Returns true if the ship's altitude is stable.
            /// Pass in desiredAltitudeVelocity if you want the ship to reach the target altitude at a certain speed.
            /// </summary>
            /// <param name="targetAltitude"></param>
            /// <param name="desiredAltitudeVelocity"></param>
            /// <returns></returns>
            public bool MaintainSeaLevelAltitude(double targetAltitude, double ascendVelocity, double descendVelocity) {
                return _altitudeController.MaintainSeaLevelAltitude(targetAltitude, ascendVelocity, descendVelocity);
            }


            public void UpdateShipMass() {
                STUVelocityController.ShipMass = ShipController.CalculateShipMass().PhysicalMass;
            }

            public double GetShipMass() {
                return STUVelocityController.ShipMass;
            }

            public void RelinquishThrusterControl() {
                foreach (var thruster in ActiveThrusters) {
                    thruster.ThrustOverride = 0;
                }
                HasThrusterControl = false;
            }

            public void RelinquishGyroControl() {
                foreach (var gyro in AllGyroscopes) {
                    gyro.Pitch = 0;
                    gyro.Roll = 0;
                    gyro.Yaw = 0;
                    gyro.GyroOverride = false;
                }
                HasGyroControl = false;
            }

            public void ReinstateGyroControl() {
                foreach (var gyro in AllGyroscopes) {
                    gyro.GyroOverride = true;
                }
                HasGyroControl = true;
            }

            public void ReinstateThrusterControl() {
                foreach (var thruster in ActiveThrusters) {
                    thruster.ThrustOverride = 0;
                }
                HasThrusterControl = true;
            }

            private StandardOutput[] FindStandardOutputDisplays(IMyGridTerminalSystem grid, IMyTerminalBlock me) {
                List<StandardOutput> output = new List<StandardOutput>();
                List<IMyTerminalBlock> allBlocksOnGrid = new List<IMyTerminalBlock>();
                grid.GetBlocks(allBlocksOnGrid);
                allBlocksOnGrid.Where((block) => block.CubeGrid == me.CubeGrid);
                foreach (var block in allBlocksOnGrid) {
                    string customDataRawText = block.CustomData;
                    string[] customDataLines = customDataRawText.Split('\n');
                    foreach (var line in customDataLines) {
                        if (line.Contains(FLIGHT_CONTROLLER_STANDARD_OUTPUT_TAG)) {
                            string[] kvp = line.Split(':');
                            // adjust font size based on what screen we're trying to initalize
                            float fontSize;
                            try {
                                fontSize = float.Parse(kvp[2]);
                                if (fontSize < 0.1f || fontSize > 10f) {
                                    throw new Exception("Invalid font size");
                                }
                            } catch (Exception e) {
                                CreateWarningFlightLog("Invalid font size for display " + block.CustomName + ". Defaulting to 0.5");
                                CreateWarningFlightLog(e.Message);
                                fontSize = 0.5f;
                            }
                            StandardOutput screen = new StandardOutput(block, int.Parse(kvp[1]), "Monospace", fontSize);
                            output.Add(screen);
                        }
                    }
                }
                return output.ToArray();
            }

            public STUGalacticMap.Planet? GetPlanetOfPoint(Vector3D point) {
                foreach (var kvp in STUGalacticMap.CelestialBodies) {
                    STUGalacticMap.Planet planet = kvp.Value;
                    BoundingSphereD sphere = new BoundingSphereD(planet.Center, 1.76f * planet.Radius);
                    // if the point is inside the planet's detection sphere or intersects it, it is on the planet
                    if (sphere.Contains(point) == ContainmentType.Contains || sphere.Contains(point) == ContainmentType.Intersects) {
                        return planet;
                    }
                }
                return null;
            }

            public static void CreateOkFlightLog(string message) {
                CreateFlightLog(message, STULogType.OK);
            }

            public static void CreateErrorFlightLog(string message) {
                CreateFlightLog(message, STULogType.ERROR);
            }

            public static void CreateInfoFlightLog(string message) {
                CreateFlightLog(message, STULogType.INFO);
            }

            public static void CreateWarningFlightLog(string message) {
                CreateFlightLog(message, STULogType.WARNING);
            }

            public static void CreateFatalFlightLog(string message) {
                CreateFlightLog(message, STULogType.ERROR);
                throw new Exception(message);
            }

            private static void CreateFlightLog(string message, STULogType type) {
                FlightLogs.Enqueue(new STULog {
                    Message = message,
                    Type = type,
                    Sender = FLIGHT_CONTROLLER_LOG_NAME
                });
            }

            IMyThrust[] FindThrusters(IMyGridTerminalSystem grid, IMyTerminalBlock me) {
                List<IMyTerminalBlock> thrusterBlocks = new List<IMyTerminalBlock>();
                grid.GetBlocksOfType<IMyThrust>(thrusterBlocks, block => block.CubeGrid == me.CubeGrid);
                if (thrusterBlocks.Count == 0) {
                    CreateFatalFlightLog("No thrusters found on grid");
                }
                IMyThrust[] allThrusters = new IMyThrust[thrusterBlocks.Count];
                for (int i = 0; i < thrusterBlocks.Count; i++) {
                    allThrusters[i] = thrusterBlocks[i] as IMyThrust;
                }
                CreateOkFlightLog("Thrusters loaded successfully");
                return allThrusters;
            }

            IMyGyro[] FindGyros(IMyGridTerminalSystem grid, IMyTerminalBlock me) {
                List<IMyTerminalBlock> gyroBlocks = new List<IMyTerminalBlock>();
                grid.GetBlocksOfType<IMyGyro>(gyroBlocks, block => block.CubeGrid == me.CubeGrid);
                if (gyroBlocks.Count == 0) {
                    CreateFatalFlightLog("No gyros found on grid");
                }
                IMyGyro[] gyros = new IMyGyro[gyroBlocks.Count];
                for (int i = 0; i < gyroBlocks.Count; i++) {
                    gyros[i] = gyroBlocks[i] as IMyGyro;
                }
                CreateOkFlightLog("Gyros loaded successfully");
                return gyros;
            }

            public void ToggleDampeners(bool on) {
                ShipController.DampenersOverride = on;
            }

        }
    }
}
