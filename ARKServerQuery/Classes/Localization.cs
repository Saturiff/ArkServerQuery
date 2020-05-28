using ARKServerQuery.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ARKServerQuery.Classes
{
    public static class Localization
    {
        public static void Load()
        {
            LoadLanguageFile(Settings.Default.customLanguage);
        }

        public static void Update(int index)
        {
            LoadLanguageFile(GetEnumByOrder(index));
            Settings.Default.customLanguage = GetEnumByOrder(index);
            Settings.Default.Save();
        }

        private static void LoadLanguageFile(LanguageList language)
            => Application.Current.Resources.MergedDictionaries[0] = new ResourceDictionary()
            {
                Source = new Uri(languageFilePath[language], UriKind.RelativeOrAbsolute)
            };

        private static readonly Dictionary<LanguageList, string> languageFilePath = new Dictionary<LanguageList, string>()
        {
            { LanguageList.zh_tw, @"/Lang/zh-tw.xaml" },
            { LanguageList.zh_cn, @"/Lang/zh-cn.xaml" },
            { LanguageList.en_us, @"/Lang/en-us.xaml" }
        };

        private static LanguageList GetEnumByOrder(int index)
            => Enum.GetValues(typeof(LanguageList)).Cast<LanguageList>().Select((x, i)
            => new { item = x, index = i }).Single(x => x.index == index).item;

        private enum LanguageList { zh_tw = 0, zh_cn = 1, en_us = 2 }
    }
}
