using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GeckoFactionRRR
{
    class ExplosionEmitter : EmitterBase
    {
        public ExplosionEmitter(int maxParticles, int particleLifespan, int emitAmount, float particleSpeed, float pScale)
            : base(maxParticles, particleLifespan, emitAmount, particleSpeed, pScale)
        {
            this.particleLifespan = particleLifespan;
            this.emitAmount = emitAmount;
            this.particleSpeed = particleSpeed;

            particles = new Particle[maxParticles];
            freeParticles = new Queue<Particle>(maxParticles);
        }

        public override void Emit(float elapsedTime, Vector3 position)
        {
            //float totalMilliseconds = (float)gameTime.TotalGameTime.TotalMilliseconds;

            for (int i = 0; i < emitAmount && freeParticles.Count > 0; i++)
            {
                Particle particle = freeParticles.Dequeue();
                particle.IsAlive = true;
                particle.Position = position;
                particle.Inception = elapsedTime;

                Vector3 velocity = new Vector3((float)GlobalRandom.NextDouble() * particleSpeed, 0, 0);

                velocity = Vector3.Transform(velocity, Matrix.CreateRotationZ(MathHelper.ToRadians(GlobalRandom.Next(360))));
                particle.Velocity = Vector3.Transform(velocity, Matrix.CreateRotationX(MathHelper.ToRadians(GlobalRandom.Next(360))));
            }
        }

        //public override void Update(GameTime gameTime)
        //{
        //    base.Update(gameTime);
        //}

        public override void Update(float totalTime)
        {
            base.Update(totalTime);
        }

        public override void Draw(GraphicsDevice graphicsDevice, Camera cam /*Matrix viewMatrix, Matrix projectionMatrix*/)
        {
            base.Draw(graphicsDevice, cam/*viewMatrix, projectionMatrix*/);
        }
    }
}
