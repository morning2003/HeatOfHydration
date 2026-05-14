using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolPro.Core.Print
{
    public class PrintQuery
    {
        public object[] Ids { get; set; }
        public Guid TemplateId { get; set; }
        /// <summary>
        /// 打印模板名称
        /// </summary>
        public string TemplateName { get; set; }
        public string Table { get; set; }

        /// <summary>
        /// 是否批量打印主表与明细表数据
        /// </summary>
        public bool BatchMainAndDetail { get; set; }

        /// <summary>
        /// 是否批量打印主表数据
        /// </summary>
        public bool BatchMain { get; set; }

        /// <summary>
        /// 是否打印明细表数据
        /// </summary>
        public bool Detail { get; set; }
    }
}
