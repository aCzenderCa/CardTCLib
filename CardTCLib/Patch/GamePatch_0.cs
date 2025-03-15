using HarmonyLib;

namespace CardTCLib.Patch;

public static class GamePatch_0
{
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.Awake)), HarmonyPostfix]
    public static void GameManager_Awake_Post()
    {
        MainRuntime.Game.LoadGlobalValues();
    }

    [HarmonyPatch(typeof(GameLoad), nameof(GameLoad.SaveGameDataToFile)), HarmonyPrefix]
    public static void GameLoad_SaveGameDataToFile_Pre(GameLoad __instance, int _Index, CheckpointTypes _Checkpoint,
        int _CustomIndex)
    {
        if (!__instance.ValidSaveCheck || _Index < 0 || _Index >= __instance.Games.Count)
            return;
        MainRuntime.Game.SaveGlobalValues(__instance.Games[_Index].GetCheckpointData(_Checkpoint, _CustomIndex));
    }
}