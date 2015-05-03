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
    class BasePowerUp : VBGameObjectBasic
    {
        public Biome City;
        public Biome Sun;
        public Biome Bouncy;
        public Biome Rapture;

        public int type { get; set; }

        // Holds if power up is currently "on"
        public bool IsActive { get; set; }

        // Sprite for the Powerup
        public Sprite3D SpriteIdentifier;

        public float SPRITE_SCALE = 0.9f;
        float YPositionOffset;
        //protected Random rand = new Random();
        //  DT used in Sin calculation
        protected float TotalDT = 0f;
        //  Total running Gametime for particles
        public float TotalGameTime = 0f;
        //  How long the powerups has been active
        protected float ActiveDT = 0f;
        //  Length of time powerup is active - set as required in each Powerup class
        protected float PowerupLifetime = 10;

        protected List<Player> playerList;
        //  Holds which car picked up the power up
        protected Car PoweredUpCar;

        //  Particle Effect
        ExplosionEmitter pEmit;

        public BasePowerUp(List<Player> pList, GraphicsDevice gd, GraphicsDeviceManager gdm,
            string fileName = "", ContentManager content = null)
            : base(gd, gdm, fileName, content)
        {

            playerList = pList;
            IsAlive = true;
            type = 0;
            Scale = 200f;
            HasFillModel = false;
            BuildCollisionModels();

            pEmit = new ExplosionEmitter(10, 200, 5, 3.5f, 0.1f);
            pEmit.Modifiers.Add(new AlphaAgeModifier());
            InitialiseEmitter(content, gd);
        }

        public void InitialiseEmitter(ContentManager content, GraphicsDevice gd)
        {
            List<Texture2D> textures = new List<Texture2D> { content.Load<Texture2D>("Materials/Particles/RRRStreak") };
            pEmit.LoadContent(textures, gd);
        }

        //  Method to call when Powerup starts
        public virtual void ActivatePower()
        {
            IsActive = true;
            IsAlive = false;
            return;
        }
        // Polymorphism!!
        public virtual void ActivatePower(List<Player> playerList)
        {
            return;
        }

        //  Method to call when Powerup ends
        public virtual void DeactivatePower()
        {
            IsActive = false;
            return;
        }


        public override void BuildCollisionModels()
        {
            float diameter = Scale * dimensions.Y;

            if (HitSpheres.Count == 0)
            {
                AddHitSphere(Position, diameter);
            }
            // If there is already a bounding sphere, replace it
            else
            {
                HitSpheres[0] =
                    new BoundingSphere(Position, diameter / 2);
            }
        }

        public bool carCollision(Car collidingCar)
        {
            //  First pass
            //  If other car is nearby
            if (Vector3.Distance(Position, collidingCar.Position) < 1000)
            {
                if (HitSpheres[0].Intersects(collidingCar.HitSpheres[0]))
                {
                    // 'Kills' powerup immediately to limit drawing and collision checks
                    IsAlive = false;
                    //  Sets collided car
                    PoweredUpCar = collidingCar;
                    return true;
                }
            }

            return false;
        }

        public override void update(float dt)
        {
            base.update(dt);

            TotalDT += (8 * dt);

            // Update Powerup postion
            // Something like:
            YPositionOffset = ((float)Math.Sin(TotalDT)) / 2;
            Vector3 newPos = new Vector3(0, YPositionOffset, 0);
            SetPosition(Position + newPos);
            //Spin Container
            Yaw += 4.5f * dt;

            // Sync Powerup elements to current position
            SpriteIdentifier.Position = Position;
            SpriteIdentifier.update(dt);

            if (IsActive)
            {
                ActiveDT += dt;
            }

            if (IsAlive)
            {
                pEmit.Emit(TotalGameTime, Position);
                pEmit.Update(TotalGameTime);
            }
            //  Check for collisions between powerup and the players
            for (int i = 0; i < playerList.Count; i++)
            {
                Car car = playerList[i].myCar;

                //  Activate power if colliding
                if (IsAlive && carCollision(car))
                {
                    ActivatePower();
                }
            }

            //  Check to see if the power up is on, and turn it off when required
            if (IsActive)
            {
                //  Deactivate Powerup after set time
                if (ActiveDT >= PowerupLifetime)
                {
                    DeactivatePower();
                }
            }
        }

        public override void draw(GraphicsDevice graphicsDevice, Camera cam)
        {
            if (IsAlive)
            {
                DebugShapeRenderer.AddBoundingSphere(HitSpheres[0], Color.Yellow);
                
                base.draw(graphicsDevice, cam);

                graphicsDevice.BlendState = BlendState.Opaque;
                graphicsDevice.RasterizerState = RasterizerState.CullNone;
                pEmit.Draw(graphicsDevice, cam);

                SpriteIdentifier.draw(graphicsDevice, cam);                
            }
        }
    }
}
