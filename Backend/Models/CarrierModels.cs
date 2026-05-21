using System;

namespace Backend.Models
{
    public class CarrierModel
    {
        public string CodTranspo { get; set; } = string.Empty;
        public string NomTranspo { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string NomEncargado { get; set; } = string.Empty;
        public string IndEstado { get; set; } = string.Empty; // 'A' or 'I'
        public string Clave { get; set; } = string.Empty; // Plaintext password for insert/update
    }

    public class CarrierDropdownItem
    {
        public string CodTranspo { get; set; } = string.Empty;
        public string NomTranspo { get; set; } = string.Empty;
    }
}
