using System;
using System.Collections;
using System.Linq;
using BepInEx;
using CardTCLib.Util;
using HarmonyLib;
using UnityEngine;

namespace CardTCLib.LuaBridge;

public class GameBridge
{
    public int MiniTicksPerTick => GameManager.Instance.DaySettings.MiniTicksPerTick;

    public UniqueIdObjectBridge? GetItem(string key)
    {
        var uniqueIDScriptable = UniqueIDScriptable.AllUniqueObjects.GetValueSafe(key);
        if (uniqueIDScriptable == null)
            uniqueIDScriptable = ModLoader.ModLoader.AllGUIDDict.GetValueSafe(key);

        if (uniqueIDScriptable == null)
        {
            var uniqueIdObjectBridge = new UniqueIdObjectBridge(null);
            uniqueIDScriptable = ModLoader.ModLoader.AllGUIDDict.Values.Concat(GameLoad.Instance.DataBase.AllData)
                .FirstOrDefault(uidScript =>
                {
                    if (uidScript.name == key) return true;
                    uniqueIdObjectBridge.UniqueIDScriptable = uidScript;
                    return uniqueIdObjectBridge.Name == key || uniqueIdObjectBridge.NameLocal == key ||
                           uniqueIdObjectBridge.NameChinese == key;
                });
        }

        if (uniqueIDScriptable == null) return null;

        return new UniqueIdObjectBridge(uniqueIDScriptable);
    }

    public CardActionBridge CreateAction(string id, string name, string type)
    {
        CardAction? action = null;
        switch (type)
        {
            case nameof(CardAction):
                action = new CardAction(new LocalizedString { DefaultText = name, ParentObjectID = id }, default);
                break;
            case nameof(CardOnCardAction):
                action = new CardOnCardAction(new LocalizedString { DefaultText = name, ParentObjectID = id }, default,
                    0);
                break;
            case nameof(DismantleCardAction):
                action = new DismantleCardAction();
                action.ActionName = new LocalizedString { DefaultText = name, ParentObjectID = id };
                break;
            case nameof(FromStatChangeAction):
                action = new FromStatChangeAction();
                action.ActionName = new LocalizedString { DefaultText = name, ParentObjectID = id };
                break;
        }

        return new CardActionBridge(action);
    }

    public UniqueIdObjectBridge CreateCard(string id, string name, string type, string? icon = null)
    {
        icon ??= "icon_" + id;
        var cardData = (CardData)ModLoader.LoaderUtil.ScriptableUtil.CreateInstance(typeof(CardData));
        cardData.UniqueID = id;
        cardData.name = id + "_" + name;
        cardData.CardName = new LocalizedString { DefaultText = name, LocalizationKey = cardData.name + "_name" };
        cardData.CardDescription = new LocalizedString { LocalizationKey = cardData.name + "_description" };
        cardData.SpillsInventoryOnDestroy = true;
        var traverse = Traverse.Create(cardData);
        foreach (var field in traverse.Fields())
        {
            var fieldTraverse = traverse.Field(field);
            if (fieldTraverse.GetValueType() == typeof(DurabilityStat))
            {
                fieldTraverse.SetValue(new DurabilityStat(false, 0));
            }
        }

        if (Enum.TryParse<CardTypes>(type, out var cardType))
        {
            cardData.CardType = cardType;
        }

        GameLoad.Instance.DataBase.AllData.Add(cardData);
        UniqueIDScriptable.AllUniqueObjects[id] = cardData;
        ModLoader.ModLoader.AllGUIDDict[id] = cardData;
        ModLoader.ModLoader.AllGUIDTypeDict[typeof(CardData)][id] = cardData;
        cardData.Init();
        var uniqueIdObjectBridge = new UniqueIdObjectBridge(cardData);
        if (!icon.IsNullOrWhiteSpace())
        {
            uniqueIdObjectBridge.SetIcon(icon);
        }

        return uniqueIdObjectBridge;
    }

    public IEnumerator PassTimeEnum(int miniTick, InGameCardBridge? fromCard = null, bool blockable = true,
        string fadeType = nameof(FadeToBlackTypes.Partial), string fadeText = "")
    {
        GameManager.Instance.CurrentMiniTicks += miniTick;
        var passedTp = GameManager.Instance.CurrentMiniTicks / MiniTicksPerTick;
        GameManager.Instance.CurrentMiniTicks %= MiniTicksPerTick;
        if (Enum.TryParse<FadeToBlackTypes>(fadeType, out var fade))
        {
            var spendDaytimePoints = GameManager.Instance.SpendDaytimePoints(passedTp, blockable, true, false,
                fromCard?.Card, fade, fadeText, blockable, false, null, null, null);

            var queuedCardAction = GameManager.Instance.QueuedCardActions[0];
            GameManager.Instance.QueuedCardActions.RemoveAt(0);

            yield return GameManager.Instance.StartCoroutine(spendDaytimePoints);

            GameManager.Instance.QueuedCardActions.Insert(0, queuedCardAction);
        }
    }

    public void PassTime(int miniTick, InGameCardBridge? fromCard = null, bool blockable = true,
        string fadeType = nameof(FadeToBlackTypes.Partial), string fadeText = "")
    {
        var spendDaytimePoints = PassTimeEnum(miniTick, fromCard, blockable, fadeType, fadeText);
        CoUtils.StartCoWithBlockAction(spendDaytimePoints);
    }

    public void Log(string message)
    {
        Debug.Log(message);
    }
}