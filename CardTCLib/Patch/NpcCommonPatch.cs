using CardTCLib.LuaBridge;
using HarmonyLib;

namespace CardTCLib.Patch;

public static class NpcCommonPatch
{
    [HarmonyPatch(typeof(InGameNPC), nameof(InGameNPC.CreateModelCard)), HarmonyPrefix]
    public static bool InGameNPC_CreateModelCard_Pre(InGameNPC __instance)
    {
        if (!__instance.NPCModel) return false;

        if (UniqueIdObjectBridge.GetGeneratedNpcCard(__instance.NPCModel) is { } npcCard)
        {
            __instance.ModelCard = npcCard;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(InGameNPC), nameof(InGameNPC.CreateModelCard)), HarmonyPostfix]
    public static void InGameNPC_CreateModelCard_Post(InGameNPC __instance)
    {
        UniqueIdObjectBridge.GeneratedNpcCards[__instance.NPCModel] = __instance.ModelCard;
    }
}