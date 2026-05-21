using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class frmActivacion : System.Web.UI.Page
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

            //controllerMenu objControllerMenu = new controllerMenu(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());
            //if (!objControllerMenu.verificarMenuItem(Session["usuario"].ToString(), "9503"))
            //    Response.Redirect("frmIndex.aspx");
            #endregion

            #region inicializacion de pagina
            Master.strPagina = "Estatus Link";
            string cod_cliente = Request.QueryString["cli"];
            if (cod_cliente != null)
                Master.strPagina = "Editar Enrolamiento Cliente";
            #endregion

            if (!this.IsPostBack)
            {
                ////
                //for (int i = 1; i <= DateTime.DaysInMonth(DateTime.Now.Year, 1); i++)
                //    ddlDia.Items.Add(new ListItem(i.ToString(), i.ToString()));

                //ddlDia.SelectedValue = DateTime.Now.Day.ToString();

            }

            //ScriptManager.RegisterStartupScript(this, this.GetType(), "LoadModal", "cargar_scripts();", true);
            //ScriptManager.RegisterStartupScript(this, this.GetType(), "LoadModal", "cargar_scripts();loadModal();", true);
        }
        catch (Exception ex)
        {
            Master.strMensajeError = ex.Message;
        }

    }

    protected void btnBuscarLink_Click(object sender, EventArgs e)
    {
        try
        {
            if (txtSKU.Text != "")
            {
                //se obtiene los productos que tiene asociados 
                if (!this.buscarLink())
                    throw new Exception(error);
            }
            //else
            //this.lblCuenta.Text = "Seleccione un número de cuenta primero.";
        }
        catch (Exception ex)
        {
            //error = ex.Message;
            Master.strMensajeError = ex.Message;
        }
    }
    public bool buscarLink()
    {
        try
        {
            VisaEnLink Link = new VisaEnLink(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());

            clsPago objPago = new clsPago();

            objPago.cod_sku = txtSKU.Text;

            if (!Link.ExisteToken())
                throw new Exception("Error: se presento un error al revisar token. " + Link.error);

            if (!Link.ConsultaLink(objPago))
                throw new Exception("Error: se presento error al consultar autorizacion. " + Link.error);

            if (Link.ObtengoEstadoLink() == "NO")
            {
                if (!Link.CambioEstado(txtSKU.Text))
                    throw new Exception("Error: al cambiar el estado del link "+Link.error);

                Master.strMensajeConfirmacion = "Link Actualizado!!!";
                Session["PageRedirect"] = true;
                Response.Redirect("frmIndex.aspx", false);
            }
            //else
            //{
            //}
            
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
}