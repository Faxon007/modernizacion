using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;


public class clsTransportadora
{
    public string clave { get; set; }
    public string cod_transpo { get; set; }
    public string nom_transpo { get; set; }
    public string direccion { get; set; }
    public string telefono { get; set; }
    public string nom_encargado { get; set; }    
    public string ind_estado { get; set; }

    public clsTransportadora()
    {
    }
}
/// <summary>
/// Descripción breve de daoTransportadora
/// </summary>
public class daoTransportadora : clsDataLayer
{
    string strMensaje;
    DataSet dsDaoTranportadora;

    public daoTransportadora(string strUsuario, string strPassword, string strPath)
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

    public DataSet dsTransportadora
    {
        get { return this.dsDaoTranportadora; }
    }
    

    public static byte[] GenerateSalt()
    {
        using (var randomNumberGenerator = new RNGCryptoServiceProvider())
        {
            var randomNumber = new byte[32];
            randomNumberGenerator.GetBytes(randomNumber);

            return randomNumber;
        }
    }

    //usar este metodo para framework 4.6 o inferior
    public static byte[] PBKDF2Sha256GetBytes(int dklen, byte[] password, byte[] salt, int iterationCount)
    {
        using (var hmac = new HMACSHA256(password))
        {
            int hashLength = hmac.HashSize / 8;
            if ((hmac.HashSize & 7) != 0)
                hashLength++;
            int keyLength = dklen / hashLength;
            if ((long)dklen > (0xFFFFFFFFL * hashLength) || dklen < 0)
                throw new ArgumentOutOfRangeException("dklen");
            if (dklen % hashLength != 0)
                keyLength++;
            byte[] extendedkey = new byte[salt.Length + 4];
            Buffer.BlockCopy(salt, 0, extendedkey, 0, salt.Length);
            using (var ms = new System.IO.MemoryStream())
            {
                for (int i = 0; i < keyLength; i++)
                {
                    extendedkey[salt.Length] = (byte)(((i + 1) >> 24) & 0xFF);
                    extendedkey[salt.Length + 1] = (byte)(((i + 1) >> 16) & 0xFF);
                    extendedkey[salt.Length + 2] = (byte)(((i + 1) >> 8) & 0xFF);
                    extendedkey[salt.Length + 3] = (byte)(((i + 1)) & 0xFF);
                    byte[] u = hmac.ComputeHash(extendedkey);
                    Array.Clear(extendedkey, salt.Length, 4);
                    byte[] f = u;
                    for (int j = 1; j < iterationCount; j++)
                    {
                        u = hmac.ComputeHash(u);
                        for (int k = 0; k < f.Length; k++)
                        {
                            f[k] ^= u[k];
                        }
                    }
                    ms.Write(f, 0, f.Length);
                    Array.Clear(u, 0, u.Length);
                    Array.Clear(f, 0, f.Length);
                }
                byte[] dk = new byte[dklen];
                ms.Position = 0;
                ms.Read(dk, 0, dklen);
                ms.Position = 0;
                for (long i = 0; i < ms.Length; i++)
                {
                    ms.WriteByte(0);
                }
                Array.Clear(extendedkey, 0, extendedkey.Length);
                return dk;
            }
        }
    }

    /*Usar este metodo para framework 4.7 o superior
    public static byte[] HashPassword(byte[] toBeHashed, byte[] salt, int numberOfRounds)
    {
        using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(toBeHashed, salt, numberOfRounds,HashAlgorithmName.SHA256))
        {
            return rfc2898DeriveBytes.GetBytes(32);
        }
    }*/

