using HarmonyLib;

namespace CardTCLib.Patch;

public static class NpcActionPatch
{
    [HarmonyPatch(typeof(NPCAction), nameof(NPCAction.ToAction)), HarmonyPostfix]
    public static void NPCAction_ToAction_Post(NPCAction __instance, CardAction __result)
    {
    }
}