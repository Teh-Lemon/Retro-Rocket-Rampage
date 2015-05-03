// Created by Anthony Lee on 19/02/2014
// Holds information on the physical track itself
// Contains method to generate and interact with it

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GeckoFactionRRR
{
    class TrackCurve : VBGameObjectBasic
    {
        #region Constants
        // Map types
        public enum Map
        {
            Normal,
            Big_Wall,
            Final_Destination
        }
        Map currentMap = Map.Normal;

        const float SECTION_WIDTH = 360; //350; //225;
        
        // Inner ramps
        const float IN_RAMP_WIDTH = 22.5f; //70f; //22.5f;
        const float IN_RAMP_HEIGHT = 3.75f; //25f; //3.75f;

        // Outer ramps
        const float OUT_RAMP_WIDTH = 70f; //22.5f;
        const float OUT_RAMP_HEIGHT = 70f; //13.5f;

        // Walls
        const float WALL_WIDTH = 1f;

        // Colour
        public Color FillColor = new Color(0, 15, 20);
        public Color WireColor = new Color(0, 255, 200);
        const int TRIG_PER_BOX = 16;

        // Procedural generation
        const float SECTION_LENGTH_MIN = 800; //375;
        const float SECTION_LENGTH_MAX = 400;
        // How tight the track can turn per section in radians
        const float TRACK_TURN_CAP = 0.5f;
        #endregion

        #region Track curve
        // List of points the track follows
        List<TrackPoint> PointsMain;
        // Offset points for the side curves
        List<Vector3> PointsRight;
        List<Vector3> PointsLeft;
        // Offset points for the left ramps
        List<Vector3> PointsLeftRampIn;
        List<Vector3> PointsLeftRampOut;
        List<Vector3> PointsLeftWall;
        // Offset points for the right ramps
        List<Vector3> PointsRightRampIn;
        List<Vector3> PointsRightRampOut;
        List<Vector3> PointsRightWall;

        // Model data
        List<Vector3> newVertices;
        List<short> newIndices;
        public bool HasModel { get; set; }

        // Collision volumes
        TriangleMesh triangleMesh;
        BoundingBox[] collisionBoxes;
        public BoundingBox KillBox { get; set; }

        public struct MainPoint
        {
            public Vector3 Position;
            // Direction to next position along the track curve
            public Vector3 Direction;

            public MainPoint(Vector3 newPos, Vector3 newDir)
            {
                Position = newPos;
                Direction = newDir; 
                Direction.Normalize();
            }
        }
        #endregion

        // Constructor
        public TrackCurve(GraphicsDevice gd, GraphicsDeviceManager gdm
            , string fileName = "", ContentManager content = null)
            : base (gd, gdm, fileName, content)
        {
            ModelPrimitiveType = PrimitiveType.TriangleStrip;

            //PointsMainOld = new List<MainPoint>();
            PointsMain = new List<TrackPoint>();
            PointsRight = new List<Vector3>();
            PointsLeft = new List<Vector3>();
            PointsLeftRampIn = new List<Vector3>();
            PointsLeftRampOut = new List<Vector3>();
            PointsLeftWall = new List<Vector3>();
            PointsRightRampIn = new List<Vector3>();
            PointsRightRampOut = new List<Vector3>();
            PointsRightWall = new List<Vector3>();
            newVertices = new List<Vector3>();
            newIndices = new List<short>();
            collisionBoxes = new BoundingBox[TrackManager.TRACK_LENGTH];
            KillBox = new BoundingBox();
            HasModel = false;

            Scale = 1;
        }

        // Reset the track before new generation
        void Initialize()
        {
            // Empty the points lists
            if (PointsMain != null)
            {
                PointsMain.Clear();
                //PointsMainOld.Clear();
                PointsRight.Clear();
                PointsLeft.Clear();
                PointsLeftRampIn.Clear();
                PointsLeftRampOut.Clear();
                PointsLeftWall.Clear();
                PointsRightRampIn.Clear();
                PointsRightRampOut.Clear();
                PointsRightWall.Clear();
                Triangles.Clear();
                newVertices.Clear();
                newIndices.Clear();
                collisionBoxes = new BoundingBox[TrackManager.TRACK_LENGTH];
                KillBox = new BoundingBox();
                HasModel = false;
            }
        }

        #region Properties
        public List<TrackPoint> TrackPoints
        {
            get
            {
                return PointsMain;
            }
        }

        /// <summary>
        /// Returns the number of points along the track curve
        /// </summary>
        public int MainPointsCount
        {
            get { return PointsMain.Count; }
        }

        /// <summary>
        /// Returns the first point of the track curve
        /// </summary>
        public TrackPoint StartPoint
        {
            get { return PointsMain[0]; }
        }

        /// <summary>
        /// Returns the end point of the track curve
        /// </summary>
        public TrackPoint EndPoint
        {
            get { return PointsMain[PointsMain.Count - 1]; }
        }

        public float TrackWidth
        {
            get 
            {
                return (OUT_RAMP_WIDTH * 2) + (IN_RAMP_WIDTH * 2) + (SECTION_WIDTH);
            }
        }

        // Find the more recent checkpoint that has been passed
        public CheckPoint CurrentCheckPoint
        {
            get
            {
                // Find the more recent checkpoint that has been passed
                for (int i = PointsMain.Count - 1; i >= 0; i--)
                {
                    if (!PointsMain[i].CheckPoint.IsAlive)
                    {
                        return PointsMain[i].CheckPoint;
                    }
                }

                return null;
            }
        }

        // Find the next checkpoint
        public CheckPoint NextCheckPoint
        {
            get
            {
                for (int i = PointsMain.Count - 1; i >= 0; i--)
                {
                    if (!PointsMain[i].CheckPoint.IsAlive)
                    {
                        // If the checkpoint in front of the current checkpoint does not exist
                        // Just return the current check point
                        if (i + 1 >= PointsMain.Count)
                        {
                            return PointsMain[i].CheckPoint;
                        }
                        else
                        {
                            return PointsMain[i + 1].CheckPoint;
                        }
                    }
                }
                return null;
            }
        }
        #endregion



        // Create a new curve for the track to follow
        void GenerateNewCurve()
        {            
            // Generate curve
            // Generate angles
            // Rolling start
            for (int i = 0; i < TrackManager.ROLLING_LENGTH; i++)
            {
                PointsMain.Add(new TrackPoint(0.0f, GetMaxDimension() / 1.5f, PointsMain.Count));
            }
            // Track
            for (int i = 1; i < TrackManager.TRACK_LENGTH; i++)
            {
                if (i < TrackManager.TRACK_LENGTH - TrackManager.ROLLING_LENGTH - 1)
                {
                    // TODO: Proper noise generation
                    PointsMain.Add(new TrackPoint((float)((GlobalRandom.NextDouble() - 0.5f) * 2) * TRACK_TURN_CAP
                        , GetMaxDimension() / 1.5f, PointsMain.Count));
                }
                // Rolling end
                else
                {
                    PointsMain.Add(new TrackPoint(0.0f, GetMaxDimension() / 1.5f, PointsMain.Count));
                }

            }

            // Generate positions
            PointsMain[0].Position = Vector3.Zero;
            PointsMain[0].Direction = Vector3.Backward;

            for (int i = 1; i < PointsMain.Count; i++)
            {
                TrackPoint point = PointsMain[i];
                TrackPoint oldPoint = PointsMain[i - 1];

                point.GeneratePosition(oldPoint, SECTION_LENGTH_MIN);
                oldPoint.GenerateDirection(point.Position);
            }
        }

        // Create a duplicate curve of the main curve but offset to the side
        void GenerateOffsetCurve()
        {
            // Make a duplicate for each point on the main curve but off to the side (section width)
            for (int i = 0; i < PointsMain.Count; i++)
            {
                // Inner ramps
                float inRampWidth = 0;
                float inRampHeight = 0;

                // Outer ramps
                float outRampWidth = 0;
                float outRampHeight = 0;

                //MainPoint mainPointOld = PointsMainOld[i];
                TrackPoint mainPoint = PointsMain[i];
                
                // Main section
                // Used to keep the track parallel to the direction of the track curve
                Vector3 offsetDistance = Vector3.Cross(mainPoint.Direction, Vector3.Up);

                switch (currentMap)
                {
                    case Map.Final_Destination:
                        // Inner ramps
                        inRampWidth = IN_RAMP_WIDTH;
                        inRampHeight = IN_RAMP_HEIGHT;

                        // Outer ramps
                        outRampWidth = OUT_RAMP_WIDTH;
                        outRampHeight = -OUT_RAMP_HEIGHT * 0.5f;
                        break;

                    default:
                    // Inner ramps
                     inRampWidth = IN_RAMP_WIDTH;
                     inRampHeight = IN_RAMP_HEIGHT;

                    // Outer ramps
                     outRampWidth = OUT_RAMP_WIDTH;
                     outRampHeight = OUT_RAMP_HEIGHT;
                        break;
                }


                // Main track
                AddPoint(PointsRight, mainPoint.Position + (offsetDistance * SECTION_WIDTH / 2));
                AddPoint(PointsLeft, mainPoint.Position - (offsetDistance * SECTION_WIDTH / 2));

                // Start position + to the side parallel to track direction of the set width + up the set height
                // Left inner ramp
                //AddPoint(PointsLeftRampIn, (PointsLeft[i] -
                    //(offsetDistance * inRampWidth) + (Vector3.Up * inRampHeight)));
                // Left outer ramp
                //AddPoint(PointsLeftRampOut, (PointsLeftRampIn[i] -
                AddPoint(PointsLeftRampOut, (PointsLeft[i] -
                    (offsetDistance * outRampWidth) + (Vector3.Up * outRampHeight)));

                float wallHeight = -PointsLeftRampOut[i].Y - Math.Abs(outRampHeight) - inRampWidth;
                // Left wall
                AddPoint(PointsLeftWall, (PointsLeftRampOut[i] -
                    (offsetDistance * WALL_WIDTH) + (Vector3.Up * wallHeight)));

                // Right inner ramp
                //AddPoint(PointsRightRampIn, (PointsRight[i] +
                    //(offsetDistance * inRampWidth) + (Vector3.Up * inRampHeight)));
                // Right outer ramp
                //AddPoint(PointsRightRampOut, (PointsRightRampIn[i] +
                AddPoint(PointsRightRampOut, (PointsRight[i] +
                    (offsetDistance * outRampWidth) + (Vector3.Up * outRampHeight)));
                // Right Wall
                AddPoint(PointsRightWall, (PointsRightRampOut[i] +
                    (offsetDistance * WALL_WIDTH) + (Vector3.Up * wallHeight)));
            }
        }

        // Create the 3D model from the curve points
        void BuildTrackModel()
        {
            // Vertices to be fed into the vertex buffer
            newVertices = new List<Vector3>();

            #region Creating Vertices
            newVertices.AddRange(PointsLeftWall);
            newVertices.AddRange(PointsLeftRampOut);
            //newVertices.AddRange(PointsLeftRampIn);
            newVertices.AddRange(PointsLeft);
            newVertices.AddRange(PointsRight);
            //newVertices.AddRange(PointsRight); // Duplicate to flip draw direction
            //newVertices.AddRange(PointsRightRampIn);
            newVertices.AddRange(PointsRightRampOut);
            newVertices.AddRange(PointsRightWall);

            #endregion

            #region Creating indices
            // List of indices to be built
            newIndices = new List<short>();
            
            int lineNo = 0;
            // Fill the index list with sequential numbers
            for (int l = 0; l < newVertices.Count / PointsMain.Count - 1; l++)
            {
                // Draw up every other line (even)
                if (lineNo % 2 == 0)
                {
                    for (int i = 0; i < PointsMain.Count; i++)
                    {
                        newIndices.Add((short)(i + (PointsMain.Count * lineNo)));
                        newIndices.Add((short)(i + (PointsMain.Count * (lineNo + 1))));
                    }
                }
                // Draw down every other line (odd)
                else
                {
                    for (int i = PointsMain.Count - 1; i >= 0; i--)
                    {
                        newIndices.Add((short)(i + (PointsMain.Count * lineNo)));
                        newIndices.Add((short)(i + (PointsMain.Count * (lineNo + 1))));
                    }
                }
                lineNo++;
            }
                
            #endregion

            // Build the model mesh
            BuildModelMesh(newVertices, newIndices.ToArray());
        }


        public override void BuildCollisionModels()
        {
            // Narrow Car collision
            triangleMesh = new TriangleMesh(newVertices, newIndices, ModelPrimitiveType);

            // Broad Car collision
            collisionBoxes = new BoundingBox[triangleMesh.Count / TRIG_PER_BOX + 1];

            // Associate the triangles with their boxes
            int count = 0;
            for (int i = 0; i < triangleMesh.Count; i += TRIG_PER_BOX)
            {
                AddCollisionBox(triangleMesh.MinMaxPoints(i, i + TRIG_PER_BOX - 1), count);
                count++;
            }
            count++;

            // Cars out of bounds collision
            Vector3 killMin = Vector3.Zero;
            Vector3 killMax = Vector3.Zero;

            for (int i = 0; i < newVertices.Count; i++)
            {
                Vector3 trackPoint = newVertices[i];

                Vector3.Min(ref killMin, ref trackPoint, out killMin);
                Vector3.Max(ref killMax, ref trackPoint, out killMax);
            }

            killMin -= new Vector3(TrackManager.KILL_RANGE, TrackManager.KILL_RANGE, TrackManager.KILL_RANGE * 2);
            killMax += new Vector3(TrackManager.KILL_RANGE, TrackManager.KILL_RANGE * 4, TrackManager.KILL_RANGE * 2);

            KillBox = new BoundingBox(killMin, killMax);
        }

        // Create a new track
        public void GenerateNewTrack(Map mapType)
        {
            // Reset track state
            Initialize();
            currentMap = mapType;

            // Create the shape of the track
            GenerateNewCurve();
            GenerateOffsetCurve();
        }

        public void GenerateModel()
        {
            // Fill in the shape
            BuildTrackModel();
            ChangeColor(FillColor, WireColor);

            // Build the collision models
            BuildCollisionModels();

            HasModel = true;
        }

        // Car collision detection
        // Calls car's terrainCollision method if collision detected        
        public void CarCollisionBroad(Car car)
        {
            // Collision with during any narrow phase has been detected
            bool collided = false;
            // Collision during this pass has been detected
            bool current = false;

            // Check for collision with checkpoints
            // Check backwards, removing the first one we find
            for (int i = PointsMain.Count - 1; i >= 0; i--)
            {
                if (PointsMain[i].CheckPoint.CheckCollision(car))
                {
                    break;
                }
            }

            // Check every bounding box of the track
            for (int i = 0; i < collisionBoxes.Length; i++)
            {
                // If the car is inside any of them
                if (collisionBoxes[i].Intersects(car.HitSpheres[0]))
                {
                    // Check for collision with the triangle faces associated with that box
                    current = CarCollisionNarrow(car, i * TRIG_PER_BOX, (i + 1) * TRIG_PER_BOX);

                    // If a collision has been detected, set collided to true
                    if (!collided && current)
                    {
                        collided = true;
                    }
                }
            }

            // If no collisions at all were detected during the narrow phase, set car's OnGround to false
            if (!collided)
            {
                Triangle nope = null;
                car.TerrainCollision(ref nope);
            }
        }

        // Checks for collision with the car and the triangle mesh
        public bool CarCollisionNarrow(Car car)
        {
            return CarCollisionNarrow(car, 0, triangleMesh.Count - 1);
        }

        // Checks for collision with the car and the triangle mesh
        // From a given start and end triangle index
        int recursionDepth = 0;
        public bool CarCollisionNarrow(Car car, int start, int end)
        {
            // Recurse a few times to prevent getting stuck in triangles
            /*
            recursionDepth++;
            if (recursionDepth > 10)
            {
                recursionDepth = 0;
                return null;
            }*/

            // The colliding triangle
            Triangle foundTriangle = null;
            BoundingSphere sphere = car.HitSpheres[0];

            triangleMesh.Intersects(ref sphere, out foundTriangle, start, end);

            if (foundTriangle != null)
            {
                car.TerrainCollision(ref foundTriangle);
                //CarCollisionNarrow(car, start, end);
                return true;
            }

            recursionDepth = 0;
            return false;
        }

        public override void draw(GraphicsDevice graphicsDevice, Camera cam)
        {
            // Draw model
            base.draw(graphicsDevice, cam);
        }

        // Adds a position to a list, increments triangle per row
        // For collision model building
        void AddPoint(List<Vector3> list, Vector3 pos)
        {
            list.Add(pos);
        }

        void AddCollisionBox(Vector3[] MinMaxPoints, int index)
        {
            Vector3 min = MinMaxPoints[0];
            Vector3 max = MinMaxPoints[1];

            collisionBoxes[index] = new BoundingBox(min, max);
        }

        float GetMaxDimension()
        {
            float max = SECTION_LENGTH_MAX;

            return max = Math.Max(max, TrackWidth);
        }
    }
}
