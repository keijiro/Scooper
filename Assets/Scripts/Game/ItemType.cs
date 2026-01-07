public enum ItemType { Bomb, Gem1, Gem2, Gem3, Gem4 }

public static class ItemUtils
{
    public static ItemType GetGemType(int index)
      => (ItemType)(1 + (index & 3));
}
