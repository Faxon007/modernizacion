using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Data.OracleClient;
public class clsLink
{
    public string num_cuenta { get; set; }
    public string tip_cuenta { get; set; }
    public string mon_cobro { get; set; }
    public string tip_pago { get; set; }
    public string es_default { get; set; }
    public string tip_envio { get; set; }
    public string num_telefono { get; set; }
    public string nom_correo { get; set; }
    public string tip_link { get; set; }
    public string dia_mes { get; set; }
    public string url_link { get; set; }
    public string url_corto { get; set; }
    public string ind_estado { get; set; }
    public string cod_sku { get; set; }

    public clsLink()
    {

    }
}
public class clsSMS
{
    public string telefono { get; set; }
    public string mensaje { get; set; }
    public string num_cta { get; set; }
}
public class clsMail
{
    public string mail { get; set; }
    public string asunto { get; set; }
    public string link { get; set; }

}
public class clsParametros
{
    public string FRE_REV_AUTORIZACION { get; set; }
    public string FRE_REV_HRS_REPETIR { get; set; }
    public string FRE_GEN_LINK { get; set; }
    public string FRE_GEN_HORA { get; set; }
    public string TC_TIP_TRANSAC { get; set; }
    public string TC_SUBTIP_TRANS { get; set; }
    public string PR_TIP_TRANSAC { get; set; }
    public string PR_SUBTIP_TRANS { get; set; }
    public string DES_TRANSACCION { get; set; }
    public string API_IMAGEN { get; set; }
    public string MSG_REMITENTE { get; set; }
    public string MSG_HEADER { get; set; }
    public string MSG_FOOTER { get; set; }
    public string MSG_SMS { get; set; }
    public string NUM_CTA_CONTA_DOL { get; set; }
    public string NUM_CTA_CONTA_QTZ { get; set; }
    public string COD_TIPO_TC { get; set; }
    public string COD_SUBTIPO_TC { get; set; }
    public string COD_TIPO_PR { get; set; }
    public string COD_SUBTIPO_PR { get; set; }
    public string COD_DEPARTAMENTO { get; set; }
    public string COD_DEPTO_PR { get; set; }
    public string COD_AGENCIA { get; set; }
}
public class clsPago
{
    public string num_cta { get; set; }
    public string cod_sku { get; set; }
    public string cod_link { get; set; }
    public string mon_pago { get; set; }
    public string aut_visa { get; set; }
}
public class daoLink : clsDataLayer
{
    string strMensaje;
    private DataSet dsDaoLink;
    public daoLink(string strUsuario, string strPassword, string strPath)
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
    public DataSet dsLinks
    {
        get { return this.dsDaoLink; }
    }
    public bool insertLink(clsLink objLink)
    {
        try
        {
            objOracle.EjecutarProcedimientoAlmacenado("BO.PKG_SCL.PkgScl_InsParamLink", "DATOS_LINK",
            new cloudconnect20.Oracle.ParametroOracle("p_NumCuenta", objLink.num_cuenta, OracleType.Number, ParameterDirection.Input),
            new cloudconnect20.Oracle.ParametroOracle("p_TipCuenta", objLink.tip_cuenta, OracleType.VarChar, ParameterDirection.Input),
            new cloudconnect20.Oracle.ParametroOracle("p_MonCobro", objLink.mon_cobro, OracleType.Number, ParameterDirection.Input),
            new cloudconnect20.Oracle.ParametroOracle("p_TipPago", objLink.tip_pago, OracleType.VarChar, ParameterDirection.Input),
            new cloudconnect20.Oracle.ParametroOracle("p_EsDefault", objLink.es_default ?? "NULL", OracleType.VarChar, ParameterDirection.Input),
            new cloudconnect20.Oracle.ParametroOracle("p_TipEnvio", objLink.tip_envio, OracleType.VarChar, ParameterDirection.Input),
            new cloudconnect20.Oracle.ParametroOracle("p_NumTelefono", objLink.num_telefono ?? "NULL", OracleType.VarChar, ParameterDirection.Input),
            new cloudconnect20.Oracle.ParametroOracle("p_NomCorreo", objLink.nom_correo ?? "NULL", OracleType.VarChar, ParameterDirection.Input),
            new cloudconnect20.Oracle.ParametroOracle("p_TipLink", objLink.tip_link, OracleType.VarChar, ParameterDirection.Input),
            new cloudconnect20.Oracle.ParametroOracle("p_DiaMes", objLink.dia_mes ?? "NULL", OracleType.VarChar, ParameterDirection.Input),
            new cloudconnect20.Oracle.ParametroOracle("p_IndEstado", objLink.ind_estado, OracleType.VarChar, ParameterDirection.Input),
            new cloudconnect20.Oracle.ParametroOracle("p_CodSku", objLink.cod_sku ?? "NULL", OracleType.VarChar, ParameterDirection.Input),
            new cloudconnect20.Oracle.ParametroOracle("p_Url", objLink.url_link ?? "NULL", OracleType.VarChar, ParameterDirection.Input),
            new cloudconnect20.Oracle.ParametroOracle("p_URLCorto", objLink.url_corto ?? "NULL", OracleType.VarChar, ParameterDirection.Input),
            new cloudconnect20.Oracle.ParametroOracle("P_MsgError", "NULL", OracleType.VarChar, ParameterDirection.Output)
            );

            if (objOracle.strError != null && objOracle.strError.Trim().Length > 0)
            {
                objOracle.TransaccionRollback();
                throw new Exception("Error al insertar Link. F/M (insertCliente): " + objOracle.strError);
            }

            objOracle.TransaccionCommit();

            return true;
        }
        catch (Exception ex)
        {
            this.strMensaje = ex.Message;
            return false;
        }
    }
    public bool aplicaPagoPR(clsPago objPago, string strMoneda)
    {
        try
        {
            //obtengo el codigo de cliente
            string strSQL = " SELECT CODIGO_CLIENTE "
                          + "  FROM PR_CREDITOS@TC_CBS "
                          + " WHERE NO_CREDITO = " + objPago.num_cta
            ;

            if (objOracle.ConsultarDatos(strSQL, "datos_pr") == null)
            {
                objOracle.TransaccionRollback();
                throw new Exception("Error al consultar dato del prestamo. F/M: (aplicaPagoPR): " + objOracle.strError);
            }

            string strCliente = Convert.ToString(objOracle.dsDatos.Tables["datos_pr"].Rows[0]["CODIGO_CLIENTE"]);

            //obtengo parametro de agencia
            string strSQLag = " SELECT COD_AGENCIA "
                            + " FROM BO.SCL_PARAM_SISTEMA "
                            + " WHERE ROWNUM = 1 "
            ;

            if (objOracle.ConsultarDatos(strSQLag, "datos_ag") == null)
            {
                objOracle.TransaccionRollback();
                throw new Exception("Error al obtener parámetro de la agencia. F/M: (aplicaPagoPR): " + objOracle.strError);
            }

            string strAgencia = Convert.ToString(objOracle.dsDatos.Tables["datos_ag"].Rows[0]["COD_AGENCIA"]);

            //Debo revisar si ya fue pagado
            string strSQLRev = " SELECT NUM_AUTORIZACION, "
                             + "        NUM_MOVIMIENTO "
                             + "   FROM BO.SCL_LISTADO_LINKS "
                             + "  WHERE COD_SKU = '" + objPago.cod_sku + "' "
                             + "    AND NUM_MOVIMIENTO IS NOT NULL ";

            if (objOracle.ConsultarDatos(strSQLRev, "revisa_pago") == null)
            {
                objOracle.TransaccionRollback();
                throw new Exception("Error al revisar información del pago. F/M: (aplicaPagoPR): " + objOracle.strError);
            }

            if (objOracle.dsDatos.Tables["revisa_pago"].Rows.Count == 0)
            {
                objOracle.TransaccionIniciar();

                //ejecuto el proceso de pago
                objOracle.EjecutarProcedimientoAlmacenado("BO.PKG_SCL.PkgScl_PagPres", "DATOS_PAGO",
                new cloudconnect20.Oracle.ParametroOracle("pCodEmpresa", "1", OracleType.VarChar, ParameterDirection.Input),
                new cloudconnect20.Oracle.ParametroOracle("pCodAgencia", strAgencia, OracleType.VarChar, ParameterDirection.Input),
                new cloudconnect20.Oracle.ParametroOracle("pCodCliente", strCliente, OracleType.Number, ParameterDirection.Input),
                new cloudconnect20.Oracle.ParametroOracle("pCodMonedaPago", strMoneda, OracleType.VarChar, ParameterDirection.Input),
                new cloudconnect20.Oracle.ParametroOracle("pQtzMonPago", objPago.mon_pago, OracleType.Number, ParameterDirection.Input),
                new cloudconnect20.Oracle.ParametroOracle("p_NumCredito", objPago.num_cta, OracleType.Number, ParameterDirection.Input),
                new cloudconnect20.Oracle.ParametroOracle("p_NumMov", "NULL", OracleType.Number, ParameterDirection.Output),
                new cloudconnect20.Oracle.ParametroOracle("pTasaAplicada", "NULL", OracleType.Number, ParameterDirection.Output),
                new cloudconnect20.Oracle.ParametroOracle("pMensajeUsuario", "NULL", OracleType.VarChar, ParameterDirection.InputOutput, 4000),
                new cloudconnect20.Oracle.ParametroOracle("pMensajeTecnico", "NULL", OracleType.VarChar, ParameterDirection.InputOutput, 4000)
                );
                //new cloudconnect20.Oracle.ParametroOracle("pNumAsiento", "NULL", OracleType.Number, ParameterDirection.Output),
                //new cloudconnect20.Oracle.ParametroOracle("pMonAplicado", "NULL", OracleType.Number, ParameterDirection.InputOutput),
                //new cloudconnect20.Oracle.ParametroOracle("pMonNuevoSaldo", "NULL", OracleType.Number, ParameterDirection.Output),

                if (objOracle.strError != null && objOracle.strError.Trim().Length > 0)
                {
                    objOracle.TransaccionRollback();
                    throw new Exception("Error al procesar pago de PR. F/M (PagoPR): " + objOracle.strError);
                }
                //mensaje de error que retorna la interface 
                if (!String.IsNullOrEmpty(objOracle.dsDatos.Tables["DATOS_PAGO"].Rows[0]["pMensajeUsuario"].ToString()))
                {
                    throw new Exception(objOracle.dsDatos.Tables["DATOS_PAGO"].Rows[0]["pMensajeUsuario"].ToString());
                }

                //debo almacenar el numero de movimiento respectivo al link
                string strSQL2 = " UPDATE BO.SCL_LISTADO_LINKS "
                               + " SET NUM_AUTORIZACION = '" + objPago.aut_visa + "', "
                               + "     NUM_MOVIMIENTO = '" + objOracle.dsDatos.Tables["DATOS_PAGO"].Rows[0]["p_NumMov"].ToString() + "', "
                               + "     VAL_TASA_CAMBIO = " + objOracle.dsDatos.Tables["DATOS_PAGO"].Rows[0]["pTasaAplicada"].ToString() + ", "
                               + "     FEC_PAGO_CORE = SYSDATE "
                               + " WHERE COD_LINK = " + objPago.cod_link
                               ;

                if (objOracle.EjecutarAccion(strSQL2) == -1)
                    throw new Exception("Error al actualizar datos de pago del link. F/M (aplicaPagoPR): " + objOracle.strError);

                objOracle.TransaccionCommit();
            }
            else
            {
                throw new Exception("Ya se encuentra pagado el link!!! F/M (aplicaPagoPR)");
            }

            return true;
        }
        catch (Exception ex)
        {
            this.strMensaje = ex.Message;
            return false;
        }
    }
    public bool aplicaPagoTC(clsPago objPago, string strMoneda)
    {
        try
        {
            //Debo revisar si ya fue pagado
            string strSQLRev = " SELECT NUM_AUTORIZACION, "
                             + "        NUM_MOVIMIENTO "
                             + "   FROM BO.SCL_LISTADO_LINKS "
                             + "  WHERE COD_SKU = '" + objPago.cod_sku + "' "
                             + "    AND NUM_MOVIMIENTO IS NOT NULL ";

            if (objOracle.ConsultarDatos(strSQLRev, "revisa_pago") == null)
            {
                objOracle.TransaccionRollback();
                throw new Exception("Error al revisar información del pago. F/M: (aplicaPagoTC): " + objOracle.strError);
            }

            if (objOracle.dsDatos.Tables["revisa_pago"].Rows.Count == 0)
            {
                objOracle.TransaccionIniciar();

                objOracle.EjecutarProcedimientoAlmacenado("BO.PKG_SCL.PkgScl_PagTc", "DATOS_PAGO",
                new cloudconnect20.Oracle.ParametroOracle("pCodEmpresa", "1", OracleType.VarChar, ParameterDirection.Input),
                new cloudconnect20.Oracle.ParametroOracle("pNumCtaCredito", objPago.num_cta, OracleType.VarChar, ParameterDirection.Input),
                new cloudconnect20.Oracle.ParametroOracle("pTarjeta", "NULL", OracleType.Number, ParameterDirection.Input),
                new cloudconnect20.Oracle.ParametroOracle("pMoneda", strMoneda, OracleType.VarChar, ParameterDirection.Input),
                new cloudconnect20.Oracle.ParametroOracle("pQtzMonPago", objPago.mon_pago, OracleType.Number, ParameterDirection.Input),
                new cloudconnect20.Oracle.ParametroOracle("pMonAplicado", "NULL", OracleType.Number, ParameterDirection.InputOutput),
                new cloudconnect20.Oracle.ParametroOracle("pNumMovPagoEfe", "NULL", OracleType.Number, ParameterDirection.InputOutput),
                new cloudconnect20.Oracle.ParametroOracle("pTasaAplicada", "NULL", OracleType.Number, ParameterDirection.Output),
                new cloudconnect20.Oracle.ParametroOracle("pMensajeUsuario", "NULL", OracleType.VarChar, ParameterDirection.Output, 4000),
                new cloudconnect20.Oracle.ParametroOracle("pMensajeTecnico", "NULL", OracleType.VarChar, ParameterDirection.Output, 4000)
                );
                //new cloudconnect20.Oracle.ParametroOracle("pCodigoError", "NULL", OracleType.VarChar, ParameterDirection.Output)

                if (objOracle.strError != null && objOracle.strError.Trim().Length > 0)
                {
                    objOracle.TransaccionRollback();
                    throw new Exception("Error al procesar pago de TC. F/M (PagoTC): " + objOracle.strError);
                }
                //mensaje de error que retorna la interface 
                if (!String.IsNullOrEmpty(objOracle.dsDatos.Tables["DATOS_PAGO"].Rows[0]["pMensajeUsuario"].ToString()))
                {
                    objOracle.TransaccionRollback();
                    throw new Exception(objOracle.dsDatos.Tables["DATOS_PAGO"].Rows[0]["pMensajeUsuario"].ToString());
                }

                //debo almacenar el numero de movimiento respectivo al link 
                string strSQL2 = " UPDATE BO.SCL_LISTADO_LINKS "
                               + " SET NUM_AUTORIZACION = '" + objPago.aut_visa + "', "
                               + "     NUM_MOVIMIENTO = '" + objOracle.dsDatos.Tables["DATOS_PAGO"].Rows[0]["pNumMovPagoEfe"].ToString() + "', "
                               + "     VAL_TASA_CAMBIO = " + objOracle.dsDatos.Tables["DATOS_PAGO"].Rows[0]["pTasaAplicada"].ToString() + ", "
                               + "     FEC_PAGO_CORE = SYSDATE "
                               + " WHERE COD_LINK = " + objPago.cod_link
                               ;

                if (objOracle.EjecutarAccion(strSQL2) == -1)
                    throw new Exception("Error al actualizar datos de pago del link. F/M (aplicaPagoTC): " + objOracle.strError);

                objOracle.TransaccionCommit();
            }
            else
            {
                throw new Exception("Ya se encuentra pagado el link!!! F/M (aplicaPagoTC)");
            }
            return true;
        }
        catch (Exception ex)
        {
            this.strMensaje = ex.Message;
            return false;
        }
    }
    public bool getParametro(string cod_Link)
    {
        try
        {
            string strSQL = " SELECT P.NUM_CUENTA, P.TIP_CUENTA, P.TIP_PAGO, P.MON_COBRO "
                          + " FROM BO.SCL_LISTADO_LINKS L "
                          + " JOIN BO.SCL_PARAMETROS_LINK P ON P.COD_PARAMETRO = L.COD_PARAMETRO "
                          + " WHERE L.COD_LINK = " + cod_Link
                            ;
            if (objOracle.ConsultarDatos(strSQL, "datos_link") == null)
            {
                objOracle.TransaccionRollback();
                throw new Exception("Error al consultar parametros del link. F/M: (getParametro): " + objOracle.strError);
            }
            this.dsDaoLink = objOracle.dsDatos;
            return true;
        }
        catch (Exception ex)
        {
            this.strMensaje = ex.Message;
            return false;
        }
    }
    public bool getLinkCta(string strNumCta)
    {
        try
        {
            string strSQL = " SELECT COD_PARAMETRO, "
                          + "        DIA_MES, "
                          + "        CASE "
                          + "           WHEN DIA_MES < EXTRACT(DAY FROM SYSDATE) THEN TO_DATE(DIA_MES || '/' || TO_CHAR(ADD_MONTHS(SYSDATE, 1), 'mm/yyyy'), 'dd/mm/yyyy') "
                          + "           WHEN DIA_MES = EXTRACT(DAY FROM SYSDATE) THEN TRUNC(SYSDATE) "
                          + "           WHEN DIA_MES > EXTRACT(DAY FROM SYSDATE) THEN  TO_DATE(DIA_MES || '/' || TO_CHAR(SYSDATE, 'mm/yyyy'), 'dd/mm/yyyy') "
                          + "        END    PROXIMA_FECHA "
                          + "  FROM BO.SCL_PARAMETROS_LINK "
                          + " WHERE TIP_LINK = '1' "
                          + "   AND IND_ESTADO = 'A' "
                          + "   AND NUM_CUENTA = " + strNumCta
                          + "   AND ROWNUM = 1";

            if (objOracle.ConsultarDatos(strSQL, "link_cta") == null)
            {
                objOracle.TransaccionRollback();
                throw new Exception("Error al buscar información de la cuenta. F/M: (getLinkCta): " + objOracle.strError);
            }

            this.dsDaoLink = objOracle.dsDatos;
            return true;
        }
        catch (Exception ex)
        {
            this.strMensaje = ex.Message;
            return false;
        }
    }
    public bool getLinkParametro(string strCodParametro)
    {
        try
        {
            string strSQL = " SELECT COD_PARAMETRO, "
                          + "        DIA_MES, "
                          + "        CASE "
                          + "           WHEN DIA_MES < EXTRACT(DAY FROM SYSDATE) THEN TO_DATE(DIA_MES || '/' || TO_CHAR(ADD_MONTHS(SYSDATE, 1), 'mm/yyyy'), 'dd/mm/yyyy') "
                          + "           WHEN DIA_MES = EXTRACT(DAY FROM SYSDATE) THEN TRUNC(SYSDATE) "
                          + "           WHEN DIA_MES > EXTRACT(DAY FROM SYSDATE) THEN  TO_DATE(DIA_MES || '/' || TO_CHAR(SYSDATE, 'mm/yyyy'), 'dd/mm/yyyy') "
                          + "        END    PROXIMA_FECHA "
                          + "  FROM BO.SCL_PARAMETROS_LINK "
                          + " WHERE TIP_LINK = '1' "
                          + "   AND IND_ESTADO = 'A' "
                          + "   AND COD_PARAMETRO = " + strCodParametro
                          + "   AND ROWNUM = 1";

            if (objOracle.ConsultarDatos(strSQL, "link_param") == null)
            {
                objOracle.TransaccionRollback();
                throw new Exception("Error al buscar información del parametro. F/M: (getLinkParametro): " + objOracle.strError);
            }

            this.dsDaoLink = objOracle.dsDatos;
            return true;
        }
        catch (Exception ex)
        {
            this.strMensaje = ex.Message;
            return false;
        }
    }
    public bool getLinks()
    {
        try
        {
            string strSQL
           = "select l.COD_PARAMETRO, "
           + "       p.NUM_CUENTA, "
           + "       NVL(l.num_autorizacion,'Pendiente') NUM_AUTORIZACION, "
           + "       NVL(l.num_movimiento,'Pendiente') NUM_MOVIMIENTO "
           + "  from BO.SCL_LISTADO_LINKS l "
           + "  join BO.SCL_PARAMETROS_LINK p ON l.cod_parametro=p.cod_parametro "
           ;
            if (objOracle.ConsultarDatos(strSQL, "links") == null)
                throw new Exception("Error al obtener los links. F/M (getLinks):" + objOracle.strError);

            this.dsDaoLink = objOracle.dsDatos;
            return true;
        }
        catch (Exception ex)
        {
            strMensaje = ex.Message;
            return false;
        }
    }
    public bool getLinks(int start, int length, string columnaOrden, string dirOrden, string busqueda, object[] Columnas)
    {
        try
        {
            //string edit = @"           '<a href=' || CHR (39) || 'frmClienteEditar.aspx?cli=' || CLI.COD_CLI_ACI || CHR (39) || '><img src=' || CHR (39) || 'images' || CHR (47) || 'icons' || CHR (47) || 'pencil.png' || CHR (39) ||'></a>' AS EDIT";
            #region conteo total
            string strSQL = "SELECT COUNT(*) "
            + "              FROM BO.SCL_PARAMETROS_LINK LNK "
            + "              WHERE TRUNC(LNK.FEC_EMISION) >= ADD_MONTHS(TRUNC(SYSDATE,'MM'),-3) "
            //+ "                   LEFT JOIN BO.ACI_TRANSPORTADORA TRA ON CLI.COD_TRANSPO = TRA.COD_TRANSPO "
            ;

            if (objOracle.ConsultarDatos(strSQL, "links-count") == null)
                throw new Exception("Error al obtener los datos de Links.(getLinks): " + objOracle.strError.Substring(0, objOracle.strError.LastIndexOf("\n")));
            #endregion

            #region paginacion
            //if (length == null) length = 1;
            int min = start;
            int max = 1;
            if (length == -1) length = Int32.Parse(objOracle.dsDatos.Tables["links-count"].Rows[0][0].ToString());
            max = start + length;

            #endregion

            #region busqueda General
            string strBusquedaGen = "";
            if (busqueda != "")
            {
                busqueda = busqueda.ToUpper();
                strBusquedaGen =
              "      AND ( "
            + "      LNK.COD_PARAMETRO like '%" + busqueda + "%' or "
            + "      LNK.NUM_CUENTA like '%" + busqueda + "%' or  "
            + "      LNK.MON_COBRO like '%" + busqueda + "%' or "
            + "      LNK.FEC_EMISION like '%" + busqueda + "%' or "
            + "      LNK.COD_USUARIO like '%" + busqueda + "%' "
            //+ "      CLI.NOM_SUCURSAL like '%" + busqueda + "' or "            
            //+ "      CLI.COD_TRANSPO like '%" + busqueda + "%' or "
            //+ "      TRA.NOM_TRANSPO like '%" + busqueda + "%' or  "
            //+ "      CLI.IND_ESTADO like '%" + busqueda + "%' "            
            + "      ) "
            ;
            }
            #endregion

            #region busquedaColumnas
            string strBusquedaCols = "";
            foreach (Dictionary<string, object> columna in Columnas)
            {
                bool buscarEnCol = (bool)columna["searchable"];
                if (buscarEnCol)
                {
                    Dictionary<string, object> search = (Dictionary<string, object>)columna["search"];
                    if (search["value"].ToString() != "")
                    {
                        strBusquedaCols = strBusquedaCols + (strBusquedaCols == "" ? " AND ( " : " AND ") + columna["name"].ToString() + " IN (" + search["value"].ToString().Replace("^(", "").Replace(")$", "").Replace("|", ",") + ") ";
                    }
                }
            }
            if (strBusquedaCols != "") strBusquedaCols = strBusquedaCols + " )";
            #endregion

            #region string busqueda
            string strBusqueda = strBusquedaGen + strBusquedaCols;
            #endregion

            #region conteo busqueda
            strSQL = "SELECT COUNT(*) "
            + "     FROM BO.SCL_PARAMETROS_LINK LNK "
            //+ "         LEFT JOIN BO.ACI_TRANSPORTADORA TRA ON CLI.COD_TRANSPO = TRA.COD_TRANSPO "
            + "         WHERE 1 = 1  "
            + strBusqueda
            ;

            if (objOracle.ConsultarDatos(strSQL, "links-count-search") == null)
                throw new Exception("Error al obtener los datos de clientes.(getLinks): " + objOracle.strError.Substring(0, objOracle.strError.LastIndexOf("\n")));
            #endregion

            #region filtros
            /*strSQL = "     SELECT DISTINCT "
            + "           PL.COD_PUESTO, PL.COD_PUESTO ||' - '||REPLACE(PP.NOM_PUESTO,'@','') AS NOM_PUESTO"
            + "     FROM PUE_PLAZA PL "
            + "         LEFT JOIN PUE_USU_PUESTO USUH ON PL.COD_PLAZA = USUH.COD_PLAZA AND USUH.IND_ESTADO <> 'I' "            
            + "         LEFT JOIN PUE_PUESTOS PP ON PL.COD_PUESTO = PP.COD_PUESTO "            
            + "     WHERE PL.IND_ESTADO NOT IN  ( " + no_estado_plazas + " ) "
            + "     START WITH  " + starwith
            + "     CONNECT BY PRIOR PL.COD_PLAZA = PL.COD_PLAZA_PADRE "
            ;
            if (objOracle.ConsultarDatos(strSQL, "filtro-puesto") == null)
                throw new Exception("Error al obtener los datos del Arbol-hijo.(getTree): " + objOracle.strError.Substring(0, objOracle.strError.LastIndexOf("\n")));

            strSQL = "     SELECT DISTINCT "
            + "           PL.COD_DEPTO, PL.COD_DEPTO ||' -  '||DP.NOM_DEPARTAMENTO AS NOM_DEPTO"
            + "      FROM PUE_PLAZA PL "
            + "         LEFT JOIN PUE_USU_PUESTO USUH ON PL.COD_PLAZA = USUH.COD_PLAZA AND USUH.IND_ESTADO <> 'I' "            
            + "         LEFT JOIN PUE_DEPARTAMENTO DP ON PL.COD_DEPTO = DP.COD_DEPTO "            
            + "     WHERE PL.IND_ESTADO NOT IN  ( " + no_estado_plazas + " ) "
            + "     START WITH  " + starwith
            + "     CONNECT BY PRIOR PL.COD_PLAZA = PL.COD_PLAZA_PADRE "
            ;
            if (objOracle.ConsultarDatos(strSQL, "filtro-unidad") == null)
                throw new Exception("Error al obtener los datos del Arbol-hijo.(getTree): " + objOracle.strError.Substring(0, objOracle.strError.LastIndexOf("\n")));

            strSQL = "     SELECT DISTINCT "
            + "           PL.COD_PLAZA_PADRE, PL.COD_PLAZA_PADRE || ' -  ' ||USUP.COD_USUARIO AS COD_USUARIO_PADRE "
            + "     FROM PUE_PLAZA PL "
            + "         LEFT JOIN PUE_USU_PUESTO USUH ON PL.COD_PLAZA = USUH.COD_PLAZA AND USUH.IND_ESTADO <> 'I' "
            + "         LEFT JOIN PUE_USU_PUESTO USUP ON PL.COD_PLAZA_PADRE = USUP.COD_PLAZA AND USUP.IND_ESTADO <> 'I' "            
            + "     WHERE PL.IND_ESTADO NOT IN  ( " + no_estado_plazas + " ) "
            + "     START WITH  " + starwith
            + "     CONNECT BY PRIOR PL.COD_PLAZA = PL.COD_PLAZA_PADRE "
            ;
            if (objOracle.ConsultarDatos(strSQL, "filtro-jefe") == null)
                throw new Exception("Error al obtener los datos del Arbol-hijo.(getTree): " + objOracle.strError.Substring(0, objOracle.strError.LastIndexOf("\n")));

            strSQL = "     SELECT DISTINCT "
            + "           PL.IND_ESTADO "
            + "      FROM PUE_PLAZA PL "
            + "         LEFT JOIN PUE_USU_PUESTO USUH ON PL.COD_PLAZA = USUH.COD_PLAZA AND USUH.IND_ESTADO <> 'I' "            
            + "     WHERE PL.IND_ESTADO NOT IN  ( " + no_estado_plazas + " ) "
            + "     START WITH  " + starwith
            + "     CONNECT BY PRIOR PL.COD_PLAZA = PL.COD_PLAZA_PADRE "
            ;
            if (objOracle.ConsultarDatos(strSQL, "filtro-estado") == null)
                throw new Exception("Error al obtener los datos del Arbol-hijo.(getTree): " + objOracle.strError.Substring(0, objOracle.strError.LastIndexOf("\n")));*/
            #endregion

            #region modificacion para orden
            /*if (columnaOrden == "NOMBRE")
                columnaOrden = @"RE.PRIMER_NOMBRE || ' '|| REGEXP_REPLACE (TRIM (RE.SEGUNDO_NOMBRE) || ' ','[\-]+|[\.]+','')|| RE.PRIMER_APELLIDO|| ' '|| REGEXP_REPLACE (TRIM (RE.SEGUNDO_APELLIDO) || ' ','^[\-]+|[\.]+$','')";*/
            #endregion

            #region datos
            strSQL = "SELECT "
            + "   LNK.COD_PARAMETRO  CORRELATIVO , "
            + "   LNK.NUM_CUENTA  PRODUCTO , "
            + "   LNK.MON_COBRO  MONTO , "
            + "   DECODE(LNK.TIP_PAGO,'1','Pagar en Dolares','Quetzales')  PAGO , "
            + "   TO_CHAR(LNK.FEC_EMISION,'dd/mm/yyyy')  EMISION_LINK , "
            + "   LNK.COD_USUARIO  USUARIO , "
            + "   DECODE(LNK.TIP_ENVIO,'1','SMS','Correo')  ENVIO , "
            + "   DECODE(LNK.TIP_LINK,'1','Automatico','Manual') TIPO_LINK "
            //+ edit
            + " FROM BO.SCL_PARAMETROS_LINK LNK "
            + " WHERE 1 = 1  "
            + " AND TRUNC(FEC_EMISION) >= ADD_MONTHS(TRUNC(SYSDATE,'MM'),-3) "
            + " AND NVL((SELECT USUARIO "
            + "             FROM GT_RRHH.RRHH_USUARIO_ROL "
            + "             WHERE ROL = 1330 "
            + "             AND USUARIO = USER ), COD_USUARIO ) = NVL((SELECT USUARIO "
            + "                                                          FROM GT_RRHH.RRHH_USUARIO_ROL "
            + "                                                          WHERE ROL = 1330 "
            + "                                                            AND USUARIO = USER), USER) "
            + strBusqueda
            + " ORDER BY " + columnaOrden + " " + dirOrden
            + " OFFSET " + min + " ROWS FETCH NEXT " + length + "ROWS ONLY"
            ;

            if (objOracle.ConsultarDatos(strSQL, "links") == null)
                throw new Exception("Error al obtener los datos de Links.(getLinks): " + objOracle.strError.Substring(0, objOracle.strError.LastIndexOf("\n")));
            #endregion

            this.dsDaoLink = objOracle.dsDatos;
            return true;

        }
        catch (Exception ex)
        {
            this.strMensaje = ex.Message;
            return false;
        }
    }
    public bool getLinksVerifica(int start, int length, string columnaOrden, string dirOrden, string busqueda, object[] Columnas)
    {
        try
        {
            //string edit = @"           '<a href=' || CHR (39) || 'frmClienteEditar.aspx?cli=' || LNK.COD_LINK || CHR (39) || '><img src=' || CHR (39) || 'images' || CHR (47) || 'icons' || CHR (47) || 'process.png' || CHR (39) ||'></a>' AS EDIT";
            //string edit = @"     '<a href=' || CHR (39) || '#' || CHR (39) || ' class=' || CHR (39) || 'table-edit' || CHR (39) || ' data-id=' || CHR (39) || 'Id' || CHR (39) || '><img src=' || CHR (39) || 'images' || CHR (47) || 'icons' || CHR (47) || 'process.png' || CHR (39) ||'></a>' AS EDIT";
            string edit = @" DECODE(LNK.NUM_AUTORIZACION,NULL, '<input type='|| CHR (39) ||'button'|| CHR (39) ||' id='|| CHR (39) ||'btnEdit'|| CHR (39) ||' class='|| CHR (39) ||'btn btn-primary'|| CHR (39) ||' value='|| CHR (39) ||'Consulta / Pago'|| CHR (39) ||' />' , NULL) AS EDIT";
            //string edit = @"       '<asp:Button ID='|| CHR (39) ||'btnSubmit'|| CHR (39) ||' Text='|| CHR (39) ||'Procesar'|| CHR (39) ||' runat='|| CHR (39) ||'server'|| CHR (39) ||' class='|| CHR (39) ||'btn btn-primary'|| CHR (39) ||' OnClick='|| CHR (39) ||'btnConsultaPago_Click'|| CHR (39) ||' UseSubmitBehavior='|| CHR (39) ||'False'|| CHR (39) ||' />' AS EDIT";

            #region conteo total
            string strSQL = "SELECT COUNT(*) "
            + "              FROM BO.SCL_LISTADO_LINKS LNK "
            + "              INNER JOIN BO.SCL_PARAMETROS_LINK PAR ON LNK.COD_PARAMETRO = PAR.COD_PARAMETRO "
            + "              WHERE TRUNC(LNK.FEC_ADICION) >= ADD_MONTHS(TRUNC(SYSDATE,'MM'),-3) "
            ;

            if (objOracle.ConsultarDatos(strSQL, "links-count") == null)
                throw new Exception("Error al obtener los datos de Links.(getLinksVerifica): " + objOracle.strError.Substring(0, objOracle.strError.LastIndexOf("\n")));
            #endregion

            #region paginacion
            //if (length == null) length = 1;
            int min = start;
            int max = 1;
            if (length == -1) length = Int32.Parse(objOracle.dsDatos.Tables["links-count"].Rows[0][0].ToString());
            max = start + length;

            #endregion

            #region busqueda General
            string strBusquedaGen = "";
            if (busqueda != "")
            {
                busqueda = busqueda.ToUpper();
                strBusquedaGen =
              "      AND ( "
            + "      LNK.COD_PARAMETRO like '%" + busqueda + "%' or "
            + "      PAR.NUM_CUENTA like '%" + busqueda + "%' or  "
            + "      LNK.COD_SKU like '%" + busqueda + "%' "
            + "      ) "
            ;
            }
            #endregion

            #region busquedaColumnas
            string strBusquedaCols = "";
            foreach (Dictionary<string, object> columna in Columnas)
            {
                bool buscarEnCol = (bool)columna["searchable"];
                if (buscarEnCol)
                {
                    Dictionary<string, object> search = (Dictionary<string, object>)columna["search"];
                    if (search["value"].ToString() != "")
                    {
                        strBusquedaCols = strBusquedaCols + (strBusquedaCols == "" ? " AND ( " : " AND ") + columna["name"].ToString() + " IN (" + search["value"].ToString().Replace("^(", "").Replace(")$", "").Replace("|", ",") + ") ";
                    }
                }
            }
            if (strBusquedaCols != "") strBusquedaCols = strBusquedaCols + " )";
            #endregion

            #region string busqueda
            string strBusqueda = strBusquedaGen + strBusquedaCols;
            #endregion

            #region conteo busqueda
            strSQL = "SELECT COUNT(*) "
            + "     FROM BO.SCL_LISTADO_LINKS LNK "
            + "   INNER JOIN BO.SCL_PARAMETROS_LINK PAR ON LNK.COD_PARAMETRO = PAR.COD_PARAMETRO "
            + "         WHERE 1 = 1  "
            + strBusqueda
            ;

            if (objOracle.ConsultarDatos(strSQL, "links-count-search") == null)
                throw new Exception("Error al obtener los datos de clientes.(getLinksVerifica): " + objOracle.strError.Substring(0, objOracle.strError.LastIndexOf("\n")));
            #endregion

            #region filtros
            /*strSQL = "     SELECT DISTINCT "
            + "           PL.COD_PUESTO, PL.COD_PUESTO ||' - '||REPLACE(PP.NOM_PUESTO,'@','') AS NOM_PUESTO"
            + "     FROM PUE_PLAZA PL "
            + "         LEFT JOIN PUE_USU_PUESTO USUH ON PL.COD_PLAZA = USUH.COD_PLAZA AND USUH.IND_ESTADO <> 'I' "            
            + "         LEFT JOIN PUE_PUESTOS PP ON PL.COD_PUESTO = PP.COD_PUESTO "            
            + "     WHERE PL.IND_ESTADO NOT IN  ( " + no_estado_plazas + " ) "
            + "     START WITH  " + starwith
            + "     CONNECT BY PRIOR PL.COD_PLAZA = PL.COD_PLAZA_PADRE "
            ;
            if (objOracle.ConsultarDatos(strSQL, "filtro-puesto") == null)
                throw new Exception("Error al obtener los datos del Arbol-hijo.(getTree): " + objOracle.strError.Substring(0, objOracle.strError.LastIndexOf("\n")));

            strSQL = "     SELECT DISTINCT "
            + "           PL.COD_DEPTO, PL.COD_DEPTO ||' -  '||DP.NOM_DEPARTAMENTO AS NOM_DEPTO"
            + "      FROM PUE_PLAZA PL "
            + "         LEFT JOIN PUE_USU_PUESTO USUH ON PL.COD_PLAZA = USUH.COD_PLAZA AND USUH.IND_ESTADO <> 'I' "            
            + "         LEFT JOIN PUE_DEPARTAMENTO DP ON PL.COD_DEPTO = DP.COD_DEPTO "            
            + "     WHERE PL.IND_ESTADO NOT IN  ( " + no_estado_plazas + " ) "
            + "     START WITH  " + starwith
            + "     CONNECT BY PRIOR PL.COD_PLAZA = PL.COD_PLAZA_PADRE "
            ;
            if (objOracle.ConsultarDatos(strSQL, "filtro-unidad") == null)
                throw new Exception("Error al obtener los datos del Arbol-hijo.(getTree): " + objOracle.strError.Substring(0, objOracle.strError.LastIndexOf("\n")));

            strSQL = "     SELECT DISTINCT "
            + "           PL.COD_PLAZA_PADRE, PL.COD_PLAZA_PADRE || ' -  ' ||USUP.COD_USUARIO AS COD_USUARIO_PADRE "
            + "     FROM PUE_PLAZA PL "
            + "         LEFT JOIN PUE_USU_PUESTO USUH ON PL.COD_PLAZA = USUH.COD_PLAZA AND USUH.IND_ESTADO <> 'I' "
            + "         LEFT JOIN PUE_USU_PUESTO USUP ON PL.COD_PLAZA_PADRE = USUP.COD_PLAZA AND USUP.IND_ESTADO <> 'I' "            
            + "     WHERE PL.IND_ESTADO NOT IN  ( " + no_estado_plazas + " ) "
            + "     START WITH  " + starwith
            + "     CONNECT BY PRIOR PL.COD_PLAZA = PL.COD_PLAZA_PADRE "
            ;
            if (objOracle.ConsultarDatos(strSQL, "filtro-jefe") == null)
                throw new Exception("Error al obtener los datos del Arbol-hijo.(getTree): " + objOracle.strError.Substring(0, objOracle.strError.LastIndexOf("\n")));

            strSQL = "     SELECT DISTINCT "
            + "           PL.IND_ESTADO "
            + "      FROM PUE_PLAZA PL "
            + "         LEFT JOIN PUE_USU_PUESTO USUH ON PL.COD_PLAZA = USUH.COD_PLAZA AND USUH.IND_ESTADO <> 'I' "            
            + "     WHERE PL.IND_ESTADO NOT IN  ( " + no_estado_plazas + " ) "
            + "     START WITH  " + starwith
            + "     CONNECT BY PRIOR PL.COD_PLAZA = PL.COD_PLAZA_PADRE "
            ;
            if (objOracle.ConsultarDatos(strSQL, "filtro-estado") == null)
                throw new Exception("Error al obtener los datos del Arbol-hijo.(getTree): " + objOracle.strError.Substring(0, objOracle.strError.LastIndexOf("\n")));*/
            #endregion

            #region modificacion para orden
            /*if (columnaOrden == "NOMBRE")
                columnaOrden = @"RE.PRIMER_NOMBRE || ' '|| REGEXP_REPLACE (TRIM (RE.SEGUNDO_NOMBRE) || ' ','[\-]+|[\.]+','')|| RE.PRIMER_APELLIDO|| ' '|| REGEXP_REPLACE (TRIM (RE.SEGUNDO_APELLIDO) || ' ','^[\-]+|[\.]+$','')";*/
            #endregion

            #region datos
            strSQL = "SELECT "
            + "   LNK.COD_LINK  CORRELATIVO , "
            + "   PAR.NUM_CUENTA  PRODUCTO , "
            + "   LNK.COD_SKU     CODIGO_VISA , "
            + "   COALESCE(LNK.NUM_AUTORIZACION,'Pendiente')  NUM_AUTO , "
            + "   COALESCE(LNK.NUM_MOVIMIENTO,'Pendiente')  NUM_MOV , "
            + edit
            + " FROM BO.SCL_LISTADO_LINKS LNK "
            + "   INNER JOIN BO.SCL_PARAMETROS_LINK PAR ON LNK.COD_PARAMETRO = PAR.COD_PARAMETRO "
            + " WHERE 1 = 1  "
            + " AND TRUNC(LNK.FEC_ADICION) >= ADD_MONTHS(TRUNC(SYSDATE,'MM'),-3) "
            + " AND NVL((SELECT USUARIO "
            + "            FROM GT_RRHH.RRHH_USUARIO_ROL "
            + "           WHERE ROL = 1330 "
            + "             AND USUARIO = USER ), PAR.COD_USUARIO ) = NVL((SELECT USUARIO "
            + "                                                              FROM GT_RRHH.RRHH_USUARIO_ROL "
            + "                                                             WHERE ROL = 1330 "
            + "                                                               AND USUARIO = USER), USER) "
            + strBusqueda
            + " ORDER BY " + columnaOrden + " " + dirOrden
            + " OFFSET " + min + " ROWS FETCH NEXT " + length + "ROWS ONLY"
            ;

            if (objOracle.ConsultarDatos(strSQL, "links") == null)
                throw new Exception("Error al obtener los datos de Links.(getLinks): " + objOracle.strError.Substring(0, objOracle.strError.LastIndexOf("\n")));
            #endregion

            this.dsDaoLink = objOracle.dsDatos;
            return true;

        }
        catch (Exception ex)
        {
            this.strMensaje = ex.Message;
            return false;
        }
    }

