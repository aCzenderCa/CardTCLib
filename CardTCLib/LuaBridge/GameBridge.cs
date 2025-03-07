namespace CardTCLib.LuaBridge;

public class GameBridge
{
    public UniqueIdObjectBridge? this[string key]
    {
        get
        {
            var uniqueIDScriptable = UniqueIDScriptable.GetFromID(key);
            if (uniqueIDScriptable == null) return null;
            return new UniqueIdObjectBridge(uniqueIDScriptable);
        }
    }
}