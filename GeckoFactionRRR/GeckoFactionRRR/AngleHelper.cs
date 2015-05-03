// Contains methods for converting from matrix to euler angles
// Created on 06/05/2014
// Based on http://www.programdevelop.com/4090766/

using System;
using Microsoft.Xna.Framework;

namespace GeckoFactionRRR
{
    public static class AngleHelper
    {
        static Vector3 AngleTo(Vector3 from, Vector3 location)
        {
            Vector3 angle = new Vector3();
            Vector3 v3 = Vector3.Normalize(location - from);

            angle.X = (float)Math.Asin(v3.Y);
            angle.Y = (float)Math.Atan2((double)-v3.X, (double)-v3.Z);

            return angle;
        }

        // Converts a Quaternion to Euler angles (X = Yaw, Y = Pitch, Z = Roll)  
        static Vector3 QuaternionToEulerAngleVector3(Quaternion rotation)
        {
            Vector3 rotationaxes = new Vector3();
            Vector3 forward = Vector3.Transform(Vector3.Forward, rotation);
            Vector3 up = Vector3.Transform(Vector3.Up, rotation);

            rotationaxes = AngleTo(new Vector3(), forward);

            if (rotationaxes.X == MathHelper.PiOver2)
            {
                rotationaxes.Y = (float)Math.Atan2((double)up.X, (double)up.Z);
                rotationaxes.Z = 0;
            }
            else if (rotationaxes.X == -MathHelper.PiOver2)
            {
                rotationaxes.Y = (float)Math.Atan2((double)-up.X, (double)-up.Z);
                rotationaxes.Z = 0;
            }
            else
            {
                up = Vector3.Transform(up, Matrix.CreateRotationY(-rotationaxes.Y));
                up = Vector3.Transform(up, Matrix.CreateRotationX(-rotationaxes.X));

                rotationaxes.Z = (float)Math.Atan2((double)-up.Z, (double)up.Y);
            }

            return rotationaxes;
        }

        // Converts a Rotation Matrix to a quaternion, then into a Vector3 containing  
        // Euler angles (X: Pitch, Y: Yaw, Z: Roll)  
        public static Vector3 MatrixToEulerAngleVector3(Matrix Rotation)
        {
            Vector3 translation, scale;
            Quaternion rotation;

            Rotation.Decompose(out scale, out rotation, out translation);

            Vector3 eulerVec = QuaternionToEulerAngleVector3(rotation);

            return eulerVec;
        }  

    }
}
