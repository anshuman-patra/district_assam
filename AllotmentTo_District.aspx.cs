using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using DataAccessLayer;
using System.IO;
using System.Text;
using System.Web.Configuration;
using System.Text.RegularExpressions;

public partial class NHM_Allotment_AllotmentTo_District : System.Web.UI.Page
{
    IDBManager db = new DBManager(DataProvider.SqlServer, ConfigurationManager.ConnectionStrings["DataConnString"].ConnectionString);
    UtilityLibrary utl = new UtilityLibrary();

    #region//Global Variables Declaration..!
    decimal _ProjAAP_Amt, PCountZeroAAP_Amt = 0;
    decimal _CompAAP_Amt, CCountZeroAAP_Amt = 0;
    decimal _SubCompAAP_Amt, SCountZeroAAP_Amt = 0;
    decimal _AllotAAP_Amt = 0;
    int _ProjNoOfTarget, _CompNoOfTarget, _SubCompNoOfTarget, _AllotNoOfTarget = 0;
    decimal num = new decimal(0);

    string OfficeType_Chk = "";
    #endregion


    

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["UserLoginDetails"] != null && Session["AuthToken"] != null && Request.Cookies["AuthToken"] != null)
        {
            if (!Session["AuthToken"].ToString().Equals(Request.Cookies["AuthToken"].Value))
                return;
            if (!Page.IsPostBack)
            {
                UserLoginDetails objUserLoginDetails = new UserLoginDetails();
                objUserLoginDetails = (UserLoginDetails)Session["UserLoginDetails"];
                hfUserID.Value = objUserLoginDetails.UserID;
                hfUserType.Value = objUserLoginDetails.UserType;
                hfDeptCode.Value = objUserLoginDetails.DeptCode;
                hfDeptName.Value = objUserLoginDetails.DeptName;
                hfOfficeCode.Value = objUserLoginDetails.OfficeCode;
                hfOffName.Value = objUserLoginDetails.OffName;
                hfOfficeType.Value = objUserLoginDetails.OfficeType;
                hfF_Year.Value = objUserLoginDetails.F_Year;
                hfFirstLogin.Value = objUserLoginDetails.FirstLogin;
                hfSchemeCode.Value = ConfigurationManager.AppSettings["KeySchemeCode"].ToString();
                lblDept.Text = hfDeptName.Value;
                if ((hfUserType.Value == "I" ) && hfFirstLogin.Value == "N")
                {
                    //CalendarExtender1.OnClientShown = this.ID + "onCheckForPastDate";
                    //CalendarExtender1.OnClientHidden = this.ID + "onCheckForFutureDate";
                    PopulateYear();
                    GetAllQuarters_FromFnYear(FnYear: ddlYear.SelectedValue);
                    PopulateScheme(hfDeptCode.Value);
                    utl.SetSessionCookie();
                    hfSession.Value = Session["AuthTokenPage"].ToString();
                    lblMsg.Text = "";
                    GetTotalAndAvail_Balance(office_code: hfOfficeCode.Value);
                }
            }

        }
        else { return; }
    }
    protected void rd_alt_SelectedIndexChanged(object sender, EventArgs e)
    {

    }
    protected void GetTotalAndAvail_Balance(string office_code)
    {
        DataSet dsHeadwiseDtls = new DataSet();
        try
        {
            db.CreateInParameters(2);
            db.AddInParameters(0, "@office_code", office_code);
            db.AddInParameters(1, "@Fn_Year", ddlYear.SelectedValue);
            db.CreateOutParameters(3);
            db.AddOutParameters(0, "@available_amt", 1, 100);
            db.AddOutParameters(1, "@total_allotment", 1, 100);
            db.AddOutParameters(2, "@exep_Amt", 1, 100);
            db.Open();
            dsHeadwiseDtls = db.ExecuteDataSet(CommandType.StoredProcedure, "SNA_retrive_amt_district");
            string Avail_amt = db.outParameters[0].Value.ToString();
            string total_amt = db.outParameters[1].Value.ToString();
            db.Close();
           
                lbl_totalAlt_Balance.Text = total_amt;
                lbl_AvailAlt_Balance.Text = Avail_amt;
            HftotalAlt_Balance.Value = lbl_totalAlt_Balance.Text;
            HfAvailAlt_Balance.Value = lbl_AvailAlt_Balance.Text;
        }
        catch (ApplicationException exception)
        {
            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('" + exception.Message + "');", true);
        }
        catch (Exception ex)
        {
            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('Your request could not be completed due to exception. Please intimate technical team for rectification!');", true);
            string message = ExceptionHandler.CreateErrorMessage(ex);
            ExceptionHandler.WriteLog(message);
            ExceptionHandler.WriteException(ex.Message);
        }
        finally
        {

            dsHeadwiseDtls.Clear();
            dsHeadwiseDtls.Dispose();
        }
    }

    #region//Header Details..!

    protected void PopulateYear()
    {
        DataSet dsYear = new DataSet();
        try
        {
            db.Open();
            dsYear = db.ExecuteDataSet(CommandType.Text, "SELECT Fyr FROM Financial_Year ORDER BY Fyr DESC");
            ddlYear.DataSource = dsYear;
            ddlYear.DataValueField = "Fyr";
            ddlYear.DataTextField = "Fyr";
            ddlYear.DataBind();
            ddlYear.SelectedValue = hfF_Year.Value;
        }
        catch (Exception ex)
        {
            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('Your request could not be completed due to exception. Please intimate technical team for rectification!');", true);
            string errorString = ExceptionHandler.CreateErrorMessage(ex);
            ExceptionHandler.WriteLog(errorString);
        }
        finally
        {
            db.Close();
            dsYear.Clear();
            dsYear.Dispose();
            ddlYear.SelectedIndex = utl.ddlSelIndex(ddlYear, hfF_Year.Value);
        }
    }
    private void GetAllQuarters_FromFnYear(string FnYear)
    {
        DataSet dsQuarter = new DataSet();
        try
        {
            db.CreateInParameters(1);
            db.AddInParameters(0, "@FnYear", FnYear);
            db.AddInParameters(1, "@action", "getDDMMYYYY");
            db.Open();
            dsQuarter = db.ExecuteDataSet(CommandType.StoredProcedure, "SNA_GetAllQuarters_FromFnYear");
            db.Close();
            if (dsQuarter.Tables[0].Rows.Count > 0)
            {
                ddlQuarterly.DataSource = dsQuarter;
                ddlQuarterly.DataValueField = "quarter_number";
                ddlQuarterly.DataTextField = "Quarters";
                ddlQuarterly.DataBind();
                ddlQuarterly.Items.Insert(0, "Select Quarter");
            }
        }
        catch (ApplicationException exception)
        {
            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('" + exception.Message + "');", true);
        }
        catch (Exception ex)
        {
            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('Your request could not be completed due to exception. Please intimate technical team for rectification!');", true);
            string message = ExceptionHandler.CreateErrorMessage(ex);
            ExceptionHandler.WriteLog(message);
        }
        finally
        {
            dsQuarter.Clear();
            dsQuarter.Dispose();
        }
    }
    protected void PopulateScheme(string deptCode)
    {
        DataSet dsScheme = new DataSet();
        try
        {
            db.Open();
            Regex regDept = new Regex(@"^\d{2}$");
            Regex regYear = new Regex(@"^\d{4}-\d{2}$");
            if (regDept.IsMatch(deptCode))
            {
                dsScheme = db.ExecuteDataSet(CommandType.Text, "SELECT Scheme_Code,Scheme_Name FROM NHM_Schemes WHERE Dept_Code='" + deptCode + "' AND Active_Status='A' ORDER BY Scheme_Name");
                ddlScheme.DataSource = dsScheme;
                ddlScheme.DataValueField = "Scheme_Code";
                ddlScheme.DataTextField = "Scheme_Name";
                ddlScheme.DataBind();
                ddlScheme.Items.Insert(0, "Select Scheme");
                if (hfSchemeCode.Value != "")
                {
                    ddlScheme.SelectedValue = hfSchemeCode.Value;
                }
            }
            else
                throw new ApplicationException("Invalid Characters!");
        }
        catch (ApplicationException exception)
        {
            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('" + exception.Message + "');", true);
        }
        catch (Exception ex)
        {
            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('Your request could not be completed due to exception. Please intimate technical team for rectification!');", true);
            string errorString = ExceptionHandler.CreateErrorMessage(ex);
            ExceptionHandler.WriteLog(errorString);
        }
        finally
        {
            db.Close();
            dsScheme.Clear();
            dsScheme.Dispose();
        }
    }
    protected void ddlScheme_Changed(object sender, System.EventArgs e)
    {
        panelAction.Visible = false;
    }
    protected void rblOfficeTypeH_Changed(object sender, EventArgs e)
    {
        
        if (rblOfficeTypeH.SelectedValue == "D")
        {
            ViewState["OfficeType"] = "D";
            trDate.Visible = true;
            lblOfficeType.Text = "District";
            lblOfficeType.Visible = true;
            trDistrict.Visible = true;
            rowBlock.Visible = false;
            rw_qtr.Visible = true;
            PopulateDistrictsH();
            panelAction.Visible = false;
        }
        //if (rblOfficeTypeH.SelectedValue == "B")
        //{
        //    ViewState["OfficeType"] = "B";
        //    lblOfficeType.Text = "District";
        //    trDistrict.Visible = true;
        //    rowBlock.Visible = false;
        //    PopulateDistrictsH();
        //    panelAction.Visible = false;
        //}
    }
    protected void PopulateDistrictsH()
    {
        DataSet dsDistrict = new DataSet();
        string strQry = string.Empty;
        try
        {

            if (hfOfficeType.Value == "H")
            {
                if (rblOfficeTypeH.SelectedValue == "H")
                {
                    strQry = "SELECT district_code,district_name FROM MASTER_DISTRICT WHERE district_code='1800'  ORDER BY district_name";
                }
                else if (rblOfficeTypeH.SelectedValue == "D" || rblOfficeTypeH.SelectedValue == "B")
                {
                    strQry = "SELECT district_code,district_name FROM MASTER_DISTRICT WHERE district_code!='1800'  ORDER BY district_name";
                }
            }
            this.db.Open();
            dsDistrict = db.ExecuteDataSet(CommandType.Text, strQry);
            this.db.Close();
            this.ddlDistrict.DataSource = dsDistrict;
            this.ddlDistrict.DataValueField = "district_code";
            this.ddlDistrict.DataTextField = "district_name";
            this.ddlDistrict.DataBind();
            if (rblOfficeTypeH.SelectedValue == "H")
                this.ddlDistrict.Items.Insert(0, "Select Office");
            if (rblOfficeTypeH.SelectedValue == "D")
                this.ddlDistrict.Items.Insert(0, "Select District");
            if (rblOfficeTypeH.SelectedValue == "B")
                this.ddlDistrict.Items.Insert(0, "Select District");
        }
        catch (Exception exception)
        {
            ExceptionHandler.WriteException(exception.Message);
        }
        finally
        {
            dsDistrict.Clear();
            dsDistrict.Dispose();
        }
    }
    protected void PopulateDistrictsD()
    {
        DataSet dsDistrict = new DataSet();
        try
        {
            this.db.Open();
            if (rblOfficeTypeH.SelectedValue == "D")
                dsDistrict = this.db.ExecuteDataSet(CommandType.Text, "SELECT district_code,district_name FROM MASTER_DISTRICT ORDER BY district_name");//WHERE district_code!='2400' 
            else
                dsDistrict = this.db.ExecuteDataSet(CommandType.Text, "SELECT district_code,district_name FROM MASTER_DISTRICT WHERE district_code!='2400'  ORDER BY district_name");
            this.ddlDistrict.DataSource = dsDistrict;
            this.ddlDistrict.DataValueField = "district_code";
            this.ddlDistrict.DataTextField = "district_name";
            this.ddlDistrict.DataBind();

            this.ddlDistrict.Items.Insert(0, "Select District");
            if ((hfUserType.Value == "A" || hfUserType.Value == "H" || hfUserType.Value == "S"))
            {
                string strOfficeCode = hfOfficeCode.Value;
                string DistCode = strOfficeCode.Substring(0, 4);

                ddlDistrict.SelectedIndex = utl.ddlSelIndex(ddlDistrict, DistCode);
                //ddlDistrict.Enabled = false;
                trDistrict.Visible = true;
                PopulateBlock(DistCode);
                rowBlock.Visible = true;
            }
            else
            {
                ddlDistrict.SelectedIndex = utl.ddlSelIndex(ddlDistrict, dsDistrict.Tables[0].Rows[0]["district_code"].ToString());
                //ddlDistrict.Enabled = false;
            }
        }
        catch (Exception exception)
        {
            ExceptionHandler.WriteException(exception.Message);
        }
        finally
        {
            this.db.Close();
            dsDistrict.Clear();
            dsDistrict.Dispose();
        }
    }
    protected void ddlDistrict_Changed(object sender, EventArgs e)
    {
        if (ddlDistrict.SelectedIndex > 0)
        {
            hfDistrictCode.Value = ddlDistrict.SelectedValue;
            if (rblOfficeTypeH.SelectedValue == "B" && ddlDistrict.SelectedIndex > 0)
            {
                PopulateBlock(ddlDistrict.SelectedValue);
                rowBlock.Visible = true;
            }
            else
            {
                ddlBlock.Items.Clear();
                rowBlock.Visible = false;
            }
        }
    }
    protected void PopulateBlock(string DistCode)
    {
        DataSet dataSet = new DataSet();
        try
        {
            this.db.Open();
            Regex regex = new Regex("^\\d{4}$");
            if (!regex.IsMatch(DistCode))
            {
                throw new ApplicationException("Invalid Characters!");
            }
            IDBManager dBManager = this.db;
            string[] strArrays = new string[] { "SELECT block_code,block_name FROM MASTER_BLOCK WHERE district_code='", DistCode, "' ORDER BY block_name" };
            dataSet = dBManager.ExecuteDataSet(CommandType.Text, string.Concat(strArrays));
            this.ddlBlock.DataSource = dataSet;
            this.ddlBlock.DataValueField = "block_code";
            this.ddlBlock.DataTextField = "block_name";
            this.ddlBlock.DataBind();
            //this.ddlBlock.Items.Insert(0, "Select Block");

            this.ddlBlock.Items.Insert(0, "Select Block");
        }
        catch (ApplicationException applicationException1)
        {
            ApplicationException applicationException = applicationException1;
            System.Web.UI.ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", string.Concat("alert('", applicationException.Message, "');"), true);
        }
        catch (Exception exception1)
        {
            Exception exception = exception1;
            System.Web.UI.ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('Your request could not be completed due to exception. Please intimate technical team for rectification!');", true);
            ExceptionHandler.WriteLog(ExceptionHandler.CreateErrorMessage(exception));
        }
        finally
        {
            this.db.Close();
            dataSet.Clear();
            dataSet.Dispose();
        }
    }
    protected void ddlDistrict_DataBound(object sender, EventArgs e)
    {
        foreach (ListItem myItem in ddlDistrict.Items)
        {
            if (myItem.Text == "State Office")
            {
                //Do some things to determine the color of the item
                //Set the item background-color like so:
                //myItem.Attributes.Add("style", "background-color:#9999CD");
                myItem.Attributes.Add("style", "color:#FF3300");
            }
        }
    }
    #endregion 
    protected void btnCancel1_Click(object sender, System.EventArgs e)
    {
        ClearFields_OB();
    }
    protected void ClearFields_OB()
    {
        ddlScheme.SelectedIndex = 0;
        rblOfficeTypeH.SelectedIndex = -1;
        panelAction.Visible = false;
    }
    protected void btnSubmit1_Click(object sender, System.EventArgs e)
    {

        if (ddlScheme.SelectedIndex == 0)
        {
            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('Please Select Scheme!');", true);
            ScriptManager.GetCurrent(Page).SetFocus(ddlScheme);
        }

        else if (rblOfficeTypeH.SelectedIndex == -1)
        {
            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('Please Select Annual ActionPlan for!');", true);
            ScriptManager.GetCurrent(Page).SetFocus(rblOfficeTypeH);
        }
        else if (ddlDistrict.SelectedIndex == -1)
        {
            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('Please Select District!');", true);
            ScriptManager.GetCurrent(Page).SetFocus(ddlDistrict);
        }
        else
        {
            //string OfficeCode = "";
            //string OfficeType = "";
            //if (rblOfficeTypeH.SelectedValue == "H")
            //{
            //    OfficeType = "H";
            //    OfficeCode = hfOfficeCode.Value;

            //}
            //if (rblOfficeTypeH.SelectedValue == "D")
            //{
            //    OfficeType = "D";
            //    OfficeCode = ddlDistrict.SelectedValue + "00";

            //}
            //if (rblOfficeTypeH.SelectedValue == "B")
            //{
            //    OfficeType = "B";
            //    OfficeCode = ddlBlock.SelectedValue;
            //}
            if(rblOfficeTypeH.SelectedValue=="D")
            {
                GetHeadwise_OB_Dtls(ddlYear.SelectedValue, hfDeptCode.Value, ddlScheme.SelectedValue);
            }
            //else
            //{
            //    GetDistwise_OB_Dtls();
            //}

        }
    }


    protected void GetHeadwise_OB_Dtls(string FnYear, string DeptCode, string SchemeCode)
    {
        string OfficeCode = "";
        string OfficeType = "";
        if (rblOfficeTypeH.SelectedValue == "H")
        {
            OfficeType = "H";
            OfficeCode = hfOfficeCode.Value;

        }
        if (rblOfficeTypeH.SelectedValue == "D")
        {
            OfficeType = "D";
            OfficeCode = ddlDistrict.SelectedValue + "00";

        }
        if (rblOfficeTypeH.SelectedValue == "B")
        {
            OfficeType = "B";
            OfficeCode = ddlBlock.SelectedValue;
        }
        DataSet dsHeadwiseDtls = new DataSet();
        try
        {
            db.CreateInParameters(6);
            db.AddInParameters(0, "@action", "fill_allotment");
            db.AddInParameters(1, "@Dept_Code", hfDeptCode.Value);
            db.AddInParameters(2, "@Scheme_Code", SchemeCode);
            db.AddInParameters(3, "@Fn_Year", FnYear);
            db.AddInParameters(4, "@Office_Code", OfficeCode);
            db.AddInParameters(5, "@Office_Type", OfficeType);
            db.Open();
            dsHeadwiseDtls = db.ExecuteDataSet(CommandType.StoredProcedure, "USP_ALLOTMENT_GET_HEADWISE_PROGRAMMEWISE_DATA");
            db.Close();
            if (dsHeadwiseDtls != null && dsHeadwiseDtls.Tables.Count > 0 && dsHeadwiseDtls.Tables[0].Rows.Count > 0)
            {
                gvActionPlan.DataSource = dsHeadwiseDtls;
                gvActionPlan.DataBind();
                gvActionPlan.Columns[15].HeaderText = "Allot Amount <br/> for Qtr-" + ddlQuarterly.SelectedValue + "<br/> (In lakhs)";
                // gvActionPlan.DataBind();
                panelAction.Visible = true;
                panel_dist.Visible = false;

            }
            else
            {
                gvActionPlan.DataSource = dsHeadwiseDtls;
                gvActionPlan.DataBind();
                panelAction.Visible = false;

            }

        }
        catch (ApplicationException exception)
        {
            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('" + exception.Message + "');", true);
        }
        catch (Exception ex)
        {
            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('Your request could not be completed due to exception. Please intimate technical team for rectification!');", true);
            string message = ExceptionHandler.CreateErrorMessage(ex);
            ExceptionHandler.WriteLog(message);
            ExceptionHandler.WriteException(ex.Message);
        }
        finally
        {

            dsHeadwiseDtls.Clear();
            dsHeadwiseDtls.Dispose();
        }
    }
   
    protected void GetDistwise_OB_Dtls()
    {
        string OfficeCode = "";
        string OfficeType = "";
        if (rblOfficeTypeH.SelectedValue == "H")
        {
            OfficeType = "H";
            OfficeCode = hfOfficeCode.Value;

        }
        if (rblOfficeTypeH.SelectedValue == "D")
        {
            OfficeType = "D";
            OfficeCode = ddlDistrict.SelectedValue + "00";

        }
        if (rblOfficeTypeH.SelectedValue == "B")
        {
            OfficeType = "B";
            OfficeCode = ddlBlock.SelectedValue;
        }
        string SQL = string.Empty;
        DataSet dsDistwiseDtls = new DataSet();
        try
        {

            Regex regDept = new Regex(@"^\d{2}$");
            Regex regYear = new Regex(@"^\d{4}-\d{2}$");
            db.Open();
            SQL = "select t1.dist_code,t1.district_name,ISNULL(t2.Allotment_Amt,0.00000) 'amt',ISNULL(t3.ac_amt,0.00000)'Action_amt',0.00000 as blank_amt from (select district_code+'00' 'dist_code',district_name from MASTER_DISTRICT with(nolock) where district_code!='1800') as t1 " +
                "left join(select Office_Code, Allotment_Amt from SNA_Allotment with(nolock) where ProgramName is null) as t2 on t1.dist_code = t2.Office_Code " +
                "left join(select sum(AAP_Amt)'ac_amt' ,Dept_Code,Scheme_Code,F_Year,Office_Code from SNA_AnnualActionPlan_ProgrammeWise_Dtls with(nolock) group by Dept_Code,Scheme_Code,F_Year,Office_Code) as t3 on t1.dist_code = t3.Office_Code  ";
            dsDistwiseDtls = db.ExecuteDataSet(CommandType.Text, SQL);
            db.Close();
            if (dsDistwiseDtls.Tables[0].Rows.Count > 0)
            {
                
                gdv_dist.DataSource = dsDistwiseDtls;
                gdv_dist.DataBind();
                panel_dist.Visible = true;
                panelAction.Visible = false;
            }
            else
            {
                gdv_dist.DataSource = dsDistwiseDtls;
                gdv_dist.DataBind();
                panel_dist.Visible = false;
            }

        }
        catch (ApplicationException exception)
        {
            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('" + exception.Message + "');", true);
        }
        catch (Exception ex)
        {
            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('Your request could not be completed due to exception. Please intimate technical team for rectification!');", true);
            string message = ExceptionHandler.CreateErrorMessage(ex);
            ExceptionHandler.WriteLog(message);
            ExceptionHandler.WriteException(ex.Message);
        }
        finally
        {

            dsDistwiseDtls.Dispose();
            dsDistwiseDtls.Clear();
        }
    }
    protected void btnFinalSave_Click(object sender, System.EventArgs e)
    {
        FinalSaveData();
        lblMsg.Text = "";
    }

    public string Covert_To_DB_Date_Format_MMDDYYYY(string pstrDate)
    {
        if (pstrDate == "")
            return "";
        else
        {
            string[] Temp_Date = pstrDate.Split('/');
            pstrDate = Temp_Date[1] + "-" + Temp_Date[0] + "-" + Temp_Date[2];

            //CultureInfo provider = new CultureInfo("en-gb", true);
            //return Convert.ToDateTime(DateTime.Parse(pstrDate, provider)).ToString("dd/MM/yyyy");
            return pstrDate;
        }
    }
    protected void FinalSaveData()
    {
        //if (Session["AuthTokenPage"] == null || utl.validateEmptyString(hfSession.Value.ToString()) || !utl.validateAphaNumeric(hfSession.Value.ToString(), 500))
        //{
        //    ExceptionHandler.WriteException("Session Value in Cookie And Hidden Field Does not Match in Scheme Sanction");
        //    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('Session Expaired. Please Login again!');", true);
        //    utl.SessionReset();
        //}
        //else if (!((hfSession.Value == Session["AuthTokenPage"].ToString()) || Session["AuthTokenPage"] != null))
        //{
        //    ExceptionHandler.WriteException("Session Value in Cookie And Hidden Field Does not Match in Scheme Sanction");
        //    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('Session Expaired. Please Login again!');", true);
        //    utl.SessionReset();
        //}
        //else
        //{
        if (ddlScheme.SelectedIndex == 0)
        {
            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('Please Select Scheme!');", true);
            ScriptManager.GetCurrent(Page).SetFocus(ddlScheme);
        }

        else
        {
            try
            {
                DateTime dt = new DateTime();
                string date = null;
                Regex regDept = new Regex(@"^\d{2}$");
                Regex regSch = new Regex(@"^\d{3}$");
                Regex regOff = new Regex(@"^\d{6}$");
                Regex regYear = new Regex(@"^\d{4}-\d{2}$");
               
                string dept = hfDeptCode.Value;
                string sch = ddlScheme.SelectedValue;
                string fyear = ddlYear.SelectedValue;
                string OfficeCode = hfOfficeCode.Value;
                string OfficeType = "";
               // Covert_To_DB_Date_Format_MMDDYYYY(pstrDate: txt_date.Text);
                
                if (rblOfficeTypeH.SelectedValue == "H")
                {
                    OfficeType = "H";
                    OfficeCode = hfOfficeCode.Value;

                }
                if (rblOfficeTypeH.SelectedValue == "D")
                {
                    OfficeType = "D";
                    OfficeCode = ddlDistrict.SelectedValue + "00";

                }
                if (rd_alt.SelectedValue == "A")
                {
                    date = DateTime.Now.ToString();
                    Covert_To_DB_Date_Format_MMDDYYYY(date);
                    dt = Convert.ToDateTime(date);
                }
                if (rblOfficeTypeH.SelectedValue == "B")
                {
                    OfficeType = "B";
                    OfficeCode = ddlBlock.SelectedValue;
                }
                if (regDept.IsMatch(dept) && regSch.IsMatch(sch) && regYear.IsMatch(fyear))
                {
                    DataSet dsAAP = new DataSet();
                    // CRTEATE DATA TABLE
                    DataTable dtAAP = new DataTable();
                    DataColumn colAAP;
                    decimal id;

                    colAAP = new DataColumn();
                    colAAP.DataType = Type.GetType("System.String");
                    colAAP.ColumnName = "Dept_Code";
                    dtAAP.Columns.Add(colAAP);

                    colAAP = new DataColumn();
                    colAAP.DataType = Type.GetType("System.String");
                    colAAP.ColumnName = "Scheme_Code";
                    dtAAP.Columns.Add(colAAP);

                    colAAP = new DataColumn();
                    colAAP.DataType = Type.GetType("System.String");
                    colAAP.ColumnName = "F_Year";
                    dtAAP.Columns.Add(colAAP);

                    colAAP = new DataColumn();
                    colAAP.DataType = Type.GetType("System.String");
                    colAAP.ColumnName = "Office_Code";
                    dtAAP.Columns.Add(colAAP);

                    colAAP = new DataColumn();
                    colAAP.DataType = Type.GetType("System.String");
                    colAAP.ColumnName = "Office_Type";
                    dtAAP.Columns.Add(colAAP);

                    colAAP = new DataColumn();
                    colAAP.DataType = Type.GetType("System.Int32");
                    colAAP.ColumnName = "SubScheme_Code";
                    dtAAP.Columns.Add(colAAP);

                    colAAP = new DataColumn();
                    colAAP.DataType = Type.GetType("System.Int32");
                    colAAP.ColumnName = "Head_Code";
                    dtAAP.Columns.Add(colAAP);

                    colAAP = new DataColumn();
                    colAAP.DataType = Type.GetType("System.String");
                    colAAP.ColumnName = "Program_Name";
                    dtAAP.Columns.Add(colAAP);

                    colAAP = new DataColumn();
                    colAAP.DataType = Type.GetType("System.Decimal");
                    colAAP.ColumnName = "Allotment_Amt";
                    dtAAP.Columns.Add(colAAP);

                    colAAP = new DataColumn();
                    colAAP.DataType = Type.GetType("System.String");
                    colAAP.ColumnName = "Entry_By";

                    dtAAP.Columns.Add(colAAP);

                    foreach (GridViewRow gr in gvActionPlan.Rows)
                    {
                        //TextBox txtAAP_Amt = (TextBox)gvActionPlan.Rows[i].Cells[5].FindControl("txtBtxtAAP_Amtudget_Amt");
                        //if (txtBudget_Amt.Text != "0.00000")
                        //{

                        //}
                        Label lblSubSchemeCode = (Label)gr.FindControl("lblSubSchemeCode");
                        Label lblHeadCode = (Label)gr.FindControl("lblheadcode");
                        Label lblProgramName = (Label)gr.FindControl("lblProgramName");

                        TextBox txtAAP_Amt = (TextBox)gr.FindControl("txtAAP_Amt");
                        if (txtAAP_Amt.Text.Trim() != "" && txtAAP_Amt.Text.Trim() != "0.00000")
                        {
                            if (decimal.Parse(txtAAP_Amt.Text.Trim()) > 0)
                            {

                                DataRow dr = dtAAP.NewRow();
                                dr["Dept_Code"] = hfDeptCode.Value;
                                dr["Scheme_Code"] = ddlScheme.SelectedItem.Value;
                                dr["F_Year"] = ddlYear.SelectedItem.Value;
                                dr["Office_Code"] = OfficeCode;
                                dr["Office_Type"] = OfficeType;
                                dr["SubScheme_Code"] = lblSubSchemeCode.Text;
                                dr["Head_Code"] = lblHeadCode.Text;
                                dr["Program_Name"] = lblProgramName.Text;

                                if (Decimal.TryParse(txtAAP_Amt.Text.Trim(), out id))
                                {
                                    if (id < 0)
                                        throw new ApplicationException("Allotment Amount can not be less than 0!");
                                    else
                                        dr["Allotment_Amt"] = txtAAP_Amt.Text.Trim();
                                }
                                else
                                    throw new ApplicationException("AnnualActionPlan Amount!");

                                dr["Entry_By"] = hfUserID.Value;
                                dtAAP.Rows.Add(dr);
                                dtAAP.AcceptChanges();

                            }
                        }
                    }
                    StringBuilder sbSql = new StringBuilder();
                    StringWriter swSql = new StringWriter(sbSql);
                    string XmlFormat;
                    dsAAP.Merge(dtAAP, true, MissingSchemaAction.AddWithKey);
                    dsAAP.Tables[0].TableName = "AllotmentTable";
                    foreach (DataColumn col in dsAAP.Tables[0].Columns)
                    {
                        col.ColumnMapping = MappingType.Attribute;
                    }
                    dsAAP.WriteXml(swSql, XmlWriteMode.WriteSchema);
                    XmlFormat = sbSql.ToString();
                    db.Open();
                    db.CreateInParameters(8);
                    db.AddInParameters(0, "@Dept_Code", hfDeptCode.Value);
                    db.AddInParameters(1, "@Scheme_Code", ddlScheme.SelectedItem.Value);
                    db.AddInParameters(2, "@Office_Code", OfficeCode);
                    db.AddInParameters(3, "@Fn_Year", ddlYear.SelectedItem.Value);
                    db.AddInParameters(4, "@Qtr", ddlQuarterly.SelectedValue);
                    db.AddInParameters(5, "@Qtr_Date", date == null ? System.Convert.DBNull : dt);
                    db.AddInParameters(6, "@Entry_By", hfUserID.Value);
                    db.AddInParameters(7, "@XmlString", XmlFormat);
                    db.CreateOutParameters(1);
                    db.AddOutParameters(0, "@msg", 1, 100);
                    db.ExecuteNonQuery(CommandType.StoredProcedure, "SNA_Allotment_ProgrammeWise");

                    // MAINTAIN ACTIVITY LOG ON ACCESSING PAGE
                    //
                    string msg = db.outParameters[0].Value.ToString();
                    db.Close();
                    if (msg.ToString() == "Allotment Submited Successfully")
                    {

                        int activityid;
                        ActivityLog activity = new ActivityLog();
                        activity.UserID = hfUserID.Value;
                        activity.UserIP = Convert.ToString(HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]);
                        activity.ActivityType = "Action";
                        activity.Activity = "Opening Balance Entry";
                        activity.PageURL = System.Web.HttpContext.Current.Request.Url.ToString();
                        activity.Remark = db.outParameters[0].Value.ToString(); ;
                        activityid = ActivityLog.InsertActivityLog(activity);
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('" + msg + "');", true);
                        //ClearFields_OB();
                        panelAction.Visible = true;
                        //pnlGrid_OB.Visible = true;

                    }
                    else
                    {
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('" + msg + "');", true);
                        panelAction.Visible = true;
                        //pnlGrid_OB.Visible = false;
                    }
                    GetHeadwise_OB_Dtls(ddlYear.SelectedValue, hfDeptCode.Value, ddlScheme.SelectedValue);
                }
                else
                    throw new ApplicationException("Invalid Characters!");
            }
            catch (ApplicationException exception)
            {
                ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('" + exception.Message + "');", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('Your request could not be completed due to exception. Please intimate technical team for rectification!');", true);
                string errorString = ExceptionHandler.CreateErrorMessage(ex);
                ExceptionHandler.WriteLog(errorString);
            }
            finally
            {

            }
        }
        //}
    }

    decimal Qtr4Allotment_Amt = 0;
    decimal Qtr3Allotment_Amt = 0;
    decimal Qtr2Allotment_Amt = 0;
    decimal Qtr1Allotment_Amt = 0;
    decimal AnnualActionPlanAmt = 0;
    decimal All4QuaterAmt = 0;
    decimal Exp_Amt = 0;
    decimal Balance_Amt = 0;
    decimal Total_OB_Sanction_Amt = 0;
    protected void gvActionPlan_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            Qtr4Allotment_Amt += DataBinder.Eval(e.Row.DataItem, "Qtr4Allotment_Amt").ToString() == "" ? 0 : Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "Qtr4Allotment_Amt").ToString());
            Qtr3Allotment_Amt += DataBinder.Eval(e.Row.DataItem, "Qtr3Allotment_Amt").ToString() == "" ? 0 : Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "Qtr3Allotment_Amt").ToString());
            Qtr2Allotment_Amt += DataBinder.Eval(e.Row.DataItem, "Qtr2Allotment_Amt").ToString() == "" ? 0 : Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "Qtr2Allotment_Amt").ToString());
            Qtr1Allotment_Amt += DataBinder.Eval(e.Row.DataItem, "Qtr1Allotment_Amt").ToString() == "" ? 0 : Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "Qtr1Allotment_Amt").ToString());
            AnnualActionPlanAmt += DataBinder.Eval(e.Row.DataItem, "AnnualActionPlanAmt").ToString() == "" ? 0 : Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "AnnualActionPlanAmt").ToString());
            All4QuaterAmt += DataBinder.Eval(e.Row.DataItem, "All4QuaterAmt").ToString() == "" ? 0 : Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "All4QuaterAmt").ToString());
            Exp_Amt += DataBinder.Eval(e.Row.DataItem, "Exp_Amt").ToString() == "" ? 0 : Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "Exp_Amt").ToString());
            Balance_Amt += DataBinder.Eval(e.Row.DataItem, "Balance_Amt").ToString() == "" ? 0 : Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "Balance_Amt").ToString());
            Total_OB_Sanction_Amt += DataBinder.Eval(e.Row.DataItem, "Total_OB_Sanction_Amt").ToString() == "" ? 0 : Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "Total_OB_Sanction_Amt").ToString());

        }
        if (e.Row.RowType == DataControlRowType.Footer)
        {
            Label lblfQtr4Allotment_Amt = (Label)e.Row.FindControl("lblfQtr4Allotment_Amt");
            Label lblfQtr3Allotment_Amt = (Label)e.Row.FindControl("lblfQtr3Allotment_Amt");
            Label lblfQtr2Allotment_Amt = (Label)e.Row.FindControl("lblfQtr2Allotment_Amt");
            Label lblfQtr1Allotment_Amt = (Label)e.Row.FindControl("lblfQtr1Allotment_Amt");

            Label lblAnnualActionPlanAmt = (Label)e.Row.FindControl("lblfAnnualActionPlanAmt");
            Label lblAll4QuaterAmt = (Label)e.Row.FindControl("lblfAll4QuaterAmt");
            Label lblExp_Amt = (Label)e.Row.FindControl("lblfExp_Amt");
            Label lblBalance_Amt = (Label)e.Row.FindControl("lblfBalance_Amt");
            Label lblfTotal_OB_Sanction_Amt = (Label)e.Row.FindControl("lblfTotal_OB_Sanction_Amt");

            lblfQtr4Allotment_Amt.Text = Qtr4Allotment_Amt.ToString();
            lblfQtr3Allotment_Amt.Text = Qtr3Allotment_Amt.ToString();
            lblfQtr2Allotment_Amt.Text = Qtr2Allotment_Amt.ToString();
            lblfQtr1Allotment_Amt.Text = Qtr1Allotment_Amt.ToString();

            lblAnnualActionPlanAmt.Text = AnnualActionPlanAmt.ToString();
            lblAll4QuaterAmt.Text = All4QuaterAmt.ToString();
            lblExp_Amt.Text = Exp_Amt.ToString();
            lblBalance_Amt.Text = Balance_Amt.ToString();
            lblfTotal_OB_Sanction_Amt.Text = Total_OB_Sanction_Amt.ToString();
        }
    }

    protected void gvActionPlan_Entered(object sender, EventArgs e)
    {
        GridViewRow row = (sender as TextBox).NamingContainer as GridViewRow;
        Label lblAnnualActionPlanAmt = (Label)row.FindControl("lblAnnualActionPlanAmt");
        Label lblAll4QuaterAmt = (Label)row.FindControl("lblAll4QuaterAmt");
        TextBox txtAAP_Amt = (TextBox)row.FindControl("txtAAP_Amt");
        if (txtAAP_Amt.Text != "")
        {
            decimal avail_balnce = decimal.Parse(HfAvailAlt_Balance.Value);
            decimal tobeAllot = decimal.Parse(lblAnnualActionPlanAmt.Text) - decimal.Parse(lblAll4QuaterAmt.Text);
            if (decimal.Parse(txtAAP_Amt.Text) > tobeAllot)
            {
                ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('Allotment must not be grater than action plan  you can only allot " + tobeAllot.ToString() + " lakhs!');", true);
                txtAAP_Amt.Text = "0.00000";
            }
            else if (decimal.Parse(txtAAP_Amt.Text) > avail_balnce)
            {
                ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('Allotment must not be grater than Available Balance  you can only allot " + avail_balnce.ToString() + " lakhs!');", true);
                txtAAP_Amt.Text = "0.00000";
            }
            else
            {
                calculateTotalAAP_Amt();
                int TotalRow = gvActionPlan.Rows.Count;
                int nextIndex = row.RowIndex + 1;
                if (nextIndex < TotalRow)
                {
                    TextBox txtbx = (TextBox)gvActionPlan.Rows[nextIndex].FindControl("txtAAP_Amt");
                    txtbx.Focus();
                }
            }
        }
    }
    void calculateTotalAAP_Amt()
    {

        if (rblOfficeTypeH.SelectedValue == "H")
            OfficeType_Chk = "H";
        if (rblOfficeTypeH.SelectedValue == "D")
            OfficeType_Chk = "D";
        if (rblOfficeTypeH.SelectedValue == "B")
            OfficeType_Chk = "B";
        //decimal Budget_Amt = ConvertText_To_Decimal(txtBudget_Amt.Text.Trim());
        //decimal Total_AAP_Amt = ConvertText_To_Decimal(lbl_Total_AAPAmt.Text.Trim());

        //decimal StateTotal = ConvertText_To_Decimal(lbl_StateOffice_AAPAmt.Text.Trim());
        //decimal DistrictTotal = ConvertText_To_Decimal(lbl_DistrictOffice_AAPAmt.Text.Trim());
        //decimal BlockTotal = ConvertText_To_Decimal(lbl_BlockOffice_AAPAmt.Text.Trim());
        Label lblfblank_Amt = (Label)this.gvActionPlan.FooterRow.FindControl("lblfblank_Amt");
        decimal SanctionAmt = new decimal(0);
        int i = 0;
        TextBox textBox = new TextBox();
        foreach (GridViewRow row in this.gvActionPlan.Rows)
        {
            textBox = (TextBox)row.FindControl("txtAAP_Amt");
            if (textBox.Text != "")
            {
                SanctionAmt = SanctionAmt + Convert.ToDecimal(textBox.Text);
            }
        }
        lblfblank_Amt.Text = SanctionAmt.ToString();

    }
    protected decimal ConvertText_To_Decimal(string value)
    {
        try
        {
            return Convert.ToDecimal(value);
        }
        catch (Exception ex)
        {
            string msg = ex.Message;
            return 0;
        }
    }

    decimal AnnualActionPlanDistAmt = 0;
    decimal Alt_distAmt = 0;
    protected void gdv_dist_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {

            AnnualActionPlanDistAmt += DataBinder.Eval(e.Row.DataItem, "Action_amt").ToString() == "" ? 0 : Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "Action_amt").ToString());
            Alt_distAmt += DataBinder.Eval(e.Row.DataItem, "amt").ToString() == "" ? 0 : Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "amt").ToString());
            

        }
        if (e.Row.RowType == DataControlRowType.Footer)
        {
            
            Label lblfdistAnnualActionPlanAmt = (Label)e.Row.FindControl("lblfdistAnnualActionPlanAmt");
            Label lblfAltAmt = (Label)e.Row.FindControl("lblfAltAmt");




            lblfdistAnnualActionPlanAmt.Text = AnnualActionPlanDistAmt.ToString();
            lblfAltAmt.Text = Alt_distAmt.ToString();
            
        }
    }
    void calculateTotalDistAAP_Amt()
    {
        if (rblOfficeTypeH.SelectedValue == "H")
            OfficeType_Chk = "H";
        if (rblOfficeTypeH.SelectedValue == "D")
            OfficeType_Chk = "D";
        if (rblOfficeTypeH.SelectedValue == "B")
            OfficeType_Chk = "B";
        //decimal Budget_Amt = ConvertText_To_Decimal(txtBudget_Amt.Text.Trim());
        //decimal Total_AAP_Amt = ConvertText_To_Decimal(lbl_Total_AAPAmt.Text.Trim());

        //decimal StateTotal = ConvertText_To_Decimal(lbl_StateOffice_AAPAmt.Text.Trim());
        //decimal DistrictTotal = ConvertText_To_Decimal(lbl_DistrictOffice_AAPAmt.Text.Trim());
        //decimal BlockTotal = ConvertText_To_Decimal(lbl_BlockOffice_AAPAmt.Text.Trim());
        Label lblfDistblank_Amt = (Label)this.gdv_dist.FooterRow.FindControl("lblfDistblank_Amt");
        decimal SanctionAmt = new decimal(0);
        int i = 0;
        TextBox textBox = new TextBox();
        foreach (GridViewRow row in this.gdv_dist.Rows)
        {
            textBox = (TextBox)row.FindControl("txtDistAAP_Amt");
            if (textBox.Text != "")
            {
                SanctionAmt = SanctionAmt + Convert.ToDecimal(textBox.Text);
            }
        }
        lblfDistblank_Amt.Text = SanctionAmt.ToString();
    }
    protected void txtDistAAP_Amt_TextChanged(object sender, EventArgs e)
    {
        GridViewRow row = (sender as TextBox).NamingContainer as GridViewRow;
        Label lbldistAnnualActionPlanAmt = (Label)row.FindControl("lbldistAnnualActionPlanAmt");
        Label lblAltAmt = (Label)row.FindControl("lblAltAmt");
        TextBox txtDistAAP_Amt = (TextBox)row.FindControl("txtDistAAP_Amt");
        if (txtDistAAP_Amt.Text != "")
        {
            decimal avail_balnce = decimal.Parse(HfAvailAlt_Balance.Value);
            decimal tobeAllot = decimal.Parse(lbldistAnnualActionPlanAmt.Text) - decimal.Parse(lblAltAmt.Text);
            if (decimal.Parse(txtDistAAP_Amt.Text) > tobeAllot)
            {
                ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('Allotment must not be grater than action plan  you can only allot " + tobeAllot.ToString() + " lakhs!');", true);
                txtDistAAP_Amt.Text = "0.00000";
            }
           else if (decimal.Parse(txtDistAAP_Amt.Text) > avail_balnce)
            {
                ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('Allotment must not be grater than Available Balance  you can only allot " + avail_balnce.ToString() + " lakhs!');", true);
                txtDistAAP_Amt.Text = "0.00000";
            }
            else
            {
                calculateTotalDistAAP_Amt();
                int TotalRow = gdv_dist.Rows.Count;
                int nextIndex = row.RowIndex + 1;
                if (nextIndex < TotalRow)
                {
                    TextBox txtbx = (TextBox)gdv_dist.Rows[nextIndex].FindControl("txtDistAAP_Amt");
                    txtbx.Focus();
                }
            }
        }
    }

    protected void btn_distSave_Click(object sender, EventArgs e)
    {
        if (ddlScheme.SelectedIndex == 0)
        {
            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('Please Select Scheme!');", true);
            ScriptManager.GetCurrent(Page).SetFocus(ddlScheme);
        }

        else
        {
            try
            {
                Regex regDept = new Regex(@"^\d{2}$");
                Regex regSch = new Regex(@"^\d{3}$");
                Regex regOff = new Regex(@"^\d{6}$");
                Regex regYear = new Regex(@"^\d{4}-\d{2}$");
                string dept = hfDeptCode.Value;
                string sch = ddlScheme.SelectedValue;
                string fyear = ddlYear.SelectedValue;
                string OfficeCode = hfOfficeCode.Value;
                string OfficeType = "";
                if (rblOfficeTypeH.SelectedValue == "H")
                {
                    OfficeType = "H";
                    OfficeCode = hfOfficeCode.Value;

                }
                if (rblOfficeTypeH.SelectedValue == "D")
                {
                    OfficeType = "D";
                    OfficeCode = ddlDistrict.SelectedValue + "00";

                }
                if (rblOfficeTypeH.SelectedValue == "B")
                {
                    OfficeType = "B";
                    OfficeCode = ddlBlock.SelectedValue;
                }
                if (regDept.IsMatch(dept) && regSch.IsMatch(sch) && regYear.IsMatch(fyear))
                {
                    DataSet dsAAP = new DataSet();
                    // CRTEATE DATA TABLE
                    DataTable dtAAP = new DataTable();
                    DataColumn colAAP;
                    decimal id;

                    colAAP = new DataColumn();
                    colAAP.DataType = Type.GetType("System.String");
                    colAAP.ColumnName = "Dept_Code";
                    dtAAP.Columns.Add(colAAP);

                    colAAP = new DataColumn();
                    colAAP.DataType = Type.GetType("System.String");
                    colAAP.ColumnName = "Scheme_Code";
                    dtAAP.Columns.Add(colAAP);

                    colAAP = new DataColumn();
                    colAAP.DataType = Type.GetType("System.String");
                    colAAP.ColumnName = "F_Year";
                    dtAAP.Columns.Add(colAAP);

                    colAAP = new DataColumn();
                    colAAP.DataType = Type.GetType("System.String");
                    colAAP.ColumnName = "Office_Code";
                    dtAAP.Columns.Add(colAAP);

                    colAAP = new DataColumn();
                    colAAP.DataType = Type.GetType("System.String");
                    colAAP.ColumnName = "Office_Type";
                    dtAAP.Columns.Add(colAAP);

                    colAAP = new DataColumn();
                    colAAP.DataType = Type.GetType("System.Decimal");
                    colAAP.ColumnName = "Allotment_Amt";
                    dtAAP.Columns.Add(colAAP);

                    colAAP = new DataColumn();
                    colAAP.DataType = Type.GetType("System.String");
                    colAAP.ColumnName = "Entry_By";

                    dtAAP.Columns.Add(colAAP);

                    foreach (GridViewRow gr in gdv_dist.Rows)
                    {
                        //TextBox txtAAP_Amt = (TextBox)gvActionPlan.Rows[i].Cells[5].FindControl("txtBtxtAAP_Amtudget_Amt");
                        //if (txtBudget_Amt.Text != "0.00000")
                        //{

                        //}
                        Label lbldistCode = (Label)gr.FindControl("lbldistCode");
                       

                        TextBox txtDistAAP_Amt = (TextBox)gr.FindControl("txtDistAAP_Amt");
                        if (txtDistAAP_Amt.Text.Trim() != "" && txtDistAAP_Amt.Text.Trim() != "0.00000")
                        {
                            if (decimal.Parse(txtDistAAP_Amt.Text.Trim()) > 0)
                            {

                                DataRow dr = dtAAP.NewRow();
                                dr["Dept_Code"] = hfDeptCode.Value;
                                dr["Scheme_Code"] = ddlScheme.SelectedItem.Value;
                                dr["F_Year"] = ddlYear.SelectedItem.Value;
                                dr["Office_Code"] = lbldistCode.Text;
                                dr["Office_Type"] = OfficeType;
                               

                                if (Decimal.TryParse(txtDistAAP_Amt.Text.Trim(), out id))
                                {
                                    if (id < 0)
                                        throw new ApplicationException("Allotment Amount can not be less than 0!");
                                    else
                                        dr["Allotment_Amt"] = txtDistAAP_Amt.Text.Trim();
                                }
                                else
                                    throw new ApplicationException("AnnualActionPlan Amount!");

                                dr["Entry_By"] = hfUserID.Value;
                                dtAAP.Rows.Add(dr);
                                dtAAP.AcceptChanges();

                            }
                        }
                    }
                    StringBuilder sbSql = new StringBuilder();
                    StringWriter swSql = new StringWriter(sbSql);
                    string XmlFormat;
                    dsAAP.Merge(dtAAP, true, MissingSchemaAction.AddWithKey);
                    dsAAP.Tables[0].TableName = "AllotmentTable";
                    foreach (DataColumn col in dsAAP.Tables[0].Columns)
                    {
                        col.ColumnMapping = MappingType.Attribute;
                    }
                    dsAAP.WriteXml(swSql, XmlWriteMode.WriteSchema);
                    XmlFormat = sbSql.ToString();
                    db.Open();
                    db.CreateInParameters(6);
                    db.AddInParameters(0, "@Dept_Code", hfDeptCode.Value);
                    db.AddInParameters(1, "@Scheme_Code", ddlScheme.SelectedItem.Value);
                    db.AddInParameters(2, "@Office_Code", OfficeCode);
                    db.AddInParameters(3, "@Fn_Year", ddlYear.SelectedItem.Value);
                  
                    db.AddInParameters(4, "@Entry_By", hfUserID.Value);
                    db.AddInParameters(5, "@XmlString", XmlFormat);
                    db.CreateOutParameters(1);
                    db.AddOutParameters(0, "@msg", 1, 100);
                    db.ExecuteNonQuery(CommandType.StoredProcedure, "SNA_Allotment_ToDistrict");

                    // MAINTAIN ACTIVITY LOG ON ACCESSING PAGE
                    //
                    string msg = db.outParameters[0].Value.ToString();
                    db.Close();
                    if (msg.ToString() == "Allotment Submited Successfully")
                    {

                        int activityid;
                        ActivityLog activity = new ActivityLog();
                        activity.UserID = hfUserID.Value;
                        activity.UserIP = Convert.ToString(HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]);
                        activity.ActivityType = "Action";
                        activity.Activity = "Opening Balance Entry";
                        activity.PageURL = System.Web.HttpContext.Current.Request.Url.ToString();
                        activity.Remark = db.outParameters[0].Value.ToString(); ;
                        activityid = ActivityLog.InsertActivityLog(activity);
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('" + msg + "');", true);
                        //ClearFields_OB();
                        panel_dist.Visible = true;
                        //pnlGrid_OB.Visible = true;

                    }
                    else
                    {
                        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('" + msg + "');", true);
                        panel_dist.Visible = true;
                        //pnlGrid_OB.Visible = false;
                    }
                    GetDistwise_OB_Dtls();
                }
                else
                    throw new ApplicationException("Invalid Characters!");
            }
            catch (ApplicationException exception)
            {
                ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('" + exception.Message + "');", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "asyncPostBack", "alert('Your request could not be completed due to exception. Please intimate technical team for rectification!');", true);
                string errorString = ExceptionHandler.CreateErrorMessage(ex);
                ExceptionHandler.WriteLog(errorString);
            }
            finally
            {

            }
        }
    }
}