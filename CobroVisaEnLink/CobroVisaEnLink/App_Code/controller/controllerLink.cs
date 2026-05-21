using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Descripción breve de controllerCliente
/// </summary>
public class controllerLink : clsController
{
    daoLink objDaoLink;
    public controllerLink(string strUsuario, string strPassword, string strPath)
        : base(strUsuario, strPassword, strPath)
    {
        objDaoLink = new daoLink(strUsuario, strPassword, strPath);
    }
    public bool insertLink(clsLink objLink)
    {
        try
        {
            if (!objDaoLink.insertLink(objLink))
                throw new Exception(objDaoLink.strMensajeRetorno);
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
    public bool aplicaPagoPR(clsPago objPago, string strMoneda)
    {
        try
        {
            if (!objDaoLink.aplicaPagoPR(objPago, strMoneda))
                throw new Exception(objDaoLink.strMensajeRetorno);
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
    public bool aplicaPagoTC(clsPago objPago, string strMoneda)
    {
        try
        {
            if (!objDaoLink.aplicaPagoTC(objPago, strMoneda))
                throw new Exception(objDaoLink.strMensajeRetorno);
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
    public bool getParametro(string cod_link)
    {
        try
        {
            if (!objDaoLink.getParametro(cod_link))
                throw new Exception(objDaoLink.strMensajeRetorno);

            dsReturn = objDaoLink.dsLinks;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
    public bool getLinkCta(string strNumCta)
    {
        try
        {
            if (!objDaoLink.getLinkCta(strNumCta))
                throw new Exception(objDaoLink.strMensajeRetorno);

            dsReturn = objDaoLink.dsLinks;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
    public bool getLinkParametro(string strCodParametro)
    {
        try
        {
            if (!objDaoLink.getLinkParametro(strCodParametro))
                throw new Exception(objDaoLink.strMensajeRetorno);

            dsReturn = objDaoLink.dsLinks;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
    public bool getLinks()
    {
        try
        {
            if (!objDaoLink.getLinks())
                throw new Exception(objDaoLink.strMensajeRetorno);

            dsReturn = objDaoLink.dsLinks;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
    public bool getLinks(int start, int length, string columnaOrden, string dirOrden, string busqueda, object[] Columnas)
    {
        try
        {
            if (!objDaoLink.getLinks(start, length, columnaOrden, dirOrden, busqueda, Columnas))
                throw new Exception(objDaoLink.strMensajeRetorno);

            dsReturn = objDaoLink.dsLinks;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
    public bool getLinksVerifica(int start, int length, string columnaOrden, string dirOrden, string busqueda, object[] Columnas)
    {
        try
        {
            if (!objDaoLink.getLinksVerifica(start, length, columnaOrden, dirOrden, busqueda, Columnas))
                throw new Exception(objDaoLink.strMensajeRetorno);

            dsReturn = objDaoLink.dsLinks;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
    public bool notificaSMS(clsSMS objParam)
    {
        try
        {
            if (!objDaoLink.notificaSMS(objParam))
                throw new Exception(objDaoLink.strMensajeRetorno);
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
    public bool notificaMail(clsMail objParam)
    {
        try
        {
            if (!objDaoLink.notificaMail(objParam))
                throw new Exception(objDaoLink.strMensajeRetorno);
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
    public bool updateEstadoLink(string strCodParametro)
    {
        try
        {
            if (!objDaoLink.updateEstadoLink(strCodParametro))
                throw new Exception(objDaoLink.strMensajeRetorno);
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
    public bool obtieneLinks()
    {
        try
        {
            if (!objDaoLink.obtieneLinks())
                throw new Exception(objDaoLink.strMensajeRetorno);

            dsReturn = objDaoLink.dsLinks;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }

    //25Nov2024.Ini
    public bool obtieneLinks_NumRow(Int16 int16NumRow)
    {
        try
        {
            if (!objDaoLink.obtieneLinks_NumRow(int16NumRow))
                throw new Exception(objDaoLink.strMensajeRetorno);

            dsReturn = objDaoLink.dsLinks;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
    //25Nov2024.Fin

    public bool existenPendientes()
    {
        try
        {
            if (!objDaoLink.existenPendientes())
                throw new Exception(objDaoLink.strMensajeRetorno);

            dsReturn = objDaoLink.dsLinks;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
    public bool existePeriferico(int intPeriferico)
    {
        try
        {
            if (!objDaoLink.existePeriferico(intPeriferico))
                throw new Exception(objDaoLink.strMensajeRetorno);

            dsReturn = objDaoLink.dsLinks;
            return true;
        }
        catch (Exception ex)
        {
            strReturnMessage = ex.Message;
            return false;
        }
    }
    public bool updateURLCorto(decimal numConsecutivo, string strURLCorto)
    {
        try
        {
            objDaoLink.iniciarTransaccion();

            if (!objDaoLink.updateURLCorto(numConsecutivo,strURLCorto))
                throw new Exception(objDaoLink.strMensajeRetorno);

            objDaoLink.commitTransaccion();

            return true;
        }
        catch (Exception ex)
        {
            objDaoLink.rollBackTransaccion();
            strReturnMessage = ex.Message;
            return false;
        }
    }
    public bool registraBitacoraBD(string strUrlLargo, string strUrlCorto, int intPeriferico)
    {
        try
        {
            objDaoLink.iniciarTransaccion();

            if (!objDaoLink.registraBitacoraBD(strUrlLargo, strUrlCorto, intPeriferico))
                throw new Exception(objDaoLink.strMensajeRetorno);

            objDaoLink.commitTransaccion();
            return true;
        }
        catch (Exception ex)
        {
            objDaoLink.rollBackTransaccion();
            strReturnMessage = ex.Message;
            return false;
        }
    }
}