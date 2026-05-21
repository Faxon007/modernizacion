using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RestSharp;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Globalization;

public partial class Data
{
    [JsonProperty("codigo")]
    public string codigo { get; set; }

    [JsonProperty("nombre")]
    public string nombre { get; set; }
}
public partial class DataInfo
{
    [JsonProperty("nombre_interno")]
    public string nombre_interno { get; set; }
    [JsonProperty("codigo_interno")]
    public string codigo_interno { get; set; }
    [JsonProperty("token")]
    public string token { get; set; }
    [JsonProperty("precio")]
    public double precio { get; set; } //regresarlo a double
    [JsonProperty("titulo")]
    public string titulo { get; set; }
    [JsonProperty("descripcion")]
    public string descripcion { get; set; }
    [JsonProperty("imagen")]
    public string imagen { get; set; }
    [JsonProperty("visitas")]
    public int visitas { get; set; }
    [JsonProperty("tipo")]
    public string tipo { get; set; }
    [JsonProperty("estado")]
    public string estado { get; set; }
    [JsonProperty("moneda")]
    public string moneda { get; set; }
    [JsonProperty("total")]
    public double total { get; set; }
    [JsonProperty("redes_sociales")]
    public List<DataLink> redes_sociales { get; set; }
    [JsonProperty("ventas")]
    public List<DataVentas> ventas { get; set; }
    [JsonProperty("usuario")]
    public string usuario { get; set; }
}
public partial class DataVentas
{
    [JsonProperty("terminal")]
    public string terminal { get; set; }
    [JsonProperty("autorizacion")]
    public string autorizacion { get; set; }
    [JsonProperty("referencia")]
    public string referencia { get; set; }
    [JsonProperty("auditoria")]
    public string auditoria { get; set; }
    [JsonProperty("suffix")]
    public string suffix { get; set; }
    [JsonProperty("monto")]
    public double monto { get; set; }
    [JsonProperty("cuota")]
    public string cuota { get; set; }
    [JsonProperty("fecha_pago")]
    public string fecha_pago { get; set; }
    [JsonProperty("cliente")]
    public DataCliente cliente { get; set; }
}
public partial class DataCliente
{
    [JsonProperty("nombre")]
    public string nombre { get; set; }
    [JsonProperty("nit")]
    public string nit { get; set; }
    [JsonProperty("correo")]
    public string correo { get; set; }
    [JsonProperty("telefono")]
    public string telefono { get; set; }
}
public partial class Data2
{
    [JsonProperty("token")]
    public string token { get; set; }
}
public partial class Login
{
    [JsonProperty("result")]
    public string result { get; set; }
    [JsonProperty("message")]
    public string message { get; set; }
    [JsonProperty("data")]
    public Data2 data { get; set; }
}
public partial class Redes
{
    [JsonProperty("result")]
    public string result { get; set; }

    [JsonProperty("message")]
    public string message { get; set; }

    [JsonProperty("data")]
    public List<Data> data { get; set; }
}
public partial class DataLink
{
    [JsonProperty("nombre")]
    public string nombre { get; set; }
    [JsonProperty("url")]
    public string url { get; set; }
}
public partial class Link
{
    [JsonProperty("result")]
    public string result { get; set; }

    [JsonProperty("message")]
    public string message { get; set; }

    [JsonProperty("data")]
    public List<DataLink> data { get; set; }

}
public partial class InfoLink
{
    [JsonProperty("result")]
    public string result { get; set; }

    [JsonProperty("message")]
    public string message { get; set; }

    [JsonProperty("data")]
    public DataInfo data { get; set; }

}

//public class ParametrosLink
//{
//    public string tip_cuenta { get; set; }
//    public string num_cuenta { get; set; }
//    public string monto { get; set; }
//    public string tip_pago { get; set; }
//    public string tip_link { get; set; }
//    public string dia_envio { get; set; }
//}

/// <summary>
/// Summary description for VisaEnLink
/// </summary>
public class VisaEnLink: clsController
{
    public string error; //variable para reportar cualquier error
    private string strKey = System.Configuration.ConfigurationManager.AppSettings["strKey"];  //"bbba722f948709955de06b9e2a7e703e3bc15996"; //key brindado por el administrador
    private string strURLVisa = System.Configuration.ConfigurationManager.AppSettings["strURLVisa"];
    private string strToken; // token para utilizar en los metodos del API
    private string strRedes; // keys separados por comas 
    private string strLink;
    private string strSku;
    private string strAutorizacion;
    private string strActivo;
    private string strNombre;
    private string strInterno;
    private double Monto;

