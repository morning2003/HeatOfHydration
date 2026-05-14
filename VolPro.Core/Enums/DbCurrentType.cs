using System;
using System.Collections.Generic;
using System.Text;

namespace VolPro.Core.Enums
{
    public enum DbCurrentType
    {
        Default = 0,
        MySql = 1,
        MsSql = 2,//2020.08.08修改sqlserver拼写
        PgSql = 3,
        Kdbndp,//人大金仓
        Oracle, //2022.12.26
        DM, //2024.02.27
    }
}
