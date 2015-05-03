//
//  Modifier code adapted from work by Jason Mitchell
//
//  Available at: http://jason-mitchell.com/game-development/3d-particle-system-for-xna/
//

using Microsoft.Xna.Framework;

namespace GeckoFactionRRR
{
    class FrictionModifier : Modifier
    {
        private readonly float friction;

        public FrictionModifier()
        {
            friction = 1f;
        }

        public FrictionModifier(float frictionCoefficient)
        {
            friction = MathHelper.Clamp((1 - frictionCoefficient), 0, 1);
        }

        public override void Update(Particle particle, float particleAge)
        {
            particle.Velocity *= friction;
        }
    }
}