    public VisaEnLink(string strUsuario, string strPassword, string strPath) 
        : base(strUsuario, strPassword, strPath)
    {
        //
        // TODO: Add constructor logic here
        //
    }
    public string ObtengoSKU()
    {
        return strSku;
    }
    public string ObtengoURL()
    {
        return strLink;
    }
    public string ObtengoAutorizacion()
    {
        return strAutorizacion;
    }
    public string ObtengoEstadoLink()
    {
        return strActivo;
    }
    public bool ExisteToken()
    {
        try
        {
            controllerSitio objControllerSitio = new controllerSitio(strUsuarioConexion, strPassword, strPath);

            if (!objControllerSitio.getTokenInterno())
                throw new Exception(objControllerSitio.strReturnMessage);

            //revisa en BD si existe un token actual
            //if (String.IsNullOrEmpty(objControllerSitio.dsReturn.Tables["token"].Rows[0]["VAL_TOKEN"].ToString()))
            if (objControllerSitio.dsReturn.Tables["token"].Rows.Count == 0)
            {
                //debo obtener el resultado del token guardado
                if (GenerarToken())//genero un nuevo token
                {
                    //se procede a guardar token
                    if (!objControllerSitio.insertToken(strToken))
                        throw new Exception(objControllerSitio.strReturnMessage);

                    return true;
                }
                else
                    throw new Exception("Error al verificar token. F/M: (ExisteToken) " + error);
            }
            else
            {
                //debo obtener el resultado del token guardado
                strToken = objControllerSitio.dsReturn.Tables["token"].Rows[0]["VAL_TOKEN"].ToString();
            }
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
    public bool CrearLink(string strProducto, string strMonto, string imgPublicitaria, string strCodigoInterno)
    {
        try
        {
            //se obtienen las redes
            if (!GenerarRedes())
                throw new Exception("Error al obtener el key de redes (CrearLink) "+error);
            //se debe crear el link 
            if (!GeneraLink(strProducto, strMonto, strCodigoInterno))                //if (!GeneraLinkNew(strProducto, strMonto))
                throw new Exception("Error al crear link (CrearLink)" + error);
            //se carga la imagen publicitaria
            if (!GeneraImagen(strSku,imgPublicitaria))
                throw new Exception("Error al cargar imagen (CrearLink)"+ error);

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
    //---------------------------------------------------------------
    public bool ConsultaLink(clsPago objPago)
    {
        try
        {
            //var client = new RestClient(strURLVisa + "/index.php/rest_movil/muestraLink");
            var client = new RestClient(strURLVisa + "/api/link/single");

            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            //parametros
            request.AddParameter("llave", strKey);
            request.AddParameter("token", strToken);
            request.AddParameter("codigo", objPago.cod_sku); 
            //certificado de seguridad
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            IRestResponse response = client.Execute(request);

            //serializar respuesta
            InfoLink ObjList = JsonConvert.DeserializeObject<InfoLink>(response.Content);

            if (ObjList.result != "success")
                throw new Exception("Error al obtener datos del link (ConsultaLink) " + ObjList.message);

            if (ObjList.data.ventas.Count != 0)
            {
                //obtengo el numero de autorizacion
                strAutorizacion = ObjList.data.ventas[0].autorizacion;
                //obtengo el estado del link 
                strActivo = ObjList.data.estado;
                strNombre = ObjList.data.nombre_interno;
                strInterno = ObjList.data.nombre_interno;
                Monto = ObjList.data.precio;
            }
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
    public bool CambioEstado(string strSKU)
    {
        try
        {
            //se obtienen las redes
            if (!GenerarRedes())
                throw new Exception("Error al obtener el key de redes (CambioEstado)");

            var client = new RestClient(strURLVisa + "/index.php/rest_movil/link");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            //parametros
            request.AddParameter("key", strKey);
            request.AddParameter("token", strToken);
            request.AddParameter("codigo", strSKU);
            request.AddParameter("activar", "SI"); 
            request.AddParameter("nombre", strNombre);//verificar si va cuenta tc y pr
            request.AddParameter("nombre_interno", strInterno); //verificar si va cuenta tc y pr
            request.AddParameter("precio", Monto.ToString());
            request.AddParameter("redes", strRedes);
            request.AddParameter("cuota", "0");//pago de contado
            //certificado de seguridad
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            IRestResponse response = client.Execute(request);

            //serializar respuesta
            Link ObjLink = JsonConvert.DeserializeObject<Link>(response.Content);

            if (ObjLink.result != "success")
                throw new Exception("Error al crear link en el API (GeneraLink) " + ObjLink.message);

            //strSku = ObjLink.token_;
            //strLink = ObjLink.link[0]; //obtener URL

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
    private bool GeneraImagen(string strCodigoSKU, string imgPublicidad)
    {
        try
        {
            var client = new RestClient(strURLVisa + "/api/link/image");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            //parametros
            request.AddParameter("llave", strKey);
            request.AddParameter("token", strToken);
            request.AddParameter("codigo", strCodigoSKU); //no se envia para luego obtener el codigo correlativo
            request.AddParameter("imagen", imgPublicidad); //imagen en base64
            request.AddParameter("tipo", "jpg");
            //certificado de seguridad
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            IRestResponse response = client.Execute(request);

            //serializar respuesta
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, string> values = serializer.Deserialize<Dictionary<string, string>>(response.Content);

            if (values["result"] != "success")
                throw new Exception("Error: al cargar imagen (insertaImagen) " + values["message"]);

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
    private bool GeneraLinkNew(string strProducto, string strMonto)
    {
        try
        {
            string uriAPI = strURLVisa + "/api/link/maintenance";
            int toapi = 30;
            string methodName = "maintenance";

            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string> ("llave", strKey),
                    new KeyValuePair<string, string> ("token", strToken),
                    new KeyValuePair<string, string> ("nombre_interno", strProducto),
                    new KeyValuePair<string, string> ("codigo_interno", "100001"),
                    new KeyValuePair<string, string> ("titulo", strProducto),
                    new KeyValuePair<string, string> ("descripcion", strProducto),
                    new KeyValuePair<string, string> ("monto", strMonto),
                    new KeyValuePair<string, string> ("estado", "1"),
                    new KeyValuePair<string, string> ("cuotas","VC00"),
                    new KeyValuePair<string, string> ("redes_sociales", strRedes),
                    new KeyValuePair<string, string> ("eliminar_imagen","")
                }
            );

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(uriAPI);
                client.Timeout = TimeSpan.FromSeconds(toapi);
                //certificado de seguridad
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = (snder, cert, chain, error) => true;
                //HTTP Post
                Task<HttpResponseMessage> postTask = client.PostAsync(methodName, content);
                postTask.Wait();

                HttpResponseMessage result = postTask.Result;

                switch (result.StatusCode)
                {
                    case HttpStatusCode.OK:
                        var readTask = result.Content.ReadAsStringAsync();
                        string jsonResponse = readTask.Result;
                        break;
                    default:
                        var readTaskError = result.Content.ReadAsStringAsync();
                        string jsonResponseError = readTaskError.Result;
                        break;

                }

            }



                return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
    private bool GeneraLink(string strProducto, string strMonto, string strCodigoInterno)
    {
        try
        {
            var client = new RestClient(strURLVisa + "/api/link/maintenance");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            //request.AlwaysMultipartFormData = true;
            //parametros
            request.AddParameter("llave", strKey);
            request.AddParameter("token", strToken);
            request.AddParameter("codigo_interno", strCodigoInterno);
            //request.AddParameter("codigo", "30013459"); //no se envia para luego obtener el codigo correlativo
            request.AddParameter("titulo", strProducto);//verificar si va cuenta tc y pr
            request.AddParameter("cuotas", "VC00");
            request.AddParameter("nombre_interno", strProducto); //verificar si va cuenta tc y pr
            request.AddParameter("descripcion", strProducto);
            request.AddParameter("monto", strMonto ); //.Replace(",","")
            request.AddParameter("estado","1");
            request.AddParameter("redes_sociales", strRedes);
            //request.AddParameter("cuota", "0");//pago de contado
            //certificado de seguridad
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            IRestResponse response = client.Execute(request);

            //serializar respuesta
            Link ObjLink = JsonConvert.DeserializeObject<Link>(response.Content);

            if (ObjLink.result != "success")
                throw new Exception("Error al crear link en el API (GeneraLink) " + ObjLink.message);

            strSku = strCodigoInterno;//ObjLink.token_;
            strLink = ObjLink.data[0].url;//ObjLink.link[0]; //obtener URL

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
    private bool GenerarRedes()
    {
        try
        {
            var client = new RestClient(strURLVisa + "/api/network/all");
            var proxy = new WebProxy("webproxy.promerica.com.gt", 9095);
            proxy.Credentials = new NetworkCredential("servicio_bo", "B4nc0Pr0m3r!c4");
            client.Proxy = proxy;
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("llave", strKey);
            request.AddParameter("token", strToken);
            //certificado de seguridad
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            IRestResponse response = client.Execute(request);

            //serializar respuesta
            Redes ObjList = JsonConvert.DeserializeObject<Redes>(response.Content);

            if (ObjList.result != "success")
                throw new Exception("Error al obtener las redes (GenerarRedes) "+ ObjList.message);

            strRedes = ObjList.data[0].codigo; //Solamente E-Mail

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }

    }
    private bool GenerarToken()
    {
        try
        {
            var client = new RestClient(strURLVisa + "/api/login");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            //request.AlwaysMultipartFormData = true;
            request.AddParameter("llave", strKey);
            request.AddParameter("usuario", System.Configuration.ConfigurationManager.AppSettings["strUsuVisa"]); //consultar si debo ponerlos en webconfig 
            request.AddParameter("clave", System.Configuration.ConfigurationManager.AppSettings["strClaveVisa"]); //consultar si debo ponerlos en webconfig
            //certificado de seguridad
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.BadRequest)
                throw new Exception("Error (GenerarToken): " + response.StatusCode.ToString() + " ->" + response.ErrorException.ToString());

            //serializar respuesta
            Login ObjList = JsonConvert.DeserializeObject<Login>(response.Content);

            //serializar respuesta
            //JavaScriptSerializer serializer = new JavaScriptSerializer();
            //Dictionary<string, string> values = serializer.Deserialize<Dictionary<string, string>>(response.Content);

            if (ObjList.result != "success")
                throw new Exception("Error: al realizar login  (GenerarToken) " + ObjList.message);

            strToken = ObjList.data.token;

            //------aqui revisar

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }

    }

    //---------------------------------------------------------------


}