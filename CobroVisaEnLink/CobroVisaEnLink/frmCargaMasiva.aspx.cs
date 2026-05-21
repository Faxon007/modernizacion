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
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using RestSharp;
using System.Net;

public partial class frmCargaMasiva : System.Web.UI.Page
{
    private string error;
    //private string strTipoCuenta;
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

    //------------------------------------------------
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
            if (!objControllerMenu.verificarMenuItem(Session["usuario"].ToString(), "9510"))
                Response.Redirect("frmIndex.aspx");
            #endregion

            #region inicializacion de pagina
            Master.strPagina = "Carga Masiva Parámetros";
            //string cod_cliente = Request.QueryString["cli"];
            //if (cod_cliente != null)
            //    Master.strPagina = "Editar Enrolamiento Cliente";
            #endregion

            if (!this.IsPostBack)
            {
                //string script = "$(document).ready(function () { $('[id*=btnImport]').click(); });";
                //ClientScript.RegisterStartupScript(this.GetType(), "load", script, true);

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
    protected void ImportCSV(object sender, EventArgs e)
    {
        try
        {
            //Check file is available in File upload Control
            if (FileUpload1.HasFile)
            {
                // Read column headers from file
                //CsvConfiguration conf = new CsvConfiguration(System.Globalization.CultureInfo.CurrentCulture);
                //conf.Delimiter = ",";
                //conf.AllowComments = false;
                //conf.MissingFieldFound = Nothing;
                //conf.HasHeaderRecord = true;

                //StreamReader streamReader = new StreamReader(csvPath);
                StreamReader streamReader = new StreamReader(FileUpload1.PostedFile.InputStream);
                CsvReader csv = new CsvReader(streamReader, new CsvConfiguration(System.Globalization.CultureInfo.CurrentCulture) { HasHeaderRecord = true, Delimiter = "," });
                //csv.Configuration.Delimiter = ",";

                csv.Read();
                csv.ReadHeader();

                //Create a DataTable.  
                DataTable dt = new DataTable();
                // se construye la estructura del DataTable
                dt.Columns.AddRange(new DataColumn[9] { new DataColumn("Tipo Cuenta", typeof(string)), //tip_cuenta
                                                        new DataColumn("Número Cuenta", typeof(string)), // num_cuenta
                                                        new DataColumn("Monto", typeof(string)),
                                                        new DataColumn("Tipo Pago", typeof(string)), //tip_pago
                                                        new DataColumn("Tipo Link", typeof(string)), //tip_link
                                                        new DataColumn("Día del Mes", typeof(string)), //dia_envio
                                                        new DataColumn("Tipo Envío", typeof(string)), // tip_envio
                                                        new DataColumn("Correo / Télefono", typeof(string)), //dato_envio
                                                        new DataColumn("Resultado", typeof(string))
                                                      });
                ////Execute a loop over the rows.
                while (csv.Read())
                {
                    //inicializar
                    error = null;
                    var row = dt.NewRow();

                    //objeto con los datos del link 
                    clsLink objLink = new clsLink();

                    //Execute a loop over the columns.
                    //foreach (DataColumn column in dt.Columns)
                    for (int i = 0; i < dt.Columns.Count - 1; i++)
                    {
                        //var variable = csv.GetField(column.DataType, column.ColumnName.ToUpper());
                        var variable = csv.GetField(i);
                        string datoVisual = "";

                        //switch (column.ColumnName)
                        switch (i)
                        {
                            #region parametros a verificar en la lectura
                            case 0: // si es PR o TC  "tip_cuenta"
                                objLink.tip_cuenta = variable.ToString();
                                switch (objLink.tip_cuenta)
                                {
                                    case "PR":
                                        datoVisual = "Préstamo";
                                        break;
                                    case "TC":
                                        datoVisual = "Tarjeta";
                                        break;
                                    default:
                                        error += "\r\n" + "Error (TIP_CUENTA): Opción no válida!!!";
                                        break;
                                }
                                break;
                            case 1: // numero de cuenta "num_cuenta"
                                if (variable.ToString() == "")
                                    error += "\r\n" + "Error (NUM_CUENTA): Ingresar número de cuenta!!!";
                                else
                                {
                                    controllerProducto objProd = new controllerProducto(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());
                                    //debo validar que la cuenta existe 
                                    if (!objProd.getExisteCta(variable.ToString()))
                                    {
                                        error += "\r\n" + objProd.strReturnMessage;
                                        datoVisual = variable.ToString();
                                    }

                                    if (!ReferenceEquals(null, objProd.dsReturn))
                                    {
                                        //TOdO: Validacion de cliente si pertenece a lista negra
                                        string codCliente = objProd.dsReturn.Tables["datos_cta"].Rows[0]["COD_CLIENTE"].ToString();

                                        if (string.IsNullOrEmpty(codCliente))
                                        {
                                            error += "\r\n" + "El cliente no existe";
                                        }

                                        if (!objProd.GetClienteListaNegra("1", codCliente))
                                        {
                                            error += "\r\n" + objProd.strReturnMessage;
                                        }
                                        //ToDO:

                                        objLink.num_cuenta = variable.ToString();
                                        datoVisual = objLink.num_cuenta;
                                    }
                                    //    else
                                    //        error += "\r\n" + "Error (NUM_CUENTA): Cuenta no existe!!!";
                                    //
                                }
                                break;
                            case 2:   //monto a redondear y validar "monto"
                                if (variable.ToString() == "0" || String.IsNullOrEmpty(variable.ToString()))
                                    error += "\r\n" + "Error (MONTO): Debe ingresar un monto!!!";
                                else
                                {
                                    if (!string.IsNullOrEmpty(objLink.num_cuenta))
                                    {
                                        objLink.mon_cobro = Decimal.Round(Convert.ToDecimal(variable), 2).ToString();
                                        //debo validar el monto aqui 
                                        ValidaMonto(objLink.num_cuenta, objLink.tip_cuenta, objLink.mon_cobro);
                                        //obtento el dato a mostrar en el grid
                                        datoVisual = string.Format(new System.Globalization.CultureInfo("es-GT"), "{0:C}", objLink.mon_cobro);
                                    }
                                }
                                break;
                            case 3: // si paga en dolares "tip_pago"
                                objLink.tip_pago = variable.ToString();
                                switch (objLink.tip_pago)
                                {
                                    case "0":
                                        datoVisual = "Quetzal";
                                        break;
                                    case "1":
                                        datoVisual = "Dólar";
                                        break;
                                    default:
                                        error += "\r\n" + "Error (TIP_PAGO): Opción no válida!!!";
                                        break;
                                }
                                break;
                            case 4: // si es link manual o automático "tip_link"
                                objLink.tip_link = variable.ToString();
                                switch (objLink.tip_link)
                                {
                                    case "1":
                                        datoVisual = "Automático";
                                        break;
                                    case "2":
                                        datoVisual = "Manual";
                                        break;
                                    default:
                                        error += "\r\n" + "Error (TIP_LINK): Opción no válida!!!";
                                        break;
                                }
                                break;
                            case 5: // solo para día automatico se toma día a generar "dia_envio"
                                if (objLink.tip_link == "1")
                                {
                                    if (!String.IsNullOrEmpty(variable.ToString()))
                                    {
                                        objLink.dia_mes = variable.ToString();
                                        datoVisual = objLink.dia_mes;
                                    }
                                    else
                                        error += "\r\n" + "Error (DIA_MES): Debe ingresar día cuando es link automático!!!";
                                }
                                else
                                {
                                    //valido que no envíe día cuando es manual
                                    if (!String.IsNullOrEmpty(variable.ToString()))
                                        error += "\r\n" + "Error (DIA_MES): No debe ingresar día cuando es link manual!!!";
                                }
                                break;
                            case 6:  // si se envía por SMS o CORREO      "tip_envio"
                                objLink.tip_envio = variable.ToString();
                                switch (objLink.tip_envio)
                                {
                                    case "1":
                                        datoVisual = "SMS";
                                        break;
                                    case "2":
                                        datoVisual = "Correo";
                                        break;
                                    default:
                                        error += "\r\n" + "Error (TIP_ENVIO): Opción no válida!!!";
                                        break;
                                }
                                break;
                            case 7: //"dato_envio"
                                switch (objLink.tip_envio)
                                {
                                    case "1":
                                        //20240312.ini Valida el telefono
                                        if (!ValidatorsHelpers.ValidateTelefono(variable.ToString()))
                                        {
                                            error += "\r\n" + "Error (TELEFONO): Num teléfono no válido!!!";
                                            datoVisual = variable.ToString();
                                        }
                                        else
                                        {
                                            objLink.num_telefono = variable.ToString();
                                            datoVisual = objLink.num_telefono;

                                        }
                                        //20240312.ini
                                        break;
                                    case "2":
                                        //20240312.ini Valida el email
                                        if (!ValidatorsHelpers.ValidateEmail(variable.ToString()))
                                        {
                                            error += "\r\n" + "Error (EMAIL): Email no válido!!!";
                                            datoVisual = variable.ToString();
                                        }
                                        else
                                        {
                                            //20240312.ini
                                            objLink.nom_correo = variable.ToString();
                                            datoVisual = objLink.nom_correo;
                                        }

                                        break;
                                }
                                break;
                                #endregion
                        }

                        // se debe colocar el nuevo valor a mostrar column.ColumnName
                        row[i] = datoVisual;//csv.GetField(column.DataType, column.ColumnName.ToUpper());
                    }

                    objLink.ind_estado = "A";

                    // se verifica si existen errores
                    if (String.IsNullOrEmpty(error))
                    {
                        // llamar al procedimiento de Link y verificar que no existe error 
                        if (!ObtencionLinkMasivo(objLink))
                        {
                            error += "\r\n" + "Error al crear Link: " + error;
                            row[8] = error;
                        }
                    }
                    else
                        row[8] = error;
                    //SE DEBE VALIDAR QUE EXISTA CUENTA

                    //Se agrega el código SKU del link
                    if (!String.IsNullOrEmpty(objLink.cod_sku))
                    {
                        row[8] = objLink.cod_sku;
                    }
                    //agrego la fila respectiva
                    dt.Rows.Add(row);
                    //value = csv.GetField("NUM_CUENTA");
                }

                //Bind the DataTable.  
                gvParametros.DataSource = dt;
                gvParametros.DataBind();

                streamReader.Close();
                streamReader.Dispose();

            }
            else
                Master.strMensajeError = "No existe archivo a cargar";
        }
        catch (DirectoryNotFoundException dirEx)
        {
            // Let the user know that the directory did not exist.
            Master.strMensajeError = "Directorio no encontrado: " + dirEx.Message;
        }
    }
    void ValidaMonto(string strCuenta, string strTipoProducto, string strMonto)
    {
        //aqui debe iniciar el proceso de validación del monto de los productos 
        try
        {
            //debo validar si es prestamo 
            if (strTipoProducto == "PR")
            {
                //procedo a validar monto de PR
                controllerProducto objProducto = new controllerProducto(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());

                if (!objProducto.getMontoPR(strCuenta))
                    throw new Exception(objProducto.strReturnMessage);

                
                
                if (Convert.ToDecimal(strMonto) > Convert.ToDecimal(objProducto.dsReturn.Tables["datos_producto"].Rows[0]["VALOR"]))
                    error += "\r\n" + "Error: Ingresar un monto correcto!!! Menor o igual a (" + string.Format(new System.Globalization.CultureInfo("es-GT"), "{0:C}", objProducto.dsReturn.Tables["datos_producto"].Rows[0]["VALOR"]) + ")";
            }
            else
            {
                //procedo a validar monto de TC
                controllerProducto objProducto = new controllerProducto(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());

                if (!objProducto.getMontoTC(strCuenta))
                    throw new Exception(objProducto.strReturnMessage);

                if (Convert.ToDecimal(strMonto) > Convert.ToDecimal(objProducto.dsReturn.Tables["datos_producto"].Rows[0]["VALOR"]))
                    error += "\r\n" + "Error: Ingresar un monto correcto!!! Menor o igual a (" + string.Format(new System.Globalization.CultureInfo("es-GT"), "{0:C}", objProducto.dsReturn.Tables["datos_producto"].Rows[0]["VALOR"]) + ")";
            }
        }
        catch (Exception ex)
        {
            Master.strMensajeError = ex.Message;
        }
    }
    private bool ObtencionLinkMasivo(clsLink objLinkParametro)
    {
        try
        {
            VisaEnLink Link = new VisaEnLink(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());
            //parametros
            controllerSitio objcontrollerSitio = new controllerSitio(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());

            if (!objcontrollerSitio.getParametros())
                throw new Exception("Error (GetParametros): " + objcontrollerSitio.strReturnMessage);

            if (objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows.Count > 0 && objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows[0]["API_IMAGEN"] != null)
            {
                byte[] byteArray = (Byte[])objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows[0]["API_IMAGEN"];
                //Convert the Byte Array to Base64 Encoded string.
                string base64String = Convert.ToBase64String(byteArray, 0, byteArray.Length);

                // verifico que el dato seleccionado fue prestamo
                if (objLinkParametro.tip_cuenta == "PR")
                {
                    controllerCliente objProducto = new controllerCliente(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());
                    //obtengo la moneda del prestamo
                    if (!objProducto.getTipoPrestamo(objLinkParametro.num_cuenta))
                        throw new Exception(objProducto.strReturnMessage);
                    //valido si los datos ingresados son correctos
                    if (Convert.ToString(objProducto.dsReturn.Tables["prestamo"].Rows[0]["MONEDA"]) == "840" && objLinkParametro.tip_pago == "0") //rdbPago
                        throw new Exception("Error: La opción de pago es para un préstamo en dolares");
                    if (Convert.ToString(objProducto.dsReturn.Tables["prestamo"].Rows[0]["MONEDA"]) == "320" && objLinkParametro.tip_pago == "1")
                        throw new Exception("Error: La opción de pago es para un préstamo en quetzales");
                }

                //verificar si es link automatico y el dia seleccionado para generar el link en visaenlink
                if (objLinkParametro.tip_link == "1" && objLinkParametro.dia_mes == DateTime.Now.Day.ToString())
                {
                    //Se revisa si existe token del dia
                    if (!Link.ExisteToken())
                        throw new Exception("Error (ExisteToken): " + Link.error);

                    //CONSTRUYO SECUENCIA 
                    if (!objcontrollerSitio.obtengoCodigoInterno())
                        throw new Exception("Error: " + objcontrollerSitio.strReturnMessage);

                    string strCodigoInterno = objcontrollerSitio.dsReturn.Tables["codigo_interno"].Rows[0]["CORRELATIVO"].ToString();

                    //Se procede a Crear Link
                    if (!Link.CrearLink(objLinkParametro.num_cuenta, objLinkParametro.mon_cobro, (base64String), strCodigoInterno)) //"data:image/png;base64," + 
                        throw new Exception("Error (CrearLink): " + Link.error);
                    //se asignan los valores respectivos obtenidos del API
                    
                    objLinkParametro.cod_sku = Link.ObtengoSKU();
                    objLinkParametro.url_link = Link.ObtengoURL();
                   

                    //se debe obtener el shortcut de rebrandly
                    //GeneraURLCorto(objLinkParametro.url_link);
                    //20240529.ini Ejecuta metodo para obtener shortcut de rebrandly con restclient
                    GeneraURLCortoRestClient(objLinkParametro.url_link);
                    //valido que se genere el link corto 
                    if (String.IsNullOrEmpty(strShortURL))
                        throw new Exception("Error (GeneraURLCorto): No se logró obtener link corto!!!");
                    //obtengo el link corto 
                    objLinkParametro.url_corto = strShortURL;
                    //*********************************************
                    //obtengo el parametro del texto SMS
                    controllerSitio objParametros = new controllerSitio(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());

                    if (!objParametros.getParametros())
                        throw new Exception(objParametros.strReturnMessage);

                    string strTipoProd = "";

                    switch (objLinkParametro.tip_cuenta)
                    {
                        case "TC":
                            strTipoProd = "tarjeta de crédito";
                            break;
                        case "PR":
                            strTipoProd = "préstamo";
                            break;
                    }

                    //Enviar link por SMS o Correo
                    if (!notificarCliente(strTipoProd, objParametros.dsReturn.Tables["param_sistema"].Rows[0]["MSG_SMS"].ToString(), objLinkParametro))
                        throw new Exception("Error (notificarCliente):" + error);
                }
                else if (objLinkParametro.tip_link == "2")
                {
                    //Se revisa si existe token del dia
                    if (!Link.ExisteToken())
                        throw new Exception("Error (ExisteToken): " + Link.error);
                    //CONSTRUYO SECUENCIA 
                    if (!objcontrollerSitio.obtengoCodigoInterno())
                        throw new Exception("Error: " + objcontrollerSitio.strReturnMessage);

                    string strCodigoInterno = objcontrollerSitio.dsReturn.Tables["codigo_interno"].Rows[0]["CORRELATIVO"].ToString();
                    //Se procede a Crear Link
                    if (!Link.CrearLink(objLinkParametro.num_cuenta, objLinkParametro.mon_cobro, (base64String), strCodigoInterno)) //"data:image/png;base64," 
                        throw new Exception("Error (CrearLink): " + Link.error);
                    //se asignan los valores respectivos obtenidos del API
                    
                   objLinkParametro.cod_sku = Link.ObtengoSKU();
                   objLinkParametro.url_link = Link.ObtengoURL();
                    /*para pruebas
                    objLinkParametro.cod_sku = "OK";
                    objLinkParametro.url_link = "https://www.clubpromerica.com/guatemala";
                    */

                    //se debe obtener el shortcut de rebrandly
                    //GeneraURLCorto(objLinkParametro.url_link);
                    //20240529.ini Ejecuta metodo para obtener shortcut de rebrandly con restclient
                    GeneraURLCortoRestClient(objLinkParametro.url_link);
                    //valido que se genere el link corto 
                    if (String.IsNullOrEmpty(strShortURL))
                        throw new Exception("Error (GeneraURLCorto): No se logró obtener link corto!!!");
                    //obtengo el link corto 
                    objLinkParametro.url_corto =  strShortURL;
                    //*********************************************
                    //obtengo el parametro del texto SMS
                    controllerSitio objParametros = new controllerSitio(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());

                    if (!objParametros.getParametros())
                        throw new Exception(objParametros.strReturnMessage);

                    string strTipoProd = "";

                    switch (objLinkParametro.tip_cuenta)
                    {
                        case "TC":
                            strTipoProd = "tarjeta de crédito";
                            break;
                        case "PR":
                            strTipoProd = "préstamo";
                            break;
                    }

                    //Enviar link por SMS o Correo
                    if (!notificarCliente(strTipoProd, objParametros.dsReturn.Tables["param_sistema"].Rows[0]["MSG_SMS"].ToString(), objLinkParametro))
                        throw new Exception("Error (notificarCliente):" + error);
                }

                controllerLink objControllerLink = new controllerLink(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());

                //guardo los parametros del link
                if (!objControllerLink.insertLink(objLinkParametro))
                    throw new Exception(objControllerLink.strReturnMessage);


                //REGISTRA BITACORA
                RegistraBitacora(objLinkParametro);

                //Master.strMensajeConfirmacion = "Link Parametrizado!!!";
                //Session["PageRedirect"] = true;
                //Response.Redirect("frmIndex.aspx", false);

            }
            else
                throw new Exception("Error: se debe tener una imagen parametrizada");

            return true;
        }
        catch (Exception ex)
        {
            error += ex.Message;
            return false;
        }
    }
    void ProcesoNotificacion(clsLink objLinkParametro)
    {

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
    public bool notificarCliente(string strTipoProducto, string strArteSMS, clsLink datosLink)//string strTipoEnvio
    {
        try
        {
            //string strHTML;

            clsMail correo = new clsMail();
            clsSMS sms = new clsSMS();

            controllerLink objControllerLink = new controllerLink(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());


            if (datosLink.tip_envio == "1")
            {
                int maxLength = Math.Min(strArteSMS.Length, 90);

                sms.mensaje = strArteSMS.Substring(0, maxLength) + "  " + strShortURL;
                sms.num_cta = datosLink.num_cuenta;
                //notificar SMS
                sms.telefono = datosLink.num_telefono;
                //envio de mensaje con el link
                if (!objControllerLink.notificaSMS(sms))
                    throw new Exception("Error: al enviar notificacion SMS del link F/M (notificarCliente)");

            }
            else if (datosLink.tip_envio == "2")
            {
                //notificar correo
                correo.asunto = "Pago En Link";
                //formato html a crear para el correo donde se manda el tipo de producto y link respectivamente
                string strHTML = "<tr>"
                               + "<td width = \"640\" bgcolor = \"#FFFFFF\" height = \"1\" style = \"line-height: 1px; font-size: 1px; background: #FFFFFF\" >"
                               + "<a href=\"" + "https://" + strShortURL + "\" target = \"_blank\" ><img src = \"https://d2myra1hg65lx1.cloudfront.net/1696368768/assets/ddd0ad583af240c6a792607ce07f0987.png\" width = \"640\" style = \"display:block\" alt = \"BANCO PROMERICA\" ></a>"
                               + "</td>"
                               + "</tr>";
                #region arte correo anterior
                /*   ARTE CORREO ANTERIOR
                string strHTML = "<tr> "
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
                //correo electronico
                correo.mail = datosLink.nom_correo;
                correo.link = strHTML;
                if (!objControllerLink.notificaMail(correo))
                    throw new Exception("Error: al enviar la notificación del link F/M (notificarCliente)");
            }

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
    protected void btnNuevaCarga_Click(object sender, EventArgs e)
    {
        Session["PageRedirect"] = true;
        Response.Redirect("frmCargaMasiva.aspx", false);
    }
    protected void btnObtenerLink_Click(object sender, EventArgs e)
    {
        /*
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
        */
    }
    void RegistraBitacora(clsLink datosLink)
    {
        try
        {
            controllerSitio objControllerSitio = new controllerSitio(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());
            //objeto bitacora
            clsBitacora objBitacora = new clsBitacora();

            objBitacora.cod_link = "";
            objBitacora.cod_parametro = "";
            objBitacora.descripcion = "Se creo link (" + datosLink.cod_sku + ") asociado al número de cuenta (" + datosLink.tip_cuenta + ") No." + datosLink.num_cuenta;
            objBitacora.tip_procesamiento = "C"; //creacion

            if (!objControllerSitio.registraBitacora(objBitacora))
                throw new Exception(objControllerSitio.strReturnMessage);

            clsBitCore objBitCore = new clsBitCore();

            controllerCliente objCliente = new controllerCliente(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());
            //obtengo el codigo de cliente
            if (!objCliente.getCliente_cta(datosLink.num_cuenta))
                throw new Exception(objCliente.strReturnMessage);

            objBitCore.cod_persona = objCliente.dsReturn.Tables["datos_cliente"].Rows[0]["COD_CLIENTE"].ToString();

            switch (datosLink.tip_cuenta)
            {
                case "PR":
                    objBitCore.tip_cuenta = "PR";
                    objBitCore.num_cta_prestamo = datosLink.num_cuenta;
                    break;
                case "TC":
                    objBitCore.tip_cuenta = "TC";
                    objBitCore.num_cta_credito = datosLink.num_cuenta;
                    break;
            }

            string strNotificacion = "";


            switch (datosLink.tip_envio)
            {
                //SMS
                case "1":
                    strNotificacion = "Teléfono: " + datosLink.num_telefono;
                    break;
                //CORREO
                case "2":
                    strNotificacion = "Correo: " + datosLink.nom_correo;
                    break;
            }

            objBitCore.descripcion = "Se realiza envío de Link para pago al " + strNotificacion + " por Q." + datosLink.mon_cobro; //

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

        using (var httpClient = new HttpClient { BaseAddress = new Uri("https://enterprise-api.rebrandly.com") }) //api viejo https://api.rebrandly.com
        {
            httpClient.DefaultRequestHeaders.Add("apikey", strApiKey);
            httpClient.DefaultRequestHeaders.Add("workspace", "1cf40cc1d152469abd784d23bb664e64");

            var body = new StringContent(
                JsonConvert.SerializeObject(linkVisa), UnicodeEncoding.UTF8, "application/json");

            var response = httpClient.PostAsync("/v1/links", body).Result;

            if (response.IsSuccessStatusCode)
            {
                var link = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);

                strShortURL = link[0].shortUrl; //link.shortUrl;
            }
            else
            {
                throw new Exception("Error: No se pudo crear el link corto (GeneraURLCorto)");
            }
        }

    }


    //ToDO: 20240529.ini Metodo de generar url corto usando RestClient
    public void GeneraURLCortoRestClientBk(string strURL)
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
        /*
        var linkVisa = new
        {
            url = strURL,
            domain = strDominioNew
        };*/

        var client = new RestClient("https://enterprise-api.rebrandly.com");
        //var client = new RestClient("https://api.tinyurl.com");
        var proxy = new WebProxy("webproxy.promerica.com.gt", 9095);
        proxy.Credentials = new NetworkCredential("servicio_bo", "B4nc0Pr0m3r!c4");
        client.Proxy = proxy;
        client.Timeout = -1;
        var request = new RestRequest("/v1/links", Method.POST);
        //var request = new RestRequest("/create", Method.POST);
        request.RequestFormat = DataFormat.Json;
        //request.AddHeader("Authorization", strApiKey);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("apikey", strApiKey);
        request.AddHeader("workspace", strWorkspace);

        request.AddJsonBody(linkVisa);
        //certificado de seguridad
        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
        IRestResponse response = client.Execute(request);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var link = JsonConvert.DeserializeObject<dynamic>(response.Content);
            //strShortURL = link.data.tiny_url.ToString();
            strShortURL = link[0].shortUrl; //link.shortUrl;
        }
        else
        {
            throw new Exception("Error: No se pudo crear el link corto (GeneraURLCorto)");
        }

    }
    //ToDO: 20240529.ini Metodo de generar url corto usando RestClient

    //23/01/2026 LDMR. Llama al acortador de links de promerica
    public void GeneraURLCortoRestClient(string strURL)
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
            throw new Exception("Error: No se pudo crear el link corto (GeneraURLCorto)");
        }
    }

}