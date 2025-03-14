namespace CardTCLib.LuaBridge;

public class CardActionBridge(CardAction? action)
{
    public readonly CardAction? Action = action;

    public bool DontClosePopup
    {
        get => Action is DismantleCardAction { DontCloseInspectionWindow: false };
        set
        {
            if (Action is DismantleCardAction dismantleCardAction) dismantleCardAction.DontCloseInspectionWindow = value;
        }
    }
}