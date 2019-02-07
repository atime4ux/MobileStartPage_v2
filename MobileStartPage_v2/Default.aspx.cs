using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI;

using System.IO;
using System.Web.UI.HtmlControls;
using System.Linq;

namespace MobileStartPage_v2
{
    public partial class Default : System.Web.UI.Page
    {
        private MobileStartPage_Helper _HELPER = null;
        private DataSet _DS_MENU = null;
        private string _PUBLIC_YN = null;

        public string PUBLIC_YN
        {
            get
            {
                if (_PUBLIC_YN == null)
                {
                    string show_nonpublic_category_word = HELPER.GetConfig("SHOW_NONPUBLIC_CATEGORY_WORD", "");
                    string privateKey = "";
                    _PUBLIC_YN = HELPER.GetRequest(privateKey, "").ToUpper() == show_nonpublic_category_word ? "" : "Y";
                }

                return _PUBLIC_YN;
            }
        }

        public DataSet DS_MENU
        {
            get
            {
                if (_DS_MENU == null)
                {
                    _DS_MENU = HELPER.GetMenuInfo();
                }
                return _DS_MENU;
            }
        }

        public MobileStartPage_Helper HELPER
        {
            get
            {
                if (_HELPER == null)
                {
                    _HELPER = new MobileStartPage_Helper(XML_PATH);
                }
                return _HELPER;
            }
        }

        public string XML_PATH
        {
            get
            {
                return Server.MapPath("/") + "MobileStartPage.xml";
            }
        }

