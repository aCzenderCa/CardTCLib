using UnityEngine;

namespace CardTCLib.LuaBridge;

public class CardActionBridge(CardAction? action)
{
    public readonly CardAction? Action = action;

    public bool DontClosePopup
    {
        get => Action is DismantleCardAction { DontCloseInspectionWindow: false };
        set
        {
            if (Action is DismantleCardAction dismantleCardAction)
                dismantleCardAction.DontCloseInspectionWindow = value;
        }
    }

    public void ToTick()
    {
        if (Action is FromStatChangeAction statChangeAction)
        {
            statChangeAction.StatChangeTrigger =
            [
                new StatValueTrigger
                {
                    Stat = MainRuntime.Game.GetItem("ca25b2c02ece6674bae6aaba4a6b8c10")!.UniqueIDScriptable as GameStat,
                    TriggerRange = new Vector2(-1e6f, 1e6f)
                }
            ];
            statChangeAction.RepeatOptions = StatTriggerTypes.Repeat;
            statChangeAction.OncePerTick = true;
        }
    }
}