using System;
using System.Collections.Generic;

namespace CardTCLib.LuaBridge;

public class Events
{
    public delegate void ObCardActionDelegate(InGameCardBridge rec, InGameCardBridge? give);

    private Action _modLoadComplete;
    private readonly Dictionary<string, ObCardActionDelegate> _onActions = new();
    private readonly Dictionary<string, ObCardActionDelegate> _onEndActions = new();

    public void RegModLoadComplete(Action action)
    {
        _modLoadComplete += action;
    }

    public void RegOnAction(string key, ObCardActionDelegate action)
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

    public void RegOnEndAction(string key, ObCardActionDelegate action)
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
}