using System.Linq;
using CardTCLib.Const;

namespace CardTCLib.Util;

public static class CardValueGetter
{
    public static bool HasFloatValue(this CardData cardData, string key)
    {
        return cardData.TimeValues.FirstOrDefault(objective => objective.ObjectiveName == key) != null;
    }

    public static float GetFloatValue(this InGameCardBase card, string key)
    {
        var baseValue = card.CardModel.TimeValues
            .FirstOrDefault(objective => objective.ObjectiveName == key)?.Value * 0.001f ?? 0.0f;
        if (card.CardModel.HasFloatValue(TCCommonAttrs.EffectScaleByUsage))
        {
            baseValue *= card.CurrentUsagePercent;
        }

        return baseValue;
    }

    public static CardTag? GetTagValue(this CardData cardData, string key)
    {
        return cardData.TagsOnBoard.FirstOrDefault(objective => objective.ObjectiveName == key)?.Tag;
    }

    public static float CollectFloatValue(this InGameCardBase card, string key)
    {
        var value = card.GetFloatValue(key);
        foreach (var inventorySlot in card.CardsInInventory)
        {
            if (inventorySlot.CardModel)
                value += inventorySlot.MainCard.GetFloatValue(key);
        }

        return value;
    }
}