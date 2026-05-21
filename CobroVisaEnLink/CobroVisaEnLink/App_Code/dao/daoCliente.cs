//using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Web;

public class clsCliente
{
    public string cod_cli_aci { get; set; }
    public string cod_cli { get; set; }
    public string cod_cli_tr { get; set; }
    public string nom_cliente { get; set; }
    public string cod_transpo { get; set; }
    public string cod_sucursal { get; set; }
    public string nom_sucursal { get; set; }
    public string num_cta { get; set; }
    public string ind_estado { get; set; }
    public string ind_bloqueo_ive { get; set; }

    public clsCliente()
    {

    }
}

/// <summary>
/// Descripción breve de daoCliente
/// </summary>
public class daoCliente : clsDataLayer
{
    string strMensaje;
    private DataSet dsDaoCliente;

    public daoCliente(string strUsuario, string strPassword, string strPath)
        : base(strUsuario, strPassword, strPath)
    {
        //
        // TODO: Agregar aquí la lógica del constructor
        //
    }
    public string strMensajeRetorno
    {
        get
        {
            return this.strMensaje;
        }
    }
    public DataSet dsCliente
    {
        get { return this.dsDaoCliente; }
    }
    public bool getCliente_cta(string num_cta) // SE MODIFICA EL PROCEDIMIENTO A NIVEL DE SQL 
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

            if (objOracle.ConsultarDatos(strSQL, "datos_cliente") == null)
            {
                throw new Exception("Error al consultar Datos del cliente. F/M: (getCliente): " + objOracle.strError);
            }
            else if (objOracle.dsDatos.Tables["datos_cliente"].Rows.Count == 0) //verifico que traiga informacion de PR
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

