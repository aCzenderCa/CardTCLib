using System;
using System.Collections.Generic;
using System.Linq;
using CardTCLib.Util;
using NLua;
using UnityEngine;

namespace CardTCLib.LuaBridge;

public partial class UniqueIdObjectBridge
{
    public static readonly Dictionary<CardData, (string mainTab, string subTab)> BpCardTabs = new();
    public CardData? CardData => UniqueIDScriptable as CardData;

    public string? Desc
    {
        get => CardData?.CardDescription;
        set
        {
            if (CardData != null)
            {
                CardData.CardDescription = new LocalizedString
                    { LocalizationKey = CardData.CardDescription.LocalizationKey, DefaultText = value };
            }
        }
    }

    #region Bp

    public void SetBpTab(string mainTab, string subTab, bool needRemoveOld = true, bool setToTable = true)
    {
        if (UniqueIDScriptable is not CardData { CardType: CardTypes.Blueprint } cardData) return;
        if (BpCardTabs.TryGetValue(cardData, out var oldTab))
        {
            foreach (var cardTabGroup in Resources.FindObjectsOfTypeAll<CardTabGroup>()
                         .Concat(GraphicsManager.Instance?.BlueprintModelsPopup?.BlueprintTabs ?? []))
            {
                if (needRemoveOld && (cardTabGroup.name == oldTab.mainTab ||
                                      cardTabGroup.TabName.Chinese() == oldTab.mainTab))
                {
                    cardTabGroup.ShopSortingList.Remove(cardData);
                    foreach (var subGroup in cardTabGroup.SubGroups)
                    {
                        if (subGroup.name == oldTab.subTab || subGroup.TabName.Chinese() == oldTab.subTab)
                        {
                            subGroup.IncludedCards.Remove(cardData);
                            break;
                        }
                    }
                }

                if (cardTabGroup.name == mainTab || cardTabGroup.TabName.Chinese() == mainTab)
                {
                    if (!cardTabGroup.ShopSortingList.Contains(cardData))
                        cardTabGroup.ShopSortingList.Add(cardData);
                    cardTabGroup.FillSortingList();
                    foreach (var subGroup in cardTabGroup.SubGroups)
                    {
                        if (subGroup.name == subTab || subGroup.TabName.Chinese() == subTab)
                        {
                            if (!subGroup.IncludedCards.Contains(cardData))
                                subGroup.IncludedCards.Add(cardData);
                            subGroup.FillSortingList();
                        }
                    }
                }
            }
        }

        if (setToTable)
            BpCardTabs[cardData] = (mainTab, subTab);
    }

    public void SetBpStage(int idx, LuaTable table)
    {
        if (UniqueIDScriptable is not CardData { CardType: CardTypes.Blueprint } cardData) return;
        if (cardData.BlueprintStages == null || cardData.BlueprintStages?.Length <= idx)
        {
            var rawStages = cardData.BlueprintStages;
            cardData.BlueprintStages = new BlueprintStage[idx + 1];
            if (rawStages != null) Array.Copy(rawStages, cardData.BlueprintStages, rawStages.Length);
            for (var i = 0; i < cardData.BlueprintStages.Length; i++)
            {
                cardData.BlueprintStages[i] ??= new BlueprintStage();
            }
        }

        var stage = new BlueprintStage();
        var blueprintElements = new List<BlueprintElement>();
        foreach (var (_, val) in table.Ipairs<LuaTable>())
        {
            var blueprintElement = new BlueprintElement
            {
                RequiredCard = val.GetObj<UniqueIdObjectBridge>("Card")?.CardData,
                RequiredQuantity = (int)val.GetNum("Count"),
                DontSpend = !val.GetBool("Spend"),
                Usage = new OptionalRangeValue(true, (float)val.GetNum("UsageCost"), 0)
            };
            blueprintElements.Add(blueprintElement);
        }

        stage.RequiredElements = blueprintElements.ToArray();
        cardData.BlueprintStages![idx] = stage;
    }

    public void SetBpResults(LuaTable table)
    {
        if (UniqueIDScriptable is not CardData { CardType: CardTypes.Blueprint } cardData) return;
        var cardDrops = new List<CardDrop>();
        foreach (var (_, val) in table.Ipairs<LuaTable>())
        {
            var count = (int)val.GetNum("Count");
            var cardDrop = new CardDrop
            {
                DroppedCard = val.GetObj<UniqueIdObjectBridge>("Card")?.CardData,
                Quantity = new Vector2Int(count, count)
            };
            cardDrops.Add(cardDrop);
        }

        cardData.BlueprintResult = cardDrops.ToArray();
    }

    public void InitEmptyBpRequire()
    {
        if (CardData is not { CardType: CardTypes.Blueprint } cardData) return;
        if (cardData.TimeValues.IsNullOrEmpty())
        {
            cardData.TimeValues ??= [];
            cardData.TimeValues.Add(new TimeObjective
            {
                ObjectiveName = "Init", CompareType = SimpleComparison.Comparisons.GreaterOrEqual,
                TimeType = TimeValueTypes.Day, CompletionWeight = 1, Value = 0
            });
        }

        cardData.Init();
    }

    public void SetBuildStatCost(LuaTable table)
    {
        if (UniqueIDScriptable is not CardData { CardType: CardTypes.Blueprint } cardData) return;
        var statModifiers = new List<StatModifier>();
        foreach (var (_, mod) in table.Ipairs<StatModifier>())
        {
            statModifiers.Add(mod);
        }

        cardData.BlueprintStatModifications = statModifiers.ToArray();
    }

    #endregion
}