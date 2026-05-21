using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Descripción breve de controllerTransacciones
/// </summary>
public class controllerTransacciones : clsController
{
    daoTransacciones objDaoTransacciones;
    public controllerTransacciones(string strUsuario, string strPassword, string strPath)
        : base(strUsuario, strPassword, strPath)
    {
        objDaoTransacciones = new daoTransacciones(strUsuario, strPassword, strPath);
    }

    public bool getTransacciones(int start, int length, string columnaOrden, string dirOrden, string busqueda, object[] Columnas)
    {
        try
        {
            if (!objDaoTransacciones.getTransacciones(start, length, columnaOrden, dirOrden, busqueda, Columnas))
                throw new Exception(objDaoTransacciones.strMensajeRetorno);

            dsReturn = objDaoTransacciones.dsTransaccion;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
}