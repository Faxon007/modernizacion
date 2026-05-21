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
    public class OracleDynamicParameters : SqlMapper.IDynamicParameters
    {
        private readonly List<OracleParameter> _oracleParameters = new();

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

        public void AddParameters(IDbCommand command, SqlMapper.Identity identity)
        {
            if (command is OracleCommand oracleCommand)
            {
                foreach (var parameter in _oracleParameters)
                {
                    oracleCommand.Parameters.Add(parameter);
                }
            }
        }

        public T? Get<T>(string name)
        {
            foreach (var parameter in _oracleParameters)
            {
                if (parameter.ParameterName == name)
                {
                    var val = parameter.Value;
                    if (val == DBNull.Value || val == null)
                    {
                        return default;
                    }
                    
                    if (val is T typedVal)
                    {
                        return typedVal;
                    }

                    return (T)Convert.ChangeType(val, typeof(T));
                }
            }
            return default;
        }
    }
}
