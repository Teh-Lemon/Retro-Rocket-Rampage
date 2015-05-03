// Created by Anthony Lee on 06/03/2014
// Contains methods to hold and interact with a collection of triangles

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GeckoFactionRRR
{
    class TriangleMesh
    {
        // List of all the triangles
        public Triangle[] TriangleList;

        // Constructors
        public TriangleMesh()
        {
        }

        public TriangleMesh(List<Vector3> vertices, List<short> indices, PrimitiveType primitiveType)
        {
            PopulateTriangles(vertices, indices, primitiveType);
        }

        // Fill in the list of triangles from a set of vertices and indices
        public void PopulateTriangles(List<Vector3> vertices, List<short> indices
            , PrimitiveType primitiveType)
        {
            // Find out the number of triangles needed
            int numOfTrigs = 0;

            // Triangle List built
            if (primitiveType == PrimitiveType.TriangleList)
            {
                numOfTrigs = indices.Count / 3;

                // Create an array to store them
                TriangleList = new Triangle[numOfTrigs];
            }
            // Triangle strip built
            else if (primitiveType == PrimitiveType.TriangleStrip)
            {
                numOfTrigs = indices.Count - 2;

                // Create an array to store them
                TriangleList = new Triangle[numOfTrigs];

                for (int i = 0; i < numOfTrigs; i++)
                {
                    Vector3 a = vertices[indices[i]];
                    Vector3 b = vertices[indices[i + 1]];
                    Vector3 c = vertices[indices[i + 2]];
                    TriangleList[i] = new Triangle(a, b, c);
                }
            }    
        }

        public void Intersects(Vector3 position, out Triangle triangle, out float? distance)
        {
            triangle = null;
            distance = null;
            float? newDistance = null;
            Ray intersectRay;
            Vector3 triangleNormal = Vector3.Zero;

            // Check for each triangle
            for (int i = 0; i < TriangleList.Length; i++)
            {
                newDistance = null;
                TriangleList[i].GetNormal(out triangleNormal);
                intersectRay = new Ray(position, Vector3.Negate(triangleNormal));

                TriangleList[i].Intersects(ref intersectRay, out newDistance);

                // If an intersection is found
                if (newDistance.HasValue)
                {
                    // If this is the first intersection, save it
                    if (distance == null)
                    {
                        distance = newDistance;
                        triangle = TriangleList[i];
                    }
                    // If previous intersection already found
                    else
                    {
                        // Check if this new intersection is shorter than the previous
                        // If so, keep it
                        if (newDistance < distance)
                        {
                            distance = newDistance;
                            triangle = TriangleList[i];
                        }
                    }
                }
            }
        }

        // Returns the first triangle to intersect with the given bounding sphere
        public void Intersects(ref BoundingSphere boundingSphere, out Triangle foundTriangle)
        {
            Intersects(ref boundingSphere, out foundTriangle, 0, Count - 1); 
        }

        // Returns the first triangle to intersect with the given bounding sphere
        // Checks only the given indexed triangles
        public void Intersects(ref BoundingSphere boundingSphere, out Triangle foundTriangle, int start, int end)
        {
            foundTriangle = null;

            int lastIndex = Math.Min(end, Count - 1);

            // Check each triangle for a collision
            for (int i = start; i <= lastIndex; i++)
            {
                bool result = false;
                TriangleList[i].Intersects(ref boundingSphere, out result);

                // Stop when a triangle has been found
                if (result)
                {
                    foundTriangle = TriangleList[i];
                    break;
                }
            }  
        }

        // Finds the minimum and maximum vertex of the given triangle indexes
        // Returns a Vector3 array of size 2, holding min and max
        public Vector3[] MinMaxPoints(int startIndex, int endIndex)
        {
            Vector3 min = TriangleList[startIndex].A;
            Vector3 max = TriangleList[startIndex].A;

            int lastIndex = Math.Min(endIndex, Count - 1);

            for (int i = startIndex; i <= lastIndex; i++)
            {
                Triangle trig = TriangleList[i];

                for (int j = 0; j < 3; j++)
                {
                    min = Vector3.Min(min, trig.Vertex[j]);
                    max = Vector3.Max(max, trig.Vertex[j]);
                }
            }

            return new Vector3[2] {min, max};
        }

        // Returns number of triangles
        public int Count
        {
            get { return TriangleList.Length; }
        }
    }
}