        protected void Page_PreInit(object sender, EventArgs e)
        {
            string ajax_mode = HELPER.GetRequest("AJAX_MODE", "").Trim();
            if (ajax_mode.Length != 0)
            {
                if (ajax_mode == "ADD_CATEGORY")
                {
                    this.ProcAjax_AddCategory();
                }
                else if (ajax_mode == "REMOVE_CATEGORY")
                {
                    this.ProcAjax_RemoveCategory();
                }
                else if (ajax_mode == "MOD_CATEGORY")
                {
                    this.ProcAjax_ModCategory();
                }
                else if (ajax_mode == "GET_SITE_INFO")
                {
                    this.ProcAjax_GetSiteInfo();
                }
                else if (ajax_mode == "SAVE_SITE_INFO")
                {
                    this.ProcAjax_SaveSiteInfo();
                }
                base.Response.End();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //HELPER.ModifyScheme();
        }

        public string GetDdlCategory()
        {
            DataTable dtCategory = DS_MENU.Tables[0].Clone();
            foreach (DataRow dr in DS_MENU.Tables[0].Select(string.Format("USE_YN='Y' AND (PUBLIC_YN='{0}' OR '{0}'='')", PUBLIC_YN), "CATEGORY_SORT"))
            {
                dtCategory.ImportRow(dr);
            }
            
            HtmlSelect selectCategory = new HtmlSelect()
            {
                ID = "ddlCategory",
                DataValueField = "CATEGORY_IDX",
                DataTextField = "CATEGORY_NAME",
                DataSource = dtCategory
            };
            selectCategory.DataBind();
            return HELPER.GetControlRenderString(selectCategory);
        }

        public string GetMenuList()
        {
            HtmlGenericControl divMainCategory = new HtmlGenericControl("div");
            divMainCategory.Attributes["class"] = "mainCategory";
            int cnt = 0;

            foreach (DataRow drCategory in DS_MENU.Tables[0].Select(string.Format("USE_YN='Y' AND (PUBLIC_YN='{0}' OR '{0}'='')", PUBLIC_YN), "CATEGORY_SORT"))
            {
                int category_idx = HELPER.GetToInt32(drCategory["CATEGORY_IDX"]);
                string category_name = HELPER.GetString(drCategory["CATEGORY_NAME"]);
                string category_id = string.Format("cat_{0}", category_idx);
                HtmlGenericControl divSubCategory = new HtmlGenericControl("div");
                divSubCategory.Attributes["class"] = "subCategory";
                HtmlGenericControl spanSubCategoryTitle = new HtmlGenericControl("span");
                spanSubCategoryTitle.Attributes["class"] = "title";
                spanSubCategoryTitle.Attributes["onclick"] = string.Format("ToggleDisplayProp('{0}');", category_id);
                spanSubCategoryTitle.InnerText = category_name;
                HtmlGenericControl ulSiteList = new HtmlGenericControl("ul");
                ulSiteList.Attributes["id"] = category_id;
                if (cnt == 0)
                {
                    ulSiteList.Style["display"] = "block";
                }

                foreach (DataRow drSite in DS_MENU.Tables[1].Select(string.Format("CATEGORY_IDX={0} AND USE_YN='Y'", category_idx.ToString()), "SITE_SORT"))
                {
                    HtmlGenericControl liSiteTitle = new HtmlGenericControl("li");
                    int site_idx = HELPER.GetToInt32(drSite["SITE_IDX"]);
                    string site_name = HELPER.GetString(drSite["SITE_NAME"]);
                    string site_url = HELPER.GetString(drSite["SITE_URL"]);
                    string site_url_mobile = HELPER.GetString(drSite["SITE_URL_MOBILE"]);
                    string site_id = string.Format("site_{0}", site_idx);
                    HtmlGenericControl spanSiteTitle = new HtmlGenericControl("span");
                    spanSiteTitle.Attributes["id"] = site_id;
                    spanSiteTitle.Attributes["onclick"] = string.Format("MovePage('{0}', '{1}')", site_url_mobile, site_url);
                    spanSiteTitle.Style["margin-right"] = "20px";
                    spanSiteTitle.InnerText = site_name;
                    liSiteTitle.Controls.Add(spanSiteTitle);
                    if (this.PUBLIC_YN == "")
                    {
                        HtmlGenericControl spanModify = new HtmlGenericControl("span");
                        spanModify.Style["cursor"] = "pointer";
                        spanModify.Style["margin-right"] = "20px";
                        spanModify.Attributes["onclick"] = string.Format("GetSiteInfo('{0}')", site_idx);
                        spanModify.InnerText = "[수정]";
                        liSiteTitle.Controls.Add(spanModify);
                    }
                    ulSiteList.Controls.Add(liSiteTitle);
                }

                divSubCategory.Controls.Add(spanSubCategoryTitle);
                divSubCategory.Controls.Add(ulSiteList);
                divMainCategory.Controls.Add(divSubCategory);
                cnt++;
            }
            
            return HELPER.GetControlRenderString(divMainCategory);
        }

        private void ProcAjax_AddCategory()
        {
        }

        private void ProcAjax_GetSiteInfo()
        {
            int site_idx = HELPER.GetToInt32(HELPER.GetRequest("SITE_IDX"));
            if (site_idx > 0)
            {
                DataRow[] dataRowArray = DS_MENU.Tables[1].Select(string.Format("SITE_IDX={0}", site_idx));
                for (int i = 0; i < (int)dataRowArray.Length; i++)
                {
                    DataRow drMenu = dataRowArray[i];
                    System.Web.Script.Serialization.JavaScriptSerializer objJs = new System.Web.Script.Serialization.JavaScriptSerializer();
                    System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
                    objJs.Serialize(new { category_idx = HELPER.GetToInt32(drMenu["CATEGORY_IDX"]), site_idx = HELPER.GetToInt32(drMenu["SITE_IDX"]), site_name = HELPER.GetString(drMenu["SITE_NAME"]), site_url = HELPER.GetString(drMenu["SITE_URL"]), site_url_mobile = HELPER.GetString(drMenu["SITE_URL_MOBILE"]), site_sort = HELPER.GetToInt32(drMenu["SITE_SORT"]) }, strBuilder);
                    base.Response.Clear();
                    base.Response.ContentType = "application/json; charset=utf-8";
                    base.Response.Write(strBuilder.ToString());
                }
            }
        }

        private void ProcAjax_ModCategory()
        {
        }

        private void ProcAjax_RemoveCategory()
        {
        }

        private void ProcAjax_SaveSiteInfo()
        {
            int site_idx = HELPER.GetToInt32(HELPER.GetRequest("txtSiteIdx"));
            int category_idx = HELPER.GetToInt32(HELPER.GetRequest("ddlCategory"));
            string site_name = HELPER.GetString(HELPER.GetRequest("txtSiteName"));
            string site_url = HELPER.GetString(HELPER.GetRequest("txtSiteUrl"));
            string site_url_mobile = HELPER.GetString(HELPER.GetRequest("txtSiteUrlMobile"));
            int site_sort = HELPER.GetToInt32(HELPER.GetRequest("txtSiteSort"));
            string use_yn = HELPER.GetString(HELPER.GetRequest("ddlUseYN"));
            string result = "";
            if (category_idx > 0)
            {
                if (site_idx <= 0)
                {
                    result = (!(HELPER.GetRequest("txtSiteIdx") == "") ? "Abnormal SiteIdx" : HELPER.AddSiteInfo(category_idx, site_name, site_url, site_url_mobile, site_sort));
                }
                else
                {
                    result = HELPER.ModifySiteInfo(site_idx, category_idx, site_name, site_url, site_url_mobile, site_sort, use_yn);
                }
            }
            base.Response.Clear();
            base.Response.Write(result);
        }
    }
}

public class MobileStartPage_Helper
{
    private string XML_PATH = "";
    public MobileStartPage_Helper(string xmlPath)
    {
        XML_PATH = xmlPath;
    }

