using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using Core.Data;
using JBMapp.Models;
using System.Data.OleDb;
using System.Runtime.InteropServices;
using JBMapp.ADUC;
namespace JBMapp.Controllers
{
    public class HomeController : Controller
    {

        [HttpGet]
        public ActionResult Index()
        {

            ActiveDirectoryWeb ad = new ActiveDirectoryWeb();
            string[] usr = ad.SearchActiveDirectory(HttpContext.User.Identity.Name).Split('|');
            ViewBag.IsAdmin = DataAccess.AdminAccess(usr[1]);

            List<MediaCompany> Mc = new List<MediaCompany>(){
                new MediaCompany {id = "Havas",Name= "Havas - Launch DRTV"},
                new MediaCompany {id="Others",Name = "Others"}
            };
            ViewBag.MediaCompany = new SelectList(Mc.ToList(), "id", "Name");
            return View();
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file, string MediaCompany, List<MediaAssignments> model, string Submit)
        {
            ActiveDirectoryWeb ad = new ActiveDirectoryWeb();
            string[] usr = ad.SearchActiveDirectory(HttpContext.User.Identity.Name).Split('|');
            ViewBag.IsAdmin = DataAccess.AdminAccess(usr[1]);

            string logI = HttpContext.User.Identity.Name;
            List<MediaCompany> Mc = new List<MediaCompany>(){
                new MediaCompany {id = "Havas",Name= "Havas - Launch DRTV"},
                new MediaCompany {id="Others",Name = "Others"}
            };

            if (Submit == "Upload")
            {
                string errMsg;
                List<MediaAssignments> mas = new List<MediaAssignments>();
                try
                {
                    mas = LoadMediaAssignments(file, MediaCompany, out errMsg);
                    ViewData["InvalidMedia"] = errMsg;
                }
                catch (Exception ex)
                {
                    ViewData["InvalidMedia"] = ex.Message;
                }
                //ViewBag.Employeeid = employeeid;
                ViewBag.MediaCompany = new SelectList(Mc.ToList(), "id", "Name", MediaCompany);
                return View(mas);
            }
            else
            {
                int retval;
                foreach (MediaAssignments ma in model)
                {
                    try
                    {
                        DataAccess da = new DataAccess();
                        retval = da.InsertMediaAssignment(ma.Company, ma.Action, ma.Station, ma.CityState,
                        ma.PhoneNumber, ma.AirDate, ma.ProductCode, logI.Split('\\').Last());
                    }
                    catch (Exception ex)
                    {
                        ViewData["MediaAssignmentSuccess"] = ex.Message;
                        return View();
                    }
                }
                ViewData["MediaAssignmentSuccess"] = "Media Assignments Successfully Sourced!";
                ViewBag.MediaCompany = new SelectList(Mc.ToList(), "id", "Name", MediaCompany);
                return View();
            }
        }

        [HttpGet]
        public ActionResult ManageTFN()
        {
            ActiveDirectoryWeb ad = new ActiveDirectoryWeb();
            string[] usr = ad.SearchActiveDirectory(HttpContext.User.Identity.Name).Split('|');
            ViewBag.IsAdmin = DataAccess.AdminAccess(usr[1]);
            return View();
        }

        [HttpPost]
        public ActionResult ManageTFN(HttpPostedFileBase file)
        {
            ActiveDirectoryWeb ad = new ActiveDirectoryWeb();
            string[] usr = ad.SearchActiveDirectory(HttpContext.User.Identity.Name).Split('|');
            ViewBag.IsAdmin = DataAccess.AdminAccess(usr[1]);

            string logI = HttpContext.User.Identity.Name;
            DataTable dt = new DataTable();
            dt.Columns.Add("TFN", typeof(string));
            dt.Columns.Add("ProductCode", typeof(string));
            dt.Columns.Add("CallingDate", typeof(string));

            if (Request.Files.Count > 0)
            {

                if (file != null && file.ContentLength > 0)
                {
                    var fName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/MediaFiles/"), fName);
                    file.SaveAs(path);

                    //Create COM Objects. Create a COM object for everything that is referenced
                    Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
                    Microsoft.Office.Interop.Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(path);
                    Microsoft.Office.Interop.Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
                    Microsoft.Office.Interop.Excel.Range xlRange = xlWorksheet.UsedRange;

                    int rowCount = xlRange.Rows.Count;
                    int colCount = xlRange.Columns.Count;

                    for (int i = 2; i <= rowCount; i++)
                    {

                        string TFN = Convert.ToString(xlRange.Cells[i, 2].value);
                        DataRow dr = dt.NewRow();
                        dr["TFN"] = TFN;
                        dr["ProductCode"] = xlRange.Cells[i, 3].value;
                        dr["CallingDate"] = xlRange.Cells[i, 1].value;
                        dt.Rows.Add(dr);
                    }

                    //cleanup
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    //rule of thumb for releasing com objects:
                    //  never use two dots, all COM objects must be referenced and released individually
                    //  ex: [somthing].[something].[something] is bad

                    //release com objects to fully kill excel process from running in the background
                    Marshal.ReleaseComObject(xlRange);
                    Marshal.ReleaseComObject(xlWorksheet);

                    //close and release
                    xlWorkbook.Close();
                    Marshal.ReleaseComObject(xlWorkbook);

                    //quit and release
                    xlApp.Quit();
                    Marshal.ReleaseComObject(xlApp);

                    DataAccess.InsertTFNAlert(dt, logI.Split('\\').Last());
                }
            }

            return View();
        }

