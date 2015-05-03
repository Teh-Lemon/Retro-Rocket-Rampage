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
    class Car : PhysicsObject
    {
        // Car Type
        // Stats
        public float weight = 100;
        public float stability = 100;
        public float boostStrength = 100;

        //  Car drive logic
        public bool EngineOn { get; set; }
        const float ENGINE_STRENGTH = 1500;
        public float BOOST_STRENGTH = 35000;
        public float ROCKET_STRENGTH = 250;// 750;
        public const float SIDE_SHUNT_STRENGTH = 25000;//15000;
        float forwardForce;
        float defaultElasticity = 0.75f;
        public float bouncyTerrainElasticity = 1.25f;
        // Used for the sideways shunt
        float sideForce;
        // Orientation of the car
        float angularVelocity;
        public float angle;
        public Vector3 forwardDirection;
        // Check if on bouncy terrain;
        public bool IsBouncy = false;
        // For score messaging, at the moment of death but not duration (IsAlive)
        public bool HasDied = false;
        // Count how long car has been dead
        public float IsDeadTimer = 0f;
        public float DEATH_TIME_UP = 2f;
        // Notify if respawn required
        public bool NeedsRespawn = false;

        public float turnSpeed = 1.25f;
        const float FRICTION = 2.0f;
        const float WHEEL_SPIN_SPEED = 0.001f;
        const float WHEEL_YAW_CAP = (float)(Math.PI / 15);
        const float CORRECTION_SPEED = 1.05f;
        const int BOUNCINESS = 150;

        Color[] colorList = new Color[] { Color.Black };
        public Color playerColor = Color.White;

        // Stage Boundaries
        const float CAR_X_BOUNDARY = 150f;

        public bool ConstantFullBoost = false;

        #region BOOST
        public int BOOST_LIFE = 100;
        // Boost amount to add when colliding
        const float BOOST_ADDITION = 10;
        public float currentBoost = 0;
        // No Knockback for short duration while boosting
        public bool BoostInvincibility = false;
        public float currentBInvTime = 0f;
        const float BOOST_INVINCIBILITY_TIMER = 0.7f;
        #endregion

        #region DEBUG
        Vector3 oldPosition = Vector3.Zero;
        public float debugAngle = 0f;
        float spinDebug = 0.0f;
        #endregion

        #region  Car part setup
        public bool hasFlag { get; set; }
        public bool hasBomb { get; set; }
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
        #endregion
        
        #region Particles
        // Particle Effect necessities
        public float TotalTime;
        // Effect on Collision
        public ExplosionEmitter emitSparks;
        #endregion

        #region Audio
        public AudioEmitter SoundEmitter { get; set; }
        #endregion

        // Last hit info - for score-giving
        public int PlyrID = 0;
        public int LastCollidedPlyrID = 0;
        public float LCPCurrentTime = 0f;
        const float LCP_TIMER_MAX = 2f;

        public Car(GraphicsDevice gd, GraphicsDeviceManager gdm
            , string fileName = "", ContentManager content = null)
            : base(gd, gdm, fileName, content)
        {
            //  Car Logic
            forwardForce = 0;
            sideForce = 0;
            angularVelocity = 0;
            forwardDirection = Vector3.Backward;
            MaxSpeed = -1;
            DragGroundForce = new Vector3(FRICTION, FRICTION, FRICTION);
            DragAirForce = new Vector3(FRICTION, 1f, FRICTION);
            IsAlive = true;
            AffectedByGravity = true;
            Elasticity = defaultElasticity;
            hasFlag = false;
            hasBomb = false;
            booster = new MainBooster(gd, gdm, this, "Content/Models/Car/bigBooster.txt", null);
            body = new Body(gd, gdm, this, "Content/Models/Car/biplane.txt", null);
            EngineOn = false;

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

            rotMatrix = Matrix.Identity;
            
            emitSparks = new ExplosionEmitter(1000, 350, 5, 5f, 0.15f);
            emitSparks.Modifiers.Add(new AlphaAgeModifier());

            SoundEmitter = new AudioEmitter();
        }

        public override void initialize()
        {
            Yaw = 0;
            Pitch = 0;
            Roll = 0;

            angle = 0;
            forwardForce = 0;
            sideForce = 0;
            angularVelocity = 0;
            forwardDirection = Vector3.Backward;
            MaxSpeed = -1;
            DragGroundForce = new Vector3(FRICTION, FRICTION, FRICTION);
            DragAirForce = new Vector3(FRICTION, 0.1f, FRICTION);
            Elasticity = 0.75f;
            hasFlag = false;
            hasBomb = false;
            IsDeadTimer = 0f;
            NeedsRespawn = false;

            currentBoost = 0;

            Scale = 5f;
        }

        #region Particle Initialisation

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
        }

        #endregion

        #region Random car parts
        public void randomizeParts(Color wireColor)
        {
            Color modelColor = colorList[GlobalRandom.Next(0, colorList.Length)];
            playerColor = wireColor;

            foreach (CarObject part in parts)
            {
                part.LoadModelFromFile(part.randomCarPart());
                randomCarPartScale(part);
                part.ChangeColor(modelColor, wireColor);
            }
            // Adjust Wheels for proper form
            // Currently 2 different front wheels and back wheels
            frontRightWheel.LoadModelFromFile(frontLeftWheel.currentPartString);
            frontRightWheel.Scale = frontLeftWheel.Scale;
            frontRightWheel.scalerValue = frontLeftWheel.scalerValue;
            frontRightWheel.ChangeColor(modelColor, wireColor);
            // change backLeftWheel to frontLeftWheel for consistency across all wheels 
            backRightWheel.LoadModelFromFile(backLeftWheel.currentPartString);
            backRightWheel.Scale = backLeftWheel.Scale;
            backRightWheel.scalerValue = backLeftWheel.scalerValue;
            backRightWheel.ChangeColor(modelColor, wireColor);

            sideBoosterOffset = new Vector3(body.dimensions.X / 2, 0f, 0f);

            rightBooster.Scale = leftBooster.Scale;
            rightBooster.scalerValue = leftBooster.scalerValue;

            rightBooster.Yaw = -(float)((float)GlobalRandom.Next(60) / 100.0f);
            leftBooster.Yaw = -rightBooster.Yaw; //-leftBooster.Yaw;
            
            rightBooster.Pitch = (60f - (float)GlobalRandom.Next(120)) / 100f;
            leftBooster.Pitch = rightBooster.Pitch;
            
            sideBoosterOffset = body.sideBoosterOffset;
            rightBooster.offsetPos = sideBoosterOffset;
            leftBooster.offsetPos = new Vector3(-sideBoosterOffset.X, sideBoosterOffset.Y, sideBoosterOffset.Z);
            
            mainBoosterOffset = body.mainBoosterOffset;
            booster.offsetPos = mainBoosterOffset;

            wheelOffset = body.wheelOffset;
            frontRightWheel.offsetPos = wheelOffset;
            frontLeftWheel.offsetPos = new Vector3(-wheelOffset.X, wheelOffset.Y, wheelOffset.Z);
            backRightWheel.offsetPos = new Vector3(wheelOffset.X, wheelOffset.Y, -wheelOffset.Z);
            backLeftWheel.offsetPos = new Vector3(-wheelOffset.X, wheelOffset.Y, -wheelOffset.Z);

            scaleMatrix = body.scaleMatrix;
            
            //Calculating stats
            weight = body.weight * body.scalerValue;
            weight = (int)MathHelper.Clamp(weight, 50f, 200f);
            boostStrength = booster.boostStrength;
            stability = ((frontLeftWheel.stability * frontLeftWheel.scalerValue)
                + (backLeftWheel.stability * backLeftWheel.scalerValue)) / 2;

            turnSpeed = (stability / 100) * 1.4f;


            name = booster.name + body.name + " " + frontLeftWheel.name;
        }

        // Will probably be useful for Scale powerup
        public void randomCarPartScale(CarObject carObject)
        {
            const float RANGE = 1.0f;
            //Random rand = new Random();
            float randFloat;

            randFloat = (float)(GlobalRandom.NextDouble() * RANGE);

            carObject.Scale = carObject.defaultScale + randFloat;
            carObject.Scale *= carObject.modelScale;
            carObject.scalerValue = carObject.Scale / carObject.defaultScale;

        }
        #endregion

        #region Collisions
        // Finds the biggest dimension value of all the wheel models used in this car
        float TallestWheelHeight()
        {
            float maxHeight = 0.0f;

            maxHeight = Math.Max(frontLeftWheel.dimensions.Y, maxHeight);
            maxHeight = Math.Max(frontRightWheel.dimensions.Y, maxHeight);
            maxHeight = Math.Max(backLeftWheel.dimensions.Y, maxHeight);
            maxHeight = Math.Max(backRightWheel.dimensions.Y, maxHeight);

            return maxHeight;
        }

        // Adds all the collision volumes to the car
        public override void BuildCollisionModels()
        {
            // Update the positions of all the car parts first
            for (int i = 0; i < parts.Count; i++)
            {
                parts[i].UpdatePosition();
            }

            // Distance from the car position to the ground level
            float distanceToFloor = (TallestWheelHeight()) + Math.Abs(wheelOffset.Y);
            // The widest axis that covers the entire car
            float maxBodySize = GetMaxDimensions(body.dimensions);

            // If the distance to the floor is larger than the max body size, then shrink the distance to floor
            // Prevents jumpy car
            if (maxBodySize < distanceToFloor)
            {
                distanceToFloor = maxBodySize;
            }

            if (HitSpheres.Count == 0)
            {
                // Whole body
                AddHitSphere(Position, maxBodySize * Scale);
                // Used for track collision
                AddHitSphere(Position, distanceToFloor * Scale);
            }
            // If there is already a bounding sphere, replace it
            else
            {
                HitSpheres[0] =
                    new BoundingSphere(Position, maxBodySize * Scale / 2);
                HitSpheres[1] =
                    new BoundingSphere(Position, distanceToFloor * Scale / 2);                 
            }
        }

        //  Handle collision with other cars
        public bool carCollisions(Car otherCar)
        {
            //  First pass
            //  If other car is nearby
            if (Vector3.Distance(Position, otherCar.Position) < 500)
            {
                if (HitSpheres[0].Intersects(otherCar.HitSpheres[0]))
                {
                    
                    //  If colliding, bounce cars off eachother
                    //bounceObjects(otherCar);
                    bounceCars(otherCar);

                    //  Adds boost upon collision, ensures boost value does not exceed max value
                    currentBoost += BOOST_ADDITION;
                    currentBoost = MathHelper.Clamp(currentBoost, 0, BOOST_LIFE);
                    otherCar.currentBoost += BOOST_ADDITION;
                    otherCar.currentBoost = MathHelper.Clamp(otherCar.currentBoost, 0, BOOST_LIFE);

                    Vector3 cOfMass = (Position + otherCar.Position) / 2;
                    emitSparks.Emit(TotalTime, cOfMass);
                    otherCar.emitSparks.Emit(TotalTime, cOfMass);

                    // Calculate rotation
                    Vector3 toOtherCar = Vector3.Normalize(otherCar.Position - Position);

                    // Are they in front or behind this car
                    bool InFront = false;
                    float angleToCar = Vector3.Dot(forwardDirection, toOtherCar);
                    if (angleToCar > 0)
                    {
                        InFront = true;
                    }

                    // Are they left or right to this car
                    bool IsLeft = false;
                    angleToCar = Vector3.Dot(Vector3.Cross(forwardDirection, Vector3.Up), toOtherCar);
                    if (angleToCar > 0)
                    {
                        IsLeft = true;
                    }

                    // Front right = anti
                    // Front left = clock
                    // Back right = clock
                    // Back left = anti
                    short clockwise = -1;
                    if (InFront)
                    {
                        if (IsLeft)
                        {
                            clockwise = 1;
                        }
                    }
                    else
                    {
                        if (!IsLeft)
                        {
                            clockwise = 1;
                        }
                    }

                    // Don't adjust rotation if invincible from boosting
                    if (!BoostInvincibility)
                    {
                        angularVelocity += (0.2f * clockwise);
                    }
                    if (!otherCar.BoostInvincibility)
                    {
                        otherCar.angularVelocity -= (0.2f * clockwise);
                    }

                    // Play sound effect
                    SoundManager.PlayCarCrash(SoundEmitter, 1.0f);

                    return true;
                }
            }

            return false;
        }

        // Terrain collision with track face
        public void TerrainCollision(ref Triangle triangle)
        {
            // Required angles and speed to bounce to the car
            const float ANGLE_TO_BOUNCE = -0.15f;
            const float VEL_Y_TO_BOUNCE = -25f;

            if (triangle != null)
            {
                // Find distance to triangle
                Ray intersectRay = new Ray(Position, Vector3.Negate(triangle.NormalUp));
                float? distance = intersectRay.Intersects(triangle.Plane);

                if (!distance.HasValue)
                {
                    // HACK
                    // Catch cases where no intersection with plane detected
                    triangle.Intersects(ref intersectRay, out distance);

                    // If still no detection, have to leave to avoid crashes
                    if (!distance.HasValue)
                    {
                        OnGround = false;
                        return;
                    }
                }
                
                // If triangle intersects with the terrain sphere
                if (distance.Value <= HitSpheres[1].Radius)
                {
                    OnGround = true;

                    // Find the angle between the track and the car's velocity
                    // 1 - perpendicular, 0 parrallel, -1 perpendicular down
                    float angleToTrack = Vector3.Dot(Vector3.Normalize(Velocity), Vector3.Normalize(triangle.NormalUp));

                    debugAngle = angleToTrack;

                    // The point where the sphere touches the plane
                    //Vector3 intersectionPoint = Position - (HitSpheres[0].Radius * triangle.NormalUp);

                    // If car is going into the ground, Slide/Bounce the velocity
                    if (angleToTrack <= 0)
                    {
                        // Place car on top of floor by pushing back perpendicular to plane
                        // distance needed to push back = Size of car - distance 
                        float distanceNeeded = HitSpheres[1].Radius - distance.Value;
                        Vector3 pushVector = triangle.NormalUp * distanceNeeded;
                        SetPosition(Position + pushVector);

                        // Bounce 
                        if ((angleToTrack <= ANGLE_TO_BOUNCE && Velocity.Y < VEL_Y_TO_BOUNCE) || IsBouncy)
                        {
                            if (IsBouncy)
                            {
                                Elasticity = bouncyTerrainElasticity;
                            }
                            else
                            {
                                Elasticity = defaultElasticity;
                            }
                            Vector3 newVel = new Vector3(Velocity.X * Elasticity,
                                Velocity.Y * Elasticity * 0.75f, Velocity.Z * Elasticity);
                            Velocity = Vector3.Reflect(newVel, triangle.NormalUp);

                            SoundManager.PlayCarCrash(SoundEmitter, 0.1f);
                        }
                        // Slide                         
                        else
                        {
                            // Calculate new velocity vector by...
                            // ...projecting the current velocity onto the plane
                            // The distance between the intersection point and the current destination
                            Vector3 destination = Position + Velocity;

                            float scaledOrigin = -((triangle.NormalUp.X * triangle.A.X)
                                + (triangle.NormalUp.Y * triangle.A.Y)
                                + (triangle.NormalUp.Z * triangle.A.Z));

                            float overshotDistance = Vector3.Dot(destination, triangle.NormalUp)
                                + scaledOrigin;

                            Vector3 newDestination = destination - (overshotDistance * triangle.NormalUp);

                            Velocity = newDestination - Position;
                        }

                        LastTriangle = triangle;
                    }
                }
                else
                {
                    OnGround = false;
                }
            }
            else
            {
                OnGround = false;
            }
        }
        #endregion

        #region Car Handling
        //  Turn on the rockets
        //  Rocket forces are reset at end of update()
        //  Forward = true = forward, false = reverse
        Vector3 FindNewDirection()
        {
            Matrix rotMatrix = Matrix.CreateRotationY(angle += angularVelocity);
            Vector3 direction = Vector3.Transform(Vector3.Backward, rotMatrix);
            return Vector3.Normalize(direction);
        }

        // Shunt
        public void applyMainRocket(bool LeftRocketIsOn, bool RightRocketIsOn)
        {
            if (!BoostInvincibility)
            {
                //if (kbstate.IsKeyDown(LBoost) && kbstate.IsKeyDown(RBoost))
                if (LeftRocketIsOn && RightRocketIsOn)
                {
                    forwardForce += (BOOST_STRENGTH * (float)(0.5 + (boostStrength / 150))
                        * (0.5f + (currentBoost / BOOST_LIFE)) * (1.5f - (weight / 175)));
                }
                //else if (kbstate.IsKeyDown(LBoost))
                else if (LeftRocketIsOn && !RightRocketIsOn)
                {
                    sideForce -= (SIDE_SHUNT_STRENGTH * (float)(0.5 + (boostStrength / 150))
                        * (0.5f + (currentBoost / BOOST_LIFE)) * (1.5f - (weight / 175)));
                }
                //else if (kbstate.IsKeyDown(RBoost))
                else if (RightRocketIsOn && !LeftRocketIsOn)
                {
                    sideForce += (SIDE_SHUNT_STRENGTH * (float)(0.5 + (boostStrength / 150))
                        * (0.5f + (currentBoost / BOOST_LIFE)) * (1.5f - (weight / 175)));
                }
                else
                {
                    forwardForce -= ((BOOST_STRENGTH * (float)(0.5 + (boostStrength / 150))
                        * (0.5f + (currentBoost / BOOST_LIFE)) * (1.5f - (weight / 175)))) * 3f;
                }

                BoostInvincibility = true;

                // Play sound effect
                SoundManager.PlayBoost(SoundEmitter);
            }
        }

        //  float power = percentage from 0.0 - 1.0
        public void applyRightRocket(float dt, float power, bool rocketThrust = true)
        {
            angularVelocity += (turnSpeed * power * dt);

            // rocketThrust determines whether or not to apply normal rocket dynamics
            if (rocketThrust)
            {
                forwardForce += (ROCKET_STRENGTH * power);

                rollCarModel(true);
            }
        }
        public void applyLeftRocket(float dt, float power, bool rocketThrust = true)
        {
            angularVelocity += -(turnSpeed * power * dt);

            if (rocketThrust)
            {
                forwardForce += (ROCKET_STRENGTH * power);

                rollCarModel(false);
            }
        }

        //  Move car based on forces
        void calculateAcceleration()
        {
            forwardDirection = FindNewDirection();

            Acceleration += (forwardDirection * (forwardForce + ENGINE_STRENGTH))
                + (Vector3.Cross(forwardDirection, Vector3.Up) * -sideForce);

        }

        // Rock the car as it turns around
        void rollCarModel(bool turningLeft)
        {
            // Rock the car body
            if (turningLeft && Roll > (-angularVelocity * 5f))
            {
                Roll += (-angularVelocity * 0.5f);
            }
            else if (!turningLeft && Roll < (-angularVelocity * 5f))
            {
                Roll += (-angularVelocity * 0.5f);
            }

            // Turn the wheels as the car steers
            frontLeftWheel.Yaw = angularVelocity * 20;
            frontRightWheel.Yaw = angularVelocity * 20;
        }
        
        //  Tilts car back to upright
        void correctTiltToNeutral(float dt)
        {
            Roll /= CORRECTION_SPEED;
            frontLeftWheel.Yaw /= CORRECTION_SPEED;
            frontRightWheel.Yaw /= CORRECTION_SPEED;
            Yaw = angle;
        }

        // Set who collided last and reset timer
        public void SetLastHit(Car car)
        {
            LastCollidedPlyrID = car.PlyrID;
            LCPCurrentTime = 0f;
        }

        // Hold last collided player for a set time before returning to null
        void LastCollidedCarHandle(float dt)
        {
            if (LastCollidedPlyrID != 0 && OnGround)
            {
                LCPCurrentTime += dt;

                if (LCPCurrentTime >= LCP_TIMER_MAX)
                {
                    LCPCurrentTime = 0f;
                    LastCollidedPlyrID = 0;
                }
            }
        }

        public virtual void bounceCars(Car collidingCar)
        {
            // Find out how much the cars are overlapping
            float overlap = (HitSpheres[0].Radius + collidingCar.HitSpheres[0].Radius)
                - (Vector3.Distance(Position, collidingCar.Position)) + 5;

            // Push the car back that much / 2
            Vector3 pushBackVector = Vector3.Negate(Vector3.Normalize(collidingCar.Position - Position)) * overlap;
            SetPosition(Position + (pushBackVector / 2));
            // Push the other car back / 2
            pushBackVector = Vector3.Negate(Vector3.Normalize(Position - collidingCar.Position)) * overlap;
            collidingCar.SetPosition(collidingCar.Position + (pushBackVector / 2));

            float distance = Vector3.Distance(HitSpheres[0].Center, collidingCar.Position);
            Vector3 direction = HitSpheres[0].Center - collidingCar.Position;
            direction.Normalize();

            direction *= distance * 250;

            float P1BoostPercent = currentBoost;
            float P2BoostPercent = collidingCar.currentBoost;

            if (P1BoostPercent < 20)
            {
                P1BoostPercent = 20;
            }
            if (P2BoostPercent < 20)
            {
                P2BoostPercent = 20;
            }

            if (BoostInvincibility & collidingCar.BoostInvincibility)
            {
                Acceleration += (direction * (float)(1.5f - (weight / 200))); //* P1BoostPercent);
                collidingCar.Acceleration -= (direction * (float)(1.5f - (collidingCar.weight / 200))); //* P2BoostPercent);
            }
            else if (BoostInvincibility && !collidingCar.BoostInvincibility)
            {
                collidingCar.Acceleration -= (direction * (float)(1.5f - (collidingCar.weight / 200)) * P2BoostPercent)/10;
            }
            else if (!BoostInvincibility && collidingCar.BoostInvincibility)
            {
                Acceleration += (direction * (float)(1.5f - (weight / 200)) * P1BoostPercent)/10;
            }
            else
            {
                Acceleration += (direction * (float)(1.5f - (weight / 200)));// * currentBoost);
                collidingCar.Acceleration -= (direction * (float)(1.5f - (collidingCar.weight / 200)));// * collidingCar.currentBoost);
            }
        }

        public void Die(bool PlaySound = true)
        {
            IsAlive = false;
            HasDied = true;

            // Play death sound effect
            if (PlaySound)
            {
                SoundManager.PlayCarDeath(SoundEmitter, 0.25f);
            }
        }
        #endregion

        public override void update(float dt)
        {
            oldPosition = Position;

            LastCollidedCarHandle(dt);
              
            base.update(dt);

            calculateAcceleration();

            // HACK to stop wheels spinning during menu
            if (!EngineOn)
            {
                Velocity = new Vector3(0, Velocity.Y, 0);//Velocity.Y
            }

            // Update all the car parts
            foreach (CarObject part in parts)
            {
                part.update(dt);
            }

            if (BoostInvincibility)
            {
                currentBInvTime += dt;

                if (currentBInvTime >= BOOST_INVINCIBILITY_TIMER)
                {
                    currentBInvTime = 0f;
                    BoostInvincibility = false;
                }
            }

            if (ConstantFullBoost)
            {
                currentBoost = BOOST_LIFE;
            }

            //  Reset engine forces
            forwardForce = 0;
            sideForce = 0;
            angularVelocity = 0;
            correctTiltToNeutral(dt);

            //Update Car hit box positions
            MoveHitSpheres(1, Position - oldPosition);
        }

        public override void draw(GraphicsDevice graphicsDevice, Camera cam)
        {
            if (IsAlive)
            {
                // Debug shapes
                DebugShapeRenderer.AddBoundingSphere(HitSpheres[0], Color.Yellow);
                DebugShapeRenderer.AddBoundingSphere(HitSpheres[1], Color.Purple);
                //DebugShapeRenderer.AddLine(Velocity + Position, Position, Color.Red);

                //
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

                graphicsDevice.BlendState = prevBlendState;
                graphicsDevice.RasterizerState = prevRasterizerState;

                SoundEmitter.Position = Position;
            }
        }
    }
}
