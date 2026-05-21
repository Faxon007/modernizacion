using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using Backend.Models;

namespace Backend.Repositories
{
    public class TransactionRepository(string connectionString) : ITransactionRepository
    {
        public async Task<PagedTransactionResult> GetTransaccionesAsync(TransactionQueryRequest request)
        {
            using var conn = new OracleConnection(connectionString);

            // 1. Total Count
            string countSql = @"SELECT COUNT(*) 
                                FROM BO.ACI_TRANSACCION TRA 
                                LEFT JOIN BO.ACI_CLIENTE CLI ON TRA.COD_CLI_ACI = CLI.COD_CLI_ACI";
            int totalRecords = await conn.ExecuteScalarAsync<int>(countSql);

            // 2. Build filters
            var dyParams = new DynamicParameters();
            var filterBuilder = new StringBuilder();

            // General Search
            if (!string.IsNullOrWhiteSpace(request.Busqueda))
            {
                string searchPattern = $"%{request.Busqueda.ToUpper()}%";
                dyParams.Add("searchPattern", searchPattern);

                filterBuilder.Append(@" AND (
                    UPPER(TRA.COD_TRANSACCION) LIKE :searchPattern OR
                    UPPER(TRA.COD_BOLSA) LIKE :searchPattern OR
                    UPPER(TRA.COD_CLI_ACI) LIKE :searchPattern OR
                    UPPER(CLI.COD_CLI_TR) LIKE :searchPattern OR
                    UPPER(CLI.COD_CLI) LIKE :searchPattern OR
                    UPPER(CLI.NUM_CTA) LIKE :searchPattern OR
                    UPPER(CLI.NOM_CLIENTE) LIKE :searchPattern OR
                    UPPER(CLI.COD_SUCURSAL) LIKE :searchPattern OR
                    UPPER(CLI.COD_TRANSPO) LIKE :searchPattern OR
                    UPPER(TO_CHAR(TRA.MON_EFECTIVO)) LIKE :searchPattern OR
                    UPPER(TO_CHAR(TRA.MON_CHQ_PROP)) LIKE :searchPattern OR
                    UPPER(TO_CHAR(TRA.MON_CHQ_AJEN)) LIKE :searchPattern OR
                    UPPER(TO_CHAR(TRA.MON_EFE_CONT)) LIKE :searchPattern OR
                    UPPER(TO_CHAR(TRA.MON_CHQ_PRP_CONT)) LIKE :searchPattern OR
                    UPPER(TO_CHAR(TRA.MON_CHQ_AJN_CONT)) LIKE :searchPattern OR
                    UPPER(TRA.NUM_TRA_EFECTIVO) LIKE :searchPattern OR
                    UPPER(TRA.NUM_TRA_CHQ_PROPIO) LIKE :searchPattern OR
                    TO_CHAR(TRA.FEC_INGRESO, 'DD/MM/YYYY') LIKE :searchPattern OR
                    TO_CHAR(TRA.FEC_CONTEO, 'DD/MM/YYYY') LIKE :searchPattern
                )");
            }

            // Column Search (from DataTables)
            int colIndex = 0;
            foreach (var col in request.Columnas)
            {
                if (col.Searchable && !string.IsNullOrWhiteSpace(col.Search?.Value))
                {
                    string cleanVal = col.Search.Value.Replace("^(", "").Replace(")$", "");
                    var values = cleanVal.Split('|').Select(v => v.Trim()).Where(v => !string.IsNullOrEmpty(v)).ToList();

                    if (values.Count > 0)
                    {
                        var paramNames = new List<string>();
                        for (int i = 0; i < values.Count; i++)
                        {
                            string pName = $"col_{colIndex}_{i}";
                            dyParams.Add(pName, values[i]);
                            paramNames.Add($":{pName}");
                        }
                        filterBuilder.Append($" AND {col.Name} IN ({string.Join(", ", paramNames)})");
                        colIndex++;
                    }
                }
            }

            string activeFilterSql = filterBuilder.ToString();

            // 3. Filtered Count
            string filteredCountSql = $@"SELECT COUNT(*) 
                                         FROM BO.ACI_TRANSACCION TRA 
                                         LEFT JOIN BO.ACI_CLIENTE CLI ON TRA.COD_CLI_ACI = CLI.COD_CLI_ACI 
                                         WHERE 1 = 1 {activeFilterSql}";
            int filteredRecords = await conn.ExecuteScalarAsync<int>(filteredCountSql, dyParams);

            // 4. Query Data
            // Validate and sanitize order column & direction to avoid sql injection
            string orderCol = "TRA.COD_TRANSACCION";
            string orderDir = "ASC";

            var allowedOrderCols = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "COD_TRANSACCION", "COD_BOLSA", "COD_CLI_ACI", "COD_CLI_TR", "COD_CLI",
                "NOM_CLIENTE", "COD_SUCURSAL", "COD_TRANSPO", "NUM_CTA", "MON_EFECTIVO",
                "MON_CHQ_PROP", "MON_CHQ_AJEN", "MON_EFE_CONT", "MON_CHQ_PRP_CONT",
                "MON_CHQ_AJN_CONT", "NUM_TRA_EFECTIVO", "NUM_TRA_CHQ_PROPIO",
                "DIF_EFECTIVO", "DIF_CHQ_PRP", "DIF_CHQ_AJN", "FEC_INGRESO", "FEC_CONTEO"
            };

