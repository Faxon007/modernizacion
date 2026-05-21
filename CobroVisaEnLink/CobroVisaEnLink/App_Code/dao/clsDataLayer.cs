using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using cloudconnect20;
using System.Text;
using System.Security.Cryptography;
using System.IO;

/// <summary>
/// Summary description for dataLayer
/// </summary>
public class clsDataLayer 
{
    public cloudconnect20.Oracle objOracle;
   
    int intSistema;
    string strConexion, strServidor, strPalabra ;

    string strMsg;
    DataSet dsPrivate;

    public string strReturnMessage
    {
        get { return this.strMsg; }
        set { this.strMsg = value; }
    }
    public DataSet dsReturn
    {
        get { return this.dsPrivate; }
        set { this.dsPrivate = value; }
    }

    public cloudconnect20.Oracle publicObjOracle
    {
        get { return this.objOracle; }
        set { this.objOracle = value;}
    }    

    public clsDataLayer(string strUsuario, string strPassword, string strPath)
	{
		//
		// TODO: Add constructor logic here
		//
        inicializarValoresConexion();
        this.objOracle = new cloudconnect20.Oracle();
        //this.objSQLServer = new SQLServer();
        //this.objSQLServerREPORT = new SQLServer();        
        //this.objOracleCbs = new cloudconnect20.Oracle();
        
        
        //this.objOracle.CargarConexion(strPath, strPalabra, strConexion, "", "", strUsuario, strPassword);
        this.objOracle.CargarConexion(strPath, strPalabra, strConexion, "", "", strUsuario, strPassword);

        /*this.objOracleCbs.CargarConexion(strPath, strPalabra, strConexionCbs, "", "","", "");
        this.objSQLServer.CargarConexion(strPath, strPalabra, strConexionSQL, "", "EVODATA", "", "");
        this.objSQLServerREPORT.CargarConexion(strPath, strPalabra, strConexionSQLReports, "", "ReportServer", "", "");*/

        
	}

    public void inicializarValoresConexion()
    {                
        intSistema = int.Parse(System.Configuration.ConfigurationManager.AppSettings["NoSistema"].ToString());
        strServidor = System.Configuration.ConfigurationManager.AppSettings["servidor"];
        strPalabra = System.Configuration.ConfigurationManager.AppSettings["palClave"]; 
        strConexion = System.Configuration.ConfigurationManager.AppSettings["conexion"];
        /*strConexionCbs = System.Configuration.ConfigurationManager.AppSettings["conexion_CBS"];
        strConexionSQL = System.Configuration.ConfigurationManager.AppSettings["conexion_SQL"];                        
        strConexionSQLReports = System.Configuration.ConfigurationManager.AppSettings["conexion_SQLREPORT"];*/
        

    }

    public void iniciarTransaccion(cloudconnect20.Oracle objTransaccion)
    {
        //this.objOracle.TransaccionIniciar();
        objTransaccion.TransaccionIniciar();
    }

    public void iniciarTransaccion()
    {
        //this.objOracle.TransaccionIniciar();
        this.objOracle.TransaccionIniciar();
    }

    public void commitTransaccion()
    {
        this.objOracle.TransaccionCommit();
    }

    public void commitTransaccion(cloudconnect20.Oracle objTransaccion)
    {
        objTransaccion.TransaccionCommit();
    }

    public void rollBackTransaccion()
    {
        this.objOracle.TransaccionRollback();
    }

    public void rollBackTransaccion(cloudconnect20.Oracle objTransaccion)
    {
        objTransaccion.TransaccionRollback();
    }

    private static readonly byte[] initVectorBytes = Encoding.ASCII.GetBytes("tu89geji340b57a2");
    private const int keysize = 256;

    public string Encrypt(string plainText, string passPhrase)
    {
        byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);

        byte[] keyBytes = password.GetBytes(keysize / 8);
        using (RijndaelManaged symmetricKey = new RijndaelManaged())
        {
            symmetricKey.Mode = CipherMode.CBC;
            using (ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                        cryptoStream.FlushFinalBlock();
                        byte[] cipherTextBytes = memoryStream.ToArray();
                        return Convert.ToBase64String(cipherTextBytes);
                    }
                }
            }
        }

    }

    public string Decrypt(string cipherText, string passPhrase)
    {
        if (cipherText == "") return "";
        byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
        PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);

        byte[] keyBytes = password.GetBytes(keysize / 8);
        using (RijndaelManaged symmetricKey = new RijndaelManaged())
        {
            symmetricKey.Mode = CipherMode.CBC;
            using (ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes))
            {
                using (MemoryStream memoryStream = new MemoryStream(cipherTextBytes))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                        int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                        return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                    }
                }
            }
        }
    }

}