    public bool notificaSMS(clsSMS objParam)
    {
        try
        {
            objOracle.EjecutarProcedimientoAlmacenado("BO.PKG_SCL.PkgScl_SmsEnviar", "envio_sms",
           new cloudconnect20.Oracle.ParametroOracle("p_NumCtaCredito", objParam.num_cta, OracleType.Number, ParameterDirection.Input),
           new cloudconnect20.Oracle.ParametroOracle("p_NumTelefono", objParam.telefono, OracleType.Number, ParameterDirection.Input),
           new cloudconnect20.Oracle.ParametroOracle("P_SmsMensaje", objParam.mensaje, OracleType.VarChar, ParameterDirection.Input),
           new cloudconnect20.Oracle.ParametroOracle("P_MsgError", "NULL", OracleType.VarChar, ParameterDirection.Output, 4000)
           );

            //if (objOracle.EjecutarAccion(strSQL) == -1)
            if (objOracle.strError != null && objOracle.strError.Trim().Length > 0)
            {
                objOracle.TransaccionRollback();
                throw new Exception("Error al enviar notificacion SMS Link. F/M (notificaSMS): " + objOracle.strError);
            }

            if (!String.IsNullOrEmpty(objOracle.dsDatos.Tables["envio_sms"].Rows[0]["P_MsgError"].ToString()))
            {
                objOracle.TransaccionRollback();
                throw new Exception(objOracle.dsDatos.Tables["envio_sms"].Rows[0]["P_MsgError"].ToString());
            }

            objOracle.TransaccionCommit();

            return true;
        }
        catch (Exception ex)
        {
            strMensaje = ex.Message;
            return false;
        }
    }

