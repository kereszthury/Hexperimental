using Hexperimental.View;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hexperimental.Controller.CameraController;

class CameraController
{
    public Camera Camera { get; }
    private float movementSpeed, rotationSpeed;

    public CameraController(Camera camera, float movementSpeed = 1f, float rotationSpeed = 1f)
    {
        Camera = camera;
        this.movementSpeed = movementSpeed;
        this.rotationSpeed = rotationSpeed;
    }

    public void Update(float frameTime)
    {
        var keyboard = Keyboard.GetState();

        if (keyboard.IsKeyDown(InputHandler.GetKey("Camera Forward")))
        {
            Camera.Position += movementSpeed * frameTime * Camera.Direction;
        }
        if (keyboard.IsKeyDown(InputHandler.GetKey("Camera Back")))
        {
            Camera.Position -= movementSpeed * frameTime * Camera.Direction;
        }

        Vector3 cameraLeft = Vector3.Cross(Vector3.Up, Camera.Direction);
        if (keyboard.IsKeyDown(InputHandler.GetKey("Camera Left")))
        {
            Camera.Position += movementSpeed * frameTime * cameraLeft;
        }
        if (keyboard.IsKeyDown(InputHandler.GetKey("Camera Right")))
        {
            Camera.Position -= movementSpeed * frameTime * cameraLeft;
        }

        if (keyboard.IsKeyDown(InputHandler.GetKey("Camera Rotate Left")))
        {
            var rotater = Matrix.CreateRotationY(rotationSpeed * frameTime);
            Camera.Direction = Vector3.Transform(Camera.Direction, rotater);
        }
        if (keyboard.IsKeyDown(InputHandler.GetKey("Camera Rotate Right")))
        {
            var rotater = Matrix.CreateRotationY(-rotationSpeed * frameTime);
            Camera.Direction = Vector3.Transform(Camera.Direction, rotater);
        }

        if (keyboard.IsKeyDown(InputHandler.GetKey("Camera Up")))
        {
            Camera.Position += movementSpeed * frameTime * 5 * Vector3.Up;
        }
        if (keyboard.IsKeyDown(InputHandler.GetKey("Camera Down")))
        {
            Camera.Position -= movementSpeed * frameTime * 5 * Vector3.Up;
        }
    }
}
