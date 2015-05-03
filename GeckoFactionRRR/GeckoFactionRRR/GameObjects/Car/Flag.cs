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
    class Flag : CarObject
    {
        //Wave flag - hold total time
        float TotalDT = 0f;

        public Flag(GraphicsDevice gd, GraphicsDeviceManager gdm, Car _parentCar
            , string fileName = "Content/Models/Car/sidebooster.txt", ContentManager content = null)
            : base(gd, gdm, _parentCar, fileName, content)
        {
            Position = new Vector3(0, -1000, 0);
        }

        public override void initialize()
        {
            base.initialize();

            Position = new Vector3(0, -1000, 0);
            parentCar = null;
        }

        public void SetParent(Car car)
        {
            parentCar = car;
            parentCar.hasFlag = true;
            ChangeColor(parentCar.playerColor, Color.White);
        }

        public override void update(float dt)
        {
            base.update(dt);

            if (parentCar != null)
            {
                Yaw = (MathHelper.PiOver4);
            }

            //'Wave' flag
            // Increase multiplication to speed up wave
            TotalDT += (50 * dt);
            // Change division change 'wave' angle
            Yaw += ((float)Math.Sin(TotalDT)) / 12;
        }

        public override void BuildCollisionModels()
        {
            AddHitSphere(Position, GetMaxDimensions(dimensions));
        }
    }
}
