using System.Collections;
using System.Linq;
using CardTCLib.Util;
using UnityEngine;

namespace CardTCLib.LuaBridge;

public class InGameCardBridge(InGameCardBase card)
{
    public readonly InGameCardBase Card = card;

    public int InventoryCardsCount => Card.CardsInInventory.Count;

    public InGameCardBridge[]? this[int index]
    {
        get
        {
            if (Card.IsInventoryCard && index < Card.CardsInInventory.Count)
            {
                var inventorySlot = Card.CardsInInventory[index];
                return inventorySlot.AllCards.Select(card => new InGameCardBridge(card)).ToArray();
            }

            return null;
        }
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

            return new UniqueIdObjectBridge(Card.CardModel)[key];
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

    private IEnumerator AddCardEnumerator(CardData cardData)
    {
        var addCard = GameManager.Instance.AddCard(cardData, Card, true,
            GameManager.SpecialDrop.InsideCardInventory, null, null,
            null, null, true, SpawningLiquid.DefaultLiquid,
            new Vector2Int(GameManager.Instance.CurrentTickInfo.z, 0), null, _MoveView: false);
        return addCard;
    }

    public void AddCard(UniqueIdObjectBridge obj)
    {
        if (obj.UniqueIDScriptable is CardData cardData)
        {
            var addCard = AddCardEnumerator(cardData);
            CoUtils.StartCoWithBlockAction(addCard);
        }
    }

    public IEnumerator DeleteEnumerator()
    {
        var removeCard = GameManager.Instance.RemoveCard(Card, false, false);
        return removeCard;
    }

    public void Delete()
    {
        var removeCard = DeleteEnumerator();
        CoUtils.StartCoWithBlockAction(removeCard);
    }

    public void ResetInventory()
    {
        IEnumerator? enumerator = null;
        for (var i = 0; i < InventoryCardsCount; i++)
        {
            var cardBridges = this[i];
            if (cardBridges == null) continue;
            foreach (var cardBridge in cardBridges)
            {
                enumerator = enumerator.Then(cardBridge.DeleteEnumerator());
            }
        }

        foreach (var cardData in Card.CardModel.InventorySlots)
        {
            enumerator = enumerator.Then(AddCardEnumerator(cardData));
        }

        if (enumerator != null)
            CoUtils.StartCoWithBlockAction(enumerator);
    }
}