
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VolPro.Core.Utilities;

namespace VolPro.Core
{
    public static class LanguageContainer
    {
        private static readonly Dictionary<string, Dictionary<string, string>> LanguagePacks = new Dictionary<string, Dictionary<string, string>>();
        public static void Add(string lang, Dictionary<string, string> dic)
        {
            LanguagePacks[lang] = dic;
        }


        public static void AddEnglish(Dictionary<string, string> dic)
        {
            LanguagePacks[LangConst.英文] = dic;
        }
        public static void AddDeutsch(Dictionary<string, string> dic)
        {
            LanguagePacks[LangConst.俄语] = dic;
        }

        public static void AddIndonesian(Dictionary<string, string> dic)
        {
            LanguagePacks[LangConst.西班牙语] = dic;
        }
        /// <summary>
        /// 普通字符串翻译
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Translator(this string key)
        {
            if (key == null) return key;
            if (HttpContext.Current.Request.Headers.TryGetValue("lang", out StringValues langType))
            {
                if (langType != LangConst.简体中文 && LanguagePacks.TryGetValue(langType.ToString(), out Dictionary<string, string> lang))
                {
                    if (lang.TryGetValue(key, out string value))
                    {
                        return value;
                    }
                }
            }
            return key;
        }

        public static string TranslatorLang(this string key, string langType)
        {
            if (key == null) return key;

            if (langType != LangConst.简体中文 && LanguagePacks.TryGetValue(langType.ToString(), out Dictionary<string, string> lang))
            {
                if (lang.TryGetValue(key, out string value))
                {
                    return value; 
                }
            }
            return key;
        }
        /// <summary>
        ///  
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static string TranslatorArray(this IEnumerable<string> keys, string langType)
        {
            if (keys == null || keys.Count() == 0) return "";
            if (langType != LangConst.简体中文)
            {
                if (LanguagePacks.TryGetValue(langType.ToString(), out Dictionary<string, string> lang))
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var str in keys)
                    {
                        if (str != null)
                        {
                            if (lang.TryGetValue(str, out string value))
                            {
                                stringBuilder.Append(" " + value);
                            }
                            else
                            {
                                stringBuilder.Append(" " + str);
                            }
                        }
                    }
                    return stringBuilder.ToString();
                }
            }
            return string.Join("", keys);
        }
        /// <summary>
        /// 对整个数组内容进行翻译
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static string TranslatorArray(this IEnumerable<string> keys)
        {
            if (keys == null || keys.Count() == 0) return "";
            if (HttpContext.Current.Request.Headers.TryGetValue("lang", out StringValues langType) && langType != LangConst.简体中文)
            {
                if (LanguagePacks.TryGetValue(langType.ToString(), out Dictionary<string, string> lang))
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var str in keys)
                    {
                        if (lang.TryGetValue(str, out string value))
                        {
                            stringBuilder.Append(" " + value);
                        }
                        else
                        {
                            stringBuilder.Append(" " + str);
                        }
                    }
                    return stringBuilder.ToString();
                }
            }
            return string.Join("", keys);
        }
        /// <summary>
        /// 翻译替换内容，如：第{$ts}行,{$ts}不能为空，param=["1","xxx"]
        /// 翻译结果：第1行,xxx不能为空
        /// </summary>
        /// <param name="key"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string TranslatorFormat(this string key, params object[] param)
        {
            if (key == null || param == null || param.Length == 0) return key;
            bool zh = false;
            if (HttpContext.Current.Request.Headers.TryGetValue("lang", out StringValues langType))
            {
                if (langType != LangConst.简体中文)
                {
                    if (LanguagePacks.TryGetValue(langType.ToString(), out Dictionary<string, string> lang))
                    {
                        if (lang.TryGetValue(key, out string value))
                        {
                            return value.Format(false, param);
                        }
                    }
                }
                else
                {
                    zh = true;
                }
            }
            return key.Format(zh, param);
        }

        public static string TranslatorLangFormat(this string key, string langType, params object[] param)
        {
            if (key == null || param == null || param.Length == 0) return key;
            bool zh = false;

            if (langType != LangConst.简体中文)
            {
                if (LanguagePacks.TryGetValue(langType, out Dictionary<string, string> lang))
                {
                    if (lang.TryGetValue(key, out string value))
                    {
                        return value.Format(false, param);
                    }
                }
            }
            else
            {
                zh = true;
            }
            return key.Format(zh, param);
        }
        /// <summary>
        /// 翻译替换内容，如：第{$ts}行，翻译成Line{$ts}
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        ///  <param name="tsValue">是否翻译value值</param>
        /// <returns></returns>
        public static string TranslatorReplace(this string key, object value, bool tsValue = false)
        {
            if (key == null) return key;

            if (HttpContext.Current.Request.Headers.TryGetValue("lang", out StringValues langType))
            {
                return key.TranslatorReplace(value, langType.ToString(), tsValue);
            }
            return key.Replace("{$ts}", tsValue ? value?.ToString().Translator() : value?.ToString());
        }


        public static string TranslatorReplace(this string key, object value, string langType, bool tsValue = false)
        {
            if (key == null) return key;

            if (langType == LangConst.简体中文)
            {
                return key.Replace("{$ts}", value.ToString());
            }

            if (LanguagePacks.TryGetValue(langType, out Dictionary<string, string> lang))
            {
                if (lang.TryGetValue(key, out string _value))
                {
                    return _value.Replace("{$ts}", tsValue ? value?.ToString().Translator() : value?.ToString());
                }
            }
            return key.Replace("{$ts}", tsValue ? value?.ToString().Translator() : value?.ToString());
        }


        private static string Format(this string key, bool zh, params object[] param)
        {
            if (key == null) return key;
            string[] template = key.Split("{$ts}");
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < template.Length - 1; i++)
            {
                if (zh)
                {
                    builder.Append(template[i] + param[i]?.ToString());
                }
                else
                {
                    builder.Append(" " + template[i] + " " + param[i]?.ToString()?.Translator() + " ");
                }
            }
            builder.Append(template[template.Length - 1]);
            return builder.ToString();
        }


        public static LangType GetCurrentLangType(this Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            if (HttpContext.Current.Request.Headers.TryGetValue("lang", out StringValues langType))
            {
                if (langType == LangConst.英文)
                {
                    return LangType.英文;
                }
                else if (langType == LangConst.西班牙语)
                {
                    return LangType.西班牙语;

                }
                else if (langType == LangConst.俄语)
                {
                    return LangType.俄语;

                }
            };
            return LangType.简体中文;
        }

        public static string GetCurrentLangTypeString(this Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            if (HttpContext.Current.Request.Headers.TryGetValue("lang", out StringValues langType))
            {
                if (langType == LangConst.英文)
                {
                    return LangConst.英文;
                }
                 if (langType == LangConst.西班牙语)
                {
                    return LangConst.西班牙语;

                }
                 if (langType == LangConst.阿拉伯语)
                {
                    return LangConst.阿拉伯语;
                }
                 if (langType == LangConst.俄语)
                {
                    return LangConst.俄语;
                }
            };
            return LangConst.简体中文;
        }
    }

    public struct LangConst
    {
        public const string 简体中文 = "zh-cn";
        public const string 繁体中文 = "zh-tw";
        public const string 英文 = "en";
        public const string 法语 = "fr";
        public const string 西班牙语 = "es";
        public const string 阿拉伯语 = "ar";
        public const string 俄语 = "ru";
    }

    public enum LangType
    {
        简体中文 = 1,
        繁体中文,
        英文 ,
        法语,
        西班牙语,
        俄语,
        阿拉伯语
    }
}
