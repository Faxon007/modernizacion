using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Descripción breve de controllerTransportadora
/// </summary>
public class controllerTransportadora : clsController
{
    daoTransportadora objDaoTransportadora;
    public controllerTransportadora(string strUsuario, string strPassword, string strPath)
        : base(strUsuario, strPassword, strPath)
    {
        objDaoTransportadora = new daoTransportadora(strUsuario, strPassword, strPath);
    }

    public bool insertTransportadora(clsTransportadora objTransportadora)
    {
        try
        {
            
            objDaoTransportadora.iniciarTransaccion();

            if (!objDaoTransportadora.insertUsuario(objTransportadora))
                throw new Exception(objDaoTransportadora.strMensajeRetorno);

            if (!objDaoTransportadora.insertTransportadora(objTransportadora))
                throw new Exception(objDaoTransportadora.strMensajeRetorno);
            
            objDaoTransportadora.commitTransaccion();
            return true;

        }
        catch (Exception ex)
        {
            objDaoTransportadora.rollBackTransaccion();
            strReturnMessage = ex.Message;
            return false;
        }
    }

    public bool updateTransportadora(clsTransportadora objTransportadora)
    {
        try
        {
            objDaoTransportadora.iniciarTransaccion();

            if (!objDaoTransportadora.updateUsuario(objTransportadora))
                throw new Exception(objDaoTransportadora.strMensajeRetorno);

            if (!objDaoTransportadora.updateTransportadora(objTransportadora))
                throw new Exception(objDaoTransportadora.strMensajeRetorno);

            objDaoTransportadora.commitTransaccion();

            return true;
        }
        catch (Exception ex)
        {
            objDaoTransportadora.rollBackTransaccion();
            strReturnMessage = ex.Message;
            return false;
        }
    }

    public bool getTransportadora(string tranportadora)
    {
        try
        {
            if (!objDaoTransportadora.getTransportadora(tranportadora))
                throw new Exception(objDaoTransportadora.strMensajeRetorno);

            dsReturn = objDaoTransportadora.dsTransportadora;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
    public bool getTransportadoras()
    {
        try
        {
            if (!objDaoTransportadora.getTransportadoras())
                throw new Exception(objDaoTransportadora.strMensajeRetorno);

            dsReturn = objDaoTransportadora.dsTransportadora;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
    public bool getTransportadorasDLL(string cod_cli_aci="")
    {
        try
        {
            if (!objDaoTransportadora.getTransportadorasDLL(cod_cli_aci))
                throw new Exception(objDaoTransportadora.strMensajeRetorno);

            dsReturn = objDaoTransportadora.dsTransportadora;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
}