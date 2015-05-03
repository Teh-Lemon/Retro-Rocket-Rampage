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
    abstract class VBGameObject : IGameObject
    {
        protected Effect shader;        
        protected VertexDeclaration myVertDec;
        protected VertexBuffer myVBuffer;
        protected IndexBuffer myIBuffer;
         

        protected int noVerts = 0;
        protected int noInd = 0;

        public VBGameObject(string fileName = "", ContentManager content = null):base()
        {
            //load required shader
            if (content != null)
            {
                shader = content.Load<Effect>(fileName);
            }
        }

        public override void update(float dt)
        {
            base.update(dt);
        }

        public abstract void setRenderState(GraphicsDevice graphicsDevice, Camera cam);

        public override void draw(GraphicsDevice graphicsDevice, Camera cam)
        {
            setRenderState(graphicsDevice,cam); 
            
            graphicsDevice.SetVertexBuffer(myVBuffer);
            graphicsDevice.Indices = myIBuffer;

            shader.CurrentTechnique.Passes[0].Apply();
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, noVerts, 0, noInd/3);
        }

        protected static Vector3 findNormal( Vector3 pos1, Vector3 pos2, Vector3 pos3 )
        {
            Vector3 side1 = pos2 - pos1;
            Vector3 side2 = pos3 - pos1;
            Vector3 cross = Vector3.Cross(side1, side2);
            return Vector3.Normalize(cross);
        }

    }
}
