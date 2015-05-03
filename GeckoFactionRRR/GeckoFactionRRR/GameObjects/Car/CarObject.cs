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
    class CarObject : VBGameObjectBasic
        //Base class for objects that snap to the car
    {
        //The parent
        public Car parentCar;
        //Offset from the car's position
        public Vector3 offsetPos = new Vector3(0, 0, 0);
        // Keep default scale, used in randomisation
        public float defaultScale;
        public float scalerValue = 1f;


        bool snapped_rotation = true;


        Matrix mat2;

        public CarObject(GraphicsDevice gd, GraphicsDeviceManager gdm, Car _parentCar
            , string fileName = "", ContentManager content = null, bool _snapped = true)
            : base(gd, gdm, fileName, content)
        {
            parentCar = _parentCar;
            snapped_rotation = _snapped;
        }

        public void UpdatePosition()
        {
            if (parentCar != null)
            {
                Position = offsetPos * parentCar.body.scalerValue;
            }
        }

        public override void update(float dt)
        {
            base.update(dt);

            if (parentCar != null)
            {
                UpdatePosition();

                if (snapped_rotation)
                {
                    worldMat *= parentCar.worldMat;
                    worldMat *= Matrix.CreateTranslation(-worldMat.Translation) * Matrix.CreateFromYawPitchRoll(Yaw, Pitch, Roll) *
                                Matrix.CreateTranslation(worldMat.Translation);
                }
                else
                {
                    mat2 = parentCar.scaleMatrix * Matrix.CreateFromYawPitchRoll(parentCar.Yaw, 0, 0)
                        * parentCar.transMatrix;

                    worldMat *= mat2;
                    worldMat = worldMat * Matrix.CreateTranslation(-worldMat.Translation) * Matrix.CreateFromYawPitchRoll(0, 0, 0) *
                                Matrix.CreateTranslation(worldMat.Translation);
                }
            }
        }

        public virtual string randomCarPart()
        {
            return "no part";
        }

    }
}

