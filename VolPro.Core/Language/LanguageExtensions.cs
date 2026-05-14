using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using VolPro.Entity.DomainModels;
using VolPro.Core.EFDbContext;
using VolPro.Core.Configuration;
using VolPro.Core.Language;
using VolPro.Core.Utilities;
using VolPro.Core.Extensions;
using VolPro.Core.DBManage;
using VolPro.Core.Const;
using VolPro.Core.DbSqlSugar;
using SqlSugar;

namespace VolPro.Core.Language
{
    public static class LanguageExtensions
    {
        public static IApplicationBuilder UseLanguagePack(this IApplicationBuilder app)
        {


            try
            {
                CreateLanguagePack(DbManger.SysDbContext);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return app;

        }
        public static void CreateLanguagePack(this ISqlSugarClient context)
        {
            //FormattableString sql = null;
           //var context = DbManger.SysDbContext;
            var lang = context.Set<Sys_Language>()
                .Select(s => new
                {
                    s.IsPackageContent,
                    ZHCN = s.ZHCN.Trim(),//简体中文
                    s.ZHTW,//繁体中文
                    s.English,//英文
                    s.Spanish,//西班牙语
                    s.French,//法语
                    s.Arabic,  //阿拉伯语
                    s.Russian //俄语
                }).ToList()
            .GroupBy(x => x.ZHCN)
            .Select(s => new
            {
                ZHCN = s.Key.Trim(),
                ZHTW = s.Max(m => m.ZHTW)?.Trim(),
                English = s.Max(m => m.English)?.Trim(),
                French = s.Max(m => m.French)?.Trim(),
                Spanish = s.Max(m => m.Spanish)?.Trim(),
                Arabic = s.Max(m => m.Arabic)?.Trim(),
                Russian = s.Max(m => m.Russian)?.Trim()
            }).ToList().GroupBy(x => x.ZHCN).Select(s => new
            {
                ZHCN = s.Key.Trim(),
                ZHTW = s.Max(m => m.ZHTW)?.Trim(),
                English = s.Max(m => m.English)?.Trim(),
                French = s.Max(m => m.French)?.Trim(),
                Spanish = s.Max(m => m.Spanish)?.Trim(),
                Arabic = s.Max(m => m.Arabic)?.Trim(),
                Russian = s.Max(m => m.Russian)?.Trim()
            }).ToList();

            string path = AppSetting.FullStaticPath;

            var lang_zhtw = lang.Where(c => !string.IsNullOrEmpty(c.ZHTW))
            .Select(s => new KeyValuePair<string, string>(s.ZHCN, s.ZHTW))
            .ToDictionary(x => x.Key, x => x.Value);
            LanguageContainer.Add(LangConst.繁体中文, lang_zhtw);

            FileHelper.WriteFile(path, LangConst.繁体中文 + ".js", $"{lang_zhtw.Serialize()}");


            var lang_english = lang.Where(c => !string.IsNullOrEmpty(c.English))
            .Select(s => new KeyValuePair<string, string>(s.ZHCN, s.English))
            .ToDictionary(x => x.Key, x => x.Value);
            LanguageContainer.Add(LangConst.英文, lang_english);

            FileHelper.WriteFile(path, LangConst.英文 + ".js", $"{lang_english.Serialize()}");

            var lang_french = lang.Where(c => !string.IsNullOrEmpty(c.French))
              .Select(s => new KeyValuePair<string, string>(s.ZHCN, s.French))
              .ToDictionary(x => x.Key, x => x.Value);
            LanguageContainer.Add(LangConst.法语, lang_french);

            FileHelper.WriteFile(path, LangConst.法语 + ".js", $"{lang_french.Serialize()}");


            var lang_spanish = lang.Where(c => !string.IsNullOrEmpty(c.Spanish))
            .Select(s => new KeyValuePair<string, string>(s.ZHCN, s.Spanish))
            .ToDictionary(x => x.Key, x => x.Value);
            LanguageContainer.Add(LangConst.西班牙语, lang_spanish);

            FileHelper.WriteFile(path, LangConst.西班牙语 + ".js", $"{lang_spanish.Serialize()}");

            var lang_arabic = lang.Where(c => !string.IsNullOrEmpty(c.Arabic))
            .Select(s => new KeyValuePair<string, string>(s.ZHCN, s.Arabic))
            .ToDictionary(x => x.Key, x => x.Value);
            LanguageContainer.Add(LangConst.阿拉伯语, lang_arabic);

            FileHelper.WriteFile(path, LangConst.阿拉伯语 + ".js", $"{lang_arabic.Serialize()}");

            var lang_ru = lang.Where(c => !string.IsNullOrEmpty(c.Russian))
            .Select(s => new KeyValuePair<string, string>(s.ZHCN, s.Russian))
            .ToDictionary(x => x.Key, x => x.Value);
            LanguageContainer.Add(LangConst.俄语, lang_ru);

            FileHelper.WriteFile(path, LangConst.俄语 + ".js", $"{lang_ru.Serialize()}");

            //其他语言包，请在此处接着添加

        }
    }

}
