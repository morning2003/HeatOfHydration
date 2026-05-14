using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace VolPro.Core.Print
{
    public class PrintOptions
    {
        public Type TableType { get; set; }
        public string Name { get; set; }
        public string TableName { get; set; }

        public string[] Fields { get; set; }

        public List<CustomField> CustomFields { get; set; }

        //public Type DetailTableType { get; set; }

        //public string DetailName { get; set; }

        //public string DetailTableName { get; set; }

        //public string[] DetailFields { get; set; }
        //public List<CustomField> DetailCustomgFields { get; set; }

        public List<PrintDetail> PrintDetails { get; set; }



    }

    public class PrintDetail
    {
        public Type DetailTableType { get; set; }

        public string DetailName { get; set; }

        public string DetailTableName { get; set; }

        public string[] DetailFields { get; set; }

        public List<CustomField> CustomgFields { get; set; }
    }

    public class PrintFields
    {
        public string Field { get; set; }
        public string Name { get; set; }
    }

    public class PrintDetailOptions<TPrint> where TPrint : class
    {
        public string Name { get; set; }
        public Expression<Func<TPrint, object>> PrintFields { get; set; }
        public List<CustomField> CustomFields { get; set; }
    }
}
