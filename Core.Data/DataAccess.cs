using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using Core.Data.Models;

namespace Core.Data
{
    public class DataAccess
    {

        public int InsertMediaAssignment(string company, string action, string station, string citystate, string phonenum
            , string airdate, string code, string employeeid)
        {
            int ret;
            using (SqlConnection scon = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLConn"].ConnectionString))
            {

                if (scon.State == ConnectionState.Closed)
                {
                    scon.Open();
                }
                SqlCommand cmd = new SqlCommand();
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = scon;
                cmd.CommandText = "usp_InsertMediaAssignmentsNew2";
                cmd.Parameters.AddWithValue("@Company", string.IsNullOrEmpty(company) ? "" : company);
                cmd.Parameters.AddWithValue("@Action", action);
                cmd.Parameters.AddWithValue("@Station", station);
                cmd.Parameters.AddWithValue("@CityStateRaw", string.IsNullOrEmpty(citystate) ? "" : citystate);
                cmd.Parameters.AddWithValue("@PhoneNum", phonenum);
                cmd.Parameters.AddWithValue("@AirDateRaw", airdate == null ? "" : airdate);
                cmd.Parameters.AddWithValue("@Code", code);
                cmd.Parameters.AddWithValue("@EmployeeID", employeeid);
                ret = cmd.ExecuteNonQuery();

                return ret;
            }
        }

        public static int ProcessMediaAssignment(string employeeid, out string err)
        {
            int ret;
            using (SqlConnection scon = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLConn"].ConnectionString))
            {
                try
                {
                    if (scon.State == ConnectionState.Closed)
                    {
                        scon.Open();
                    }
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandTimeout = 120;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = scon;
                    cmd.CommandText = "usp_ProcessMediaAssignment";
                    cmd.Parameters.AddWithValue("@EmployeeID", employeeid);
                    ret = cmd.ExecuteNonQuery();
                    err = null;
                    return ret;
                }
                catch (Exception ex)
                {
                    err = ex.Message;
                    return 0;
                }
            }
        }

        public static int InsertTFNAlert(DataTable dt, string windowsid)
        {
            int proc;
            using (SqlConnection scon = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLConn"].ConnectionString))
            {
                //try
                //{
                if (scon.State == ConnectionState.Closed)
                {
                    scon.Open();
                }
                SqlCommand cmd = new SqlCommand();
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = scon;
                cmd.CommandText = "usp_InsertTFNFromAlert";
                cmd.Parameters.AddWithValue("@Dump", dt);
                cmd.Parameters.AddWithValue("@WindowsID", windowsid);
                proc = cmd.ExecuteNonQuery();
                //}
                //catch (Exception)
                //{
                //    throw;
                //}
                return proc;
            }
        }

        public static bool AdminAccess(string hrid)
        {
            DataTable dt = new DataTable();
            using (SqlConnection scon = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLConn"].ConnectionString))
            {
                if (scon.State == ConnectionState.Closed)
                {
                    scon.Open();
                }
                SqlCommand cmd = new SqlCommand();
                cmd.CommandTimeout = 120;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = scon;
                cmd.CommandText = "Select * FROM JB_Mapp_AdminUser WHERE EmployeeID = @EmployeeID";
                cmd.Parameters.AddWithValue("@EmployeeID", hrid);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    return true;
                }
                else
                { return false; }




            }
        }
    }

    public class ScriptDataAccess
    {
        public static List<PromoScript> GetScripts()
        {
            List<PromoScript> retVal = new List<PromoScript>();

            using (SqlConnection scon = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLConn"].ConnectionString))
            {
                try
                {
                    if (scon.State == ConnectionState.Closed)
                    {
                        scon.Open();
                    }
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = scon;
                    cmd.CommandText = "SELECT * FROM [Script] order by ScriptName";

                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        PromoScript sc = new PromoScript();
                        sc.ScriptTemplate = rdr["ScriptCode"].ToString();
                        sc.ScriptName = rdr["ScriptName"].ToString();
                        sc.BiScriptCode = rdr["BiScriptCode"].ToString();
                        sc.Description = rdr["Description"].ToString();
                        sc.CoreOfferSalesTax = rdr["CoreOfferSalesTax"].ToString();
                        sc.CrossSalesTax = rdr["CrossSaleTax"].ToString();
                        retVal.Add(sc);
                    }

                }
                catch (Exception) { throw; }
            }
            return retVal;

        }

        public static int SaveNewScript(PromoScript ps, string userId)
        {
            using (SqlConnection scon = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLConn"].ConnectionString))
            {

                if (scon.State == ConnectionState.Closed)
                {
                    scon.Open();
                }
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = scon;
                cmd.CommandText = "usp_InsertScript";
                cmd.Parameters.AddWithValue("@ScriptCode", ps.ScriptTemplate);
                cmd.Parameters.AddWithValue("@ScriptName", string.IsNullOrEmpty(ps.ScriptName) ? "" : ps.ScriptName);
                cmd.Parameters.AddWithValue("@BiScriptCode", string.IsNullOrEmpty(ps.BiScriptCode) ? "" : ps.BiScriptCode);
                cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(ps.Description) ? "" : ps.Description);
                cmd.Parameters.AddWithValue("@CoreOfferSalesTax", string.IsNullOrEmpty(ps.CoreOfferSalesTax) ? "" : ps.CoreOfferSalesTax);
                cmd.Parameters.AddWithValue("@CrossSaleTax", string.IsNullOrEmpty(ps.CrossSalesTax) ? "" : ps.CrossSalesTax);
                cmd.Parameters.AddWithValue("@BrandCode", string.IsNullOrEmpty(ps.BrandCode) ? "" : ps.BrandCode);
                cmd.Parameters.AddWithValue("@UserId", userId);
                SqlParameter Result = new SqlParameter();
                Result.ParameterName = "@Result";
                Result.Direction = ParameterDirection.Output;
                Result.SqlDbType = SqlDbType.Int;
                cmd.Parameters.Add(Result);
                cmd.ExecuteNonQuery();
                int ret = Convert.ToInt32(Result.Value);
                return ret;
            }
        }

        public static DataRow GetScript(string sTemplate = "")
        {
            DataTable dt = new DataTable();
            DataRow dr;
            using (SqlConnection scon = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLConn"].ConnectionString))
            {
                if (scon.State == ConnectionState.Closed)
                {
                    scon.Open();
                }
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.Text;
                cmd.Connection = scon;
                cmd.CommandText = "SELECT * FROM [Script] WHERE SCRIPTCODE = @SCRIPTCODE";
                cmd.Parameters.AddWithValue("@SCRIPTCODE", sTemplate);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    dr = dt.Rows[0];
                    return dr;
                }
                else
                { return null; }

            }


        }

        public static void UpdateScript(PromoScript ps, string oldScriptCode, string userId)
        {
            using (SqlConnection scon = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLConn"].ConnectionString))
            {
                if (scon.State == ConnectionState.Closed)
                {
                    scon.Open();
                }
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = scon;
                cmd.CommandText = "usp_UpdateScript";
                cmd.Parameters.AddWithValue("@ScriptCode", ps.ScriptTemplate);
                cmd.Parameters.AddWithValue("@ScriptName", string.IsNullOrEmpty(ps.ScriptName) ? "" : ps.ScriptName);
                cmd.Parameters.AddWithValue("@BiScriptCode", string.IsNullOrEmpty(ps.BiScriptCode) ? "" : ps.BiScriptCode);
                cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(ps.Description) ? "" : ps.Description);
                cmd.Parameters.AddWithValue("@CoreOfferSalesTax", string.IsNullOrEmpty(ps.CoreOfferSalesTax) ? "" : ps.CoreOfferSalesTax);
                cmd.Parameters.AddWithValue("@CrossSaleTax", string.IsNullOrEmpty(ps.CrossSalesTax) ? "" : ps.CrossSalesTax);
                cmd.Parameters.AddWithValue("@BrandCode", string.IsNullOrEmpty(ps.BrandCode) ? "" : ps.BrandCode);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@OldScriptCode", oldScriptCode);
                cmd.ExecuteNonQuery();
            }
        }

        public static List<Brands> GetBrands()
        {
            List<Brands> ret = new List<Brands>();
            using (SqlConnection scon = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLConn"].ConnectionString))
            {
                if (scon.State == ConnectionState.Closed)
                {
                    scon.Open();
                }
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = scon;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM BRANDS";
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Brands br = new Brands();
                    br.BrandCode = rdr["BrandCode"].ToString();
                    br.BrandName = rdr["BrandName"].ToString();
                    ret.Add(br);
                }
                Brands br2 = new Brands();
                br2.BrandCode = "";
                br2.BrandName = "Please Select";
                ret.Add(br2);
                scon.Close();
            }
            return ret;
        }
    }

    public class ProductCodeDataAccess
    {
        public static List<ProductCode> GetProductCodes()
        {
            List<ProductCode> retVal = new List<ProductCode>();

            using (SqlConnection scon = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLConn"].ConnectionString))
            {
                try
                {
                    if (scon.State == ConnectionState.Closed)
                    {
                        scon.Open();
                    }
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = scon;
                    cmd.CommandText = "SELECT Brand,A.PRoductCode,Script,Offer,A.BrandCode,A.MediaCompany,B.ScriptName FROM [ListOFProducts] A LEFT JOIN [Script] B ON A.VxiScriptCode = B.ScriptCode Order By Brand,A.ProductCode";

                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        ProductCode pc = new ProductCode();
                        pc.Brand = rdr["Brand"].ToString();
                        pc.PCode = rdr["ProductCode"].ToString();
                        pc.Script = rdr["Script"].ToString();
                        pc.Offer = rdr["Offer"].ToString();
                        pc.BrandCode = rdr["BrandCode"].ToString();
                        pc.MediaCompany = string.IsNullOrEmpty(rdr["MediaCompany"].ToString()) ? "" : rdr["MediaCompany"].ToString();
                        pc.ScriptTemplate = rdr["ScriptName"].ToString();
                        retVal.Add(pc);
                    }

                }
                catch (Exception) { throw; }
            }
            return retVal;
        }

        public static ProductCode GetProductCode(string pcode)
        {
            ProductCode pc = new ProductCode();
            DataTable dt = new DataTable();
            using (SqlConnection scon = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLConn"].ConnectionString))
            {
                if (scon.State == ConnectionState.Closed)
                {
                    scon.Open();
                }
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.Text;
                cmd.Connection = scon;
                cmd.CommandText = "SELECT * FROM [ListOFProducts] WHERE ProductCode = @ProductCode";
                cmd.Parameters.AddWithValue("@ProductCode", pcode);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    pc.Brand = dt.Rows[0]["Brand"].ToString();
                    pc.PCode = dt.Rows[0]["ProductCode"].ToString();
                    pc.Script = dt.Rows[0]["Script"].ToString();
                    pc.Offer = dt.Rows[0]["Offer"].ToString();
                    pc.BrandCode = dt.Rows[0]["BrandCode"].ToString();
                    pc.ScriptTemplate = dt.Rows[0]["VxiScriptCode"].ToString();
                    pc.MediaCompany = dt.Rows[0]["MediaCompany"].ToString();
                }
                else
                { return null; }

            }
            return pc;
        }

        public static bool CheckIfExist(string ProductCode)
        {
            DataTable dt = new DataTable();
            using (SqlConnection scon = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLConn"].ConnectionString))
            {
                if (scon.State == ConnectionState.Closed)
                {
                    scon.Open();
                }
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.Text;
                cmd.Connection = scon;
                cmd.CommandText = "SELECT * FROM [ListOFProducts] WHERE ProductCode LIKE '%' + @ProductCode + '%'";
                cmd.Parameters.AddWithValue("@ProductCode", ProductCode.Trim());
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    return true;
                }

            }

            return false;
        }

        public static int InsertProduct(ProductCode pc, string userid)
        {
            int ret = 0;
            using (SqlConnection scon = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLConn"].ConnectionString))
            {
                if (scon.State == ConnectionState.Closed)
                {
                    scon.Open();
                }

                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = scon;
                cmd.CommandText = "usp_InsertProductCode";
                cmd.Parameters.AddWithValue("@Brand", pc.Brand);
                cmd.Parameters.AddWithValue("@ProductCode", pc.PCode);
                cmd.Parameters.AddWithValue("@Script", pc.Script);
                cmd.Parameters.AddWithValue("@OFFER", pc.Offer);
                cmd.Parameters.AddWithValue("@MediaCompany", pc.MediaCompany);
                cmd.Parameters.AddWithValue("@ScriptCode", pc.ScriptTemplate);
                cmd.Parameters.AddWithValue("@UserId", userid);
                ret = cmd.ExecuteNonQuery();
            }
            return ret;
        }

        public static int UpdateProductCode(ProductCode pc, string userid)
        {
            int ret = 0;
            using (SqlConnection scon = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLConn"].ConnectionString))
            {
                if (scon.State == ConnectionState.Closed)
                {
                    scon.Open();
                }
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = scon;
                cmd.CommandText = "usp_UpdateListOfProductsProductCode";
                cmd.Parameters.AddWithValue("@ProductCode", pc.PCode);
                cmd.Parameters.AddWithValue("@ScriptCode", pc.ScriptTemplate);
                cmd.Parameters.AddWithValue("@UserId", userid);
                ret = cmd.ExecuteNonQuery();
            }
            return ret;
        }

        public static bool CheckIfForUpdating(ProductCode pc)
        {
            //future function not in-used
            DataTable dt = new DataTable();
            using (SqlConnection scon = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLConn"].ConnectionString))
            {
                if (scon.State == ConnectionState.Closed)
                {
                    scon.Open();
                }
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = scon;
                cmd.CommandText = "usp_CheckIfForUpdatingListOfProductsProductCode";
                cmd.Parameters.AddWithValue("@ProductCode", pc.PCode);
                cmd.Parameters.AddWithValue("@ScriptCode", pc.Script.Trim());
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }



}
