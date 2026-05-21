using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for controllerProducto
/// </summary>
public class controllerProducto : clsController
{
    daoProducto objDaoProducto;
    public controllerProducto(string strUsuario, string strPassword, string strPath) 
        : base(strUsuario, strPassword, strPath)
    {
        //
        // TODO: Add constructor logic here
        //
        objDaoProducto = new daoProducto(strUsuario, strPassword, strPath);
    }

    public bool getMontoPR(string num_cuenta)
    {
        try
        {
            if (!objDaoProducto.getMontoPR(num_cuenta))
                throw new Exception(objDaoProducto.strMensajeRetorno);

            dsReturn = objDaoProducto.dsProducto;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            Logger.LogError(ex);
            return false;
        }
    }
    public bool getMontoTC(string num_cuenta)
    {
        try
        {
            if (!objDaoProducto.getMontoTC(num_cuenta))
                throw new Exception(objDaoProducto.strMensajeRetorno);

            dsReturn = objDaoProducto.dsProducto;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
    public bool getExisteCta(string num_cta)
    {
        try
        {
            if (!objDaoProducto.getExisteCta(num_cta))
                throw new Exception(objDaoProducto.strMensajeRetorno);

            dsReturn = objDaoProducto.dsProducto;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }

    public bool GetClienteListaNegra(string codEmpresa, string codCliente)
    {
        try
        {
            if (!objDaoProducto.GetClienteListaNegra(codEmpresa, codCliente))
                throw new Exception(objDaoProducto.strMensajeRetorno);

            dsReturn = objDaoProducto.dsProducto;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }

    }
}