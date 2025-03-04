using UnityEngine;

namespace CardTCLib.Util;

public static class ActionCopyTool
{
    public static CardAction CopyRaw(this CardAction cardAction)
    {
        if (cardAction is CardOnCardAction)
        {
            return JsonUtility.FromJson<CardOnCardAction>(JsonUtility.ToJson(cardAction));
        }

        if (cardAction is DismantleCardAction)
        {
            return JsonUtility.FromJson<DismantleCardAction>(JsonUtility.ToJson(cardAction));
        }

        if (cardAction is FromStatChangeAction)
        {
            return JsonUtility.FromJson<FromStatChangeAction>(JsonUtility.ToJson(cardAction));
        }

        return JsonUtility.FromJson<CardAction>(JsonUtility.ToJson(cardAction));
    }
}