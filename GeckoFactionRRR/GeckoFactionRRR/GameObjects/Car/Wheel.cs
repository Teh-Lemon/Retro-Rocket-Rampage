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
    class Wheel : CarObject
    {
        //string[] wheelModels = new string[2];
        List<string> wheelModels = new List<string>();
        public string currentPartString = null;

        public int stability = 100;

        public Wheel(GraphicsDevice gd, GraphicsDeviceManager gdm, Car _parentCar
            , string fileName = "Content/Models/Car/thinwheel.txt", ContentManager content = null
            )
            : base(gd, gdm, _parentCar, fileName, content, false)
        {

            wheelModels.Add("Content/Models/Car/thinwheel.txt");
            wheelModels.Add("Content/Models/Car/thickwheel.txt");
            wheelModels.Add("Content/Models/Car/wheel3.txt");
            wheelModels.Add("Content/Models/Car/wheel3wspike.txt");
            wheelModels.Add("Content/Models/Car/bloodhound_wheel.txt");

            offsetPos = new Vector3(1f, 0f, 1);
            Scale = 5;
            defaultScale = Scale;

            currentPartString = fileName;
        }

        public override void LoadModelFromFile(string fileName = "")
        {
            base.LoadModelFromFile(fileName);

            stability = (int)modelStat;
        }

        public override string randomCarPart()
        {
            int randInt;
            string part;

            randInt = GlobalRandom.Next(0, wheelModels.Count());

            part = wheelModels[randInt];

            currentPartString = part;

            return part;
        }

        // Roll the wheels forward
        // Velocity is linear
        void RollWheels(float dt, float velocity)
        {
            if (velocity > 0)
            {
                float radius = GetMaxDimensions(dimensions) * Scale / 2;
                float angularVelocity = velocity / radius;

                Pitch += angularVelocity * dt;
            }
        }

        public override void update(float dt)
        {
            base.update(dt);

            if (parentCar.EngineOn)
            {
                RollWheels(dt, parentCar.Velocity.Length());
            }
        }

        public override void draw(GraphicsDevice graphicsDevice, Camera cam)
        {
            if (HitSpheres.Count > 0)
            {
                DebugShapeRenderer.AddBoundingSphere(HitSpheres[0], Color.Yellow);
            }

            base.draw(graphicsDevice, cam);
        }
    }
}
