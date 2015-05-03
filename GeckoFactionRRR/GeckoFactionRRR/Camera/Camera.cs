//  Camera
//  Created on 16/10/2013

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace GeckoFactionRRR
{
    class Camera : GameObject
    {
        public enum States
        {
            CAR_PREVIEW,
            TRACK_PREVIEW,
            PLAYING
        }
        public States CurrentState = States.PLAYING;

        //  Camera
        public GameWindow window;
        public Matrix projMat { get; /*private*/protected set; }
        public Matrix viewMat { get; protected set; }

        public float fieldOfView { get; set; }
        public float aspectRatio { get; set; }
        public float nearPlaneDistance { get; set; }
        public float farPlaneDistance { get; set; }

        public Vector3 target { get; set; }
        public Vector3 up { get; set; }
        
        //  Fps look around variables
        float leftrightRot = (float)Math.PI;
        float updownRot = -(float)Math.PI / 10f;
        const float rotationSpeed = 0.3f;
        const float moveSpeed = 30.0f;
        MouseState originalMouseState;
        //  Used to move camera around while following cars
        Vector3 fpsPositionOffset = Vector3.Zero;

        Vector3 SPAWN_POSITION = new Vector3(250, 50, -200);
        //  Position of camera, relative to player
        Vector3 offsetDistance = new Vector3(0, 75, -150);

        //  Collision
        public BoundingFrustum viewFrustum { get; set; }

        //  External objects
        public List<Player> playerList { get; set; }
        public TrackManager track { get; set; }
        
        public Camera(float _fieldOfView, float _aspectRatio, float _nearPlaneDistance,
            float _farPlaneDistance, Vector3 _target, Vector3 _up)
        {
            fieldOfView = _fieldOfView;
            aspectRatio = _aspectRatio;
            nearPlaneDistance = _nearPlaneDistance;
            farPlaneDistance = _farPlaneDistance;        

            target = _target;
            up = _up;

            initialize();
            
        }

        public override void initialize()
        {
            target = new Vector3(25f, 2.5f, 0);
            leftrightRot = (float)Math.PI;
            updownRot = -(float)Math.PI / 10f;
            //  Used to move camera around while following cars
            fpsPositionOffset = SPAWN_POSITION;
        }

        //  Creates the bounding frustrum for the current camera view
        void BuildCollisionFrustrum(Matrix view, Matrix project)
        {
            viewFrustum = new BoundingFrustum(view * project);
        }

        //  Returns whether the entered object intersects with the view frumstum
        public bool OnScreen(IGameObject item)
        {
            BoundingSphere bSphere;
            BuildCollisionFrustrum(viewMat, projMat);

            for (int i = 0; i < item.HitSpheres.Count; i++)
            {
                bSphere = item.HitSpheres[i];

                if(viewFrustum.Intersects(bSphere))
                {
                    return true;
                }
            }
            return false;   
        }

        public override void update(float dt)
        {
            if (window == null)
            {
                return;
            }

            //  Handle fps look around controls
            ProcessInput(dt);

            //  All position movements are relative to origin
            Position = Vector3.Zero;

            //  Move camera around with fps look around style
            Position += fpsPositionOffset;
            
            projMat = Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance);

            base.update(dt);            
        }

        //Following functions are all for FPSCamera
        private void UpdateViewMatrix()
        {
            Matrix cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);

            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            Vector3 cameraFinalTarget = Position + cameraRotatedTarget;

            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);
            Vector3 cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, cameraRotation);

            viewMat = Matrix.CreateLookAt(Position, cameraFinalTarget, cameraRotatedUpVector);
        }

        private void ProcessInput(float amount)
        {
            const float MOVE_SPEED = 4;

            MouseState currentMouseState = Mouse.GetState();
            
            if (currentMouseState != originalMouseState)
            {
                float xDifference = currentMouseState.X - originalMouseState.X;
                float yDifference = currentMouseState.Y - originalMouseState.Y;
                leftrightRot -= rotationSpeed * xDifference * amount;
                updownRot -= rotationSpeed * yDifference * amount;
                UpdateViewMatrix();
            }

            Vector3 moveVector = new Vector3(0, 0, 0);
            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.NumPad8))
                moveVector += new Vector3(0, 0, -MOVE_SPEED);
            if (keyState.IsKeyDown(Keys.NumPad5))
                moveVector += new Vector3(0, 0, MOVE_SPEED);
            if (keyState.IsKeyDown(Keys.NumPad6))
                moveVector += new Vector3(MOVE_SPEED, 0, 0);
            if (keyState.IsKeyDown(Keys.NumPad4))
                moveVector += new Vector3(-MOVE_SPEED, 0, 0);
            if (keyState.IsKeyDown(Keys.NumPad7))
                moveVector += new Vector3(0, MOVE_SPEED, 0);
            if (keyState.IsKeyDown(Keys.NumPad9))
                moveVector += new Vector3(0, -MOVE_SPEED, 0);
            AddToCameraPosition(moveVector * amount);

            originalMouseState = Mouse.GetState();
        }

        //  Move fps look relative to it's proper position during the race
        private void AddToCameraPosition(Vector3 vectorToAdd)
        {
            Matrix cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);
            fpsPositionOffset += (moveSpeed * rotatedVector);
            UpdateViewMatrix();
        }

    }
}
