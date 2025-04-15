namespace Factory.Crypto
{
    internal class CipherString:Cipher
    {
        private readonly string secretMsg;

        internal CipherString(string msg) {
            secretMsg = EncryptString(msg);
        }


        internal string GetEncryptedMsg() { return secretMsg; }
        internal string GetDecryptedMsg() { return DecryptString(secretMsg); }
    }
}
