using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CardTCLib.Util;
using HarmonyLib;
using ModLoader.LoaderUtil;
using NLua;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CardTCLib.LuaBridge;

public partial class UniqueIdObjectBridge(UniqueIDScriptable? uniqueIDScriptable)
{
    public UniqueIDScriptable? UniqueIDScriptable = uniqueIDScriptable;
    public static readonly Dictionary<NPCAgent, CardData> GeneratedNpcCards = new();

    public static CardData? GetGeneratedNpcCard(NPCAgent agent)
    {
        if (GeneratedNpcCards.TryGetValue(agent, out var generatedCard))
        {
            generatedCard.CardImage = agent.AgentImage;
            return generatedCard;
        }

        return null;
    }

    public float GetTValue(string key)
    {
        if (UniqueIDScriptable is not CardData cardData) return 0;
        return cardData.GetFloatValue(key);
    }

    public void SetTValue(string key, float value)
    {
        if (UniqueIDScriptable is not CardData cardData) return;
        var timeObjective = cardData.TimeValues.FirstOrDefault(objective => objective.ObjectiveName == key);
        if (timeObjective != null)
        {
            timeObjective.Value = (int)(value * 1000f);
        }
        else
        {
            cardData.TimeValues.Add(new TimeObjective { ObjectiveName = key, Value = (int)(value * 1000f) });
        }
    }

    public void AddSlot(UniqueIdObjectBridge? uniqueIdObjectBridge = null)
    {
        if (UniqueIDScriptable is not CardData cardData) return;
        cardData.InventorySlots ??= [];
        cardData.InventorySlots =
            cardData.InventorySlots.AddToArray(uniqueIdObjectBridge is { UniqueIDScriptable: CardData addCardData }
                ? addCardData
                : null);
    }

    public void RemoveSlot(int idx)
    {
        if (UniqueIDScriptable is not CardData cardData) return;
        cardData.InventorySlots ??= [];
        if (idx >= 0 && idx < cardData.InventorySlots.Length)
        {
            cardData.InventorySlots = cardData.InventorySlots.RemoveAtOnArr(idx);
        }
    }

    public float MaxWeightCapacity
    {
        get
        {
            if (UniqueIDScriptable is CardData cardData)
                return cardData.MaxWeightCapacity;
            return 0f;
        }
        set
        {
            if (UniqueIDScriptable is CardData cardData)
                cardData.MaxWeightCapacity = value;
        }
    }

    public float Weight
    {
        get
        {
            if (UniqueIDScriptable is CardData cardData)
                return cardData.ObjectWeight;
            return 0f;
        }
        set
        {
            if (UniqueIDScriptable is CardData cardData)
                cardData.ObjectWeight = value;
        }
    }

    public string Name
    {
        get
        {
            if (UniqueIDScriptable is CardData cardData)
            {
                return cardData.CardName.DefaultText;
            }

            if (UniqueIDScriptable is NPCAgent npcAgent)
            {
                return npcAgent.AgentName.DefaultText;
            }

            if (UniqueIDScriptable is GameStat stat)
            {
                return stat.GameName.DefaultText;
            }

            return "";
        }
    }

    public string NameLocal
    {
        get
        {
            if (UniqueIDScriptable is CardData cardData)
            {
                return cardData.CardName;
            }

            if (UniqueIDScriptable is NPCAgent npcAgent)
            {
                return npcAgent.AgentName;
            }

            if (UniqueIDScriptable is GameStat stat)
            {
                return stat.GameName;
            }

            return "";
        }
    }

    public string NameChinese
    {
        get
        {
            if (UniqueIDScriptable is CardData cardData)
            {
                return cardData.CardName.Chinese();
            }

            if (UniqueIDScriptable is NPCAgent npcAgent)
            {
                return npcAgent.AgentName.Chinese();
            }

            if (UniqueIDScriptable is GameStat stat)
            {
                return stat.GameName.Chinese();
            }

            return "";
        }
    }

    public float GetItem(string key)
    {
        if (UniqueIDScriptable is CardData cardData) return cardData.GetFloatValue(key);

        return 0;
    }

    public void SetItem(string key, float value)
    {
        if (UniqueIDScriptable is CardData cardData)
        {
            var timeObjective = cardData.TimeValues.FirstOrDefault(objective => objective.ObjectiveName == key);
            if (timeObjective == null)
            {
                timeObjective = new TimeObjective
                {
                    ObjectiveName = key
                };
                cardData.TimeValues.Add(timeObjective);
            }

            timeObjective.Value = Mathf.RoundToInt(value * 1000);
        }
    }

    public void GetStatInfo(out float? value, out float? rate, out float? minValue, out float? maxValue)
    {
        value = rate = minValue = maxValue = null;
        if (UniqueIDScriptable is GameStat stat)
        {
            var gm = GameManager.Instance;
            var inGameStat = gm.StatsDict[stat];
            value = inGameStat.SimpleCurrentValue;
            rate = inGameStat.SimpleRatePerTick;
            var currentMinMaxValue = inGameStat.CurrentMinMaxValue;
            minValue = currentMinMaxValue.x;
            maxValue = currentMinMaxValue.y;
        }
    }

    public void SetStat(float newValue, float? newRate = null)
    {
        var enumerator = SetStatEnum(newValue, newRate);
        CoUtils.StartCoWithBlockAction(enumerator);
    }

    public IEnumerator? SetStatEnum(float newValue, float? newRate = null)
    {
        if (UniqueIDScriptable is GameStat stat)
        {
            var gm = GameManager.Instance;
            var inGameStat = gm.StatsDict[stat];
            var enumerator = gm.ChangeStatValue(inGameStat, newValue - inGameStat.SimpleCurrentValue,
                StatModification.Permanent);
            if (newRate != null)
                enumerator = enumerator.Then(gm.ChangeStatRate(inGameStat, newRate.Value - inGameStat.SimpleRatePerTick,
                    StatModification.Permanent));
            return enumerator;
        }

        return null;
    }

    public void AddStat(float value, float? rate = null)
    {
        var enumerator = AddStatEnum(value, rate);
        CoUtils.StartCoWithBlockAction(enumerator);
    }

    public IEnumerator? AddStatEnum(float value, float? rate = null)
    {
        if (UniqueIDScriptable is GameStat stat)
        {
            var gm = GameManager.Instance;
            var inGameStat = gm.StatsDict[stat];
            var enumerator = gm.ChangeStatValue(inGameStat, value, StatModification.Permanent);
            if (rate != null)
                enumerator = enumerator.Then(gm.ChangeStatRate(inGameStat, rate.Value, StatModification.Permanent));
            return enumerator;
        }

        return null;
    }

    public UniqueIdObjectBridge GenNpcCardModel()
    {
        if (UniqueIDScriptable is not NPCAgent npcAgent) return new UniqueIdObjectBridge(null);
        // ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
        var inGameNpc = new GameObject().AddComponent<InGameNPC>();
        inGameNpc.name = npcAgent.name;
        inGameNpc.SetModel(npcAgent);
        inGameNpc.CreateModelCard();
        var modelCard = inGameNpc.ModelCard;
        Object.Destroy(inGameNpc.gameObject);
        return new UniqueIdObjectBridge(modelCard);
    }

    public void SetIcon(string icon)
    {
        if (UniqueIDScriptable is CardData cardData)
        {
            if (ModLoader.ModLoader.SpriteDict.TryGetValue(icon, out var sprite))
            {
                cardData.CardImage = sprite;
            }
            else
            {
                cardData.PostSetEnQueue((card, sp) => ((CardData)card).CardImage = (Sprite)sp, icon);
            }
        }
    }
}