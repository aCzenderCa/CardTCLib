using System;
using System.Collections;
using System.Collections.Generic;
using CardTCLib.Util;
using NLua;

namespace CardTCLib.LuaBridge;

public class CoroutineHelper(Lua lua)
{
    public Lua Lua = lua;

    public void BatchEnumerators(Action<List<IEnumerator>> action)
    {
        var enumerators = new List<IEnumerator>();
        action(enumerators);

        if (enumerators.Count > 0)
        {
            var enumerator = enumerators[0];
            for (var i = 1; i < enumerators.Count; i++)
            {
                enumerator = enumerator.Then(enumerators[i]);
            }

            CoUtils.StartCoWithBlockAction(enumerator);
        }
    }
}