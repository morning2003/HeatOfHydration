using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VolPro.Core.Controllers.Basic;
using VolPro.Core.DBManager;
using VolPro.Core.DbSqlSugar;
using VolPro.Core.Enums;
using VolPro.Core.Extensions;
using VolPro.Core.Filters;
using VolPro.Core.ManageUser;
using VolPro.Core.Utilities;
using VolPro.Entity.AttributeManager;
using VolPro.Entity.DomainModels;

namespace VolPro.WebApi.Controllers.DataView
{
    [Route("api/dataview/project")]
    [PermissionTable(Name = nameof(DataViewProjects))]
    public class DataViewProjectController : VolController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public DataViewProjectController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        [Route("list"), HttpGet]
        public async Task<IActionResult> GetList(int page = 1, int limit = 12)
        {
           var query = DBServerProvider.DbContext.Set<DataViewProjects>().Where(x => x.IsDel == 0);
            var data = await query.OrderByDescending(x => x.OrderNo)
                   .ThenByDescending(x => x.CreateDate)
                   .TakePage(page, limit)
                  .ToListAsync();
            return Json(new
            {
                code = 200,
                msg = "获取成功",
                count = await query.CountAsync(),
                data = data
            });
        }

        [Route("create"), HttpPost]
        [ApiActionPermission(ActionPermissionOptions.Add | ActionPermissionOptions.Update)]
        public IActionResult Create([FromBody] DataViewProjects data)
        {
            var id = new IdWorker();
            data.Id = id.NextId();

            return Success("获取成功", data);
        }


        [Route("getData"), HttpGet]
        public async Task<IActionResult> GetData(long projectId)
        {
            var data = await DBServerProvider.DbContext.Set<DataViewProjects>().Where(x => x.Id == projectId).FirstOrDefaultAsync();
            if (data == null)
            {
                return Success();
            }
            return Success("获取成功", data);
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [Route("save/data"), HttpPost]
        [ApiActionPermission(ActionPermissionOptions.Add | ActionPermissionOptions.Update)]
        public IActionResult SaveData(IFormCollection formdata)
        {
            DataViewProjects data = new DataViewProjects();
            data.Id = Convert.ToInt64(formdata["projectId"]);
            data.Content = formdata["Content"];
            return UpdateData(data, x => new { x.Content });
        }
        /// <summary>
        /// 编辑项目名称
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [Route("edit"), HttpPost]
        [ApiActionPermission(ActionPermissionOptions.Add | ActionPermissionOptions.Update)]
        public IActionResult Edit([FromBody] DataViewProjects data)
        {
            data = HttpContext.GetRequestParameters().DeserializeObject<DataViewProjects>();
            return UpdateData(data, x => new { x.ProjectName });
        }

        [Route("publish"), HttpPost, HttpPut]
        [ApiActionPermission(ActionPermissionOptions.Add | ActionPermissionOptions.Update)]
        public IActionResult Publish([FromBody] DataViewProjects data)
        {
            data = HttpContext.GetRequestParameters().DeserializeObject<DataViewProjects>();
            return UpdateData(data, x => new { x.State });
        }

        private static object dataviewLockObject = new object();
        private IActionResult UpdateData(DataViewProjects data, Expression<Func<DataViewProjects, object>> updateField)
        {
            lock (dataviewLockObject)
            {
                var dbContext = DBServerProvider.DbContext.Set<DataViewProjects>();
                var dataView = dbContext.Where(x => x.Id == data.Id).FirstOrDefault();
                if (dataView != null)
                {
                    var arr = updateField.GetExpressionToArray();
                    //foreach (var field in arr)
                    //{
                    //    DBServerProvider.DbContext.Entry<DataViewProjects>(data).Property(field).IsModified = true;
                    //}
                    DBServerProvider.DbContext.Update(data, arr);
                }
                else
                {
                    data.IsDel = 0;
                    data.SetCreateDefaultVal();
                    data.DbServiceId = UserContext.CurrentServiceId;
                    DBServerProvider.DbContext.Add(data);
                }
                DBServerProvider.DbContext.SaveChanges();
            }
            return Success();
        }


        [Route("upload"), HttpPost]

        public IActionResult Upload(List<IFormFile> fileInput)
        {
            string fullPath = "dataView".MapPath(true);
            try
            {
                if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);
                for (int i = 0; i < fileInput.Count; i++)
                {
                    string fileName = fileInput[i].FileName;
                    string id = fileInput[i].FileName.Split("_")[0];
                    using (var stream = new FileStream(fullPath + "\\" + fileName, FileMode.Create))
                    {
                        fileInput[i].CopyTo(stream);
                    }
                    DataViewProjects data = new DataViewProjects()
                    {
                        Id = Convert.ToInt64(id),
                        IndexImage = "dataView\\" + fileName
                    };
                    UpdateData(data, x => new { x.IndexImage });
                }
                return Json(new
                {
                    code = 200,
                    msg = "ok",
                    data = new { fileurl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/dataView/{ fileInput[0].FileName}" }
                });
            }
            catch (Exception ex)
            {
                return Success(ex.Message);
            }
        }


        /// <summary>
        /// 编辑项目名称
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [Route("delete"), HttpDelete,HttpGet,HttpPost]
        [ApiActionPermission(ActionPermissionOptions.Add | ActionPermissionOptions.Update | ActionPermissionOptions.Delete)]
        public IActionResult Delete(long ids)
        {
            DataViewProjects dataView = new DataViewProjects() { Id = ids, IsDel = 1 };
            DBServerProvider.DbContext.Update(dataView,x=>new { x.IsDel });
            DBServerProvider.DbContext.SaveChanges();
            return Success("删除成功");
        }

        [Route("updateOrderNo"), HttpPost]
        [ApiActionPermission(ActionPermissionOptions.Add | ActionPermissionOptions.Update)]
        public IActionResult UpdateOrderNo([FromBody] DataViewProjects data)
        {
            data = HttpContext.GetRequestParameters().DeserializeObject<DataViewProjects>();
            UpdateData(data, x => new { x.OrderNo, x.ProjectName });
            return Success("修改成功");
        }


        [Route("copy"), HttpPost]
        [ApiActionPermission(ActionPermissionOptions.Add | ActionPermissionOptions.Update)]
        public async Task<IActionResult> Copy([FromBody] DataViewProjects data)
        {
            data = HttpContext.GetRequestParameters().DeserializeObject<DataViewProjects>();
            long dataId= data.Id;
            data = await DBServerProvider.DbContext.Set<DataViewProjects>().Where(x => x.Id == data.Id).FirstOrDefaultAsync();
            if (data==null)
            {
                string msg = $"未查到id数据：{dataId}";
                Console.WriteLine(msg);
                return Success(msg);
            }
            var id = new IdWorker();
            data.Id = id.NextId();
            data.ProjectName = data.ProjectName + "副本";
            data.SetCreateDefaultVal();
            DBServerProvider.DbContext.Add(data);
            DBServerProvider.DbContext.SaveChanges();
            return Success("修改成功");
        }

        private new IActionResult Success(string msg = "ok")
        {
            return Json(new
            {
                code = 200,
                msg = msg
            });
        }

        private new IActionResult Success(string msg, object data)
        {

            return Json(new
            {
                code = 200,
                msg = msg,
                data = data
            });
        }
    }
}
