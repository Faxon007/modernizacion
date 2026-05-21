using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class frmLinksVerifica : System.Web.UI.Page
{
    public enum MessageType { Success, Error, Info, Warning };
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
            //txtSKU.Text = "0";
            #region validacion de permisos
            Response.Cookies.Add(new HttpCookie("returnUrl", Request.Url.PathAndQuery));
            if (Session["usuario"] == null)
                Response.Redirect("Default.aspx");

            controllerMenu objControllerMenu = new controllerMenu(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());
            if (!objControllerMenu.verificarMenuItem(Session["usuario"].ToString(), "9506"))
                Response.Redirect("frmIndex.aspx");
            #endregion

            #region inicializacion de pagina
            Master.strPagina = "Listado de Links";
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
    protected void ShowMessage(string Message, MessageType type)
    {
        ScriptManager.RegisterStartupScript(this, this.GetType(), System.Guid.NewGuid().ToString(), "ShowMessage('" + Message + "','" + type + "');", true);
    }
    protected void btnConsultaPago_Click(object sender, EventArgs e)
    {
        try
        {
            //20240227.ini: Se valida elemento xml para determinar en que ambiente se esta ejecutando.
            var environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            if (!string.IsNullOrEmpty(environment) && environment.Equals("Development"))
            {
                //Set values objPago
                clsPago pago = new clsPago()
                {
                    aut_visa = "8956540",
                    num_cta = hdfProducto.Value,
                    cod_sku = hdfSKU.Value,
                    cod_link = hdfCodLink.Value,

                };
                ProcesarPagoCORE(pago);
            }
            else
            {
                clsPago objPago = new clsPago();
                // se obtiene la información seleccionada
                objPago.num_cta = hdfProducto.Value;
                objPago.cod_sku = hdfSKU.Value;
                objPago.cod_link = hdfCodLink.Value;
                // se realiza la consulta al API de Visanet
                VisaEnLink Link = new VisaEnLink(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());

                //Se revisa si existe token del dia
                if (!Link.ExisteToken())
                    throw new Exception("Error: inconveniente al verificar token. " + Link.error);

                if (!Link.ConsultaLink(objPago))
                    throw new Exception("Error: se presento error al consultar autorizacion. " + Link.error);

                // se revisa que la autorizacion del pago se encuentre 
                if (!String.IsNullOrEmpty(Link.ObtengoAutorizacion()))
                {
                    //obtengo el número de autorizacion de visa
                    objPago.aut_visa = Link.ObtengoAutorizacion();

                    //se debe proceder con el pago del link 
                    ProcesarPagoCORE(objPago);

                    //ScriptManager.RegisterStartupScript(this.Page, typeof(Page), "hideContenido", "HideDiv();", true);
                    //ShowMessage(error, MessageType.Info);

                }
                else
                {
                    ScriptManager.RegisterStartupScript(this.Page, typeof(Page), "hideContenido", "HideDiv();", true);
                    //muestro el mensaje en el modal-fade
                    ShowMessage("El link aún no posee un número de autorizacion en Visa", MessageType.Warning);
                }
            }
        }
        catch (Exception ex)
        {
            //error = ex.Message;
            ScriptManager.RegisterStartupScript(this.Page, typeof(Page), "Pop", "$('#divPago').modal('hide');", true);
            Master.strMensajeError = ex.Message;
        }
    }
    private void ProcesarPagoCORE(clsPago objPagar)
    {
        //try
        //{
        //debo revisar que tipo de producto es, el monto a aplicar y si realiza pago en dolares
        controllerLink objProceso = new controllerLink(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());

        if (!objProceso.getParametro(objPagar.cod_link))
        {
            ScriptManager.RegisterStartupScript(this.Page, typeof(Page), "hideContenido", "HideDiv();", true);
            ShowMessage("Error al consultar datos del link.F / M: (ProcesarPagoCORE)", MessageType.Error);
            //throw new Exception("Error al consultar datos del link. F/M: (ProcesarPagoCORE): " + objProceso.strReturnMessage);
        }
        else
        {    //obtengo el monto a aplicar del link 
            objPagar.mon_pago = Convert.ToString(objProceso.dsReturn.Tables["datos_link"].Rows[0]["MON_COBRO"]);

            //reviso que tipo de producto es
            switch (Convert.ToString(objProceso.dsReturn.Tables["datos_link"].Rows[0]["TIP_CUENTA"]))
            {
                case "PR": //Se debe pagar para PR
                    //valido si la transaccion debe realizarse en moneda local
                    if (Convert.ToString(objProceso.dsReturn.Tables["datos_link"].Rows[0]["TIP_PAGO"]) == "0")
                    {
                        if (!objProceso.aplicaPagoPR(objPagar, "320"))
                        {
                            ScriptManager.RegisterStartupScript(this.Page, typeof(Page), "hideContenido", "HideDiv();", true);
                            string[] mensaje = objProceso.strReturnMessage.Split(new[] { "\n" }, StringSplitOptions.None);
                            //muestra mensaje en el modal-fade
                            ShowMessage(mensaje[0], MessageType.Error);
                            //throw new Exception("Error: Se presento inconveniente al efectuar pago de prestamo en Quetzales (ProcesarPagoCORE) " + objProceso.strReturnMessage);
                        }
                        else
                        {
                            ScriptManager.RegisterStartupScript(this.Page, typeof(Page), "hideContenido", "HideDiv();", true);
                            //muestra mensaje en el modal-fade
                            ShowMessage("Se efectuó de forma exitosa el pago de prestamo", MessageType.Success);
                        }
                    }
                    else
                    {
                        if (!objProceso.aplicaPagoPR(objPagar, "840"))
                        {
                            ScriptManager.RegisterStartupScript(this.Page, typeof(Page), "hideContenido", "HideDiv();", true);
                            string[] mensaje = objProceso.strReturnMessage.Split(new[] { "\n" }, StringSplitOptions.None);
                            //muestra mensaje en el modal-fade
                            ShowMessage(mensaje[0], MessageType.Error);
                            //throw new Exception("Error: Se presento inconveniente al efectuar pago de prestamo en Dolares (ProcesarPagoCORE) " + objProceso.strReturnMessage);
                        }
                        else
                        {
                            ScriptManager.RegisterStartupScript(this.Page, typeof(Page), "hideContenido", "HideDiv();", true);
                            //muestra mensaje en el modal-fade
                            ShowMessage("Se efectuó de forma exitosa el pago de prestamo", MessageType.Success);
                        }
                    }
                    break;
                case "TC": //se debe pagar para TC
                    if (Convert.ToString(objProceso.dsReturn.Tables["datos_link"].Rows[0]["TIP_PAGO"]) == "0")
                    {
                        if (!objProceso.aplicaPagoTC(objPagar, "320"))
                        {
                            ScriptManager.RegisterStartupScript(this.Page, typeof(Page), "hideContenido", "HideDiv();", true);
                            string[] mensaje = objProceso.strReturnMessage.Split(new[] { "\n" }, StringSplitOptions.None);
                            //muestra mensaje en el modal-fade
                            ShowMessage(mensaje[0], MessageType.Error);
                            //throw new Exception("Error: Se presento inconveniente al efectuar pago de TC en Quetzales (ProcesarPagoCORE) " + objProceso.strReturnMessage);
                        }
                        else
                        {
                            ScriptManager.RegisterStartupScript(this.Page, typeof(Page), "hideContenido", "HideDiv();", true);
                            //muestra mensaje en el modal-fade
                            ShowMessage("Se efectuó de forma exitosa el pago de TC", MessageType.Success);
                        }
                    }
                    else
                    {
                        if (!objProceso.aplicaPagoTC(objPagar, "840"))
                        {
                            ScriptManager.RegisterStartupScript(this.Page, typeof(Page), "hideContenido", "HideDiv();", true);
                            string[] mensaje = objProceso.strReturnMessage.Split(new[] { "\n" }, StringSplitOptions.None);
                            //muestra mensaje en el modal-fade
                            ShowMessage(mensaje[0], MessageType.Error);
                            //throw new Exception("Error: Se presento inconveniente al efectuar pago de TC en Dolares (ProcesarPagoCORE) " + objProceso.strReturnMessage);
                        }
                        else
                        {
                            ScriptManager.RegisterStartupScript(this.Page, typeof(Page), "hideContenido", "HideDiv();", true);
                            //muestra mensaje en el modal-fade
                            ShowMessage("Se efectuó de forma exitosa el pago de TC", MessageType.Success);
                        }
                    }
                    break;
            }

            //REGISTRA BITACORA
            RegistraBitacora(objPagar);
        }

        //    return true;
        //}
        //catch (Exception ex)
        //{
        //    error = ex.Message;
        //    return false;
        //    //Master.strMensajeError = ex.Message;
        //}
    }
    void RegistraBitacora(clsPago datosLink)
    {
        try
        {
            controllerSitio objControllerSitio = new controllerSitio(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());
            clsBitacora objBitacora = new clsBitacora();

            objBitacora.cod_link = datosLink.cod_link;
            objBitacora.cod_parametro = "";
            objBitacora.descripcion = "MANUAL: Se procedio con el pago  (" + datosLink.cod_sku + ") según link No." + datosLink.cod_link + " asociado al número de cuenta de No." + datosLink.num_cta + ", Valor = " + datosLink.mon_pago;
            objBitacora.tip_procesamiento = "P"; //creacion

            if (!objControllerSitio.registraBitacora(objBitacora))
                throw new Exception(objControllerSitio.strReturnMessage);
        }
        catch (Exception ex)
        {
            Master.strMensajeError = ex.Message;
        }
    }
}