using System;
using HollowGround.Core;
using HollowGround.World;

namespace HollowGround.Combat
{
    /// <summary>
    /// Thin event hub for battle reports. Expedition lifecycle is managed by ExpeditionSystem.
    /// UI panels subscribe here to display battle results.
    /// </summary>
    public class BattleManager : Singleton<BattleManager>
    {
        public event Action<BattleReport> OnBattleCompleted;

        public void PublishBattleReport(BattleReport report)
        {
            OnBattleCompleted?.Invoke(report);
        }
    }
}