    public bool insertUsuario(clsTransportadora objTransportadora)
    {
        try
        {
            //System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

            byte[] salt = GenerateSalt();
            //byte[] hashedPassword = HashPassword(Encoding.UTF8.GetBytes(objTransportadora.clave), salt, 5000); //para framework 4.7 o superior            
            byte[] hashedPassword = PBKDF2Sha256GetBytes(32, Encoding.UTF8.GetBytes(objTransportadora.clave), salt, 5000); //para framework 4.6 o inferior  


            //insertar en tabla generica de usuarios
            string strSQL = "INSERT INTO BO.SWS_TOKEN_CONTROL (COD_SISTEMA, COD_USUARIO, CLAVE, IND_ACTIVO, USU_INGRESO,FEC_INGRESO, SALT) "
                + "VALUES (189,'"+objTransportadora.cod_transpo+"','" +  Encoding.UTF8.GetString(hashedPassword).Replace("'","''") + "','A',USER,SYSDATE,'" + Encoding.UTF8.GetString(salt).Replace("'", "''") + "') ";
            
            if (objOracle.EjecutarAccion(strSQL) == -1)
                throw new Exception("Error al insertar Usuario. F/M (insertUsuario): " + objOracle.strError);

            return true;
        }
        catch (Exception ex)
        {
            this.strMensaje = ex.Message;
            return false;
        }
    }

    public bool updateUsuario(clsTransportadora objTransportadora)
    {
        try
        {
            string strSQL;
            if (objTransportadora.clave != "")
            {
                byte[] salt = GenerateSalt();
                //byte[] hashedPassword = HashPassword(Encoding.UTF8.GetBytes(objTransportadora.clave), salt, 5000); //para framework 4.7 o superior            
                byte[] hashedPassword = PBKDF2Sha256GetBytes(32, Encoding.UTF8.GetBytes(objTransportadora.clave), salt, 5000); //para framework 4.6 o inferior  

                strSQL = "UPDATE BO.SWS_TOKEN_CONTROL SET "
                    + " CLAVE = '"+ Convert.ToBase64String(hashedPassword) + "',"
                    + " IND_ACTIVO = '" + objTransportadora.ind_estado + "',"
                    + " SALT = '" + Convert.ToBase64String(salt) + "',"
                    + " USU_ACTUALIZO = USER ,"
                    + " FEC_ACTUALIZO = SYSDATE "
                    + " WHERE COD_USUARIO = '"+objTransportadora.cod_transpo+"'"
                    ;
            }
            else
            {
                strSQL = "UPDATE BO.SWS_TOKEN_CONTROL SET "                    
                    + " IND_ACTIVO = '" + objTransportadora.ind_estado + "'," 
                    + " USU_ACTUALIZO = USER ,"
                    + " FEC_ACTUALIZO = SYSDATE "
                    + " WHERE COD_USUARIO = '" + objTransportadora.cod_transpo + "'"
                    ;
            }
            if (objOracle.EjecutarAccion(strSQL) == -1)
                throw new Exception("Error al actualizar Usuario. F/M (updateUsuario): " + objOracle.strError);

            return true;
        }
        catch (Exception ex)
        {
            this.strMensaje = ex.Message;
            return false;
        }
    }

    public bool insertTransportadora(clsTransportadora objTransportadora)
    {
        try
        {
            //insertar en tabla generica de usuarios desde controlador, para poder integrarlo en una transaccion   

            //insertar en tabla especifiga de transportadoras
            string strSQL = "INSERT INTO BO.ACI_TRANSPORTADORA (COD_TRANSPO, NOM_TRANSPO, DIRECCION, TELEFONO, NOM_ENCARGADO) "
               + "VALUES ('" + objTransportadora.cod_transpo + "','" + objTransportadora.nom_transpo + "','" + objTransportadora .direccion + "','" + objTransportadora.telefono + "','" + objTransportadora.nom_encargado + "') ";

            if (objOracle.EjecutarAccion(strSQL) == -1)
                throw new Exception("Error al insertar Transportadora. F/M (insertTransportadora): " + objOracle.strError);

            return true;
        }
        catch (Exception ex)
        {
            this.strMensaje = ex.Message;
            return false;
        }
    }

    public bool updateTransportadora(clsTransportadora objTransportadora)
    {
        try
        {
            string strSQL = "UPDATE BO.ACI_TRANSPORTADORA SET"
                    + " NOM_TRANSPO ='" + objTransportadora.nom_transpo + "', "
                    + " DIRECCION= '" + objTransportadora.direccion + "', "
                    + " TELEFONO= '" + objTransportadora.telefono + "', "
                    + " NOM_ENCARGADO='" + objTransportadora.nom_encargado + "' "
                    + " WHERE COD_TRANSPO='" + objTransportadora.cod_transpo + "'"
                    ;
            
            if (objOracle.EjecutarAccion(strSQL) == -1)
                throw new Exception("Error al actualizar transportadora. F/M (updateTransportadora): " + objOracle.strError);
            return true;
        }
        catch (Exception ex)
        {
            strMensaje = ex.Message;
            return false;
        }
    }

