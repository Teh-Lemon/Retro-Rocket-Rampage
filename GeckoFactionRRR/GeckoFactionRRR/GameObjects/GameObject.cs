//  Source: Coding for Games Development tutorial 7

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
    class GameObject : IGameObject
    {
        public  Model model { get; set; }

        public Color colour { get; set; }

        public GameObject( string fileName = "", ContentManager content = null )
        {      

            //load required model
            if (content != null)
            {
                model = content.Load<Model>(fileName);
            }
        }

        public override void update(float dt )
        {
            base.update(dt);
        }

        public override void draw(GraphicsDevice graphicsDevice, Camera camera)
        {
            if (model != null)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();
                        effect.PreferPerPixelLighting = true;

                        effect.AmbientLightColor = colour.ToVector3();
                        
                        effect.World = worldMat;
                        effect.Projection = camera.projMat;
                        effect.View = camera.viewMat;
                    }

                    mesh.Draw();
                }
            }
        }
    }
}
