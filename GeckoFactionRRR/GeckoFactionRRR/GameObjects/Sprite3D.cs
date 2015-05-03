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
    class Sprite3D : VBGameObjectBasic
    {
        //public float size = 0.0f;

        public Texture2D texture;
        public VertexBuffer vertexBuffer;
        public IndexBuffer indexBuffer;
        bool isBillboard;
        Effect effect;
        Matrix billboardMatrix;
        public float scaleTarget;
        public bool canRescale = true;

        List<Texture2D> sprites = new List<Texture2D>();

        int updateCount = 0;
        int frameRate = 2;
        int frame = 0;
        bool animated;

        public Sprite3D(Vector3 position, Texture2D spriteTex1, ContentManager Content,
            GraphicsDevice gd, GraphicsDeviceManager gdm, bool animation = false, bool billboarded = true, float spriteScale = 1.0f, Texture2D spriteTex2 = null, int framerate = 3)
            : base(gd, gdm)
        {

            sprites.Add(spriteTex1);
            if (animation) sprites.Add(spriteTex2);

            Position = position;
            animated = animation;
            
            isBillboard = billboarded;
            Scale = spriteScale;
            scaleTarget = spriteScale;
            
            effect = Content.Load<Effect>(@"Effects\Albedo");
            effect.CurrentTechnique = effect.Techniques["Albedo"];

            frameRate = framerate;

            texture = spriteTex1;
            CreateTextureQuad(gd);
        }

        public void CreateTextureQuad(GraphicsDevice gd)
        {
            float width = texture.Bounds.Width * Scale;
            float height = texture.Bounds.Height * Scale;

            float xCenter = width * 0.5f;
            float yCenter = height * 0.5f;

            Vector3 upperLeft = new Vector3(xCenter, yCenter, 0.0f);
            Vector3 upperRight = new Vector3(-xCenter, yCenter, 0.0f);
            Vector3 lowerLeft = new Vector3(xCenter, -yCenter, 0.0f);
            Vector3 lowerRight = new Vector3(-xCenter, -yCenter, 0.0f);

            VertexPositionTexture[] vertices =
            {
                new VertexPositionTexture(upperLeft,  new Vector2(0.0f, 0.0f)),  // 0
                new VertexPositionTexture(upperRight, new Vector2(1.0f, 0.0f)),  // 1
                new VertexPositionTexture(lowerLeft,  new Vector2(0.0f, 1.0f)),  // 2
                new VertexPositionTexture(lowerRight, new Vector2(1.0f, 1.0f)),  // 3
            };

            vertexBuffer = new VertexBuffer(gd, typeof(VertexPositionTexture), vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);

            short[] indices =
            {
                0, 1, 2,
                2, 1, 3
            };

            indexBuffer = new IndexBuffer(gd, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
        }

        public override void update(float dt)
        {
            if (animated)
            {
                updateCount++;

                if (updateCount >= (int)(60 / frameRate))
                {
                    frame++;
                    updateCount = 0;
                }

                if (frame >= sprites.Count)
                {
                    frame = 0;
                }

                texture = sprites[frame];
            }

            base.update(dt);
        }

        public override void draw(GraphicsDevice graphicsDevice, Camera cam)
        {
            // Rescale sprites if they are not at target scale
            if (canRescale && Scale != scaleTarget)
            {
                if (Scale > scaleTarget)
                {
                    Scale -= 0.01f;
                }
                else if (Scale < scaleTarget)
                {
                    Scale += 0.01f;
                }
                CreateTextureQuad(graphicsDevice);
            }

            graphicsDevice.SetVertexBuffer(vertexBuffer);
            graphicsDevice.Indices = indexBuffer;

            RasterizerState prevRasterizerState = graphicsDevice.RasterizerState;
            BlendState prevBlendState = graphicsDevice.BlendState;

            graphicsDevice.BlendState = BlendState.NonPremultiplied;
            graphicsDevice.RasterizerState = RasterizerState.CullNone;

            // Create a matrix that will rotate the billboard so that it will
            // always face the camera.

            billboardMatrix = Matrix.CreateConstrainedBillboard(Position,
                cam.Position, Vector3.Up, new Vector3(100, 100, 100), new Vector3(100, 100, 100));

            if (isBillboard)
            {
                effect.Parameters["world"].SetValue(billboardMatrix);
            }
            else
            {
                effect.Parameters["world"].SetValue(worldMat);
            }

            effect.Parameters["view"].SetValue(cam.viewMat);
            effect.Parameters["projection"].SetValue(cam.projMat);
            effect.Parameters["colorMap"].SetValue(texture);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                    0, 0, vertexBuffer.VertexCount, 0, 2);
            }
            
            graphicsDevice.SetVertexBuffer(null);
            graphicsDevice.Indices = null;

            graphicsDevice.BlendState = prevBlendState;
            graphicsDevice.RasterizerState = prevRasterizerState;
        }

    }
}
