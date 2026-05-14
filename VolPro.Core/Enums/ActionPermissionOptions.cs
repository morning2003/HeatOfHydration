using System;
using System.Collections.Generic;
using System.Text;

namespace VolPro.Core.Enums
{
    public enum ActionPermissionOptions
    {
        /// <summary>
        /// 注意添加的枚举值一定要是前面的值倍数，即x2
        /// </summary>
        None=0,
        Add = 1,
        Delete = 2,
        Update = 4,
        Search=8,
        Export=16,
        Audit=32,
        Upload=64,//上传文件
        Import=128 //导入表数据Excel
    }
}
