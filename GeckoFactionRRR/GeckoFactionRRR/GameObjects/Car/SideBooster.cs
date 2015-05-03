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
    class SideBooster : CarObject
    {
        List<string> sideBoosterModels = new List<string>();
        

        public SideBooster(GraphicsDevice gd, GraphicsDeviceManager gdm, Car _parentCar
            , string fileName = "Content/Models/Car/sidebooster.txt", ContentManager content = null
            )
            : base(gd, gdm, _parentCar, fileName, content)
        {

            offsetPos = new Vector3(0, 0.7f, -3);
            Scale = 1;
            defaultScale = Scale;

            sideBoosterModels.Add("Content/Models/Car/sidebooster.txt");
        }

        public override string randomCarPart()
        {
            int randInt;
            string part;

            randInt = GlobalRandom.Next(0, sideBoosterModels.Count);

            part = sideBoosterModels[randInt];

            return part;
        }
    }
}
