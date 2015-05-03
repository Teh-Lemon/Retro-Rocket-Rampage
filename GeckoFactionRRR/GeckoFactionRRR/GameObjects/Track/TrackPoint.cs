// Created on 30/04/2014 by Anthony Lee
// Stores information about a point along the curve of the track

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GeckoFactionRRR
{
    class TrackPoint
    {
        int ID = 0;
        // Position of point in world space
        public Vector3 Position { get; set; }
        // Angle of forward direction to next point
        public float Angle { get; set; }
        // Direction vector to the next point
        Vector3 direction;
        // Checkpoint
        public CheckPoint CheckPoint { get; set; }

        public Boolean hasScenery = false;

        public Vector3 Direction
        {
            get { return direction; }
            set { direction = Vector3.Normalize(value); } 
        }

        public TrackPoint(float angle, float checkPointSize, int id)
        {
            Angle = angle;
            Position = Vector3.Zero;
            Direction = Vector3.Backward;
            ID = id;

            CheckPoint = new CheckPoint(Position, checkPointSize, ID);
        }

        // Find the Position from the previous point and angle
        public void GeneratePosition(TrackPoint oldPoint, float distance)
        {
            // Previous point
            Vector3 origin = oldPoint.Position;
            // New point projected forward from previous point
            Vector3 projectedVector = Vector3.Normalize(oldPoint.Direction);
            Vector3 preTarget = oldPoint.Position
                + (projectedVector * distance);

            Vector3 newPosition = Vector3.Zero;
            // Rotate the preTarget around the previous point
            newPosition = Vector3.Transform(preTarget - origin, Matrix.CreateRotationY(oldPoint.Angle));
            newPosition += origin;

            Direction = oldPoint.Direction;

            Position = newPosition;

            CheckPoint.Position = Position;
            CheckPoint.UpdateSpherePosition();
        }

        // Finds the direction vector to the next point
        public void GenerateDirection(Vector3 nextPosition)
        {
            Direction = Vector3.Normalize(nextPosition - Position);
        }


        /// <summary>
        /// Finds the 2 positions outside the track curve
        /// </summary>
        /// <param name="distance">Distance from the main track point</param>
        /// <returns>Vector3 array, size 2. Left and right.</returns>
        public Vector3[] GetOutsidePoints(float distance)
        {
            Vector3 offsetDistance = Vector3.Cross(Direction, Vector3.Up);
            offsetDistance = Vector3.Normalize(offsetDistance);

            Vector3 left = (offsetDistance * distance) + Position;
            Vector3 right = (offsetDistance * -distance) + Position;

            return new Vector3[2] {left, right};
        }
    }
}
