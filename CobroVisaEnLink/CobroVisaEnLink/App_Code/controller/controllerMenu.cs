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
/// Summary description for controllerMenu
/// </summary>
public class controllerMenu : clsController
{
    daoMenu objDaoMenu;
    //atributos propios para la generación de menús
    string strScript;
    int[] vctNiveles = new int[100];
    DataTable dtTabla = new DataTable();
    DataSet dsMenuItems = new DataSet();
    //int intItemSeleccionado;

    public controllerMenu(string strUsuario, string strPassword, string strPath)
        : base(strUsuario, strPassword, strPath)
	{
		//
		// TODO: Add constructor logic here
		//
        objDaoMenu = new daoMenu(strUsuario, strPassword, strPath);
	}

    public bool getMenuItems(string strUsuario, string strCodSistema)
    {
        try
        {
            if (!objDaoMenu.getMenuItems(strUsuario, strCodSistema))
                throw new Exception(objDaoMenu.strReturnMessage);
            if (objDaoMenu.dsReturn.Tables["f1_menu"].Rows.Count < 1)
                throw new Exception("No tiene permisos para este sistema");
            dsReturn = objDaoMenu.dsReturn;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }

    #region " Métodos para la generación de los menús horizontales boostrap"

    public bool generarMenuItemsHorizontales_boostrap(DataSet dsMenuItemsParametro)
    {
        try
        {
            dsMenuItems = dsMenuItemsParametro;
            string strCriterio = " cod_item_padre is null and visible = 'S' ";
            DataRow[] drRowsFound;
            drRowsFound = dsMenuItems.Tables["f1_menu"].Select(strCriterio);
            int intContItemsPrincipal;
            for (intContItemsPrincipal = 0; intContItemsPrincipal < drRowsFound.Length; intContItemsPrincipal++)
            {
                

                if (drRowsFound[intContItemsPrincipal]["PATH"].ToString() != "")
                {
                    strScript += "<li><a href=\"";
                    strScript += drRowsFound[intContItemsPrincipal]["PATH"].ToString();
                    strScript += "\" class=\"text-capitalize\" role=\"button\" >" + drRowsFound[intContItemsPrincipal]["nombre"].ToString() + "</a>";
                }
                else
                {
                    strScript += "<li class=\"dropdown\"><a href=\"#";                    
                    strScript += "\" class=\"dropdown-toggle text-capitalize\" data-toggle=\"dropdown\" role=\"button\" aria-haspopup=\"true\" aria-expanded=\"false\">" + drRowsFound[intContItemsPrincipal]["nombre"].ToString() + "<span class=\"caret\"></span></a>";
                }

                
                generarSubItems_boostrap(Int32.Parse(drRowsFound[intContItemsPrincipal]["COD_MENU_ITEM"].ToString()));
                strScript += "</li>";
            }

            strScript += "<li><a href=\"Default.aspx\">Salir</a></li>";
            strScript = "<ul id=\"dvMenuBar\" class=\"nav navbar-nav\" runat=\"server\">" + strScript + "</ul>";
            strReturnMessage = strScript.ToString();
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }

    public void generarSubItems_boostrap(int intCodPadre)
    {
        try
        {
            string strCriterio = " visible = 'S' and cod_item_padre = " + intCodPadre.ToString();
            DataRow[] drRowsFound;
            drRowsFound = dsMenuItems.Tables["f1_menu"].Select(strCriterio);
            int i = 0;
            if (drRowsFound.Length > 0)
            {
                strScript += "<ul class=\"dropdown-menu\">";
                for (i = 0; i < drRowsFound.Length; i++)
                {

                    strScript += "<li><a class=\"text-capitalize\" href=\"";
                    //drRowsFound[i]["PATH"].ToString()
                    if (drRowsFound[i]["PATH"].ToString() != "")
                        strScript += drRowsFound[i]["PATH"].ToString();
                    else
                        strScript += "#";
                    strScript += "\">" + drRowsFound[i]["nombre"].ToString() + "</a>";
                    generarSubItems_boostrap(Int32.Parse(drRowsFound[i]["COD_MENU_ITEM"].ToString()));
                    strScript += "</li>";
                }                
                strScript += "</ul>";
            }
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
        }
    }
    #endregion

    public bool validateRRHH(string strUsuario)
    {
        try
        {
            if (!objDaoMenu.validateRRHH(strUsuario))
                throw new Exception(objDaoMenu.strReturnMessage);
            if (objDaoMenu.dsReturn.Tables["rrhh_usuario"].Rows.Count < 1)
                throw new Exception("No se encontro el usuario en RRHH");
            if (objDaoMenu.dsReturn.Tables["rrhh_usuario"].Rows[0]["ACTIVO"].ToString() != "S")
                throw new Exception(" Usuario inactivo en RRHH.");
            //            dsReturn = objDaoMenu.dsReturn;
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
            if (!objDaoMenu.validatePA(strUsuario))
                throw new Exception(objDaoMenu.strReturnMessage);
            if (objDaoMenu.dsReturn.Tables["pa_usuario"].Rows.Count < 1)
                throw new Exception("No se encontro el usuario en PA.USUARIO");
            if (objDaoMenu.dsReturn.Tables["pa_usuario"].Rows[0]["EST_ACTIVO"].ToString() != "S")
                throw new Exception(" Usuario inactivo en PA.");
            //            dsReturn = objDaoMenu.dsReturn;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }

    public bool verificarMenuItem(string strUsuario, string mitems)
    {
        try
        {
            if (!objDaoMenu.verificarRol(strUsuario))
                throw new Exception(objDaoMenu.strMensajeRetorno);
            dsReturn = objDaoMenu.dsReturn;

            foreach (var row in dsReturn.Tables["verificarRol"].Rows)
            {
                DataRow rw = row as DataRow;
                string permiso = Convert.ToString(rw[2]);

                string[] items_buscados = mitems.Split(',');
                foreach (string item in items_buscados)
                {
                    if (permiso == item)
                        return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {

            string msg = ex.Message;
            return false;
        }
    }
}
