using CardTCLib.Const;
using CardTCLib.Util;
using UnityEngine;

namespace CardTCLib.Patch;

public static class ActionPathUtil
{
    internal static CardAction ActionEffect(CardAction _Action, InGameCardBase? _ReceivingCard,
        InGameCardBase? _GivenCard)
    {
        var timeCostReduce = 0f;
        var miniTimeCostReduce = 0f;
        var actionCopied = _Action.ActionName.ParentObjectID == "copied";
        if (actionCopied) return _Action;
        if (_ReceivingCard != null && _ReceivingCard.IsTCTool())
        {
            timeCostReduce = _ReceivingCard.CollectFloatValue(TCToolAttrs.TimeCostReduce);
            miniTimeCostReduce = _ReceivingCard.CollectFloatValue(TCToolAttrs.MiniTimeCostReduce);
        }

        if (_GivenCard != null && _GivenCard.IsTCTool())
        {
            timeCostReduce = Mathf.Max(_GivenCard.CollectFloatValue(TCToolAttrs.TimeCostReduce), timeCostReduce);
            miniTimeCostReduce = Mathf.Max(_GivenCard.CollectFloatValue(TCToolAttrs.MiniTimeCostReduce),
                miniTimeCostReduce);
        }

        if (Mathf.RoundToInt(timeCostReduce) > 0 && _Action.DaytimeCost > 0)
        {
            if (!actionCopied)
            {
                _Action = _Action.CopyRaw();
                var actionName = _Action.ActionName;
                actionName.ParentObjectID = "copied";
                _Action.ActionName = actionName;
                actionCopied = true;
            }

            _Action.DaytimeCost -= Mathf.RoundToInt(timeCostReduce);
            _Action.TotalDaytimeCost -= Mathf.RoundToInt(timeCostReduce);
        }

        if (Mathf.RoundToInt(miniTimeCostReduce) > 0 && _Action.DaytimeCost > 0)
        {
            if (!actionCopied)
            {
                _Action = _Action.CopyRaw();
                var actionName = _Action.ActionName;
                actionName.ParentObjectID = "copied";
                _Action.ActionName = actionName;
                actionCopied = true;
            }

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

        return _Action;
    }
}