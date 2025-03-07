using CardTCLib.Const;
using CardTCLib.Util;
using HarmonyLib;
using UnityEngine;

namespace CardTCLib.Patch;

public static class TCEffectCardPatch
{
    [HarmonyPatch(typeof(InGameCardBase), nameof(InGameCardBase.ModifyDurability)), HarmonyPrefix]
    public static void InGameCardBase_ModifyDurability_Pre(InGameCardBase __instance, DurabilitiesTypes _Type,
        ref float _Amt)
    {
        if (__instance.IsTCTool() || __instance.IsTCWeapon())
        {
            if (_Type == DurabilitiesTypes.Usage && _Amt < 0)
            {
                _Amt = Mathf.Min(0, _Amt + __instance.CollectFloatValue(TCCommonAttrs.UsageCostReduce));
                foreach (var inventorySlot in __instance.CardsInInventory)
                {
                    var mainCard = inventorySlot.MainCard;
                    if (mainCard == null) continue;
                    var itemUsageCost = mainCard.CollectFloatValue(TCCommonAttrs.ItemUsageCost);
                    GameManager.Instance.ChangeCardDurability(mainCard, -itemUsageCost, DurabilitiesTypes.Usage);
                }
            }
        }
    }
}