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

//Link to particle emitter tutorial http://rbwhitaker.wikidot.com/2d-particle-engine-4

namespace GeckoFactionRRR.Particles
{
    public class Particles
    {
        public Texture2D particleTexture { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Angle { get; set; }
        public float AngularVelocity { get; set; }
        public Color Color { get; set; }
        public float Size { get; set; }
        public int TTL { get; set; }

        public Particles(Texture2D particle, Vector2 position, Vector2 velocity,
            float angle, float angularVelocity, Color color, float size, int ttl)
        {
            particleTexture = particle;
            Position = position;
            Velocity = velocity;
            Angle = angle;
            AngularVelocity = angularVelocity;
            Color = color;
            Size = size;
            TTL = ttl;
        }

        public void Update()
        {
            TTL--;
            Position += Velocity;
            Angle += AngularVelocity;
        }

        public void Draw(SpriteBatch spriteBatch)
        {

            Rectangle sourceRectangle = new Rectangle(0, 0, particleTexture.Width, particleTexture.Height);
            Vector2 origin = new Vector2(particleTexture.Width / 2, particleTexture.Height / 2);
            spriteBatch.Draw(particleTexture, Position, sourceRectangle, Color,
                Angle, origin, Size, SpriteEffects.None, 0f);
        }
    }
}
