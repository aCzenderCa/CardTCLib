using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BepInEx;
using CardTCLib.Util;
using gfoidl.Base64;
using ModLoader;
using UnityEngine;

namespace CardTCLib.LuaBridge;

public class InGameCardBridge
{
    public readonly Dictionary<string, CommonValue> ExtraValues = new();
    public static readonly Regex ExtraValueRegex = new(@"\[_TCLib__Ex_\](?<b64>.*)", RegexOptions.Compiled);

    public void SaveExtraValues()
    {
        var memoryStream = new MemoryStream();
        var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true);
        writer.Write(ExtraValues);
        writer.Flush();
        var base64Str = Base64.Default.Encode(memoryStream.ToArray());
        var key = Card.DroppedCollections.FirstOrDefault(pair => ExtraValueRegex.IsMatch(pair.Key)).Key;
        if (!key.IsNullOrWhiteSpace()) Card.DroppedCollections.Remove(key);

        Card.DroppedCollections[$"[_TCLib__Ex_]{base64Str}"] = Vector2Int.zero;
    }

    public void LoadExtraValues()
    {
        var extraValueData = Card.DroppedCollections.FirstOrDefault(pair => ExtraValueRegex.IsMatch(pair.Key)).Key;
        if (!extraValueData.IsNullOrWhiteSpace())
        {
            var value = ExtraValueRegex.Match(extraValueData).Groups["b64"].Value;
            var decode = Base64.Default.Decode(value.ToCharArray());
            var reader = new BinaryReader(new MemoryStream(decode));
            var commonValueTable = reader.ReadCommonValueTable();
            foreach (var (key, val) in commonValueTable)
            {
                ExtraValues[key] = val;
            }
        }
    }

    public void SetExtraValue(string key, object value)
    {
        ExtraValues[key] = new CommonValue(value);
        SaveExtraValues();
    }

    public object? GetExtraValue(string key)
    {
        if (ExtraValues.TryGetValue(key, out var value))
        {
            return value.Value;
        }

        return null;
    }

    public static InGameCardBridge? Get(InGameCardBase? card)
    {
        if (card == null) return null;
        try
        {
            if (CardBridges.TryGetValue(card, out var reference))
            {
                if (reference.TryGetTarget(out var cardBridge))
                {
                    return cardBridge;
                }

                cardBridge = new InGameCardBridge(card);
                reference.SetTarget(cardBridge);
                return cardBridge;
            }
            else
            {
                var cardBridge = new InGameCardBridge(card);
                CardBridges[card] = new WeakReference<InGameCardBridge>(cardBridge);
                return cardBridge;
            }
        }
        catch (Exception e)
        {
            if (card != null) Get(card);
            return null;
        }
    }

    public static readonly Dictionary<InGameCardBase, WeakReference<InGameCardBridge>> CardBridges = [];
    public readonly InGameCardBase Card;

    private InGameCardBridge(InGameCardBase card)
    {
        Card = card;
        LoadExtraValues();
    }

    ~InGameCardBridge()
    {
        CardBridges.Remove(Card);
    }

    public int InventoryCardsCount => Card.CardsInInventory.Count;
    public static Vector2Int CurrentTickForAddCard => new(GameManager.Instance.CurrentTickInfo.z, 0);

    public InGameCardBridge[] GetItem(int index)
    {
        if (Card.IsInventoryCard && index < Card.CardsInInventory.Count)
        {
            var inventorySlot = Card.CardsInInventory[index];
            return inventorySlot.AllCards.Where(card => card != null).Select(card => Get(card)!).ToArray();
        }

        return [];
    }

    public float this[string key]
    {
        get
        {
            switch (key)
            {
                case "Usage":
                    return Card.CurrentUsageDurability;
                case "Fuel":
                    return Card.CurrentFuel;
                case "Progress":
                    return Card.CurrentProgress;
                case "Spoilage":
                    return Card.CurrentSpoilage;
                case "Special1":
                    return Card.CurrentSpecial1;
                case "Special2":
                    return Card.CurrentSpecial2;
                case "Special3":
                    return Card.CurrentSpecial3;
                case "Special4":
                    return Card.CurrentSpecial4;
            }

            return new UniqueIdObjectBridge(Card.CardModel).GetItem(key);
        }
        set
        {
            IEnumerator? changeEnumerator = null;
            switch (key)
            {
                case "Usage":
                    changeEnumerator = GameManager.Instance.ChangeCardDurabilities(Card, 0, value - this[key], 0,
                        0, 0, 0, 0, 0, 0, false,
                        false);
                    break;
                case "Fuel":
                    changeEnumerator = GameManager.Instance.ChangeCardDurabilities(Card, 0, 0, value - this[key],
                        0, 0, 0, 0, 0, 0, false,
                        false);
                    break;
                case "Progress":
                    changeEnumerator = GameManager.Instance.ChangeCardDurabilities(Card, 0, 0, 0, value - this[key], 0,
                        0, 0, 0, 0, false,
                        false);
                    break;
                case "Spoilage":
                    changeEnumerator = GameManager.Instance.ChangeCardDurabilities(Card, value - this[key], 0, 0,
                        0, 0, 0, 0, 0, 0, false,
                        false);
                    break;
                case "Special1":
                    changeEnumerator = GameManager.Instance.ChangeCardDurabilities(Card, 0, 0, 0,
                        0, 0, value - this[key], 0, 0, 0, false,
                        false);
                    break;
                case "Special2":
                    changeEnumerator = GameManager.Instance.ChangeCardDurabilities(Card, 0, 0, 0,
                        0, 0, 0, value - this[key], 0, 0, false,
                        false);
                    break;
                case "Special3":
                    changeEnumerator = GameManager.Instance.ChangeCardDurabilities(Card, 0, 0, 0,
                        0, 0, 0, 0, value - this[key], 0, false,
                        false);
                    break;
                case "Special4":
                    changeEnumerator = GameManager.Instance.ChangeCardDurabilities(Card, 0, 0, 0,
                        0, 0, 0, 0, 0, value - this[key], false,
                        false);
                    break;
            }

            if (changeEnumerator != null)
                CoUtils.StartCoWithBlockAction(changeEnumerator);
        }
    }

    public IEnumerator AddCardEnum(CardData cardData)
    {
        var addCard = GameManager.Instance.AddCard(cardData, Card, true,
            GameManager.SpecialDrop.InsideCardInventory, null, null,
            null, null, true, SpawningLiquid.DefaultLiquid,
            CurrentTickForAddCard, null, _MoveView: false);
        return addCard;
    }

    public void AddCard(UniqueIdObjectBridge obj)
    {
        if (obj.UniqueIDScriptable is CardData cardData)
        {
            var addCard = AddCardEnum(cardData);
            CoUtils.StartCoWithBlockAction(addCard);
        }
    }

    public IEnumerator DeleteEnum(bool doDrop = false)
    {
        var removeCard = GameManager.Instance.RemoveCard(Card, false, doDrop);
        return removeCard;
    }

    public void Delete()
    {
        var removeCard = DeleteEnum();
        CoUtils.StartCoWithBlockAction(removeCard);
    }

    public IEnumerator? ResetInventoryEnum()
    {
        IEnumerator? enumerator = null;
        for (var i = 0; i < InventoryCardsCount; i++)
        {
            var cardBridges = GetItem(i);
            foreach (var cardBridge in cardBridges)
            {
                enumerator = enumerator.Then(cardBridge.DeleteEnum());
            }
        }

        foreach (var cardData in Card.CardModel.InventorySlots)
        {
            enumerator = enumerator.Then(AddCardEnum(cardData));
        }

        return enumerator;
    }

    public void ResetInventory()
    {
        CoUtils.StartCoWithBlockAction(ResetInventoryEnum());
    }

    public IEnumerator? ClearInventoryEnum()
    {
        IEnumerator? enumerator = null;
        for (var i = 0; i < InventoryCardsCount; i++)
        {
            var cardBridges = GetItem(i);
            foreach (var cardBridge in cardBridges)
            {
                enumerator = enumerator.Then(cardBridge.DeleteEnum());
            }
        }

        return enumerator;
    }

    public void ClearInventory()
    {
        CoUtils.StartCoWithBlockAction(ClearInventoryEnum());
    }

    public IEnumerator? TransformToEnum(UniqueIdObjectBridge uniqueIdObjectBridge)
    {
        if (uniqueIdObjectBridge.UniqueIDScriptable is not CardData cardData) return null;

        var addCard = GameManager.Instance.AddCard(cardData, Card.CurrentContainer, true,
            GameManager.SpecialDrop.InsideCardInventory, Card.GetDurabilities(), Card.BlueprintData,
            Card.CurrentFlavours, Card.CurrentSpices, false,
            Card.ContainedLiquid ? new SpawningLiquid(Card.ContainedLiquid.CardModel) : SpawningLiquid.DefaultLiquid,
            CurrentTickForAddCard, Card.TravelTarget.DictionnaryKey, _MoveView: false);

        var e = addCard.Then(DeleteEnum());
        return e;
    }

    public void TransformTo(UniqueIdObjectBridge uniqueIdObjectBridge)
    {
        CoUtils.StartCoWithBlockAction(TransformToEnum(uniqueIdObjectBridge));
    }
}