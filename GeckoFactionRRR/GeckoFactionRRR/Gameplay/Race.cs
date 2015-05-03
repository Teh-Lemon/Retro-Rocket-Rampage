using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GeckoFactionRRR
{
    class Race
    {
        public const int RACE_LEADER_BONUS_SCORE = 2000;

        public int ID { get; set; }

        // Winning player ID
        public Player Winner { get; set; }

        BoundingBox finishLineBox;
        TrackManager track;
        Sprite3D finishLineSprite;

        List<Player> potentialWinners = new List<Player>();

        Game game;
        GraphicsDeviceManager graphics;

        public Race(Game newGame, TrackManager newTrack, GraphicsDevice gd, GraphicsDeviceManager gdm, int id)
        {
            Winner = null;
            track = newTrack;
            graphics = gdm;
            game = newGame;
            ID = id;

            float finishLineSpriteScale = track.TrackWidth / 267.5f;
            finishLineSprite = new Sprite3D(track.EndPoint.Position, game.Content.Load<Texture2D>("Materials/FinishLine")
                , game.Content, gd, graphics, false, false, finishLineSpriteScale);
            
            BuildCollisionModels();
        }

        void BuildCollisionModels()
        {
            Vector3 finishLineMin = Vector3.Zero;
            Vector3 finishLineMax = Vector3.Zero;

            Vector3 endPointPosition = track.EndPoint.Position;

            float width = track.TrackWidth;   
            float newHeight = width / 4;

            finishLineSprite.Position += new Vector3(0, newHeight, 0);

            finishLineMin = endPointPosition + new Vector3(-width, -width, 0.0f);
            finishLineMax = endPointPosition + new Vector3(width, width, width);

            finishLineBox = new BoundingBox(finishLineMin, finishLineMax);
        }

        public void update(float dt)
        {
            finishLineSprite.update(dt);
        }

        // Find all the players that have past the finish line
        // Returns whether someone has past the finish line
        public bool CheckCollisions(Player player)
        {
            Car car = player.myCar;
            if (car.HitSpheres[0].Intersects(finishLineBox))
            {
                if (!potentialWinners.Contains(player))
                {
                    potentialWinners.Add(player);
                }
            }

            if (potentialWinners.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Check the potential winners for the car furthest ahead
        // Return the winner
        public Player CheckWinners()
        {
            Player furthestPlayer = potentialWinners[0];

            for (int i = 1; i < potentialWinners.Count; i++)
            {
                Player newPlayer = potentialWinners[i];
                if (newPlayer.myCar.Position.Z > furthestPlayer.myCar.Position.Z)
                {
                    furthestPlayer = newPlayer;
                }
            }

            furthestPlayer.score += RACE_LEADER_BONUS_SCORE;

            Winner = furthestPlayer;

            return furthestPlayer;
        }

        public void draw(GraphicsDevice graphicsDevice, Camera camera)
        {
            DebugShapeRenderer.AddBoundingBox(finishLineBox, Color.Blue);

            finishLineSprite.draw(graphicsDevice, camera);
        }



    }
}
