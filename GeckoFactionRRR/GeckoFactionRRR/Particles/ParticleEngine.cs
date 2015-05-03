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


namespace GeckoFactionRRR.Particles
{
    public class ParticleEngine
    {
        //private Random random;
        public Vector2 EmitterLocation { get; set; }
        private List<Particles> particles;
        private List<Texture2D> textures;

        public ParticleEngine(List<Texture2D> textures, Vector2 location)
        {
            EmitterLocation = location;
            this.textures = textures;
            this.particles = new List<Particles>();
            //random = new Random();
        }

        private Particles GenerateNewParticle()
        {
            Texture2D texture = textures[GlobalRandom.Next(textures.Count)];
            Vector2 position = EmitterLocation;
            Vector2 velocity = new Vector2(
                1f * (float)(GlobalRandom.NextDouble() * 2 - 1),
                1f * (float)(GlobalRandom.NextDouble() * 2 - 1));
            float angle = 0;
            float angularVelocity = 0.1f * (float)(GlobalRandom.NextDouble() * 2 - 1);
            Color color = new Color(
                (float)GlobalRandom.NextDouble(),
                (float)GlobalRandom.NextDouble(),
                (float)GlobalRandom.NextDouble());
            float size = (float)GlobalRandom.NextDouble();
            int ttl = 20 + GlobalRandom.Next(40);

            return new Particles(texture, position, velocity, angle, angularVelocity, color, size, ttl);
        }

        public void Update()
        {
            int total = 10;
            for (int i = 0; i < total; i++)
            {
                particles.Add(GenerateNewParticle());
            }
            for (int particle = 0; particle < particles.Count; particle++)
            {
                particles[particle].Update();
                if (particles[particle].TTL <= 0)
                {
                    particles.RemoveAt(particle);
                    particle--;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            for (int index = 0; index < particles.Count; index++)
            {
                particles[index].Draw(spriteBatch);
            }

                spriteBatch.End();
        }
    }
}
