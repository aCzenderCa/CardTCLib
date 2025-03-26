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
using Random = UnityEngine.Random;

namespace CardTCLib.LuaBridge;

public class GameBridge
{
    public int MiniTicksPerTick => GameManager.Instance.DaySettings.MiniTicksPerTick;
    public static Dictionary<string, UniqueIdObjectBridge> FindCache = new();

    public UniqueIdObjectBridge? GetItem(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        var uniqueIDScriptable = UniqueIDScriptable.AllUniqueObjects.GetValueSafe(key);
        if (uniqueIDScriptable == null)
            uniqueIDScriptable = ModLoader.ModLoader.AllGUIDDict.GetValueSafe(key);

        if (FindCache.TryGetValue(key, out var bridge)) return bridge;
        if (uniqueIDScriptable == null)
        {
            var c = "";
            if (key.Length > 2 && key[2] == '_')
            {
                c = key.Substring(0, 2);
                key = key.Substring(3);
            }

            var uniqueIdObjectBridge = new UniqueIdObjectBridge(null);
            uniqueIDScriptable = ModLoader.ModLoader.AllGUIDDict.Values.Concat(GameLoad.Instance.DataBase.AllData)
                .FirstOrDefault(uidScript =>
                {
                    if (!string.IsNullOrEmpty(c) && uidScript is CardData cardData &&
                        !cardData.CardType.ToString().StartsWith(c)) return false;
                    if (uidScript.name == key) return true;
                    uniqueIdObjectBridge.UniqueIDScriptable = uidScript;
                    return uniqueIdObjectBridge.Name == key || uniqueIdObjectBridge.NameLocal == key ||
                           uniqueIdObjectBridge.NameChinese == key;
                });
        }

        if (uniqueIDScriptable == null) return null;
        FindCache[key] = new UniqueIdObjectBridge(uniqueIDScriptable);

        return FindCache[key];
    }

    #region Create

    public CardTag CreateCardTag(string tagId, string tagName)
    {
        var cardTag = ScriptableObject.CreateInstance<CardTag>();
        cardTag.InGameName = new LocalizedString
            { DefaultText = tagName, ParentObjectID = tagId, LocalizationKey = tagId };
        cardTag.name = tagId;
        return cardTag;
    }

    public DurabilityStat CreateDurabilityStat(string id, LuaTable table, bool active = true)
    {
        var zeroAction = table.GetObj<CardActionBridge>("OnZero");
        var fullAction = table.GetObj<CardActionBridge>("OnFull");
        var durabilityStat = new DurabilityStat(active, (float)table.GetNum("InitValue"))
        {
            MaxValue = (float)table.GetNum("MaxValue"),
            CardStatName = new LocalizedString
                { DefaultText = id, LocalizationKey = id + "_CardStatName", ParentObjectID = id },
        };
        if (zeroAction != null)
        {
            durabilityStat.OnZero = zeroAction.Action;
            durabilityStat.HasActionOnZero = true;
        }

        if (fullAction != null)
        {
            durabilityStat.OnFull = fullAction.Action;
            durabilityStat.HasActionOnFull = true;
        }

        return durabilityStat;
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

    public LuaTable FindCards(UniqueIdObjectBridge uniqueIdObjectBridge, LuaTable save, bool includeBackground = false)
    {
        var cardData = uniqueIdObjectBridge.CardData;
        if (cardData == null) return save;
        var idx = 1;
        foreach (var inGameCardBase in GameManager.Instance.AllCards)
        {
            if ((!inGameCardBase.InBackground || includeBackground) && inGameCardBase.CardModel == cardData)
            {
                save[idx] = InGameCardBridge.Get(inGameCardBase);
                idx++;
            }
        }

        return save;
    }

    public LuaTable FindCardsByTag(string tag, LuaTable save, bool includeBackground = false)
    {
        var idx = 1;
        foreach (var inGameCardBase in GameManager.Instance.AllCards)
        {
            if ((!inGameCardBase.InBackground || includeBackground) &&
                InGameCardBridge.Get(inGameCardBase)!.HasTag(tag))
            {
                save[idx] = InGameCardBridge.Get(inGameCardBase);
                idx++;
            }
        }

        return save;
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

    #region Encounter

    public void AddEncounterAction(string id, string name)
    {
        var instanceEncounterPopupWindow = GraphicsManager.Instance.EncounterPopupWindow;
        if (!instanceEncounterPopupWindow.OngoingEncounter) return;

        var actionName = new LocalizedString
        {
            DefaultText = name, ParentObjectID = id, LocalizationKey = "EncounterAction_" + id
        };
        instanceEncounterPopupWindow.AddMainButton([
            new GenericEncounterPlayerAction
            {
                GeneratedActionName = actionName,
                ActionName = actionName,
                DontShowSuccessChance = true,
                ActionType = EncounterPlayerActionType.FreeAction,
                TemporaryEffects = [],
                DoesNotAttack = true,
            }
        ], null);
    }

    public void DoDamageToEnemy(float damage, float change, float hpScale = 1, float moraleScale = 1)
    {
        var instanceEncounterPopupWindow = GraphicsManager.Instance.EncounterPopupWindow;
        if (!instanceEncounterPopupWindow.OngoingEncounter) return;
        if (Random.value > change) return;
        instanceEncounterPopupWindow.CurrentEncounter.ModifyEnemyValue(EnemyValueNames.Blood, -damage * hpScale);
        instanceEncounterPopupWindow.CurrentEncounter.ModifyEnemyValue(EnemyValueNames.Morale, -damage * moraleScale);
    }

    public void HealPlayer(float heal, float change, float hpScale = 1, float moraleScale = 1)
    {
        var instanceEncounterPopupWindow = GraphicsManager.Instance.EncounterPopupWindow;
        if (!instanceEncounterPopupWindow.OngoingEncounter) return;
        if (Random.value > change) return;
        var healCo = GetItem("失血")!.AddStatEnum(-heal * hpScale).Then(GetItem("士气")!.AddStatEnum(heal * moraleScale));
        CoUtils.StartCoWithBlockAction(healCo);
    }

    #endregion

    public void Log(string message)
    {
        Debug.Log(message);
    }
}