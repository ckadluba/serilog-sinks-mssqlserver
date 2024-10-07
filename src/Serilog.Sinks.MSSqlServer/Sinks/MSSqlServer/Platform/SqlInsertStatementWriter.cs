using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer.Output;

namespace Serilog.Sinks.MSSqlServer.Platform
{
    internal class SqlInsertStatementWriter : ISqlBulkBatchWriter, ISqlLogEventWriter
    {
        private readonly string _tableName;
        private readonly string _schemaName;
        private readonly ISqlConnectionFactory _sqlConnectionFactory;
        private readonly ILogEventDataGenerator _logEventDataGenerator;
        private readonly StringBuilder _fieldList = new StringBuilder();
        private string _fieldListSql = string.Empty;

        public SqlInsertStatementWriter(
            string tableName,
            string schemaName,
            ISqlConnectionFactory sqlConnectionFactory,
            ILogEventDataGenerator logEventDataGenerator)
        {
            _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            _schemaName = schemaName ?? throw new ArgumentNullException(nameof(schemaName));
            _sqlConnectionFactory =
                sqlConnectionFactory ?? throw new ArgumentNullException(nameof(sqlConnectionFactory));
            _logEventDataGenerator =
                logEventDataGenerator ?? throw new ArgumentNullException(nameof(logEventDataGenerator));
        }

        public Task WriteBatch(IEnumerable<LogEvent> events, DataTable dataTable) => WriteBatch(events);

        public void WriteEvent(LogEvent logEvent) => WriteBatch(new[] { logEvent }).GetAwaiter().GetResult();

        public async Task WriteBatch(IEnumerable<LogEvent> events)
        {
            try
            {
                using (var cn = _sqlConnectionFactory.Create())
                {
                    await cn.OpenAsync().ConfigureAwait(false);
                    using (var command = cn.CreateCommand())
                    {
                        command.CommandType = CommandType.Text;

                        foreach (var logEvent in events)
                        {
                            // Optimization: fieldlist is equal for all log events, so create it only once
                            if (_fieldListSql == string.Empty)
                            {
                                CreateFieldListSql(logEvent);
                            }


                            var parameterList = new StringBuilder(") VALUES (");

                            var index = 0;
                            foreach (var field in _logEventDataGenerator.GetColumnsAndValues(logEvent))
                            {
                                if (index != 0)
                                {
                                    parameterList.Append(',');
                                }

                                parameterList.Append("@P");
                                parameterList.Append(index);

                                command.AddParameter("@P" + index, field.Value);

                                index++;
                            }

                            parameterList.Append(')');

                            command.CommandText += _fieldListSql + parameterList + ";" + Environment.NewLine;
                        }

                        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine("Unable to write log event to the database due to following error: {0}", ex);
                throw;
            }
        }

        private void CreateFieldListSql(LogEvent logEvent)
        {
            _fieldList.Append("INSERT INTO [");
            _fieldList.Append(_schemaName);
            _fieldList.Append("].[");
            _fieldList.Append(_tableName);
            _fieldList.Append("] (");

            var index = 0;
            foreach (var field in _logEventDataGenerator.GetColumnsAndValues(logEvent))
            {
                if (index != 0)
                {
                    _fieldList.Append(',');
                }

                _fieldList.Append('[');
                _fieldList.Append(field.Key);
                _fieldList.Append(']');

                index++;
            }

            _fieldListSql = _fieldList.ToString();
        }
    }
}
