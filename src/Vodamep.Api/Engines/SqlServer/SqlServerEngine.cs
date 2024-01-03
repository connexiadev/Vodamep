using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.Security.Principal;
using Vodamep.Api.CmdQry;
using Vodamep.ReportBase;

namespace Vodamep.Api.Engines.SqlServer
{
    public class SqlServerEngine : IEngine
    {
        private readonly string _connectionString;
        private readonly ILogger<SqlServerEngine> _logger;
        private readonly DbUpdater _dbUpdater;
        private bool _dbUpdateDone = false;
        private AuthContext _authContext;

        public SqlServerEngine(SqlServerEngineConfiguration configuration, DbUpdater dbUpdater, ILogger<SqlServerEngine> logger)
        {
            _connectionString = configuration.ConnectionString;
            _dbUpdater = dbUpdater;
            _logger = logger;
        }

        public void Execute(ICommand cmd)
        {
            if (!_dbUpdateDone)
            {
                _dbUpdater.Upgrade(_connectionString);
                _dbUpdateDone = true;
            }

            switch (cmd)
            {
                case ReportSaveCommand save:
                    this.Save(save.Report);
                    return;
                case TestCommand test:
                    this.Test();
                    return;
            }

            throw new System.NotImplementedException();
        }

        public void Login(IPrincipal principal)
        {
            this._authContext = principal != null ? new AuthContext(principal) : null;
        }


        private void Test()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var cmd = new SqlCommand("select count(*) from Message", connection);

                var c = cmd.ExecuteScalar();
            }
        }

        private void Save(IReport report)
        {
            if (_authContext?.Principal == null)
            {
                _logger.LogError("Not logged in");
                throw new Exception("Not logged in");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Aktuelle Reportinformation extrahieren und Datenbank Report suchen
                ReportInfo sentInfo = ReportInfo.Create(report, -1, DateTime.Now);
                ReportInfo databaseInfo = GetReportInfoFromDatabase(connection, sentInfo);

                this.SaveReport(connection, report, sentInfo);
            }
        }



        /// <summary>
        /// Vorgänger Report anhand eines aktuellen Reports auslesen
        /// </summary>
        public IReport GetPrevious(IReport current)
        {
            IReport result = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                ReportInfo previousInfo = ReportInfo.CreatePrevious(current);

                if (previousInfo != null)
                {
                    ReportInfo previousInfoFromDatabase = GetReportInfoFromDatabase(connection, previousInfo, true);
                    if (previousInfoFromDatabase?.Data != null)
                    {
                        ReportFactory factory = new ReportFactory();
                        result = factory.Create(current.ReportType, previousInfoFromDatabase?.Data);
                    }
                }
            }

            return result;

        }


        /// <summary>
        /// Report Informationen aus der Datenbank auslesen
        /// </summary>
        private ReportInfo GetReportInfoFromDatabase(SqlConnection connection, ReportInfo reportInfo, bool withData = false)
        {
            try
            {
                int institutionId = this.GetRowId("Institution", reportInfo.Institution, connection);

                // and (isnull(State, '') in ('', 'OK')) ...

                // State kann
                // - null sein (default) - noch nicht importiert
                // - leer (falsches Update) - noch nicht importiert
                // - oder OK (nach Import) - bereits importiert

                // alles gilt als "aktives" Paket

                string dataField = ", Message.Data ";

                var command =

                    new SqlCommand
                    (
                        @$"Select top 1
                                  Message.Id
                                , Message.Month
                                , Message.Year
                                , Message.Hash_SHA256
                                , Message.Date 
                                {dataField}
                                , Institution.Name 
                           from Message 
                           inner join Institution on Institution.Id = Message.InstitutionId
                           where InstitutionId =  @institudionId 

                           and Month = @month
                           and Year = @year

                           and (isnull(State, '') in ('', 'OK'))

                           order by Date desc",

                        connection
                    );

                command.Parameters.AddWithValue("@institudionId", institutionId);
                command.Parameters.AddWithValue("@month", reportInfo.Month);
                command.Parameters.AddWithValue("@year", reportInfo.Year);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var result = new ReportInfo()
                        {
                            Month = reader.Get<short>("Month"),
                            Year = reader.Get<short>("Year"),
                            HashSHA256 = reader.Get<string>("Hash_SHA256"),
                            Created = reader.Get<DateTime>("Date"),
                            Institution = reader.Get<string>("Name"),
                        };

                        if (withData)
                            result.Data = reader.Get<byte[]>("Data");

                        return result;
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogInformation(exception.Message);
                throw;
            }


            return null;
        }


        private void SaveReport(SqlConnection connection, IReport report, ReportInfo info)
        {

            using (var ms = report.WriteToStream(asJson: false, compressed: true))
            {
                var institutionId = this.GetRowId("Institution", report.Institution.Id, connection);
                var userId = this.GetRowId("User", _authContext.Principal.Identity.Name, connection);

                SqlCommand insert =
                    new SqlCommand("insert into [Message]([UserId], [InstitutionId], [Hash_SHA256], [Month], [Year], [Date], [Data], [State]) values (@userId, @institutionId, @hash, @month, @year, @date, @data, @state)",
                        connection);
                insert.Parameters.AddWithValue("@userId", userId);
                insert.Parameters.AddWithValue("@institutionId", institutionId);
                insert.Parameters.AddWithValue("@hash", info.HashSHA256);
                insert.Parameters.AddWithValue("@month", info.Month);
                insert.Parameters.AddWithValue("@year", info.Year);
                insert.Parameters.AddWithValue("@date", DateTime.Now);
                insert.Parameters.AddWithValue("@state", "");
                insert.Parameters.AddWithValue("@data", ms.ToArray());
                try
                {
                    insert.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    throw;
                }

                _logger.LogInformation("Report saved");
            }
        }

        private int GetRowId(string tablename, string name, SqlConnection connection)
        {
            if (string.IsNullOrEmpty(name))
            {
                var msg = $"A name is required ({tablename})";
                _logger.LogError(msg);
                throw new Exception(msg);
            }

            var command = new SqlCommand($"SELECT Id from [{tablename}] where Name =  @name", connection);
            command.Parameters.AddWithValue("@name", name);

            var result = command.ExecuteScalar();

            if (result is int l)
                return l;


            var insert = new SqlCommand($"INSERT into [{tablename}] (Name) values (@name)", connection);
            insert.Parameters.AddWithValue("@name", name);
            insert.ExecuteNonQuery();

            result = command.ExecuteScalar();

            if (result is int l2)
                return l2;

            throw new Exception($"Can not resolve Id '{name}' from '{tablename}'");
        }


    }
}