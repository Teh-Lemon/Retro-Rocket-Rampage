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
    class Body : CarObject
    {
        List<string> bodyModels = new List<string>();

        public Vector3 mainBoosterOffset = new Vector3(0, 0, 0);
        public Vector3 sideBoosterOffset = new Vector3(0, 0, 0);
        public Vector3 wheelOffset = new Vector3(0, 0, 0);

        public int weight = 100;

        public Body(GraphicsDevice gd, GraphicsDeviceManager gdm, Car _parentCar
            , string fileName = "", ContentManager content = null)
            : base(gd, gdm, _parentCar, fileName, content)
        {

            offsetPos = new Vector3(0,0,0);
            Scale = 1;
            defaultScale = Scale;

            bodyModels.Add("Content/Models/Car/spaceship.txt");
            bodyModels.Add("Content/Models/Car/joypad.txt");
            bodyModels.Add("Content/Models/Car/boat.txt");
            bodyModels.Add("Content/Models/Car/jet.txt");
            bodyModels.Add("Content/Models/Car/crane.txt");
            bodyModels.Add("Content/Models/Car/tank.txt");
            bodyModels.Add("Content/Models/Car/spaceracer.txt");
            bodyModels.Add("Content/Models/Car/bloodhound_body.txt");
        }

        public override void LoadModelFromFile(string fileName = "")
        {
            base.LoadModelFromFile(fileName);

            weight = (int)modelStat;
            weight = (int)MathHelper.Clamp(weight, 0, 200f);

            if (partOffsets.Count() == 3)
            {
                mainBoosterOffset = partOffsets[0];
                sideBoosterOffset = partOffsets[1];
                wheelOffset = partOffsets[2];
            }
            partOffsets.Clear();
        }

        public override string randomCarPart()
        {
            int randInt;
            string part;

            randInt = GlobalRandom.Next(0, bodyModels.Count);

            part = bodyModels[randInt];

            return part;
        }

        public override void update(float dt)
        {
            //Scale = modelScale * parentCar.Scale;
            base.update(dt);
        }

        public override void draw(GraphicsDevice graphicsDevice, Camera cam)
        {
            base.draw(graphicsDevice, cam);
        }

    }
}
