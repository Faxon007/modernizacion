namespace Backend.Models
{
    public class ClientEntity
    {
        public string CodCliente { get; set; } = string.Empty;
        public string NomCliente { get; set; } = string.Empty;
    }

    public class PrestamoInfo
    {
        public string NumCuenta { get; set; } = string.Empty;
        public string Moneda { get; set; } = string.Empty;
    }

    public class CuentaInfo
    {
        public string NumCuenta { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
    }
}
