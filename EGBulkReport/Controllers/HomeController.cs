using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EGBulkReport.Controllers
{
    public class HomeController : Controller
    {
        
        public JsonResult DataSource()
        {
            List<dynamic> lst = new List<dynamic>();
            using (Oracle.ManagedDataAccess.Client.OracleConnection conn = new Oracle.ManagedDataAccess.Client.OracleConnection("data source=192.168.1.101/xe;user id=eagle;password=maithee"))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "select * from dumpdata";
                Oracle.ManagedDataAccess.Client.OracleDataAdapter da = new Oracle.ManagedDataAccess.Client.OracleDataAdapter(cmd);
                System.Data.DataTable dt = new System.Data.DataTable();
                da.Fill(dt);
                var qry = from System.Data.DataRow row in dt.Rows
                          select new
                          {
                              Id = row["ID"],
                              Name = row["NAME"],
                              Salary = row["SALARY"]
                          };
                lst = qry.ToList<dynamic>();
                conn.Close();
            }

            return new JsonResult()
            {
                Data = lst,
                MaxJsonLength = 50000000,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        public ActionResult Index()
        {

            return View();
        }
    }
}