using System.Collections;
using CardTCLib.LuaBridge;
using CardTCLib.Util;
using HarmonyLib;
using UnityEngine;

namespace CardTCLib.Patch;

public static class NpcActionPatch
{
    [HarmonyPatch(typeof(NPCAction), nameof(NPCAction.ToAction)), HarmonyPostfix]
    public static void NPCAction_ToAction_Post(NPCAction __instance, CardAction __result)
    {
        var actionName = __result.ActionName;
        actionName.ParentObjectID = __instance.ActionID;
        __result.ActionName = actionName;
    }

    [HarmonyPatch(typeof(CardAction), nameof(CardAction.WillHaveAnEffect)), HarmonyPostfix]
    public static void CardAction_WillHaveAnEffect_Post(CardAction __instance, ref bool __result)
    {
        if (!__result)
        {
            __result |= MainRuntime.Events.HasEffect(__instance.ActionName.ParentObjectID);
        }
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.ActionRoutine)), HarmonyPostfix]
    public static void GameManager_ActionRoutine_Post(ref IEnumerator __result, CardAction _Action,
        InGameCardBase _ReceivingCard, InGameCardBase? _GivenCard)
    {
        if (_Action.ActionName.ParentObjectID == null) return;
        __result = __result.OnEnumerator(() =>
        {
            MainRuntime.Events.NotifyCardAction(_Action.ActionName.ParentObjectID,
                new InGameCardBridge(_ReceivingCard), _GivenCard != null ? new InGameCardBridge(_GivenCard) : null);
        }, () =>
        {
            MainRuntime.Events.NotifyCardActionEnd(_Action.ActionName.ParentObjectID,
                new InGameCardBridge(_ReceivingCard), _GivenCard != null ? new InGameCardBridge(_GivenCard) : null);
        });
    }
}