    public void ModifyScheme()
    {
        DataSet ds = GetMenuInfo();
        DataSet dsClone = ds.Clone();
        dsClone.Tables[0].Columns["CATEGORY_IDX"].DataType = typeof(int);
        dsClone.Tables[0].Columns["CATEGORY_SORT"].DataType = typeof(int);
        dsClone.Tables[0].Columns["CREATE_DATE"].DataType = typeof(DateTime);
        dsClone.Tables[0].Columns["UPDATE_DATE"].DataType = typeof(DateTime);
        dsClone.Tables[1].Columns["SITE_IDX"].DataType = typeof(int);
        dsClone.Tables[1].Columns["SITE_SORT"].DataType = typeof(int);
        dsClone.Tables[1].Columns["CATEGORY_IDX"].DataType = typeof(int);
        dsClone.Tables[1].Columns["CREATE_DATE"].DataType = typeof(DateTime);
        dsClone.Tables[1].Columns["UPDATE_DATE"].DataType = typeof(DateTime);

        foreach (DataTable dt in ds.Tables)
        {
            foreach (DataRow dr in dt.Rows)
            {
                DataRow drClone = dsClone.Tables[dt.TableName].NewRow();
                dsClone.Tables[dt.TableName].Rows.Add(drClone);

                foreach (DataColumn dc in dr.Table.Columns)
                {
                    drClone[dc.ColumnName] = dr[dc.ColumnName];
                }
            }
        }

        dsClone.WriteXml(XML_PATH.Replace(".xml", "2.xml"), XmlWriteMode.WriteSchema);
    }

    public void SaveMenuInfo(DataSet ds)
    {
        ds.WriteXml(XML_PATH, XmlWriteMode.WriteSchema);
    }

    public DataSet GetMenuInfo()
    {
        DataSet ds = new DataSet();
        ds.ReadXml(XML_PATH);
        //MENU_CATEGORY_INFO
        //MENU_SITE_INFO

        return ds;
    }

    public string AddSiteInfo(int category_idx, string site_name, string site_url, string site_url_mobile, int site_sort)
    {
        DataSet ds = GetMenuInfo();
        DataRow drSite = ds.Tables[1].NewRow();
        ds.Tables[1].Rows.Add(drSite);

        drSite["SITE_IDX"] = GetToInt32(ds.Tables[1].Compute("MAX(SITE_IDX)", null)) + 1;
        drSite["CATEGORY_IDX"] = category_idx;
        drSite["SITE_NAME"] = site_name;
        drSite["SITE_URL"] = site_url;
        drSite["SITE_URL_MOBILE"] = site_url_mobile;
        drSite["SITE_SORT"] = site_sort;
        drSite["USE_YN"] = "Y";
        drSite["CREATE_DATE"] = DateTime.Now;
        drSite["UPDATE_DATE"] = DateTime.Now;

        SaveMenuInfo(ds);

        return "";
    }

