using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class frmCancelarLink : System.Web.UI.Page
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
            if (!objControllerMenu.verificarMenuItem(Session["usuario"].ToString(), "9504"))
                Response.Redirect("frmIndex.aspx");
            #endregion

            #region inicializacion de pagina
            Master.strPagina = "Cancelar Programación Link";
            string cod_cliente = Request.QueryString["cli"];
            #endregion

            if (!this.IsPostBack)
            {

                //if (!cargarDatos())
                //    throw new Exception(error);

                //cod_cliente = Request.QueryString["cli"];
                //if (cod_cliente != null)
                //{
                //    //ddlEstado.Visible = true;
                //    //lblEstado.Visible = true;
                //    //ddlBloqueo.Visible = true;
                //    //lblBloqueo.Visible = true;
                //    //txtCodigoAci.Visible = true;
                //    //lblCodigo.Visible = true;
                //    if (!cargarCliente(cod_cliente))
                //        throw new Exception(error);
                //}

            }

            //ScriptManager.RegisterStartupScript(this, this.GetType(), "LoadModal", "cargar_scripts();", true);
            //ScriptManager.RegisterStartupScript(this, this.GetType(), "LoadModal", "cargar_scripts();loadModal();", true);
        }
        catch (Exception ex)
        {
            Master.strMensajeError = ex.Message;
        }
    }
    public bool buscarCorrelativo(string strCodParametro)
    {
        try
        {
            controllerLink objLink = new controllerLink(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());

            //verifico y obtengo la información del parametro proporcionado
            if (!objLink.getLinkParametro(strCodParametro) || objLink.dsReturn.Tables["link_param"].Rows.Count == 0)
                throw new Exception("ERROR: Se presento un error al buscar los datos o no existe información. " + objLink.strReturnMessage);

            //coloco la información a mostrar 
            txtCodCorrelativo.Text = objLink.dsReturn.Tables["link_param"].Rows[0]["COD_PARAMETRO"].ToString();
            txtDia.Text = objLink.dsReturn.Tables["link_param"].Rows[0]["DIA_MES"].ToString();
            txtFechaProx.Text = objLink.dsReturn.Tables["link_param"].Rows[0]["PROXIMA_FECHA"].ToString();

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
    public bool buscarCuenta(string strNumCta)
    {
        try
        {
            controllerLink objLink = new controllerLink(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());

            //verifico que exista y obtengo la información de la cuenta proporcionada
            if (!objLink.getLinkCta(strNumCta) || objLink.dsReturn.Tables["link_cta"].Rows.Count == 0)
                throw new Exception("ERROR: Se presento un error al buscar los datos o no existe información. "+objLink.strReturnMessage);

            //coloco la información a mostrar 
            txtCodCorrelativo.Text = objLink.dsReturn.Tables["link_cta"].Rows[0]["COD_PARAMETRO"].ToString();
            txtDia.Text = objLink.dsReturn.Tables["link_cta"].Rows[0]["DIA_MES"].ToString();
            txtFechaProx.Text = objLink.dsReturn.Tables["link_cta"].Rows[0]["PROXIMA_FECHA"].ToString();

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
    protected void btnCancelar_Click(object sender, EventArgs e)
    {
        try
        {
            Session["PageRedirect"] = true;
            Response.Redirect("frmIndex.aspx", false);
            //Response.Redirect("frmClientes.aspx", false);
        }
        catch (Exception ex)
        {
            Master.strMensajeError = ex.Message;
        }
    }
    protected void btnGuardar_Click(object sender, EventArgs e)
    {
        try
        {
            
            if (((Button)sender).ID == "btnGuardar")
            {

                controllerLink objLink = new controllerLink(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());

                if (!String.IsNullOrEmpty(txtCodCorrelativo.Text) && !String.IsNullOrEmpty(txtDia.Text) && !String.IsNullOrEmpty(txtFechaProx.Text))
                {
                    if (!objLink.updateEstadoLink(txtCodCorrelativo.Text))
                        throw new Exception(objLink.strReturnMessage);
                    //REGISTRA BITACORA
                    RegistraBitacora();
                }
                else
                    throw new Exception("ERROR: Se debe tener información del link pre-seleccionada!!!");

                Master.strMensajeConfirmacion = "Parametro del Link Modificado!!!";
                Session["PageRedirect"] = true;
                Response.Redirect("frmIndex.aspx", false);

            }
        }
        catch (Exception ex)
        {
            Master.strMensajeError = ex.Message;
        }
    }
    protected void btnBuscarCta_Click(object sender, EventArgs e)
    {
        try
        {
            if (!String.IsNullOrEmpty(txtNumCta.Text))
            {
                //se procede a buscar la cuenta 
                if (!buscarCuenta(txtNumCta.Text))
                    throw new Exception(error);
            }
            else
                throw new Exception("ERROR: Se debe ingresar un número de cuenta a buscar!!!");

            ScriptManager.RegisterStartupScript(this.Page, typeof(Page), "hideCorrelativo", "ShowHideDiv();", true);
            //ScriptManager.RegisterStartupScript(this, this.GetType(), "LoadModal", "cargar_scripts();", true);

        }
        catch (Exception ex)
        {
            Master.strMensajeError = ex.Message;
        }
    }
    protected void btnBuscarParametro_Click(object sender, EventArgs e)
    {
        try
        {
            if (!String.IsNullOrEmpty(txtCorrelativo.Text))
            {
                //se procede a buscar el parametro respectivo 
                if (!buscarCorrelativo(txtCorrelativo.Text))
                    throw new Exception(error);

            }
            else
                throw new Exception("ERROR: Se debe ingresar un código de parametro a buscar!!!");

            ScriptManager.RegisterStartupScript(this.Page, typeof(Page), "hideProducto", "ShowHideDiv();", true);
        }
        catch (Exception ex)
        {
            Master.strMensajeError = ex.Message;
        }
    }
    void RegistraBitacora()
    {
        try
        {
            controllerSitio objControllerSitio = new controllerSitio(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());
            clsBitacora objBitacora = new clsBitacora();

            objBitacora.cod_link = "";
            objBitacora.cod_parametro = txtCodCorrelativo.Text;
            objBitacora.descripcion = "Se da de baja al parametro   (" + txtCodCorrelativo.Text + ")  que se encontraba configurado para generarse automaticamente el " + txtDia.Text + " de cada mes.";
            objBitacora.tip_procesamiento = "B"; //dar de baja 

            if (!objControllerSitio.registraBitacora(objBitacora))
                throw new Exception(objControllerSitio.strReturnMessage);
        }
        catch (Exception ex)
        {
            Master.strMensajeError = ex.Message;
        }
    }
}