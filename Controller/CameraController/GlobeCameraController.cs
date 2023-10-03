using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Hexperimental.Model;

namespace Hexperimental.Controller.CameraController
{
    public class GlobeCameraController
    {
        private readonly Camera camera;
        private readonly Globe globe;
        private float baseMovementSpeed, baseRotationSpeed;
        private Vector3 cameraBase, relativeRight;

        private readonly float rotationSpeed = 1.5f;
        private float movementSpeed = 0.5f;

        private float zoom = 10f, zoomSpeed = 15f;
        private readonly float minZoom = 5f, maxZoom;
        private readonly float minViewAngle = 45f, maxViewAngle = 89f;

        public GlobeCameraController(Camera camera, Globe globe, Vector3 startAngles, float baseMovementSpeed = 1f, float baseRotationSpeed = 1f)
        {
            this.camera = camera;
            this.globe = globe;
            this.baseMovementSpeed = baseMovementSpeed;
            this.baseRotationSpeed = baseRotationSpeed;
            
            cameraBase = new Vector3(0, 0, -1);
            relativeRight = new Vector3(1, 0, 0);

            Matrix initialRotation = Matrix.CreateFromYawPitchRoll(startAngles.X, startAngles.Y, startAngles.Z);

            // TODO set minZoom to maximum height of map? - OR - Bring back raycast to cameraBasei zoom < maxHeight

            cameraBase = Vector3.Transform(cameraBase, initialRotation);
            relativeRight = Vector3.Transform(relativeRight, initialRotation);

            maxZoom = globe.radius * 2;
        }

        public void Update(float frameTime)
        {
            var keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(InputHandler.GetKey("Camera Rotate Left")))
            {
                relativeRight = Vector3.Transform(relativeRight, Matrix.CreateFromAxisAngle(cameraBase, rotationSpeed * frameTime));
            }
            if (keyboard.IsKeyDown(InputHandler.GetKey("Camera Rotate Right")))
            {
                relativeRight = Vector3.Transform(relativeRight, Matrix.CreateFromAxisAngle(cameraBase, -rotationSpeed * frameTime));
            }

            if (keyboard.IsKeyDown(InputHandler.GetKey("Camera Forward")))
            {
                cameraBase = Vector3.Transform(cameraBase, Matrix.CreateFromAxisAngle(relativeRight, movementSpeed * frameTime));
            }
            if (keyboard.IsKeyDown(InputHandler.GetKey("Camera Back")))
            {
                cameraBase = Vector3.Transform(cameraBase, Matrix.CreateFromAxisAngle(relativeRight, -movementSpeed * frameTime));
            }

            Vector3 relativeBack = Vector3.Cross(cameraBase, relativeRight);

            if (keyboard.IsKeyDown(InputHandler.GetKey("Camera Left")))
            {
                cameraBase = Vector3.Transform(cameraBase, Matrix.CreateFromAxisAngle(relativeBack, movementSpeed * frameTime));
                relativeRight = Vector3.Transform(relativeRight, Matrix.CreateFromAxisAngle(relativeBack, movementSpeed * frameTime));
            }
            if (keyboard.IsKeyDown(InputHandler.GetKey("Camera Right")))
            {
                cameraBase = Vector3.Transform(cameraBase, Matrix.CreateFromAxisAngle(relativeBack, -movementSpeed * frameTime));
                relativeRight = Vector3.Transform(relativeRight, Matrix.CreateFromAxisAngle(relativeBack, -movementSpeed * frameTime));
            }

            if (keyboard.IsKeyDown(InputHandler.GetKey("Camera Up")))
            {
                zoom -= zoomSpeed * frameTime;
                zoom = Math.Max(minZoom, zoom);
            }
            if (keyboard.IsKeyDown(InputHandler.GetKey("Camera Down")))
            {
                zoom += zoomSpeed * frameTime;
                zoom = Math.Min(zoom, maxZoom);
            }

            Vector3 cameraOffset = zoom * Vector3.Transform(relativeBack, Matrix.CreateFromAxisAngle(relativeRight, MathHelper.ToRadians(MathHelper.Lerp(minViewAngle, maxViewAngle, zoom / maxZoom))));

            camera.Up = cameraBase;
            camera.Position = cameraBase * globe.radius + cameraOffset;
            camera.Direction = -cameraOffset;
        }
    }
}
