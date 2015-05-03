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
    class CarOld : PhysicsObject
    {
        //  Car drive logic
        //const float ENGINE_STRENGTH = 2750;
        //const float ENGINE_STRENGTH = 1400;
        //const float ROCKET_STRENGTH = 750;
        public float ENGINE_STRENGTH = 25000;
        public float ROCKET_STRENGTH = 750;
        float forwardForce;
        float sideForce;
        float aimX;
        Vector3 forwardDirection = Vector3.Zero;
        //const float KB_TURN_SPEED = 0.3f;
        const float KB_TURN_SPEED = 1f;
        const float FRICTION = 3.0f;
        //const float AIMX_CAP = (float)(Math.PI / 15);
        const float CORRECTION_SPEED = 1.05f;
        const float WHEEL_SPIN_SPEED = 0.001f;
        const float WHEEL_YAW_CAP = (float)(Math.PI / 15);
        const float CAR_X_BOUNDARY = 150f;

        // BOOST
        public int BOOST_LIFE = 100;
        // Boost amount to add when colliding
        const float BOOST_ADDITION = 10;
        public float currentBoost = 0;

        #region DEBUG
        public bool isColliding = false;
        float spinDebug = 0f;
        Vector3 oldPosition = Vector3.Zero;
        #endregion

        //  Car part setup

        public bool hasFlag { get; set; }
        List<CarObject> parts = new List<CarObject>();
        protected MainBooster booster;
        public Body body;
        SideBooster leftBooster;
        SideBooster rightBooster;
        Wheel frontLeftWheel;
        Wheel frontRightWheel;
        Wheel backLeftWheel;
        Wheel backRightWheel;

        Vector3 mainBoosterOffset = new Vector3(0, 0, 0);
        Vector3 sideBoosterOffset = new Vector3(1f, 0f, 0f);
        Vector3 wheelOffset = new Vector3(1f, 0f, 1f);

        // Stats
        public float weight = 100;
        public float traction = 100;
        public float boostThrust = 100;

        /*Color[] colorList = new Color[] {Color.Red, Color.OrangeRed
                                            , Color.Orange, Color.Yellow
                                            , Color.YellowGreen, Color.Green
                                            , Color.Aqua, Color.Blue
                                            , Color.BlueViolet, Color.Violet
                                            , Color.Pink, Color.Black, Color.White};
        */
        Color[] colorList = new Color[] { Color.Black };
        public Color playerColor = Color.White;

        Random rand = new Random();


        // Particle Effect necessities
        public float TotalTime;
        // Effect on Collision
        public ExplosionEmitter emitSparks;
        // Effect from sideboosters
        public ExplosionEmitter emitLeftRocket;
        public ExplosionEmitter emitRightRocket;

        public CarOld(GraphicsDevice gd, GraphicsDeviceManager gdm
            , string fileName = "", ContentManager content = null)
            : base(gd, gdm, fileName, content)
        {
            //  Car Logic
            forwardForce = 0;
            sideForce = 0;
            aimX = 0;
            MaxSpeed = -1;
            DragForce = new Vector3(FRICTION, 1f, FRICTION);
            IsAlive = true;
            AffectedByGravity = true;
            Elasticity = 1.05f;
            hasFlag = false;
            booster = new MainBooster(gd, gdm, this, "Content/Models/Car/bigBooster.txt", null);
            body = new Body(gd, gdm, this, "Content/Models/Car/biplane.txt", null);

            leftBooster = new SideBooster(gd, gdm, this, "Content/Models/Car/sidebooster.txt", null);
            rightBooster = new SideBooster(gd, gdm, this, "Content/Models/Car/sidebooster.txt", null);

            frontLeftWheel = new Wheel(gd, gdm, this, "Content/Models/Car/thinwheel.txt", null);
            frontRightWheel = new Wheel(gd, gdm, this, "Content/Models/Car/thinwheel.txt", null);
            backLeftWheel = new Wheel(gd, gdm, this, "Content/Models/Car/thinwheel.txt", null);
            backRightWheel = new Wheel(gd, gdm, this, "Content/Models/Car/thinwheel.txt", null);

            parts.Add(booster);
            parts.Add(body);
            parts.Add(leftBooster);
            parts.Add(rightBooster);
            parts.Add(frontLeftWheel);
            parts.Add(frontRightWheel);
            parts.Add(backLeftWheel);
            parts.Add(backRightWheel);

            Scale = 5f;

            rot = Matrix.Identity;

            emitSparks = new ExplosionEmitter(1000, 350, 5, 5f, 0.15f);
            emitSparks.Modifiers.Add(new AlphaAgeModifier());
            emitLeftRocket = new ExplosionEmitter(500, 100, 500, 4f, 0.15f);
            emitLeftRocket.Modifiers.Add(new AlphaAgeModifier());
            emitRightRocket = new ExplosionEmitter(500, 100, 500, 4f, 0.15f);
            emitRightRocket.Modifiers.Add(new AlphaAgeModifier());

            
        }

        // INITIALISE PARTICLE STUFF

        public void InitialiseEmitter(int playerID, ContentManager content, GraphicsDevice gd)
        {
            switch (playerID)
            {                  
                case 1:
                    List<Texture2D> textures1 = new List<Texture2D> { /*content.Load<Texture2D>("Materials/Particles/RRRstreak"), */
                        content.Load<Texture2D>("Materials/Particles/fire"), content.Load<Texture2D>("Materials/Particles/Collisions/RRR_P1_Collision1"),
                    content.Load<Texture2D>("Materials/Particles/Collisions/RRR_P1_Collision2"), content.Load<Texture2D>("Materials/Particles/Collisions/RRR_P1_Collision3")};
                    emitSparks.LoadContent(textures1, gd);
                    break;
                case 2:
                    List<Texture2D> textures2 = new List<Texture2D> { /*content.Load<Texture2D>("Materials/Particles/RRRstreak"), */
                        content.Load<Texture2D>("Materials/Particles/fire"), content.Load<Texture2D>("Materials/Particles/Collisions/RRR_P2_Collision1"),
                    content.Load<Texture2D>("Materials/Particles/Collisions/RRR_P2_Collision2"), content.Load<Texture2D>("Materials/Particles/Collisions/RRR_P2_Collision3")};
                    emitSparks.LoadContent(textures2, gd);
                    break;
                case 3:
                    List<Texture2D> textures3 = new List<Texture2D> { /*content.Load<Texture2D>("Materials/Particles/RRRstreak"), */
                        content.Load<Texture2D>("Materials/Particles/fire"), content.Load<Texture2D>("Materials/Particles/Collisions/RRR_P3_Collision1"),
                    content.Load<Texture2D>("Materials/Particles/Collisions/RRR_P3_Collision2"), content.Load<Texture2D>("Materials/Particles/Collisions/RRR_P3_Collision3")};
                    emitSparks.LoadContent(textures3, gd);
                    break;
                case 4:
                    List<Texture2D> textures4 = new List<Texture2D> { /*content.Load<Texture2D>("Materials/Particles/RRRstreak"), */
                        content.Load<Texture2D>("Materials/Particles/fire"), content.Load<Texture2D>("Materials/Particles/Collisions/RRR_P4_Collision1"),
                    content.Load<Texture2D>("Materials/Particles/Collisions/RRR_P4_Collision2"), content.Load<Texture2D>("Materials/Particles/Collisions/RRR_P4_Collision3")};
                    emitSparks.LoadContent(textures4, gd);
                    break;
                default:
                    break;
            }
            List<Texture2D> textures = new List<Texture2D> { content.Load<Texture2D>("Materials/Particles/sidespark"), content.Load<Texture2D>("Materials/Particles/RRRStreak") };
            emitLeftRocket.LoadContent(textures, gd);
            emitRightRocket.LoadContent(textures, gd);
        }

        //  RANDOM CAR PARTS

        public void randomizeParts(Color wireColor)
        {
            Color modelColor = colorList[rand.Next(0, colorList.Length)];
            playerColor = wireColor;

            foreach (CarObject part in parts)
            {
                part.LoadModelFromFile(part.randomCarPart());
                randomCarPartScale(part);
                part.ChangeColor(modelColor, wireColor);
            }

            sideBoosterOffset = new Vector3(body.dimensions.X / 2, 0f, 0f);

            leftBooster.Yaw = -(float)((float)rand.Next(60) / 100.0f);
            rightBooster.Yaw = -leftBooster.Yaw;

            leftBooster.Pitch = (60f - (float)rand.Next(120)) / 100f;
            rightBooster.Pitch = leftBooster.Pitch;

            sideBoosterOffset = body.sideBoosterOffset * body.Scale;
            leftBooster.offsetPos = sideBoosterOffset;
            rightBooster.offsetPos = new Vector3(-sideBoosterOffset.X, sideBoosterOffset.Y, sideBoosterOffset.Z);

            mainBoosterOffset = body.mainBoosterOffset * body.Scale;
            booster.offsetPos = mainBoosterOffset;

            wheelOffset = body.wheelOffset * body.Scale;
            frontLeftWheel.offsetPos = wheelOffset;
            frontRightWheel.offsetPos = new Vector3(-wheelOffset.X, wheelOffset.Y, wheelOffset.Z);
            backLeftWheel.offsetPos = new Vector3(wheelOffset.X, wheelOffset.Y, -wheelOffset.Z);
            backRightWheel.offsetPos = new Vector3(-wheelOffset.X, wheelOffset.Y, -wheelOffset.Z);

            scale = body.scale;

            //Calculating stats
            weight = body.modelStat * body.scalerValue;
            boostThrust = booster.modelStat * booster.scalerValue;
            traction = frontLeftWheel.modelStat * frontLeftWheel.scalerValue;


        }

        // Will probably be useful for Scale powerup
        public void randomCarPartScale(CarObject carObject)
        {
            Random rand = new Random();
            float randFloat;

            randFloat = (float)(rand.NextDouble()) - 0.5f;
            //randFloat /= 1.65f;
            
            carObject.Scale = carObject.defaultScale + randFloat;
            carObject.scalerValue = carObject.Scale / carObject.defaultScale;
        }

        //  COLLISION

        public override void BuildCollisionModels()
        {
            if (HitSpheres.Count == 0)
            {
                AddHitSphere(Position, GetMaxDimensions(body.dimensions) * Scale);

                //Wheels
                frontLeftWheel.AddHitSphere(frontLeftWheel.Position, GetMaxDimensions(frontLeftWheel.dimensions) * frontLeftWheel.Scale);
                frontRightWheel.AddHitSphere(frontRightWheel.Position, GetMaxDimensions(frontRightWheel.dimensions) * frontRightWheel.Scale);
                backLeftWheel.AddHitSphere(backLeftWheel.Position, GetMaxDimensions(backLeftWheel.dimensions) * backLeftWheel.Scale);
                backRightWheel.AddHitSphere(backLeftWheel.Position, GetMaxDimensions(backRightWheel.dimensions) * backRightWheel.Scale);
            }
            // If there is already a bounding sphere, replace it
            else
            {
                HitSpheres[0] = 
                    new BoundingSphere(Position, GetMaxDimensions(body.dimensions) * Scale / 2);

                //Wheels
                frontLeftWheel.HitSpheres[0] =
                    new BoundingSphere(frontLeftWheel.Position, GetMaxDimensions(frontLeftWheel.dimensions) * frontLeftWheel.Scale);
                frontRightWheel.HitSpheres[0] =
                    new BoundingSphere(frontRightWheel.Position, GetMaxDimensions(frontRightWheel.dimensions) * frontRightWheel.Scale);
                backLeftWheel.HitSpheres[0] =
                    new BoundingSphere(backLeftWheel.Position, GetMaxDimensions(backLeftWheel.dimensions) * backLeftWheel.Scale);
                backRightWheel.HitSpheres[0] =
                    new BoundingSphere(backLeftWheel.Position, GetMaxDimensions(backRightWheel.dimensions) * backRightWheel.Scale);
            }
        }

        //  Handle collision with other cars
        public bool carCollisions(Car otherCar)
        {
            //  First pass
            //  If other car is nearby
            if (Vector3.Distance(Position, otherCar.Position) < 1000)
            {
                if (HitSpheres[0].Intersects(otherCar.HitSpheres[0]))
                {
                    //  If colliding, bounce cars off eachother
                    bounceObjects(otherCar);

                    BuildCollisionModels();

                    //  Adds boost upon collision, ensures boost value does not exceed max value
                    currentBoost += BOOST_ADDITION;
                    currentBoost = MathHelper.Clamp(currentBoost, 0, BOOST_LIFE);
                    otherCar.currentBoost += BOOST_ADDITION;
                    otherCar.currentBoost = MathHelper.Clamp(otherCar.currentBoost, 0, BOOST_LIFE);

                    Vector3 cOfMass = (Position + otherCar.Position) / 2;
                    emitSparks.Emit(TotalTime, cOfMass);
                    otherCar.emitSparks.Emit(TotalTime, cOfMass);

                    return true;
                }
            }

            return false;
        }

        //  Handle collision with terrain obstacles/power up
        void obstacleCollisions()
        {
        }

        //  Handle collision with terrain
        public void terrainCollision(TrackSection track)
        {
            //  If the car is on a track piece
            if (track != null)
            {
                //  The plane of that Triangle
                Plane plane;
                // Individual triangle for testing
                Vector3[] triangle;

                for (int i = 0; i < track.TrianglesList.Count; i++)
                {
                    triangle = track.TrianglesList[i];
                    plane = track.HitPlanes[i];
                    Vector3 planeOrigin = triangle[0];

                    //track.AddHitBox(triangle[0], triangle[1], triangle[2]);
                    if (!HitSpheres[0].Intersects(track.HitBoxes[i]))
                    {
                        continue;
                    }

                    //  If car has collided with a plane
                    if (HitSpheres[0].Intersects(plane) == PlaneIntersectionType.Intersecting)
                    {
                        // USED FOR DEBUG HUD
                        isColliding = true;
                        // USED FOR DEBUG HUD

                        // Ray from car to the plane
                        Ray intersectRay = new Ray(Position, Vector3.Negate(plane.Normal));
                        // Distance from the car to the plane
                        float? distanceToPlane = intersectRay.Intersects(plane);

                        // If Sphere is intersecting with plane
                        if (distanceToPlane.HasValue)
                        {
                            // Size of car - distance = distance needed to push back
                            // Then push back car perpendicular to plane
                            SetPosition(Position + (plane.Normal 
                                * (HitSpheres[0].Radius - distanceToPlane.Value)));                            

                            // The point where the sphere touches the plane
                            Vector3 intersectionPoint = Position - (HitSpheres[0].Radius * plane.Normal);

                            // TODO
                            // FIND IF INTERSECTION POINT LIE IN THE TRIANGLE
                            // FIND IF INTERSECTION POINT LIES ON POINTS OR EDGES OF TRIANGLE

                            // Calculate new velocity vector by...
                            // ...projecting the current velocity onto the plane
                            // The distance between the intersection point and the current destination
                            Vector3 destination = Position + Velocity;

                            float scaledOrigin = -( (plane.Normal.X * planeOrigin.X)
                                + (plane.Normal.Y * planeOrigin.Y)
                                + (plane.Normal.Z * planeOrigin.Z) );

                            float overshotDistance = Vector3.Dot(destination, plane.Normal)
                                + scaledOrigin;

                            Vector3 newDestination = destination - (overshotDistance * plane.Normal);

                            // Adjust angle of car
                            rot = Matrix.CreateLookAt(Vector3.Zero, Velocity, plane.Normal);

                            //Velocity = Velocity + (plane.Normal * HitSpheres[0].Radius * 2);
                            
                        }

                        //Velocity = Vector3.Zero;
                        //AffectedByGravity = false;
                    }
                }
            }
        }

                /*(
//  Check for collision with each hitbox
for (int i = 0; i < track.HitBoxes.Count; i++)
{                    
    box = track.HitBoxes[i];

    //  If car has collided with a hitbox
    if (HitSpheres[0].Intersects(box))
    {
        //  Plane of the triangle of the hitbox
        plane = track.HitPlanes[i];

        // Find the intersection point with the terrain triangle plane
        Ray velocityRay = new Ray(Position, Vector3.Normalize(Velocity));
        float? distanceToPlane = velocityRay.Intersects(plane);

        // If car is heading into a triangle
        if (distanceToPlane.HasValue)
        {
            // Point the car hits the plane
            // Current direction * distance to the plane
            Vector3 intersectionPoint = Position
                + (Vector3.Normalize(Velocity) * distanceToPlane.Value);

            // Move the car onto the floor minus the boundingsphere size
            // Current position + (direction * (distance to plane - sphere size))
            SetPosition(Position + (Vector3.Normalize(Velocity) 
                * (distanceToPlane.Value - HitSpheres[0].Radius)));

            // Calculate new velocity vector
            // Project the current velocity onto the plane
            // The distance between the intersection point and the current destination
            Vector3 destination = Position + Velocity;
            float overshotDistance = -Vector3.Distance(intersectionPoint, destination);
            Vector3 newDestination = destination - (overshotDistance * plane.Normal);

            //Velocity = Vector3.Reflect(Velocity * Elasticity, plane.Normal);
            //Velocity = Position - newDestination;
            Velocity = Vector3.Zero;
            AffectedByGravity = false;
        }

        return;
    }
}
 */      

        //  CAR HANDLING LOGIC

        //  Turn on the rockets
        //  Rocket forces are reset at end of update()
        //  Forward = true = forward, false = reverse
        Vector3 FindNewDirection()
        {
            Matrix rotMatrix = Matrix.CreateRotationY(Yaw += aimX);
            Vector3 direction = Vector3.Transform(Vector3.Backward, rotMatrix);
            return Vector3.Normalize(direction);
        }

        public void applyMainRocket(bool forward)
        {
            if (forward)
            {
                forwardForce += ENGINE_STRENGTH;
            }
                // Reverse
            else
            {
                forwardForce += -ENGINE_STRENGTH;
            }
        }

        //  float power = percentage from 0.0 - 1.0
        public void applyRightRocket(float dt, float power)
        {
            //if (aimX <= AIMX_CAP)
            if (true)
            {
                aimX += (KB_TURN_SPEED * power * dt);
            }

            sideForce += (ROCKET_STRENGTH * power);
            forwardForce += (ROCKET_STRENGTH * power);

            // Particles
            emitRightRocket.Emit(TotalTime, Position + new Vector3(-(HitSpheres[0].Radius / 2), 0, 0));
        }
        public void applyLeftRocket(float dt, float power)
        {
            //if (aimX >= -AIMX_CAP)
            if (true)
            {
                aimX += -(KB_TURN_SPEED * power * dt);
            }

            sideForce += -(ROCKET_STRENGTH * power);
            forwardForce += (ROCKET_STRENGTH * power);

            // Particles
            emitLeftRocket.Emit(TotalTime, Position + new Vector3((HitSpheres[0].Radius / 2), 0, 0));//leftBooster.offsetPos * leftBooster.Scale));
        }

        //  Move car based on forces
        void calculateAcceleration()
        {
            //  Move car left and right along the axises
            //float dX = sideForce;
            //float dZ = forwardForce;
            forwardDirection = FindNewDirection();

            /*
            //  Move car left and right along the axises
            float dX = sideForce;
            float dZ = forwardForce;

            //  Convert forces to acceleration
            dX = dX / Mass;
            dZ = dZ / Mass;
             */

            Acceleration += (forwardDirection * (forwardForce + 1000));
        }

        void rotateCarModel()
        {
            //Yaw = aimX;
            Roll = -aimX / 2;
        }

        //  Tilts car back to upright
        void correctTiltToNeutral(float dt)
        {
            aimX /= (CORRECTION_SPEED);
        }

        // Keeps car within 'screen' boundary to simulate NES/retro platformers
        void KeepCarInBoundary()
        {
            if (Position.X > CAR_X_BOUNDARY)
            {
                SetPosition(Position - new Vector3((2 * CAR_X_BOUNDARY) + 1f, 0f, 0f));
            }
            if (Position.X < -CAR_X_BOUNDARY)
            {
                SetPosition(Position + new Vector3((2 * CAR_X_BOUNDARY) + 1f, 0f, 0f));
            }
            /*
            if (Position.Y < -150.0f)
            {
                IsAlive = false;
            }*/
        }

        public override void update(float dt)
        {
            //DEBUG
            isColliding = false;
            //DEBUG

            oldPosition = Position;

            base.update(dt);

            frontLeftWheel.Yaw = aimX;
            frontRightWheel.Yaw = aimX;

            /*
            //  Rock car based on side direction
            Yaw = aimX;
            Roll = -aimX / 2;
            */

            calculateAcceleration();
            //Acceleration += Vector3.Zero;

            // Rotate the car model to face the velocity
            rotateCarModel();

            //  If not using side rockets, tip car back upright
            if (sideForce == 0)
            {
                //correctTiltToNeutral(dt);
            }
            // Spin wheels according to forward velocity
            if (Velocity.Z > 0)
            {
                frontLeftWheel.Pitch += WHEEL_SPIN_SPEED * Velocity.Z;
                frontRightWheel.Pitch += WHEEL_SPIN_SPEED * Velocity.Z;
                backLeftWheel.Pitch += WHEEL_SPIN_SPEED * Velocity.Z;
                backRightWheel.Pitch += WHEEL_SPIN_SPEED * Velocity.Z;
            }

            // Pac man style boundaries
            //KeepCarInBoundary();

            /*
            // Spin car round for debugging
            rot *= Matrix.CreateFromAxisAngle(Vector3.Up, spinDebug);
            worldMat = scale * rot * trans;
             * */

            // Update all the car parts
            foreach (CarObject part in parts)
            {
                part.update(dt);
            }
            //  Reset engine forces
            forwardForce = 0;
            sideForce = 0;
            aimX = 0;

            emitLeftRocket.Update(TotalTime);
            emitRightRocket.Update(TotalTime);
            emitSparks.Update(TotalTime);

            //test
            //Velocity = Vector3.Zero;

            //Update Car hit box positions
            MoveHitSpheres(1, Position - oldPosition);
        }

        public override void draw(GraphicsDevice graphicsDevice, Camera cam)
        {
            
            if (IsAlive)
            {
                DebugShapeRenderer.AddBoundingSphere(HitSpheres[0], Color.Yellow);

                foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    foreach (CarObject part in parts)
                    {
                        part.draw(graphicsDevice, cam);
                    }                   
                }

                RasterizerState prevRasterizerState = graphicsDevice.RasterizerState;
                BlendState prevBlendState = graphicsDevice.BlendState;

                graphicsDevice.BlendState = BlendState.NonPremultiplied;
                graphicsDevice.RasterizerState = RasterizerState.CullNone;

                graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

                emitSparks.Draw(graphicsDevice, cam/*.viewMat, cam.projMat*/);
                emitLeftRocket.Draw(graphicsDevice, cam);
                emitRightRocket.Draw(graphicsDevice, cam);

                graphicsDevice.BlendState = prevBlendState;
                graphicsDevice.RasterizerState = prevRasterizerState;

            }
        }
    }
}
