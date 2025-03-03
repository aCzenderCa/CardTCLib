using System.Linq;

namespace CardTCLib.Util;

public static class CardValueGetter
{
    public static bool HasFloatValue(this CardData cardData, string key)
    {
        return cardData.TimeValues.FirstOrDefault(objective => objective.ObjectiveName == key) != null;
    }

    public static float GetFloatValue(this CardData cardData, string key)
    {
        return cardData.TimeValues.FirstOrDefault(objective => objective.ObjectiveName == key)?.Value * 0.001f ?? 0.0f;
    }

    public static CardTag? GetTagValue(this CardData cardData, string key)
    {
        return cardData.TagsOnBoard.FirstOrDefault(objective => objective.ObjectiveName == key)?.Tag;
    }

    public static float CollectFloatValue(this InGameCardBase card, string key)
    {
        var value = 0f;
        foreach (var inventorySlot in card.CardsInInventory)
        {
            if (inventorySlot.CardModel)
                value += inventorySlot.CardModel.GetFloatValue(key);
        }
        
        return value;
    }
}