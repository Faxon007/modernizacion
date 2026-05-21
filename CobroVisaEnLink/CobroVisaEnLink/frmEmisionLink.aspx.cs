using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using RestSharp;
using System.Net;

public partial class frmEmisionLink : System.Web.UI.Page
{
    private string error;
    private string strTipoCuenta;
    //------------------------------------------------
    //string strDominio = "pagos.bancopromerica.com.gt";
    //string strDominioNew = "sl.bpgt.com.gt";
    string strDominioNew = "bpgt.com.gt";
    //string strApiKey = "Bearer 8rkNKUWqtHU9bRomng9b3xJd2m87kBXWdki6QlN7HIXefVbRiowOPTYYOHoZ";
    string strApiKey = "262cce792cc7478fab58da1942e5e25b";
    string strWorkspace = "1cf40cc1d152469abd784d23bb664e64";
    string strShortURL;
    string strDominio = System.Configuration.ConfigurationManager.AppSettings["strDominio"];
    string strServer = System.Configuration.ConfigurationManager.AppSettings["strServer"];
    string apikey = System.Configuration.ConfigurationManager.AppSettings["apikey"];

    public string codCliente
    {
        get
        {
            return ViewState["codCliente"] == null ? "" : (string)ViewState["codCliente"];
        }
        set
        {
            ViewState["codCliente"] = value;
        }
    }
    //------------------------------------------------
    clsLink objLink = new clsLink();
    //public clsLink objLink
    //{
    //    get
    //    {
    //        return ViewState["objLink"] == null ? null : (clsLink)ViewState["objLink"];
    //    }
    //    set
    //    {
    //        ViewState["objLink"] = value;
    //    }
    //}
    //bool AccionBoton = true;
    public bool AccionBoton
    {
        get
        {
            return ViewState["AccionBoton"] == null ? false : (bool)ViewState["AccionBoton"];
        }
        set
        {
            ViewState["AccionBoton"] = value;
        }
    }
    //public enum MessageType { Success, Error, Info, Warning };
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
            Response.Cookies.Add(new System.Web.HttpCookie("returnUrl", Request.Url.PathAndQuery));
            if (Session["usuario"] == null)
                Response.Redirect("Default.aspx");

            controllerMenu objControllerMenu = new controllerMenu(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());
            if (!objControllerMenu.verificarMenuItem(Session["usuario"].ToString(), "9503"))
                Response.Redirect("frmIndex.aspx");
            #endregion

            #region inicializacion de pagina
            Master.strPagina = "Identificar Producto";
            //string cod_cliente = Request.QueryString["cli"];
            //if (cod_cliente != null)
                //Master.strPagina = "Editar Enrolamiento Cliente";
            #endregion

