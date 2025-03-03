namespace CardTCLib.Util;

public static class TCCardCheck
{
    public static bool IsTCWeapon(this InGameCardBase card)
    {
        return card.IsInventoryCard && card.CardModel.LegacyInventory && card.CardModel.HasFloatValue("TCLib.Weapon");
    }

    public static bool IsTCTool(this InGameCardBase card)
    {
        return card.IsInventoryCard && card.CardModel.LegacyInventory && card.CardModel.HasFloatValue("TCLib.Tool");
    }
}