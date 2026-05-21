using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Drawing.Imaging;

public partial class frmParametros : System.Web.UI.Page
{
    private string error;
    protected void Page_PreLoad(object sender, EventArgs e)
    {
        Master.strMensajeError = "";
        Master.strMensajeErrorModal = "";
        Master.strMensajeConfirmacion = "";
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            #region validacion de permisos
            Response.Cookies.Add(new HttpCookie("returnUrl", Request.Url.PathAndQuery));
            if (Session["usuario"] == null)
                Response.Redirect("Default.aspx");

            controllerMenu objControllerMenu = new controllerMenu(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());
            if (!objControllerMenu.verificarMenuItem(Session["usuario"].ToString(), "9507"))
                Response.Redirect("frmIndex.aspx");
            #endregion

            #region inicializacion de pagina
            Master.strPagina = "Editar parámetros del Sistema";
            string cod_transpo = Request.QueryString["tra"];
            if (cod_transpo != null)
                Master.strPagina = "Editar Transportadora";
            #endregion      

            if (!this.IsPostBack)
            {
                //debo llenar los valores de la lista 

                for (int i = 0; i < 24; i++)
                    ddlHora.Items.Add(new ListItem(i.ToString(), i.ToString()));

                if (!cargarParametros())
                    throw new Exception(error);
            }
        }
        catch (Exception ex)
        {
            Master.strMensajeError = ex.Message;
        }
    }
    public bool cargarParametros()
    {
        try
        {
            controllerSitio objcontrollerSitio = new controllerSitio(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());

            if (!objcontrollerSitio.getParametros())
                throw new Exception(objcontrollerSitio.strReturnMessage);

            if (objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows.Count > 0)
            {
                #region parametros de revision

                switch (objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows[0]["FRE_REV_AUTO"].ToString())
                {
                    case "U":
                        rdbUnicaR.Checked = true;
                        break;
                    case "D":
                        rdbDiarioR.Checked = true;
                        break;
                    case "S":
                        rdbDiarioR.Checked = true;
                        break;
                };

                ddlHora.SelectedValue = objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows[0]["FRE_REV_HRS_REP"].ToString();
                #endregion

                #region parametros de generacion
                switch (objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows[0]["FRE_GEN_LINK"].ToString())
                {
                    case "U":
                        rdbUnicaG.Checked = true;
                        break;
                    case "D":
                        rdbDiarioG.Checked = true;
                        break;
                    case "S":
                        rdbSemanalG.Checked = true;
                        break;
                };

                txtTime.Text= objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows[0]["FRE_GEN_HORA"].ToString();
                #endregion

                txtTrxTC.Text = objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows[0]["TC_TIP_TRANSAC"].ToString();
                txtSubTrx.Text = objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows[0]["TC_SUBTIP_TRANS"].ToString();
                txtCuentaQ.Text = objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows[0]["NUM_CTA_CONTA_QTZ"].ToString();
                txtCuentaD.Text = objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows[0]["NUM_CTA_CONTA_DOL"].ToString();
                txtAgencia.Text = objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows[0]["COD_AGENCIA"].ToString();
                txtTipoTC.Text = objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows[0]["COD_TIPO_TC"].ToString();
                txtSubtipoTC.Text = objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows[0]["COD_SUBTIPO_TC"].ToString();
                txtDesPagoTC.Text = objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows[0]["DES_TRANSACCION"].ToString();
                txtTipoPR.Text = objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows[0]["COD_TIPO_PR"].ToString();
                txtSubtipoPR.Text = objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows[0]["COD_SUBTIPO_PR"].ToString();
                txtDepartamento.Text = objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows[0]["COD_DEPARTAMENTO"].ToString();
                txtCampanaPR.Text = objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows[0]["COD_DEPTO_PR"].ToString();

                #region obtener la imagen
                byte[] byteArray = (Byte[]) objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows[0]["API_IMAGEN"];
                //Convert the Byte Array to Base64 Encoded string.
                string base64String = Convert.ToBase64String(byteArray, 0, byteArray.Length);

                //***Save Base64 Encoded string as Image File***//
                imgPreview.ImageUrl = "data:image/png;base64," + base64String;
                imgPreview.Visible = true;
                #endregion

                #region parametros de correo y sms
                txtRemitente.Text = objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows[0]["MSG_REMITENTE"].ToString();
                byte[] byteHeader = (Byte[])objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows[0]["MSG_HEADER"];
                txtHeader.Text = System.Text.Encoding.Default.GetString(byteHeader);
                byte[] byteFooter = (Byte[])objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows[0]["MSG_FOOTER"];
                txtFooter.Text = System.Text.Encoding.Default.GetString(byteFooter);
                txtSMS.Text = objcontrollerSitio.dsReturn.Tables["param_sistema"].Rows[0]["MSG_SMS"].ToString();
                #endregion
            }

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
    protected void btnCancelar_Click(object sender, EventArgs e)
    {
        try
        {
            Session["PageRedirect"] = true;
            Response.Redirect("frmIndex.aspx", false);
            //Response.Redirect("frmTransportadoras.aspx", false);
        }
        catch (Exception ex)
        {
            Master.strMensajeError = ex.Message;
        }
    }
    protected void btnGuardar_Click(object sender, EventArgs e)
    {
        try
        { 
            controllerSitio objcontrollerSitio = new controllerSitio(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());

            clsParametros objParametros = new clsParametros();

            //frecuencia revision
            if (rdbUnicaR.Checked)
                objParametros.FRE_REV_AUTORIZACION = "U";
            else if (rdbDiarioR.Checked)
                objParametros.FRE_REV_AUTORIZACION = "D";
            else if (rdbSemanalR.Checked)
                objParametros.FRE_REV_AUTORIZACION = "S";

            objParametros.FRE_REV_HRS_REPETIR = ddlHora.SelectedValue;

            //frecuencia generacion
            if (rdbUnicaG.Checked)
                objParametros.FRE_GEN_LINK = "U";
            else if (rdbDiarioG.Checked)
                objParametros.FRE_GEN_LINK = "D";
            else if (rdbSemanalG.Checked)
                objParametros.FRE_GEN_LINK = "S";

            objParametros.FRE_GEN_HORA = txtTime.Text;

            //tipo de transacciones
            objParametros.TC_TIP_TRANSAC = txtTrxTC.Text;
            objParametros.TC_SUBTIP_TRANS = txtSubTrx.Text;
            objParametros.COD_AGENCIA = txtAgencia.Text;

            //imagen 
            objParametros.API_IMAGEN = imgPreview.ImageUrl;

            //formato correo 
            objParametros.MSG_REMITENTE = txtRemitente.Text;
            objParametros.MSG_HEADER = txtHeader.Text;
            objParametros.MSG_FOOTER = txtFooter.Text;
            //formato SMS
            objParametros.MSG_SMS = txtSMS.Text;

            //cuentas contables 
            objParametros.NUM_CTA_CONTA_QTZ = txtCuentaQ.Text;
            objParametros.NUM_CTA_CONTA_DOL = txtCuentaD.Text;

            //parametros de bitacora
            objParametros.COD_TIPO_TC = txtTipoTC.Text;
            objParametros.COD_SUBTIPO_TC = txtSubtipoTC.Text;
            objParametros.DES_TRANSACCION = txtDesPagoTC.Text;
            objParametros.COD_TIPO_PR = txtTipoPR.Text;
            objParametros.COD_SUBTIPO_PR = txtSubtipoPR.Text;

            objParametros.COD_DEPARTAMENTO = txtDepartamento.Text;
            objParametros.COD_DEPTO_PR = txtCampanaPR.Text;

            if (((Button)sender).ID == "btnGuardar")
            {

                if (!objcontrollerSitio.updateParametros(objParametros))
                    throw new Exception(objcontrollerSitio.strReturnMessage);

                //REGISTRA BITACORA
                RegistraBitacora();

                Master.strMensajeConfirmacion = "Parametros Modificados!!!";
                Session["PageRedirect"] = true;
                Response.Redirect("frmIndex.aspx", false);
            }

        }
        catch (Exception ex)
        {
            Master.strMensajeError = ex.Message;
        }
    }
    protected void btnGenerar_Click(object sender, EventArgs e)
    {       
        //txtClave.Text = GenerarPalabra();
        
    }
    protected void btnUpload_Click(object sender, EventArgs e)
    {
        //***Convert Image File to Base64 Encoded string***//

        //Read the uploaded file using BinaryReader and convert it to Byte Array.
        BinaryReader br = new BinaryReader(flUpload.PostedFile.InputStream);
        byte[] bytes = br.ReadBytes((int)flUpload.PostedFile.InputStream.Length);

        //Convert the Byte Array to Base64 Encoded string.
        string base64String = Convert.ToBase64String(bytes, 0, bytes.Length);

        //***Save Base64 Encoded string as Image File***//

        imgPreview.ImageUrl = "data:image/png;base64," + base64String;
        imgPreview.Visible = true;

        ////Convert Base64 Encoded string to Byte Array.
        //byte[] imageBytes = Convert.FromBase64String(base64String);

        ////Save the Byte Array as Image File.
        //string filePath = Server.MapPath("~/Files/" + Path.GetFileName(FileUpload1.PostedFile.FileName));
        //File.WriteAllBytes(filePath, imageBytes);
    }
    void RegistraBitacora()
    {
        try
        {
            controllerSitio objControllerSitio = new controllerSitio(Session["usuario"].ToString(), Session["clave"].ToString(), Session["path"].ToString());
            clsBitacora objBitacora = new clsBitacora();

            objBitacora.cod_link = "";
            objBitacora.cod_parametro = "";
            objBitacora.descripcion = "Se modificaron parametros del sistema.";
            objBitacora.tip_procesamiento = "S"; //SISTEMA

            if (!objControllerSitio.registraBitacora(objBitacora))
                throw new Exception(objControllerSitio.strReturnMessage);
        }
        catch (Exception ex)
        {
            Master.strMensajeError = ex.Message;
        }
    }
}