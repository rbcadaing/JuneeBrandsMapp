using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Core.Data;
using Core.Data.Models;
using System.IO;
using System.Data.OleDb;
using System.Data;
using JBMapp.ADUC;

namespace JBMapp.Controllers
{
    public class ProductCodeController : Controller
    {
        // GET: ProductCode
        public ActionResult Index()
        {
            ActiveDirectoryWeb ad = new ActiveDirectoryWeb();
            string[] usr = ad.SearchActiveDirectory(HttpContext.User.Identity.Name).Split('|');
            ViewBag.IsAdmin = DataAccess.AdminAccess(usr[1]);

            IEnumerable<ProductCode> pc = ProductCodeDataAccess.GetProductCodes();
            return View(pc);
        }

        [HttpGet]
        public ActionResult EditProductCode(string pId)
        {
            ActiveDirectoryWeb ad = new ActiveDirectoryWeb();
            string[] usr = ad.SearchActiveDirectory(HttpContext.User.Identity.Name).Split('|');
            ViewBag.IsAdmin = DataAccess.AdminAccess(usr[1]);

            ProductCode Pc = ProductCodeDataAccess.GetProductCode(pId);

            ViewBag.ScriptTemplate = new SelectList(ScriptDataAccess.GetScripts(), "ScriptTemplate", "ScriptName", Pc.ScriptTemplate);
            return View(Pc);
        }

        [HttpPost]
        public ActionResult EditProductCode(ProductCode pc)
        {
            ActiveDirectoryWeb ad = new ActiveDirectoryWeb();
            string[] usr = ad.SearchActiveDirectory(HttpContext.User.Identity.Name).Split('|');
            ViewBag.IsAdmin = DataAccess.AdminAccess(usr[1]);

            string logI = HttpContext.User.Identity.Name;
            ProductCodeDataAccess.UpdateProductCode(pc, logI);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult UploadProducts()
        {
            ActiveDirectoryWeb ad = new ActiveDirectoryWeb();
            string[] usr = ad.SearchActiveDirectory(HttpContext.User.Identity.Name).Split('|');
            ViewBag.IsAdmin = DataAccess.AdminAccess(usr[1]);
            //IEnumerable<ProductCode> pcs = null;
            return View();
        }

        [HttpPost]
        public ActionResult UploadProducts(HttpPostedFileBase file, List<ProductCode> model, string Submit)
        {
            ActiveDirectoryWeb ad = new ActiveDirectoryWeb();
            string[] usr = ad.SearchActiveDirectory(HttpContext.User.Identity.Name).Split('|');
            ViewBag.IsAdmin = DataAccess.AdminAccess(usr[1]);

            string logI = HttpContext.User.Identity.Name;
            List<ProductCode> pcs = new List<ProductCode>();
            List<ProductCode> RetPcs = new List<ProductCode>();
            string err;

            if (Submit == "Upload")
            {
                try
                {
                    pcs = LoadProductCodes(file, out err);

                    if (err == null)
                    {
                        foreach (ProductCode pc in pcs)
                        {
                            bool exist = ProductCodeDataAccess.CheckIfExist(pc.PCode);
                            if (exist == false)
                            {
                                RetPcs.Add(pc);
                            }
                        }

                        //future function
                        //foreach (ProductCode pc in pcs)
                        //{
                        //    bool forUpdating = ProductCodeDataAccess.CheckIfForUpdating(pc);
                        //    if (forUpdating == true)
                        //    {
                        //        RetPcs.Add(pc);
                        //    }
                        //}
                    }
                    else
                    {
                        ViewData["Error"] = err;
                        return View();
                    }
                }
                catch (Exception ex)
                {
                    ViewData["Error"] = ex.Message;
                    return View();
                }
                if (RetPcs.Count() <= 0)
                {
                    ViewData["Error"] = "No New ProductCode Found!";
                }
                ViewBag.ScriptTemplate = new SelectList(ScriptDataAccess.GetScripts(), "ScriptTemplate", "ScriptName");
                return View(RetPcs);
            }
            else
            {
                if (model != null)
                {
                    foreach (ProductCode pc in model)
                    {
                        try
                        {
                            ProductCodeDataAccess.InsertProduct(pc, logI.Split('\\').LastOrDefault());
                        }
                        catch (Exception ex)
                        {
                            ViewData["Error"] = ex.Message;
                            ViewBag.ScriptTemplate = new SelectList(ScriptDataAccess.GetScripts(), "ScriptTemplate", "ScriptName");
                            return View(model);
                        }
                    }
                }
                ViewData["Success"] = "Product Code Successfully uploaded!";
                return RedirectToAction("Index");
            }
        }

        public List<ProductCode> LoadProductCodes(HttpPostedFileBase file, out string errMsg)
        {
            List<ProductCode> mas = new List<ProductCode>();
            if (Request.Files.Count > 0)
            {
                if (file != null && file.ContentLength > 0)
                {
                    var fName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/MediaFiles/"), fName);
                    file.SaveAs(path);
                    string conStr;

                    string ext = Path.GetExtension(path);
                    if (ext.ToLower().Trim() == ".xls")
                    {
                        conStr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                        errMsg = null;
                        return LoadExcel(path, conStr);

                    }
                    else if (ext.ToLower().Trim() == ".xlsx")
                    {
                        conStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                        errMsg = null;
                        return LoadExcel(path, conStr);
                    }
                    else
                    {
                        errMsg = "Invalid File!";
                        return null;
                    }

                }
            }

            errMsg = null;
            return mas.ToList();
        }

        public static List<ProductCode> LoadExcel(string strFilePath, string conStr)
        {
            OleDbConnection oledbConn = new OleDbConnection(conStr);
            DataTable dt = new DataTable();
            List<ProductCode> pcs = new List<ProductCode>();

            try
            {
                if (oledbConn.State == ConnectionState.Closed)
                { oledbConn.Open(); }

                using (OleDbCommand cmd = new OleDbCommand("SELECT * FROM [Sheet1$] WHERE [Product Code] IS NOT NULL", oledbConn))
                {
                    OleDbDataAdapter oleda = new OleDbDataAdapter();
                    oleda.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    oleda.Fill(ds);
                    dt = ds.Tables[0];

                    foreach (DataRow dr in dt.Rows)
                    {
                        ProductCode pc = new ProductCode();
                        pc.Brand = dr["Brand"].ToString();
                        pc.PCode = dr["Product Code"].ToString();
                        pc.MediaCompany = dr["Media Company"].ToString();
                        pc.Script = dr["Script"].ToString();
                        pc.Offer = dr["Offer"].ToString();
                        pcs.Add(pc);
                    }
                }
            }
            catch 
            {
                oledbConn.Close();
            }
            finally
            {
                oledbConn.Close();
            }
            return pcs;
        }
    }
}
