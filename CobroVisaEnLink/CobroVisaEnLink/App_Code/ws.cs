using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using System.Threading;
using RestSharp;
using System.Net;
using System.Threading.Tasks;

public class ArregloLinks
{
    public decimal codConsecutivo { get; set; }
    public string strURLOriginal { get; set; }
    public string strURLCorto { get; set; }
}

public class LinkAcortar
{
    public string destination { get; set; }
    public LinkAcortar(string url)
    {
        destination = url;
    }
    public domain domain = new domain();

}

public class domain
{
    public string id { get; set; }
    public domain()
    {
        id = "76b6fd2fb2814a729c67d881a118181c";
    }
}
//        id = "626c34612a1a45e1803256de68537d45";
//----------------------------------------
public class TinyUrlRequest
{
    public string domain { get; set; }
    public List<TinyUrlItem> items { get; set; }
}

public class TinyUrlItem
{
    public string operation { get; set; }
    public string url { get; set; }
    public string domain { get; set; }
    public string description { get; set; }
    public TinyUrlItem(string urlLargo, string consecutivo)
    {
        operation = "create";
        url = urlLargo;
        domain = "sl.bpgt.com.gt";
        description = consecutivo;
    }
}
//----------------------------------------
public class ApiResponse
{
    public List<UrlData> data { get; set; }
    public int code { get; set; }
    public List<object> errors { get; set; }
}

