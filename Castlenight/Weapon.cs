using System;

namespace Castlenight
{
    public class Weapon
    {
        private int ammo;
        private int range;
        private int power;

        public int Ammo { get => ammo; }
        public int Range { get => range; }
        public int Power { get => power; }


        public Weapon(int ammo, int range, int power)
        {
            this.ammo = ammo;
            this.range = range;
            this.power = power;
        }

        public int Shoot(Character target)
        {
            if(ammo <= 0) 
                throw new Exception("No more ammo");
            --ammo;
            return target.TakeDamage(power);
        }
    }
}