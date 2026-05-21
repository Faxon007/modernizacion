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
/// Summary description for daoMenu
/// </summary>
public class daoMenu : clsDataLayer
{
    string strMensaje = "";
    public daoMenu(string strUsuario, string strPassword, string strPath)
        : base(strUsuario, strPassword, strPath)
	{
		//
		// TODO: Add constructor logic here
		//
	}

    public string strMensajeRetorno
    {
        get
        {
            return this.strMensaje;
        }
    }

    public bool getMenuItems(string strUsuario, string strCodSistema)
    {
        string strSQL = "select distinct m.cod_menu_item,m.nombre, m.path,  m.descripcion, m.cod_item_padre, m.visible  " +
          "from rrhh_menu_item m, rrhh_usuario_rol ur, rrhh_permiso_item pe " +
          "where ur.rol = pe.rol  " +
          "and pe.COD_MENU_ITEM = m.cod_menu_item  " +
          "and upper(ur.USUARIO) = upper('" + strUsuario.Trim().ToUpper() + "') " +
          "and m.cod_menu_item <> 0  " +
          "and m.sistema =   " + strCodSistema +
          " order by m.nombre asc ";
        
        try
        {            
            if (objOracle.ConsultarDatos(strSQL, "f1_menu") == null)
            {
                throw new Exception("Error al obtener items del menu. Error (getMenuItems): " + objOracle.strError);
            }
            this.dsReturn = objOracle.dsDatos;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }

    public bool validateRRHH(string strUsuario)
    {
        try
        {
            string strSQL = "SELECT activo "
                + " FROM RRHH_USUARIO "
                + "WHERE upper(USUARIO) = upper('" + strUsuario.Trim().ToUpper() + "') "
              ;

            if (objOracle.ConsultarDatos(strSQL, "rrhh_usuario") == null)
            {
                throw new Exception("Error al consultar rrhh_usuario. Error (validateRRHH): " + objOracle.strError);
            }
            this.dsReturn = objOracle.dsDatos;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }

    public bool validatePA(string strUsuario)
    {
        try
        {
            string strSQL = "SELECT est_activo"
                + " FROM USUARIOS "
                + "WHERE upper(cod_usuario) = upper('" + strUsuario.Trim().ToUpper() + "') "
              ;

            if (objOracle.ConsultarDatos(strSQL, "pa_usuario") == null)
            {
                throw new Exception("Error al consultar pa.usuarios. Error (validatePA): " + objOracle.strError);
            }
            this.dsReturn = objOracle.dsDatos;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }

    public bool verificarRol(string strUsuario)
    {
        try
        {
            string strCodSistema = System.Configuration.ConfigurationManager.AppSettings["NoSistema"].ToString();

            string strSQL = "SELECT RU.USUARIO, RUR.ROL, RPI.COD_MENU_ITEM, ACCION, SISTEMA  "
                + "FROM RRHH_USUARIO RU "
                + "LEFT JOIN RRHH_USUARIO_ROL RUR ON RUR.USUARIO =  RU.USUARIO "
                + "LEFT JOIN RRHH_ROL RO ON RO.ROL = RUR.ROL "
                + "LEFT JOIN RRHH_PERMISO_ITEM RPI ON RPI.ROL = RUR.ROL "
                + "WHERE  RO.SISTEMA  =" + strCodSistema
                + " AND UPPER(RU.USUARIO) = UPPER('" + strUsuario.Trim().ToUpper() + "')  "
            ;



            if (objOracle.ConsultarDatos(strSQL, "verificarRol") == null)
                throw new Exception("Error al verificar Rol");

            this.dsReturn = objOracle.dsDatos;
            return true;
        }
        catch (Exception ex)
        {

            string msg = ex.Message;
            return false;
        }
    }
}
