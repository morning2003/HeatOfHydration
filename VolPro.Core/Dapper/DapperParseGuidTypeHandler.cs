using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using VolPro.Core.Const;
using VolPro.Core.Enums;

namespace VolPro.Core.Dapper
{
    public class DapperParseGuidTypeHandler
    {
        public static void InitParseGuid()
        {
            if (DBType.Name == DbCurrentType.MySql.ToString())
            {
                SqlMapper.AddTypeHandler(new DapperParseGuidTypeHandlerMySql());
                SqlMapper.RemoveTypeMap(typeof(Guid?));
            }
            else if (DBType.Name == DbCurrentType.Oracle.ToString())
            {
                SqlMapper.AddTypeHandler(new DapperParseGuidTypeHandlerOracle());
                SqlMapper.RemoveTypeMap(typeof(Guid?));
            }
        }
    }
}
