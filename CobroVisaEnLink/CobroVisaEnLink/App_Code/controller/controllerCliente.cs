using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Descripción breve de controllerCliente
/// </summary>
public class controllerCliente : clsController
{
    daoCliente objDaoCliente;
    public controllerCliente(string strUsuario, string strPassword, string strPath)
        : base(strUsuario, strPassword, strPath)
    {
        objDaoCliente = new daoCliente(strUsuario, strPassword, strPath);
    }

    public bool getCliente_cta(string num_cta)
    {
        try
        {
            if (!objDaoCliente.getCliente_cta(num_cta))
                throw new Exception(objDaoCliente.strMensajeRetorno);

            dsReturn = objDaoCliente.dsCliente;
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
            if (!objDaoCliente.GetClienteListaNegra(codEmpresa, codCliente))
                throw new Exception(objDaoCliente.strMensajeRetorno);

            dsReturn = objDaoCliente.dsCliente;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }

    }

    public bool getTipoPrestamo(string num_cta)
    {
        try
        {
            if (!objDaoCliente.getTipoPrestamo(num_cta))
                throw new Exception(objDaoCliente.strMensajeRetorno);

            dsReturn = objDaoCliente.dsCliente;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
    public bool getCorreoCliente(string nombre)
    {
        try
        {
            if (!objDaoCliente.getCorreoCliente(nombre))
                throw new Exception(objDaoCliente.strMensajeRetorno);

            dsReturn = objDaoCliente.dsCliente;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
    public bool getTelefonoCliente(string nombre)
    {
        try
        {
            if (!objDaoCliente.getTelefonoCliente(nombre))
                throw new Exception(objDaoCliente.strMensajeRetorno);

            dsReturn = objDaoCliente.dsCliente;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
    public bool getCuentas(string cod_cliente)
    {
        try
        {
            if (!objDaoCliente.getCuentas(cod_cliente))
                throw new Exception(objDaoCliente.strMensajeRetorno);

            dsReturn = objDaoCliente.dsCliente;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }

}