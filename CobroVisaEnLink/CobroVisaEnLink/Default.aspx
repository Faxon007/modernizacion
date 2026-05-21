<%@ Page Title="" Language="C#" MasterPageFile="~/mpaSistema.master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>
<%@ MasterType VirtualPath="~/mpaSistema.master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="headContent" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="mainContent" Runat="Server">
    <div class="container">
        <div class="row">
            <div class="form-group form-group-sm"> 
                <asp:Login ID="loginUsuario" runat="server" 
                    width="350px" CssClass="panel panel-success login_from" 
                    DestinationPageUrl="Default.aspx" onauthenticate="loginUsuario_Authenticate"
                    DisplayRememberMe="False"
                    TitleText="Inicio de Sesión" UserNameLabelText="Usuario:" PasswordLabelText="Clave:" LoginButtonText="Ingresar"
                    UserNameRequiredErrorMessage="Por favor ingrese su Usuario" PasswordRequiredErrorMessage="Por favor ingrese su clave" FailureText=""
                    >
                    <TitleTextStyle CssClass="login_title" />
                    <LabelStyle CssClass="login_label" />
                    <TextBoxStyle CssClass="form-control login_input"  />
                    <LoginButtonStyle CssClass="btn btn-success login_button" />
                </asp:Login>
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="footerContent" Runat="Server">
</asp:Content>

