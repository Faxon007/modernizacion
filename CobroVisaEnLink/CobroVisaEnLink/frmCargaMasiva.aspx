<%@ Page Title="" Language="C#" MasterPageFile="~/mpaSistema.master" AutoEventWireup="true" CodeFile="frmCargaMasiva.aspx.cs" Inherits="frmCargaMasiva" %>

<%@ MasterType VirtualPath="~/mpaSistema.master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="Server">
    <script type="text/javascript">


        function ShowProgress() {
            document.getElementById('<% Response.Write(updateProgress.ClientID); %>').style.display = "inline";
        }


    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="Server">
    <div class="container">
        <div class="row">
            <div class="col-md-12" style="padding: 15px">
                <div class="row">
                    <div class="col-md-12 ">
                        <div class="panel panel-success">
                            <div class="panel-heading">Proceso de carga de archivo</div>
                            <div class="panel-body">
                                <asp:UpdateProgress ID="updateProgress" runat="server" AssociatedUpdatePanelID="udpCargarLink">
                                    <ProgressTemplate>
                                        <div style="position: fixed; text-align: center; height: 100%; width: 100%; top: 0; right: 0; left: 0; z-index: 9999999; background-color: #000000; opacity: 0.7;">
                                            <div class='loader'>
                                                <div class='circle'></div>
                                                <div class='circle'></div>
                                                <div class='circle'></div>
                                                <div class='circle'></div>
                                                <div class='circle'></div>
                                            </div>
                                        </div>
                                    </ProgressTemplate>
                                </asp:UpdateProgress>
                                <asp:UpdatePanel runat="server" ID="udpCargarLink">
                                    <ContentTemplate>
                                        <div class="col-md-8">
                                            <div class="col-md-offset-5 col-md-10">
                                                <div class="form-group">

                                                    <h3 class="text-info">Elegir archivo a cargar:</h3>
                                                    <div class="input-group">
                                                        <asp:FileUpload ID="FileUpload1" CssClass="button" runat="server" accept=".csv" />
                                                        <asp:RegularExpressionValidator ID="regexValidator" runat="server"
                                                            ControlToValidate="FileUpload1" ErrorMessage="Unicamente archivos CSV son permitidos"
                                                            ValidationExpression="(.*\.([cC][sS][vV])$)">
                                                        </asp:RegularExpressionValidator>
                                                        <span class="input-group-btn"></span>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                                <asp:UpdatePanel runat="server" ID="UpdatePanel1">
                                    <ContentTemplate>
                                        <div class="form-group">
                                            <div class="col-md-offset-5 col-md-10">
                                                <asp:Button ID="btnImport" runat="server" Text="Cargar Archivo" CssClass="btn btn-sm btn-danger" OnClick="ImportCSV" OnClientClick="ShowProgress();" />
                                            </div>
                                        </div>
                                    </ContentTemplate>
                                    <Triggers>
                                        <asp:PostBackTrigger ControlID="btnImport" />
                                    </Triggers>
                                    <%--<Triggers>
                                        <asp:AsyncPostBackTrigger ControlID="btnImport" EventName="Click" />
                                    </Triggers>--%>
                                </asp:UpdatePanel>
                                <hr />
                                <%--<asp:UpdatePanel runat="server" ID="udpMostrarLinks">
                                    <ContentTemplate>--%>
                                <div class="col-lg-12 ">
                                    <div class="table-responsive">
                                        <asp:GridView ID="gvParametros" runat="server" CssClass="table table-striped table-bordered table-hover">
                                        </asp:GridView>
                                    </div>
                                </div>

                                <%--</ContentTemplate>
                                </asp:UpdatePanel>--%>
                                <asp:Panel ID="grbCambios" runat="server">
                                    <div class="col-md-11 col-xs-9 text-right">
                                        <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn btn-default" CausesValidation="False" OnClick="btnCancelar_Click" UseSubmitBehavior="False" />
                                        <asp:Button ID="btnNuevaCarga" runat="server" Text="Nueva Carga" CssClass="btn btn-success" OnClick="btnNuevaCarga_Click" />
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
        function cargar_scripts() {
            $("select").chosen({
                width: "100%",
                no_results_text: "No se encontraron resultados."
            });
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="footerContent" runat="Server">
    <script type="text/javascript" charset="UTF-8" src="js/jquery.validationEngine.js"></script>
    <script type="text/javascript" charset="UTF-8" src="js/jquery.validationEngine-es.js"></script>
    <script type="text/javascript">

        $(document).ready(function () {
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

            //$('#exampleFakeBrowseFile1, #exampleFakeFile1').on('click', function () {
            //    $('#exampleFile1').trigger("click");
            //});

            //$('#exampleFile1').change(function () {
            //    //var file_name = this.value.replace(/\\/g, '/').replace(/.*\//, '');
            //    var file_name = document.getElementById("exampleFile1").files[0].name;
            //    //alert(file_name);
            //    $('#exampleFakeFile1').val(file_name);
            //});

        });
    </script>
</asp:Content>
