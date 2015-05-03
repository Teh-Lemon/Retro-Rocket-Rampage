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
    class TrackSection : VBGameObjectBasic
    {
        //  Length of track
        public float length { get; set; }
        //  Width of track
        public float width { get; set; }

        //  DEBUG - Show model or not
        public bool ShowFloor { get; set; }

        public TrackSection(GraphicsDevice gd, GraphicsDeviceManager gdm
            , string fileName = "", ContentManager content = null)
            : base(gd, gdm, fileName, content)
        {
            Scale = 7.5f;
            length = dimensions.Z * Scale;
            width = dimensions.X * Scale;
        }

        public override void BuildCollisionModels()
        {
            AddHitSphere(Position + new Vector3(0, 0, length / 2), GetMaxDimensions(dimensions * Scale));
            CreateTriangles();
            UpdateTrianglesPositions();

            //  For each triangle
            for (int t = 0; t < VectorTrianglesList.Count; t++)
            {
                AddHitPlane(VectorTrianglesList[t][0], VectorTrianglesList[t][1], VectorTrianglesList[t][2]);
                AddHitBox(VectorTrianglesList[t][0], VectorTrianglesList[t][1], VectorTrianglesList[t][2]);
            }
        }

        public override void update(float dt)
        {
            base.update(dt);
        }

        public override void draw(GraphicsDevice graphicsDevice, Camera camera)
        { 
            // DEBUG
            /*
            for (int i = 0; i < HitBoxes.Count; i++)
            {
                DebugShapeRenderer.AddBoundingBox(HitBoxes[i], Color.Blue);
            }*/
            // DEBUG            

            base.draw(graphicsDevice, camera);

        }
    }
}
