using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolPro.Core.CacheManager;
using VolPro.Core.DBManager;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Core.Utilities;
using VolPro.Entity.DomainModels;

namespace VolPro.Core.UserManager
{
    public static class DepartmentContext
    {
        static DepartmentContext()
        {
            GetAllDept();
        }
        /// <summary>
        /// 从数据库获取菜单时锁定的对象
        /// </summary>
        private static object _deptObj = new object();

        /// <summary>
        /// 当前服务器的菜单版本
        /// </summary>
        private static string _deptVersionn = "";

        public const string _deptCacheKey = "inernalDept";
        private static List<Dept> _depts { get; set; }
        public static ICacheService CacheContext
        {
            get
            {
                return AutofacContainerModule.GetService<ICacheService>();
            }
        }

        public static List<Dept> GetAllDept()
        {
            //每次比较缓存是否更新过，如果更新则重新获取数据
            string _cacheVersion = CacheContext.Get(_deptCacheKey);
            if (_deptVersionn != "" && _deptVersionn == _cacheVersion)
            {
                return _depts ?? new List<Dept>();
            }
            lock (_deptObj)
            {
                if (_deptVersionn != "" && _depts != null && _deptVersionn == _cacheVersion)
                {
                    return _depts;
                }

                _depts = DBServerProvider.DbContext.Set<Sys_Department>()
                    .Select(s => new Dept()
                    {
                        id = s.DepartmentId,
                        key = s.DepartmentId,
                        value = s.DepartmentName,
                        parentId = s.ParentId,
                        dbServiceId = s.DbServiceId
                    }).ToList();

                string cacheVersion = CacheContext.Get(_deptCacheKey);
                if (string.IsNullOrEmpty(cacheVersion))
                {
                    cacheVersion = DateTime.Now.ToString("yyyyMMddHHMMssfff");
                    CacheContext.Add(_deptCacheKey, cacheVersion);
                }
                else
                {
                    _deptVersionn = cacheVersion;
                }
            }
            return _depts;
        }


        public static List<Guid> GetAllChildrenIds([NotNull] List<Guid> ids)
        {
            ids = ids.Distinct().ToList();
            if (ids.Count == 0)
            {
                return new List<Guid>() { Guid.NewGuid() };
            }

            for (int i = 0; i < ids.Count(); i++)
            {
                Guid id = ids[i];
                var list = _depts.Where(x => x.parentId == id && !ids.Contains(x.id)).Select(s => s.id).Distinct().ToList();
                if (list.Count > 0)
                {
                    ids.AddRange(list);
                }
            }

            return ids;
        }
        public static List<Guid> GetAllChildrenIds(Guid id)
        {
            return GetAllChildrenIds(new List<Guid>() { id });
        }


        public static WebResponseContent Reload(this WebResponseContent webResponse)
        {
            if (webResponse.Status)
            {
                CacheContext.Remove(_deptCacheKey);
                GetAllDept();
            }
            return webResponse;
        }
    }

}
public partial class Dept
{
    public Guid id { get; set; }
    public Guid? parentId { get; set; }
    public Guid key { get; set; }
    public string value { get; set; }

    public Guid? dbServiceId { get; set; }
}
