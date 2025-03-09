using CardTCLib.Const;
using CardTCLib.Util;
using HarmonyLib;
using UnityEngine;
using static CardTCLib.Patch.ActionPatchUtil;

namespace CardTCLib.Patch;

public static class EffectWithSlotPatch
{
    [HarmonyPatch(typeof(GenericEncounterPlayerAction),
         nameof(GenericEncounterPlayerAction.ApplyClashAndDamageFromCard)), HarmonyPostfix]
    public static void GenericEncounterPlayerAction_ApplyClashAndDamageFromCard_Post(
        GenericEncounterPlayerAction __instance, InGameCardBase _Card)
    {
        var extraClash = 0f;
        var extraDamage = 0f;
        if (_Card.IsTCWeapon())
        {
            extraClash = _Card.CollectFloatValue(TCWeaponAttrs.ExtraClash);
            extraDamage = _Card.CollectFloatValue(TCWeaponAttrs.ExtraDamage);
        }

        __instance.InitialClashValue += new Vector2(extraClash, extraClash);
        __instance.InitialDamage += new Vector2(extraDamage, extraDamage);
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.CardOnCardActionRoutine)), HarmonyPrefix]
    public static void GameManager_CardOnCardActionRoutine_Pre(ref CardOnCardAction _Action,
        InGameCardBase _ReceivingCard, InGameCardBase _GivenCard)
    {
        _Action = (CardOnCardAction)ActionEffect(_Action, _ReceivingCard, _GivenCard);
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.ActionRoutine)), HarmonyPrefix]
    public static void GameManager_ActionRoutine_Pre(ref CardAction _Action,
        InGameCardBase _ReceivingCard, InGameCardBase? _GivenCard)
    {
        _Action = ActionEffect(_Action, _ReceivingCard, _GivenCard);
    }
}