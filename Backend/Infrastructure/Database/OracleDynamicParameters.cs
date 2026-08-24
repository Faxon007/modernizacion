using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using Oracle.ManagedDataAccess.Client;

namespace Backend.Infrastructure.Database
{
    /// <summary>
    /// Clase auxiliar para mapear parámetros específicos de Oracle (usando OracleDbType) con Dapper.
    /// </summary>
    public class OracleDynamicParameters : SqlMapper.IDynamicParameters, SqlMapper.IParameterCallbacks
    {
        private readonly List<OracleParameter> _oracleParameters = new();
        private readonly Dictionary<string, OracleParameter> _attachedParameters = new(StringComparer.OrdinalIgnoreCase);

        public void Add(string name, object? value = null, OracleDbType? dbType = null, ParameterDirection? direction = null, int? size = null)
        {
            var parameter = new OracleParameter
            {
                ParameterName = name,
                Value = value ?? DBNull.Value
            };

            if (dbType.HasValue)
            {
                parameter.OracleDbType = dbType.Value;
            }

            if (direction.HasValue)
            {
                parameter.Direction = direction.Value;
            }

            if (size.HasValue)
            {
                parameter.Size = size.Value;
            }

            _oracleParameters.Add(parameter);
        }

        public void Add(string name, object? value = null, DbType? dbType = null, ParameterDirection? direction = null, int? size = null)
        {
            var parameter = new OracleParameter
            {
                ParameterName = name,
                Value = value ?? DBNull.Value
            };

            if (dbType.HasValue)
            {
                parameter.DbType = dbType.Value;
            }

            if (direction.HasValue)
            {
                parameter.Direction = direction.Value;
            }

            if (size.HasValue)
            {
                parameter.Size = size.Value;
            }

            _oracleParameters.Add(parameter);
        }

        public void AddParameters(IDbCommand command, SqlMapper.Identity identity)
        {
            if (command is OracleCommand oracleCommand)
            {
                oracleCommand.BindByName = true;
                _attachedParameters.Clear();
                
                foreach (var parameter in _oracleParameters)
                {
                    var clone = (OracleParameter)((ICloneable)parameter).Clone();
                    oracleCommand.Parameters.Add(clone);
                    _attachedParameters[clone.ParameterName] = clone;
                }
            }
        }

        public void OnCompleted()
        {
            // Sync values back to original parameters if needed, but we read from _attachedParameters directly in Get<T>
        }

        public T? Get<T>(string name)
        {
            string searchName = name.StartsWith(":") || name.StartsWith("@") || name.StartsWith("p_") || name.StartsWith("P_") ? name : name;
            
            if (_attachedParameters.TryGetValue(searchName, out var attachedParam))
            {
                var val = attachedParam.Value;
                if (val == DBNull.Value || val == null)
                {
                    return default;
                }
                
                // Tratar OracleString especial
                if (val is Oracle.ManagedDataAccess.Types.OracleString oracleString)
                {
                    if (oracleString.IsNull) return default;
                    val = oracleString.Value;
                }
                
                // Tratar OracleDecimal
                if (val is Oracle.ManagedDataAccess.Types.OracleDecimal oracleDecimal)
                {
                    if (oracleDecimal.IsNull) return default;
                    val = oracleDecimal.Value;
                }

                if (val is T typedVal)
                {
                    return typedVal;
                }

                return (T)Convert.ChangeType(val, typeof(T));
            }

            // Fallback en caso de que no encontremos el exacto
            foreach (var key in _attachedParameters.Keys)
            {
                if (key.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    var val = _attachedParameters[key].Value;
                    if (val == DBNull.Value || val == null) return default;
                    if (val is Oracle.ManagedDataAccess.Types.OracleString os) { if (os.IsNull) return default; val = os.Value; }
                    if (val is Oracle.ManagedDataAccess.Types.OracleDecimal od) { if (od.IsNull) return default; val = od.Value; }
                    if (val is T typedVal) return typedVal;
                    return (T)Convert.ChangeType(val, typeof(T));
                }
            }

            return default;
        }
    }
}
