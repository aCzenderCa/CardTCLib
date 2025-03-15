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
            CoUtils.CacheEnumerators = [];
            MainRuntime.Events.NotifyCardAction(_Action.ActionName.ParentObjectID,
                InGameCardBridge.Get(_ReceivingCard)!, _GivenCard != null ? InGameCardBridge.Get(_GivenCard) : null);
            return CoUtils.CollectCacheAndStart();
        }, () =>
        {
            MainRuntime.Events.NotifyCardActionEnd(_Action.ActionName.ParentObjectID,
                InGameCardBridge.Get(_ReceivingCard)!, _GivenCard != null ? InGameCardBridge.Get(_GivenCard) : null);
            return CoUtils.CollectCacheAndStart();
        });
    }

    [HarmonyPatch(typeof(InGameCardBase), nameof(InGameCardBase.CardName)), HarmonyPostfix]
    public static void InGameCardBase_CardName(InGameCardBase __instance, ref string __result)
    {
        if (__instance && __instance.CardModel &&
            MainRuntime.Events.CardNameOverrides.TryGetValue(__instance.CardModel.UniqueID, out var func))
        {
            __result = func(InGameCardBridge.Get(__instance)!) ?? __result;
        }
    }

    [HarmonyPatch(typeof(InGameCardBase), nameof(InGameCardBase.CardDescription)), HarmonyPostfix]
    public static void InGameCardBase_CardDescription(InGameCardBase __instance, ref string __result)
    {
        if (__instance && __instance.CardModel &&
            MainRuntime.Events.CardDescOverrides.TryGetValue(__instance.CardModel.UniqueID, out var func))
        {
            __result = func(InGameCardBridge.Get(__instance)!) ?? __result;
        }
    }

    [HarmonyPatch(typeof(InspectionPopup), nameof(InspectionPopup.SetupAction)), HarmonyPostfix]
    public static void InspectionPopup_SetupAction_Post(int _Index, DismantleCardAction[]? _ActionsList,
        List<DismantleActionButton> _OptionsButtons, InGameCardBase _CurrentCard)
    {
        var currentCard = _CurrentCard;
        if (!currentCard || !currentCard.CardModel) return;
        if (_ActionsList == null || _Index >= _ActionsList.Length) return;
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

    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(DismantleActionButton), nameof(DismantleActionButton.Setup), typeof(int),
         typeof(DismantleCardAction), typeof(InGameCardBase), typeof(bool), typeof(bool)), HarmonyPostfix]
    public static void InspectionPopup_SetupAction_Post(DismantleActionButton __instance, InGameCardBase _Card,
        DismantleCardAction _Action)
    {
        var currentCard = _Card;
        if (!currentCard || !currentCard.CardModel) return;
        var action = _Action;
        if (!MainRuntime.Events.ActionTipOverrides.TryGetValue(currentCard.CardModel.UniqueID, out var func)) return;
        var newTip = func(action.ActionName, InGameCardBridge.Get(currentCard)!);
        if (newTip != null)
        {
            __instance.ButtonTooltip = newTip;
        }
    }

    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(InGameCardBase), nameof(InGameCardBase.GetPossibleAction)), HarmonyPostfix]
    public static void InGameCardBase_GetPossibleAction(InGameCardBase __instance, InGameCardBase? _WithCard)
    {
        if (__instance.PossibleAction == null && _WithCard?.CardModel?.UniqueID != null)
        {
            if (MainRuntime.Events.PossibleActionOverrides.TryGetValue(_WithCard.CardModel.UniqueID, out var func))
            {
                var value = func(InGameCardBridge.Get(_WithCard)!, InGameCardBridge.Get(__instance)!);
                if (value != null)
                {
                    __instance.PossibleAction = _WithCard.CardModel.CardInteractions[value.Value];
                    __instance.ActionIsReversed = true;
                }
            }

            if (__instance.PossibleAction == null &&
                MainRuntime.Events.PossibleActionOverrides.TryGetValue(__instance.CardModel.UniqueID, out func))
            {
                var value = func(InGameCardBridge.Get(__instance)!, InGameCardBridge.Get(_WithCard)!);
                if (value != null)
                {
                    __instance.PossibleAction = __instance.CardModel.CardInteractions[value.Value];
                    __instance.ActionIsReversed = false;
                }
            }
        }
    }
}