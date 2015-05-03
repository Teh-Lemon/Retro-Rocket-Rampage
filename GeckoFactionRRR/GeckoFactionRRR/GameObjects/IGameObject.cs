//  Source: Coding for Games Development tutorial 7

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
    abstract class IGameObject
    {
        public bool IsAlive { get; set; }
        //data for world transform of object
        public float Scale { get; set; }

        public float Pitch { get; set; }
        public float Yaw { get; set; }
        public float Roll { get; set; }

        public Vector3 Position { get; set; }

        public Matrix worldMat { get; protected set; }

        public Matrix rotMatrix;
        public Matrix scaleMatrix;
        public Matrix transMatrix;

        public Triangle LastTriangle { get; set; }

        //  Collision model
        // First hit sphere must always cover the whole object
        public List<BoundingSphere> HitSpheres = new List<BoundingSphere>();

        public IGameObject()
        {
            IsAlive = true;

            //initalize all world transform data
            Scale = 1.0f;

            Pitch = 0.0f;
            Yaw = 0.0f;
            Roll = 0.0f;

            Position = Vector3.Zero;

            worldMat = Matrix.Identity;
        }

        public virtual void initialize()
        {
            IsAlive = true;

            Position = Vector3.Zero;
            Pitch = 0.0f;
            Yaw = 0.0f;
            Roll = 0.0f;

            Scale = 1.0f;
        }

        public virtual void update(float dt)
        {            
            //create component matrices for world transform
            rotMatrix = Matrix.CreateFromYawPitchRoll(Yaw, Pitch, Roll);
            scaleMatrix = Matrix.CreateScale(Scale);
            transMatrix = Matrix.CreateTranslation(Position);

            // If on the ground, rotate the model with the surface
            if (LastTriangle != null)
            {
                if (LastTriangle.NormalUp != Vector3.Up)
                {
                    rotMatrix *= RotateToSurfaceNormal(ref LastTriangle.NormalUp);
                }
            }

            // multiply to create world transform
            worldMat = scaleMatrix * rotMatrix * transMatrix;

            LastTriangle = null;
        }

        public abstract void draw(GraphicsDevice graphicsDevice, Camera camera);

        // Create a rotation matrix based on current up vector and the surface normal
        protected Matrix RotateToSurfaceNormal(ref Vector3 surfaceNormal)
        {
            Vector3 axis = Vector3.Cross(rotMatrix.Up, surfaceNormal);
            //axis.Normalize();
            float angle = Vector3.Dot(rotMatrix.Up, surfaceNormal);

            if (axis == Vector3.Zero)
            {
                return Matrix.Identity;
            }
            else if (Math.Abs(angle) < 0.2f)
            {
                return Matrix.Identity;
            }
            else
            {                
                return Matrix.CreateFromAxisAngle(axis, angle);
            }
        }
    }
}
