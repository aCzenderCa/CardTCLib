using CardTCLib.Const;
using CardTCLib.Util;
using HarmonyLib;
using UnityEngine;

namespace CardTCLib.Patch;

[HarmonyPatch]
public static class EffectWithSlotPatch
{
    [HarmonyPatch(typeof(EncounterPlayerAction),
         nameof(EncounterPlayerAction.ApplyClashAndDamageFromCard)), HarmonyPostfix]
    public static void EncounterPlayerAction_ApplyClashAndDamageFromCard_Post(
        EncounterPlayerAction __instance, InGameCardBase _Card)
    {
        var extraClash = 0f;
        var extraDamage = 0f;
        if (_Card.IsTCWeapon())
        {
            extraClash = _Card.CollectFloatValue(TCWeaponAttrs.ExtraClash);
            extraDamage = _Card.CollectFloatValue(TCWeaponAttrs.ExtraDamage);
        }

        __instance.Clash += new Vector2(extraClash, extraClash);
        __instance.Damage += new Vector2(extraDamage, extraDamage);
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.CardOnCardActionRoutine)), HarmonyPrefix]
    public static void GameManager_CardOnCardActionRoutine_Pre(ref CardOnCardAction _Action,
        InGameCardBase _GivenCard, InGameCardBase _ReceivingCard)
    {
        var timeCostReduce = 0f;
        var tcTool = _GivenCard.IsTCTool()
            ? _GivenCard
            : _ReceivingCard.IsTCTool()
                ? _ReceivingCard
                : null;
        var actionCopied = false;
        if (tcTool != null)
        {
            timeCostReduce = tcTool.CollectFloatValue(TCToolAttrs.TimeCostReduce);
        }

        if ((int)timeCostReduce > 0 && _Action.DaytimeCost > 0)
        {
            if (!actionCopied)
            {
                _Action = _Action.;
                actionCopied = true;
            }

            _Action.SetDayTimeCost(_Action.DaytimeCost - (int)timeCostReduce);
        }
    }

    [HarmonyPatch(typeof(CardAction), nameof(CardAction.CollectActionModifiers)), HarmonyPostfix]
    public static void CardAction_CollectActionModifiers_Post(CardAction __instance, InGameCardBase _ReceivingCard,
        InGameCardBase _GivenCard)
    {
        var usageCostReduceGive = 0f;
        var usageCostReduceRec = 0f;

        if (_ReceivingCard.IsTCTool())
        {
            usageCostReduceRec = _ReceivingCard.CollectFloatValue(TCToolAttrs.UsageCostReduce);
        }

        if (_GivenCard.IsTCTool())
        {
            usageCostReduceGive = _GivenCard.CollectFloatValue(TCToolAttrs.UsageCostReduce);
        }

        if (usageCostReduceRec > 0 && __instance.ReceivingDurabilityChanges.Usage.FloatValue < 0)
        {
            __instance.ReceivingDurabilityChanges.Usage.FloatValue = Mathf.Min(0,
                __instance.ReceivingDurabilityChanges.Usage.FloatValue + usageCostReduceRec);
        }

        if (usageCostReduceGive > 0 && __instance.GivenDurabilityChanges.Usage.FloatValue < 0)
        {
            __instance.GivenDurabilityChanges.Usage.FloatValue = Mathf.Min(0,
                __instance.GivenDurabilityChanges.Usage.FloatValue + usageCostReduceGive);
        }
    }
}