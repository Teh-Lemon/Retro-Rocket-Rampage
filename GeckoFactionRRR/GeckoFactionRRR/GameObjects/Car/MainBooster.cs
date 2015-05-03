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
    class MainBooster : CarObject
    {
        List<string> boosterModels = new List<string>();

        public int boostStrength = 100;

        public MainBooster(GraphicsDevice gd, GraphicsDeviceManager gdm, Car _parentCar
            , string fileName = "", ContentManager content = null)
            : base(gd, gdm, _parentCar, fileName, content)
        {
            offsetPos = new Vector3(0, 0.7f, -3);
            Scale = 0.8f;
            defaultScale = Scale;

            boosterModels.Add("Content/Models/Car/boltbooster.txt");
            boosterModels.Add("Content/Models/Car/bigbooster.txt");
            boosterModels.Add("Content/Models/Car/prismbooster.txt");
            boosterModels.Add("Content/Models/Car/bigbooster2.txt");
            boosterModels.Add("Content/Models/Car/bloodhound_booster.txt");
        }

        public override void LoadModelFromFile(string fileName = "")
        {
            base.LoadModelFromFile(fileName);

            boostStrength = (int)modelStat;
        }

        public override string randomCarPart()
        {
            int randInt;
            string part;

            randInt = GlobalRandom.Next(0, boosterModels.Count);

            part = boosterModels[randInt];

            return part;
        }

    }
}
