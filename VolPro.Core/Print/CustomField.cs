using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolPro.Core.Print
{
    /// <summary>
    /// 自定义字段，某些字段不在当前表，可以预先自定义字段，在PrintCustom类QueryResult字自定义返回这些字段的值
    /// </summary>
    public class CustomField
    {
        /// <summary>
        /// 列显示名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 字段
        /// </summary>
        public string Field { get; set; }
    }
}
