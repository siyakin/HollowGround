using System;

namespace HollowGround.Domain.Production
{
    public static class ProductionCalc
    {
        public static float WorkerModifier(int assignedWorkers, int requiredWorkers, float dependency)
        {
            if (requiredWorkers == 0) return 1f;

            float ratio = Math.Clamp((float)assignedWorkers / requiredWorkers, 0f, 1f);
            return 1f - dependency * (1f - ratio);
        }

        public static float TotalProductionBonus(float baseBonus, int level)
        {
            return baseBonus * (level - 1) * 0.1f;
        }

        public static int ProductionAmount(int baseAmount, int level)
        {
            return baseAmount + (level - 1);
        }

        public static float ModifiedInterval(float baseInterval, float devMult, float productionBonus, float workerModifier)
        {
            float interval = baseInterval * devMult;
            if (productionBonus > 0f)
                interval *= (1f - productionBonus);
            if (workerModifier > 0.01f)
                interval /= workerModifier;
            return interval;
        }
    }
}
