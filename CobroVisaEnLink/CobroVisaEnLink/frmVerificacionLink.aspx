<%@ Page Title="" Language="C#" MasterPageFile="~/mpaSistema.master" AutoEventWireup="true" CodeFile="frmVerificacionLink.aspx.cs" Inherits="frmLinksVerifica" %>
<%@ MasterType VirtualPath="~/mpaSistema.master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="Server">
    <link type="text/css" rel="stylesheet" href="css/dataTables.bootstrap.css" media="screen" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="Server">
    <div class="modal fade" tabindex="-1" id="divPago">
        <div class="modal-dialog modal-lg" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h4>Confirmar consulta a NeoNet y realizar pago</h4>
                </div>
                <div class="modal-body">
                    <div class="messagealert" id="alert_container"></div>
                    <div id="contenido">
                        <asp:UpdatePanel runat="server" ID="UpdatePanel1">
                            <ContentTemplate>
                                <div class="row">
                                    <div class="col-md-4 text-right">
                                        <label>Producto:</label>
                                    </div>
                                    <div class="col-md-8">
                                        <asp:HiddenField ID="hdfProducto" runat="server" />
                                        <asp:Label runat="server" ID="lblProducto"></asp:Label>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-4 text-right">
                                        <label>C&oacute;digo NeoLink:</label>
                                    </div>
                                    <div class="col-md-8">
                                        <asp:HiddenField ID="hdfSKU" runat="server" />
                                        <asp:Label runat="server" ID="lblSKU"></asp:Label>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-4 text-right">
                                        <label>ID:</label>
                                    </div>
                                    <div class="col-md-8">
                                        <asp:HiddenField ID="hdfCodLink" runat="server" />
                                        <asp:Label runat="server" ID="lblCodLink"></asp:Label>
                                    </div>
                                </div>
                                <div class="row" style="margin-top: 15px">
                                    <div class="row">
                                        <div class="col-md-6 text-right">
                                            <asp:Button ID="btnSubmit" Text="Procesar" runat="server" class="btn btn-success" OnClick="btnConsultaPago_Click" CausesValidation="False" UseSubmitBehavior="False" />
                                        </div>
                                    </div>
                                </div>
                                <div class="row" style="margin-top: 15px">
                                    <div class="col-md-12" style="font-size: 10px">
                                        *Solo se muestra el resultado selecionado.<br />
                                    </div>
                                </div>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-default" data-dismiss="modal" id="btnNoCta">Cancelar</button>
                </div>
            </div>
        </div>
    </div>
    <div class="container">
        <div class="row">
            <div class="col-md-12" style="margin-top: 15px;">
                <div class="table-responsive">
                    <table id="tblLinks" class="table table-striped display compact" style="width: 100%">
                        <thead>
                            <tr>
                                <th>Correlativo</th>
                                <th>Producto</th>
                                <th>C&oacute;digo NeoNet</th>
                                <th>Autorizacion</th>
                                <th>Movimiento</th>
                                <th>Consulta / Pago</th>
                            </tr>
                        </thead>
                    </table>
                </div>
            </div>
        </div>
    </div>
    <script type="text/javascript" src="js/jquery.dataTables-1.10.16.js"></script>
    <%--<script type="text/javascript" src="js/jquery.dataTables.yadcf-0.9.3.js"></script>--%>
    <script type="text/javascript" src="js/dataTables.bootstrap-1.10.16.min.js"></script>
    <%--<script type="text/javascript" charset="UTF-8" src="js/chosen.jquery-1.6.2.min.js"></script>--%>
    <script type="text/javascript" charset="UTF-8" src="js/dataTables.colReorder-1.3.2.min.js"></script>

    <script src="js/dataTables.buttons-1.5.1.min.js"></script>
    <script src="js/buttons.bootstrap-1.5.1.min.js"></script>
    <script src="js/jszip.min.js"></script>
    <script src="js/vfs_fonts.js"></script>
    <script src="js/buttons.html5.min.js"></script>
    <script src="js/buttons.print.min.js"></script>
    <script src="js/buttons.colVis-1.5.1.min.js"></script>
    <script type="text/javascript">
        function HideDiv() {

            var divContenido = document.getElementById("contenido");

            divContenido.style.display = "none";

        }
        function ShowMessage(message, messagetype) {
            var cssclass;
            switch (messagetype) {
                case 'Success':
                    cssclass = 'alert-success'
                    break;
                case 'Error':
                    cssclass = 'alert-danger'
                    break;
                case 'Warning':
                    cssclass = 'alert-warning'
                    break;
                default:
                    cssclass = 'alert-info'
            }
            $('#alert_container').append('<div id="alert_div" style="margin: 0 0.5%; -webkit-box-shadow: 3px 4px 6px #999;" class="alert fade in ' + cssclass + '"><a href="#" class="close" data-dismiss="alert" aria-label="close">&times;</a><strong>' + messagetype + '!</strong> <span>' + message + '</span></div>');
        }
        $(document).ready(function () {

            var table = $('#tblLinks').DataTable(
                {
                    stateSave: true,
                    processing: true,
                    serverSide: true,
                    ajax: {
                        method: "POST",
                        "url": "ws.asmx/GetLinksVerifica",
                        contentType: "application/json; charset=utf-8",
                        "dataType": "json",
                        "dataSrc": "data",
                        //"dataSrc": function (json) {
                        //    if (json.d == "401") alert("error");
                        //    else
                        //        return json.data;
                        //},
                        "data": function (d) {
                            return JSON.stringify({ parameters: d });
                        }
                    },
                    columns: [
                        { "data": "CORRELATIVO", "name": "Correlativo", "searchable": true },
                        { "data": "PRODUCTO", "name": "Producto", "searchable": true },
                        { "data": "CODIGO_VISA", "name": "Codigo_Visa", "searchable": true, "visible": true },
                        { "data": "NUM_AUTO", "name": "Num_Auto", "searchable": false, "visible": true },
                        { "data": "NUM_MOV", "name": "Num_Mov", "searchable": false, "visible": true },
                        //{ "data": "NOM_SUCURSAL",       "name": "CLI.NOM_SUCURSAL", "searchable": true,     "visible": true },
                        //{ "data": "COD_TRANSPO",        "name": "CLI.COD_TRANSPO", "searchable": true },
                        //{ "data": "NOM_TRANSPO",        "name": "TRA.NOM_TRANSPO", "searchable": true },
                        //{ "data": "IND_ESTADO",         "name": "CLI.IND_ESTADO",   "searchable": false,    "visible": true  },                       
                        { "data": "EDIT", "name": "EDIT", "searchable": true, "visible": true, "orderable": false, "exportOption": false }
                    ],
                    colReorder: true,
                    language: {
                        "url": "js/spanish.json",
                        buttons: {
                            colvisRestore: "Mostrar Columnas Iniciales",
                            pageLength: "Cantidad Por Pagina"
                        },
                        sProcessing: "<div class='loader'><div class='circle'></div><div class='circle'></div><div class='circle'></div><div class='circle'></div><div class='circle'></div></div></div>"
                    },
                    iDisplayLength: 25,
                    pagingType: "full_numbers",
                    dom: 'Bfrtip',
                    lengthMenu: [
                        [10, 25, 50, 100, 1000, -1],
                        ['10 Filas', '25 Filas', '50 Filas', '100 Filas', '1000 Filas', 'Todo']
                    ],
                    buttons: [
                        {
                            extend: 'pageLength',
                            text: 'Cantidad Por Pagina',
                            className: 'btn-primary'
                        },
                        {
                            extend: 'colvis',
                            postfixButtons: ['colvisRestore'],
                            text: 'Mostrar/Ocultar Columnas'
                        },
                        {
                            extend: 'copy',
                            text: 'Copiar',
                            className: 'btn-warning',
                            exportOptions: {
                                columns: ':visible'
                            }
                        },
                        {
                            extend: 'excel',
                            className: 'btn-success',
                            exportOptions: {
                                columns: ':visible'
                            }
                        },
                        {
                            extend: 'print',
                            text: 'Imprimir',
                            className: 'btn-info',
                            exportOptions: {
                                columns: ':visible'
                            }
                        }
                    ]
                }
            );

            $('#divPago').on('hidden.bs.modal', function () {
                location.reload();
            })

            $('body').on('click', '[id*=btnEdit]', function () {
                var data = $(this).parents('tr').find('td');
                var id = data.eq(0).html();
                var producto = data.eq(1).html();
                var codSku = data.eq(2).html();
                var divContenido = document.getElementById("contenido");

                $('[id*=lblProducto]').text(producto);
                $('[id*=hdfProducto]').val(producto);
                $('[id*=lblSKU]').text(codSku);
                $('[id*=hdfSKU]').val(codSku);
                $('[id*=lblCodLink]').text(id);
                $('[id*=hdfCodLink]').val(id);

                $('#divPago').modal("show");

                divContenido.style.display = "block";

            });
        });
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="footerContent" runat="Server">
</asp:Content>

