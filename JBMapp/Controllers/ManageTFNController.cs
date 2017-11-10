using Core.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
using System.Data.OleDb;
using JBMapp.ADUC;


namespace JBMapp.Controllers
{
    public class ManageTFNController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            ActiveDirectoryWeb ad = new ActiveDirectoryWeb();
            string[] usr = ad.SearchActiveDirectory(HttpContext.User.Identity.Name).Split('|');
            ViewBag.IsAdmin = DataAccess.AdminAccess(usr[1]);

            return View();
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {
            ActiveDirectoryWeb ad = new ActiveDirectoryWeb();
            string[] usr = ad.SearchActiveDirectory(HttpContext.User.Identity.Name).Split('|');
            ViewBag.IsAdmin = DataAccess.AdminAccess(usr[1]);

            string logI = HttpContext.User.Identity.Name;
            string errMsg;
            DataTable ret = new DataTable();
            int proc;
            try
            {
                ret = TFNAlert(file, out errMsg);
                if (errMsg == null)
                {
                    proc = DataAccess.InsertTFNAlert(ret, logI.Split('\\').Last());
                }
                else
                {
                    ViewData["ProcessError"] = errMsg;
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewData["ProcessError"] = ex.Message;
                return View();
            }

            ViewData["ProcessSuccess"] = Url.Encode("TFN Succesfully Processed!");
            return View();
        }

        public DataTable TFNAlert(HttpPostedFileBase file, out string errMsg)
        {
            DataTable ret = new DataTable();
            if (Request.Files.Count > 0)
            {
                if (file != null && file.ContentLength > 0)
                {
                    var fName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/MediaFiles/"), fName);
                    file.SaveAs(path);
                    string conStr;
                    try
                    {
                        string ext = Path.GetExtension(path);
                        if (ext.ToLower().Trim() == ".xls")
                        {
                            conStr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                           
                            ret = LoadExcel(path, conStr,out errMsg);


                            return ret;

                        }
                        else if (ext.ToLower().Trim() == ".xlsx")
                        {
                            conStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                           
                            ret = LoadExcel(path, conStr,out errMsg);

                            return ret;
                        }
                        else
                        {
                            errMsg = "Invalid File!";
                            return null;
                        }
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
            errMsg = null;
            return null;
        }

        public DataTable LoadExcel(string strFilePath, string conStr,out string errMsg)
        {
            OleDbConnection oledbConn = new OleDbConnection(conStr);
            DataTable dt = new DataTable();
            dt.Columns.Add("TFN", typeof(string));
            dt.Columns.Add("ProductCode", typeof(string));
            dt.Columns.Add("CallingDate", typeof(string));

            try
            {
                using (OleDbCommand cmd = new OleDbCommand("SELECT * FROM [Sheet1$]", oledbConn))
                {
                    if (oledbConn.State == ConnectionState.Closed)
                    { oledbConn.Open(); }
                    OleDbDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        DataRow dr = dt.NewRow();
                        dr["TFN"] = rdr["TFN"].ToString();
                        dr["ProductCode"] = rdr["ScriptCode"].ToString();
                        dr["CallingDate"] = rdr["CallingDate"].ToString();
                        dt.Rows.Add(dr);
                    }
                }
            }
            catch(Exception ex)
            {
                errMsg = ex.Message;
                oledbConn.Close();
                return null;
            }
            finally
            {
                oledbConn.Close();
            }
            errMsg = null;
            return dt;
        }
    }
}
