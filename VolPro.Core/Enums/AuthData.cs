using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolPro.Core.Enums
{
    public enum AuthData
    {
        全部 = 1,
        本组织与本角色以及下数据 = 10,
        本组织及下数据 = 20,
        本组织数据 = 30,
        本角色以及下数据 = 40,
        本角色数据 = 50,
        仅自己数据 = 60
    }
}
