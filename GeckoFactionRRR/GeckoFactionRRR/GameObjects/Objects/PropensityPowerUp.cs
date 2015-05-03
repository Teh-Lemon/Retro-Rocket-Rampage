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
    class PropensityPowerUp : BasePowerUp
    {

        public PropensityPowerUp(List<Player> pList, GraphicsDevice gd, GraphicsDeviceManager gdm,
            string fileName = "", ContentManager content = null)
            : base(pList, gd, gdm, fileName, content)
        {
            if (content != null)
            {
                SpriteIdentifier = new Sprite3D(new Vector3(-50, 0, 500),
                    content.Load<Texture2D>("Materials/Powerups/RRRUnknownPlaceholder"), content, gd, gdm, false, true, SPRITE_SCALE);
            }

            ChangeColor(Color.Olive, Color.Olive);
        }

        public override void ActivatePower()
        {
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
