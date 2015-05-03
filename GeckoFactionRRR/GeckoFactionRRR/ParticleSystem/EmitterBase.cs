//
//  ParticleEffect code adapted from work by Jason Mitchell
//
//  Available at: http://jason-mitchell.com/game-development/3d-particle-system-for-xna/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GeckoFactionRRR
{
    class EmitterBase
    {
        protected int particleLifespan;
        protected int emitAmount;
        protected float particleSpeed;
        protected Queue<Particle> freeParticles;
        protected Particle[] particles;
        protected List<Modifier> modifiers = new List<Modifier>();

        //protected Random random = new Random();

        protected float ParticleScale;

        public EmitterBase(int maxParticles, int particleLifespan, int emitAmount, float particleSpeed, float pScale)
        {
            this.particleLifespan = particleLifespan;
            this.emitAmount = emitAmount;
            this.particleSpeed = particleSpeed;

            particles = new Particle[maxParticles];
            freeParticles = new Queue<Particle>(maxParticles);

            ParticleScale = pScale;
        }

        public void LoadContent(List<Texture2D> textures, GraphicsDevice gd)
        {
            if (textures.Count == 0)
                throw new InvalidOperationException("Cannot load a particle effect without a list of textures.");

            List<TextureQuad> quadList = new List<TextureQuad>();

            for (int i = 0; i < textures.Count; i++)
            {
                TextureQuad quad = new TextureQuad(gd, textures[i], textures[i].Width, textures[i].Height, ParticleScale);
                quadList.Add(quad);
            }

            for (int i = 0; i < particles.Length; i++)
            {
                particles[i] = new Particle(quadList[GlobalRandom.Next(quadList.Count)], particleLifespan, textures[GlobalRandom.Next(textures.Count)], gd, ParticleScale);
                freeParticles.Enqueue(particles[i]);
            }
        }

        public virtual void Emit(GameTime gameTime, Vector3 position)
        {
            return;
        }

        public virtual void Emit(float elapsedTime, Vector3 position)
        {
            return;
        }

        public virtual void Emit(float elapsedTime, Vector3 position, Vector3 parentVelocity)
        {
            return;
        }

        //public virtual void Update(GameTime gameTime)
        //{
        //    foreach (Particle particle in particles)
        //    {
        //        if (particle.IsAlive)
        //        {
        //            float particleAge = (float)(gameTime.TotalGameTime.TotalMilliseconds - particle.Inception) / particle.Lifespan;

        //            foreach (Modifier modifier in modifiers)
        //                modifier.Update(particle, particleAge);

        //            particle.Update((float)gameTime.TotalGameTime.TotalMilliseconds);

        //            if (!particle.IsAlive)
        //                freeParticles.Enqueue(particle);
        //        }
        //    }
        //}

        public virtual void Update(float totalTime)
        {
            foreach (Particle particle in particles)
            {
                if (particle != null && particle.IsAlive)
                {
                    float particleAge = (float)(totalTime - particle.Inception) / particle.Lifespan;

                    foreach (Modifier modifier in modifiers)
                        modifier.Update(particle, particleAge);

                    particle.Update(totalTime);

                    if (!particle.IsAlive)
                        freeParticles.Enqueue(particle);
                }
            }
        }

        public virtual void Draw(GraphicsDevice graphicsDevice, Camera cam/*Matrix viewMatrix, Matrix projectionMatrix*/)
        {
            ConfigureEffectGraphics(graphicsDevice);

            foreach (Particle particle in particles)
            {
                if (particle != null && particle.IsAlive)
                    particle.Draw(graphicsDevice, cam/*.viewMat, cam.projMat*/);
            }

            ResetGraphicsDevice(graphicsDevice);
        }

        public static void ConfigureEffectGraphics(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.BlendFactor = Color.Transparent;
            graphicsDevice.BlendState = BlendState.Additive;
            graphicsDevice.DepthStencilState = DepthStencilState.None;
        }

        public static void ResetGraphicsDevice(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        public List<Modifier> Modifiers
        {
            get { return modifiers; }
        }
    }
}