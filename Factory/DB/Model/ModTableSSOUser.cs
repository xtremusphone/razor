
namespace Factory.DB.Model
{
    [SqlTable("tbl_sso")]
    public class ModTableSSOUser : IDBObjectBase
    {

        [SqlProperty("id", DataType.INT)]
        public int Id { get; set; }

        [SqlPrimaryKey]
        [SqlProperty("username", DataType.TEXT)]
        public string? UserName { get; set; }

        [SqlProperty("etkiv", DataType.TEXT)]
        public string? ETKiv { get; set; }

        [SqlProperty("token", DataType.TEXT)]
        public string? EncryptedRefreshToken { get; set; }

        [SqlProperty("expiryDate", DataType.DATETIME)]
        public string? RefreshTokenExpireDate { get; set; }

        [SqlProperty("logDate", DataType.DATETIME)]
        public string LogDate { get; set; }

        public ModTableSSOUser() { 
            LogDate = string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
        }

        public ModTableSSOUser(string username)
        {
            UserName = username;
            LogDate = string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
        }

        /// <summary>
        /// Initialize ModTableSSOUser
        /// </summary>
        /// <param name="username"></param>
        /// <param name="etkiv"> </param>
        /// <param name="encryptedRefreshToken"> Only send in encrypted refresh token!</param>
        /// <param name="refreshTokenExpireDate"></param>
        public ModTableSSOUser(string username, string etkiv, string encryptedRefreshToken, DateTime refreshTokenExpireDate)
        {
            UserName = username;
            ETKiv = etkiv;
            EncryptedRefreshToken = encryptedRefreshToken;
            RefreshTokenExpireDate = string.Format("{0:yyyy-MM-dd HH:mm:ss}", refreshTokenExpireDate);
            LogDate = string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
        }

        //public string getDecryptedToken()
        //{
        //    return Crypto.Cipher.Instance.DecryptString(_refreshToken);
        //}

  

    }


    [SqlTable("tbl_audit_log")]
    public class ModTableAuditLog: IDBObjectBase
    {
        [SqlPrimaryKey]
        [SqlAutoIncrement]
        [SqlProperty("id", DataType.INT)]
        public int Id { get; set; }

        [SqlProperty("username", DataType.TEXT)]
        public string? UserName { get; set; }

        [SqlProperty("action", DataType.TEXT)]
        public string? Action { get; set; }

        [SqlProperty("action_desc", DataType.TEXT)]
        public string? ActionDesc { get; set; }

        [SqlProperty("logDate", DataType.DATETIME)]
        public string? LogDate { get; set; }

        public ModTableAuditLog() { LogDate = string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now); }

        public ModTableAuditLog(string username, string action, string actionDesc)
        {
            UserName = username;
            Action = action;
            ActionDesc = actionDesc;
            LogDate = string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);

        }


    }

    [SqlTable("tbl_machine_log")]
    public class ModTableMachineLog: IDBObjectBase
    {
        [SqlPrimaryKey]
        [SqlAutoIncrement]
        [SqlProperty("id", DataType.INT)]
        public int Id { get; set; }


        [SqlProperty("fingerprint", DataType.TEXT)]
        public string? Fingerprint { get; set; }


        [SqlProperty("logDate", DataType.DATETIME)]
        public string LogDate { get; set; }

        public ModTableMachineLog() { LogDate = string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now); }

        public ModTableMachineLog(string fingerprint)
        {
            Fingerprint = fingerprint;
            LogDate = string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);

        }


    }

    [SqlTable("tbl_userlogin_log")]
    public class UserLoginLog:IDBObjectBase
    {
        [SqlProperty("id", DataType.INT)]
        public int Id { get; set; }

        [SqlPrimaryKey]
        [SqlProperty("sid", DataType.TEXT)]
        public string Sid { get; set; }

        [SqlProperty("username", DataType.TEXT)]
        public string UserName { get; set; }

        [SqlProperty("domain", DataType.TEXT)]
        public string Domain { get; set; }

        [SqlProperty("ssoAuthStatus", DataType.TEXT)]
        public string SSOAuthStatus { get; set; }

        [SqlProperty("winAuthStatus", DataType.TEXT)]
        public string WinAuthStatus { get; set; }

        [SqlProperty("deviceIdCheck", DataType.BOOL)]
        public bool DeviceIdCheck { get; set; }

        [SqlProperty("ssohealthstatus", DataType.BOOL)]
        public bool SSOHealthStatus { get; set; }

        [SqlProperty("sessionId", DataType.TEXT)]
        public string? SessionId { get; set; }

        [SqlProperty("logindate", DataType.DATETIME)]
        public DateTime LoginDate { get; set; }

    }


    [SqlTable("tbl_trusted_client")]
    public class TrustedClient : IDBObjectBase
    {
        [SqlProperty("id", DataType.INT)]
        public int Id { get; set; }

        [SqlPrimaryKey]
        [SqlProperty("client_id", DataType.TEXT)]
        public string ClientId { get; set; }

        [SqlProperty("client_name", DataType.TEXT)]
        public string ClientName { get; set; }

    }
}
