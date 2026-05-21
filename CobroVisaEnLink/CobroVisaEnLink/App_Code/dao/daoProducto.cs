using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

/// <summary>
/// Summary description for daoProducto
/// </summary>
public class daoProducto : clsDataLayer
{
    string strMensaje;
    private DataSet dsDaoProducto;
    public daoProducto(string strUsuario, string strPassword, string strPath) 
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
    public DataSet dsProducto
    {
        get { return this.dsDaoProducto; }
    }
    public bool getMontoPR(string num_cuenta)
    {
        try
        {
            string strSQL = " SELECT ROUND(MON_CANCELACION * MON_TASA,2) AS VALOR "
                          + " FROM BO.PKG_SCL.PkgScl_GetInfoPR("+ num_cuenta + ") "
                            ;
            Logger.LogInfo("getMontoPR strSQL " + strSQL);
            if (objOracle.ConsultarDatos(strSQL, "datos_producto") == null)
            {
                objOracle.TransaccionRollback();
                throw new Exception("Error al consultar monto del link. F/M: (getMontoPR): " + objOracle.strError);
            }
            this.dsDaoProducto = objOracle.dsDatos;
            Logger.LogInfo("getMontoPR " + dsDaoProducto);
            return true;
        }
        catch (Exception ex)
        {
            this.strMensaje = ex.Message;
            Logger.LogError(ex);
            return false;
        }
    }
    public bool getMontoTC(string num_cuenta)
    {
        try
        {
            string strSQL = " SELECT "
                          + "      ( NVL(MON_DEUDA_QTZ,0) "
                          + "      + NVL(MON_EXTRAF_QTZ,0) "
                          + "      + NVL(MON_RESERPRIN_QTZ,0) "
                          + "      + NVL(MON_RESERINT_QTZ,0)  "
                          + "      + NVL(MON_OTROS_QTZ,0)) "
                          + "  + ROUND((  NVL(MON_DEUDA_USD,0) "
                          + "           + NVL(MON_EXTRAF_USD,0)  "
                          + "           + NVL(MON_RESERPRIN_USD,0)   "
                          + "           + NVL(MON_RESERINT_USD,0) "
                          + "           + NVL(MON_OTROS_USD,0) ) * MON_TASA, 2) "
                          + "       AS VALOR "
                          + " FROM BO.PKG_SCL.PkgScl_GetInfoTC(" + num_cuenta + ") "
                            ;
            if (objOracle.ConsultarDatos(strSQL, "datos_producto") == null)
            {
                objOracle.TransaccionRollback();
                throw new Exception("Error al consultar monto del link. F/M: (getMontoTC): " + objOracle.strError);
            }
            this.dsDaoProducto = objOracle.dsDatos;
            return true;
        }
        catch (Exception ex)
        {
            this.strMensaje = ex.Message;
            return false;
        }
    }
    public bool getExisteCta(string num_cta) // SE MODIFICA EL PROCEDIMIENTO A NIVEL DE SQL 
    {
        try
        {
            string strSQL = "SELECT c.codigo_cliente COD_CLIENTE,"
                           + "      p.nombre         NOM_CLIENTE"
                           + " FROM pr_creditos@tc_cbs c "
                           + " JOIN personas p ON p.cod_persona = c.codigo_cliente"
                           + " WHERE c.estado in ('D','I','V') "
                           + " AND c.no_credito = " + num_cta
                           + " AND ROWNUM = 1";

            if (objOracle.ConsultarDatos(strSQL, "datos_cta") == null)
            {
                throw new Exception("Error al consultar Datos del cliente. F/M: (getExisteCta): " + objOracle.strError);
            }
            else if (objOracle.dsDatos.Tables["datos_cta"].Rows.Count == 0) //verifico que traiga informacion de PR
            {
                string strSQL2 = "SELECT c.COD_CLIENTE,"
                               + "       pe.nombre NOM_CLIENTE"
                               + " FROM tc_cuenta_credito c "
                               + "  JOIN tc_prod_emis_tjt p ON p.cod_emisor = c.cod_emisor"
                               + "                          AND p.cod_prod_emisor = c.cod_prod_emisor"
                               + "                          AND p.cod_empresa = c.cod_empresa"
                               + "                          AND p.cod_marca not like '%DEB%'"
                               + "  JOIN personas pe ON pe.cod_persona = c.cod_cliente"
                               + " WHERE c.ind_estado_cta IN ('A', 'P', 'V')"
                               + "   AND c.num_cta_credito = " + num_cta
                               + "   AND ROWNUM = 1";

                if (objOracle.ConsultarDatos(strSQL2, "datos_cta") == null)
                {
                    throw new Exception("Error al consultar Datos. F/M: (getExisteCta): " + objOracle.strError);
                }
                else if (objOracle.dsDatos.Tables["datos_cta"].Rows.Count == 0) // verifico si existe en TC
                    throw new Exception("ERROR: La cuenta brindada no existe. F/M: (getExisteCta): ");
            }
            this.dsDaoProducto = objOracle.dsDatos;
            return true;
        }
        catch (Exception ex)
        {
            this.strMensaje = ex.Message;
            return false;
        }
    }

    public bool GetClienteListaNegra(string codEmpresa, string codCliente)
    {
        try
        {

            string strMsSQL = "SELECT COUNT(*) EXISTE"
                + " FROM BO.BO_LISTA_EXCLUSION_PAGOSTD blep "
                + " WHERE blep.COD_PERSONA = '" + codCliente + "' "
                + " AND blep.IND_ESTADO = 'A'";


            if (objOracle.ConsultarDatos(strMsSQL, "lista_negra") == null)
                throw new Exception("Error al obtener los links largos en BD. F/M (obtieneLinks):" + objOracle.strError);


            if (objOracle.strError != null && objOracle.strError.Trim().Length > 0)
                throw new Exception("Error: F/M: (" + objOracle.strError + "): ");

            this.dsDaoProducto = objOracle.dsDatos; //objOracle.dsDatos;

            var numero = string.IsNullOrEmpty(objOracle.dsDatos.Tables["lista_negra"].Rows[0]["EXISTE"].ToString()) ? "-1" : objOracle.dsDatos.Tables["lista_negra"].Rows[0]["EXISTE"].ToString();

            var conteo = int.Parse(numero);

            if (conteo > 0)
            {
                //throw new Exception("(El cliente debe realizar su pago por otro medio)");
                throw new Exception("(Tu transacción no pudo ser procesada debido a que el " +
                    "método de pago utilizado fue rechazado. Por favor, acércate a una agencia " +
                    "para realizar el pago.)");
            }

            return true;
        }
        catch (Exception ex)
        {
            strMensaje = ex.Message;
            return false;
        }
    }
}