    public string ModifySiteInfo(int site_idx, int category_idx, string site_name, string site_url, string site_url_mobile, int site_sort, string use_yn)
    {
        DataSet ds = GetMenuInfo();

        foreach (DataRow drSite in ds.Tables[1].Select(string.Format("SITE_IDX={0}", site_idx.ToString())))
        {
            drSite["CATEGORY_IDX"] = category_idx;
            drSite["SITE_NAME"] = site_name;
            drSite["SITE_URL"] = site_url;
            drSite["SITE_URL_MOBILE"] = site_url_mobile;
            drSite["SITE_SORT"] = site_sort;
            drSite["USE_YN"] = use_yn;
            drSite["UPDATE_DATE"] = DateTime.Now;
        }

        SaveMenuInfo(ds);

        return "";
    }

    public double GetToDouble(object obj)
    {
        double result = 0;

        try
        {
            result = Convert.ToDouble(obj);
        }
        catch (Exception ex)
        { }

        return result;
    }

    public int GetToInt32(object obj)
    {
        int result = 0;

        try
        {
            result = Convert.ToInt32(obj);
        }
        catch (Exception ex)
        { }

        return result;
    }

    public string GetString(object obj)
    {
        string result = "";

        try
        {
            result = Convert.ToString(obj);
        }
        catch (Exception ex)
        { }

        return result;
    }

    public string GetRequest(string asKey, string nullValue)
    {
        string reVal = nullValue;
        if (HttpContext.Current.Request[asKey] != null)
        {
            reVal = HttpContext.Current.Request[asKey];
        }
        return reVal;
    }

    public string GetRequest(string asKey)
    {
        return GetRequest(asKey, "");
    }

    public string GetConfig(string asKey, string return_default_string)
    {
        string lsRet = return_default_string;
        try
        {
            lsRet = System.Configuration.ConfigurationManager.AppSettings[asKey].ToString();
        }
        catch (Exception exception)
        {
            lsRet = return_default_string;
        }
        return lsRet;
    }

    public DataTable FilterSortDataTable(DataTable dtStart, string filter, string sort, string tableName)
    {
        DataTable dt = dtStart.Clone();
        if ((tableName == null ? false : tableName != ""))
        {
            dt.TableName = tableName;
        }
        DataRow[] drs = null;
        drs = (sort != null ? dtStart.Select(filter, sort) : dtStart.Select(filter));
        DataRow[] dataRowArray = drs;
        for (int i = 0; i < (int)dataRowArray.Length; i++)
        {
            dt.ImportRow(dataRowArray[i]);
        }
        return dt;
    }

    public DataTable FilterSortDataTable(DataTable dtStart, string filter)
    {
        return FilterSortDataTable(dtStart, filter, null, null);
    }

    public static string GetSortString(string[] sortColumns, string lastfix, string asc_desc)
    {
        string str;
        string[] strArrays;
        string sorter = "";
        for (int i = 0; i < (int)sortColumns.Length; i++)
        {
            if (i != 0)
            {
                str = sorter;
                strArrays = new string[] { str, ",", sortColumns[i], lastfix, " ", asc_desc };
                sorter = string.Concat(strArrays);
            }
            else
            {
                str = sorter;
                strArrays = new string[] { str, sortColumns[i], lastfix, " ", asc_desc };
                sorter = string.Concat(strArrays);
            }
        }
        return sorter;
    }

    public string GetSortString(string[] sortColumns)
    {
        return GetSortString(sortColumns, "", "");
    }

    public string GetControlRenderString(HtmlControl control)
    {
        System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
        control.RenderControl(new HtmlTextWriter(new StringWriter(strBuilder)));
        return strBuilder.ToString();
    }

    public string GetFilterString(DataRow dataRow, string[] filterColumns)
    {
        string filter = "1 = 1";
        for (int i = 0; i < (int)filterColumns.Length; i++)
        {
            filter = string.Concat(filter, string.Format(" AND {0} = '{1}'", filterColumns[i], dataRow[filterColumns[i]]));
        }
        return filter;
    }

