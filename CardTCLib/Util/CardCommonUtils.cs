namespace CardTCLib.Util;

public static class CardCommonUtils
{
    public static bool CheckTag(this CardTag? cardTag, string tag)
    {
        if (cardTag == null) return false;
        return cardTag.name == tag || cardTag.InGameName == tag || cardTag.InGameName.Chinese() == tag ||
               cardTag.InGameName.DefaultText == tag;
    }

    public static (CardTag? tag, int idx) FindTag(this CardData cardData, string tag)
    {
        for (var i = 0; i < cardData.CardTags.Length; i++)
        {
            var cardTag = cardData.CardTags[i];
            if (cardTag.CheckTag(tag))
            {
                return (cardTag, i);
            }
        }

        return (null, -1);
    }
}