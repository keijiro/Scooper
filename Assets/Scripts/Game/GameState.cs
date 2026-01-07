public static class GameState
{
    public static int ItemSpawnCount { get; set; }
    public static int GemVariationCount { get; set; } = 4;
    public static ItemController DetectedItem { get; set; }
    public static ItemController DetonatedBomb { get; set; }
}
