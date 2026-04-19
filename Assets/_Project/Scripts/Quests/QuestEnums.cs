namespace HollowGround.Quests
{
    public enum QuestType
    {
        Main,
        Side,
        Daily,
        Repeatable
    }

    public enum QuestStatus
    {
        Locked,
        Available,
        Active,
        Completed,
        TurnedIn
    }

    public enum ObjectiveType
    {
        BuildBuilding,
        GatherResource,
        KillMutants,
        TrainTroops,
        ResearchTech,
        ReachLevel,
        ExploreNodes,
        WinBattles,
        TradeWithFaction,
        SurviveWaves
    }
}
