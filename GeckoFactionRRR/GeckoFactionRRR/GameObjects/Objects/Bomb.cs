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
    class Bomb : VBGameObjectBasic
    {
        // Notifier that the bomb gas exploded, for particle effect
        public bool HasExploded = false;
        // Notifier that the bomb is exploding, for physics/reset
        public bool IsExploding = false;
        // Notifier that the bomb has finished exploding, for bomb reset
        public bool FinishedExploding = false;
        const float ExplodedTimer = 3f;
        public Player parentPlayer;
        // Current total time
        float TotalDT = 0f;
        // BombTimer
        float BombDT = 0f;
        // Time for Bomb to explode
        float BombTimer = 10f;
        // Bomb explosion notification 1
        const float TimerAlert = 4f;
        // Bomb explosion notification 2
        const float TimerDanger = 8f;
        // Points removed off of exploded player
        const int EXPLODED_SCORE_DECREMENT = 500;
        //dontevenknow
        float scalar = 1f;
        // dt scalar for sin calc
        float timescale = 10f;
        Vector3 positionOffset = new Vector3(4f, 40f, 0f);
        public List<Player> playerList;
        // Change colour switch
        bool ColourChangeB = false;

        public Bomb(GraphicsDevice gd, GraphicsDeviceManager gdm/*, Car _parentCar*/
            , string fileName = "Content/Models/bomb.txt", ContentManager content = null)
            : base(gd, gdm, /*_parentCar,*/ fileName, content)
        {
            Position = new Vector3(0, 10000, 2000);
            SetPosition(Position);
            Scale = 10f;//5f;
            scalar = Scale;
            BuildCollisionModels();
            ChangeColor(Color.Black, Color.WhiteSmoke);
        }

        public override void initialize()
        {
            base.initialize();

            Position = new Vector3(0, 10000, 2000); //new Vector3(0, -1000, 0);
            Scale = 10f;//5f;
            scalar = Scale;
            SetPosition(Position);
            BuildCollisionModels();
            parentPlayer = null;
            ChangeColor(Color.Black, Color.WhiteSmoke);
            HasExploded = false;
            IsExploding = false;
            FinishedExploding = false;
            TotalDT = 0;
        }

        public void SetParent(Player player/*Car car*/)
        {
            // Remove flag from old car
            if (parentPlayer != null)
            {
                parentPlayer.myCar.hasBomb = false;
            }
            // Set new player
            parentPlayer = player;
            //parentCar = player.car;
            player.myCar.hasBomb = true;
        }

        public void Explode()
        {
            HasExploded = true;
            IsExploding = true;
            // Adjust collision scalar for explosion
            scalar = 55f;
            Position = parentPlayer.myCar.Position;
            // Rebuild collision model for explosions
            Scale = 10;
            //SetPosition(Position);
            BuildCollisionModels();
            SetPosition(Position);
            if (parentPlayer != null)
            {
                parentPlayer.score -= EXPLODED_SCORE_DECREMENT;
                parentPlayer.myCar.hasBomb = false;
                parentPlayer.IsAlive = false;
                parentPlayer.myCar.HasDied = true;
                parentPlayer.myCar.Die(true);
            }
            IsAlive = false;

            parentPlayer = null;
            timescale = 10f;

            // Reset timers after explosion
            TotalDT = 0f;
            BombDT = 0f;
        }

        public bool CarTrackCollision(Player collidingPlayer/*Car collidingCar*/)
        {
            //  First pass
            //  If other car is nearby
            if (Vector3.Distance(Position, collidingPlayer.myCar.Position) < 1000)
            {
                if (HitSpheres[0].Intersects(collidingPlayer.myCar.HitSpheres[0]))
                {
                    //  Sets collided car
                    //parentCar = collidingPlayer.car;
                    SetParent(collidingPlayer);
                    return true;
                }
            }
            return false;
        }

        public bool PlayerToPlayerCollision(Player collidingPlayer)
        {

            if (Vector3.Distance(parentPlayer.myCar.Position, collidingPlayer.myCar.Position) < 1000)
            {
                if (parentPlayer.myCar.HitSpheres[0].Intersects(collidingPlayer.myCar.HitSpheres[0]))
                {
                    //  Sets collided car
                    //parentCar = collidingPlayer.car;
                    SetParent(collidingPlayer);
                    return true;
                }
            }

            return false;
        }

        public void ExplosionCollision(Player collidingPlayer)
        {
            //If colliding, create force according to relative position from explosion centre
            if (Vector3.Distance(Position, collidingPlayer.myCar.Position) < 1000)
            {
                if (HitSpheres[0].Intersects(collidingPlayer.myCar.HitSpheres[0]))
                {
                    // Do explosion physics
                    float distance = Vector3.Distance(HitSpheres[0].Center, collidingPlayer.myCar.Position);
                    Vector3 direction = HitSpheres[0].Center - collidingPlayer.myCar.Position;
                    direction.Normalize();
                    direction += new Vector3(0, -0.5f, 0);
                    direction *= distance * 2000;// 2500;

                    direction *= (1.5f - (collidingPlayer.myCar.weight/200));
                    
                    direction /= 250;
                    if (collidingPlayer.myCar.currentBoost >=20)
                    {
                        direction *= collidingPlayer.myCar.currentBoost;
                    }
                    else
                    {
                        direction *= 20;
                    }

                    collidingPlayer.myCar.Acceleration -= direction;
                }
            }
        }

        public override void update(float dt)
        {
            //base.update(dt);

            //SetPosition(Position);
            if (HasExploded)
            {
                HasExploded = false;
            }

            if (IsAlive)
            {
                if (parentPlayer == null)
                {
                    for (int i = 0; i < playerList.Count; i++)
                    {
                        CarTrackCollision(playerList[i]);
                    }
                }

                if (parentPlayer != null)
                {
                    TotalDT += dt;
                    // Time variable for sin calc
                    BombDT += (timescale * dt);
                    //reset timescale
                    //timescale = 10f;

                    Scale =  8 + ((float)Math.Sin(BombDT));

                    if (TotalDT >= TimerAlert && TotalDT < TimerDanger && !ColourChangeB)
                    {
                        ChangeColor(Color.Maroon, Color.WhiteSmoke);
                        timescale = 20f;
                        ColourChangeB = true;
                    }
                    else if (TotalDT >= TimerDanger && TotalDT < BombTimer && ColourChangeB)
                    {
                        ChangeColor(Color.Red, Color.WhiteSmoke);
                        timescale = 40f;
                        ColourChangeB = false;
                    }
                    else if (TotalDT >= BombTimer)
                    {
                        Explode();
                    }
                } 
            }

            if (IsExploding)
            {
                for (int i = 0; i < playerList.Count; i++)
                {
                    if (playerList[i].myCar.IsAlive)
                    {
                        ExplosionCollision(playerList[i]);
                    }

                }

                TotalDT += dt;
                if (TotalDT >= ExplodedTimer)
                {
                    TotalDT = 0;
                    IsExploding = false;
                    FinishedExploding = true;
                }
            }

            base.update(dt);

            SetPosition(Position);
        }

        public override void draw(GraphicsDevice graphicsDevice, Camera cam)
        {
            if (IsAlive)
            {
                base.draw(graphicsDevice, cam);     

                graphicsDevice.BlendState = BlendState.Opaque;
                graphicsDevice.RasterizerState = RasterizerState.CullNone;

                if (parentPlayer != null)
                {
                    Position = parentPlayer.myCar.Position + positionOffset;
                }
            }
        }

        public override void BuildCollisionModels()
        {
            if (HitSpheres.Count == 0)
            {
                AddHitSphere(Position + (centerPoint * Scale/*centre not scaled, need to add to base loading*/), GetMaxDimensions(dimensions) * scalar);
            }
            else
            {
                HitSpheres[0] =
                    new BoundingSphere(Position + (centerPoint * Scale), GetMaxDimensions(dimensions) / 2 * scalar);
            }
        }
    }
}
