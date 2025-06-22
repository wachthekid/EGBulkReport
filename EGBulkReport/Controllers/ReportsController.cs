using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DevExpress.DataAccess.Json;
using Oracle.ManagedDataAccess.Client;

namespace EGBulkReport.Controllers
{
    public class ReportsController : BaseController
    {
        // GET: Reports
        public ActionResult Report1()
        {
            /*OracleConnectionStringBuilder sb = new OracleConnectionStringBuilder();
            sb.DataSource = "192.168.1.101/XE";
            sb.UserID = "EAGLE";
            sb.Password = "maithee";
            OracleConnection conn = new OracleConnection(sb.ToString());
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "select * from dumpdata";
            OracleDataAdapter da = new OracleDataAdapter(cmd);
            System.Data.DataTable dt = new System.Data.DataTable();
            da.Fill(dt);
            conn.Close();

            ViewBag.DataSource = dt;*/

            ViewBag.StartTime = DateTime.UtcNow;

            var jsonDataSource = new JsonDataSource();
            Uri dataUri = new Uri("http://localhost:61260/Home/DataSource", UriKind.Absolute);
            jsonDataSource.JsonSource = new UriJsonSource(dataUri);
            jsonDataSource.Fill();

            ViewBag.DataSource = jsonDataSource;

            return View();
        }

        public ActionResult DataTable()
        {
            return View();
        }
    }
}