using System.Linq;
using CardTCLib.Util;
using UnityEngine;

namespace CardTCLib.LuaBridge;

public class UniqueIdObjectBridge(UniqueIDScriptable uniqueIDScriptable)
{
    public readonly UniqueIDScriptable UniqueIDScriptable = uniqueIDScriptable;

    public float this[string key]
    {
        get
        {
            if (UniqueIDScriptable is CardData cardData) return cardData.GetFloatValue(key);

            return 0;
        }
        set
        {
            if (UniqueIDScriptable is CardData cardData)
            {
                var timeObjective = cardData.TimeValues.FirstOrDefault(objective => objective.ObjectiveName == key);
                if (timeObjective == null)
                {
                    timeObjective = new TimeObjective
                    {
                        ObjectiveName = key
                    };
                    cardData.TimeValues.Add(timeObjective);
                }
                
                timeObjective.Value = Mathf.RoundToInt(value *1000);
            }
        }
    }
}