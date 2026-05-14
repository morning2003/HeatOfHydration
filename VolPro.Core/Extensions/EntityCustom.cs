using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolPro.Core.ManageUser;
using VolPro.Entity.DomainModels;

namespace VolPro.Core.Extensions
{

    public static class EntityCustom
    {
        private static string[] DefaultFields = null;// new string[] { "你的字段1", "你的字段2" };

        /// <summary>
        /// 功能作用：给所有表包括DefaultFields数组的字段都统一设置默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public static void SetValue<T>(this T entity) where T : class
        {
            if (DefaultFields == null) return;
            var properties = typeof(T).GetProperties().Where(x => DefaultFields.Contains(x.Name));
            if (!properties.Any()) return;

            //如果某些表有相同的字段但不需要设置值，在这此执行
            //if (nameof(T)==nameof(表model))
            //{
            //    return;
            //}

            //在这里给表对象设置默认值
            foreach (var item in properties)
            {
                //UserContext.Current.UserInfo获取用户信息，也可以在userinfo里面加其他字段
                //usercontext是已缓存的对象，尽量不要在这里查询数据库
                switch (item.Name)
                {
                    case "你的字段1":
                        //可以从UserContext.Current.UserInfo取值
                        item.SetValue(entity, "你的字段1值");
                        break;
                    case "你的字段2":
                        item.SetValue(entity, "你的字段2值");
                        break;
                    default:
                        break;
                }

            }
        }
    }
}