    public bool getTransportadora(string usuario)
    {
        try
        {
            string strSQL
            = " SELECT * "
            + " FROM BO.ACI_TRANSPORTADORA TRA"
            + " LEFT JOIN BO.SWS_TOKEN_CONTROL SWS ON TRA.COD_TRANSPO =  SWS.COD_USUARIO "
            + " WHERE COD_TRANSPO = '" + usuario + "'"            
            ;

            if (objOracle.ConsultarDatos(strSQL, "datos_transpo") == null)
            {
                throw new Exception("Error al consultar Datos de Transportadora. F/M: (getTransportadora): " + objOracle.strError);
            }
            this.dsDaoTranportadora = objOracle.dsDatos;
            return true;
        }
        catch (Exception ex)
        {
            this.strMensaje = ex.Message;
            return false;
        }
    }
    public bool getTransportadoras()
    {
        try
        {
            string strSQL
           = "SELECT COD_TRANSPO, NOM_TRANSPO, DIRECCION, TELEFONO, NOM_ENCARGADO, DECODE(IND_ACTIVO,'A','Activo','Inactivo') AS IND_ACTIVO"
           + " FROM BO.ACI_TRANSPORTADORA TRA"
           + " LEFT JOIN BO.SWS_TOKEN_CONTROL SWS ON TRA.COD_TRANSPO =  SWS.COD_USUARIO "
           //+  (ind_estado!=""?" WHERE SWS.IND_ACTIVO = '"+ind_estado+"'":"") 
           + "ORDER BY NOM_TRANSPO ASC "
           ;
            if (objOracle.ConsultarDatos(strSQL, "transportadora") == null)
                throw new Exception("Error al obtener las transportadoras. F/M (getTransportadoras):" + objOracle.strError);
            this.dsDaoTranportadora = objOracle.dsDatos;
            return true;
        }
        catch (Exception ex)
        {
            strMensaje = ex.Message;
            return false;
        }
    }

    public bool getTransportadorasDLL(string cod_cli_aci="")
    {
        try
        {
            string strSQL =
            "SELECT COD_TRANSPO, NOM_TRANSPO "
            + " FROM BO.ACI_TRANSPORTADORA TRA "
            + " LEFT JOIN BO.SWS_TOKEN_CONTROL SWS ON TRA.COD_TRANSPO =  SWS.COD_USUARIO "
            + " WHERE SWS.IND_ACTIVO = 'A' "
            ;
            if(cod_cli_aci!="")
            strSQL = strSQL + " UNION " 
            +" SELECT TRA.COD_TRANSPO, NOM_TRANSPO || ' ('||DECODE(IND_ACTIVO,'A','ACTIVO','INACTIVO')|| ')' AS NOM_TRANSPO " 
            +" FROM BO.ACI_TRANSPORTADORA TRA " 
            +" LEFT JOIN BO.ACI_CLIENTE CLI ON TRA.COD_TRANSPO = CLI.COD_TRANSPO " 
            +" LEFT JOIN BO.SWS_TOKEN_CONTROL SWS ON TRA.COD_TRANSPO =  SWS.COD_USUARIO " 
            +" WHERE CLI.COD_CLI_ACI = "+ cod_cli_aci 
            +" AND IND_ACTIVO = 'I' ";

            if (objOracle.ConsultarDatos(strSQL, "transportadora") == null)
                throw new Exception("Error al obtener las transportadoras. F/M (getTransportadoras):" + objOracle.strError);
            this.dsDaoTranportadora = objOracle.dsDatos;
            return true;
        }
        catch (Exception ex)
        {
            strMensaje = ex.Message;
            return false;
        }
    }

    //public bool validarCodTranspotadora()
    //{

    //}
}
 