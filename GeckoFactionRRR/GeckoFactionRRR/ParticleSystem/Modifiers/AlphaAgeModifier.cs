//
//  Modifier code adapted from work by Jason Mitchell
//
//  Available at: http://jason-mitchell.com/game-development/3d-particle-system-for-xna/
//

using Microsoft.Xna.Framework;

namespace GeckoFactionRRR
{
    class AlphaAgeModifier : Modifier
    {
        public override void Update(Particle particle, float particleAge)
        {
            particle.Alpha = MathHelper.Lerp(1, 0, particleAge);
        }
    }
}