<%@ Page Title="" Language="C#" MasterPageFile="~/mpaSistema.master" AutoEventWireup="true" CodeFile="frmControlLink.aspx.cs" Inherits="frmControlLink" %>

<%@ MasterType VirtualPath="~/mpaSistema.master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="Server">
    <link type="text/css" rel="stylesheet" href="css/dataTables.bootstrap.css" media="screen" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="Server">
    <div class="container">
        <div class="row">
            <div class="col-md-12" style="margin-top: 15px;">
                <div class="table-responsive">
                    <table id="tblLinks" class="table table-striped display compact" style="width: 100%">
                        <thead>
                            <tr>
                                <th>Correlativo</th>
                                <th>Producto</th>
                                <th>Monto</th>
                                <th>Pago</th>
                                <th>Emision_Link</th>
                                <th>Usuario</th>
                                <th>Envio</th>
                                <th>Tipo_Link</th>
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
        $(document).ready(function () {

            var table = $('#tblLinks').DataTable(
                {
                    stateSave: true,
                    processing: true,
                    serverSide: true,
                    ajax: {
                        method: "POST",
                        "url": "ws.asmx/GetLinks",
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
                        { "data": "CORRELATIVO", "name": "CORRELATIVO", "searchable": true },
                        { "data": "PRODUCTO", "name": "PRODUCTO", "searchable": true },
                        { "data": "MONTO", "name": "MONTO", "searchable": true, "visible": true },
                        { "data": "PAGO", "name": "PAGO", "searchable": true, "visible": true },
                        { "data": "EMISION_LINK", "name": "EMISION_LINK", "searchable": true, "visible": true },
                        { "data": "USUARIO", "name": "USUARIO", "searchable": true, "visible": true },
                        { "data": "ENVIO", "name": "ENVIO", "searchable": true },
                        { "data": "TIPO_LINK", "name": "TIPO_LINK", "searchable": true },
                        
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

            //$("select").chosen({ 
            //    //width: "100%" 
            //});
        });
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="footerContent" runat="Server">
</asp:Content>