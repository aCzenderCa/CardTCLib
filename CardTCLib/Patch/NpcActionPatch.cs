using System.Collections;
using System.Collections.Generic;
using CardTCLib.LuaBridge;
using CardTCLib.Util;
using HarmonyLib;
using UnityEngine;

namespace CardTCLib.Patch;

public static class NpcActionPatch
{
    public static HashSet<string> DisablePulsedCards = [];

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

    [HarmonyPatch(typeof(InGameCardBase), nameof(InGameCardBase.Pulse)), HarmonyPrefix]
    public static bool InGameCardBase_Pulse_Pre(InGameCardBase __instance)
    {
        if (DisablePulsedCards.Contains(__instance.CardModel.UniqueID))
        {
            return false;
        }

        return true;
    }
}