    public DataTable GetGroupByDataTable(DataTable originDataTable, string[] groupByColumns)
    {
        DataTable groupByDataTable = GetGroupByDataTable(originDataTable, "", "", groupByColumns, "", "", "");
        return groupByDataTable;
    }

    public DataTable GetGroupByDataTable(DataTable originDataTable, string aggregateOriginField1, string aggregateOriginField2, string[] groupByColumns, string aggregateNewField1, string aggregateNewField2, string primaryKey)
    {
        int i;
        int j;
        DataTable originDataTable_copy = originDataTable.Copy();
        if ((aggregateOriginField1 == "" ? true : aggregateOriginField1 == null))
        {
            bool exsistColmn = false;
            DateTime now = DateTime.Now;
            aggregateOriginField1 = string.Concat("T", now.ToString("yyyyMMddHHmmddfff"));
            foreach (DataColumn dc in originDataTable_copy.Columns)
            {
                if (dc.ColumnName == aggregateOriginField1)
                {
                    exsistColmn = true;
                }
            }
            if (!exsistColmn)
            {
                originDataTable_copy.Columns.Add(aggregateOriginField1, typeof(int));
            }
            for (i = 0; i < originDataTable_copy.Rows.Count; i++)
            {
                originDataTable_copy.Rows[i][aggregateOriginField1] = 0;
            }
            originDataTable_copy.AcceptChanges();
        }
        if ((aggregateNewField1 == "" ? true : aggregateNewField1 == null))
        {
            aggregateNewField1 = aggregateOriginField1;
        }
        if ((primaryKey == "" ? true : primaryKey == null))
        {
            primaryKey = "IDX";
            if (!originDataTable_copy.Columns.Contains(primaryKey))
            {
                originDataTable_copy.Columns.Add(primaryKey, typeof(int));
            }
            for (i = 0; i < originDataTable_copy.Rows.Count; i++)
            {
                originDataTable_copy.Rows[i][primaryKey] = i + 1;
            }
            originDataTable_copy.AcceptChanges();
        }
        string aggregateSumField1 = string.Format("{0}_{1}", "SUM", aggregateNewField1);
        string aggregateCntField1 = string.Format("{0}_{1}", "CNT", aggregateNewField1);
        string aggregateAvgField1 = string.Format("{0}_{1}", "AVG", aggregateNewField1);
        string aggregateSumField2 = string.Format("{0}_{1}", "SUM", aggregateNewField2);
        string aggregateCntField2 = string.Format("{0}_{1}", "CNT", aggregateNewField2);
        string aggregateAvgField2 = string.Format("{0}_{1}", "AVG", aggregateNewField2);
        if (!originDataTable_copy.Columns.Contains("IS_AGGEGATED"))
        {
            originDataTable_copy.Columns.Add("IS_AGGEGATED", typeof(int));
        }
        foreach (DataRow dr in originDataTable_copy.Rows)
        {
            dr["IS_AGGEGATED"] = 0;
        }
        DataTable dataTempTable = originDataTable_copy.Clone();
        List<string> removeColList = new List<string>();
        foreach (DataColumn dataColumn in dataTempTable.Columns)
        {
            bool isContain = false;
            i = 0;
            while (i < (int)groupByColumns.Length)
            {
                if (!(groupByColumns[i] == dataColumn.ColumnName))
                {
                    if (i == (int)groupByColumns.Length - 1)
                    {
                        if (!isContain)
                        {
                            removeColList.Add(dataColumn.ColumnName);
                        }
                    }
                    i++;
                }
                else
                {
                    isContain = true;
                    break;
                }
            }
        }
        for (i = 0; i < removeColList.Count; i++)
        {
            dataTempTable.Columns.Remove(removeColList[i]);
        }
        dataTempTable.AcceptChanges();
        dataTempTable.Columns.Add(aggregateSumField1, typeof(double));
        dataTempTable.Columns.Add(aggregateCntField1, typeof(double));
        dataTempTable.Columns.Add(aggregateAvgField1, typeof(double));
        dataTempTable.Columns.Add(aggregateSumField2, typeof(double));
        dataTempTable.Columns.Add(aggregateCntField2, typeof(double));
        dataTempTable.Columns.Add(aggregateAvgField2, typeof(double));
        dataTempTable.Columns.Add(primaryKey, typeof(string));
        DataRow dataTempRow = null;
        DataRow[] dataRowCol = null;
        foreach (DataRow dataRow in originDataTable_copy.Rows)
        {
            dataRowCol = originDataTable_copy.Select(string.Concat(GetFilterString(dataRow, groupByColumns), " AND IS_AGGEGATED = 0"));
            double aggreSumData1 = 0;
            double aggreCntData1 = 0;
            double aggreAvgData1 = 0;
            double aggreSumData2 = 0;
            double aggreCntData2 = 0;
            double aggreAvgData2 = 0;
            string idx_array = "";
            if ((int)dataRowCol.Length != 0)
            {
                for (i = 1; i <= (int)dataRowCol.Length; i++)
                {
                    aggreSumData1 += GetToDouble(dataRowCol[i - 1][aggregateOriginField1]);
                    if (!aggregateOriginField2.Equals(""))
                    {
                        aggreSumData2 += GetToDouble(dataRowCol[i - 1][aggregateOriginField2]);
                    }
                    if (aggreCntData1 == 0)
                    {
                        aggreCntData1 = (double)((int)dataRowCol.Length);
                    }
                    if (!aggregateOriginField2.Equals(""))
                    {
                        if (aggreCntData2 == 0)
                        {
                            aggreCntData2 = (double)((int)dataRowCol.Length);
                        }
                    }
                    aggreAvgData1 += GetToDouble(dataRowCol[i - 1][aggregateOriginField1]);
                    if (!aggregateOriginField2.Equals(""))
                    {
                        aggreAvgData2 += GetToDouble(dataRowCol[i - 1][aggregateOriginField2]);
                    }
                    if (i == (int)dataRowCol.Length)
                    {
                        aggreAvgData1 /= GetToDouble(i);
                    }
                    if (!aggregateOriginField2.Equals(""))
                    {
                        if (i == (int)dataRowCol.Length)
                        {
                            aggreAvgData2 /= GetToDouble(i);
                        }
                    }
                    idx_array = (i != 1 ? string.Concat(idx_array, ";", dataRowCol[i - 1][primaryKey]) : string.Concat(idx_array, dataRowCol[i - 1][primaryKey]));
                    dataRowCol[i - 1]["IS_AGGEGATED"] = 1;
                }
                dataTempRow = dataTempTable.NewRow();
                string[] strArrays = groupByColumns;
                for (j = 0; j < (int)strArrays.Length; j++)
                {
                    string groupByColumn = strArrays[j];
                    dataTempRow[groupByColumn] = dataRow[groupByColumn];
                }
                dataTempRow[aggregateSumField1] = aggreSumData1;
                dataTempRow[aggregateCntField1] = aggreCntData1;
                dataTempRow[aggregateAvgField1] = Math.Round(aggreAvgData1, 4);
                if (!aggregateOriginField2.Equals(""))
                {
                    dataTempRow[aggregateSumField2] = aggreSumData2;
                    dataTempRow[aggregateCntField2] = aggreCntData2;
                    dataTempRow[aggregateAvgField2] = Math.Round(aggreAvgData2, 4);
                }
                dataTempRow[primaryKey] = idx_array;
                dataTempTable.Rows.Add(dataTempRow);
            }
        }
        DataTable dataSotedTable = dataTempTable.Clone();
        dataRowCol = dataTempTable.Select("", GetSortString(groupByColumns));
        DataRow[] dataRowArray = dataRowCol;
        for (j = 0; j < (int)dataRowArray.Length; j++)
        {
            dataSotedTable.ImportRow(dataRowArray[j]);
        }
        originDataTable_copy.Dispose();
        dataTempTable.Dispose();
        return dataSotedTable;
    }
}