using System;
using System.Collections.Generic;

namespace CardTCLib.LuaBridge;

public class Events
{
    public delegate void OnCardActionDelegate(InGameCardBridge rec, InGameCardBridge? give);

    private Action _modLoadComplete;
    private Action _modLoadCompletePost;
    private readonly Dictionary<string, OnCardActionDelegate> _onActions = new();
    private readonly Dictionary<string, OnCardActionDelegate> _onEndActions = new();
    public readonly Dictionary<string, Func<InGameCardBridge, string>> CardNameOverrides = new();
    public readonly Dictionary<string, Func<string, InGameCardBridge, string>> ActionNameOverrides = new();

    public void RegModLoadComplete(Action action)
    {
        _modLoadComplete += action;
    }

    public void RegModLoadCompletePost(Action action)
    {
        _modLoadCompletePost += action;
    }

    public void RegOnAction(string key, OnCardActionDelegate action)
    {
        if (_onActions.ContainsKey(key))
        {
            _onActions[key] += action;
        }
        else
        {
            _onActions[key] = action;
        }
    }

    public void RegOnEndAction(string key, OnCardActionDelegate action)
    {
        if (_onEndActions.ContainsKey(key))
        {
            _onEndActions[key] += action;
        }
        else
        {
            _onEndActions[key] = action;
        }
    }

    public void RegCardNameOverride(UniqueIdObjectBridge card, Func<InGameCardBridge, string> func)
    {
        if (card.UniqueIDScriptable == null) return;
        var id = card.UniqueIDScriptable.UniqueID;
        CardNameOverrides[id] = func;
    }

    public void RegActionNameOverride(UniqueIdObjectBridge card, Func<string, InGameCardBridge, string> func)
    {
        if (card.UniqueIDScriptable == null) return;
        var id = card.UniqueIDScriptable.UniqueID;
        ActionNameOverrides[id] = func;
    }

    public bool HasEffect(string id)
    {
        return _onActions.ContainsKey(id) || _onEndActions.ContainsKey(id);
    }

    public void NotifyCardAction(string id, InGameCardBridge rec, InGameCardBridge? give = null)
    {
        if (_onActions.TryGetValue(id, out var action))
        {
            action(rec, give);
        }
    }

    public void NotifyCardActionEnd(string id, InGameCardBridge rec, InGameCardBridge? give = null)
    {
        if (_onEndActions.TryGetValue(id, out var action))
        {
            action(rec, give);
        }
    }

    public Events()
    {
        ModLoader.ModLoader.OnLoadModComplete += OnModLoadComplete;
    }

    protected virtual void OnModLoadComplete()
    {
        _modLoadComplete?.Invoke();
    }

    public void OnModLoadCompletePost()
    {
        _modLoadCompletePost?.Invoke();
    }
}