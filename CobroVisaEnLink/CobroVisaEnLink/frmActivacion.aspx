<%@ Page Title="" Language="C#" MasterPageFile="~/mpaSistema.master" AutoEventWireup="true" CodeFile="frmActivacion.aspx.cs" Inherits="frmActivacion" %>

<%@ MasterType VirtualPath="~/mpaSistema.master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" runat="Server">
    <div class="container">
        <div class="row">
            <div class="col-md-12" style="padding: 15px">
                <div class="row">
                    <div class="col-md-12 ">
                        <div class="panel panel-success">
                            <div class="panel-heading">Informaci&oacute;n del Link</div>
                            <div class="panel-body">
                                <asp:UpdatePanel runat="server" ID="udpBuscarLink">
                                    <ContentTemplate>
                                        <div class="form-group form-group-sm">
                                            <asp:Label ID="Label8" runat="server" CssClass="col-md-3 col-md-offset-1 col-xs-3 control-label" Text="Código de Link:"></asp:Label>
                                            <div class="col-md-4 col-xs-9">
                                                <asp:TextBox ID="txtSKU" runat="server" CssClass="form-control"></asp:TextBox>
                                            </div>
                                            <div class="col=md-4">
                                                <asp:Button ID="btnBuscarSKU" runat="server" Text="Buscar Link" CssClass="btn btn-sm btn-info" UseSubmitBehavior="false" OnClick="btnBuscarLink_Click" OnClientClick="$('#divStep2').show();" />
                                            </div>
                                        </div>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                                
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="footerContent" runat="Server">
</asp:Content>