            if (!this.IsPostBack)
            {
                //
                for (int i = 1; i <= DateTime.DaysInMonth(DateTime.Now.Year, 1); i++)
                    ddlDia.Items.Add(new ListItem(i.ToString(), i.ToString()));

                ddlDia.SelectedValue = DateTime.Now.Day.ToString();

            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "LoadModal", "cargar_scripts();", true);
            //ScriptManager.RegisterStartupScript(this, this.GetType(), "disabledll", "document.getElementById(\"<%= ddlTipoLink.ClientID %>\").disabled = false;", true);
            //ScriptManager.RegisterStartupScript(this, this.GetType(), "LoadModal", "cargar_scripts();loadModal();", true);
        }
        catch (Exception ex)
        {
            Master.strMensajeError = ex.Message;
        }
    }
    protected void btnCancelar_Click(object sender, EventArgs e)
    {
        try
        {
            Session["PageRedirect"] = true;
            Response.Redirect("frmIndex.aspx", false);
        }
        catch (Exception ex)
        {
            Master.strMensajeError = ex.Message;
        }
    }
    //GUARDAR PARAMETROS DE LINK
    protected void btnGuardar_Click(object sender, EventArgs e)
    {
        if (AccionBoton)
        {
            try
            {
                //20240312.ini
                //validar que el dato default de correo exista
                if (ddlDatosCliente.SelectedValue == "1" && rdbsMail.Checked)
                {
                    if (!ValidatorsHelpers.ValidateEmail(lblCorreo.Text))
                        throw new Exception("Se debe seleccionar otro medio de envío o digitar un correo electrónico.");
                }
                else if (ddlDatosCliente.SelectedValue == "1" && rdbSMS.Checked)
                { //validar que el dato default del telefono exista 
                    if (!ValidatorsHelpers.ValidateTelefono(lblTelefono.Text))
                        throw new Exception("Se debe seleccionar otro medio de envío o digitar un número de teléfono.");
                }
                //20240312.ini
                controllerLink objControllerLink = new controllerLink(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());

                //REGRESO MOD
                #region parametros a tomar en cuentra en el link a guardar
                #region validaciones previas
                //numero de la cuenta
                if (txtCuenta.Text == "")
                    throw new Exception("Error: Debe ingresar un número cuenta!!!");
                objLink.num_cuenta = txtCuenta.Text;
                //tipo de cuenta
                switch (this.gvCuentas.SelectedRow.Cells[1].Text)
                {
                    case "Tarjeta":
                        objLink.tip_cuenta = "TC";
                        break;
                    case "Prestamo":
                        objLink.tip_cuenta = "PR";
                        break;
                }
                //monto de cobro
                if (txtMonto.Text == "0" || String.IsNullOrEmpty(txtMonto.Text))
                    throw new Exception("Error: Debe ingresar un monto!!!");
                //20240222.ini: Se parsea el texto a numero
                objLink.mon_cobro = decimal.Round(Convert.ToDecimal(txtMonto.Text.Trim()), 2).ToString();
                //si realizara pago en dolares
                if (chkPago.Checked == true) //rdbPago
                    objLink.tip_pago = "1";
                else
                    objLink.tip_pago = "0";
                //tipo de link
                objLink.tip_link = ddlTipoLink.SelectedValue;
                if (ddlTipoLink.SelectedValue == "1")
                    objLink.dia_mes = ddlDia.SelectedValue;
                else if (ddlTipoLink.SelectedValue == "2")
                {
                    //obtengo el link generado 
                    objLink.url_link = lnkURL.NavigateUrl;
                }

                //utilizara datos default
                objLink.es_default = ddlDatosCliente.SelectedValue;
                //enviara link por SMS o Correo
                if (rdbSMS.Checked == true)
                {
                    objLink.tip_envio = "1"; //SMS

                    if (ddlDatosCliente.SelectedValue == "1") //default
                    {
                        if (String.IsNullOrEmpty(lblTelefono.Text) || lblTelefono.Text.Length == 0)
                            throw new Exception("Error: Recuerde que debe ingresar un número de teléfono para el envío de link!!!");
                        // se asigna el default
                        objLink.num_telefono = lblTelefono.Text;
                    }
                    else if (ddlDatosCliente.SelectedValue == "2") //editado
                    {
                        if (String.IsNullOrEmpty(txtTelefono.Text) || txtTelefono.Text.Length == 0)
                            throw new Exception("Error: Recuerde que debe ingresar un número de teléfono para el envío de link!!!");

                        //20240312.ini
                        if (!ValidatorsHelpers.ValidateTelefono(txtTelefono.Text))
                            throw new Exception("ERROR: Se debe ingresar un valor de teléfono válido!!!");
                        //20240312.ini
                        // se asigna el editado
                        objLink.num_telefono = txtTelefono.Text;
                    }
                }
                else if (rdbsMail.Checked == true)
                {
                    objLink.tip_envio = "2"; //MAIL

                    if (ddlDatosCliente.SelectedValue == "1")
                    {
                        if (String.IsNullOrEmpty(lblCorreo.Text) || lblCorreo.Text.Length == 0)
                            throw new Exception("Error: Recuerde que debe ingresar un correo electrónico para el envío de link!!!");
                        // se asigna el default
                        objLink.nom_correo = lblCorreo.Text;
                    }
                    else if (ddlDatosCliente.SelectedValue == "2")
                    {
                        if (String.IsNullOrEmpty(txtMail.Text) || txtMail.Text.Length == 0)
                            throw new Exception("Error: Recuerde que debe ingresar un correo electrónico para el envío de link!!!");
                        // se asigna el editado
                        objLink.nom_correo = txtMail.Text;
                    }
                }

                if (!rdbSMS.Checked && !rdbsMail.Checked)
                    throw new Exception("Error: Debe seleccionar un método de envió de link!!!");
                #endregion
                objLink.cod_sku = lnkURL.Text;
                objLink.url_link = lnkURL.NavigateUrl;//aqui vacio si es automatico

                objLink.ind_estado = "A";
                //se deberá validar que exista informacion de correo o telefono ingresado segun la opcion seleccionada
                #endregion

                //insertar o actualizar
                if (((Button)sender).ID == "btnGuardar")
                {
                    #region validacion prestamo
                    // verifico que el dato seleccionado fue prestamo
                    if (this.gvCuentas.SelectedRow.Cells[1].Text == "Prestamo")
                    {
                        controllerCliente objProducto = new controllerCliente(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());
                        //obtengo la moneda del prestamo
                        if (!objProducto.getTipoPrestamo(txtCuenta.Text))
                            throw new Exception(objProducto.strReturnMessage);

                        if (Convert.ToString(objProducto.dsReturn.Tables["prestamo"].Rows[0]["MONEDA"]) == "840" && !chkPago.Checked) //rdbPago
                        {
                            //chkPago.Checked = false; //rdbPago
                            throw new Exception("Error: Se debe seleccionar la opcion de -Pagar en Dolares- para prestamos en dolares");
                        }

                    }
                    #endregion
                    //verificar si es link automatico y el dia seleccionado para generar el link en visaenlink
                    if (ddlTipoLink.SelectedValue == "1" && ddlDia.SelectedValue == DateTime.Now.Day.ToString())
                    {
                        //validaremos que existan datos en cuenta y monto
                        if (txtCuenta.Text == "" || String.IsNullOrEmpty(txtCuenta.Text))
                            throw new Exception("Error: Debe ingresar un número cuenta!!!");

                        if (txtMonto.Text == "0" || String.IsNullOrEmpty(txtMonto.Text))
                            throw new Exception("Error: Debe ingresar un monto!!!");

                        //metodo
                        ObtencionLink();
                        // volver a asignar los valores 
                        objLink.cod_sku = lnkURL.Text;
                        objLink.url_link = lnkURL.NavigateUrl;
                    }

                    //---------------------------------------------
                    //se debe obtener el shortcut de rebrandly
                    if (lnkURL.Text != "Pendiente")
                    {
                        //GeneraURLCorto(lnkURL.NavigateUrl);
                        //20240529.ini Ejecutar metodo de url corto con rest cliente
                        GeneraURLCortoRestClient(lnkURL.NavigateUrl);
                        #region proceso final de envio y guardado de link
                        //valida
                        if (String.IsNullOrEmpty(strShortURL))
                            throw new Exception("Error: No se logró obtener el link corto!!!");
                        //link corto 
                        objLink.url_corto = strShortURL; //"https://" + 
                        //guardo el link
                        if (!objControllerLink.insertLink(objLink))
                            throw new Exception(objControllerLink.strReturnMessage);
                        //--------------------------------------------------------------------------------------------
                        //obtengo el parametro del texto SMS
                        controllerSitio objParametros = new controllerSitio(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());

                        if (!objParametros.getParametros())
                            throw new Exception(objParametros.strReturnMessage);

                        string strArteSMS = objParametros.dsReturn.Tables["param_sistema"].Rows[0]["MSG_SMS"].ToString();
                        string strTipoProd = "";
                        //obtengo la palabra a colocar en el arte del correo 
                        switch (this.gvCuentas.SelectedRow.Cells[1].Text)
                        {
                            case "Tarjeta":
                                strTipoProd = "tarjeta de crédito";
                                break;
                            case "Prestamo":
                                strTipoProd = "préstamo";
                                break;
                        }

                        if (lnkURL.Text != "Pendiente")
                        {
                            //Enviar link por SMS o Correo
                            if (!notificarCliente(strTipoProd, strArteSMS))
                                throw new Exception("Error: al notificar al cliente. " + error);
                        }
                        #endregion
                    }
                    else
                    {
                        //esto se da cuando es link programado
                        //guardo el link
                        if (!objControllerLink.insertLink(objLink))
                            throw new Exception(objControllerLink.strReturnMessage);
                    }
                    //    throw new Exception("Error: el link no puede estar pendiente!!!");
                    //---------------------------------------------

                    //REGISTRA BITACORA
                    RegistraBitacora(objLink);

                    Master.strMensajeConfirmacion = "Link Parametrizado!!!";
                    Session["PageRedirect"] = true;
                    Response.Redirect("frmIndex.aspx", false);
                }
            }
            catch (Exception ex)
            {
                Master.strMensajeError = ex.Message;
                
            }
        }
        else
        {
            Master.strMensajeError = "ERROR: No puede guardar un monto incorrecto!!!";
            Session["PageRedirect"] = true;
            Response.Redirect("frmIndex.aspx", false);
        }
    }
    protected void gvCuentas_SelectedIndexChanged(object sender, EventArgs e)
    {
        this.txtCuenta.Text = this.gvCuentas.SelectedRow.Cells[0].Text;

        strTipoCuenta = this.gvCuentas.SelectedRow.Cells[1].Text;

        if (gvCuentas.Rows.Count != 0)
            gvCuentas.HeaderRow.TableSection = TableRowSection.TableHeader;

        txtCuenta.Enabled = false;
        btnBuscarCta.Enabled = false;

        #region se obtiene el correo y telefono default (analizar proceso)
        controllerCliente objCliente = new controllerCliente(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());
        //mostrar datos default del cliente
        //obtengo el correo de cliente
        if (!objCliente.getCorreoCliente(codCliente))
            throw new Exception(objCliente.strReturnMessage);

        //20240308.ini BCd
        if (objCliente.dsReturn.Tables["correo_cliente"].Rows.Count > 0)
            lblCorreo.Text = Convert.ToString(objCliente.dsReturn.Tables["correo_cliente"].Rows[0]["CORREO"]).Trim();
        else
            lblCorreo.Text = "Dato default no encontrado, digitar correo electrónico";

        //obtengo el telefono de cliente
        //20240308.ini BC
        if (!objCliente.getTelefonoCliente(codCliente))
            throw new Exception(objCliente.strReturnMessage);

        if (objCliente.dsReturn.Tables["tel_cliente"].Rows.Count > 0)
            lblTelefono.Text = Convert.ToString(objCliente.dsReturn.Tables["tel_cliente"].Rows[0]["TELEFONO"]).Trim();
        else
            lblTelefono.Text = "Dato default no encontrado, digitar teléfono";

        #endregion
    }
    protected void gvCuentas_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        try
        {
            if (!buscarCuentas())
                throw new Exception(error);
            gvCuentas.PageIndex = e.NewPageIndex;
            gvCuentas.DataBind();
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }
    }
    //busca cuentas del cliente
    protected void btnBuscarCta_Click(object sender, EventArgs e)
    {
        try
        {
            //se obtiene los productos que tiene asociados 
            if (!this.buscarCuentas())
                throw new Exception(error);
        }
        catch (Exception ex)
        {
            //error = ex.Message;
            Master.strMensajeError = ex.Message;
        }
    }
    public bool buscarCuentas()
    {
        try
        {
            if (string.IsNullOrEmpty(txtCuenta.Text))
                throw new Exception("ERROR: Debe ingresar el número de cuenta");

            if (txtCuenta.Text.Any(c => c < '0' || c > '9'))
                throw new Exception("Por favor ingresar valores numéricos.");

            //if (txtBuscarNombre.Text != "" && txtBuscarNombre.Text.Length > 2)
            //inicializamos
            gvCuentas.DataSource = null;
            gvCuentas.DataBind();
            //se obtiene los productos que tiene asociados 

            controllerCliente objCliente = new controllerCliente(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());
            //obtengo el codigo de cliente
            if (!objCliente.getCliente_cta(txtCuenta.Text))
                throw new Exception(objCliente.strReturnMessage);

            //TOdO: Validacion de cliente si pertenece a lista negra
            codCliente = objCliente.dsReturn.Tables["datos_cliente"].Rows[0]["COD_CLIENTE"].ToString();

            if (string.IsNullOrEmpty(codCliente)) 
                throw new Exception("El cliente no existe");
            //cliente en lista negra
            if (!objCliente.GetClienteListaNegra("1", codCliente))
                throw new Exception(objCliente.strReturnMessage);

            lblCliente.Text = Convert.ToString(objCliente.dsReturn.Tables["datos_cliente"].Rows[0]["NOM_CLIENTE"]).Trim()
                            + "(" + Convert.ToString(objCliente.dsReturn.Tables["datos_cliente"].Rows[0]["COD_CLIENTE"]).Trim() + ")";

            //obtengo las cuentas de TC y PR respectivas
            if (!objCliente.getCuentas(Convert.ToString(objCliente.dsReturn.Tables["datos_cliente"].Rows[0]["COD_CLIENTE"])))
                throw new Exception(objCliente.strReturnMessage);

            gvCuentas.DataSource = objCliente.dsReturn.Tables["cuentas"];
            gvCuentas.DataBind();

            ScriptManager.RegisterStartupScript(this, this.GetType(), "ServerLoadModal", "<script>$('#divBusquedaCuenta').modal('show');</script>", false);

            //if (gvCuentas.Rows.Count != 0)
            //    gvCuentas.HeaderRow.TableSection = TableRowSection.TableHeader;

            //-----------------------------------
            //se cambio codigo de lugar
            //-----------------------------------

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
    void ObtencionLink() //metodo cuando es link manual 
    {
        //parametros
        controllerSitio objcontrollerSitio = new controllerSitio(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());

        if (!objcontrollerSitio.getParametros())
            throw new Exception(objcontrollerSitio.strReturnMessage);

        if (objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows.Count > 0 && objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows[0]["API_IMAGEN"] != null)
        {
            #region imagen obtención
            byte[] byteArray = (Byte[])objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows[0]["API_IMAGEN"];
            //Convert the Byte Array to Base64 Encoded string.
            string base64String = Convert.ToBase64String(byteArray, 0, byteArray.Length);
            #endregion
            VisaEnLink Link = new VisaEnLink(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());
            //Se revisa si existe token del dia
            if (!Link.ExisteToken())
                throw new Exception("Error: (ObtencionLink) " + Link.error);

            //CONSTRUYO SECUENCIA 
            if (!objcontrollerSitio.obtengoCodigoInterno())
                throw new Exception("Error: (ObtencionLink)" + objcontrollerSitio.strReturnMessage);

            
            string strCodigoInterno = objcontrollerSitio.dsReturn.Tables["codigo_interno"].Rows[0]["CORRELATIVO"].ToString();

            //Se procede a Crear Link
            if (!Link.CrearLink(txtCuenta.Text, txtMonto.Text, (base64String), strCodigoInterno)) //"data:image/jpg;base64,"
                throw new Exception("Error: " + Link.error);

            //se asignan los valores respectivos obtenidos del API
            lnkURL.Text = Link.ObtengoSKU();
            lnkURL.NavigateUrl = Link.ObtengoURL();

            //deshabilito boton
            btnObtenerLink.Enabled = false;

        }
        else
            throw new Exception("Error: se debe tener una imagen parametrizada");
    }
    protected void btnObtenerLink_Click(object sender, EventArgs e)
    {
        if (AccionBoton)
        {
            try
            {
                txtCuenta.ReadOnly = true;
                btnBuscarCta.Enabled = false;
                txtMonto.ReadOnly = true;
                //ddlTipoLink.Enabled = false;
                //document.getElementById("clientdropdownid").disabled = true;
                ddlTipoLink.Attributes.Add("disabled", "disabled");
                //ScriptManager.RegisterStartupScript(this, this.GetType(), "disabledll", "document.getElementById(\"<%= ddlTipoLink.ClientID %>\").disabled = false;", true);

                chkPago.Enabled = false;

                //debo validar si es prestamo en dolares 
                if (this.gvCuentas.SelectedRow.Cells[1].Text == "Prestamo")
                {
                    controllerCliente objProducto = new controllerCliente(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());
                    //obtengo la moneda del prestamo
                    if (!objProducto.getTipoPrestamo(txtCuenta.Text))
                        throw new Exception(objProducto.strReturnMessage);

                    if (Convert.ToString(objProducto.dsReturn.Tables["prestamo"].Rows[0]["MONEDA"]) == "840" && !chkPago.Checked) //rdbPago
                    {
                        //chkPago.Checked = false; //rdbPago
                        throw new Exception("Error: Debe seleccionar la opcion de -Pagar en Dolares- para prestamos en dolares");
                    }
                    else
                    {
                        //$('#divEnvio').show();
                        ObtencionLink();
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "showEnvio", "$('#divEnvio').show();", true);
                    }
                }
                else
                {
                    //$('#divEnvio').show();
                    ObtencionLink();
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "showEnvio", "$('#divEnvio').show();", true);
                }

            }
            catch (Exception ex)
            {
                //error = ex.Message;
                txtCuenta.ReadOnly = false;
                btnBuscarCta.Enabled = true;
                txtMonto.ReadOnly = false;
                ddlTipoLink.Enabled = true;
                chkPago.Enabled = true;
                Master.strMensajeError = ex.Message;
            }
        }
    }
    protected void chkPago_CheckedChanged(object sender, EventArgs e)
    {
        try
        {
            // verifico que el dato seleccionado fue prestamo
            if (this.gvCuentas.SelectedRow.Cells[1].Text == "Prestamo")
            {
                controllerCliente objProducto = new controllerCliente(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());
                //obtengo la moneda del prestamo
                if (!objProducto.getTipoPrestamo(txtCuenta.Text))
                    throw new Exception(objProducto.strReturnMessage);

                if (Convert.ToString(objProducto.dsReturn.Tables["prestamo"].Rows[0]["MONEDA"]) == "320" && chkPago.Checked) //rdbPago
                {
                    chkPago.Checked = false; //rdbPago
                    throw new Exception("Error: Solo se permite la opcion de -Pagar en Dolares- para prestamos en dolares");
                }

            }

            if (ddlTipoLink.SelectedValue == "1")
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "showManual", "$('#divManual').hide();", true);
            }
        }
        catch (Exception ex)
        {
            Master.strMensajeError = ex.Message;
        }
    }
    void RegistraBitacora(clsLink datosLink)
    {
        try
        {
            controllerSitio objControllerSitio = new controllerSitio(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());
            clsBitacora objBitacora = new clsBitacora();

            objBitacora.cod_link = "";
            objBitacora.cod_parametro = "";
            objBitacora.descripcion = "Se creo link (" + datosLink.cod_sku + ") asociado al número de cuenta (" + datosLink.tip_cuenta + ") No." + txtCuenta.Text;
            objBitacora.tip_procesamiento = "C"; //creacion

            if (!objControllerSitio.registraBitacora(objBitacora))
                throw new Exception(objControllerSitio.strReturnMessage);

            clsBitCore objBitCore = new clsBitCore();

            controllerCliente objCliente = new controllerCliente(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());
            //obtengo el codigo de cliente
            if (!objCliente.getCliente_cta(txtCuenta.Text))
                throw new Exception(objCliente.strReturnMessage);

            objBitCore.cod_persona = objCliente.dsReturn.Tables["datos_cliente"].Rows[0]["COD_CLIENTE"].ToString();

            switch (datosLink.tip_cuenta)
            {
                case "PR":
                    objBitCore.tip_cuenta = "PR";
                    objBitCore.num_cta_prestamo = txtCuenta.Text;
                    break;
                case "TC":
                    objBitCore.tip_cuenta = "TC";
                    objBitCore.num_cta_credito = txtCuenta.Text;
                    break;
            }

            string strNotificacion = "";

            if (rdbSMS.Checked)
            { // SMS
                switch (ddlDatosCliente.SelectedValue)
                {
                    //default
                    case "1":
                        strNotificacion = "Teléfono: " + lblTelefono.Text;
                        break;
                    //editado
                    case "2":
                        strNotificacion = "Teléfono: " + txtTelefono.Text;
                        break;
                }
            }
            else if (rdbsMail.Checked)
            { // correo
                switch (ddlDatosCliente.SelectedValue)
                {
                    //default
                    case "1":
                        strNotificacion = "Correo: " + lblCorreo.Text;
                        break;
                    //editado
                    case "2":
                        strNotificacion = "Correo: " + txtMail.Text;
                        break;
                }
            }

            objBitCore.descripcion = "Se realiza envío de Link para pago al " + strNotificacion + " por Q." + txtMonto.Text; //


            controllerSitio objControllerSitio2 = new controllerSitio(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());


            if (!objControllerSitio2.registraBitacoraCore(objBitCore))
                throw new Exception(objControllerSitio2.strReturnMessage);
        }
        catch (Exception ex)
        {
            Master.strMensajeError = ex.Message;
        }
    }
    public void GeneraURLCorto(string strURL)
    {
        var linkVisa = new
        {
            destination = strURL,
            domain = new
            {
                fullName = strDominioNew //strDominio
            }
            //, slashtag = "A_NEW_SLASHTAG"
            //, title = "Rebrandly YouTube channel"     
        };

        using (var httpClient = new HttpClient { BaseAddress = new Uri("https://enterprise-api.rebrandly.com") }) // enterprise-api api viejo  https://api.rebrandly.com
        {
            httpClient.DefaultRequestHeaders.Add("apikey", strApiKey);
            httpClient.DefaultRequestHeaders.Add("workspace", "1cf40cc1d152469abd784d23bb664e64"); // 883b07a8e89c4cbaa1b6eebf983c7b61 wokspace extended ( 1cf40cc1d152469abd784d23bb664e64 ) 

            var body = new StringContent(
                JsonConvert.SerializeObject(linkVisa), UnicodeEncoding.UTF8, "application/json");

            var response = httpClient.PostAsync("/v1/links", body).Result;

            if (response.IsSuccessStatusCode)
            {
                var link = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);

                strShortURL = link[0].shortUrl; //link[0].shortUrl;        link.shortUrl;
            }
            else
            {
                var resultEx = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);

                throw new Exception("Error: No se pudo crear el link corto (GeneraURLCorto)");
            }
        }

    }
    //ToDO: 20240529.ini Metodo de generar url corto usando RestClient
    public void GeneraURLCortoRestClientBk(string strURL)
    {
        #region codigo del proceso de rebrandly
        
        var linkVisa = new
        {
            destination = strURL,
            domain = new
            {
                fullName = strDominioNew //strDominio
            }
            //, slashtag = "A_NEW_SLASHTAG"
            //, title = "Rebrandly YouTube channel"     
        };

        /*
        var linkVisa = new
        {
            url = strURL,
            domain = strDominioNew
            //, slashtag = "A_NEW_SLASHTAG"
            //, title = "Rebrandly YouTube channel"     
        };*/

        //se coloca para hacer prueba manual
        var client = new RestClient("https://enterprise-api.rebrandly.com");
        //var client = new RestClient("https://api.tinyurl.com");
        //var client = new RestClient("https://pagospromerica.promerica.com.gt/APIPagospromerica");
        var proxy = new WebProxy("webproxy.promerica.com.gt", 9095);
        proxy.Credentials = new NetworkCredential("servicio_bo", "B4nc0Pr0m3r!c4");
        client.Proxy = proxy;
        client.Timeout = -1;
        //var request = new RestRequest("/create", Method.POST);
        var request = new RestRequest("/v1/links", Method.POST);
        request.RequestFormat = DataFormat.Json;
        //request.AddHeader("Authorization", strApiKey);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("apikey", strApiKey);
        request.AddHeader("workspace", strWorkspace);

        request.AddJsonBody(linkVisa);
        //certificado de seguridad
        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
        IRestResponse response = client.Execute(request);
        #endregion
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var link = JsonConvert.DeserializeObject<dynamic>(response.Content);
            //strShortURL = link.data.tiny_url.ToString();
            strShortURL = link[0].shortUrl; //link.shortUrl;
            //strShortURL = link.RootElement.GetProperty("data").GetProperty("tiny_url").GetString();
        }
        else
        {
            //se toma el correo default
            lblCorreoAlterno.Text = lblCorreo.Text;
            // se levanta el modal
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ServerLoadModal", "<script>$('#divNotificaAlterno').modal({backdrop: 'static', keyboard: false},\"show\");</script>", false);
        }

    }
    //ToDO: 20240529.ini Metodo de generar url corto usando RestClient

    //20250801 Lander Montenegro.
    public void GeneraURLCortoRestClient(string strURL)
    {

        try
        {
            var linkVisa = new List<LinkRequestItem>
            {
                new LinkRequestItem{
                    destination = strURL,
                    domain = new Domain
                        {
                        fullName = strDominio
                    }
                }
            };

            var client = new RestClient(strServer);
            var proxy = new WebProxy("webproxy.promerica.com.gt", 9095);
            proxy.Credentials = new NetworkCredential("servicio_bo", "B4nc0Pr0m3r!c4");
            client.Proxy = proxy;
            client.Timeout = -1;
            var request = new RestRequest("/shortlinks/shorten", Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("apikey", apikey);
            request.AddBody(linkVisa);
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            IRestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var link = JsonConvert.DeserializeObject<dynamic>(response.Content);
                strShortURL = link[0].shortUrl; //link.shortUrl;
            }
            else
            {
                Logger.LogInfo("GeneraURLCortoRestClient " + response.StatusCode);
                lblCorreoAlterno.Text = lblCorreo.Text;
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ServerLoadModal", "<script>$('#divNotificaAlterno').modal({backdrop: 'static', keyboard: false},\"show\");</script>", false);
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e);
        }
    }

    protected void btnEnvioCorreoLink_Click(object sender, EventArgs e)
    {
        try
        {
            #region construccion y preparacion envio correo alternativo
            //numero de la cuenta
            objLink.num_cuenta = txtCuenta.Text;
            //tipo de cuenta
            switch (this.gvCuentas.SelectedRow.Cells[1].Text)
            {
                case "Tarjeta":
                    objLink.tip_cuenta = "TC";
                    break;
                case "Prestamo":
                    objLink.tip_cuenta = "PR";
                    break;
            }
            //monto de cobro
            //20240222.ini: Se parsea el texto a numero
            objLink.mon_cobro = decimal.Round(Convert.ToDecimal(txtMonto.Text.Trim()), 2).ToString();
            //si realizara pago en dolares
            if (chkPago.Checked == true) //rdbPago
                objLink.tip_pago = "1";
            else
                objLink.tip_pago = "0";
            //tipo de link
            objLink.tip_link = ddlTipoLink.SelectedValue;
            if (ddlTipoLink.SelectedValue == "1")
                objLink.dia_mes = ddlDia.SelectedValue;
            else if (ddlTipoLink.SelectedValue == "2")
            {
                //obtengo el link generado 
                objLink.url_link = lnkURL.NavigateUrl;
            }
            //envio correo
            objLink.tip_envio = "2";
            //utilizara datos default
            objLink.es_default = ddlDatosCliente.SelectedValue;
            objLink.cod_sku = lnkURL.Text;
            objLink.url_link = lnkURL.NavigateUrl;

            objLink.ind_estado = "A";
            //notifica correo 
            string strHTML;

            clsMail correo = new clsMail();

            //reeleccion de correo a enviar
            switch (ddlDatoCorreo.SelectedValue)
            {
                //default
                case "1":
                    objLink.nom_correo = lblCorreoAlterno.Text;
                    correo.mail = lblCorreoAlterno.Text;
                    break;
                case "2":
                    objLink.nom_correo = txtMailAlterno.Text;
                    correo.mail = txtMailAlterno.Text;
                    break;
            }

            controllerLink objControllerLink = new controllerLink(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());
            //guardo el link
            if (!objControllerLink.insertLink(objLink))
                throw new Exception(objControllerLink.strReturnMessage);
            
            /* codigo antiguo arte correo 
            string strTipoProd = "";
            //obtengo la palabra a colocar en el arte del correo 
            switch (this.gvCuentas.SelectedRow.Cells[1].Text)
            {
                case "Tarjeta":
                    strTipoProd = "tarjeta de crédito";
                    break;
                case "Prestamo":
                    strTipoProd = "préstamo";
                    break;
            }*/
            
            
            //notificar correo
            correo.asunto = "Pago En Link";
            //formato html a crear para el correo donde se manda el tipo de producto y link respectivamente
            strHTML = "<tr>"
                        + "<td width = \"640\" bgcolor = \"#FFFFFF\" height = \"1\" style = \"line-height: 1px; font-size: 1px; background: #FFFFFF\" >"
                        + "<a href=\"" + objLink.url_link + "\" target = \"_blank\" ><img src = \"https://d2myra1hg65lx1.cloudfront.net/1696368768/assets/ddd0ad583af240c6a792607ce07f0987.png\" width = \"640\" style = \"display:block\" alt = \"BANCO PROMERICA\" ></a>"
                        + "</td>"
                        + "</tr>";
            #region ARTE CORREO ANTERIOR
            /* ARTE CORREO ANTERIOR
            strHTML = "<tr> "
                    + " <td> "
                    + "  <table border = \"0\" cellpadding = \"0\" cellspacing = \"0\" width = \"640\" > "
                    + "   <tbody> "
                    + "    <tr> "
                    + "     <td width = \"615\" bgcolor = \"#FFFFFF\" "
                    + "         style = \"padding: 0px 0px 0px 35px; text-align: left; color: #006937; font-family: Tahoma, Geneva, sans-serif; font-size: 30px; font-weight: bold;\" "
                    + "         valign = \"bottom\" >la forma ágil, rápida y segura de pagar tu " + strTipoProd + " </td> "
                    + "     </tr> "
                    + "   </tbody> "
                    + "  </table> "
                    + " </td> "
                    + "</tr> "
                    + "<tr> "
                    + " <td bgcolor = \"#FFFFFF\" ><img "
                    + "     src = \"https://www.bancopromerica.com.gt/recursos/images/mailing/2020/visalink1_251120_04.jpg\" "
                    + "     width = \"640\" height = \"145\" border=\"0\" style = \"display:block\" alt = \"BANCO PROMERICA\" ></td> "
                    + "</tr> "
                    + "<tr> "
                    + " <td> "
                    + "  <table border = \"0\" cellpadding = \"0\" cellspacing = \"0\" width = \"640\" > "
                    + "   <tbody> "
                    + "    <tr> "
                    + "     <td width=\"320\" bgcolor = \"#FFFFFF\" style = \"line-height: 1px; font-size: 1px;\" > "
                    + "        <a href=\"" + objLink.url_link + "\" target=\"_blank\"> "
                    + "           <img alt = \"BANCO PROMERICA\" width = \"320\" height = \"43\" "
                    + "               src = \"https://www.bancopromerica.com.gt/recursos/images/mailing/2020/visalink1_251120_05.jpg\" "
                    + "               style = \"display:block\" /></a> "
                    + "     </td> "
                    + "     <td width = \"320\" bgcolor = \"#FFFFFF\" style = \"line-height: 1px; font-size: 1px;\" ></td> "
                    + "    </tr> "
                    + "   </tbody> "
                    + "  </table> "
                    + " </td> "
                    + "</tr> ";*/
            #endregion

            //correo.mail = lblCorreo.Text;
            correo.link = strHTML;
            #endregion

            controllerLink objControllerLink2 = new controllerLink(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());
            if (!objControllerLink2.notificaMail(correo))
                        throw new Exception("Error: al enviar la notificación del link F/M (notificarCliente)");

            //REGISTRA BITACORA
            RegistraBitacora(objLink);

            Master.strMensajeConfirmacion = "Link Parametrizado y Enviado!!!";
            Session["PageRedirect"] = true;
            Response.Redirect("frmIndex.aspx", false);
        }
        catch (Exception ex)
        {
            Master.strMensajeError = ex.Message;
        }
    }
    protected void OnBlur(object sender, EventArgs e) //ValidationErrorEventArgs 
    {
        //aqui debe iniciar el proceso de validación del monto de los productos 
        try
        {
            //debo validar si es prestamo 
            Logger.LogInfo("onBlur " + this.gvCuentas.SelectedRow.Cells[1].Text);
            if (this.gvCuentas.SelectedRow.Cells[1].Text == "Prestamo")
            {
                //procedo a validar monto de PR
                controllerProducto objProducto = new controllerProducto(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());

                if (!objProducto.getMontoPR(txtCuenta.Text))
                    throw new Exception(objProducto.strReturnMessage);

                Logger.LogInfo("onBlur conversión a decimal " + objProducto.dsReturn.Tables["datos_producto"].Rows[0]["VALOR"]);
                if (Convert.ToDecimal(txtMonto.Text) > Convert.ToDecimal(objProducto.dsReturn.Tables["datos_producto"].Rows[0]["VALOR"]))
                {
                    //bandera para evitar realizar acción en boton
                    AccionBoton = false;
                    Logger.LogInfo("onBlur conversión a decimal >  " + objProducto.dsReturn.Tables["datos_producto"].Rows[0]["VALOR"]);
                    MuestraAlerta("Ingresar un monto correcto!!! Menor o igual a (" + string.Format(new System.Globalization.CultureInfo("es-GT"), "{0:C}", objProducto.dsReturn.Tables["datos_producto"].Rows[0]["VALOR"]) + ") ");
                }
                else
                {
                    AccionBoton = true;
                }
            }
            else
            {
                //procedo a validar monto de TC
                controllerProducto objProducto = new controllerProducto(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());

                if (!objProducto.getMontoTC(txtCuenta.Text))
                    throw new Exception(objProducto.strReturnMessage);

                if (Convert.ToDecimal(txtMonto.Text) > Convert.ToDecimal(objProducto.dsReturn.Tables["datos_producto"].Rows[0]["VALOR"]))
                {
                    //bandera para evitar realizar acción en boton
                    AccionBoton = false;
                    MuestraAlerta("Ingresar un monto correcto!!! Menor o igual a (" + string.Format(new System.Globalization.CultureInfo("es-GT"), "{0:C}", objProducto.dsReturn.Tables["datos_producto"].Rows[0]["VALOR"]) + ") ");
                }
                else
                {
                    AccionBoton = true;
                }

            }
        }
        catch (Exception ex)
        {
            Master.strMensajeError = ex.Message;
        }
        //MuestraAlerta("Aqui va el mensaje respectivo!!!"); //, MessageType.Error
    }
    protected void MuestraAlerta(string Message) //, MessageType type
    {
        ScriptManager.RegisterStartupScript(this, this.GetType(), System.Guid.NewGuid().ToString(), "ShowMessage('" + Message + "');", true); //,'" + type + "'
    }
    public bool notificarCliente(string strTipoProducto, string strArteSMS)
    {
        try
        {
            string strHTML;

            clsMail correo = new clsMail();
            clsSMS sms = new clsSMS();

            controllerLink objControllerLink = new controllerLink(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());

            if (rdbSMS.Checked)
            {
                int maxLength = Math.Min(strArteSMS.Length, 90);

                sms.mensaje = strArteSMS.Substring(0, maxLength) + "  "+strShortURL;
                sms.num_cta = txtCuenta.Text;
                //notificar SMS
                switch (ddlDatosCliente.SelectedValue)
                {
                    //default
                    case "1":
                        sms.telefono = lblTelefono.Text;
                        //envio de mensaje con el link
                        if (!objControllerLink.notificaSMS(sms))
                            throw new Exception("Error: al enviar notificacion SMS del link F/M (notificarCliente)");
                        break;
                    //editado
                    case "2":
                        sms.telefono = txtTelefono.Text;
                        //envio de mensaje con el link
                        if (!objControllerLink.notificaSMS(sms))
                            throw new Exception("Error: al enviar notificacion SMS del link F/M (notificarCliente)");
                        break;
                }
            }
            else if (rdbsMail.Checked)
            {
                //notificar correo
                correo.asunto = "Pago En Link";
                //formato html a crear para el correo donde se manda el tipo de producto y link respectivamente
                #region codigo html
                strHTML = "<tr>"
                        + "<td width = \"640\" bgcolor = \"#FFFFFF\" height = \"1\" style = \"line-height: 1px; font-size: 1px; background: #FFFFFF\" >"
                        + "<a href=\"" + strShortURL + "\" target = \"_blank\" ><img src = \"https://d2myra1hg65lx1.cloudfront.net/1696368768/assets/ddd0ad583af240c6a792607ce07f0987.png\" width = \"640\" style = \"display:block\" alt = \"BANCO PROMERICA\" ></a>"
                        + "</td>"
                        + "</tr>";
                #region ARTE CORREO ANTERIOR
                /*   ARTE CORREO ANTERIOR
                strHTML = "<tr> "
                        + " <td> "
                        + "  <table border = \"0\" cellpadding = \"0\" cellspacing = \"0\" width = \"640\" > "
                        + "   <tbody> "
                        + "    <tr> "
                        + "     <td width = \"615\" bgcolor = \"#FFFFFF\" "
                        + "         style = \"padding: 0px 0px 0px 35px; text-align: left; color: #006937; font-family: Tahoma, Geneva, sans-serif; font-size: 30px; font-weight: bold;\" "
                        + "         valign = \"bottom\" >la forma ágil, rápida y segura de pagar tu " + strTipoProducto + " </td> "
                        + "     </tr> "
                        + "   </tbody> "
                        + "  </table> "
                        + " </td> "
                        + "</tr> "
                        + "<tr> "
                        + " <td bgcolor = \"#FFFFFF\" ><img "
                        + "     src = \"https://www.bancopromerica.com.gt/recursos/images/mailing/2020/visalink1_251120_04.jpg\" "
                        + "     width = \"640\" height = \"145\" border=\"0\" style = \"display:block\" alt = \"BANCO PROMERICA\" ></td> "
                        + "</tr> "
                        + "<tr> "
                        + " <td> "
                        + "  <table border = \"0\" cellpadding = \"0\" cellspacing = \"0\" width = \"640\" > "
                        + "   <tbody> "
                        + "    <tr> "
                        + "     <td width=\"320\" bgcolor = \"#FFFFFF\" style = \"line-height: 1px; font-size: 1px;\" > "
                        + "        <a href=\"" + "https://" + strShortURL + "\" target=\"_blank\"> "
                        + "           <img alt = \"BANCO PROMERICA\" width = \"320\" height = \"43\" "
                        + "               src = \"https://www.bancopromerica.com.gt/recursos/images/mailing/2020/visalink1_251120_05.jpg\" "
                        + "               style = \"display:block\" /></a> "
                        + "     </td> "
                        + "     <td width = \"320\" bgcolor = \"#FFFFFF\" style = \"line-height: 1px; font-size: 1px;\" ></td> "
                        + "    </tr> "
                        + "   </tbody> "
                        + "  </table> "
                        + " </td> "
                        + "</tr> ";*/
                #endregion
                #endregion
                switch (ddlDatosCliente.SelectedValue)
                {
                    //default
                    case "1":
                        correo.mail = lblCorreo.Text;
                        correo.link = strHTML;
                        if (!objControllerLink.notificaMail(correo))
                            throw new Exception("Error: al enviar la notificación del link F/M (notificarCliente)");
                        break;
                    //editado
                    case "2":
                        correo.mail = txtMail.Text;
                        correo.link = strHTML;
                        if (!objControllerLink.notificaMail(correo))
                            throw new Exception("Error: al enviar la notificación del link F/M (notificarCliente)");
                        break;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

}