using Serilog;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace Factory.Crypto
{
    /// <summary>
    /// Cipher - Providing Encryption and Decryption library
    /// Set to Internal to prevent library accessed by third party software
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal class Cipher:Keyman
    {
        protected string salt = "";

        protected Cipher()
        {
            
        }

        /// <summary>
        /// EncryptString - return as base64 string
        /// Max encryption data length is 200
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="CryptographicException"></exception>
        protected string EncryptString(string data)
        {
            var saltedData = $"{salt}{data}";
            try
            {
                // Combine data with the salt
                byte[] dataToEncrypt = Encoding.UTF8.GetBytes(saltedData);

                // Get the public key
                string publicKey = GetPublicKey();

                // Initialize the RSACryptoServiceProvider
                using (var rsa = new RSACryptoServiceProvider())
                {
                    // Load the public key
                    rsa.FromXmlString(publicKey);

                    byte[] encryptedData;

                    int maxLength = (rsa.KeySize / 8) - 11; // 11 is the PKCS#1 padding overhead

                    // Check if the data is within the maximum size for encryption
                    if (dataToEncrypt.Length <= maxLength)
                    {
                        encryptedData = rsa.Encrypt(dataToEncrypt, true);
                        return Convert.ToBase64String(encryptedData);
                    }
                    else
                    {
                        throw new Exception($"Data '{data}-{salt}' length {dataToEncrypt.Length} is too long for encryption using this key size. Maxlength: {maxLength}");
                    }
          
                }
            }
            catch (CryptographicException ex)
            {
                // Log the exception and re-throw a more general CryptographicException
                var funcName = string.Format("{0} : {1}", new StackFrame().GetMethod().DeclaringType.FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                Log.Error("{funcName}: {error}", funcName, ex.Message);
                throw new CryptographicException("Error in encryption");
            }
            catch (Exception ex)
            {
                // Log other exceptions and throw a CryptographicException
                var funcName = string.Format("{0} : {1}", new StackFrame().GetMethod().DeclaringType.FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                Log.Error("{funcName}: {error}", funcName, ex.Message);
                throw new CryptographicException("An unexpected error occurred");
            }
        }

        /// <summary>
        /// DecryptString
        /// </summary>
        /// <param name="encryptedBase64Str"></param>
        /// <returns>original string</returns>
        /// <exception cref="CryptographicException"></exception>
        protected string DecryptString(string encryptedBase64Str)
        {
            try
            {
                // Get the private key
                string privateKey = GetPrivateKey();

                // Initialize the RSACryptoServiceProvider
                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.FromXmlString(privateKey);

                    // Convert the encrypted data back to bytes
                    byte[] dataToDecrypt = Convert.FromBase64String(encryptedBase64Str);

                    // Decrypt the data
                    byte[] decryptedData = rsa.Decrypt(dataToDecrypt, true);

                    // Convert the decrypted bytes back to string
                    string decryptedText = Encoding.UTF8.GetString(decryptedData);

                    // Extract original data by splitting on the salt
                    var originalData = decryptedText.Substring(salt.Length);
                    //string originalData = decryptedText.Split(new[] { '-' }, 2)[0];

                    return originalData;
                }
            }
            catch (CryptographicException ex)
            {
                // Log the exception and re-throw a more general CryptographicException
                var funcName = string.Format("{0} : {1}", new StackFrame().GetMethod().DeclaringType.FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                Log.Error("{funcName}: {error}", funcName, ex.Message);
                throw new CryptographicException("Error in decryption");
            }
            catch (Exception ex)
            {
                // Log other exceptions and throw a CryptographicException
                var funcName = string.Format("{0} : {1}", new StackFrame().GetMethod().DeclaringType.FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                Log.Error("{funcName}: {error}", funcName, ex.Message);
                throw new CryptographicException("An unexpected error occurred");
            }
        }



        /// <summary>
        /// AES Encrypt string
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="Key"></param>
        /// <param name="IV"></param>
        /// <returns>byte[]</returns>
        /// <exception cref="ArgumentNullException"></exception>
        internal static byte[] AesEncryptString(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException(nameof(plainText));
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException(nameof(Key));
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException(nameof(IV));
            byte[] encrypted;

            // Create an Aes object with the specified key and IV.
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                // Create the streams used for encryption.
                using MemoryStream msEncrypt = new MemoryStream();
                using CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                {
                    //Write all data to the stream.
                    swEncrypt.Write(plainText);
                }
                encrypted = msEncrypt.ToArray();
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        /// <summary>
        /// AES Decrypt string
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="Key"></param>
        /// <param name="IV"></param>
        /// <returns>string</returns>
        /// <exception cref="ArgumentNullException"></exception>
        internal static string AesDecryptString(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException(nameof(cipherText));
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException(nameof(Key));
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException(nameof(IV));

            // Declare the string used to hold the decrypted text.
            string plaintext = null;

            // Create an Aes object with the specified key and IV.
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                // Create the streams used for decryption.
                using MemoryStream msDecrypt = new MemoryStream(cipherText);
                using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using StreamReader srDecrypt = new StreamReader(csDecrypt);

                // Read the decrypted bytes from the decrypting stream
                // and place them in a string.
                plaintext = srDecrypt.ReadToEnd();
            }

            return plaintext;
        }
    }


}
