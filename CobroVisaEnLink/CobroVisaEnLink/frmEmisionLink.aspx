<%@ Page Title="" Language="C#" MasterPageFile="~/mpaSistema.master" AutoEventWireup="true" CodeFile="frmEmisionLink.aspx.cs" Inherits="frmEmisionLink" %>

<%@ MasterType VirtualPath="~/mpaSistema.master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="Server">
    <link rel="stylesheet" type="text/css" href="css/validationEngine.jquery.css" media="screen" />
    <link rel="stylesheet" type="text/css" href="css/chosen.min.css" media="screen" />
    <link href="css/tooltip.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="Server">
    <div class="container">
        <div class="row">
            <div class="col-md-12" style="padding: 15px">
                <div class="row">
                    <div class="col-md-12 ">
                        <div class="panel panel-success">
                            <div class="panel-heading">Informaci&oacute;n del Producto</div>
                            <div class="panel-body">
                                <asp:UpdatePanel runat="server" ID="udpBuscarCta">
                                    <ContentTemplate>
                                        <div class="form-group form-group-sm">
                                            <label for="mainContent_txtCuenta" class="col-md-3 col-md-offset-1 col-xs-3 control-label" />Número de cuenta TC/Préstamo:</label>
                                            <div class="col-md-4 col-xs-9">
                                                <asp:TextBox ID="txtCuenta" runat="server" CssClass="form-control validate[required, custom[onlyNumberSp]]"></asp:TextBox>
                                            </div>
                                            <div class="col-md-4">
                                                <asp:Button ID="btnBuscarCta" runat="server" Text="Buscar Producto" CssClass="btn btn-sm btn-info" UseSubmitBehavior="false" OnClick="btnBuscarCta_Click" />
                                            </div>
                                        </div>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                                <div id="divStep2">
                                    <asp:UpdatePanel runat="server" ID="udpCliente">
                                        <ContentTemplate>
                                            <div class="row">
                                                <div class="col-md-4 text-right">
                                                    <label>Cliente:</label>
                                                </div>
                                                <div class="col-md-8">
                                                    <asp:Label runat="server" ID="lblCliente"></asp:Label>
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="col-md-4 text-right">
                                                    <label>Correo Electr&oacute;nico:</label>
                                                </div>
                                                <div class="col-md-8">
                                                    <asp:Label runat="server" ID="lblCorreo"></asp:Label>
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="col-md-4 text-right">
                                                    <label>Tel&eacute;fono:</label>
                                                </div>
                                                <div class="col-md-8">
                                                    <asp:Label runat="server" ID="lblTelefono"></asp:Label>
                                                </div>
                                            </div>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                    <div class="form-group form-group-sm">
                                        <asp:UpdatePanel runat="server" ID="UpdatePanel2">
                                            <ContentTemplate>
                                                <div class="row">
                                                    <asp:Label ID="Label5" runat="server" CssClass="col-md-3 col-md-offset-1 col-xs-3 control-label" Text="Monto o Cantidad:"></asp:Label>
                                                    <div class="relative">
                                                        <div id="monto_container" class="col-md-4 col-xs-9">
                                                            <asp:TextBox ID="txtMonto" runat="server" CssClass="form-control validate[required, custom[onlyCurrencySp]]" AutoPostBack="true" OnTextChanged="OnBlur" onfocus="InicializaMensaje();"></asp:TextBox>
                                                        </div>
                                                    </div>
                                                    <div class="col-md-4">
                                                        <asp:CheckBox ID="chkPago" runat="server" Text="Pagar en Dolares($)" Checked="false" AutoPostBack="true" OnCheckedChanged="chkPago_CheckedChanged" />
                                                    </div>
                                                </div>
                                            </ContentTemplate>
                                        </asp:UpdatePanel>
                                        <asp:HiddenField ID="hfdNotificacion" runat="server" />
                                    </div>
                                    <div class="form-group form-group-sm">
                                        <asp:UpdatePanel runat="server" ID="updTipoLink">
                                            <ContentTemplate>
                                                <div class="row">
                                                    <asp:Label ID="Label4" runat="server" CssClass="col-md-3 col-md-offset-1 col-xs-3 control-label" Text="Programación o enrolamiento de link:"></asp:Label>
                                                    <div class="col-md-4">
                                                        <asp:DropDownList ID="ddlTipoLink" runat="server" onchange="MostrarParametros()">
                                                            <asp:ListItem Text="Si" Value="1"></asp:ListItem>
                                                            <asp:ListItem Text="No" Value="2" Selected="True"></asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>
                                                </div>
                                            </ContentTemplate>
                                        </asp:UpdatePanel>
                                    </div>
                                    <div class="form-group form-group-sm">
                                        <div id="divProgra" class="row" style="display: none">
                                            <asp:Label ID="Label1" runat="server" CssClass="col-md-3 col-md-offset-1 col-xs-3 control-label" Text="Día del mes a generar link:"></asp:Label>
                                            <div class="col-md-4">
                                                <asp:DropDownList ID="ddlDia" runat="server"></asp:DropDownList>
                                            </div>
                                        </div>
                                        <asp:UpdatePanel runat="server" ID="updObtenerLink">
                                            <ContentTemplate>
                                                <div id="divManual" class="row">
                                                    <asp:Label ID="Label2" runat="server" CssClass="col-md-3 col-md-offset-1 col-xs-3 control-label" Text="Link:"></asp:Label>
                                                    <asp:HyperLink ID="lnkURL" Text="Pendiente" runat="server" Target="_blank" />
                                                    <div class="col=md-4">
                                                        <asp:Button ID="btnObtenerLink" runat="server" Text="Obtener Link" CssClass="btn btn-sm btn-info" OnClick="btnObtenerLink_Click" OnClientClick="javascript:if (!validar()) return false;" />
                                                    </div>
                                                </div>
                                            </ContentTemplate>
                                        </asp:UpdatePanel>
                                    </div>
                                </div>
                                <div class="modal fade" tabindex="-1" id="divNotificaAlterno">
                                    <div class="modal-dialog" role="document">
                                        <div class="modal-content">
                                            <div class="modal-header">
                                                <h4 class="modal-title">No se pudo acortar link ¿Desea enviar link largo por correo?</h4>
                                            </div>
                                            <div class="modal-body">
                                                <div class="container">
                                                    <div class="row">
                                                        <div class="col-md-5 text-right">
                                                            <label>Seleccione Opci&oacute;n:</label>
                                                        </div>
                                                        <div class="col-md-7">
                                                            <asp:DropDownList ID="ddlDatoCorreo" runat="server">
                                                                <asp:ListItem Text="Utilizar Datos Default" Value="1" Selected="True"></asp:ListItem>
                                                                <asp:ListItem Text="Editar información de envío" Value="2"></asp:ListItem>
                                                            </asp:DropDownList>
                                                        </div>
                                                    </div>
                                                    <div class="row" style="margin-top: 15px">
                                                        <div id="divMailDefault">
                                                            <div class="col-md-5 text-right">
                                                                <label>Correo Default:</label>
                                                            </div>
                                                            <div class="col-md-7">
                                                                <asp:Label runat="server" ID="lblCorreoAlterno"></asp:Label>
                                                            </div>
                                                        </div>
                                                    </div>
                                                    <div class="row">
                                                        <div id="divMailedit" style="display: none">
                                                            <div class="col-md-5 text-right">
                                                                <label>Correo Electr&oacute;nico:</label>
                                                            </div>
                                                            <div class="col-md-7">
                                                                <asp:TextBox ID="txtMailAlterno" runat="server" CssClass="form-control validate[custom[email]]"></asp:TextBox>
                                                            </div>
                                                        </div>
                                                    </div>
                                                    <div class="row" style="margin-top: 15px">
                                                        <div class="col-md-4"></div>
                                                        <div class="col-md-4">
                                                            <asp:Button ID="Button1" runat="server" Text="Enviar Correo" CssClass="btn btn-sm btn-info" OnClick="btnEnvioCorreoLink_Click"  />
                                                            <%--<asp:LinkButton runat="server" ID="lnkEnvioCorreo" CommandName="Select" OnClientClick="$('#divNotificaAlterno').modal('hide');" CssClass="btn btn-sm btn-primary">Enviar Correo</asp:LinkButton>--%>
                                                        </div>
                                                    </div>
                                                    <div class="row" style="margin-top: 15px">
                                                        <div class="col-md-12" style="font-size: 10px">
                                                            *Esta accci&oacute;n se di&oacute; por problemas en el servicio de acortar link.<br />
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="modal-footer">
                                                <%--<button type="button" class="btn btn-default" data-dismiss="modal" id="btnEnvio">Enviar</button>--%>
                                            </div>
                                        </div>
                                    </div>
                                </div> 
                                <div class="modal fade" tabindex="-1" id="divBusquedaCuenta">
                                    <div class="modal-dialog modal-lg" role="document">
                                        <div class="modal-content">
                                            <div class="modal-header">
                                                <h4>Busqueda de Cuenta</h4>
                                            </div>
                                            <div class="modal-body">
                                                <asp:UpdatePanel runat="server" ID="UpdatePanel1">
                                                    <ContentTemplate>
                                                        <div class="row" style="margin-top: 15px">
                                                            <div class="col-md-12">
                                                                <asp:GridView runat="server" ID="gvCuentas" AutoGenerateColumns="False" AllowPaging="true" PageSize="10" CssClass="table table-striped display compact"
                                                                    GridLines="None" EmptyDataText="No hay resultados para mostrar." ShowHeader="true" OnSelectedIndexChanged="gvCuentas_SelectedIndexChanged" OnPageIndexChanging="gvCuentas_PageIndexChanging" PagerStyle-CssClass="pagination-ys">
                                                                    <Columns>
                                                                        <asp:BoundField HeaderText="Numero de Cuenta" DataField="NUM_CUENTA" />
                                                                        <asp:BoundField HeaderText="Tipo Cuenta" DataField="TIPO" />
                                                                        <asp:BoundField HeaderText="Estado Cuenta" DataField="ESTADO" />
                                                                        <asp:TemplateField ShowHeader="true">
                                                                            <ItemTemplate>
                                                                                <asp:LinkButton runat="server" ID="lnkSelCuenta" CommandName="Select" OnClientClick="$('#divBusquedaCuenta').modal('hide'); $('#divStep2').show(); SetTxtFocus();" CssClass="btn btn-sm btn-primary">Seleccionar</asp:LinkButton>
                                                                            </ItemTemplate>
                                                                        </asp:TemplateField>
                                                                    </Columns>
                                                                </asp:GridView>
                                                            </div>
                                                        </div>
                                                        <div class="row" style="margin-top: 15px">
                                                            <div class="col-md-12" style="font-size: 10px">
                                                                *Solo se muestran resultados de cuentas activas, vigentes y en proceso de cancelaci&oacute;n para el cliente selecionado.<br />
                                                            </div>
                                                        </div>
                                                    </ContentTemplate>
                                                </asp:UpdatePanel>
                                            </div>
                                            <div class="modal-footer">
                                                <button type="button" class="btn btn-default" data-dismiss="modal" id="btnNoCta">Cancelar</button>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div id="divEnvio" class="container">
                                    <div class="row">
                                        <div class="panel panel-info">
                                            <div class="panel-heading">
                                                <h2 class="panel-title">Informaci&oacute;n de env&iacute;o</h2>
                                            </div>
                                            <div class="panel-body">
                                                <div class="col-md-3 col-md-offset-1 col-xs-3">
                                                    <asp:Panel ID="cfgEnvio" runat="server">
                                                        <div class="row">
                                                            <asp:RadioButton ID="rdbSMS" GroupName="grpEnvio" runat="server" Text="Envío SMS" Checked="false" TextAlign="Right" onclick="ShowHideDiv()" />
                                                        </div>
                                                        <div class="row">
                                                            <asp:RadioButton ID="rdbsMail" GroupName="grpEnvio" runat="server" Text="Envío correo electrónico" Checked="false" TextAlign="Right" onclick="ShowHideDiv()" />
                                                        </div>
                                                    </asp:Panel>
                                                </div>
                                                <div class="col-xs-4">
                                                    <asp:DropDownList ID="ddlDatosCliente" runat="server">
                                                        <asp:ListItem Text="Utilizar Datos Default" Value="1" Selected="True"></asp:ListItem>
                                                        <asp:ListItem Text="Editar información de envío" Value="2"></asp:ListItem>
                                                    </asp:DropDownList>
                                                </div>
                                                <div id="divTelefono" class="col-xs-3" style="display: none">
                                                    <asp:Label ID="Label3" runat="server" Text="Teléfono:"></asp:Label>
                                                    <asp:TextBox ID="txtTelefono" runat="server" CssClass="form-control validate[custom[phone]]"></asp:TextBox>
                                                </div>
                                                <div id="divMail" class="col-xs-3" style="display: none">
                                                    <asp:Label ID="Label6" runat="server" Text="Correo Electrónico:"></asp:Label>
                                                    <asp:TextBox ID="txtMail" runat="server" CssClass="form-control validate[custom[email]]"></asp:TextBox>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <asp:Panel ID="grbCambios" runat="server">
                                    <div class="col-md-11 col-xs-9 text-right">
                                        <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn btn-default" CausesValidation="False" OnClick="btnCancelar_Click" UseSubmitBehavior="False" />
                                        <asp:Button ID="btnGuardar" runat="server" Text="Guardar" CssClass="btn btn-success" OnClick="btnGuardar_Click" />
                                    </div>
                                </asp:Panel>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <script type="text/javascript" charset="UTF-8" src="js/chosen.jquery-1.6.2.min.js"></script>
    <script type="text/javascript">
        $(function () {
            MostrarParametros();

            $("select[id$=ddlTipoLink]").change(function () {
                MostrarParametros();
            });

            $("select[id$=ddlDatosCliente]").change(function () {
                ShowHideDiv()
            });

            $("select[id$=ddlDatoCorreo]").change(function () {
                MuestraObjetos()
            });
        });
        function InicializaMensaje() {
            document.getElementById('toolTipDiv').className = 'idleToolTip';
        }
        function DisableDDL() {
            document.getElementById("<%=ddlTipoLink.ClientID%>").disabled = false;
        }
        function SetTxtFocus() {
            //document.getElementById("<%=txtMonto.ClientID%>").focus();
        }
        function validar() {
            console.log("entra a validar")
            var result = $('#aspnetForm').validationEngine('validate');
            console.log("result ",result)
            return result;
        }
        function MuestraObjetos() {
            if ($("select[id$=ddlDatoCorreo]").val() == "2") {
                $("#divMailedit").show();
                $("#divMailDefault").hide();
            }
            else {
                $("#divMailDefault").show();
                $("#divMailedit").hide();
            }
        }
        function ShowHideDiv() {
            var rdbSMS = document.getElementById("<%=rdbSMS.ClientID%>");
            var rdbsMail = document.getElementById("<%=rdbsMail.ClientID%>");
            var divTelefono = document.getElementById("divTelefono");
            var divMail = document.getElementById("divMail");

            if ($("select[id$=ddlDatosCliente]").val() == "2") {
                divTelefono.style.display = rdbSMS.checked ? "block" : "none";
                divMail.style.display = rdbsMail.checked ? "block" : "none";
            }

            if ($("select[id$=ddlDatosCliente]").val() == "1") {
                divTelefono.style.display = "none";
                divMail.style.display = "none";
            }
        }
        function MostrarParametros() {
            if ($("select[id$=ddlTipoLink]").val() == "1") {
                $("#divProgra").show();
                $("#divEnvio").show();
                $("#divManual").hide();
            }
            else {
                $("#divProgra").hide();
                $("#divEnvio").hide();
                $("#divManual").show();
            }
        }
        function cargar_scripts() {
            $("select").chosen({
                width: "100%",
                no_results_text: "No se encontraron resultados."
            });

            $("#btnBuscaarCli").click(function () {
                $("#divBusquedaCliente").modal();
            });
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="footerContent" runat="Server">
    <script type="text/javascript" charset="UTF-8" src="js/jquery.validationEngine.js"></script>
    <script type="text/javascript" charset="UTF-8" src="js/jquery.validationEngine-es.js"></script>
    <script type="text/javascript">
        function ShowMessage(message) {
            //alert_container
            $('#monto_container').append('<div id="toolTipDiv" class="tool_tip">' + message + '</div>');
            //'<div id="toolTipDiv" class="activeToolTip">'
            //$('#alert_container').append('<div id="alert_div" style="margin: 0 0.5%; -webkit-box-shadow: 3px 4px 6px #999;" class="alert fade in ' + cssclass + '"><a href="#" class="close" data-dismiss="alert" aria-label="close">&times;</a><strong>' + messagetype + '!</strong> <span>' + message + '</span></div>');
        }
        $(document).ready(function () {
            $("#divStep2").hide();
            $("#divEnvio").hide();
            $(window).keydown(function (event) {
                if (event.keyCode == 13) {
                    event.preventDefault();
                    return false;
                }
            });
            $("#aspnetForm").validationEngine({
                validateNonVisibleFields: true,
                prettySelect: true,
                useSuffix: "_chosen"
            });

        });
    </script>
</asp:Content>
