using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Core.Data;
using Core.Data.Models;
using System.Data;
using JBMapp.ADUC;


namespace JBMapp.Controllers
{
    public class ScriptController : Controller
    {
        public ActionResult Index()
        {
            ActiveDirectoryWeb ad = new ActiveDirectoryWeb();
            string[] usr = ad.SearchActiveDirectory(HttpContext.User.Identity.Name).Split('|');
            ViewBag.IsAdmin = DataAccess.AdminAccess(usr[1]);

            IEnumerable<PromoScript> scripts = ScriptDataAccess.GetScripts().ToList();
            return View(scripts);
        }

        [HttpGet]
        public ActionResult AddNewScript()
        {
            ActiveDirectoryWeb ad = new ActiveDirectoryWeb();
            string[] usr = ad.SearchActiveDirectory(HttpContext.User.Identity.Name).Split('|');
            ViewBag.IsAdmin = DataAccess.AdminAccess(usr[1]);

            ViewBag.BrandCode = new SelectList(ScriptDataAccess.GetBrands(), "BrandCode", "BrandName","");
            return View();
        }

        [HttpPost]
        public ActionResult AddNewScript(PromoScript ps)
        {
            ActiveDirectoryWeb ad = new ActiveDirectoryWeb();
            string[] usr = ad.SearchActiveDirectory(HttpContext.User.Identity.Name).Split('|');
            ViewBag.IsAdmin = DataAccess.AdminAccess(usr[1]);
            try
            {
                string logI = HttpContext.User.Identity.Name;
                int ret = ScriptDataAccess.SaveNewScript(ps, logI.Split('\\').LastOrDefault());
                if (ret > 0)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewData["Err"] = "Script Template Already Exist!";
                }
            }
            catch (Exception ex)
            {

                ViewData["Err"] = ex.Message;
            }
            return View(ps);
        }

        [HttpGet]
        public ActionResult EditScript(string tId)
        {
            ActiveDirectoryWeb ad = new ActiveDirectoryWeb();
            string[] usr = ad.SearchActiveDirectory(HttpContext.User.Identity.Name).Split('|');
            ViewBag.IsAdmin = DataAccess.AdminAccess(usr[1]);

            PromoScript ps = new PromoScript();
            DataRow dr = ScriptDataAccess.GetScript(tId);
            ps.ScriptTemplate = dr["ScriptCode"].ToString();
            ps.ScriptName = dr["ScriptName"].ToString();
            ps.BiScriptCode = dr["BIScriptCode"].ToString();
            ps.CoreOfferSalesTax = dr["CoreOfferSalesTax"].ToString();
            ps.CrossSalesTax = dr["CrossSaleTax"].ToString();
            ps.Description = dr["Description"].ToString();
            ps.BrandCode = dr["BrandCode"].ToString();
            ViewBag.OldScriptCode = dr["ScriptCode"].ToString();
            ViewBag.BrandCode = new SelectList(ScriptDataAccess.GetBrands(), "BrandCode", "BrandName", ps.BrandCode);
            return View(ps);
        }

        [HttpPost]
        public ActionResult EditScript(PromoScript ps, string OldScriptCode)
        {
            ActiveDirectoryWeb ad = new ActiveDirectoryWeb();
            string[] usr = ad.SearchActiveDirectory(HttpContext.User.Identity.Name).Split('|');
            ViewBag.IsAdmin = DataAccess.AdminAccess(usr[1]);
            string logI = HttpContext.User.Identity.Name;
            try
            {
                ScriptDataAccess.UpdateScript(ps, OldScriptCode, logI.Split('\\').LastOrDefault());
            }
            catch
            {
                ViewBag.OldScriptCode = OldScriptCode;
                return View(ps);
            }
            return RedirectToAction("Index");
        }
    }
}