    public bool notificaMail(clsMail objParam)
    {
        try
        {
            objOracle.EjecutarProcedimientoAlmacenado("BO.PKG_SCL.PkgScl_SndMail", "envio_mail",
           new cloudconnect20.Oracle.ParametroOracle("p_EMail", objParam.mail, OracleType.VarChar, ParameterDirection.Input),
           new cloudconnect20.Oracle.ParametroOracle("p_Asunto", objParam.asunto, OracleType.VarChar, ParameterDirection.Input),
           new cloudconnect20.Oracle.ParametroOracle("p_DesBody", objParam.link, OracleType.VarChar, ParameterDirection.Input),
           new cloudconnect20.Oracle.ParametroOracle("P_MsgError", "NULL", OracleType.VarChar, ParameterDirection.Output)
           );

            //if (objOracle.EjecutarAccion(strSQL) == -1)
            if (objOracle.strError != null && objOracle.strError.Trim().Length > 0)
            {
                objOracle.TransaccionRollback();
                throw new Exception("Error al enviar notificacion SMS Link. F/M (notificaSMS): " + objOracle.strError);
            }

            objOracle.TransaccionCommit();

            return true;
        }
        catch (Exception ex)
        {
            strMensaje = ex.Message;
            return false;
        }
    }

    public bool updateEstadoLink(string strCodParametro)
    {
        try
        {
            objOracle.EjecutarProcedimientoAlmacenado("BO.PKG_SCL.PkgScl_UpdParamLinkEst", "ESTADO_LINK",
            new cloudconnect20.Oracle.ParametroOracle("p_CodParametro", strCodParametro, OracleType.Number, ParameterDirection.Input),
            new cloudconnect20.Oracle.ParametroOracle("p_IndEstado", "I", OracleType.VarChar, ParameterDirection.Input),
            new cloudconnect20.Oracle.ParametroOracle("P_MsgError", "NULL", OracleType.VarChar, ParameterDirection.InputOutput)
            );

            if (objOracle.strError != null && objOracle.strError.Trim().Length > 0)
            {
                objOracle.TransaccionRollback();
                throw new Exception("Error al modificar estado del link. F/M (updateEstadoLink): " + objOracle.strError);
            }

            if (!String.IsNullOrEmpty(objOracle.dsDatos.Tables["ESTADO_LINK"].Rows[0]["P_MsgError"].ToString()))
                throw new Exception("Error en procedimiento BO.PKG_SCL.PkgScl_UpdParamLinkEst. F/M (updateEstadoLink): " + objOracle.dsDatos.Tables["ESTADO_LINK"].Rows[0]["P_MsgError"].ToString());

            return true;
        }
        catch (Exception ex)
        {
            strMensaje = ex.Message;
            return false;
        }
    }
    public bool obtieneLinks()
    {
        try
        {
            string strSQL
           = "SELECT COD_CONSECUTIVO, "
           + "       LONG_LINK "
           + "   FROM BO.CLI_SHORT_LINKS "
           + "  WHERE IND_INI_PROCESO = 0 "
           + "  AND SHORT_LINK IS NULL "
           + "  AND ROWNUM <= 25"
           ;
            if (objOracle.ConsultarDatos(strSQL, "links_largos") == null)
                throw new Exception("Error al obtener los links largos. F/M (obtieneLinks):" + objOracle.strError);

            this.dsDaoLink = objOracle.dsDatos;
            return true;
        }
        catch (Exception ex)
        {
            strMensaje = ex.Message;
            return false;
        }

    }

