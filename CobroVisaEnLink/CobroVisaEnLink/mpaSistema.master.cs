using cloudconnect20;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class mpaSistema : System.Web.UI.MasterPage
{
    // Variables de conexion
    string strUsuario;
    string strPassword;
    string strPath;
    string strPalabra;
    string strConexion;
    string strConexionCbs;
    string strPathSrv;

    // Funcionamiento del sistema
    //char[] chrDelimitador = new char[] { ',' };
    //public const int intSistema = 150;
    const string strNombreSistema = "Sistema de Pagos NeoLink";
    const string strPaginaInicio = "default.aspx";

    /// <summary>
    /// Objeto de acceso a la base de datos
    /// </summary>
    private cloudconnect20.Oracle objOracle;
    //private cloudconnect20.Oracle objOracleCbs;

    /// <summary>
    /// Nombre del sistema.
    /// </summary>
    /// <value>Nombre a colocar para el sistema.</value>
    public string strSistema
    {
        get { return lblSistema.Text; }
        set { lblSistema.Text = value; }
    }

    /// <summary>
    /// Nombre de la página.
    /// </summary>
    /// <value>Nombre a colocar para la página.</value>
    public string strPagina
    {
        //set { lblTituloPagina.Text = value; }
        set
        {
            lblTituloPagina.Text = value;
            if (value != "")
                dvTituloPagina.Visible = true;
            else
                dvTituloPagina.Visible = false;
        }
        get { return lblTituloPagina.Text; }
    }

    public string strPathServer
    {
        get { return strPathSrv; }
        set { strPathSrv = value; }
    }

    /// <summary>
    /// Devuelve el codigo del usuario que está logueado en el sistema.
    /// </summary>
    /// <value>Codigo del usuario logueado.</value>
    public string strCodigoUsuario
    {
        get { strUsuario = Session["usuario"].ToString(); return strUsuario; }
    }

    /// <summary>
    /// Devuelve el nombre del usuario que está logueado
    /// </summary>
    /// <returns>Nombre del usuario logueado</returns>
    public string obtenerNombreUsuario()
    {
        string strResultado = "";

        if (!usuarioAutenticado())
        {
            strMensajeErrorModal = "Usuario no autenticado. Por favor ingrese desde la página default.";
            return strResultado;
        }

        inicializarValoresConexion();
        objOracle = new cloudconnect20.Oracle();

        if (!objOracle.CargarConexion(strPath, strPalabra, strConexion, "", "", strUsuario, strPassword))
        {
            strMensajeErrorModal = "No se pudo iniciar la conexion al sistema: " + objOracle.strError.Substring(0, 150);
            return strResultado;
        }

        //Consultar el nombre de la persona
        string strSql = "SELECT empleado.primer_nombre||' '||empleado.segundo_nombre||' '||empleado.primer_apellido||' '||empleado.segundo_apellido AS NOMBRE\n"
            + "  FROM RRHH_USUARIO usuario, RRHH_EMPLEADO empleado\n"
            + " WHERE usuario.empleado = empleado.empleado\n"
            + "   AND usuario.usuario = '" + strUsuario + "'";
        DataSet dsResultado = objOracle.ConsultarDatos(strSql, "Datos");
        if (dsResultado == null)
        {
            strMensajeErrorModal = "Ocurrió un error al consultar el nombre del usuario: " + objOracle.strError.Substring(0, 150);
            return strResultado;
        }
        if (dsResultado.Tables["Datos"].Rows.Count == 0)
        {
            strMensajeErrorModal = "No se pudo obtener el nombre del usuario.";
            return strResultado;
        }
        strResultado = dsResultado.Tables["Datos"].Rows[0]["NOMBRE"].ToString();

        return strResultado;
    }

    /// <summary>
    /// Mensaje a mostrar.
    /// </summary>
    /// <value>El valor a colocar en el mensaje.</value>
    public string strMensajeErrorModal
    {
        set
        {

            lblMensajeErrorModal.Text = value;
            if (value != "")
            {
                //upnlMensajes.
                ScriptManager.RegisterStartupScript(this, this.GetType(), "master_modal", "ejacutar_modal();", true);
                divErrorModalMaster.Visible = true;
            }
            else
                divErrorModalMaster.Visible = false;
        }
        get { return lblMensajeErrorModal.Text; }
    }

    public string strMensajeConfirmacion
    {
        set
        {
            lblMensajeSuccess.Text = value;
            if (value != "")
            {
                //ScriptManager.RegisterStartupScript(this, this.GetType(), "master_alert_success", " master_alert_success();", true);
                divSuccessMaster.Visible = true;
            }
            else
                divSuccessMaster.Visible = false;
        }
        get { return lblMensajeSuccess.Text; }
    }

    public string strMensajeError
    {
        set
        {
            lblMensajeError.Text = value;
            if (value != "")
            {
                divErrorMaster.Visible = true;
            }
            else
                divErrorMaster.Visible = false;
        }
        get { return lblMensajeError.Text; }
    }

    protected void Page_Load()
    {
        strSistema = strNombreSistema;
        Page.Header.Title = strSistema;

        if (Session["PageRedirect"] != null)
            if (Session["PageRedirect"].ToString() == "True")
            {
                strMensajeError = Session["strMensajeError"].ToString();
                strMensajeErrorModal = Session["strMensajeErrorModal"].ToString();
                strMensajeConfirmacion = Session["strMensajeConfirmacion"].ToString();

                Session["PageRedirect"] = "";
                Session["strMensajeError"] = "";
                Session["strMensajeErrorModal"] = "";
                Session["strMensajeConfirmacion"] = "";
            }

        //Menu
        if (Session["strScriptMenuHorizontal"] != null)
            ltlMenu.Text = Session["strScriptMenuHorizontal"].ToString();
        else
            ltlMenu.Text = "";

        //PARA SISTEMAS EXTERNOS
        //string requi = Request.QueryString["requi"];
        //string path = HttpContext.Current.Request.Url.AbsolutePath;
        //string pagepath = path.Substring(path.LastIndexOf("/"));
        //if ((requi == null && pagepath == "/frmRequiTraslado.aspx") || pagepath == "/frmRequiPlaza.aspx" || pagepath == "/frmRequiPuesto.aspx" || pagepath == "/frmRequiBaja.aspx")
        //{
        //    if (Session["ccl_strSistemaExterno"] != null)
        //    {
        //        this.footer.Visible = false;
        //        this.header_sis.Visible = false;
        //        //this.title.Visible = false;
        //    }
        //}
    }
    protected void Page_Unload()
    {
        if (Session["PageRedirect"] != null)
            if (Session["PageRedirect"].ToString() == "True")
            {
                Session["strMensajeError"] = strMensajeError;
                Session["strMensajeErrorModal"] = strMensajeErrorModal;
                Session["strMensajeConfirmacion"] = strMensajeConfirmacion;
            }        
    }

    /*public void colocarFoco(Control c)
    {
        smgPrincipal.SetFocus(c);
    }*/

    /// <summary>
    /// Quita caracteres extraños de la cadena, y la convierte a mayúsculas si se desea.
    /// Caracteres que remueve: Espacio en blanco, apóstrofe.
    /// Caracteres que reemplaza: Letras con tilde, ñ.
    /// </summary>
    /// <param name="strCadena">Cadena a aplicar el formato</param>
    /// <param name="boolMayusculas">Indica si se convierte a mayúsculas también</param>
    /// <returns>Cadena con el formato nuevo</returns>
    public string formatoCadena(string strCadena, bool boolMayusculas)
    {
        string strResultado;
        strResultado = strCadena.Trim().Replace("'", "");
        strResultado = strResultado.Replace('Á', 'A').Replace('É', 'E').Replace('Í', 'I').Replace('Ó', 'O').Replace('Ú', 'U');
        strResultado = strResultado.Replace('á', 'a').Replace('é', 'e').Replace('í', 'i').Replace('ó', 'o').Replace('ú', 'u');
        strResultado = strResultado.Replace('Ñ', 'N');
        strResultado = strResultado.Replace('ñ', 'n');
        if (boolMayusculas)
            strResultado = strResultado.ToUpper();
        return strResultado;
    }

    /// <summary>
    /// Asigna el maximo tiempo de espera para las solicitudes de las páginas.
    /// </summary>
    /// <param name="tiempo">El tiempo máximo de espera (En segundos).</param>
    public void asignarMaximoTiempoEspera(int tiempo)
    {
        smgPrincipal.AsyncPostBackTimeout = tiempo;
        Server.ScriptTimeout = tiempo;
    }

    /// <summary>
    /// Carga los valores para iniciar la conexion con la base de datos
    /// </summary>
    public void inicializarValoresConexion()
    {
        strPath = Server.MapPath("") + "\\App_Data\\promerica.cef";
        strPalabra = "Prmer!c@";
        strConexion = System.Configuration.ConfigurationManager.AppSettings["conexion"];
        strConexionCbs = System.Configuration.ConfigurationManager.AppSettings["conexion_CBS"];
    }

    /// <summary>
    /// Indica si el usuario ha entrado al sistema correctamente
    /// </summary>
    /// <returns>true si ingresó correctamente, false si no</returns>
    public bool usuarioAutenticado()
    {
        if (Session["usuario"] == null || Session["pwd"] == null)
            return false;
        strUsuario = Session["usuario"].ToString();
        strPassword = Session["pwd"].ToString();
        return true;
    }

    public void verificarUsuarioAutenticado()
    {
        if (!usuarioAutenticado())
            Response.Redirect(strPaginaInicio);
    }

    /// <summary>
    /// Autentica el usuario y crea el menú, haciendo todas las validaciones necesarias.
    /// </summary>
    /// <param name="strUsuario">Nombre del usuario</param>
    /// <param name="strPassword">Contraseña del usuario</param>
    /// <returns>true en exito, false si no</returns>
    public bool autenticarUsuario(string strUsuario, string strPassword)
    {
        this.strUsuario = strUsuario;
        this.strPassword = strPassword;

        string strCadena = "";
        string strPermisos = "";
        string strPaginas = "";

        strUsuario = formatoCadena(strUsuario, true);
        strPassword = formatoCadena(strPassword, false);

        inicializarValoresConexion();
        objOracle = new cloudconnect20.Oracle();
        Session["ses_conexion"] = strCadena;
        Session["ses_permisos"] = strPermisos;
        Session["ses_menu"] = strPaginas;
        Session["usuario"] = this.strUsuario.Replace("PROMERICA\\", "");
        Session["usuario_dominio"] = this.strUsuario;
        Session["clave"] = strPassword;
        Session["palabra"] = strPalabra;
        //Session["servidor"] = strServidor;
        Session["path"] = strPath;

        if (!objOracle.CargarConexion(strPath, strPalabra, strConexion, "", "", strUsuario, strPassword))
        {
            if (objOracle.strError.Length > 150)
                strMensajeErrorModal = "No se pudo autenticar el usuario: " + objOracle.strError.Substring(0, 150);
            else
                strMensajeErrorModal = "No se pudo autenticar el usuario: " + objOracle.strError;

            if (objOracle.strError.Contains("ORA-01017: invalid username/password"))
                strMensajeErrorModal = "No se pudo autenticar el usuario: <br> <span style='margin-left:20px;'>Usuario o Clave incorrecta.</span>";

            if (objOracle.strError.Contains("ORA-28000: the account is locked"))
                strMensajeErrorModal = "No se pudo autenticar el usuario: <br> <span style='margin-left:20px;'>Usuario Bloqueado, ha intentado demasiadas veces una clave incorrecta.</span>";

            if (objOracle.strError.Contains("ORA-28001: the password has expired"))
                strMensajeErrorModal = "No se pudo autenticar el usuario: <br> <span style='margin-left:20px;'>Su clave ha expirado, debe cambiarla para poder usar el sistema.</span>";

            return false;
        }
        controllerMenu objControllerMenu = new controllerMenu(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());

        //validar en RRHH_USUARIO
        if (!objControllerMenu.validateRRHH(Session["usuario"].ToString()))
        {
            strMensajeErrorModal = objControllerMenu.strReturnMessage;
            return false;
        }

        //validar en PA.USUAIRO
        if (!objControllerMenu.validatePA(Session["usuario"].ToString()))
        {
            strMensajeErrorModal = objControllerMenu.strReturnMessage;
            return false;
        }

        //obtener el menu para el usuario
        if (!objControllerMenu.getMenuItems(Session["usuario"].ToString(), System.Configuration.ConfigurationManager.AppSettings["NoSistema"].ToString()))
        {
            strMensajeErrorModal = objControllerMenu.strReturnMessage;
            return false;
        }

        if (!objControllerMenu.generarMenuItemsHorizontales_boostrap(objControllerMenu.dsReturn))
        {
            strMensajeErrorModal = objControllerMenu.strReturnMessage;
            return false;
        }

        Session["strScriptMenuHorizontal"] = objControllerMenu.strReturnMessage;
        return true;
    }

    /// <summary>
    /// Devuelve el objeto que realiza la conexión con la base de datos ORACLE de CBS, para realizar operaciones
    /// </summary>
    /// <returns>Objeto que realiza operaciones en la base de datos de CBS</returns>
    /*public cloudconnect20.Oracle obtenerAccesoCBS()
    {
        if (!usuarioAutenticado())
        {
            strMensajeErrorModal = "Usuario no autenticado.";
            return null;
        }

        inicializarValoresConexion();
        objOracleCbs = new cloudconnect20.Oracle();

        if (!objOracleCbs.CargarConexion(strPath, strPalabra, strConexionCbs, "", "", "", ""))
        {
            strMensajeErrorModal = "No se pudo iniciar la conexion al sistema CBS: " + objOracleCbs.strError;
            return null;
        }

        return objOracleCbs;
    }*/

    /// <summary>
    /// Limpia las variables de sesión, excepto las necesarias.
    /// </summary>
    public void limpiarSesion()
    {
        // Guardar los datos necesarios
        string strCadena = Session["ses_conexion"].ToString();
        string strPermisos = Session["ses_permisos"].ToString();
        string strPaginas = Session["ses_menu"].ToString();
        string strUsuario = Session["usuario"].ToString();
        string strPassword = Session["pwd"].ToString();
        string strPath = Session["path"].ToString();

        // Limpiar la sesion
        Session.Clear();

        // Volver a asignar los datos
        Session["ses_conexion"] = strCadena;
        Session["ses_permisos"] = strPermisos;
        Session["ses_menu"] = strPaginas;
        Session["usuario"] = strUsuario;
        Session["pwd"] = strPassword;
        Session["path"] = strPath;
    }

    /// <summary>
    /// Maneja el evento AsyncPostBackError del ScriptManager.
    /// </summary>
    /// <param name="sender">El control que lanza el evento.</param>
    /// <param name="e">La instancia <see cref="System.Web.UI.AsyncPostBackErrorEventArgs"/> que contiene los datos del evento.</param>
    protected void smgPrincipal_AsyncPostBackError(object sender, AsyncPostBackErrorEventArgs e)
    {
        smgPrincipal.AsyncPostBackErrorMessage = e.Exception.Message + " En " + e.Exception.Source + " " + e.Exception.StackTrace;
    }

   
}
