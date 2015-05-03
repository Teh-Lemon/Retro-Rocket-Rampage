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
    class TerrainPowerUp : BasePowerUp
    {
        TrackManager TrManager;

        public TerrainPowerUp(TrackManager tManage, List<Player> pList, GraphicsDevice gd, GraphicsDeviceManager gdm,
            string fileName = "", ContentManager content = null)
            : base(pList, gd, gdm, fileName, content)
        {
            TrManager = tManage;

            PowerupLifetime = 0;

            SetPosition(new Vector3(-10, 20, 1000));

            if (content != null)
            {
                SpriteIdentifier = new Sprite3D(new Vector3(-50, 0, 500),
                    content.Load<Texture2D>("Materials/Powerups/RRRTerrainPlaceholder"), content, gd, gdm, false, true, SPRITE_SCALE);
            }

            ChangeColor(Color.PaleGoldenrod, Color.PaleGoldenrod);
        }

        public override void ActivatePower()
        {
            int ran = 0;
            Biome newBiome = null;

            do
            {
                ran = GlobalRandom.Next(1, 4); // Removed city from selection

                switch (ran)
                {
                    case 0:
                        newBiome = City;
                        for (int i = 0; i < playerList.Count; i++)
                        {
                            playerList[i].myCar.IsBouncy = false;
                            playerList[i].myCar.ConstantFullBoost = false;
                        }
                        break;
                    case 1:
                        newBiome = Bouncy;
                        for (int i = 0; i < playerList.Count; i++)
                        {
                            playerList[i].myCar.IsBouncy = true;
                            playerList[i].myCar.ConstantFullBoost = false;
                        }
                        break;
                    case 2:
                        newBiome = Sun;
                        for (int i = 0; i < playerList.Count; i++)
                        {
                            playerList[i].myCar.IsBouncy = false;
                            playerList[i].myCar.ConstantFullBoost = true;
                        }
                        break;
                    case 3:
                        newBiome = Rapture;
                        for (int i = 0; i < playerList.Count; i++)
                        {
                            playerList[i].myCar.IsBouncy = false;
                            playerList[i].myCar.ConstantFullBoost = false;
                        }
                        break;
                    default:
                        break;
                }
            } while (newBiome == TrManager.CurrentBiome);

            TrManager.setBiome(newBiome);

            SoundManager.PlayTrackPowerUp(PoweredUpCar.SoundEmitter, 0.3f);
        }

        public override void update(float dt)
        {
            base.update(dt);
        }

        public override void draw(GraphicsDevice graphicsDevice, Camera cam)
        {
            base.draw(graphicsDevice, cam);
        }
    }
}
