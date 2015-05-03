//  Created 21/10/2013
//  Manages the track
//  Holds the reference and position of each track section
//  Calls the track generator to generate new sections

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GeckoFactionRRR
{
    class TrackManager : VBGameObjectBasic
    {
        //Define length of track - in track pieces
		public const int TRACK_LENGTH = 75;
        //public const int TRACK_LENGTH = 11;
        // Length of the straight road at the start/end of racetrack in sections
        public const int ROLLING_LENGTH = 5;
        string[] sectionModels = new string[4];
        // Terrain types
        const int TR_BASIC = 0;
        const int TR_BOUNCY = 1;
        const int TR_BOSS = 2;
        bool newTerrain = true;

        int genTimer = 0;

        public Camera camera;

        Random rand = new Random();

        // Cars die below this level
        public const int KILL_RANGE = 250;

        TrackCurve track;

        Game game;

        GraphicsDevice graphicsDevice;
        GraphicsDeviceManager graphics;

        // Colors
        Color fillColor = new Color(0, 25, 0);
        Color wireColor = new Color(0, 255, 0);
        // Target colours for terrain colour swapping
        Color fillColorTarget;
        Color wireColorTarget;
        // Don't update track colour if it hasn't changed
        bool ChangedColor;

        public List<Player> PlayerList;

        Texture2D lineDot;

        // Track Biome
        public Biome CurrentBiome { get; set; }
        public Biome defaultBiome;

        bool biomeTimerOn = false;
        float currentBiomeTime = 0f;
        const float BIOME_TIMER = 20f;

        int oldestSceneryIndex = 0;

        // Hold/update environment sprites
        List<Sprite3D> NormalTerrainSprites = new List<Sprite3D>();

        //  DEBUG        
        public bool ShowFloor { get; set; }

        //  Constructor
        public TrackManager(Biome startBiome, Game thisGame, GraphicsDevice gd, GraphicsDeviceManager gdm, string fileName = "", ContentManager content = null)
            : base(gd, gdm, fileName, content)
        {
            graphicsDevice = gd;
            graphics = gdm;
              
            // Track
            track = new TrackCurve(graphicsDevice, graphics);

            ShowFloor = true;

            ChangedColor = false;

            game = thisGame;

            defaultBiome = startBiome;

            setBiome(startBiome);

            lineDot = new Texture2D(gd, 1, 1);
            lineDot.SetData<Color>(new Color[] { Color.White });

            initialize();
        }

        public void setBiome(Biome newBiome)
        {
            CurrentBiome = newBiome;
            CurrentBiome.setTrackPoints(TrackPoints);

            fillColorTarget = CurrentBiome.trackFillColor;
            wireColorTarget = CurrentBiome.trackWireColor;

            foreach (Sprite3D sprite in NormalTerrainSprites)
            {
                sprite.texture = CurrentBiome.scenerySelection[GlobalRandom.Next(1, CurrentBiome.scenerySelection.Count + 1) - 1];
            }
            
            newTerrain = true;
            biomeTimerOn = true;
            currentBiomeTime = 0f;
        }

        // Set target colours to random
        public void changeColorRandom()
        {
            fillColor = new Color(GlobalRandom.Next(0, 255), GlobalRandom.Next(0, 255), GlobalRandom.Next(0, 255));
            wireColor = new Color(GlobalRandom.Next(0, 255), GlobalRandom.Next(0, 255), GlobalRandom.Next(0, 255));
            fillColorTarget = new Color(GlobalRandom.Next(0, 255), GlobalRandom.Next(0, 255), GlobalRandom.Next(0, 255));
            wireColorTarget = new Color(GlobalRandom.Next(0, 255), GlobalRandom.Next(0, 255), GlobalRandom.Next(0, 255));
        }

        public void SetTrackColor(Color fill, Color wire)
        {
            track.ChangeColor(fill, wire);
        }

        // Specify target colours
        public void setColourTargets(Color fill, Color wire)
        {
            fillColorTarget = fill;
            wireColorTarget = wire;
        }

        public void initialize(int raceNumber)
        {
            fillColor = new Color(0, 25, 0);
            wireColor = new Color(0, 255, 0);
            fillColorTarget = fillColor;
            wireColorTarget = wireColor;

            setBiome(defaultBiome);

            GenerateNewTrack(raceNumber);        
        }

        #region Properties
        // List of all the points the track follows
        public List<TrackPoint> TrackPoints
        {
            get { return track.TrackPoints; }
        }

        public TrackPoint StartPoint
        {
            get { return track.StartPoint; }
        }

        public TrackPoint EndPoint
        {
            get
            {
                return track.EndPoint;
            }
        }

        // Width of the track including ramps
        public float TrackWidth
        {
            get { return track.TrackWidth; }
        }

        // The most recently past checkpoint
        public CheckPoint CurrentCheckPoint
        {
            get
            {
                return track.CurrentCheckPoint;
            }
        }

        // Find the next checkpoint
        public CheckPoint NextCheckPoint
        {
            get
            {
                return track.NextCheckPoint;
            }
        }

        // Get nearest track centerpoint to input vector
        public TrackPoint getNearestNode(Vector3 inPos, int pointOffset = 0)
        {
            int selected = 0;

            for (int i = 0; i < TrackPoints.Count; i++)
            {
                if (Vector3.Distance(inPos, TrackPoints[i].Position) < Vector3.Distance(inPos, TrackPoints[selected].Position))
                {
                    selected = i;
                }
                else if (selected > 0)
                {
                    return TrackPoints[selected + pointOffset];
                }
            }

            return TrackPoints[selected + pointOffset];
        }

        public int getNearestIndex(Vector3 inPos, int pointOffset = 0)
        {
            int selected = 0;

            for (int i = 0; i < TrackPoints.Count; i++)
            {
                if (Vector3.Distance(inPos, TrackPoints[i].Position) < Vector3.Distance(inPos, TrackPoints[selected].Position))
                {
                    selected = i;
                }
                else if (selected > 0)
                {
                    return selected;
                }
            }

            return selected;
        }

        public int getPreviousIndex(Vector3 inPos)
        {
            int selected = 0;

            for (int i = 0; i < TrackPoints.Count; i++)
            {
                if (TrackPoints[i].Position.Z < inPos.Z)
                {
                    selected = i;
                }
                else if (selected > 0)
                {
                    return selected;
                }
            }
            return selected;
        }
        #endregion

        //  Deletes old and makes a new track (DEPRECATED)
        public void GenerateNewTrack(int raceNumber)
        {
            //  Remove old track
            NormalTerrainSprites.Clear();
            oldestSceneryIndex = 0;
            newTerrain = true;

            if (raceNumber == 2)
            {
                track.GenerateNewTrack(TrackCurve.Map.Final_Destination);
            }
            else
            {
                track.GenerateNewTrack(TrackCurve.Map.Normal);
            }

            genTimer = 0;
        }

        // Test for whether the selected node has scenery attatched
        public void tryAddScenery(int index)
        {
            int lastPoint = Math.Min(TRACK_LENGTH, index + 20);

            for (int i = index; i < lastPoint; i++)
            {
                if (!TrackPoints[i].hasScenery)
                {
                    addSceneryObject(TrackPoints[i].GetOutsidePoints(1000)[0]);
                    addSceneryObject(TrackPoints[i].GetOutsidePoints(1000)[1]);

                    TrackPoints[i].hasScenery = true;
                }
            }
        }

        // Create or replace a scenery object at the given position
        public void addSceneryObject(Vector3 pos)
        {
            pos = new Vector3(pos.X, GlobalRandom.Next(0, 300), pos.Z);

            if (NormalTerrainSprites.Count < 40)
            {
                NormalTerrainSprites.Add(new Sprite3D(pos, CurrentBiome.scenerySelection[GlobalRandom.Next(1, CurrentBiome.scenerySelection.Count + 1) - 1],
                    game.Content, graphicsDevice, graphics, true, true, 10, null));
            }
            else
            {
                NormalTerrainSprites[oldestSceneryIndex].Position = pos;
                NormalTerrainSprites[oldestSceneryIndex].texture = CurrentBiome.scenerySelection[GlobalRandom.Next(1, CurrentBiome.scenerySelection.Count + 1) - 1];

                if (oldestSceneryIndex < 39) oldestSceneryIndex++;
                else oldestSceneryIndex = 0;


            }
        }

        //  Checks whether a car is over any track piece
        //  Returns that track piece if one is found
        //  Return null if not over any track piece
        public void CheckCarCollisions(Car car)
        {
            if (GameState.Current == GameState.States.PLAYING)
            {
                // Check if car falls below the track
                if (CheckCarKillY(car))
                {
                    return;
                }
            }

            // Check if car is driving on the track
            track.CarCollisionBroad(car);
        }

        public void TrackColourCheck()
        {
            if (fillColor.R < fillColorTarget.R)
            {
                fillColor.R++;
                ChangedColor = true;
            }
            else if (fillColor.R > fillColorTarget.R)
            {
                fillColor.R--;
                ChangedColor = true;
            }

            if (fillColor.G < fillColorTarget.G)
            {
                fillColor.G++;
                ChangedColor = true;
            }
            else if (fillColor.G > fillColorTarget.G)
            {
                fillColor.G--;
                ChangedColor = true;
            }

            if (fillColor.B < fillColorTarget.B)
            {
                fillColor.B++;
                ChangedColor = true;
            }
            else if (fillColor.B > fillColorTarget.B)
            {
                fillColor.B--;
                ChangedColor = true;
            }

            if (wireColor.R < wireColorTarget.R)
            {
                wireColor.R++;
                ChangedColor = true;
            }
            else if (wireColor.R > wireColorTarget.R)
            {
                wireColor.R--;
                ChangedColor = true;
            }

            if (wireColor.G < wireColorTarget.G)
            {
                wireColor.G++;
                ChangedColor = true;
            }
            else if (wireColor.G > wireColorTarget.G)
            {
                wireColor.G--;
                ChangedColor = true;
            }

            if (wireColor.B < wireColorTarget.B)
            {
                wireColor.B++;
                ChangedColor = true;
            }
            else if (wireColor.B > wireColorTarget.B)
            {
                wireColor.B--;
                ChangedColor = true;
            }
        }

        // Change terrain type
        public void RandomiseTerrain()
        {
            int trType = GlobalRandom.Next(3);

            switch (trType)
            {
                    // Cases likely to need to call seperate functions in the future
                case TR_BASIC:
                    changeColorRandom();
                    for (int i = 0; i < PlayerList.Count; i++)
                    {
                        PlayerList[i].myCar.IsBouncy = false;
                    }
                    break;

                case TR_BOUNCY:
                    setColourTargets(Color.Pink, Color.White);
                    for (int i = 0; i < PlayerList.Count; i++)
                    {
                        PlayerList[i].myCar.IsBouncy = true;
                    }
                    break;

                case TR_BOSS:
                    setColourTargets(Color.BurlyWood, Color.Brown);
                    for (int i = 0; i < PlayerList.Count; i++)
                    {
                        PlayerList[i].myCar.IsBouncy = false;
                    }
                    break;

                default:
                    break;
            }
        }

        public override void update(float dt)
        {
            // Track Curve

            if (TrackPoints.Count == 0)
            {
                newTerrain = true;
            }
            else if (newTerrain)
            {
                setBiome(CurrentBiome);
                //SetTrackColor(biome.trackFillColor, biome.trackWireColor);
                newTerrain = false;
            }

            if (track.HasModel)
            {
                track.update(dt);

                TrackColourCheck();

                if (ChangedColor)
                {

                    track.ChangeColor(fillColor, wireColor);
                    ChangedColor = false;
                }

                if (Math.Abs(camera.target.Z -
                    getNearestNode(new Vector3(camera.target.X, 0, camera.target.Z)).Position.Z) < 50)
                {
                    tryAddScenery(getNearestIndex(new Vector3(camera.target.X, 0, camera.target.Z)));
                }

                track.WireColor = wireColor;
                track.FillColor = fillColor;
            }


            if (biomeTimerOn)
            {
                currentBiomeTime += dt;

                if (currentBiomeTime >= BIOME_TIMER)
                {
                    setBiome(defaultBiome);
                    biomeTimerOn = false;
                    currentBiomeTime = 0f;

                    for (int i = 0; i < PlayerList.Count; i++)
                    {
                        PlayerList[i].myCar.IsBouncy = false;
                        PlayerList[i].myCar.ConstantFullBoost = false;
                    }
                }
            }
        }


        public void drawCurve(SpriteBatch spritebatch, Vector2 origin, float scale = 1)
        {
            Vector2 startPoint = new Vector2();
            Vector2 endPoint = new Vector2();

            if (genTimer < TrackPoints.Count - 2)
            {
                genTimer++;
            }

            for (int i = 0; i < genTimer; i++)
            {
                startPoint = origin + 
                    (new Vector2(TrackPoints[i].Position.Z, -TrackPoints[i].Position.X) * scale);
                endPoint = origin +
                    (new Vector2(TrackPoints[i + 1].Position.Z, -TrackPoints[i + 1].Position.X) * scale);

                drawLine(spritebatch, startPoint, endPoint);
            }
        }

        public void drawLine(SpriteBatch spritebatch, Vector2 startPoint, Vector2 endPoint)
        {
            Vector2 edge = endPoint - startPoint;

            float lineAngle = (float)Math.Atan2(edge.Y, edge.X);

            spritebatch.Draw(lineDot, new Rectangle(
                (int)startPoint.X, (int)startPoint.Y, (int)edge.Length(), 3),
                null, Color.White, lineAngle, Vector2.Zero, SpriteEffects.None, 0);
        }

        public override void draw(GraphicsDevice graphicsDevice, Camera camera)
        {
            // Track Curve
            track.draw(graphicsDevice, camera);
        }

        public void drawSprites(GraphicsDevice graphicsDevice, Camera camera)
        {
            foreach (Sprite3D sprite in NormalTerrainSprites)
            {
                if (Vector3.Distance(sprite.Position, camera.Position) < camera.farPlaneDistance)
                {
                    sprite.draw(graphicsDevice, camera);
                }
            }

        }

        // Checks to see if the car has fallen below the track
        bool CheckCarKillY(Car car)
        {
            if (!car.HitSpheres[0].Intersects(track.KillBox))
            {
                car.Die(true);
                return true;
            }
            else
            {
                return false;
            }            
        }

        // Turn all check points back on
        public void TurnOnCheckPoints()
        {
            for (int i = 0; i < TrackPoints.Count; i++)
            {
                TrackPoints[i].CheckPoint.IsAlive = true;
            }
        }

        public void GenerateTrackModel()
        {
            track.GenerateModel();
        }
    }
}
