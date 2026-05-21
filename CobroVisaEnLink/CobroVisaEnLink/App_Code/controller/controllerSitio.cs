using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Descripción breve de controllerSitio
/// </summary>
public class controllerSitio : clsController
{
    daoSitio objDaoSitio;
    public controllerSitio (string strUsuario, string strPassword, string strPath)
        : base(strUsuario, strPassword, strPath)
    {
        objDaoSitio = new daoSitio(strUsuario, strPassword, strPath);
    }
    public bool getParametros()
    {
        try
        {
            if (!objDaoSitio.getParametros())
                throw new Exception(objDaoSitio.strMensajeRetorno);

            dsReturn = objDaoSitio.dsSitio;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
    public bool obtengoCodigoInterno()
    {
        try
        {
            if (!objDaoSitio.obtengoCodigoInterno())
                throw new Exception(objDaoSitio.strMensajeRetorno);

            dsReturn = objDaoSitio.dsSitio;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
    public bool updateParametros(clsParametros objParametros)
    {
        try
        {
            if (!objDaoSitio.updateParametros(objParametros))
                throw new Exception(objDaoSitio.strMensajeRetorno);
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
    public bool insertToken(string strToken)
    {
        try
        {
            if (!objDaoSitio.insertToken(strToken))
                throw new Exception(objDaoSitio.strMensajeRetorno);
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
    public bool getTokenInterno()
    {
        try
        {
            if (!objDaoSitio.getTokenInterno())
                throw new Exception(objDaoSitio.strMensajeRetorno);
            dsReturn = objDaoSitio.dsSitio;

            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
    public bool registraBitacora(clsBitacora objRegistrar)
    {
        try
        {
            if (!objDaoSitio.registraBitacora(objRegistrar))
                throw new Exception(objDaoSitio.strMensajeRetorno);
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
    public bool registraBitacoraCore(clsBitCore objRegistrar)
    {
        try
        {
            if (!objDaoSitio.registraBitacoraCore(objRegistrar))
                throw new Exception(objDaoSitio.strMensajeRetorno);
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
}