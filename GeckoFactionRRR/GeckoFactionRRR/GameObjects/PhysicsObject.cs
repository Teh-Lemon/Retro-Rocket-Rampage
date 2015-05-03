//  Anthony Lee 11010841
//  Gives game objects the ability to be affected by forces
//  Applies gravity and drag
//  Applies speed limits

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
    class PhysicsObject : VBGameObjectBasic
    {
        public int GRAVITY = 350;

        public Vector3 Velocity { get; set; }
        public Vector3 Acceleration { get; set; }
        public bool AffectedByGravity { get; set; }
        public float MaxSpeed { get; set; }
        public Vector3 DragGroundForce { get; set; }

        //  New
        public float Mass { get; set; }
        public float InverseMass { get; set; }
        public float Elasticity { get; set; }
        public bool OnGround { get; set; }
        public Vector3 DragAirForce { get; set; }
        public Vector3 Momentum { get; set; }

        public PhysicsObject(GraphicsDevice gd, GraphicsDeviceManager gdm
            , string fileName = "", ContentManager content = null)
            : base(gd, gdm, fileName, content)
        {
            initialize();
        }

        public override void initialize()
        {
            base.initialize();

            Velocity = Vector3.Zero;
            Acceleration = Vector3.Zero;
            AffectedByGravity = false;
            MaxSpeed = -1;
            DragGroundForce = Vector3.Zero;
            DragAirForce = Vector3.Zero;
            Mass = 1;
            Elasticity = 1f;
            OnGround = false;
        }

        //  Accelerate object down
        void applyGravity()
        {
            Acceleration += new Vector3(0, -GRAVITY, 0);
        }

        //  Stop the object moving faster than it's max speeds
        //  if max speed set to -1, max speed = infinite
        Vector3 setSpeedLimits(Vector3 newVelocity)
        {
            if (MaxSpeed >= 0)
            {
                if (newVelocity.Length() > MaxSpeed)
                {
                    newVelocity.Normalize();
                    newVelocity *= MaxSpeed;
                }
            }
            
            return newVelocity;
            
        }

        //  Slow the object down (friction/air resistance etc.)
        void applyDrag()
        {
            Vector3 dAccel = Vector3.Zero;

            if (OnGround)
            {
                //  dAcceleration = -kx
                dAccel = -DragGroundForce * Velocity;
            }
            else
            {
                dAccel = -DragAirForce * Velocity;
            }

            Acceleration += dAccel;
        }

        //  Makes the current object bounce off the input object when called
        public virtual void bounceObjects(PhysicsObject collidingObject)
        {
            // Find out how much the cars are overlapping
            float overlap = (HitSpheres[0].Radius + collidingObject.HitSpheres[0].Radius)
                - (Vector3.Distance(Position, collidingObject.Position)) + 5;

            // Push the car back that much / 2
            Vector3 pushBackVector = Vector3.Negate(Vector3.Normalize(collidingObject.Position - Position)) * overlap;
            SetPosition(Position + (pushBackVector / 2));
            // Push the other car back / 2
            pushBackVector = Vector3.Negate(Vector3.Normalize(Position - collidingObject.Position)) * overlap;
            collidingObject.SetPosition(collidingObject.Position + (pushBackVector / 2));

            // Bounce them off each other
            Vector3 cOfMass = (Velocity + collidingObject.Velocity) / 2;
            Vector3 normal1 = collidingObject.Position - Position;
            Vector3 normal2 = Position - collidingObject.Position;
            normal1.Normalize();
            normal2.Normalize();

            Velocity -= cOfMass;
            Velocity = Vector3.Reflect(Velocity, normal1);
            Velocity += cOfMass;

            collidingObject.Velocity -= cOfMass;
            collidingObject.Velocity = Vector3.Reflect(collidingObject.Velocity, normal2);
            collidingObject.Velocity += cOfMass;            
        }

        public override void update(float dt)
        {
            //Apply gravity
            if (AffectedByGravity)
            {
                applyGravity();
            }

            //  apply drag
            applyDrag();

            Vector3 newVelocity = Velocity + dt * Acceleration;
            Vector3 newPosition = Position + dt * Velocity;

            //  Set speed limits
            newVelocity = setSpeedLimits(newVelocity);

            Position = newPosition;
            Velocity = newVelocity;

            base.update(dt);

            Acceleration = Vector3.Zero;
        }
    }
}
