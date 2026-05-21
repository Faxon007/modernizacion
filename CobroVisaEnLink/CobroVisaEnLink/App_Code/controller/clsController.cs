using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

/// <summary>
/// Summary description for clsController
/// </summary>
public class clsController
{
    protected string strUsuarioConexion;
    protected string strPassword;
    protected string strPath;
    protected DataSet ds;
    protected string strMensaje;

    //string strMsj;
    DataSet dsPrivate;

    public string strReturnMessage
    {
        get { return this.strMensaje; }
        set { this.strMensaje = value; }
    }
    public DataSet dsReturn
    {
        get { return this.dsPrivate; }
        set { this.dsPrivate = value; }
    }

	public clsController(string strUsuarioParam, string strPasswordParam, string strPathParam)
	{
		//
		// TODO: Add constructor logic here
		//
        this.strUsuarioConexion = strUsuarioParam;
        this.strPassword = strPasswordParam;
        this.strPath = strPathParam;
	}

}
