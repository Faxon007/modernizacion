using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Data.OracleClient;

public class clsBitacora
{
    public string cod_link { get; set; }
    public string cod_parametro { get; set; }
    public string descripcion { get; set; }
    public string tip_procesamiento { get; set; }

}
public class clsBitCore
{
    public string cod_persona { get; set; }
    public string num_cta_credito { get; set; }
    public string descripcion { get; set; }
    public string num_cta_prestamo { get; set; }
    public string tip_cuenta { get; set; }
}

/// <summary>
/// Descripción breve de daoSitio
/// </summary>
public class daoSitio : clsDataLayer
{ 
    string strMensaje;
    private DataSet dsDaoSitio;

    public daoSitio(string strUsuario, string strPassword, string strPath)
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
    public DataSet dsSitio
    {
        get { return this.dsDaoSitio; }
    }
    public bool getParametros()
    {
        try
        {
            string strSQL = " SELECT FRE_REV_AUTO, "
                           + " FRE_REV_HRS_REP, "
                           + " FRE_GEN_LINK,     "
                           + " FRE_GEN_HORA,     "
                           + " TC_TIP_TRANSAC,   "
                           + " TC_SUBTIP_TRANS,    "
                           + " NUM_CTA_CONTA_QTZ,   "
                           + " NUM_CTA_CONTA_DOL,     "
                           + " COD_AGENCIA,        "
                           + " COD_TIPO_TC,"
                           + " COD_SUBTIPO_TC,"
                           + " COD_TIPO_PR,"
                           + " COD_SUBTIPO_PR,"
                           + " COD_DEPARTAMENTO,"
                           + " COD_DEPTO_PR,"
                           + " DES_TRANSACCION, "
                           + " API_IMAGEN,      "
                           + " MSG_REMITENTE, "
                           + " MSG_HEADER,    "
                           + " MSG_FOOTER,    "
                           + " MSG_SMS        "
                           + " FROM BO.SCL_PARAM_SISTEMA "
                            ;
            if (objOracle.ConsultarDatos(strSQL, "param_sistema") == null)
            {
                throw new Exception("Error al consultar parametros del sistema. F/M: (getParametros): " + objOracle.strError);
            }
            this.dsDaoSitio = objOracle.dsDatos;
            return true;
        }
        catch (Exception ex)
        {
            strMensaje = ex.Message;
            return false;
        }
    }
    public bool obtengoCodigoInterno()
    {
        try
        {
            string strSQL = "SELECT S_SCL_CORRELATIVO.NEXTVAL   CORRELATIVO "
                          + " FROM DUAL"
                ;
            
            if (objOracle.ConsultarDatos(strSQL, "codigo_interno") == null)
            {
                throw new Exception("Error al obtener código correlativo. F/M: (obtengoCodigoInterno)" + objOracle.strError);
            }
            this.dsDaoSitio = objOracle.dsDatos;
            return true;
        }
        catch (Exception ex)
        {
            strMensaje = ex.Message;
            return false;
        }
    }
    public bool insertParametros(clsParametros objParametros)
    {
        try
        {
            OracleConnection con = new OracleConnection(objOracle.strCadenaConexion);
            con.Open();

            string strSQL = String.Format("INSERT INTO BO.SCL_PARAM_SISTEMA("
                            + "FRE_REV_AUTO ,  FRE_REV_HRS_REP ,   FRE_GEN_LINK    ,   FRE_GEN_HORA      ,"
                            + "TC_TIP_TRANSAC       ,  TC_SUBTIP_TRANS   ,  NUM_CTA_CONTA_QTZ  ,   NUM_CTA_CONTA_DOL   ,"
                            + "COD_AGENCIA          ,    COD_TIPO_TC       ,   COD_SUBTIPO_TC  ,   COD_TIPO_PR     ," 
                            + "COD_SUBTIPO_PR      ,    COD_DEPARTAMENTO  ,  "
                            + "DES_TRANSACCION      ,  API_IMAGEN       ,   MSG_REMITENTE      ,  MSG_HEADER ,"
                            + "MSG_FOOTER           , "
                            + "MSG_SMS        ,   COD_DEPTO_PR )"
                            + "VALUES('{0}', {1}, '{2}', '{3}', "
                                   + " {4}, '{5}','{6}', '{7}', "
                                   + " '{8}', {9},  {10}, {11}, "
                                   + " {12}, '{13}', "
                                   + " '{14}', :IMAGEN, '{15}', :HEADER,"
                                   + " :FOOTER, "
                                   + " '{16}', '{17}' )",
                                objParametros.FRE_REV_AUTORIZACION, objParametros.FRE_REV_HRS_REPETIR, objParametros.FRE_GEN_LINK, objParametros.FRE_GEN_HORA,
                                objParametros.TC_TIP_TRANSAC, objParametros.TC_SUBTIP_TRANS, objParametros.NUM_CTA_CONTA_QTZ, objParametros.NUM_CTA_CONTA_DOL,
                                objParametros.COD_AGENCIA, objParametros.COD_TIPO_TC, objParametros.COD_SUBTIPO_TC, objParametros.COD_TIPO_PR, 
                                objParametros.COD_SUBTIPO_PR, objParametros.COD_DEPARTAMENTO,
                                objParametros.DES_TRANSACCION, objParametros.MSG_REMITENTE,
                                objParametros.MSG_SMS, objParametros.COD_DEPTO_PR);

            OracleCommand cmd = new OracleCommand();
            cmd.CommandText = strSQL;
            cmd.Connection = con;
            cmd.CommandType = CommandType.Text;

            #region parametro imagen
            OracleParameter blobParameterImagen = new OracleParameter();
            blobParameterImagen.ParameterName = "IMAGEN";
            blobParameterImagen.OracleType = OracleType.Blob;
            blobParameterImagen.Direction = ParameterDirection.Input;

            String[] substrings = objParametros.API_IMAGEN.Split(',');

            string header = substrings[0];
            string imgData = substrings[1];

            Byte[] ImagenData = Convert.FromBase64String(imgData);
            #endregion

            #region parametro header
            OracleParameter blobParameterHeader = new OracleParameter();
            blobParameterHeader.ParameterName = "HEADER";
            blobParameterHeader.OracleType = OracleType.Blob;
            blobParameterHeader.Direction = ParameterDirection.Input;

            blobParameterHeader.Value = System.Text.Encoding.UTF8.GetBytes(objParametros.MSG_HEADER);
            #endregion

            #region parametro footer
            OracleParameter blobParameterFooter = new OracleParameter();
            blobParameterFooter.ParameterName = "FOOTER";
            blobParameterFooter.OracleType = OracleType.Blob;
            blobParameterFooter.Direction = ParameterDirection.Input;

            blobParameterFooter.Value = System.Text.Encoding.UTF8.GetBytes(objParametros.MSG_FOOTER);
            #endregion

            #region establecer conexion y guardar datos
            cmd.Parameters.Clear();
            blobParameterImagen.Value = ImagenData;
            cmd.Parameters.Add(blobParameterImagen);
            cmd.Parameters.Add(blobParameterHeader);
            cmd.Parameters.Add(blobParameterFooter);

            cmd.ExecuteNonQuery();

            dsReturn = objOracle.dsDatos;
            objOracle.TransaccionCommit();
            #endregion

            //strReturnMessage = "Error al registrar imagen. Error(nuevaConexionImg): " + ex.Message;
            //objOracle.TransaccionRollback();

            con.Close();

            return true;
        }
        catch (Exception ex)
        {
            strMensaje = ex.Message;
            return false;
        }
    }
    public bool updateParametros(clsParametros objParametros)
    {
        try
        {
            string strSQL1 = "SELECT 1 FROM BO.SCL_PARAM_SISTEMA";

            if (objOracle.ConsultarDatos(strSQL1, "param_sistema") == null)
                throw new Exception("Error al consultar parametros del sistema. F/M (updateParametros): " + strMensaje + objOracle.strError);
            //verifico que exista ya información parametrizada
            if (objOracle.dsDatos.Tables["param_sistema"].Rows.Count == 0)
            {
                if (!insertParametros(objParametros))
                    throw new Exception("Error al registrar parametros del sistema. F/M (updateParametros): " + strMensaje + objOracle.strError);
            }
            else
            {
                OracleConnection con = new OracleConnection(objOracle.strCadenaConexion);
                con.Open();

                string strSQL = String.Format("UPDATE BO.SCL_PARAM_SISTEMA "
                                            + " SET FRE_REV_AUTO = '{0}' ,  "
                                            + "  FRE_REV_HRS_REP = {1} ,  "
                                            + "  FRE_GEN_LINK  = '{2}' ,  "
                                            + "  FRE_GEN_HORA  = '{3}' ,  "
                                            + "  TC_TIP_TRANSAC   = {4} , "
                                            + "  TC_SUBTIP_TRANS  = '{5}' , "
                                            + "  NUM_CTA_CONTA_QTZ  = '{6}' , "
                                            + "  NUM_CTA_CONTA_DOL = '{7}' ,  "
                                            + "  COD_AGENCIA = '{8}' ,   "
                                            + "  COD_TIPO_TC = {9} ,     "
                                            + "  COD_SUBTIPO_TC = {10} , "
                                            + "  COD_TIPO_PR = {11} ,    "
                                            + "  COD_SUBTIPO_PR = {12} , "
                                            + "  COD_DEPARTAMENTO = '{13}' , "
                                            + "  DES_TRANSACCION = '{14}' ,  "
                                            + "  API_IMAGEN   = :IMAGEN ,    "
                                            + "  MSG_REMITENTE  = '{15}' ,  "
                                            + "  MSG_HEADER  = :HEADER ,    "
                                            + "  MSG_FOOTER  = :FOOTER ,    "
                                            + "  MSG_SMS    = '{16}' ,      "
                                            + "  USU_MODIFICA = USER ,      "
                                            + "  FEC_MODIFICA = SYSDATE ,   "
                                            + "  COD_DEPTO_PR = '{17}'      ",
                                    objParametros.FRE_REV_AUTORIZACION, objParametros.FRE_REV_HRS_REPETIR, objParametros.FRE_GEN_LINK, objParametros.FRE_GEN_HORA,
                                    objParametros.TC_TIP_TRANSAC, objParametros.TC_SUBTIP_TRANS, objParametros.NUM_CTA_CONTA_QTZ, objParametros.NUM_CTA_CONTA_DOL,
                                    objParametros.COD_AGENCIA, objParametros.COD_TIPO_TC, objParametros.COD_SUBTIPO_TC, objParametros.COD_TIPO_PR, 
                                    objParametros.COD_SUBTIPO_PR, objParametros.COD_DEPARTAMENTO,
                                    objParametros.DES_TRANSACCION, objParametros.MSG_REMITENTE, 
                                    objParametros.MSG_SMS, objParametros.COD_DEPTO_PR);

                OracleCommand cmd = new OracleCommand();
                cmd.CommandText = strSQL;
                cmd.Connection = con;
                cmd.CommandType = CommandType.Text;

                #region parametro imagen
                OracleParameter blobParameterImagen = new OracleParameter();
                blobParameterImagen.ParameterName = "IMAGEN";
                blobParameterImagen.OracleType = OracleType.Blob;
                blobParameterImagen.Direction = ParameterDirection.Input;

                String[] substrings = objParametros.API_IMAGEN.Split(',');

                string header = substrings[0];
                string imgData = substrings[1];

                Byte[] ImagenData = Convert.FromBase64String(imgData);
                #endregion

                #region parametro header
                OracleParameter blobParameterHeader = new OracleParameter();
                blobParameterHeader.ParameterName = "HEADER";
                blobParameterHeader.OracleType = OracleType.Blob;
                blobParameterHeader.Direction = ParameterDirection.Input;

                blobParameterHeader.Value = System.Text.Encoding.UTF8.GetBytes(objParametros.MSG_HEADER);
                #endregion

                #region parametro footer
                OracleParameter blobParameterFooter = new OracleParameter();
                blobParameterFooter.ParameterName = "FOOTER";
                blobParameterFooter.OracleType = OracleType.Blob;
                blobParameterFooter.Direction = ParameterDirection.Input;

                blobParameterFooter.Value = objParametros.MSG_FOOTER != "" ? System.Text.Encoding.UTF8.GetBytes(objParametros.MSG_FOOTER) :null;
                #endregion

                #region establecer conexion y guardar datos
                cmd.Parameters.Clear();
                blobParameterImagen.Value = ImagenData;
                cmd.Parameters.Add(blobParameterImagen);
                cmd.Parameters.Add(blobParameterHeader);
                cmd.Parameters.Add(blobParameterFooter);
                cmd.ExecuteNonQuery();

                dsReturn = objOracle.dsDatos;
                objOracle.TransaccionCommit();

                con.Close();
                #endregion
                //if (objOracle.EjecutarAccion(strSQL) == -1)
                //    throw new Exception("Error al actualizar parametros del sistema. F/M (updateParametros): " + objOracle.strError);

            }

            return true;
        }
        catch (Exception ex)
        {
            strMensaje = ex.Message;
            return false;
        }
    }
    public bool insertToken(string strToken)
    {
        try
        {
            string strSQL = "INSERT INTO BO.SCL_TOKEN_LINK (VAL_TOKEN) "
                            + "VALUES ('" + strToken + "' ) ";

            if (objOracle.EjecutarAccion(strSQL) == -1)
                throw new Exception("Error al insertar token. F/M (insertToken): " + objOracle.strError);

            objOracle.TransaccionCommit();

            return true;
        }
        catch (Exception ex)
        {
            this.strMensaje = ex.Message;
            return false;
        }
    }
    public bool getTokenInterno()
    {
        try
        {
            string strSQL = " SELECT VAL_TOKEN "
                        + " FROM BO.SCL_TOKEN_LINK "
                        + " WHERE TRUNC(FEC_EMISION) = TRUNC(SYSDATE)"
                        + "  ORDER BY FEC_EMISION DESC "
                        + " FETCH FIRST 1 ROW ONLY  "
            ;
            if (objOracle.ConsultarDatos(strSQL, "token") == null)
                throw new Exception("Error al consultar token. F/M: (getTokenInterno): " + objOracle.strError);

            this.dsDaoSitio = objOracle.dsDatos;
            return true;
        }
        catch (Exception ex)
        {
            this.strMensaje = ex.Message;
            return false;
        }
    }
    public bool registraBitacora(clsBitacora objRegistrar)
    {
        try
        {
            objOracle.EjecutarProcedimientoAlmacenado("BO.PKG_SCL.PkgScl_InsBitacoraLink", "BITACORA",
            new cloudconnect20.Oracle.ParametroOracle("p_CodLink", objRegistrar.cod_link, OracleType.VarChar, ParameterDirection.Input),
            new cloudconnect20.Oracle.ParametroOracle("p_CodParametro", objRegistrar.cod_parametro, OracleType.VarChar, ParameterDirection.Input),
            new cloudconnect20.Oracle.ParametroOracle("p_Descripcion", objRegistrar.descripcion, OracleType.VarChar, ParameterDirection.Input),
            new cloudconnect20.Oracle.ParametroOracle("p_TipoProcesamiento", objRegistrar.tip_procesamiento, OracleType.VarChar, ParameterDirection.Input),
            new cloudconnect20.Oracle.ParametroOracle("P_MsgError", "NULL", OracleType.VarChar, ParameterDirection.InputOutput)
            );

            if (objOracle.strError != null && objOracle.strError.Trim().Length > 0)
            {
                objOracle.TransaccionRollback();
                throw new Exception("Error al registrar bitacora del sistema. F/M (registraBitacora): " + objOracle.strError);
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
    public bool registraBitacoraCore(clsBitCore objRegistrar)
    {
        try
        {
            //objOracle.TransaccionIniciar();

            objOracle.EjecutarProcedimientoAlmacenado("BO.PKG_SCL.PkgScl_InsBitacoraCore", "BITACORA",
            new cloudconnect20.Oracle.ParametroOracle("P_CodPersona", objRegistrar.cod_persona, OracleType.VarChar, ParameterDirection.Input),
            new cloudconnect20.Oracle.ParametroOracle("P_NumCtaCredito", objRegistrar.num_cta_credito ?? "NULL", OracleType.VarChar, ParameterDirection.Input),
            new cloudconnect20.Oracle.ParametroOracle("P_NumPrestamo", objRegistrar.num_cta_prestamo ?? "NULL", OracleType.VarChar, ParameterDirection.Input),
            new cloudconnect20.Oracle.ParametroOracle("P_DesDetalle", objRegistrar.descripcion, OracleType.VarChar, ParameterDirection.Input),
            new cloudconnect20.Oracle.ParametroOracle("P_MsgError", "NULL", OracleType.VarChar, ParameterDirection.Output)
            );

            if (objOracle.strError != null && objOracle.strError.Trim().Length > 0)
            {
                objOracle.TransaccionRollback();
                throw new Exception("Error al ingresar bitacora al core. F/M (registraBitacoraCore): " + objOracle.strError);
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
        
}