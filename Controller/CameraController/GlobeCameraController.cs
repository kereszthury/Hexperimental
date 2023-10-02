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
        public Camera Camera { get; }
        private float baseMovementSpeed, baseRotationSpeed;
        private Vector3 cameraBase, relativeRight;

        private readonly float rotationSpeed = 0.5f;
        private float movementSpeed = 0.5f;

        private float zoom = 10f, zoomSpeed = 15f;
        private readonly float minZoom = 5f, maxZoom = 50f;
        private readonly float minViewAngle = 20f, maxViewAngle = 89f;

        public GlobeCameraController(Camera camera, Vector3 startAngles, float baseMovementSpeed = 1f, float baseRotationSpeed = 1f)
        {
            Camera = camera;
            this.baseMovementSpeed = baseMovementSpeed;
            this.baseRotationSpeed = baseRotationSpeed;
            
            cameraBase = new Vector3(0, 0, -1);
            relativeRight = new Vector3(1, 0, 0);

            Matrix initialRotation = Matrix.CreateFromYawPitchRoll(startAngles.X, startAngles.Y, startAngles.Z);

            cameraBase = Vector3.Transform(cameraBase, initialRotation);
            relativeRight = Vector3.Transform(relativeRight, initialRotation);
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

            RaycastHit hit = Raycaster.GetHit(Vector3.Zero, cameraBase);

            Vector3 cameraOffset = zoom * Vector3.Transform(relativeBack, Matrix.CreateFromAxisAngle(relativeRight, MathHelper.ToRadians(MathHelper.Lerp(minViewAngle, maxViewAngle, zoom / maxZoom))));

            Camera.Up = cameraBase;
            Camera.Position = hit.Position + cameraOffset;
            Camera.Direction = -cameraOffset;
        }
    }
}
