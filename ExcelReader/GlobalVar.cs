using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelReader
{
    public class GlobalVar
    {
        public static string con = ConfigurationManager.ConnectionStrings["Con"].ToString();
        public const string staffTB = "ORStaff";
        public const string seletedStaff = "[Id],[StaffDate],[RN],[NA],[Hour]";
        public const string convertStaffDate = "Convert(varchar(10),StaffDate,120)";
        public const string insertedStaff = "@StaffDate, @RN, @NA, @Hour";
        public const string dateFormate = "yyyy-MM-dd";
    }
}
