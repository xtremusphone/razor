using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Security;
using System.Text;

namespace Cryptolib2
{
    public class Crypto
    {
        static readonly string PasswordHash = "cf5tmr2tm8oimrhfUh8TVbgx+iiYVqd6hCq2KuRzptc=";
        static readonly string SaltKey = "A11i@nC3S@LT&KEY";
        static readonly string VIKey = "@1B2c3D4e5F6g7H8";

        X509Certificate2 encryptionCertificate = null;

        public Crypto(string certificateName, StoreLocation storeLocation = StoreLocation.LocalMachine)
        {
            //encryptionCertificate = GetEncryptionCertificate(certificateName, storeLocation);
        }

        public static List<string> GetCertificates(StoreLocation storeLocation = StoreLocation.LocalMachine)
        {
            List<string> results = new List<string>();
            X509Store certificateStore = new X509Store(storeLocation);
            certificateStore.Open(OpenFlags.ReadOnly);

            foreach (var certificate in certificateStore.Certificates)
            {
                results.Add(certificate.FriendlyName);

            }

            certificateStore.Close();
            return results;

        }

        private X509Certificate2 GetEncryptionCertificate(string certificateName, StoreLocation storeLocation)
        {
            X509Certificate2 encryptionCertificate = null;
            X509Store certificateStore = new X509Store(storeLocation);
            certificateStore.Open(OpenFlags.ReadOnly);

            foreach (var certificate in certificateStore.Certificates)
            {
                if (certificate.FriendlyName == certificateName)
                {
                    encryptionCertificate = certificate;
                    break;
                }
            }

            certificateStore.Close();

            if (encryptionCertificate == null)
            {
                throw new Exception("Certificate Not Found");
            }

            return encryptionCertificate;
        }

        public string Encrypt(string dataToEncrypt)
        {
            if (dataToEncrypt == null || dataToEncrypt.Length < 1)
            {
                throw new ArgumentNullException("dataToEncrypt");
            }

            return EncryptText(dataToEncrypt);
        }
        public dynamic Decrypt(string dataToDecrypt, bool asSecureString = true)
        {
            if (dataToDecrypt == null || dataToDecrypt.Length < 1)
            {
                throw new ArgumentNullException("dataToDecrypt");
            }
            return DecryptText(dataToDecrypt, asSecureString);
        }

        public string EncryptRSA(string dataToEncrypt)
        {
            if (dataToEncrypt == null || dataToEncrypt.Length < 1)
            {
                throw new ArgumentNullException("dataToEncrypt");
            }

            var data = Encoding.UTF8.GetBytes(dataToEncrypt);
            var encrypted = Encrypt(data);

            return Convert.ToBase64String(encrypted);
        }

        public dynamic DecryptRSA(string dataToDecrypt, bool asSecureString = true)
        {
            if (dataToDecrypt == null || dataToDecrypt.Length < 1)
            {
                throw new ArgumentNullException("dataToDecrypt");
            }

            var data = Convert.FromBase64String(dataToDecrypt);
            var decrypted = Decrypt(data);

            string source = Encoding.UTF8.GetString(decrypted);

            if (asSecureString)
            {
                SecureString result = new SecureString();
                foreach (char c in source.ToCharArray())
                    result.AppendChar(c);
                return result;
            }
            else
                return source;

        }

        //public SecureString Decrypt(string dataToDecrypt)
        //{
        //    if (dataToDecrypt == null || dataToDecrypt.Length < 1)
        //    {
        //        throw new ArgumentNullException("dataToDecrypt");
        //    }

        //    var data = Convert.FromBase64String(dataToDecrypt);
        //    var decrypted = Decrypt(data);

        //    string source = Encoding.UTF8.GetString(decrypted);

        //    SecureString result = new SecureString();
        //    foreach (char c in source.ToCharArray())
        //        result.AppendChar(c);
        //    return result;

        //}

        public byte[] Encrypt(byte[] dataToEncrypt)
        {
            if (dataToEncrypt == null || dataToEncrypt.Length < 1)
            {
                throw new ArgumentNullException("dataToEncrypt");
            }

            using (RSA rsa = encryptionCertificate.GetRSAPublicKey())
            {
                return rsa.Encrypt(dataToEncrypt, RSAEncryptionPadding.Pkcs1);
            }


        }

        public byte[] Decrypt(byte[] dataToDecrypt)
        {
            if (dataToDecrypt == null || dataToDecrypt.Length < 1)
            {
                throw new ArgumentNullException("dataToDecrypt");
            }

            using (RSA rsa = encryptionCertificate.GetRSAPrivateKey())
            {
                return rsa.Decrypt(dataToDecrypt, RSAEncryptionPadding.Pkcs1);
            }

        }

        public static string EncryptText(string plainText)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.Zeros };
            var encryptor = symmetricKey.CreateEncryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));

            byte[] cipherTextBytes;

            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    cipherTextBytes = memoryStream.ToArray();
                    cryptoStream.Close();
                }
                memoryStream.Close();
            }
            return Convert.ToBase64String(cipherTextBytes);
        }

        public static dynamic DecryptText(string encryptedText, bool asSecureString = true)
        {
            byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);
            byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.None };

            var decryptor = symmetricKey.CreateDecryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));
            var memoryStream = new MemoryStream(cipherTextBytes);
            var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];

            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();

            string source = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
            if (asSecureString)
            {
                SecureString result = new SecureString();
                foreach (char c in source.ToCharArray())
                    result.AppendChar(c);
                return result;
            }
            else
                return source;

        }
    }
}
