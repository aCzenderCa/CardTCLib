using System.Collections;
using System.Collections.Generic;
using CardTCLib.LuaBridge;
using CardTCLib.Util;
using HarmonyLib;

namespace CardTCLib.Patch;

public static class CardPatch_0
{
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.ActionRoutine)), HarmonyPostfix]
    public static void GameManager_ActionRoutine_Post(ref IEnumerator __result, CardAction _Action,
        InGameCardBase _ReceivingCard, InGameCardBase? _GivenCard)
    {
        if (string.IsNullOrEmpty(_Action.ActionName.ParentObjectID)) return;
        if (!MainRuntime.Events.HasEffect(_Action.ActionName.ParentObjectID)) return;
        if (_ReceivingCard != null)
            NpcActionPatch.DisablePulsedCards.Add(_ReceivingCard.CardModel.UniqueID);
        __result = __result.OnEnumerator(() =>
        {
            MainRuntime.Events.NotifyCardAction(_Action.ActionName.ParentObjectID,
                InGameCardBridge.Get(_ReceivingCard)!, _GivenCard != null ? InGameCardBridge.Get(_GivenCard) : null);
        }, () =>
        {
            MainRuntime.Events.NotifyCardActionEnd(_Action.ActionName.ParentObjectID,
                InGameCardBridge.Get(_ReceivingCard)!, _GivenCard != null ? InGameCardBridge.Get(_GivenCard) : null);
        });
    }

    [HarmonyPatch(typeof(InGameCardBase), nameof(InGameCardBase.CardName)), HarmonyPostfix]
    public static void InGameCardBase_CardName(InGameCardBase __instance, ref string __result)
    {
        if (__instance && __instance.CardModel &&
            MainRuntime.Events.CardNameOverrides.TryGetValue(__instance.CardModel.UniqueID, out var func))
        {
            __result = func(InGameCardBridge.Get(__instance)!);
        }
    }

    [HarmonyPatch(typeof(InspectionPopup), nameof(InspectionPopup.SetupAction)), HarmonyPostfix]
    public static void InspectionPopup_SetupAction_Post(int _Index, DismantleCardAction[] _ActionsList,
        List<DismantleActionButton> _OptionsButtons, InGameCardBase _CurrentCard)
    {
        var currentCard = _CurrentCard;
        if (!currentCard || !currentCard.CardModel) return;
        var action = _ActionsList[_Index];
        if (!MainRuntime.Events.ActionNameOverrides.TryGetValue(currentCard.CardModel.UniqueID, out var func)) return;
        var newName = func(action.ActionName, InGameCardBridge.Get(currentCard)!);
        if (newName == null)
        {
            _OptionsButtons[_Index].gameObject.SetActive(false);
        }
        else
        {
            _OptionsButtons[_Index].gameObject.SetActive(true);
            _OptionsButtons[_Index].Interactable = true;
            _OptionsButtons[_Index].Text = newName;
        }
    }
}