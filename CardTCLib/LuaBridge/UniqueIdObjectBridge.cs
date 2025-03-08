using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CardTCLib.Util;
using HarmonyLib;
using ModLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CardTCLib.LuaBridge;

public class UniqueIdObjectBridge(UniqueIDScriptable? uniqueIDScriptable)
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

    public void AddAction(CardActionBridge action)
    {
        if (UniqueIDScriptable is CardData cardData)
        {
            switch (action.Action)
            {
                case DismantleCardAction dismantleCardAction:
                    cardData.DismantleActions ??= [];
                    cardData.DismantleActions.Add(dismantleCardAction);
                    break;
                case FromStatChangeAction fromStatChangeAction:
                    cardData.OnStatsChangeActions ??= [];
                    cardData.OnStatsChangeActions = cardData.OnStatsChangeActions.AddToArray(fromStatChangeAction);
                    break;
                case CardOnCardAction cardOnCardAction:
                    cardData.CardInteractions ??= [];
                    cardData.CardInteractions = cardData.CardInteractions.AddToArray(cardOnCardAction);
                    break;
            }
        }
    }

    public void RemoveAction(string type, object id)
    {
        var cardData = UniqueIDScriptable as CardData;
        switch (type)
        {
            case nameof(CardAction):
                break;
            case nameof(CardOnCardAction):
                if (cardData != null)
                {
                    if (id is string sid)
                    {
                        cardData.CardInteractions = cardData.CardInteractions
                            .Where(action => action.ActionName.ParentObjectID != sid).ToArray();
                    }
                    else if (id is long iid)
                    {
                        cardData.CardInteractions = cardData.CardInteractions.Where((_, i) => i == iid).ToArray();
                    }
                }

                break;
            case nameof(DismantleCardAction):
                if (cardData != null)
                {
                    if (id is string sid)
                    {
                        cardData.DismantleActions = cardData.DismantleActions
                            .Where(action => action.ActionName.ParentObjectID != sid).ToList();
                    }
                    else if (id is long iid)
                    {
                        cardData.DismantleActions = cardData.DismantleActions.Where((_, i) => i == iid).ToList();
                    }
                }

                break;
            case nameof(FromStatChangeAction):
                if (cardData != null)
                {
                    if (id is string sid)
                    {
                        cardData.OnStatsChangeActions = cardData.OnStatsChangeActions
                            .Where(action => action.ActionName.ParentObjectID != sid).ToArray();
                    }
                    else if (id is long iid)
                    {
                        cardData.OnStatsChangeActions =
                            cardData.OnStatsChangeActions.Where((_, i) => i == iid).ToArray();
                    }
                }

                break;
        }
    }
}