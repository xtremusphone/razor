using Factory;
using System.Security;

namespace Model
{
    public enum LoginType
    {
        Windows,
        SSO,
        None
    };

    public struct LoginModel
    {
        private SecureString _password;

        public string UserName { get; set;}
        public string Password
        {           
            set
            {
                _password = value.ToSecureString();
            }
        }
        public string Domain{ get; set;}

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
    }

    public enum LoginStatus
    {
        Pass,       
        Failed
    }
    

    
}
