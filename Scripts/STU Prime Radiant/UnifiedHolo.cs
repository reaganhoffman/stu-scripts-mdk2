using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript {
    partial class Program {
        class UnifiedHolo {

            public Queue<STULog> Logs;

            const float _spriteWidth = 30f;
            const float _spriteHeight = 30f;

            const float LARGE_GRID_ONE_BLOCK_WIDTH = 2.5f; // width of large grid blocks
            const float LARGE_GRID_ONE_BLOCK_HEIGHT = 2.5f; // height of large grid blocks

            Vector3D _observerPosition;
            Vector3D _observerToTarget;
            Vector3D _screenPoint;

            Vector3 _worldHit;
            Vector3 _localHit;

            Vector2 _screenPosition;

            Plane _screenPlane;

            IMyCockpit _observerCockpit;

            List<STUDisplay> _displays;
            IMySensorBlock _observerSensor;

            MyDetectedEntityInfo _observer;

            Action<string> echo;

            public float ScreenOffset = 0.5f; // distance from the center of the display block to the screen portion of the block

            public UnifiedHolo(List<STUDisplay> displays, IMySensorBlock observerSensor, Action<string> echo, IMyCockpit observerCockpit) {
                _displays = displays;
                _observerSensor = observerSensor;
                _observerCockpit = observerCockpit;
                Logs = new Queue<STULog>();
                this.echo = echo;
            }

            public void Update(Dictionary<long, MyDetectedEntityInfo> targets) {
                foreach (var display in _displays) {
                    DrawTags(display, targets);
                }
            }

            void DrawTags(STUDisplay display, Dictionary<long, MyDetectedEntityInfo> targets) {

                _observer = _observerSensor.LastDetectedEntity;

                // SE takes the engineer's location to be the center of body's mass, so we adjust up toward the "eyes"
                if (_observerCockpit.IsUnderControl) {
                    // use cockpit
                    _observerPosition = _observerCockpit.GetPosition();
                } else {
                    // use player
                    _observerPosition = _observer.Position + _observer.Orientation.Up * 0.5;
                }

                display.StartFrame();

                Vector2 resolution = display.DisplayBlock.SurfaceSize; // (w,h)
                MySpriteDrawFrame sprites = display.CurrentFrame;


                foreach (var target in targets) {

                    var adjustedPosition = target.Value.Position;

                    _observerToTarget = adjustedPosition - _observerPosition;

                    _screenPoint = display.DisplayBlock.GetPosition() + display.DisplayBlock.WorldMatrix.Left * ScreenOffset;
                    _screenPlane = new Plane(_screenPoint,
                                           display.DisplayBlock.WorldMatrix.Right);

                    _worldHit = _screenPlane.Intersection(ref _observerPosition, ref _observerToTarget);
                    _localHit = STUTransformationUtils.WorldPositionToLocalPosition(display.DisplayBlock, _worldHit);

                    // map to [0..1]
                    float u = (LARGE_GRID_ONE_BLOCK_WIDTH * 0.5f - _localHit.Z) / LARGE_GRID_ONE_BLOCK_WIDTH;
                    float v = (LARGE_GRID_ONE_BLOCK_HEIGHT * 0.5f - _localHit.Y) / LARGE_GRID_ONE_BLOCK_HEIGHT;

                    // to pixels
                    _screenPosition = new Vector2(u * resolution.X, v * resolution.Y);

                    // if outside of the screen more than the size of the sprite, skip
                    if (_screenPosition.X < -_spriteWidth / 2 || _screenPosition.X > resolution.X + _spriteWidth / 2 ||
                        _screenPosition.Y < -_spriteHeight / 2 || _screenPosition.Y > resolution.Y + _spriteHeight / 2) {
                        continue;
                    }

                    var tag = new MySprite() {
                        Type = SpriteType.TEXTURE,
                        Data = "Circle",
                        Alignment = TextAlignment.CENTER,
                        RotationOrScale = display.DisplayBlock.CustomData.Contains("true") ? 0 : (float)Math.PI,
                        Position = _screenPosition,
                        Color = GetColor(target.Value),
                        Size = new Vector2(_spriteWidth, _spriteHeight),
                    };

                    // based on velocity, draw antoher sprite predicting where it'llb e in one second

                    if (target.Value.Velocity != Vector3.Zero) {
                        Vector3 predictedPosition = target.Value.Position + target.Value.Velocity * 1; // 1 second prediction
                        Vector3D observerToPredictedPosition = predictedPosition - _observerPosition;
                        Vector3 predictedWorldHit = _screenPlane.Intersection(ref _observerPosition, ref observerToPredictedPosition);
                        Vector3 predictedLocalHit = STUTransformationUtils.WorldPositionToLocalPosition(display.DisplayBlock, predictedWorldHit);
                        float predictedU = (LARGE_GRID_ONE_BLOCK_WIDTH * 0.5f - predictedLocalHit.Z) / LARGE_GRID_ONE_BLOCK_WIDTH;
                        float predictedV = (LARGE_GRID_ONE_BLOCK_HEIGHT * 0.5f - predictedLocalHit.Y) / LARGE_GRID_ONE_BLOCK_HEIGHT;
                        Vector2 predictedScreenPosition = new Vector2(predictedU * resolution.X, predictedV * resolution.Y);
                        var predictedTag = new MySprite() {
                            Type = SpriteType.TEXTURE,
                            Data = "Circle",
                            Alignment = TextAlignment.CENTER,
                            RotationOrScale = display.DisplayBlock.CustomData.Contains("true") ? 0 : (float)Math.PI,
                            Position = predictedScreenPosition,
                            Color = Color.Blue,
                            Size = new Vector2(_spriteWidth, _spriteHeight),
                        };
                        sprites.Add(predictedTag);
                    }

                    sprites.Add(tag);
                }

                display.EndAndPaintFrame();
            }

            Color GetColor(MyDetectedEntityInfo entityInfo) {
                switch (entityInfo.Relationship) {
                    case VRage.Game.MyRelationsBetweenPlayerAndBlock.Enemies:
                        return Color.Red;
                    case VRage.Game.MyRelationsBetweenPlayerAndBlock.Friends:
                        return Color.Green;
                    default:
                        return Color.Gray;
                }
            }

            public void CreateLog(string message, STULogType type) {
                Logs.Enqueue(new STULog {
                    Sender = "PR",
                    Message = message,
                    Type = type
                });
            }

        }
    }
}
