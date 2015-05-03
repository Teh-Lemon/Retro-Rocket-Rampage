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
    class Biome
    {
        // List of objects to randomly spawn
        List<VBGameObjectBasic> obstacleSelection;
        public List<Texture2D> scenerySelection;

        // Lists to contain objects spawned
        List<VBGameObjectBasic> activeObstacles;
        List<Sprite3D> activeScenery;

        public Color trackFillColor;
        public Color trackWireColor;
        public Color skyBoxColor;

        // Spawning frequencies, chance out of 100% to spawn at each track node
        int obstacleFrequency = 50;
        int pickupFrequency = 50;

        List<TrackPoint> trackPoints = null;
        Camera camera;

        public Biome(Camera cam, List<Texture2D> sceneryList, Color fillColor, Color wireColor,
            Color skyColor, int obstacleFreq = 50, int pickupFreq = 50)
        {

            scenerySelection = sceneryList;

            obstacleFrequency = obstacleFreq;
            pickupFrequency = pickupFreq;

            camera = cam;

            trackFillColor = fillColor;
            trackWireColor = wireColor;
            skyBoxColor = skyColor;
        }

        public void setTrackPoints(List<TrackPoint> pointlist)
        {
            trackPoints = pointlist;
        }
    }
}
