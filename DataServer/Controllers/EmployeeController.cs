using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Http.Cors;
using Oracle.ManagedDataAccess.Client;
using System.Net.Http;

namespace DataServer.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")] // tune to your needs
    public class EmployeeController : ApiController
    {
        protected Models.DataTableRequest GetRequest()
        {
            var httpRequest = HttpContext.Current.Request;
            Models.DataTableRequest req = new Models.DataTableRequest()
            {
                Draw = Convert.ToInt32(httpRequest["draw"]),
                Start = Convert.ToInt32(httpRequest["start"]),
                Length = Convert.ToInt32(httpRequest["length"]),
                Search = new Models.Search()
                    {
                        Value = httpRequest["search[value]"],
                        Regex = Convert.ToBoolean(httpRequest["search[regex]"])
                    }
                };

            req.Order = new List<Models.Order>();
            int i = 0;
            string sclm = httpRequest[$"order[{i}][column]"];
            while (sclm != null)
            {
                Models.Order order = new Models.Order()
                {
                    Column = httpRequest[$"columns[{sclm}][data]"],
                    Dir = httpRequest[$"order[{i}][dir]"]
                };
                req.Order.Add(order);
                i++;
                sclm = httpRequest[$"order[{i}][column]"];
            }

            return req;

        }
        public IHttpActionResult Post()
        {

            var request = this.GetRequest();

            var totalRecords = 0;
            var dataPage = new List<dynamic>();

            using (OracleConnection conn = new OracleConnection("Data Source=192.168.1.101/XE;User ID=eagle;Password=maithee"))
            {
                conn.Open();
                var cmd = conn.CreateCommand();

                #region Get Where filter
                string sqlWhere="";
                if (request.Search.Value != "")
                {
                    List<string> swhere = new List<string>();
                    var stokens = request.Search.Value.Trim().Split();
                    foreach(var token in stokens)
                    {
                        swhere.Add($"lower(name) like lower('%{token}%')");
                    }
                    sqlWhere = "(" + string.Join(" Or ", swhere) + ")";
                }
                #endregion

                #region Get Order Column SQL
                string sqlOrder = string.Join(",", request.Order.Select(odr => odr.Column + " " + odr.Dir));
                if (sqlOrder != "")
                {
                    cmd.CommandText += " Order by " + sqlOrder;
                }
                #endregion

                #region Get Total Records
                cmd.CommandText = "select count(*) from dumpdata";
                if (sqlWhere != "")
                {
                    cmd.CommandText += " Where " + sqlWhere;
                }
                object o = cmd.ExecuteScalar();
                totalRecords = Convert.ToInt32(o);
                #endregion

                if (request.Length == -1)
                {
                    cmd.CommandText = $"SELECT id,name,salary FROM dumpdata";
                    if (sqlWhere != "")
                    {
                        cmd.CommandText += " Where " + sqlWhere;
                    }
                    if (sqlOrder != "")
                    {
                        cmd.CommandText += " Order By " + sqlOrder;
                    }
                }
                else
                {
                    if (sqlOrder != "")
                    {
                        if (sqlWhere != "")
                        {
                            cmd.CommandText = $"SELECT id,name,salary FROM (SELECT id,name,salary,row_number() over (order by {sqlOrder}) rnk FROM dumpdata Where {sqlWhere}) WHERE rnk BETWEEN {request.Start + 1} AND {request.Start + request.Length}";
                        }
                        else
                        {
                            cmd.CommandText = $"SELECT id,name,salary FROM (SELECT id,name,salary,row_number() over (order by {sqlOrder}) rnk FROM dumpdata) WHERE rnk BETWEEN {request.Start + 1} AND {request.Start + request.Length}";
                        }
                    }
                    else
                    {
                        if (sqlWhere != "")
                        {
                            cmd.CommandText = $"SELECT id,name,salary FROM (SELECT id,name,salary,row_number() over (order by id) rnk FROM dumpdata Where {sqlWhere}) WHERE rnk BETWEEN {request.Start + 1} AND {request.Start + request.Length}";
                        }
                        else
                        {
                            cmd.CommandText = $"SELECT id,name,salary FROM (SELECT id,name,salary,row_number() over (order by id) rnk FROM dumpdata) WHERE rnk BETWEEN {request.Start + 1} AND {request.Start + request.Length}";
                        }
                    }
                }


                OracleDataAdapter da = new OracleDataAdapter(cmd);
                System.Data.DataTable dt = new System.Data.DataTable();
                da.Fill(dt);
                var qry = from System.Data.DataRow d in dt.Rows
                          select new
                          {
                              ID = d["ID"],
                              Name = d["Name"],
                              Salary = d["Salary"]
                          };

                dataPage = qry.ToList<dynamic>();

                conn.Close();
            }

            return Json(new { 
                draw = request.Draw,
                recordsTotal = totalRecords,
                recordsFiltered = totalRecords,
                data = dataPage
            });

        }

        public JsonResult Get()
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
    }
}