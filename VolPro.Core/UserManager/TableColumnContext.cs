using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolPro.Core.DBManager;
using VolPro.Entity.DomainModels;

namespace VolPro.Core.UserManager
{

    public static class TableColumnContext
    {
        private static object _colObject = new object();
        private static List<TableColumnField> _data = null;

        public static List<TableColumnField> Data
        {
            get
            {
                if (_data == null)
                {
                    lock (_colObject)
                    {
                        if (_data == null)
                        {
                            _data = DBServerProvider.DbContext.Set<Sys_TableColumn>().Select(s => new TableColumnField
                            {
                                ColumnCnName = s.ColumnCnName,
                                ColumnName = s.ColumnName,
                                ColumnType = s.ColumnType,
                                IsDisplay = s.IsDisplay ?? 1,
                                TableName = s.TableName

                            }).ToList();
                        }
                    }
                }
                return _data;
            }
        }
        /// <summary>
        /// 获取表隐藏的字段
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<string> GetTableHideFields(string table)
        {
             return Data.Where(x => x.TableName == table&&x.IsDisplay==0).Select(s=>s.ColumnName).ToList(); 
        }

    }

    public class TableColumnField
    {
        /// <summary>
        ///
        /// <summary>
        [Display(Name = "")]
        [Editable(true)]
        public string ColumnName { get; set; }

        /// <summary>
        ///
        /// <summary>
        [Display(Name = "")]
        [Editable(true)]
        public string ColumnCnName { get; set; }

        /// <summary>
        ///
        /// <summary>
        [Display(Name = "")]
        [Editable(true)]
        public string ColumnType { get; set; }

        /// <summary>
        ///
        /// <summary>
        [Display(Name = "")]
        [Editable(true)]
        public string TableName { get; set; }

        /// <summary>
        ///
        /// <summary>
        [Display(Name = "")]
        [Column(TypeName = "int")]
        [Editable(true)]
        public int? IsDisplay { get; set; }
    }
}
