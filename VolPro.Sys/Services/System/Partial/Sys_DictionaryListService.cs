using VolPro.Core.BaseProvider;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;
using System.Linq;
using VolPro.Core.Extensions;
using System.Collections.Generic;
using VolPro.Core.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using VolPro.Sys.IRepositories;

namespace VolPro.Sys.Services
{
    public partial class Sys_DictionaryListService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISys_DictionaryListRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public Sys_DictionaryListService(
            ISys_DictionaryListRepository dbRepository,
            IHttpContextAccessor httpContextAccessor
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
        }
        public override PageGridData<Sys_DictionaryList> GetPageData(PageDataOptions pageData)
        {
            base.OrderByExpression = x => new Dictionary<object, QueryOrderBy>() { {
                    x.OrderNo,QueryOrderBy.Desc
                },
                {
                    x.DicList_ID,QueryOrderBy.Asc
                }
            };
            return base.GetPageData(pageData);
        }
    }
}

