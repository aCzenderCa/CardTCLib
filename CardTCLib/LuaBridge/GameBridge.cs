using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx;
using CardTCLib.Util;
using HarmonyLib;
using NLua;
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

    #region Create

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

    #endregion

    #region GameLogic

    public IEnumerator PassTimeEnum(int miniTick, InGameCardBridge? fromCard = null, bool blockable = true,
        string fadeType = nameof(FadeToBlackTypes.Partial), string fadeText = "")
    {
        if (miniTick <= 0) yield break;
        GameManager.Instance.CurrentMiniTicks += miniTick;
        var passedTp = GameManager.Instance.CurrentMiniTicks / MiniTicksPerTick;
        GameManager.Instance.CurrentMiniTicks %= MiniTicksPerTick;
        if (passedTp <= 0) yield break;
        if (Enum.TryParse<FadeToBlackTypes>(fadeType, out var fade))
        {
            var spendDaytimePoints = GameManager.Instance.SpendDaytimePoints(passedTp, blockable, true, false,
                fromCard?.Card, fade, fadeText, blockable, false, null, null, null);

            yield return GameManager.Instance.StartCoroutine(spendDaytimePoints);
        }
    }

    public void PassTime(int miniTick, InGameCardBridge? fromCard = null, bool blockable = true,
        string fadeType = nameof(FadeToBlackTypes.Partial), string fadeText = "")
    {
        var spendDaytimePoints = PassTimeEnum(miniTick, fromCard, blockable, fadeType, fadeText);
        CoUtils.StartCoWithBlockAction(spendDaytimePoints);
    }

    #endregion

    public void CloseCurrentInspectionPopup()
    {
        var inspectionPopup = GraphicsManager.Instance.CurrentInspectionPopup;
        if (inspectionPopup)
        {
            inspectionPopup.Hide(false);
        }
    }

    public void RefreshCurrentInspectionPopup()
    {
        var inspectionPopup = GraphicsManager.Instance.CurrentInspectionPopup;
        if (inspectionPopup)
        {
            inspectionPopup.SetupCurrentCardActions();
            var currentCard = inspectionPopup.CurrentCard;
            inspectionPopup.PopupTitle.text = currentCard.CardName();
            inspectionPopup.DescriptionText.text = currentCard.CardDescription();
        }
    }

    public void FindCards(UniqueIdObjectBridge uniqueIdObjectBridge, LuaTable save, bool includeBackground = false)
    {
        var cardData = uniqueIdObjectBridge.CardData;
        if (cardData == null) return;
        var idx = 1;
        foreach (var inGameCardBase in GameManager.Instance.AllCards)
        {
            if ((!inGameCardBase.InBackground || includeBackground) && inGameCardBase.CardModel == cardData)
            {
                save[idx] = InGameCardBridge.Get(inGameCardBase);
                idx++;
            }
        }
    }

    #region GlobalValue

    public static readonly Regex GlobalValuesDatRx = new Regex(@"^\[TCLib.GData\](?<gdata>.+)$");
    public readonly Dictionary<string, CommonValue> GlobalVariables = [];
    public void LoadGlobalValues()
    {
        var saveData = GameManager.Instance.CurrentSaveData;
        var gdata = "";
        foreach (var quest in saveData.PinnedQuests)
        {
            var match = GlobalValuesDatRx.Match(quest);
            if (match.Success)
            {
                gdata = match.Groups["gdata"].Value;
                break;
            }
        }

        if (gdata.Length > 0)
        {
            GlobalVariables.Clear();
            RWUtils.DecodeB64CommonValueTable(gdata, GlobalVariables);
        }
    }

    public string SaveGlobalValues(GameSaveData? saveData = null)
    {
        var b64String = GlobalVariables.EncodeB64CommonValueTable();
        var saveString = $"[TCLib.GData]{b64String}";
        saveData?.PinnedQuests.Add(saveString);
        return saveString;
    }

    public object? GetGlobalValue(string key)
    {
        if (GlobalVariables.TryGetValue(key, out var commonValue))
        {
            return commonValue.Value;
        }

        return null;
    }

    public void SetGlobalValue(string key, object value)
    {
        GlobalVariables[key] = new CommonValue(value);
    }

    #endregion

    public void Log(string message)
    {
        Debug.Log(message);
    }
}