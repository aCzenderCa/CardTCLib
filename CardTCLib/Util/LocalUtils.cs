using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ModLoader;

namespace CardTCLib.Util;

public static class LocalUtils
{
    public static bool CurrentLangIsChinese =>
        LocalizationManager.Instance.Languages[LocalizationManager.CurrentLanguage].LanguageName == "简体中文";

    public static readonly Dictionary<string, string> ChineseTexts = new();

    public static void InitChineseTextsIfNeed()
    {
        if (ChineseTexts.Count == 0 && !CurrentLangIsChinese)
        {
            var chineseSetting =
                LocalizationManager.Instance.Languages.FirstOrDefault(setting => setting.LanguageName == "简体中文");
            var dictionary = CSVParser.LoadFromString(chineseSetting.GetLocalizationString());
            var regex = new Regex(@"\\n");
            foreach (var (key, list) in dictionary)
            {
                if (!ChineseTexts.ContainsKey(key) && list.Count >= 2)
                    ChineseTexts.Add(key, regex.Replace(list[1], "\n"));
            }
        }
    }

    public static string Chinese(this LocalizedString localizedString)
    {
        InitChineseTextsIfNeed();
        if (CurrentLangIsChinese)
        {
            return localizedString;
        }

        if (ChineseTexts.TryGetValue(localizedString.LocalizationKey, out var text))
        {
            return text;
        }
                
        return localizedString.DefaultText;
    }
}