using Factory;
using static Factory.DB.DBContext;
using Razor01.Global;
using System.Security;
using System.Data;
using Factory.DB;
using System.Threading.Tasks;

namespace Model
{
    public enum LoginStatus
    {
        Pass,
        Failed,
        Locked
    }

    public class LoginModel(IDatabaseService db)
    {
        private IDatabaseService _db = db;
        private SecureString _password;

        public string UserName { get; set; }
        public string Password
        {
            set
            {
                _password = value.ToSecureString();
            }
        }
        public string Domain { get; set; }
        public string Email { get; set; }
        public bool IsNew { get; set; }

        public string GetPasswordAsString()
        {
            return _password.ToCString();
        }

        public SecureString GetPasswordAsSecureString()
        {
            return _password;
        }

        public void SetPassword(SecureString securePwd)
        {
            _password = securePwd;
        }

        public async Task UpdateNewUserAsync(string newUsername)
        {
            var param = new DynamicSqlParameter(GlobalConfig.Instance.DBType.ToEnum<DBType>());
            param.Add("@username", UserName);
            param.Add("@newusername", newUsername);

            var updateQuery = $"UPDATE common.user SET username=@newusername , is_new=false WHERE username=@username";

            UserName = newUsername;
            IsNew = false;

            await db.ExecuteNonQueryAsync(updateQuery, param);
        }

        public async Task<LoginStatus> Login()
        {
            var param = new DynamicSqlParameter(GlobalConfig.Instance.DBType.ToEnum<DBType>());
            param.Add("@username", UserName);

            var query = $"SELECT * FROM common.user WHERE username=@username";
            var temp = db.GetDataSetAsync(query, param);

            var dataReader = temp.Result.CreateDataReader();

            if (dataReader.HasRows == false)
            {
                return LoginStatus.Failed;
            }

            // To move the row pointer
            dataReader.Read();

            IsNew = dataReader.GetBoolean("is_new");
            var loginTryCount = dataReader.GetInt32("retry_count");
            
            if (BCrypt.Net.BCrypt.Verify(GetPasswordAsString(), dataReader.GetString("password")) == false)
            {
                var lockQuery = "";

                if (loginTryCount + 1 > 5)
                {
                    lockQuery = ", disable = true ";
                }

                var retryUpdQuery = $"UPDATE common.user SET retry_count = retry_count + 1 {lockQuery} WHERE username=@username";
                await db.ExecuteNonQueryAsync(retryUpdQuery, param);

                return LoginStatus.Failed;
            }
            else
            {
                if (dataReader.GetBoolean("disable"))
                {
                    return LoginStatus.Locked;
                }
            }

            await LoginSuccessAsync(param, "username/password");

            return LoginStatus.Pass;
        }

        public async void Unlock()
        {
            var param = new DynamicSqlParameter(GlobalConfig.Instance.DBType.ToEnum<DBType>());
            param.Add("@username", UserName);

            var unlockQuery = "UPDATE common.user SET retry_count = 0, disable = false WHERE username=@username";

            await db.ExecuteNonQueryAsync(unlockQuery, param);
        }

        public async void Disable()
        {
            var param = new DynamicSqlParameter(GlobalConfig.Instance.DBType.ToEnum<DBType>());
            param.Add("@username", UserName);

            var disableQuery = "UPDATE common.user SET disable = true WHERE username=@username";

            await db.ExecuteNonQueryAsync(disableQuery, param);
        }

        public async Task<LoginStatus> LoginViaIDP(string idp, string authToken)
        {
            var param = new DynamicSqlParameter(GlobalConfig.Instance.DBType.ToEnum<DBType>());
            param.Add("@username", UserName);

            await LoginSuccessAsync(param, idp, authToken);

            return LoginStatus.Pass;
        }

        private async Task LoginSuccessAsync(DynamicSqlParameter param, string method, string token = null)
        {
            var tokenProvided = "";
            if (token != null)
            {
                tokenProvided = $"last_idp_token = @token,";
                param.Add("@token", token);
            }

            var resetQuery = $"UPDATE common.user SET {tokenProvided} retry_count = 0, disable = false, last_login_method = '{method}' , last_login_timestamp = NOW() WHERE username=@username";
            await db.ExecuteNonQueryAsync(resetQuery, param);
        }

        public static LoginModel GetUserByEmail(IDatabaseService db, string email)
        {
            var param = new DynamicSqlParameter(GlobalConfig.Instance.DBType.ToEnum<DBType>());
            param.Add("@email", email);

            var findByEmailQuery = "SELECT * FROM common.user WHERE email=@email";

            var temp = db.GetDataSetAsync(findByEmailQuery, param);
            var dataReader = temp.Result.CreateDataReader();

            if (dataReader.HasRows == false)
            {
                throw new Exception("Invalid email");
            }

            dataReader.Read();

            return new LoginModel(db)
            {
                UserName = dataReader.GetString("username"),
                Email = dataReader.GetString("email"),
                IsNew = dataReader.GetBoolean("is_new")
            };
        }

        public static LoginModel GetUserByUsername(IDatabaseService db, string username)
        {
            var param = new DynamicSqlParameter(GlobalConfig.Instance.DBType.ToEnum<DBType>());
            param.Add("@username", username);

            var findByUsernameQuery = "SELECT * FROM common.user WHERE username=@username";

            var temp = db.GetDataSetAsync(findByUsernameQuery, param);
            var dataReader = temp.Result.CreateDataReader();

            if (dataReader.HasRows == false)
            {
                throw new Exception("Invalid username");
            }

            dataReader.Read();

            return new LoginModel(db)
            {
                UserName = dataReader.GetString("username"),
                Email = dataReader.GetString("email"),
                IsNew = dataReader.GetBoolean("is_new")
            };
        }

        public static async Task<LoginModel> RegisterForIDP(IDatabaseService db, string email, string code)
        {
            var param = new DynamicSqlParameter(GlobalConfig.Instance.DBType.ToEnum<DBType>());
            param.Add("@email", email);
            param.Add("@code", code);

            // Temporary use code as the user id. Will be reset by user later
            var createNewQuery = "INSERT INTO common.user (username, email) VALUES (@code, @email)";

            await db.ExecuteNonQueryAsync(createNewQuery, param);

            return new LoginModel(db)
            {
                UserName = code,
                Email = email,
                IsNew = true
            };
        }
    }
}
