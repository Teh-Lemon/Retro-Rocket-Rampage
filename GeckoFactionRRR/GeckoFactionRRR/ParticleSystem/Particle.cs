//
//  Particle code adapted from work by Jason Mitchell
//
//  Available at: http://jason-mitchell.com/game-development/3d-particle-system-for-xna/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GeckoFactionRRR
{
    class Particle// : IGameObject
    {
        private readonly TextureQuad textureQuad;
        public float ParticleScale;

        public Particle(TextureQuad tQuad, int lifespan, Texture2D texture, GraphicsDevice graphicsDevice, float pScale):base()
        {
            textureQuad = tQuad;
            Lifespan = lifespan;

            ParticleScale = pScale;
        }

        public void Update(float totalMilliseconds)
        {
            Position += Velocity;

            if (Lifespan < (totalMilliseconds - Inception))
                IsAlive = false;
            //base.update(totalMilliseconds);
            textureQuad.Position = Position;
            textureQuad.update(totalMilliseconds);
        }

        public void Draw(GraphicsDevice graphicsDevice, /*Matrix viewMatrix, Matrix projectionMatrix*/ Camera cam)
        {
            textureQuad.Draw(/*viewMatrix, projectionMatrix,*/ Matrix.CreateTranslation(Position), cam);
        }

        /*public override void draw(GraphicsDevice graphicsDevice, Camera camera)
        {
            throw new NotImplementedException();
        }*/

        public bool IsAlive { get; set; }
        public float Inception { get; set; }
        public float Lifespan { get; private set; }
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }

        public float Alpha
        {
            get { return textureQuad.Alpha; }
            set { textureQuad.Alpha = value; }
        }
    }
}
