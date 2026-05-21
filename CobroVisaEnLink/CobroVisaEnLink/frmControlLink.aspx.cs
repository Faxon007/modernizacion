using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class frmControlLink : System.Web.UI.Page
{
    private string error;

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
            #region validacion de permisos
            Response.Cookies.Add(new HttpCookie("returnUrl", Request.Url.PathAndQuery));
            if (Session["usuario"] == null)
                Response.Redirect("Default.aspx");

            controllerMenu objControllerMenu = new controllerMenu(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());
            if (!objControllerMenu.verificarMenuItem(Session["usuario"].ToString(), "9505"))
                Response.Redirect("frmIndex.aspx");
            #endregion

            #region inicializacion de pagina
            Master.strPagina = "Consulta emision links";
            #endregion      

            //if (!cargarDatos())
            //    throw new Exception(error);

            if (!IsPostBack)
            {

            }
        }
        catch (Exception ex)
        {
            Master.strMensajeError = ex.Message;
        }
    }

    private bool cargarDatos()
    {
        try
        {
            /*controllerCliente objcontrollerCliente = new controllerCliente(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());
            if (!objcontrollerCliente.getClientes())
                throw new Exception(objcontrollerCliente.strReturnMessage);

            this.gvClientes.DataSource = objcontrollerCliente.dsReturn.Tables["clientes"];
            gvClientes.DataBind();

            if (gvClientes.Rows.Count != 0)
                gvClientes.HeaderRow.TableSection = TableRowSection.TableHeader;*/
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }


}