using CardTCLib.Const;
using CardTCLib.Util;
using UnityEngine;

namespace CardTCLib.Patch;

public static class ActionPatchUtil
{
    internal static CardAction ActionEffect(CardAction _Action, InGameCardBase? _ReceivingCard,
        InGameCardBase? _GivenCard)
    {
        var timeCostReduce = 0f;
        var miniTimeCostReduce = 0f;
        var commonWorkCostReduce = 0f;
        var commonWorkDamageReduce = 0f;
        var actionCopied = _Action.ActionName.LocalizationKey == "__copied__";
        if (actionCopied) return _Action;
        if (_ReceivingCard.IsTCTool())
        {
            timeCostReduce = _ReceivingCard.CollectFloatValue(TCToolAttrs.TimeCostReduce);
            miniTimeCostReduce = _ReceivingCard.CollectFloatValue(TCToolAttrs.MiniTimeCostReduce);
        }

        if (_GivenCard.IsTCTool())
        {
            timeCostReduce = Mathf.Max(_GivenCard.CollectFloatValue(TCToolAttrs.TimeCostReduce), timeCostReduce);
            miniTimeCostReduce = Mathf.Max(_GivenCard.CollectFloatValue(TCToolAttrs.MiniTimeCostReduce),
                miniTimeCostReduce);
        }

        if (_ReceivingCard.IsTCCommon())
        {
            commonWorkCostReduce = _ReceivingCard.CollectFloatValue(TCCommonAttrs.CommonWorkCostReduce);
            commonWorkDamageReduce = _ReceivingCard.CollectFloatValue(TCCommonAttrs.CommonWorkDamageReduce);
        }

        if (_GivenCard.IsTCCommon())
        {
            commonWorkCostReduce = Mathf.Max(commonWorkCostReduce,
                _GivenCard.CollectFloatValue(TCCommonAttrs.CommonWorkCostReduce));
            commonWorkDamageReduce = Mathf.Max(commonWorkDamageReduce,
                _GivenCard.CollectFloatValue(TCCommonAttrs.CommonWorkDamageReduce));
        }

        if (Mathf.RoundToInt(timeCostReduce) > 0 && _Action.DaytimeCost > 0)
        {
            CpActionIfNeed(ref _Action);

            _Action.DaytimeCost -= Mathf.RoundToInt(timeCostReduce);
            _Action.TotalDaytimeCost -= Mathf.RoundToInt(timeCostReduce);
        }

        if (Mathf.RoundToInt(miniTimeCostReduce) > 0 && _Action.DaytimeCost > 0)
        {
            CpActionIfNeed(ref _Action);

            var fullMiniCost = _Action.DaytimeCost * 5 + _Action.MiniTicksCost;
            fullMiniCost -= Mathf.RoundToInt(miniTimeCostReduce);
            _Action.DaytimeCost = fullMiniCost / 5;
            _Action.TotalDaytimeCost = fullMiniCost / 5;
            GameManager.Instance.CurrentMiniTicks += fullMiniCost % 5;
            if (GameManager.Instance.CurrentMiniTicks >= 5)
            {
                _Action.DaytimeCost += 1;
                _Action.TotalDaytimeCost += 1;
                GameManager.Instance.CurrentMiniTicks -= 5;
            }

            if (_Action.DaytimeCost == 0)
                GraphicsManager.Instance.UpdateTimeInfo(false);
        }

        if (commonWorkCostReduce > 0)
        {
            CpActionIfNeed(ref _Action);

            var actionStatModifications = _Action.StatModifications;
            for (var i = 0; i < actionStatModifications.Length; i++)
            {
                var statModification = actionStatModifications[i];
                if (statModification.Stat && statModification.Stat.UniqueID == StatUids.Stamina_耐力)
                {
                    var modifier = statModification.ValueModifier;
                    if (modifier.x > 0 || modifier.y > 0) continue;
                    modifier.x = Mathf.Min(0f, modifier.x + commonWorkCostReduce);
                    modifier.y = Mathf.Min(0f, modifier.y + commonWorkCostReduce);
                    actionStatModifications[i].ValueModifier = modifier;
                }

                if (statModification.Stat && statModification.Stat.UniqueID == StatUids.Weight_体重)
                {
                    var modifier = statModification.ValueModifier;
                    if (modifier.x > 0 || modifier.y > 0) continue;
                    modifier.x = Mathf.Min(0f, modifier.x + commonWorkCostReduce * 1.5f);
                    modifier.y = Mathf.Min(0f, modifier.y + commonWorkCostReduce * 1.5f);
                    actionStatModifications[i].ValueModifier = modifier;
                }
            }
        }

        if (commonWorkDamageReduce > 0)
        {
            CpActionIfNeed(ref _Action);

            var actionStatModifications = _Action.StatModifications;
            for (var i = 0; i < actionStatModifications.Length; i++)
            {
                var statModification = actionStatModifications[i];
                if (statModification.Stat && statModification.Stat.UniqueID == StatUids.HandDamage_手掌损伤)
                {
                    var modifier = statModification.ValueModifier;
                    if (modifier.x > 0 || modifier.y > 0) continue;
                    modifier.x = Mathf.Max(0f, modifier.x - commonWorkCostReduce);
                    modifier.y = Mathf.Max(0f, modifier.y - commonWorkCostReduce);
                    actionStatModifications[i].ValueModifier = modifier;
                }

                if (statModification.Stat && statModification.Stat.UniqueID == StatUids.FootDamage_足部损伤)
                {
                    var modifier = statModification.ValueModifier;
                    if (modifier.x < 0 || modifier.y < 0) continue;
                    modifier.x = Mathf.Max(0f, modifier.x - commonWorkCostReduce);
                    modifier.y = Mathf.Max(0f, modifier.y - commonWorkCostReduce);
                    actionStatModifications[i].ValueModifier = modifier;
                }
            }
        }

        return _Action;

        void CpActionIfNeed(ref CardAction action)
        {
            var copied = action.ActionName.LocalizationKey == "__copied__";
            if (!copied)
            {
                action = action.CopyRaw();
                var actionName = action.ActionName;
                actionName.LocalizedText = actionName;
                actionName.LocalizationKey = "__copied__";
                action.ActionName = actionName;
            }
        }
    }
}