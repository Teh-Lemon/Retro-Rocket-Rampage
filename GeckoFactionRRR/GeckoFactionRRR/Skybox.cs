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
    class Skybox
    {
        const float FADE_SPEED = 0.75f;

        Camera camera;
        Vector3 targetDiff;
        double yRot;

        Texture2D texture;
        GameWindow window;
        Rectangle drawRectangle;

        public int horizon;

        public float Opacity { get; set; }
        float opacityVelocity = 0.0f;

        public Skybox(Texture2D inTexture, Camera cam, GameWindow win)
        {
            camera = cam;
            window = win;
            texture = inTexture;

            Opacity = 1.0f;
        }

        public void update(float dt)
        {
            targetDiff = camera.target - camera.Position;
            yRot = Math.Atan2(targetDiff.Y, Math.Sqrt(targetDiff.X * targetDiff.X + targetDiff.Z * targetDiff.Z));

            drawRectangle = new Rectangle(0, 0 - window.ClientBounds.Height +
                (int)((yRot * 900 - 10) / 768 * window.ClientBounds.Height), window.ClientBounds.Width,
                window.ClientBounds.Height * 3);

            horizon = drawRectangle.Center.Y;

            // Play fade animation
            Opacity += (opacityVelocity * dt);

            // Stop fade animation
            if (Opacity < 0)
            {
                Opacity = 0.0f;
                opacityVelocity = 0.0f;
            }
            else if (Opacity > 1)
            {
                Opacity = 1.0f;
                opacityVelocity = 0.0f;
            }
        }

        public void draw(SpriteBatch aSpriteBatch)
        {
            aSpriteBatch.Draw(texture, drawRectangle, Color.White * Opacity);
        }

        // Fade the skybox to black or back on
        public void FadeOn(bool on)
        {
            if (on)
            {
                opacityVelocity = FADE_SPEED;
            }
            else
            {
                opacityVelocity = -FADE_SPEED;
            }
        }

    }
}
