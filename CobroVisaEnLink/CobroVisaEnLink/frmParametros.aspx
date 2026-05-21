<%@ Page Title="" Language="C#" MasterPageFile="~/mpaSistema.master" AutoEventWireup="true" CodeFile="frmParametros.aspx.cs" Inherits="frmParametros" ValidateRequest="false" %>

<%@ MasterType VirtualPath="~/mpaSistema.master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="Server">
    <link rel="stylesheet" type="text/css" href="css/validationEngine.jquery.css" media="screen" />
    <script type="text/javascript" charset="UTF-8" src="js/moment-with-locales.js"></script>
    <script type="text/javascript" charset="UTF-8" src="js/bootstrap-datetimepicker.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="Server">
    <div class="container">
        <div class="row">
            <div class="col-md-12" style="padding: 15px">
                <div class="row">
                    <div class="col-md-12 ">
                        <div class="panel panel-success">
                            <div class="panel-heading">Parametros del Sistema</div>
                            <div class="panel-body">
                                <ul class="nav nav-tabs">
                                    <li class="active"><a data-toggle="tab" href="#nav-progra">Programaci&oacute;n</a></li>
                                    <li><a data-toggle="tab" href="#nav-trx">Transacci&oacute;n a Utilizar</a></li>
                                    <li><a data-toggle="tab" href="#nav-api">Par&aacute;metros API</a></li>
                                    <li><a data-toggle="tab" href="#nav-formato">Formato</a></li>
                                </ul>
                                <div class="tab-content" id="nav-tabContent">
                                    <div class="tab-pane fade in active" id="nav-progra">
                                        <div class="container">
                                            <div class="row">
                                                <h4>Programaci&oacute;n sobre Revisi&oacute;n de autorizaci&oacute;n y Pagos:</h4>
                                                <div class="col-sm-3">
                                                    <div class="form-group">
                                                        <div class="row">
                                                            <asp:RadioButton ID="rdbUnicaR" GroupName="grpRev" runat="server" Text="Una vez" Checked="false" TextAlign="Right" />
                                                        </div>
                                                        <div class="row">
                                                            <asp:RadioButton ID="rdbDiarioR" GroupName="grpRev" runat="server" Text="Diariamente" Checked="false" TextAlign="Right" />
                                                        </div>
                                                        <div class="row">
                                                            <asp:RadioButton ID="rdbSemanalR" GroupName="grpRev" runat="server" Text="Semanal" Checked="false" TextAlign="Right" />
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="col-sm-6">
                                                    <div class="row">
                                                        <asp:Label ID="Label5" runat="server" CssClass="col-md-3 col-md-offset-1 col-xs-3 control-label" Text="Repetir cada:"></asp:Label>
                                                        <div class="col-md-4">
                                                            <asp:DropDownList ID="ddlHora" runat="server"></asp:DropDownList>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                            <hr />
                                            <div class="row">
                                                <h4>Programaci&oacute;n sobre Generaci&oacute;n Links:</h4>
                                                <div class="col-sm-3">
                                                    <div class="form-group">
                                                        <div class="row">
                                                            <asp:RadioButton ID="rdbUnicaG" GroupName="grpGen" runat="server" Text="Una vez" Checked="false" TextAlign="Right" />
                                                        </div>
                                                        <div class="row">
                                                            <asp:RadioButton ID="rdbDiarioG" GroupName="grpGen" runat="server" Text="Diariamente" Checked="false" TextAlign="Right" />
                                                        </div>
                                                        <div class="row">
                                                            <asp:RadioButton ID="rdbSemanalG" GroupName="grpGen" runat="server" Text="Semanal" Checked="false" TextAlign="Right" />
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="col-sm-6">
                                                    <div class="row">
                                                        <div class='input-group date' style="width: 200px">
                                                            <asp:TextBox ID="txtTime" runat="server" CssClass="form-control" />
                                                            <span class="input-group-addon"><span class="glyphicon glyphicon-time"></span></span>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="tab-pane fade" id="nav-trx">
                                        <h4>Configuraci&oacute;n de pagos en TC</h4>
                                        <div class="row">
                                            <div class="col-6 col-md-4">
                                                <asp:Label ID="Label4" runat="server" CssClass="control-label" Text="Transacción:"></asp:Label>
                                                <asp:TextBox ID="txtTrxTC" runat="server" CssClass="form-control validate[custom[onlyNumberSp]]" ></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="col-6 col-md-4">
                                                <asp:Label ID="Label2" runat="server" CssClass="control-label" Text="Sub-Transacción:"></asp:Label>
                                                <asp:TextBox ID="txtSubTrx" runat="server" CssClass="form-control validate[custom[onlyNumberSp]]" ></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="col-6 col-md-4">
                                                <asp:Label ID="Label13" runat="server" CssClass="control-label" Text="Descripción de pago (TC):"></asp:Label>
                                                <asp:TextBox ID="txtDesPagoTC" runat="server" CssClass="form-control" ></asp:TextBox>
                                            </div>
                                        </div>
                                        <hr />
                                        <h4>Configuraci&oacute;n de pagos en PR</h4>
                                        <div class="row">
                                            <div class="col-6 col-md-4">
                                                <asp:Label ID="Label3" runat="server" CssClass="control-label" Text="Cuenta (Q):"></asp:Label>
                                                <asp:TextBox ID="txtCuentaQ" runat="server" CssClass="form-control validate[custom[onlyNumberSp]]" ></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="col-6 col-md-4">
                                                <asp:Label ID="Label6" runat="server" CssClass="control-label" Text="Cuenta ($):"></asp:Label>
                                                <asp:TextBox ID="txtCuentaD" runat="server" CssClass="form-control validate[custom[onlyNumberSp]]" ></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="col-6 col-md-4">
                                                <asp:Label ID="Label10" runat="server" CssClass="control-label" Text="Código de Agencia:"></asp:Label>
                                                <asp:TextBox ID="txtAgencia" runat="server" CssClass="form-control validate[custom[onlyNumberSp]]" ></asp:TextBox>
                                            </div>
                                        </div>
                                        <hr />
                                        <h4>Configuraci&oacute;n de bit&aacute;cora</h4>
                                        <div class="row">
                                            <div class="col-6 col-md-4">
                                                <asp:Label ID="Label7" runat="server" CssClass="control-label" Text="Tipología Primaria (TC):"></asp:Label>
                                                <asp:TextBox ID="txtTipoTC" runat="server" CssClass="form-control validate[custom[onlyNumberSp]]" ></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="col-6 col-md-4">
                                                <asp:Label ID="Label8" runat="server" CssClass="control-label" Text="Tipología Secundaria (TC):"></asp:Label>
                                                <asp:TextBox ID="txtSubtipoTC" runat="server" CssClass="form-control validate[custom[onlyNumberSp]]" ></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="col-6 col-md-4">
                                                <asp:Label ID="Label9" runat="server" CssClass="control-label" Text="Departamento para Bitacora TC:"></asp:Label>
                                                <asp:TextBox ID="txtDepartamento" runat="server" CssClass="form-control validate[custom[onlyNumberSp]]" ></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="col-6 col-md-4">
                                                <asp:Label ID="Label11" runat="server" CssClass="control-label" Text="Tipología Primaria (PR):"></asp:Label>
                                                <asp:TextBox ID="txtTipoPR" runat="server" CssClass="form-control validate[custom[onlyNumberSp]]" ></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="col-6 col-md-4">
                                                <asp:Label ID="Label12" runat="server" CssClass="control-label" Text="Tipología Secundaria (PR):"></asp:Label>
                                                <asp:TextBox ID="txtSubtipoPR" runat="server" CssClass="form-control validate[custom[onlyNumberSp]]" ></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="col-6 col-md-4">
                                                <asp:Label ID="Label14" runat="server" CssClass="control-label" Text="Campaña para Bitacora PR:"></asp:Label>
                                                <asp:TextBox ID="txtCampanaPR" runat="server" CssClass="form-control validate[custom[onlyNumberSp]]" ></asp:TextBox>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="tab-pane fade" id="nav-api">
                                        <div class="container">
                                            <h4>Parametros necesarios para la generaci&oacute;n de los links:</h4>
                                            <div class="form-group form-group-sm">
                                                <div class="row">
                                                    <asp:FileUpload ID="flUpload" runat="server" accept=".png,.jpg,.jpeg,.gif" />
                                                    <asp:Button ID="btnUpload" runat="server" Text="Cargar Imagen" CssClass="btn btn-sm btn-info" OnClick="btnUpload_Click" />
                                                </div>
                                                <hr />
                                                <asp:Image ID="imgPreview" Visible="false" runat="server" Height="100" Width="100" />
                                            </div>
                                        </div>
                                    </div>
                                    <div class="tab-pane fade" id="nav-formato">
                                        <div class="container">
                                            <h4>Parametros necesarios para el env&iacute;o de correo:</h4>
                                            <div class="row">
                                                <asp:Label ID="Label1" runat="server" CssClass="col-md-3 col-md-offset-1 col-xs-3 control-label" Text="Remitente:"></asp:Label>
                                                <div class="col-md-4 col-xs-9">
                                                    <asp:TextBox ID="txtRemitente" runat="server" CssClass="form-control validate[custom[email]]"></asp:TextBox>
                                                </div>
                                            </div>
                                            <div class="form-group">
                                                <label for="exampleFormControlTextarea3">HEADER del correo:</label>
                                                <asp:TextBox CssClass="form-control" runat="server" ID="txtHeader" TextMode="MultiLine" Rows="7" />
                                            </div>
                                            <div class="form-group">
                                                <label for="exampleFormControlTextarea3">FOOTER del correo:</label>
                                                <asp:TextBox CssClass="form-control" runat="server" ID="txtFooter" TextMode="MultiLine" Rows="7" />
                                            </div>
                                            <h4>Parametros para el env&iacute;o de SMS:</h4>
                                            <div class="form-group">
                                                <label for="exampleFormControlTextarea3">Texto:</label>
                                                <asp:TextBox CssClass="form-control" runat="server" ID="txtSMS" TextMode="MultiLine" Rows="2" />
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <div class="col-md-11 col-xs-9 text-right">
                                    <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn btn-default" CausesValidation="False" OnClick="btnCancelar_Click" UseSubmitBehavior="False" />
                                    <asp:Button ID="btnGuardar" runat="server" Text="Guardar" CssClass="btn btn-success" OnClick="btnGuardar_Click" />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="footerContent" runat="Server">
    <script type="text/javascript" charset="UTF-8" src="js/jquery.validationEngine.js"></script>
    <script type="text/javascript" charset="UTF-8" src="js/jquery.validationEngine-es.js"></script>
    <script type="text/javascript">

        $(function () {
            $('[id*=txtTime]').datetimepicker({
                format: 'LT'
            });
        });

        $(document).ready(function () {
            $("#aspnetForm").validationEngine();
            //  $("select").chosen({ width: "100%" });
        });
    </script>
</asp:Content>