public class UrlData
{
    public string operation { get; set; }
    public string domain { get; set; }
    public string url { get; set; }
    public string description { get; set; }
    public string tinyurl { get; set; }
    public string status { get; set; }
    public string expires_at { get; set; }
}
//----------------------------------------
public class ResponseLocal
{
    public string shortUrl { get; set; }
    public string destination { get; set; }
}
//----------------------------------------
/// <summary>
/// Descripción breve de ws
/// </summary>
[WebService(Namespace = "http://www.bancopromerica.com.gt/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// Para permitir que se llame a este servicio web desde un script, usando ASP.NET AJAX, quite la marca de comentario de la línea siguiente. 
[System.Web.Script.Services.ScriptService]
public class ws : System.Web.Services.WebService
{
    //string strDominio = "pagos.bancopromerica.com.gt";
    //string strApiKey = "df72607c47014d41bec5555af145e41f";
    string strApiKey = "8111d1f3ef22403a93587b625e6edb01";
    
    //LDMR 23/02/2025
    string strDominio = System.Configuration.ConfigurationManager.AppSettings["strDominio"];
    string strServer = System.Configuration.ConfigurationManager.AppSettings["strServer"];
    string apikey = System.Configuration.ConfigurationManager.AppSettings["apikey"];

    public ws()
    {
        //Elimine la marca de comentario de la línea siguiente si utiliza los componentes diseñados 
        //InitializeComponent(); 
    }

    public string Estado(string cod_estado)
    {
        string nom_estado = "";
        switch (cod_estado)
        {

            case "A":
                nom_estado = "Activo";
                break;
            case "I":
                nom_estado = "Inactivo";
                break;
            default:
                nom_estado = cod_estado;
                break;
        }

        return nom_estado;
    }

    //metodo para acortar links en cualquier periférico
    [WebMethod(EnableSession = true, Description = "Obtiene el link corto brindando el URL largo")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string AcortarLink(string strURL)
    {
        JavaScriptSerializer js = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue };

        try
        {
            List<string> salida = new List<string>();
            //--
            
            var linkVisa = new[]
            {
                new
                {
                    destination = strURL,
                    domain = new
                    {
                        fullName = "lc.bpgt.com.gt" //strDominio
                    }
                    //, slashtag = "A_NEW_SLASHTAG"
                    //, title = "Rebrandly YouTube channel"
                }
            };
            /*
            var linkVisa = new
            {
                url = strURL,
                domain = "sl.bpgt.com.gt"
            };*/
            //TodO: 20240529.ini Cambio en request para acortar link
            IRestResponse response = CreateShortUrlBrandly(linkVisa);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var link = JsonConvert.DeserializeObject<dynamic>(response.Content);

                //var dato = link.data.tiny_url.ToString();
                var dato = link[0].shortUrl;
                salida.Add(dato.ToString()); //ser.Deserialize<List<string>>(link) ;

                //salida.Add("https://" + dato);
                //salida.Add(dato);
                //SE LLEVA BITACORA 
                WriteToFile(DateTime.Now + "==> Se creo link corto (" + dato + ") asociado al URL largo (" + strURL + ") ");
            }
            else
            {
                throw new Exception("Error: No se pudo crear el link corto");
            }
            //TodO: 20240529.ini Cambio en request para acortar link

            #region codigo viejo 

            //using (var httpClient = new HttpClient { BaseAddress = new Uri("https://enterprise-api.rebrandly.com") }) //api  https://enterprise-api.rebrandly.com 
            //{
            //    httpClient.DefaultRequestHeaders.Add("apikey", "862b888158024429bbdff231e4c600ca"); //strApiKey
            //    httpClient.DefaultRequestHeaders.Add("workspace", "1cf40cc1d152469abd784d23bb664e64");

            //    var body = new StringContent(
            //        JsonConvert.SerializeObject(linkVisa), UnicodeEncoding.UTF8, "application/json");

            //    //certificado
            //    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            //    var response = httpClient.PostAsync("/v1/links", body).Result;

            //    if (response.IsSuccessStatusCode)
            //    {
            //        var link = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
            //        var dato = link[0].shortUrl; //ser.Deserialize<List<string>>(link) ;

            //        salida.Add("https://" + dato);
            //        //SE LLEVA BITACORA 
            //        WriteToFile(DateTime.Now +"==> Se creo link corto (" + "https://" + dato + ") asociado al URL largo (" + strURL + ") ");
            //    }
            //    else
            //    {
            //        throw new Exception("Error: No se pudo crear el link corto");
            //    }
            //}

            //--
            #endregion
            string serializado = js.Serialize(salida);//salida
            return serializado;
        }
        catch (Exception ex)
        {
            return js.Serialize(ex.Message);
        }
    }

    //metodo para acortar links según periférico brindado
    [WebMethod(EnableSession = true, Description = "Obtiene el link corto brindando el URL largo identificando el periférico respectivo")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string AcortarLinkPeriferico(int intPeriferico, string strURL)
    {
        JavaScriptSerializer js = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue };

        try
        {
            //(  System.AppDomain.CurrentDomain.BaseDirectory + System.Configuration.ConfigurationManager.AppSettings["pathCef"]
            //validar si el periferico se encuentra en listado de BD 
            controllerLink objLink = new controllerLink("SERVICIO", //HttpContext.Current.Session["usuario"].ToString(), 
                                                        "QHeb6$CW4LbkIn", //HttpContext.Current.Session["clave"].ToString(), 
                                                        System.AppDomain.CurrentDomain.BaseDirectory + System.Configuration.ConfigurationManager.AppSettings["pathCef"] //HttpContext.Current.Session["path"].ToString()
                                                        );

            #region consultaPrincipal
            if (!objLink.existePeriferico(intPeriferico))
                throw new Exception(objLink.strReturnMessage);
            #endregion

            if ((decimal)objLink.dsReturn.Tables["lista_perifericos"].Rows[0]["perifericos"] == 0)
                throw new Exception("Error: No existe el periférico indicado en el parámetro -intPeriferico-");

            //proceso de acortar link 
            List<string> salida = new List<string>();
            //--OBJETO SOBRE REBRANDLY
            var linkVisa = new []
            {
                new
                {
                    destination = strURL,
                    domain = new
                    {
                        fullName = "bpgt.com.gt" //strDominio
                    }
                    //, slashtag = "A_NEW_SLASHTAG"
                    //, title = "Rebrandly YouTube channel"
                }
            };

            //TodO: 20240529.ini Cambio en request para acortar link
            IRestResponse response = CreateShortUrlBrandly(linkVisa);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var link = JsonConvert.DeserializeObject<dynamic>(response.Content);

                var dato = link[0].shortUrl; //ser.Deserialize<List<string>>(link) ;

                salida.Add(dato.ToString());
                //SE LLENA BITACORA 
                if (!objLink.registraBitacoraBD(strURL, dato, intPeriferico))
                    throw new Exception(objLink.strReturnMessage);
            }
            else
            {
                throw new Exception("Error: No se pudo crear el link corto. Codigo (" + response.StatusCode + ") " + response.Content);
            }
            //TodO: 20240529.ini Cambio en request para acortar link
            #region codigo viejo
            /*
            //using (var httpClient = new HttpClient { BaseAddress = new Uri("https://enterprise-api.rebrandly.com") }) //api  https://enterprise-api.rebrandly.com 
            //{
            //    httpClient.DefaultRequestHeaders.Add("apikey", "862b888158024429bbdff231e4c600ca"); //strApiKey
            //    httpClient.DefaultRequestHeaders.Add("workspace", "1cf40cc1d152469abd784d23bb664e64");

            //    var body = new StringContent(
            //        JsonConvert.SerializeObject(linkVisa), UnicodeEncoding.UTF8, "application/json");

            //    //certificado
            //    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            //    var response = httpClient.PostAsync("/v1/links", body).Result;

            //    if (response.IsSuccessStatusCode)
            //    {
            //        var link = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
            //        var dato = link[0].shortUrl; //ser.Deserialize<List<string>>(link) ;

            //        salida.Add("https://" + dato);
            //        //SE LLENA BITACORA 
            //        if (!objLink.registraBitacoraBD(strURL, "https://" + dato, intPeriferico))
            //            throw new Exception(objLink.strReturnMessage);

            //        //WriteToFile(DateTime.Now + "==> Se creo link corto (" + "https://" + dato + ") asociado al URL largo (" + strURL + ") ");
            //    }
            //    else
            //    {
            //        throw new Exception("Error: No se pudo crear el link corto. Codigo (" + response.StatusCode + ") " + response.ReasonPhrase);
            //    }
            //}*/
            #endregion
            //--
            string serializado = js.Serialize(salida);//salida
            return serializado;
        }
        catch (Exception ex)
        {
            return js.Serialize(ex.Message);
        }
    }

    //metodo para acortar links de forma masiva
    [WebMethod(EnableSession = true, Description = "Obtiene el link corto de forma masiva")]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string AcortarLinkMasivo()
    {
        JavaScriptSerializer js = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue };
        Int16 int16NumRow = 100;//80;
        //Int16 int16NumLotesAsync = 4;

        try
        {
            string strUsuario = "SERVICIO";
            //string strPassword = "Desarrollo2025.";
            string strPassword = "QHeb6$CW4LbkIn";
            #region Parte donde se valida que exista informacion en la tabla de links a acortar
            bool existeDatos = false;

            controllerLink objRevision = new controllerLink(strUsuario, strPassword, System.AppDomain.CurrentDomain.BaseDirectory + System.Configuration.ConfigurationManager.AppSettings["pathCef"]);

            if (!objRevision.existenPendientes())
                throw new Exception(objRevision.strReturnMessage);

            //verifico si existen datos 
            if ((Convert.ToDecimal(objRevision.dsReturn.Tables["url_conteo"].Rows[0]["REGISTROS"]) > 0))
                existeDatos = true;
            #endregion

            int totalLinks = Convert.ToInt32(objRevision.dsReturn.Tables["url_conteo"].Rows[0]["REGISTROS"]);
            //recorre hasta cumplir toda la información en la tabla
            while (existeDatos) 
            {
                controllerLink objLink = new controllerLink(strUsuario, strPassword, System.AppDomain.CurrentDomain.BaseDirectory + System.Configuration.ConfigurationManager.AppSettings["pathCef"]);
                //SE OBTIENEN LOS 100 LINKS
                if (!objLink.obtieneLinks_NumRow(int16NumRow))
                    throw new Exception(objLink.strReturnMessage);

                #region aqui es donde obtengo el conjunto de registros a acortar
                // debo realizar la estructura o array a tratar 
                var lista = objLink.dsReturn.Tables["links_largos"].AsEnumerable().Select(
                                                                        datarow => new ArregloLinks
                                                                        {
                                                                            codConsecutivo = datarow.Field<decimal>("COD_CONSECUTIVO"),
                                                                            strURLOriginal = datarow.Field<string>("LONG_LINK")
                                                                        }).ToList();



                // debo preparar la estructura para convertir el JSON a mandar 
                LinkAcortar[] linkJSON = new LinkAcortar[lista.Count];

                int ind = 0;

                foreach (var elemento in lista)
                {
                    linkJSON[ind] = new LinkAcortar(elemento.strURLOriginal); //HttpUtility.UrlEncode(elemento.strURLOriginal)
                    ind++;
                }
                #endregion

                #region nuevo codigo 

                IRestResponse response = CreateShortUrlBrandlyPut(linkJSON);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var listadoResultado = JsonConvert.DeserializeObject<dynamic>(response.Content);

                    for (int i = 0; i < listadoResultado.Count; i++)
                    {
                        controllerLink objUpdate = new controllerLink(strUsuario, strPassword, System.AppDomain.CurrentDomain.BaseDirectory + System.Configuration.ConfigurationManager.AppSettings["pathCef"]);
                        //aqui debo ir por el consecutivo de la lista en memoria contra la lista para obtener    
                        if (!objUpdate.updateURLCorto(lista[i].codConsecutivo, listadoResultado[i].shortUrl.ToString()))
                            throw new Exception(objLink.strReturnMessage);
                    }
                    //SE LLEVA BITACORA 
                    //WriteToFile(DateTime.Now + "==> Se crea batch de link corto de " + listadoResultado.Count + " Links. ");
                }
                else
                {
                    WriteToFile(DateTime.Now + "==> Error(HTTP response Status): " + response.StatusCode);
                    throw new Exception("401");
                }

                #endregion

                #region codigo por lotes REBRANDLY antiguo
                /*
                #region nuevo:
                int loteSize = (int)Math.Ceiling((double)linkJSON.Length / int16NumLotesAsync);
                var lotes = linkJSON
                            .Select((item, index) => new { item, index })
                            .GroupBy(x => x.index / loteSize)
                            .Select(g => g.Select(x => x.item).ToArray())
                            .ToArray();
                #endregion

                Task.Run(async () =>
                {
                    //TodO: 20240529.ini Cambio en request para acortar link masivo
                    //IRestResponse response = CreateShortUrlBrandlyPut(linkJSON);
                    var tareas = lotes.Select(lote => CreateShortUrlBrandlyPut_Async(lote));
                    IRestResponse[] responses = await Task.WhenAll(tareas);

                    //aqui debo iniciar el indice de la lista de 100 items
                    int l = 0;

                    foreach (var response in responses)
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var resultado = JsonConvert.DeserializeObject<dynamic>(response.Content);
                            //var dato = resultado[0].shortUrl; //ser.Deserialize<List<string>>(link) ;
                            // obtengo la respuesta y asigno la URL corta UPDATE
                            for (int i = 0; i < resultado.Count; i++) // CAMBIO LA VARIABLE ind POR CONTEO del resultado resultado.Count
                            {
                                controllerLink objUpdate = new controllerLink(strUsuario, strPassword, System.AppDomain.CurrentDomain.BaseDirectory + System.Configuration.ConfigurationManager.AppSettings["pathCef"]);

                                if (!objUpdate.updateURLCorto(lista[l].codConsecutivo, "https://" + resultado[i].shortUrl))
                                    throw new Exception(objLink.strReturnMessage);
                                //indice recorrido lista de 100 items
                                l++;
                            }

                            //SE LLEVA BITACORA 
                            WriteToFile(DateTime.Now + "==> Se crea batch de link corto de " + resultado.Count + " Links. ");
                        }
                        else
                        {
                            WriteToFile(DateTime.Now + "==> Error(HTTP response Status): " + response.StatusCode);
                            throw new Exception("401");
                        }
                    }

                }).GetAwaiter().GetResult();


                */
                #endregion

                WriteToFile(DateTime.Now + "==> Se consume batch completo de " + lista.Count + " Links. ");

                #region Parte donde se valida que exista informacion en la tabla de links a acortar
                controllerLink objRevision2 = new controllerLink(strUsuario, strPassword, System.AppDomain.CurrentDomain.BaseDirectory + System.Configuration.ConfigurationManager.AppSettings["pathCef"]);

                if (!objRevision2.existenPendientes())
                    throw new Exception(objRevision2.strReturnMessage);

                //verifico si existen datos aun
                if ((Convert.ToDecimal(objRevision2.dsReturn.Tables["url_conteo"].Rows[0]["REGISTROS"]) > 0))
                    existeDatos = true;
                else
                    existeDatos = false;
                #endregion
                //fin del ciclo
                //se procede a colocar un sleep de 15 segundos 
                // Using Sleep() method 
                //Thread.Sleep(3000);
            }

            return "EXITO";
        }
        catch (Exception ex)
        {
            WriteToFile(DateTime.Now + "==> Error:  " + ex.Message);
            //if (ex.Message == "401")
            //    throw new Exception("401");
            throw new Exception("401");
            //return js.Serialize(ex.Message);
        }
    }

    void WriteToFile(string Message)
    {
        string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\CreaLinks_Log_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
        if (!File.Exists(filepath))
        {
            // Create a file to write to.   
            using (StreamWriter sw = File.CreateText(filepath))
            {
                sw.WriteLine(Message);
            }
        }
        else
        {
            using (StreamWriter sw = File.AppendText(filepath))
            {
                sw.WriteLine(Message);
            }
        }
    }

    //metodo para listado de parametros de links
    [WebMethod(EnableSession = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetLinks(object parameters)
    {
        JavaScriptSerializer js = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue };
        try
        {

            if (Session["usuario"] == null)
                throw new Exception("401");

            //controllerCliente objCliente = new controllerCliente(HttpContext.Current.Session["usuario"].ToString(), HttpContext.Current.Session["clave"].ToString(), HttpContext.Current.Session["path"].ToString());
            controllerLink objLink = new controllerLink(HttpContext.Current.Session["usuario"].ToString(), HttpContext.Current.Session["clave"].ToString(), HttpContext.Current.Session["path"].ToString());

            Dictionary<string, object> parametros = (Dictionary<string, object>)parameters;
            Dictionary<string, object> salida = new Dictionary<string, object>();

            #region columnas
            object[] columnasO = (object[])parametros["columns"];
            #endregion

            #region orden
            object[] ordenO = (object[])parametros["order"];
            Dictionary<string, object> orden = (Dictionary<string, object>)ordenO[0];
            int columnaOrden = (int)orden["column"];
            string direccionOrden = (string)orden["dir"];
            Dictionary<string, object> nombreColOrden = (Dictionary<string, object>)columnasO[columnaOrden];
            #endregion

            #region busqueda
            Dictionary<string, object> busquedaO = (Dictionary<string, object>)parametros["search"];
            string strbusqueda = busquedaO["value"].ToString().Replace(' ', '%');
            #endregion

            #region consultaPrincipal
            if (!objLink.getLinks(
                Int32.Parse(parametros["start"].ToString()),
                Int32.Parse(parametros["length"].ToString()),
                nombreColOrden["name"].ToString(),//nombre columna orden
                direccionOrden, //direccion orden
                strbusqueda, //texto a buscar
                columnasO //busqueda por columnas
                ))
                throw new Exception(objLink.strReturnMessage);
            #endregion

            #region filtros
            /*List<Dictionary<string, object>> puestos = new List<Dictionary<string, object>>();
            Dictionary<string, object> puesto = new Dictionary<string, object>();

            List<Dictionary<string, object>> unidades = new List<Dictionary<string, object>>();
            Dictionary<string, object> unidad = new Dictionary<string, object>();

            List<Dictionary<string, object>> jefes = new List<Dictionary<string, object>>();
            Dictionary<string, object> jefe = new Dictionary<string, object>();

            List<Dictionary<string, object>> estados = new List<Dictionary<string, object>>();
            Dictionary<string, object> estado = new Dictionary<string, object>();*/

            /*

            foreach (DataRow dr in objControllerPuesto.dsReturn.Tables["filtro-puesto"].Rows)
            {
                puesto = new Dictionary<string, object>();
                puesto.Add("value", dr[0]);
                puesto.Add("label", dr[1]);
                puestos.Add(puesto);
            }

            foreach (DataRow dr in objControllerPuesto.dsReturn.Tables["filtro-unidad"].Rows)
            {
                unidad = new Dictionary<string, object>();
                unidad.Add("value", dr[0]);
                unidad.Add("label", dr[1]);
                unidades.Add(unidad);
            }

            foreach (DataRow dr in objControllerPuesto.dsReturn.Tables["filtro-jefe"].Rows)
            {
                jefe = new Dictionary<string, object>();
                jefe.Add("value", dr[0]);
                jefe.Add("label", dr[1]);
                jefes.Add(jefe);
            }            
            //filtro estado 
            foreach (DataRow dr in objControllerPuesto.dsReturn.Tables["filtro-estado"].Rows)
            {
                estado = new Dictionary<string, object>();
                estado.Add("value", "'" + dr[0].ToString().Trim() + "'");
                estado.Add("label", Estado(dr[0].ToString()));
                estados.Add(estado);
            }*/
            #endregion

            #region recorsTotal
            int recordsTotal = Int32.Parse(objLink.dsReturn.Tables["links-count"].Rows[0][0].ToString());
            #endregion

            #region recordsFiltered
            int recordsFiltered = Int32.Parse(objLink.dsReturn.Tables["links-count-search"].Rows[0][0].ToString());
            #endregion

            #region join
            DataSet ds = new DataSet("DataSet");
            ds.Tables.Add(objLink.dsReturn.Tables["links"].Copy());

            //join con sql


            #endregion

            #region construccion Arreglo Data
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;

            foreach (DataRow dr in ds.Tables["links"].Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in ds.Tables["links"].Columns)
                {
                    if (col.ColumnName == "IND_ESTADO")
                        dr[col] = Estado(dr[col].ToString());
                    row.Add(col.ColumnName, dr[col]);
                }
                rows.Add(row);

            }
            #endregion

            salida.Add("draw", parametros["draw"]);
            salida.Add("recordsTotal", recordsTotal);
            salida.Add("recordsFiltered", recordsFiltered);
            salida.Add("data", rows);
            //salida.Add("yadcf_data_9", puestos);

            string serializado = js.Serialize(salida);
            return serializado;
        }
        catch (Exception ex)
        {
            if (ex.Message == "401")
                throw new Exception("401");

            return js.Serialize(ex.Message);
        }

    }

    //metodo para listado de links a revisar
    [WebMethod(EnableSession = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string GetLinksVerifica(object parameters)
    {
        JavaScriptSerializer js = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue };
        try
        {

            if (Session["usuario"] == null)
                throw new Exception("401");

            controllerLink objLinks = new controllerLink(HttpContext.Current.Session["usuario"].ToString(), HttpContext.Current.Session["clave"].ToString(), HttpContext.Current.Session["path"].ToString());

            Dictionary<string, object> parametros = (Dictionary<string, object>)parameters;
            Dictionary<string, object> salida = new Dictionary<string, object>();

            #region columnas
            object[] columnasO = (object[])parametros["columns"];
            #endregion

            #region orden
            object[] ordenO = (object[])parametros["order"];
            Dictionary<string, object> orden = (Dictionary<string, object>)ordenO[0];
            int columnaOrden = (int)orden["column"];
            string direccionOrden = (string)orden["dir"];
            Dictionary<string, object> nombreColOrden = (Dictionary<string, object>)columnasO[columnaOrden];
            #endregion

            #region busqueda
            Dictionary<string, object> busquedaO = (Dictionary<string, object>)parametros["search"];
            string strbusqueda = busquedaO["value"].ToString().Replace(' ', '%');
            #endregion

            #region consultaPrincipal
            if (!objLinks.getLinksVerifica(
                Int32.Parse(parametros["start"].ToString()),
                Int32.Parse(parametros["length"].ToString()),
                nombreColOrden["name"].ToString(),//nombre columna orden
                direccionOrden, //direccion orden
                strbusqueda, //texto a buscar
                columnasO //busqueda por columnas
                ))
                throw new Exception(objLinks.strReturnMessage);
            #endregion

            #region filtros
            /*List<Dictionary<string, object>> puestos = new List<Dictionary<string, object>>();
            Dictionary<string, object> puesto = new Dictionary<string, object>();

            List<Dictionary<string, object>> unidades = new List<Dictionary<string, object>>();
            Dictionary<string, object> unidad = new Dictionary<string, object>();

            List<Dictionary<string, object>> jefes = new List<Dictionary<string, object>>();
            Dictionary<string, object> jefe = new Dictionary<string, object>();

            List<Dictionary<string, object>> estados = new List<Dictionary<string, object>>();
            Dictionary<string, object> estado = new Dictionary<string, object>();*/

            /*

            foreach (DataRow dr in objControllerPuesto.dsReturn.Tables["filtro-puesto"].Rows)
            {
                puesto = new Dictionary<string, object>();
                puesto.Add("value", dr[0]);
                puesto.Add("label", dr[1]);
                puestos.Add(puesto);
            }

            foreach (DataRow dr in objControllerPuesto.dsReturn.Tables["filtro-unidad"].Rows)
            {
                unidad = new Dictionary<string, object>();
                unidad.Add("value", dr[0]);
                unidad.Add("label", dr[1]);
                unidades.Add(unidad);
            }

            foreach (DataRow dr in objControllerPuesto.dsReturn.Tables["filtro-jefe"].Rows)
            {
                jefe = new Dictionary<string, object>();
                jefe.Add("value", dr[0]);
                jefe.Add("label", dr[1]);
                jefes.Add(jefe);
            }            
            //filtro estado 
            foreach (DataRow dr in objControllerPuesto.dsReturn.Tables["filtro-estado"].Rows)
            {
                estado = new Dictionary<string, object>();
                estado.Add("value", "'" + dr[0].ToString().Trim() + "'");
                estado.Add("label", Estado(dr[0].ToString()));
                estados.Add(estado);
            }*/
            #endregion

            #region recorsTotal
            int recordsTotal = Int32.Parse(objLinks.dsReturn.Tables["links-count"].Rows[0][0].ToString());
            #endregion

            #region recordsFiltered
            int recordsFiltered = Int32.Parse(objLinks.dsReturn.Tables["links-count-search"].Rows[0][0].ToString());
            #endregion

            #region join
            DataSet ds = new DataSet("DataSet");
            ds.Tables.Add(objLinks.dsReturn.Tables["links"].Copy());

            //join con sql

            #endregion

            #region construccion Arreglo Data
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;

            foreach (DataRow dr in ds.Tables["links"].Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in ds.Tables["links"].Columns)
                {
                    //if (col.ColumnName == "IND_ESTADO")
                    //    dr[col] = Estado(dr[col].ToString());
                    row.Add(col.ColumnName, dr[col]);
                }
                rows.Add(row);

            }
            #endregion

            salida.Add("draw", parametros["draw"]);
            salida.Add("recordsTotal", recordsTotal);
            salida.Add("recordsFiltered", recordsFiltered);
            salida.Add("data", rows);
            //salida.Add("yadcf_data_9", puestos);

            string serializado = js.Serialize(salida);
            return serializado;
        }
        catch (Exception ex)
        {
            if (ex.Message == "401")
                throw new Exception("401");

            return js.Serialize(ex.Message);
        }

    }



    //------------------------------------------------------------------------------------
    //20240529.ini Metodo privado para crear conexion http para obtener url acortado
    #region Metodo Acortado Link Rebrandly RestClient
    private IRestResponse CreateShortUrlBrandly(object model)
    {
        try
        {
            //TodO: 20240529.ini Cambio en request para acortar link
            //var client = new RestClient("https://lc.promerica.com.gt"); LDMR dinamico
            var client = new RestClient(strServer);
            
            //var client = new RestClient("https://enterprise-api.rebrandly.com");
            //var client = new RestClient("https://api.tinyurl.com");
            var proxy = new WebProxy("webproxy.promerica.com.gt", 9095);
            proxy.Credentials = new NetworkCredential("servicio_bo", "B4nc0Pr0m3r!c4");
            client.Proxy = proxy;
            client.Timeout = -1;
            var request = new RestRequest("/shortlinks/shorten", Method.POST);
            //var request = new RestRequest("/v1/links", Method.POST);
            //var request = new RestRequest("/create", Method.POST);
            request.RequestFormat = DataFormat.Json;
            //request.AddHeader("Authorization", "Bearer 8rkNKUWqtHU9bRomng9b3xJd2m87kBXWdki6QlN7HIXefVbRiowOPTYYOHoZ");
            request.AddHeader("Content-Type", "application/json");
            //request.AddHeader("apikey", "ApiSL2025"); //"862b888158024429bbdff231e4c600ca" ldmr 23/02/2026
            request.AddHeader("apikey", apikey); //"862b888158024429bbdff231e4c600ca"
            //request.AddHeader("workspace", "1cf40cc1d152469abd784d23bb664e64");
            request.AddJsonBody(model);
            //certificado de seguridad
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            IRestResponse response = client.Execute(request);

            return response;
            //TodO: 20240529.ini Cambio en request para acortar link
        }
        catch (Exception)
        {
            return null;
        }
    }
    #endregion
    //20240529.ini Metodo privado para crear conexion http para obtener url acortado

    private IRestResponse ShortUrlReport(int id)
    {
        try
        {
            //TodO: 20240529.ini Cambio en request para acortar link
            //var client = new RestClient("https://enterprise-api.rebrandly.com");
            var client = new RestClient("https://api.tinyurl.com");
            var proxy = new WebProxy("webproxy.promerica.com.gt", 9095);
            proxy.Credentials = new NetworkCredential("servicio_bo", "B4nc0Pr0m3r!c4");
            client.Proxy = proxy;
            client.Timeout = -1;
            //var request = new RestRequest("/v1/links", Method.PUT);
            var request = new RestRequest("/bulk/"+id.ToString()+"/report", Method.GET);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Authorization", "Bearer 8rkNKUWqtHU9bRomng9b3xJd2m87kBXWdki6QlN7HIXefVbRiowOPTYYOHoZ");
            request.AddHeader("Content-Type", "application/json");

            //request.AddHeader("apikey", strApiKey);
            //request.AddHeader("workspace", "1cf40cc1d152469abd784d23bb664e64");

            //request.AddJsonBody(model);
            //certificado de seguridad
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            IRestResponse response = client.Execute(request);

            return response;
            //TodO: 20240529.ini Cambio en request para acortar link
        }
        catch (Exception)
        {
            return null;
        }
    }
    private IRestResponse StatusUrlReport(int id)
    {
        try
        {
            //TodO: 20240529.ini Cambio en request para acortar link
            //var client = new RestClient("https://enterprise-api.rebrandly.com");
            var client = new RestClient("https://api.tinyurl.com");
            var proxy = new WebProxy("webproxy.promerica.com.gt", 9095);
            proxy.Credentials = new NetworkCredential("servicio_bo", "B4nc0Pr0m3r!c4");
            client.Proxy = proxy;
            client.Timeout = -1;
            //var request = new RestRequest("/v1/links", Method.PUT);
            var request = new RestRequest("/bulk/" + id.ToString() + "/status", Method.GET);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Authorization", "Bearer 8rkNKUWqtHU9bRomng9b3xJd2m87kBXWdki6QlN7HIXefVbRiowOPTYYOHoZ");
            request.AddHeader("Content-Type", "application/json");

            //request.AddHeader("apikey", strApiKey);
            //request.AddHeader("workspace", "1cf40cc1d152469abd784d23bb664e64");

            //request.AddJsonBody(model);
            //certificado de seguridad
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            IRestResponse response = client.Execute(request);

            return response;
            //TodO: 20240529.ini Cambio en request para acortar link
        }
        catch (Exception)
        {
            return null;
        }
    }

    //20240529.ini Metodo privado para crear conexion http para obtener url acortado verbo PUT
    #region Metodo Acortado Link Rebrandly RestClient
    private IRestResponse CreateShortUrlBrandlyPut(object model)
    {
        try
        {
            //TodO: 20240529.ini Cambio en request para acortar link
            //var client = new RestClient("https://enterprise-api.rebrandly.com");
            //var client = new RestClient("https://api.tinyurl.com");
            //var client = new RestClient("https://lc.promerica.com.gt");
            var client = new RestClient(strServer);
            var proxy = new WebProxy("webproxy.promerica.com.gt", 9095);
            proxy.Credentials = new NetworkCredential("servicio_bo", "B4nc0Pr0m3r!c4");
            client.Proxy = proxy;
            client.Timeout = -1;
            //var request = new RestRequest("/v1/links", Method.PUT);
            //var request = new RestRequest("/bulk", Method.POST);
            var request = new RestRequest("/shortlinks/shorten", Method.POST);
            request.RequestFormat = DataFormat.Json;
            //request.AddHeader("Authorization", "Bearer 8rkNKUWqtHU9bRomng9b3xJd2m87kBXWdki6QlN7HIXefVbRiowOPTYYOHoZ");
            request.AddHeader("Content-Type", "application/json");
            //request.AddHeader("apikey", "ApiSL2025");
            request.AddHeader("apikey", apikey);
            //request.AddHeader("workspace", "1cf40cc1d152469abd784d23bb664e64");

            request.AddJsonBody(model);
            //certificado de seguridad
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            IRestResponse response = client.Execute(request);

            return response;
            //TodO: 20240529.ini Cambio en request para acortar link
        }
        catch (Exception)
        {
            return null;
        }
    }
    #endregion
    //20240529.ini Metodo privado para crear conexion http para obtener url acortado

    //27oct2024.ini Metodo privado para crear conexion http para obtener url acortado verbo PUT de forma asincrona
    #region Metodo Acortado Link Rebrandly RestClient
    private async Task<IRestResponse> CreateShortUrlBrandlyPut_Async(object model)
    {
        #region
        try
        {
            //TodO: 20240529.ini Cambio en request para acortar link
            var client = new RestClient("https://enterprise-api.rebrandly.com");
            var proxy = new WebProxy("webproxy.promerica.com.gt", 9095);
            proxy.Credentials = new NetworkCredential("servicio_bo", "B4nc0Pr0m3r!c4");
            client.Proxy = proxy;
            client.Timeout = -1;
            var request = new RestRequest("/v1/links", Method.PUT);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("apikey", apikey);
            request.AddHeader("workspace", "1cf40cc1d152469abd784d23bb664e64");

            request.AddJsonBody(model);
            //certificado de seguridad
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            IRestResponse response = await client.ExecuteTaskAsync(request);

            return response;
            //TodO: 20240529.ini Cambio en request para acortar link
        }
        catch (Exception)
        {
            return null;
        }
        #endregion
    }
    #endregion
    //27oct2024.Fin



}
