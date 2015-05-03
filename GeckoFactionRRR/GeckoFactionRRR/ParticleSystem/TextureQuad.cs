//
//  TextureQuad code adapted from work by Jason Mitchell
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
    class TextureQuad : IGameObject
    {
        private static readonly Vector2 UpperLeft = new Vector2(0, 0);
        private static readonly Vector2 UpperRight = new Vector2(1, 0);
        private static readonly Vector2 BottomLeft = new Vector2(0, 1);
        private static readonly Vector2 BottomRight = new Vector2(1, 1);

        private readonly VertexBuffer vertexBuffer;
        private readonly BasicEffect effect;

        GraphicsDevice graphicsDevice;

        bool isBillboarded;
        Matrix billboardMatrix;

        public TextureQuad(GraphicsDevice gd, Texture2D texture, /*int*/ float width, float height, float particleScale, bool billboarded = true):base()
        {
            Scale = particleScale;
            graphicsDevice = gd;
            width *= Scale;
            height *= Scale;
            VertexPositionTexture[] vertices = CreateQuadVertices(width, height);
            vertexBuffer = new VertexBuffer(gd, typeof(VertexPositionTexture), vertices.Length, BufferUsage.None);
            vertexBuffer.SetData(vertices);

            effect = new BasicEffect(gd) { TextureEnabled = true, Texture = texture };

            isBillboarded = billboarded;
        }

        private static VertexPositionTexture[] CreateQuadVertices(/*int*/ float width, /*int*/ float height)
        {
            
            float halfWidth = width / 2;
            float halfHeight = height / 2;

            VertexPositionTexture[] vertices = new VertexPositionTexture[4];

            vertices[3] = new VertexPositionTexture(new Vector3(-halfWidth, halfHeight, 0), UpperLeft);
            vertices[1] = new VertexPositionTexture(new Vector3(halfWidth, halfHeight, 0), UpperRight);
            vertices[2] = new VertexPositionTexture(new Vector3(-halfWidth, -halfHeight, 0), BottomLeft);
            vertices[0] = new VertexPositionTexture(new Vector3(halfWidth, -halfHeight, 0), BottomRight);

            return vertices;
        }

        public void Draw(/*Matrix viewMatrix, Matrix projectionMatrix, */Matrix worldMatrix , Camera cam)
        {
            effect.GraphicsDevice.SetVertexBuffer(vertexBuffer);

            if (isBillboarded)
            {
                billboardMatrix = Matrix.CreateConstrainedBillboard(Position,
                cam.Position, Vector3.Up, new Vector3(100, 100, 100), new Vector3(100, 100, 100));
                effect.World = billboardMatrix;
            }
            else
            {
                effect.World = worldMatrix;
            }
            effect.View = cam.viewMat;
            effect.Projection = cam.projMat;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                effect.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
                
                effect.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }
        }

        public override void draw(GraphicsDevice graphicsDevice, Camera camera)
        {
            throw new NotImplementedException();
        }

        public override void update(float dt)
        {
            base.update(dt);
        }

        public float Alpha
        {
            get { return effect.Alpha; }
            set { effect.Alpha = value; }
        }
    }
}
