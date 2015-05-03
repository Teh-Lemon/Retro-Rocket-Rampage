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
    class ChaseCamera : Camera
    {
        //  How far back the camera is from cars
        const float distanceFromCarsX = -100f;
        const float distancefromCarsY = 100f;//100f;
        Vector3 averageCarPosition = Vector3.Zero;
        const float TOLERANCE_FROM_LEAD = 1000f;
        Vector3 CAMERA_OFFSET = new Vector3(distanceFromCarsX, distancefromCarsY, 0);

        // Debugging
        // Use it to switch between chase cam and free cam

        bool chaseOn = true;
        BoundingSphere averagePosSphere = new BoundingSphere(Vector3.Zero, 30);


        // Chase Cam Vectors
        public Vector3 chasePosition;
        Vector3 prevChasePosition;
        Vector3 chaseDirection;
        Vector3 desiredPosition;
        Vector3 lookAtOffset = new Vector3(0, 2.8f, 0);
        Vector3 Velocity;
        Vector3 desiredDirection;

        // Start Menu
        float currentMenuCamChangeTimer = 0f;
        const float MENU_CAMERA_CHANGE_TIMER = 10f;
        Vector3 startMenuChaseMovement = Vector3.Zero;

        // Change Cam Direction during race
        float currentCamDirectionTime = 0f;
        const float CAM_DIRECTION_TIMER = 8f;
        bool hasChangedDirection = false;
        bool canChangeDirection = false;
        float currentCamCDCooldownTime = 0f;
        const float CAM_COOLDOWN_TIMER = 20f;

        // Victory lap
        public Player Winner { get; set; }


        #region Chase properties
        // Physics coefficient which controls the influence of the camera's position
        // over the spring force. The stiffer the spring, the closer it will stay to
        // the chased object.  //default was 1800.0f
        //float stiffness = 1800.0f;
        float stiffness = 1000;

        // Physics coefficient which approximates internal friction of the spring.
        // Sufficient damping will prevent the spring from oscillating infinitely.
        // default was 600.0f
        float damping = 700.0f;

        // Mass of the camera body. Heavier objects require stiffer springs with less
        // damping to move at the same rate as lighter objects.
        // default was 50.0f
        float mass = 50.0f;
        #endregion

        #region
        // Are we shaking?
        private bool shaking;

        // The maximum magnitude of our shake offset
        private float shakeMagnitude;

        // The total duration of the current shake
        private float shakeDuration;

        // A timer that determines how far into our shake we are
        private float shakeTimer;

        // The shake offset vector
        private Vector3 shakeOffset;
        #endregion

        public ChaseCamera(float _fieldOfView, float _aspectRatio, float _nearPlaneDistance,
                                    float _farPlaneDistance, Vector3 _target, Vector3 _up)
            : base(_fieldOfView, _aspectRatio, _nearPlaneDistance, _farPlaneDistance, _target, _up)
        {
        }

        public override void initialize()
        {
            base.initialize();
            chaseDirection = Vector3.Left * 1.5f;
            desiredDirection = Vector3.Left * 1.5f;
            chasePosition = Vector3.Zero;
            Position = Vector3.Zero;
            currentMenuCamChangeTimer = MENU_CAMERA_CHANGE_TIMER;
            currentCamCDCooldownTime = 0f;
            currentMenuCamChangeTimer = 0f;
            hasChangedDirection = false;
            canChangeDirection = false;
        }

        //Chasecam reset
        public void Reset()
        {
            UpdateWorldPositions();

            // Stop motion
            Velocity = Vector3.Zero;

            // Force desired position
            Position = desiredPosition;

            UpdateMatrices();
        }

        //  Finds the average ZY position of the cars
        Vector3 getAverageCarPositions(bool checkGroundOnly = false)
        {
            // All cars
            float totalPosX = 0f;
            float totalPosY = 0f;
            float totalPosZ = 0f;

            // Excludes in air cars
            float totalNoAirPosX = 0f;
            float totalNoAirPosY = 0f;
            float totalNoAirPosZ = 0f;

            int carsUsed = 0;

            //  Go through all the cars
            for (int i = 0; i < playerList.Count; i++)
            {
                Car car = playerList[i].myCar;

                // Don't check dead cars
                if (!car.IsAlive) { continue; }
                // Don't check cars in the air if CheckOnGroundOnly is true


                //  If a car is too far back, drop them
                if (compareCarDistances(car.Position).Length()
                    > new Vector3(TOLERANCE_FROM_LEAD).Length())
                {
                    car.Die(true);
                    continue;
                }
                else
                {
                    totalPosX += car.Position.X;
                    totalPosY += car.Position.Y;
                    totalPosZ += car.Position.Z;

                    if (checkGroundOnly && !car.OnGround)
                    {
                        totalNoAirPosX += car.Position.X;
                        totalNoAirPosY += car.Position.Y;
                        totalNoAirPosZ += car.Position.Z;
                    }
                    carsUsed++;
                }
            }

            // Average position of the cars
            Vector3 averageCar = new Vector3(totalPosX, totalPosY, totalPosZ) / carsUsed;

            // The closest track point to the average car positions, rounded down
            int prevTrackIndex = track.getPreviousIndex(averageCar);
            Vector3 prevTrackPointPosition = track.TrackPoints[prevTrackIndex].Position;

            // Interpolate the along the track and get the point parrallel to the Z position
            // of the average car position
            // Call this average track position
            float plusX = GetTrackXPosFromZ(prevTrackIndex, averageCar.Z);
            Vector3 averageTrackPosition = new Vector3(prevTrackPointPosition.X + plusX
                , 0.0f, averageCar.Z);

            // The average position which includes the cars and the average track position
            Vector3 averageCarTrackPosition = ((new Vector3(totalPosX, totalPosY, totalPosZ) + averageTrackPosition) / ++carsUsed);

            // Stop the average track position dipping below the track
            if (averageCarTrackPosition.Y < 0)
            {
                averageCarTrackPosition = new Vector3(averageCarTrackPosition.X, 0, averageCarTrackPosition.Z);
            }


#if DEBUG
            averagePosSphere = new BoundingSphere(averageCarTrackPosition, 20);
#endif

            // Follow the cars/track during gameplay
            if (GameState.Current != GameState.States.OVERALL_WINNER)
            {
                return averageCarTrackPosition;
            }
            // During the overall winner screen show the end of the track
            else
            {
                return track.EndPoint.Position;
            }
        }

        // Find the X position along the track path from a given Z value
        float GetTrackXPosFromZ(int prevIndex, float z)
        {
            // Stop out of bounds error
            if (prevIndex >= track.TrackPoints.Count - 1)
            {
                prevIndex--;
            }

            // Vector from previous checkpoint to next checkpoint
            Vector3 distance = track.TrackPoints[prevIndex + 1].Position
                - track.TrackPoints[prevIndex].Position;

            z = z - track.TrackPoints[prevIndex].Position.Z;

            float ratio = distance.Z / z;
            return distance.X / ratio;
        }

        //  Returns the difference in entered Z positions with car in the lead
        Vector3 compareCarDistances(Vector3 currentPosition)
        {
            //  Holds the car currently in first place temporarily
            Car topCar = playerList[0].myCar;

            //  Loop through all the cars
            //  If a car is further ahead than the temp first place
            //  Make that car the new first place
            for (int i = 0; i < playerList.Count; i++)
            {
                if (playerList[i].myCar.Position.Z > topCar.Position.Z)
                {
                    topCar = playerList[i].myCar;
                }
            }

            //  Return the difference
            return topCar.Position - currentPosition;
        }

        /// <summary>
        /// Rebuilds object space values in world space. Invoke before publicly
        /// returning or privately accessing world space values.
        /// </summary>
        private void UpdateWorldPositions()
        {
            // Construct a matrix to transform from object space to worldspace
            Matrix transform = Matrix.Identity;
            transform.Forward = chaseDirection;
            transform.Up = up;
            transform.Right = Vector3.Cross(up, chaseDirection);

            // Calculate desired camera properties in world space
            desiredPosition = chasePosition +
                Vector3.TransformNormal(/*DesiredPositionOffset*/CAMERA_OFFSET, transform);
            target = chasePosition +
                Vector3.TransformNormal(lookAtOffset, transform);
        }

        /// <summary>
        /// Rebuilds camera's view and projection matricies.
        /// </summary>
        private void UpdateMatrices()
        {
            // offset values if camera is shaking
            if (shaking)
            {
                Position += shakeOffset;
                target += shakeOffset;
            }

            viewMat = Matrix.CreateLookAt(this.Position, this.target, this.up);
            projMat = Matrix.CreatePerspectiveFieldOfView(fieldOfView,
                aspectRatio, nearPlaneDistance, farPlaneDistance);
        }

        //  Change desired direction
        public void ChangeDirection(bool includeFrontandBack)
        {
            int rand;
            if (includeFrontandBack)
            {
                rand = GlobalRandom.Next(5);
            }
            else
            {
                rand = GlobalRandom.Next(3);
            }

            switch (rand)
            {
                case 1:
                    desiredDirection = Vector3.Forward * 2;
                    break;
                case 2:
                    desiredDirection = Vector3.Backward * 2;
                    break;
                case 3:                    
                    desiredDirection = Vector3.Left * 2;
                    break;
                case 4:                    
                    desiredDirection = Vector3.Right * 2;
                    break;
                default:
                    break;
            }
        }

        #region Shake Camera

        // Shake the camera according to magnitufe for a gven duration
        public void Shake(float magnitude, float duration)
        {
            // We're now shaking
            shaking = true;

            // Store our magnitude and duration
            shakeMagnitude = magnitude;
            shakeDuration = duration;

            // Reset our timer
            shakeTimer = 0f;
        }

        // Randomise floats between -1 and 1, for offsets
        private float NextFloat()
        {
            return (float)GlobalRandom.NextDouble() * 2f - 1f;
        }
        #endregion

        public override void update(float dt)
        {
            #region Shake Handling
            // If we're shaking...
            if (shaking)
            {
                // Move our timer ahead based on the elapsed time
                shakeTimer += dt;

                // If we're at the max duration, we're not going to be shaking anymore
                if (shakeTimer >= shakeDuration)
                {
                    shaking = false;
                    shakeTimer = shakeDuration;
                }

                // Compute our progress in a [0, 1] range
                float progress = shakeTimer / shakeDuration;

                // Compute our magnitude based on our maximum value and our progress. This causes
                // the shake to reduce in magnitude as time moves on, giving us a smooth transition
                // back to being stationary. We use progress * progress to have a non-linear fall 
                // off of our magnitude. We could switch that with just progress if we want a linear 
                // fall off.
                float magnitude = shakeMagnitude * (1f - (progress * progress));

                // Generate a new offset vector with three random values and our magnitude
                shakeOffset = new Vector3(NextFloat(), NextFloat(), NextFloat()) * magnitude;
            }
            #endregion

            if (chaseOn)
            {
                UpdateWorldPositions();

                switch (GameState.Current)
                {
                    case GameState.States.PRE_START_SCREEN:
                        Position = track.TrackPoints[GlobalRandom.Next(5, track.TrackPoints.Count - 5)].Position;
                            chasePosition = Position;
                            ChangeDirection(true);
                            desiredDirection = new Vector3(desiredDirection.X * 3, 50 + (desiredDirection.Y * 3), desiredDirection.Z * 3);
                            currentMenuCamChangeTimer = 0;
                            startMenuChaseMovement = new Vector3(GlobalRandom.Next(-1, 1), GlobalRandom.Next(0, 1), GlobalRandom.Next(-1, 1));
                        break;

                    case GameState.States.START_SCREEN:

                        currentMenuCamChangeTimer += dt;
                        if (currentMenuCamChangeTimer >= MENU_CAMERA_CHANGE_TIMER)
                        {
                            Position = track.TrackPoints[GlobalRandom.Next(5, track.TrackPoints.Count - 5)].Position;
                            chasePosition = Position;
                            ChangeDirection(true);
                            desiredDirection = new Vector3(desiredDirection.X * 3, 50 + (desiredDirection.Y * 3), desiredDirection.Z * 3);
                            currentMenuCamChangeTimer = 0;
                            startMenuChaseMovement = new Vector3(GlobalRandom.Next(-1, 1), GlobalRandom.Next(0, 1), GlobalRandom.Next(-1, 1));
                            //startMenuChaseMovement /= 5;
                        }

                        chasePosition += startMenuChaseMovement;
                        break;

                    case GameState.States.MENU:
                        switch (CurrentState)
                        {
                            case States.CAR_PREVIEW:
                                chaseDirection = Vector3.Right * 1.5f;
                                desiredDirection = Vector3.Right * 1.5f;
                                chasePosition = getAverageCarPositions(false);
                                break;

                            case States.TRACK_PREVIEW:
                                chaseDirection = Vector3.Left * 1.5f;
                                desiredDirection = Vector3.Left * 1.5f;
                                chasePosition = getAverageCarPositions(false);
                                break;

                            default:
                                chaseDirection = Vector3.Left * 1.5f;
                                desiredDirection = Vector3.Left * 1.5f;
                                chasePosition = getAverageCarPositions(false);

                                break;
                        }

                        break;

                    case GameState.States.MENU_TIMEOUT:
                        chaseDirection = Vector3.Left * 1.5f;
                        desiredDirection = Vector3.Left * 1.5f;
                        chasePosition = getAverageCarPositions(false);
                        break;

                    case GameState.States.PLAYING:
                        prevChasePosition = chasePosition;
                        chasePosition = getAverageCarPositions(false);
                        int deadCount = 0;

                        foreach (Player player in playerList)
                        {
                            if (!player.myCar.IsAlive)
                            {
                                deadCount++;
                            }
                        }

                        if (deadCount >= playerList.Count)
                        {
                            chasePosition = prevChasePosition;
                        }

                        
                        // Change Camera Direction after cooldown
                        if (hasChangedDirection)
                        {
                            currentCamDirectionTime += dt;
                        }
                        else
                        {
                            currentCamCDCooldownTime += dt;
                        }
                        if (currentCamCDCooldownTime >= CAM_COOLDOWN_TIMER)
                        {
                            canChangeDirection = true;
                        }
                        else
                        {
                            canChangeDirection = false;
                        }
                        if (canChangeDirection)
                        {
                            int rand = GlobalRandom.Next(0, 2);

                            switch (rand)
                            {
                                case 0:
                                    desiredDirection = Vector3.Left * 1.5f;
                                    break;
                                case 1:
                                    ChangeDirection(false);
                                    hasChangedDirection = true;
                                    canChangeDirection = false;
                                    currentCamDirectionTime = 0;
                                    currentCamCDCooldownTime = 0;
                                    break;
                                default:
                                    desiredDirection = Vector3.Left * 1.5f;
                                    break;
                            }
                        }

                        if (currentCamDirectionTime >= CAM_DIRECTION_TIMER)
                        {
                            hasChangedDirection = false;
                            desiredDirection = Vector3.Left * 1.5f;
                            currentCamCDCooldownTime = 0f;
                            currentCamDirectionTime = 0f;
                        }


                        break;

                    case GameState.States.VICTORY_LAP:
                        chaseDirection = Vector3.Right * 5f;
                        desiredDirection = Vector3.Right * 5f;
                        chasePosition = Winner.myCar.Position;
                        break;

                    case GameState.States.OVERALL_WINNER:
                        chaseDirection = Vector3.Right * 5f;
                        desiredDirection = Vector3.Right * 5f;
                        chasePosition = getAverageCarPositions(false);
                        break;

                    default:
                        chasePosition = getAverageCarPositions(false);
                        break;
                }


                // Has to be left to actually follow behind - may make default left and instead
                // allow direction to be changed for more dynamic stuff
                //chaseDirection = Vector3.Left;

                //  Move camera towards desired direction
                if (chaseDirection != desiredDirection)
                {
                    chaseDirection = Vector3.SmoothStep(chaseDirection, desiredDirection, 0.1f);
                }

                // Calculate spring force
                Vector3 stretch = Position - desiredPosition;
                Vector3 force = -stiffness * stretch - damping * Velocity;

                // Apply acceleration
                Vector3 acceleration = force / mass;
                Velocity += acceleration * dt;

                Position += Velocity * dt;

                UpdateMatrices();
            }
            else
            {
                base.update(dt);

                //  Move the camera forward to keep up with cars
                Position += getAverageCarPositions(true) + CAMERA_OFFSET;
            }
        }

        public override void draw(GraphicsDevice graphicsDevice, Camera camera)
        {
            DebugShapeRenderer.AddBoundingSphere(averagePosSphere, Color.Red);
        }
    }
}
