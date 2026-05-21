using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

/// <summary>
/// Descripción breve de daoTransacciones
/// </summary>
public class daoTransacciones : clsDataLayer
{
    string strMensaje;
    DataSet dsDaoTransaccion;
    public daoTransacciones(string strUsuario, string strPassword, string strPath)
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

    public DataSet dsTransaccion
    {
        get { return this.dsDaoTransaccion; }
    }

    public bool getTransacciones(int start, int length, string columnaOrden, string dirOrden, string busqueda, object[] Columnas)
    {
        try
        {
            
            #region conteo total
            string strSQL = "SELECT COUNT(*) "
            + "              FROM BO.ACI_TRANSACCION TRA "
            + "                   LEFT JOIN BO.ACI_CLIENTE CLI ON TRA.COD_CLI_ACI = CLI.COD_CLI_ACI "
            ;

            if (objOracle.ConsultarDatos(strSQL, "transaccion-count") == null)
                throw new Exception("Error al obtener los datos de transacciones.(getTransacciones): " + objOracle.strError.Substring(0, objOracle.strError.LastIndexOf("\n")));
            #endregion

            #region paginacion
            //if (length == null) length = 1;
            int min = start;
            int max = 1;
            if (length == -1) length = Int32.Parse(objOracle.dsDatos.Tables["transaccion-count"].Rows[0][0].ToString());
            max = start + length;

            #endregion

            #region busqueda General
            string strBusquedaGen = "";
            if (busqueda != "")
            {
                busqueda = busqueda.ToUpper();
                strBusquedaGen =
              "      AND ( "
            + "     TRA.COD_TRANSACCION like '%" + busqueda + "%' or "
            + "     TRA.COD_BOLSA like '%" + busqueda + "%' or "
            + "     TRA.COD_CLI_ACI like '%" + busqueda + "%' or "
            + "     CLI.COD_CLI_TR like '%" + busqueda + "%' or "
            + "     CLI.COD_CLI like '%" + busqueda + "%' or "
            + "     CLI.NUM_CTA like '%" + busqueda + "%' or " 
            + "     CLI.NOM_CLIENTE like '%" + busqueda + "%' or "
            + "     CLI.COD_SUCURSAL like '%" + busqueda + "%' or "
            + "     CLI.COD_TRANSPO like '%" + busqueda + "%' or "
            + "     MON_EFECTIVO like '%" + busqueda + "%' or "
            + "     MON_CHQ_PROP like '%" + busqueda + "%' or "
            + "     MON_CHQ_AJEN like '%" + busqueda + "%' or "
            + "     MON_EFE_CONT like '%" + busqueda + "%' or "
            + "     MON_CHQ_PRP_CONT like '%" + busqueda + "%' or "
            + "     MON_CHQ_AJN_CONT like '%" + busqueda + "%' or "
            + "     NUM_TRA_EFECTIVO like '%" + busqueda + "%' or "
            + "     NUM_TRA_CHQ_PROPIO like '%" + busqueda + "%' or "
            //+ "     DIF_EFECTIVO like '%" + busqueda + "%' or "
            //+ "     DIF_CHQ_PRP like '%" + busqueda + "%' or "
            //+ "     DIF_CHQ_AJN like '%" + busqueda + "%' or "
            + "     FEC_INGRESO like '%" + busqueda + "%' or "
            + "     FEC_CONTEO  like '%" + busqueda + "%' "            
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
            + "     FROM BO.ACI_TRANSACCION TRA "
            + "          LEFT JOIN BO.ACI_CLIENTE CLI ON TRA.COD_CLI_ACI = CLI.COD_CLI_ACI "
            + "         WHERE 1 = 1  "
            + strBusqueda
            ;

            if (objOracle.ConsultarDatos(strSQL, "transaccion-count-search") == null)
                throw new Exception("Error al obtener los datos de transacciones.(getTransacciones): " + objOracle.strError.Substring(0, objOracle.strError.LastIndexOf("\n")));
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
            //strSQL = "SELECT * FROM ( "
            //+ "     SELECT "            
            //+ "           ROW_NUMBER() OVER (ORDER BY " + columnaOrden + " " + dirOrden + ", PL.COD_PLAZA " + dirOrden + " ) RNUM, "
            strSQL = "SELECT "
            + " TRA.COD_TRANSACCION, "
            + " TRA.COD_BOLSA,"
            + " TRA.COD_CLI_ACI,"
            + " CLI.COD_CLI_TR,"
            + " CLI.COD_CLI,"
            + " CLI.NOM_CLIENTE,"
            + " CLI.COD_SUCURSAL,"
            + " CLI.COD_TRANSPO,"
            + " CLI.NUM_CTA,"
            + " TO_CHAR(MON_EFECTIVO, '99,990.99') AS MON_EFECTIVO,"
            + " TO_CHAR(MON_CHQ_PROP, '99,990.99') AS MON_CHQ_PROP,"
            + " TO_CHAR(MON_CHQ_AJEN, '99,990.99') AS MON_CHQ_AJEN,"
            + " TO_CHAR(MON_EFE_CONT, '99,990.99') AS MON_EFE_CONT,"
            + " TO_CHAR(MON_CHQ_PRP_CONT, '99,990.99') AS MON_CHQ_PRP_CONT,"
            + " TO_CHAR(MON_CHQ_AJN_CONT, '99,990.99') AS MON_CHQ_AJN_CONT,"
            + " NUM_TRA_EFECTIVO,"
            + " NUM_TRA_CHQ_PROPIO,"
            + @" CASE WHEN DIF_EFECTIVO>0 THEN '<span class=' || CHR (39) || 'over'|| CHR (39) || '>' || TO_CHAR(DIF_EFECTIVO, '99,990.99')||'</span>' WHEN DIF_EFECTIVO<0 THEN '<span class=' || CHR (39) || 'under'|| CHR (39) || '>' ||TO_CHAR(DIF_EFECTIVO, '99,990.99')||'</span>' ELSE TO_CHAR(DIF_EFECTIVO, '99,990.99') END AS DIF_EFECTIVO,"
            + @" CASE WHEN DIF_CHQ_PRP>0 THEN '<span class=' || CHR (39) || 'over'|| CHR (39) || '>' || TO_CHAR(DIF_CHQ_PRP, '99,990.99')||'</span>' WHEN DIF_CHQ_PRP<0 THEN '<span class=' || CHR (39) || 'under'|| CHR (39) || '>' ||TO_CHAR(DIF_CHQ_PRP, '99,990.99')||'</span>' ELSE TO_CHAR(DIF_CHQ_PRP, '99,990.99') END AS DIF_CHQ_PRP,"
            + @" CASE WHEN DIF_CHQ_AJN>0 THEN '<span class=' || CHR (39) || 'over'|| CHR (39) || '>' || TO_CHAR(DIF_CHQ_AJN, '99,990.99')||'</span>' WHEN DIF_CHQ_AJN<0 THEN '<span class=' || CHR (39) || 'under'|| CHR (39) || '>' ||TO_CHAR(DIF_CHQ_AJN, '99,990.99')||'</span>' ELSE TO_CHAR(DIF_CHQ_AJN, '99,990.99') END AS DIF_CHQ_AJN,"
            + " TO_CHAR(FEC_INGRESO,'DD/MM/YYYY') AS FEC_INGRESO, "
            + " TO_CHAR(FEC_CONTEO ,'DD/MM/YYYY') AS FEC_CONTEO  "
            + " FROM BO.ACI_TRANSACCION TRA "
            + "     LEFT JOIN BO.ACI_CLIENTE CLI ON TRA.COD_CLI_ACI = CLI.COD_CLI_ACI "
            + " WHERE 1 = 1  "
            + strBusqueda
            + " ORDER BY "+columnaOrden+" "+dirOrden
            + " OFFSET " + min + " ROWS FETCH NEXT " + length + "ROWS ONLY"
            ;

            if (objOracle.ConsultarDatos(strSQL, "transaccion") == null)
                throw new Exception("Error al obtener los datos de transacciones.(getTransacciones): " + objOracle.strError.Substring(0, objOracle.strError.LastIndexOf("\n")));
            #endregion

            this.dsDaoTransaccion = objOracle.dsDatos;
            return true;

        }
        catch (Exception ex)
        {
            this.strMensaje = ex.Message;
            return false;
        }
    }
}