            if (!string.IsNullOrEmpty(request.ColumnaOrden))
            {
                string cleanedCol = request.ColumnaOrden.ToUpper().Replace("TRA.", "").Replace("CLI.", "");
                if (allowedOrderCols.Contains(cleanedCol))
                {
                    if (cleanedCol == "NOM_CLIENTE" || cleanedCol == "COD_CLI" || cleanedCol == "COD_CLI_TR" || 
                        cleanedCol == "COD_SUCURSAL" || cleanedCol == "COD_TRANSPO" || cleanedCol == "NUM_CTA")
                    {
                        orderCol = "CLI." + cleanedCol;
                    }
                    else
                    {
                        orderCol = "TRA." + cleanedCol;
                    }
                }
            }

            if (!string.IsNullOrEmpty(request.DirOrden) && 
                (request.DirOrden.Equals("ASC", StringComparison.OrdinalIgnoreCase) || 
                 request.DirOrden.Equals("DESC", StringComparison.OrdinalIgnoreCase)))
            {
                orderDir = request.DirOrden.ToUpper();
            }

            string dataSql = $@"SELECT 
                                    TRA.COD_TRANSACCION AS CodTransaccion,
                                    TRA.COD_BOLSA AS CodBolsa,
                                    TRA.COD_CLI_ACI AS CodCliAci,
                                    CLI.COD_CLI_TR AS CodCliTr,
                                    CLI.COD_CLI AS CodCli,
                                    CLI.NOM_CLIENTE AS NomCliente,
                                    CLI.COD_SUCURSAL AS CodSucursal,
                                    CLI.COD_TRANSPO AS CodTranspo,
                                    CLI.NUM_CTA AS NumCta,
                                    TRA.MON_EFECTIVO AS MonEfectivoRaw,
                                    TRA.MON_CHQ_PROP AS MonChqPropRaw,
                                    TRA.MON_CHQ_AJEN AS MonChqAjenRaw,
                                    TRA.MON_EFE_CONT AS MonEfeContRaw,
                                    TRA.MON_CHQ_PRP_CONT AS MonChqPrpContRaw,
                                    TRA.MON_CHQ_AJN_CONT AS MonChqAjnContRaw,
                                    TRA.NUM_TRA_EFECTIVO AS NumTraEfectivo,
                                    TRA.NUM_TRA_CHQ_PROPIO AS NumTraChqPropio,
                                    TRA.DIF_EFECTIVO AS DifEfectivoRaw,
                                    TRA.DIF_CHQ_PRP AS DifChqPrpRaw,
                                    TRA.DIF_CHQ_AJN AS DifChqAjnRaw,
                                    TRA.FEC_INGRESO AS FecIngresoRaw,
                                    TRA.FEC_CONTEO AS FecConteoRaw
                                FROM BO.ACI_TRANSACCION TRA
                                LEFT JOIN BO.ACI_CLIENTE CLI ON TRA.COD_CLI_ACI = CLI.COD_CLI_ACI
                                WHERE 1 = 1 {activeFilterSql}
                                ORDER BY {orderCol} {orderDir}
                                OFFSET :start ROWS FETCH NEXT :length ROWS ONLY";

