using System;

namespace Backend.Models
{
    public class TransactionGridItem
    {
        public string CodTransaccion { get; set; } = string.Empty;
        public string CodBolsa { get; set; } = string.Empty;
        public string CodCliAci { get; set; } = string.Empty;
        public string CodCliTr { get; set; } = string.Empty;
        public string CodCli { get; set; } = string.Empty;
        public string NomCliente { get; set; } = string.Empty;
        public string CodSucursal { get; set; } = string.Empty;
        public string CodTranspo { get; set; } = string.Empty;
        public string NumCta { get; set; } = string.Empty;
        public string MonEfectivo { get; set; } = string.Empty;
        public string MonChqProp { get; set; } = string.Empty;
        public string MonChqAjen { get; set; } = string.Empty;
        public string MonEfeCont { get; set; } = string.Empty;
        public string MonChqPrpCont { get; set; } = string.Empty;
        public string MonChqAjnCont { get; set; } = string.Empty;
        public string NumTraEfectivo { get; set; } = string.Empty;
        public string NumTraChqPropio { get; set; } = string.Empty;
        public string DifEfectivo { get; set; } = string.Empty;
        public string DifChqPrp { get; set; } = string.Empty;
        public string DifChqAjn { get; set; } = string.Empty;
        public string FecIngreso { get; set; } = string.Empty;
        public string FecConteo { get; set; } = string.Empty;
    }

    public class TransactionQueryRequest
    {
        public int Start { get; set; }
        public int Length { get; set; }
        public string ColumnaOrden { get; set; } = "COD_TRANSACCION";
        public string DirOrden { get; set; } = "ASC";
        public string Busqueda { get; set; } = string.Empty;
        public List<ColumnDefinition> Columnas { get; set; } = new();
    }

    public class ColumnDefinition
    {
        public string Name { get; set; } = string.Empty;
        public bool Searchable { get; set; }
        public SearchDefinition Search { get; set; } = new();
    }

    public class SearchDefinition
    {
        public string Value { get; set; } = string.Empty;
    }

    public class PagedTransactionResult
    {
        public int TotalRecords { get; set; }
        public int FilteredRecords { get; set; }
        public List<TransactionGridItem> Data { get; set; } = new();
    }
}
