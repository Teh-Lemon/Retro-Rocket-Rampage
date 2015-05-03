// Created by Anthony Lee on 08/05/2014
// Spawned along on each Track Point
// Marks the progress the cars have made along the track
// Used by the cahes camera

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GeckoFactionRRR
{
    class CheckPoint : IGameObject
    {
        BoundingSphere HitSphere;
        public int ID { get; set; }

        public CheckPoint(Vector3 newPosition, float newRadius, int id)
        {
            IsAlive = true;
            Position = newPosition;
            ID = id;

            HitSphere = new BoundingSphere(newPosition, newRadius);
        }

        public void UpdateSpherePosition()
        {
            //  Translate the sphere
            HitSphere = HitSphere.Transform(Matrix.CreateTranslation(Position));
        }

        // Check if car has touched checkpoint
        // If so, IsAlive is off (effectively removing the checkpoint)
        public bool CheckCollision(Car car)
        {
            if (car.HitSpheres[0].Intersects(HitSphere))
            {
                IsAlive = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void draw(GraphicsDevice graphicsDevice, Camera camera)
        {
            if (IsAlive)
            {
                DebugShapeRenderer.AddBoundingSphere(HitSphere, Color.Yellow);
            }
        }
    }
}
