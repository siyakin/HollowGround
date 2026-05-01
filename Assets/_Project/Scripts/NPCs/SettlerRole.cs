namespace HollowGround.NPCs
{
    public enum SettlerRole
    {
        None,
        Builder,
        Farmer,
        Miner,
        Woodcutter,
        WaterCarrier,
        Engineer,
        Medic,
        Guard,
        Researcher,
        Trader,
        Hauler
    }

    public static class SettlerRoleInfo
    {
        public static string GetDisplayName(SettlerRole role)
        {
            return role switch
            {
                SettlerRole.None => "Idle",
                SettlerRole.Builder => "Builder",
                SettlerRole.Farmer => "Farmer",
                SettlerRole.Miner => "Miner",
                SettlerRole.Woodcutter => "Woodcutter",
                SettlerRole.WaterCarrier => "Water Carrier",
                SettlerRole.Engineer => "Engineer",
                SettlerRole.Medic => "Medic",
                SettlerRole.Guard => "Guard",
                SettlerRole.Researcher => "Researcher",
                SettlerRole.Trader => "Trader",
                SettlerRole.Hauler => "Hauler",
                _ => role.ToString()
            };
        }
    }
}
