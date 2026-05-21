using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _Default : System.Web.UI.Page
{
    protected void Page_PreLoad(object sender, EventArgs e)
    {
        Master.strMensajeError = "";
        Master.strMensajeErrorModal = "";
        Master.strMensajeConfirmacion = "";
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            Master.strPagina = "";
            if (!this.IsPostBack)
            {
                Master.strMensajeError = "";
                Master.strMensajeErrorModal = "";
                Master.strMensajeConfirmacion = "";
                Session.Clear();
            }
            
        }
        catch (Exception ex)
        {
            Master.strMensajeError = ex.Message;
        }

    }
    protected void loginUsuario_Authenticate(object sender, AuthenticateEventArgs e)
    {

        if (!Master.autenticarUsuario(loginUsuario.UserName, loginUsuario.Password))
        {
            //loginUsuario.FailureText = Master.strMensaje;
            //Master.strMensaje = "";
            e.Authenticated = false;
        }
        else
        {
            Session["reinicia_password"] = false;

            HttpCookie returnCookie = Request.Cookies["returnUrl"];
            if ((returnCookie == null) || string.IsNullOrEmpty(returnCookie.Value))
            {
                Response.Redirect("frmIndex.aspx");
            }
            else
            {
                HttpCookie deleteCookie = new HttpCookie("returnUrl");
                deleteCookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(deleteCookie);
                Response.Redirect(returnCookie.Value);
            }
        }


    }    
}