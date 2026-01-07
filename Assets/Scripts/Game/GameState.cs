public static class GameState
{
    public static int GemSpawnCount { get; set; }

    public static ItemType GetNextGemType()
    {
        var type = ItemUtils.GetGemType(GemSpawnCount % GemVariationCount);
        GemSpawnCount++;
        return type;
    }

    public static int TraySpawnCount { get; set; }

    public static ItemType GetNextTrayItemType()
    {
        var type = ItemUtils.GetGemType(TraySpawnCount % GemVariationCount);
        TraySpawnCount++;
        return type;
    }

    public static int GemVariationCount { get; set; } = 4;

    public static int ReloadCount { get; set; }

    public static ItemController DetectedItem { get; set; }

    public static ItemController DetonatedBomb { get; set; }
}
