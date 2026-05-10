using System.Collections.Generic;
using HollowGround.Army;
using HollowGround.Heroes;

namespace HollowGround.Domain.Walkers
{
    public class PatrolScheduler
    {
        public const int MaxTroopWalkers = 5;
        public const float PatrolMoveSpeed = 1.8f;
        public const float SpawnCheckInterval = 10f;
        public const int TroopsPerWalker = 3;

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
            int raw = totalTroops / TroopsPerWalker;
            if (raw < 1) raw = totalTroops > 0 ? 1 : 0;
            return ClampTroopCount(raw);
        }

        public int GetPopulationBudget(int population, int settlerCount)
        {
            return population - settlerCount;
        }

        public int ClampTroopCount(int raw)
        {
            if (raw < 0) return 0;
            if (raw > MaxTroopWalkers) return MaxTroopWalkers;
            return raw;
        }

        public int ApplyPopulationCap(int desired, int populationBudget)
        {
            if (populationBudget <= 0) return 0;
            return desired > populationBudget ? populationBudget : desired;
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