                if (objOracle.ConsultarDatos(strSQL2, "datos_cliente") == null)
                {
                    throw new Exception("Error al consultar Datos del cliente. F/M: (getCliente_cta): " + objOracle.strError);
                }
                else if (objOracle.dsDatos.Tables["datos_cliente"].Rows.Count == 0) // verifico si existe en TC
                    throw new Exception("ERROR: La cuenta brindada no existe. F/M: (getCliente_cta): ");
            }
            this.dsDaoCliente = objOracle.dsDatos;
            return true;
        }
        catch (Exception ex)
        {
            this.strMensaje = ex.Message;
            return false;
        }
    }
    public bool getTipoPrestamo(string num_cta)
    {
        try
        {
            string strSQL
               = "SELECT p.no_credito NUM_CUENTA, "
                + "       T.CODIGO_MONEDA MONEDA "
                + "  FROM pr_creditos@tc_cbs P "
                + "  JOIN PR_TIPO_CREDITO T ON T.CODIGO_EMPRESA = P.CODIGO_EMPRESA "
                + "                        AND T.TIPO_CREDITO = P.TIPO_CREDITO"
                + " WHERE P.estado in ('D','I','V')"
                + "   AND P.NO_CREDITO = '" + num_cta + "'"
               ;
            if (objOracle.ConsultarDatos(strSQL, "prestamo") == null)
                throw new Exception("Error al consultar Datos del cliente. F/M: (getTipoPrestamo): " + objOracle.strError);

            this.dsDaoCliente = objOracle.dsDatos;
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
                throw new Exception("Error: F/M (GetClienteListaNegra): " + objOracle.strError);


            if (objOracle.strError != null && objOracle.strError.Trim().Length > 0)
                throw new Exception("Error: F/M (GetClienteListaNegra): " + objOracle.strError + ".");

            this.dsDaoCliente = objOracle.dsDatos; //objOracle.dsDatos;

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
    public bool getCorreoCliente(string cod_cliente)
    {
        try
        {
            string strSQL = "SELECT EMAIL_USUARIO || '@' ||EMAIL_SERVIDOR CORREO"
                          + "  FROM EMAIL_PERSONAS "
                          + " WHERE ES_DEFAULT='S'"
                          + " AND COD_PERSONA = " + cod_cliente;

            if (objOracle.ConsultarDatos(strSQL, "correo_cliente") == null)
            {
                throw new Exception("Error al obtener correo electrónico del cliente. F/M (getCorreoCliente):" + objOracle.strError);
            }
            else if (objOracle.dsDatos.Tables["correo_cliente"].Rows.Count == 0) //verifico que traiga informacion de correo
            {
                string strSQL2 = "SELECT EMAIL_USUARIO || '@' ||EMAIL_SERVIDOR CORREO"
                               + "  FROM EMAIL_PERSONAS "
                               + " WHERE COD_PERSONA = " + cod_cliente
                               + "   AND ROWNUM = 1";

                if (objOracle.ConsultarDatos(strSQL2, "correo_cliente") == null)
                    throw new Exception("Error al obtener correo electrónico del cliente. F/M (getCorreoCliente):" + objOracle.strError);
            }

            this.dsDaoCliente = objOracle.dsDatos;
            return true;
        }
        catch (Exception ex)
        {
            strMensaje = ex.Message;
            return false;
        }
    }
    public bool getTelefonoCliente(string cod_cliente)
    {
        try
        {
            string strSQL = "SELECT TRIM(NUM_TELEFONO) TELEFONO"
                          + " FROM TEL_PERSONAS "
                          + " WHERE ES_DEFAULT ='S'"
                          + "  AND COD_PERSONA = " + cod_cliente;

            if (objOracle.ConsultarDatos(strSQL, "tel_cliente") == null)
            {
                throw new Exception("Error al obtener teléfono del cliente. F/M (getTelefonoCliente):" + objOracle.strError);
            }
            else if (objOracle.dsDatos.Tables["tel_cliente"].Rows.Count == 0) //verifico que traiga informacion de telefono
            {
                string strSQL2 = "SELECT TRIM(NUM_TELEFONO) TELEFONO"
                               + " FROM TEL_PERSONAS "
                               + " WHERE COD_PERSONA = " + cod_cliente
                               + "   AND ROWNUM = 1 ";

                if (objOracle.ConsultarDatos(strSQL2, "tel_cliente") == null)
                    throw new Exception("Error al obtener teléfono del cliente. F/M (getTelefonoCliente):" + objOracle.strError);
            }

            this.dsDaoCliente = objOracle.dsDatos;
            return true;
        }
        catch (Exception ex)
        {
            strMensaje = ex.Message;
            return false;
        }
    }
    public bool getCuentas(string cod_cliente)// SE MODIFICA EL PROCEDIMIENTO A NIVEL DE SQL  
    {
        try
        {
            string strSQL = "SELECT p.no_credito NUM_CUENTA, "
                           + "       DECODE(p.estado, 'D', 'Vigente', "
                           + "                        'I', 'Castigado', "
                           + "                        'V', 'Vencido' ) ESTADO, "
                           + "       'Prestamo' TIPO  "
                           + "  FROM pr_creditos@tc_cbs P "
                           + "  JOIN PR_TIPO_CREDITO T ON T.CODIGO_EMPRESA = P.CODIGO_EMPRESA "
                           + "                        AND T.TIPO_CREDITO = P.TIPO_CREDITO"
                           + " WHERE P.estado in ('D','I','V') "
                           + "   AND P.codigo_cliente = " + cod_cliente
                           + " UNION "
                           + " SELECT c.num_cta_credito NUM_CUENTA,"
                           + "       DECODE(c.ind_estado_cta, 'A', 'Activa',"
                           + "                                'P', 'Proceso de Cancelacion',"
                           + "                                'V', 'Vencida' ) ESTADO,"
                           + "       'Tarjeta' TIPO "
                           + " FROM tc_cuenta_credito c"
                           + " JOIN tc_prod_emis_tjt p ON p.cod_emisor = c.cod_emisor"
                           + "                       AND p.cod_prod_emisor = c.cod_prod_emisor"
                           + "                       AND p.cod_empresa = c.cod_empresa"
                           + "                       AND p.cod_marca not like '%DEB%'"
                           + " WHERE c.ind_estado_cta IN ('A', 'P', 'V')"
                           + "  AND c.COD_GRUPO_CTACTE NOT IN (830,837) "
                          // + "  AND c.COD_PROD_EMISOR NOT IN (5191,5922,6040,6041,5901,3702,5903,3701,5902,5910,3700,6191,6102) "
                           + "  AND c.cod_cliente = " + cod_cliente
                           ;
            if (objOracle.ConsultarDatos(strSQL, "cuentas") == null)
                throw new Exception("Error al obtener las cuentas. F/M (getCuentas):" + objOracle.strError);
            this.dsDaoCliente = objOracle.dsDatos;
            return true;
        }
        catch (Exception ex)
        {
            strMensaje = ex.Message;
            return false;
        }
    }

}