            dyParams.Add("start", request.Start);
            dyParams.Add("length", request.Length <= 0 ? 10 : request.Length);

            var dbItems = await conn.QueryAsync<dynamic>(dataSql, dyParams);
            var gridItems = new List<TransactionGridItem>();

            foreach (var item in dbItems)
            {
                var gridItem = new TransactionGridItem
                {
                    CodTransaccion = Convert.ToString(item.CODTRANSACCION) ?? string.Empty,
                    CodBolsa = Convert.ToString(item.CODBOLSA) ?? string.Empty,
                    CodCliAci = Convert.ToString(item.CODCLIACI) ?? string.Empty,
                    CodCliTr = Convert.ToString(item.CODCLITR) ?? string.Empty,
                    CodCli = Convert.ToString(item.CODCLI) ?? string.Empty,
                    NomCliente = Convert.ToString(item.NOMCLIENTE) ?? string.Empty,
                    CodSucursal = Convert.ToString(item.CODSUCURSAL) ?? string.Empty,
                    CodTranspo = Convert.ToString(item.CODTRANSPO) ?? string.Empty,
                    NumCta = Convert.ToString(item.NUMCTA) ?? string.Empty,
                    NumTraEfectivo = Convert.ToString(item.NUMTRAEFECTIVO) ?? string.Empty,
                    NumTraChqPropio = Convert.ToString(item.NUMTRACHQPROPIO) ?? string.Empty,
                    
                    MonEfectivo = FormatDecimal(item.MONEFECTIVORAW),
                    MonChqProp = FormatDecimal(item.MONCHQPROPRAW),
                    MonChqAjen = FormatDecimal(item.MONCHQAJENRAW),
                    MonEfeCont = FormatDecimal(item.MONEFECONTRAW),
                    MonChqPrpCont = FormatDecimal(item.MONCHQPRPCONTRAW),
                    MonChqAjnCont = FormatDecimal(item.MONCHQAJNCONTRAW),

                    DifEfectivo = FormatDifference(item.DIFEFECTIVORAW),
                    DifChqPrp = FormatDifference(item.DIFCHQPRPRAW),
                    DifChqAjn = FormatDifference(item.DIFCHQAJNRAW),

                    FecIngreso = item.FECINGRESORAW is DateTime fIng ? fIng.ToString("dd/MM/yyyy") : string.Empty,
                    FecConteo = item.FECCONTEORAW is DateTime fCont ? fCont.ToString("dd/MM/yyyy") : string.Empty
                };

                gridItems.Add(gridItem);
            }

            return new PagedTransactionResult
            {
                TotalRecords = totalRecords,
                FilteredRecords = filteredRecords,
                Data = gridItems
            };
        }

        private static string FormatDecimal(dynamic value)
        {
            if (value == null) return "0.00";
            if (decimal.TryParse(Convert.ToString(value), out decimal decVal))
            {
                return decVal.ToString("N2");
            }
            return Convert.ToString(value);
        }

        private static string FormatDifference(dynamic value)
        {
            if (value == null) return "0.00";
            if (decimal.TryParse(Convert.ToString(value), out decimal decVal))
            {
                string formatted = decVal.ToString("N2");
                if (decVal > 0)
                {
                    return $"<span class='over'>{formatted}</span>";
                }
                if (decVal < 0)
                {
                    return $"<span class='under'>{formatted}</span>";
                }
                return formatted;
            }
            return Convert.ToString(value);
        }
    }
}
