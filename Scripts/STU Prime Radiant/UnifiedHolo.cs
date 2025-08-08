using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript {
    partial class Program {
        class UnifiedHolo {

            bool SCREEN_PLANE_INITIALIZED = false;

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

            List<STUDisplay> _displays;
            IMySensorBlock _observerSensor;

            MyDetectedEntityInfo _observer;

            Action<string> echo;

            public float ScreenOffset = 0.5f; // distance from the center of the display block to the screen portion of the block

            public UnifiedHolo(List<STUDisplay> displays, IMySensorBlock observerSensor, Action<string> echo) {
                _displays = displays;
                _observerSensor = observerSensor;
                this.echo = echo;
            }

            public void Update(Dictionary<long, MyDetectedEntityInfo> targets) {
                if (_observerSensor.LastDetectedEntity.IsEmpty()) {
                    echo("Observer sensor has no detected entity.");
                    return;
                }
                foreach (var display in _displays) {
                    DrawTags(display, targets);
                }
            }

            void DrawTags(STUDisplay display, Dictionary<long, MyDetectedEntityInfo> targets) {

                _observer = _observerSensor.LastDetectedEntity;

                display.StartFrame();

                Vector2 resolution = display.DisplayBlock.SurfaceSize; // (w,h)
                MySpriteDrawFrame sprites = display.CurrentFrame;

                // SE takes the engineer's location to be the center of body's mass, so we adjust up toward the "eyes"
                _observerPosition = _observer.Position + _observer.Orientation.Up * 0.5;

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

                    // add your sprite
                    var tag = new MySprite() {
                        Type = SpriteType.TEXTURE,
                        Data = "Triangle",
                        Alignment = TextAlignment.CENTER,
                        RotationOrScale = display.DisplayBlock.CustomData.Contains("true") ? 0 : (float)Math.PI,
                        Position = _screenPosition,
                        Color = GetColor(target.Value),
                        Size = new Vector2(_spriteWidth, _spriteHeight),
                    };

                    var velocity = new MySprite() {
                        Type = SpriteType.TEXT,
                        Data = target.Value.Velocity.Length().ToString(),
                        Alignment = TextAlignment.CENTER,
                        Position = new Vector2(_screenPosition.X, display.DisplayBlock.CustomData.Contains("true") ? _screenPosition.Y + 20 : _screenPosition.Y - 40),
                        RotationOrScale = 0.7f,
                        FontId = "Monospace",
                    };

                    sprites.Add(tag);
                    sprites.Add(velocity);
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

        }
    }
}
