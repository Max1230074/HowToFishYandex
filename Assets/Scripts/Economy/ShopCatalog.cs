public enum ItemKind
{
    WeaponMelee,
    WeaponGun,
    Upgrade,
    Consumable
}

public class ShopItem
{
    public string id;
    public string name;
    public string desc;
    public int cost;
    public ItemKind kind;
}

public static class ShopCatalog
{
    public static readonly ShopItem[] All =
    {
        new ShopItem { id = "knife", name = "Нож", cost = 30, desc = "Урон 35, ближний бой", kind = ItemKind.WeaponMelee },
        new ShopItem { id = "pistol", name = "Пистолет", cost = 120, desc = "Урон 45, дальний бой", kind = ItemKind.WeaponGun },
        new ShopItem { id = "rod2", name = "Удочка 2 ур.", cost = 80, desc = "Дальше заброс, сильнее вываживание", kind = ItemKind.Upgrade },
        new ShopItem { id = "medkit", name = "Аптечка", cost = 20, desc = "+50 HP", kind = ItemKind.Consumable }
    };
}