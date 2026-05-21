namespace Backend.Models
{
    public class MenuItem
    {
        public int CodMenuItem { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int CodItemPadre { get; set; }
        public string Visible { get; set; } = string.Empty;
    }

    public class UserRoleInfo
    {
        public string Usuario { get; set; } = string.Empty;
        public int Rol { get; set; }
        public int CodMenuItem { get; set; }
        public string Accion { get; set; } = string.Empty;
        public int Sistema { get; set; }
    }
}
