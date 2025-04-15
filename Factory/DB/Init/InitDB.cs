using Factory.DB.Model;
using Serilog;


namespace Factory.DB.Init
{
    public class InitDB
    {
        private readonly DBContext dbContext;
        public InitDB(string _dbName, IDatabaseService _dBContext)
        {
            dbContext = (DBContext?)_dBContext;
       
  
            //_ = dbContext.CreateDatabaseAsync(_dbName).Result;// for mysql only       

            try
            {

                var result = new List<Task>();
                var query = dbContext.QueryFactory.CreateTable(typeof(ModTableSSOUser));
                result.Add(dbContext.ExecuteNonQueryAsync(query));

                query = dbContext.QueryFactory.CreateTable(typeof(ModTableAuditLog));
                result.Add(dbContext.ExecuteNonQueryAsync(query));

                query = dbContext.QueryFactory.CreateTable(typeof(ModTableMachineLog));
                result.Add(dbContext.ExecuteNonQueryAsync(query));

                query = dbContext.QueryFactory.CreateTable(typeof(UserLoginLog));
                result.Add(dbContext.ExecuteNonQueryAsync(query));

                query = dbContext.QueryFactory.CreateTable(typeof(TrustedClient));
                result.Add(dbContext.ExecuteNonQueryAsync(query));


                var trustedClient = new TrustedClient()
                {
                    ClientId = "projectowl",
                    ClientName = "Olif",
                };

                result.Add(trustedClient.Save());


                trustedClient = new TrustedClient()
                {
                    ClientId = "projectwms",
                    ClientName = "WMS",
                };

                result.Add(trustedClient.Save());

                Task.WaitAll(result.ToArray());

                Log.Debug("Init DB done");

            }
            catch (Exception ex)
            {
                Log.Error("Init " + ex.Message);
            }
        }
    }
}
