using System.Collections.Generic;
using HollowGround.Army;
using HollowGround.Heroes;

namespace HollowGround.Domain.Walkers
{
    public class PatrolScheduler
    {
        public const int MaxTroopWalkers = 5;
        public const float PatrolMoveSpeed = 1.8f;
        public const float SpawnCheckInterval = 15f;

        public int GetDesiredHeroWalkerCount(List<Hero> availableHeroes)
        {
            if (availableHeroes == null) return 0;

            int count = 0;
            foreach (var hero in availableHeroes)
                if (!hero.IsDeployed && !hero.IsInjured)
                    count++;

            return count;
        }

        public int GetDesiredTroopWalkerCount(int totalTroops)
        {
            if (totalTroops <= 0) return 0;
            return ClampTroopCount(totalTroops / 10);
        }

        public int ClampTroopCount(int raw)
        {
            if (raw < 0) return 0;
            if (raw > MaxTroopWalkers) return MaxTroopWalkers;
            return raw;
        }

        public bool ShouldSpawn(int current, int desired)
        {
            return current < desired;
        }

        public bool ShouldDespawn(int current, int desired)
        {
            return current > desired;
        }
    }
}
