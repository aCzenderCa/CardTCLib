using HarmonyLib;

namespace CardTCLib.Patch;

public static class NpcActionPatch
{
    [HarmonyPatch(typeof(InGameCardBase), nameof(InGameCardBase.DropInInventory)), HarmonyPrefix]
    public static bool CanBeDragged(InGameCardBase _Card)
    {
        if (_Card.IsOwnedByNPC) return false;
        return true;
    }
}