    public bool obtieneLinks_NumRow(Int16 int16NumRow)
    {
        try
        {
            string strSQL
           = "SELECT COD_CONSECUTIVO, "
           + "       LONG_LINK "
           + "   FROM BO.CLI_SHORT_LINKS "
           + "  WHERE IND_INI_PROCESO = 0 "
           + "  AND SHORT_LINK IS NULL "
           + "  AND ROWNUM <= " + int16NumRow
           ;
            if (objOracle.ConsultarDatos(strSQL, "links_largos") == null)
                throw new Exception("Error al obtener los links largos. F/M (obtieneLinks):" + objOracle.strError);

            this.dsDaoLink = objOracle.dsDatos;
            return true;
        }
        catch (Exception ex)
        {
            strMensaje = ex.Message;
            return false;
        }

    }
    //25Nov2024.Fin

    public bool existenPendientes()
    {
        try
        {
            string strSQL
           = "SELECT COUNT(1) REGISTROS "
           + "   FROM BO.CLI_SHORT_LINKS "
           + "  WHERE IND_INI_PROCESO = 0 "
           + "  AND SHORT_LINK IS NULL "
           ;
            if (objOracle.ConsultarDatos(strSQL, "url_conteo") == null)
                throw new Exception("Error al verificar información. F/M (existenPendientes):" + objOracle.strError);

            this.dsDaoLink = objOracle.dsDatos;
            return true;
        }
        catch (Exception ex)
        {
            strMensaje = ex.Message;
            return false;
        }
    }
    public bool existePeriferico(int intPeriferico)
    {
        try
        {
            string strSQL
           = "SELECT COUNT(1) perifericos "
           + "   FROM BO.PPL_LISTADO_PERIFERICO "
           + "  WHERE COD_PERIFERICO = " + intPeriferico.ToString()
           ;
            if (objOracle.ConsultarDatos(strSQL, "lista_perifericos") == null)
                throw new Exception("Error al verificar información. F/M (existePeriferico):" + objOracle.strError);

            this.dsDaoLink = objOracle.dsDatos;

            return true;
        }
        catch (Exception ex)
        {
            strMensaje = ex.Message;
            return false;
        }
    }
    public bool updateURLCorto(decimal numConsecutivo, string strURLCorto)
    {
        try
        {
            string strSQL;

            strSQL = "UPDATE BO.CLI_SHORT_LINKS "
                    + " SET SHORT_LINK = '" + strURLCorto + "',"
                    + "     IND_INI_PROCESO = 1,"
                    + "     IND_FIN_PROCESO = 1,"
                    + "     FEC_RESPUESTA = SYSDATE "
                    + " WHERE COD_CONSECUTIVO =" + numConsecutivo
                    ;
            if (objOracle.EjecutarAccion(strSQL) == -1)
                throw new Exception("Error al actualizar registro. F/M (updateURLCorto): " + objOracle.strError);
            
            objOracle.TransaccionCommit();

            return true;

        }
        catch (Exception ex)
        {
            strMensaje = ex.Message;
            return false;
        }
    }
    public bool registraBitacoraBD(string strUrlLargo, string strUrlCorto, int intPeriferico)
    {
        try
        {
            //WriteToFile(DateTime.Now + "==> Se creo link corto (" + "https://" + dato + ") asociado al URL largo (" + strURL + ") ");
            string strDetalle = "Se creo link corto (" + strUrlCorto + ") asociado al URL largo (" + strUrlLargo + ")";
            //insertar en tabla especifica
            string strSQL = "INSERT INTO  BO.PPL_BITACORA_WEBSERVICE (COD_PERIFERICO, DET_BITACORA) "
               + "VALUES (" + intPeriferico + ", '" + strDetalle + "') ";

            if (objOracle.EjecutarAccion(strSQL) == -1)
                throw new Exception("Error al insertar bitacora. F/M (registraBitacoraBD): " + objOracle.strError);

            return true;
        }
        catch (Exception ex)
        {
            strMensaje = ex.Message;
            return false;
        }
    }
}