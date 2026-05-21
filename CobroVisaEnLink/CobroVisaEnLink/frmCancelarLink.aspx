<%@ Page Title="" Language="C#" MasterPageFile="~/mpaSistema.master" AutoEventWireup="true" CodeFile="frmCancelarLink.aspx.cs" Inherits="frmCancelarLink" %>

<%@ MasterType VirtualPath="~/mpaSistema.master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="Server">
    <link rel="stylesheet" type="text/css" href="css/validationEngine.jquery.css" media="screen" />
    <link rel="stylesheet" type="text/css" href="css/chosen.min.css" media="screen" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="Server">
    <div class="container">
        <div class="row">
            <div class="col-md-12" style="padding: 15px">
                <div class="row">
                    <div class="col-md-12 ">
                        <div class="panel panel-success">
                            <div class="panel-heading">Desactivaci&oacute;n Link Autom&aacute;tico</div>
                            <div class="panel-body">
                                <div class="container">
                                    <asp:Panel ID="cfgBusqueda" runat="server">
                                        <div class="row">
                                            <asp:RadioButton ID="rdbProducto" GroupName="grpBusqueda" runat="server" Text="Búsqueda por producto" Checked="false" TextAlign="Right" onclick="ShowHideDiv()" />
                                        </div>
                                        <div class="row">
                                            <asp:RadioButton ID="rdbCorrelativo" GroupName="grpBusqueda" runat="server" Text="Búsqueda por correlativo" Checked="false" TextAlign="Right" onclick="ShowHideDiv()" />
                                        </div>
                                    </asp:Panel>

                                    <asp:UpdatePanel runat="server" ID="updObtenerLink">
                                        <ContentTemplate>
                                            <div id="divProducto" class="form-group form-group-sm">
                                                <asp:Label ID="Label5" runat="server" CssClass="col-md-3 col-md-offset-1 col-xs-3 control-label" Text="Número de cuenta TC/Préstamo:"></asp:Label>
                                                <div class="col-md-4 col-xs-9">
                                                    <asp:TextBox ID="txtNumCta" runat="server" CssClass="form-control validate[required, custom[onlyNumberSp]]"></asp:TextBox>
                                                </div>
                                                <div class="col=md-4">
                                                    <asp:Button ID="btnBuscarCta" runat="server" Text="Buscar" CssClass="btn btn-sm btn-info" UseSubmitBehavior="false" AutoPostBack="false" OnClick="btnBuscarCta_Click" OnClientClick="$('#divDetalle').show();" />
                                                </div>
                                            </div>
                                            <div id="divCorrelativo" class="form-group form-group-sm">
                                                <asp:Label ID="Label1" runat="server" CssClass="col-md-3 col-md-offset-1 col-xs-3 control-label" Text="Número de Correlativo:"></asp:Label>
                                                <div class="col-md-4 col-xs-9">
                                                    <asp:TextBox ID="txtCorrelativo" runat="server" CssClass="form-control validate[required, custom[onlyNumberSp]]"></asp:TextBox>
                                                </div>
                                                <div class="col=md-4">
                                                    <asp:Button ID="btnBuscarCorrelativo" runat="server" Text="Buscar" CssClass="btn btn-sm btn-info" UseSubmitBehavior="false" AutoPostBack="false" OnClick="btnBuscarParametro_Click" OnClientClick="$('#divDetalle').show();" />
                                                </div>
                                            </div>
                                            <div id="divDetalle" class="form-group form-group-sm">
                                                <div class="row">
                                                    <asp:Label ID="Label4" runat="server" CssClass="col-md-3 col-md-offset-1 col-xs-3 control-label" Text="Correlativo parametro:"></asp:Label>
                                                    <div class="col-md-4 col-xs-9">
                                                        <asp:TextBox ID="txtCodCorrelativo" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <asp:Label ID="Label6" runat="server" CssClass="col-md-3 col-md-offset-1 col-xs-3 control-label" Text="Día del mes:"></asp:Label>
                                                    <div class="col-md-4 col-xs-9">
                                                        <asp:TextBox ID="txtDia" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <asp:Label ID="Label7" runat="server" CssClass="col-md-3 col-md-offset-1 col-xs-3 control-label" Text="Próxima Fecha:"></asp:Label>
                                                    <div class="col-md-4 col-xs-9">
                                                        <asp:TextBox ID="txtFechaProx" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
                                                    </div>
                                                </div>
                                            </div>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                </div>
                                <div class="col-md-11 col-xs-9 text-right">
                                    <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn btn-default" CausesValidation="False" OnClick="btnCancelar_Click" UseSubmitBehavior="False" />
                                    <asp:Button ID="btnGuardar" runat="server" Text="Deshabilitar" CssClass="btn btn-success" OnClick="btnGuardar_Click" />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <script type="text/javascript" charset="UTF-8" src="js/chosen.jquery-1.6.2.min.js"></script>
    <script type="text/javascript">

        function cargar_scripts() {
            $("select").chosen({
                width: "100%",
                no_results_text: "No se encontraron resultados."
            });

            $("#btnBuscaarCli").click(function () {
                $("#divBusquedaCliente").modal();
            });
        }

        function ShowHideDiv() {
            var rdbProducto = document.getElementById("<%=rdbProducto.ClientID%>");
            var rdbCorrelativo = document.getElementById("<%=rdbCorrelativo.ClientID%>");
            var divProducto = document.getElementById("divProducto");
            var divCorrelativo = document.getElementById("divCorrelativo");

            divProducto.style.display = rdbProducto.checked ? "block" : "none";
            divCorrelativo.style.display = rdbCorrelativo.checked ? "block" : "none";

        }

    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="footerContent" runat="Server">
    <script type="text/javascript" charset="UTF-8" src="js/jquery.validationEngine.js"></script>
    <script type="text/javascript" charset="UTF-8" src="js/jquery.validationEngine-es.js"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            $("#divProducto").hide();
            $("#divCorrelativo").hide();
            $("#divDetalle").hide();


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