        public ActionResult About()
        {
            ActiveDirectoryWeb ad = new ActiveDirectoryWeb();
            string[] usr = ad.SearchActiveDirectory(HttpContext.User.Identity.Name).Split('|');
            ViewBag.IsAdmin = DataAccess.AdminAccess(usr[1]);
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ActiveDirectoryWeb ad = new ActiveDirectoryWeb();
            string[] usr = ad.SearchActiveDirectory(HttpContext.User.Identity.Name).Split('|');
            ViewBag.IsAdmin = DataAccess.AdminAccess(usr[1]);
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public List<MediaAssignments> LoadMediaAssignments(HttpPostedFileBase file, string mediacompany, out string errMsg)
        {
            List<MediaAssignments> mas = new List<MediaAssignments>();
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
                            errMsg = null;
                            return LoadExcel(path, conStr, mediacompany, out errMsg);

                        }
                        else if (ext.ToLower().Trim() == ".xlsx")
                        {
                            conStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                            errMsg = null;
                            return LoadExcel(path, conStr, mediacompany, out errMsg);
                        }
                        else
                        {
                            errMsg = "Invalid File!";
                            return null;
                        }
                    }
                    catch (Exception ex)
                    {
                        errMsg = ex.Message;
                        return null;
                    }
                }
            }
            errMsg = null;
            return mas.ToList();
        }

        public static List<MediaAssignments> LoadExcel(string strFilePath, string conStr, string mediacompany, out string errMsg)
        {
            OleDbConnection oledbConn = new OleDbConnection(conStr);
            DataTable dt = new DataTable();
            List<MediaAssignments> mas = new List<MediaAssignments>();

            try
            {
                oledbConn.Open();
                using (OleDbCommand cmd = new OleDbCommand("SELECT * FROM [Sheet1$]", oledbConn))
                {
                    OleDbDataAdapter oleda = new OleDbDataAdapter();
                    oleda.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    oleda.Fill(ds);
                    dt = ds.Tables[0];

                    if (mediacompany.ToLower() == "Havas".ToLower() || mediacompany.ToLower() == "Launch DRTV".ToLower())
                    {
                        try
                        {
                            foreach (DataRow dr in dt.Rows)
                            {
                                MediaAssignments ma = new MediaAssignments();
                                ma.Company = dr["company"].ToString();
                                ma.Action = dr["action"].ToString();
                                ma.Station = dr["station"].ToString();
                                ma.CityState = dr["citystate"].ToString();
                                ma.PhoneNumber = dr["phonenum"].ToString();
                                ma.AirDate = dr["airdate"].ToString();
                                ma.ProductCode = dr["code"].ToString();
                                mas.Add(ma);
                            }
                        }
                        catch
                        {
                            oledbConn.Close();
                            errMsg = "Invalid Media File! please select havas media assignment";
                            return null;
                        }
                    }
                    else
                    {
                        try
                        {
                            foreach (DataRow dr in dt.Rows)
                            {
                                MediaAssignments ma = new MediaAssignments();
                                ma.Company = dr["media buyer name"].ToString();
                                ma.Action = dr["ADD/DELETE"].ToString();
                                ma.Station = dr["station"].ToString();
                                ma.CityState = dr["MARKET"].ToString();
                                ma.PhoneNumber = dr["PHONE NUMBER"].ToString();
                                ma.AirDate = dr["START DATE"].ToString();
                                ma.ProductCode = dr["PRODUCT CODE"].ToString();
                                mas.Add(ma);
                            }
                        }
                        catch (Exception ex)
                        {
                            oledbConn.Close();
                            errMsg = "Invalid Media File!";
                            return null;
                        }
                    }
                }

            }
            catch (Exception)
            {

                oledbConn.Close();
                throw;
            }
            finally
            {
                oledbConn.Close();
            }
            errMsg = null;
            return mas;

        }

  
    }

}