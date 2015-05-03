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
    class PhysicsPowerUp : BasePowerUp
    {
        int oldGravity = 350;

        public PhysicsPowerUp(List<Player> pList, GraphicsDevice gd, GraphicsDeviceManager gdm,
            string fileName = "", ContentManager content = null)
            : base(pList, gd, gdm, fileName, content)
        {
            //test postion
            SetPosition(new Vector3(50, 20, 500));

            if (content != null)
            {
                SpriteIdentifier = new Sprite3D(new Vector3(-50, 0, 500),
                    content.Load<Texture2D>("Materials/Powerups/RRRPhysicsPlaceholder"), content, gd, gdm, false, true, SPRITE_SCALE);
            }

            ChangeColor(Color.Violet, Color.Violet);
        }

        public override void ActivatePower()
        {
            PoweredUpCar.GRAVITY = GlobalRandom.Next(250, 450);
            PoweredUpCar.IsBouncy = true;

            base.ActivatePower();

            SoundManager.PlayScalePowerUp(PoweredUpCar.SoundEmitter, 0.3f);
        }

        public override void DeactivatePower()
        {
            PoweredUpCar.GRAVITY = oldGravity;
            PoweredUpCar.IsBouncy = false;
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
