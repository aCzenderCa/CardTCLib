using System;
using System.Collections.Generic;
using CardTCLib.Util;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CardTCLib.Patch;

public static class UtilsPatch
{
    [HarmonyPatch(typeof(BlueprintConstructionPopup), nameof(BlueprintConstructionPopup.AutoFill)), HarmonyPostfix]
    public static void BlueprintConstructionPopup_AutoFill(BlueprintConstructionPopup __instance)
    {
        if (!__instance.CurrentCard?.CardModel || __instance.CurrentCard.CardModel.BlueprintStages.IsNullOrEmpty())
            return;

        __instance.AutoFillFromAny();
    }

    public static void FillSlotFromAny(this BlueprintConstructionPopup __instance, int index,
        ref List<InGameCardBase> withCards, int upTo)
    {
        var cardAmt = __instance.CurrentCard.CardsInInventory[index].CardAmt;
        for (var i = 0; i < withCards.Count && cardAmt + i < upTo; ++i)
        {
            if (withCards[i].CurrentSlot != __instance.InventorySlotsLine.Slots[index])
            {
                if (withCards[i].CurrentSlot?.SlotType == SlotsTypes.Inventory)
                {
                    withCards[i].CurrentSlot.RemoveSpecificCard(withCards[i], false);
                    withCards[i].CurrentContainer.RemoveCardFromInventory(withCards[i]);
                    withCards[i].CurrentContainer = null;
                }

                GraphicsManager.Instance.MoveCardToSlot(withCards[i],
                    __instance.InventorySlotsLine.Slots[index].ToInfo(), true);
            }
        }
    }

    public static void AutoFillFromAny(this BlueprintConstructionPopup __instance)
    {
        var blueprintStage = __instance.CurrentCard.CardModel.BlueprintStages[
            Mathf.Clamp(__instance.CurrentCard.BlueprintData.CurrentStage, 0,
                __instance.CurrentCard.CardModel.BlueprintStages.Length - 1)];

        var withCards1 = new List<InGameCardBase>();
        for (var i = 0; i < blueprintStage.RequiredElements.Length; ++i)
        {
            if (!__instance.CurrentCard.CardsInInventory[i].IsFree)
            {
                if (__instance.CurrentCard.CardsInInventory[i].CardAmt !=
                    blueprintStage.RequiredElements[i].GetQuantity)
                {
                    withCards1.Clear();
                    if (__instance.GM.CardIsOnBoard(__instance.CurrentCard.CardsInInventory[i].CardModel, true,
                            __instance.GM.CurrentEnvironment, _Results: withCards1))
                        __instance.FillSlotFromAny(i, ref withCards1,
                            blueprintStage.RequiredElements[i].GetQuantity);
                }
            }
            else
            {
                var cardDataList = blueprintStage.RequiredElements[i].AllCards(true);
                var withCards2 = new List<InGameCardBase>();
                var index2 = 0;
                var num = 0;
                for (var index3 = 0; index3 < cardDataList.Count; ++index3)
                {
                    withCards1.Clear();
                    if (__instance.GM.CardIsOnBoard(cardDataList[index3], true, __instance.GM.CurrentEnvironment,
                            _Results: withCards1))
                    {
                        for (var index4 = withCards1.Count - 1; index4 >= 0; --index4)
                        {
                            if (__instance.GrM.CharacterWindow.HasCardEquipped(withCards1[index4]))
                                withCards1.RemoveAt(index4);
                            else if ((bool)(Object)withCards1[index4].CardModel &&
                                     withCards1[index4].CardModel.CardType != CardTypes.Item &&
                                     withCards1[index4].CardModel.CardType != CardTypes.Base &&
                                     withCards1[index4].CardModel.CardType != CardTypes.Liquid)
                                withCards1.RemoveAt(index4);
                            else if (withCards1[index4].IsLiquid &&
                                     withCards1[index4].CurrentContainer &&
                                     withCards1[index4].CurrentContainer.CardModel &&
                                     withCards1[index4].CurrentContainer.CardModel.CardType != CardTypes.Item &&
                                     withCards1[index4].CurrentContainer.CardModel.CardType != CardTypes.Base)
                                withCards1.RemoveAt(index4);
                            else if (blueprintStage.RequiredElements[i].GetLiquidQuantity > 0 ||
                                     cardDataList[index3].CanContainLiquid)
                            {
                                if (!blueprintStage.RequiredElements[i].CompatibleInGameCard(withCards1[index4]))
                                    withCards1.RemoveAt(index4);
                                else if ((bool)(Object)withCards1[index4].CurrentContainer &&
                                         ((bool)(Object)withCards1[index4].CurrentContainer
                                              .CurrentContainer ||
                                          __instance.GrM.CharacterWindow.HasCardEquipped(withCards1[index4]
                                              .CurrentContainer)))
                                    withCards1.RemoveAt(index4);
                            }
                        }

                        if (withCards1.Count > 0)
                        {
                            if (num == 0 || withCards1.Count >= blueprintStage.RequiredElements[i].GetQuantity)
                            {
                                withCards2.Clear();
                                withCards2.AddRange(withCards1);
                                index2 = index3;
                                num = withCards1.Count;
                            }

                            if (withCards1.Count >= blueprintStage.RequiredElements[i].GetQuantity)
                                break;
                        }
                    }
                }

                if (num != 0)
                {
                    if ((blueprintStage.RequiredElements[i].GetLiquidQuantity > 0 ||
                         cardDataList[index2].CanContainLiquid) && withCards2[0].IsLiquid)
                    {
                        var count = withCards2.Count;
                        for (var index5 = 0; index5 < count; ++index5)
                        {
                            withCards2.Add(withCards2[0].CurrentContainer);
                            withCards2.RemoveAt(0);
                        }
                    }

                    __instance.FillSlotFromAny(i, ref withCards2, blueprintStage.RequiredElements[i].GetQuantity);
                }
            }
        }
    }
}