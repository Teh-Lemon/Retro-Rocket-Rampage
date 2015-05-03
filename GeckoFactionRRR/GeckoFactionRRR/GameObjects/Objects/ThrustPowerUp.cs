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
    class ThrustPowerUp : BasePowerUp
    {
        const float ENGINE_BOOST = 500;
        const float ROCKET_BOOST = 300;

        public ThrustPowerUp(List<Player> pList, GraphicsDevice gd, GraphicsDeviceManager gdm,
            string fileName = "", ContentManager content = null)
            : base(pList, gd, gdm, fileName, content)
        {
            PowerupLifetime = 4;


            SetPosition(new Vector3(0, 20, 700));

            if (content != null)
            {
                SpriteIdentifier = new Sprite3D(new Vector3(-50, 0, 500),
                    content.Load<Texture2D>("Materials/Powerups/RRRThrustPlaceholder"), content, gd, gdm, false, true, SPRITE_SCALE);
            }

            ChangeColor(Color.OrangeRed, Color.OrangeRed);
        }

        public override void ActivatePower()
        {
            PoweredUpCar.BOOST_STRENGTH += ENGINE_BOOST;
            PoweredUpCar.ROCKET_STRENGTH += ROCKET_BOOST;
            PoweredUpCar.currentBoost = PoweredUpCar.BOOST_LIFE; 
            base.ActivatePower();

            SoundManager.PlayBoostPowerUp(PoweredUpCar.SoundEmitter, 0.3f);
        }

        public override void DeactivatePower()
        {
            PoweredUpCar.BOOST_STRENGTH -= ENGINE_BOOST;
            PoweredUpCar.ROCKET_STRENGTH -= ROCKET_BOOST;
            base.DeactivatePower();
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
