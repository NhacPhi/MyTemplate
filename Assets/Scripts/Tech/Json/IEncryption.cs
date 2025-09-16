namespace Tech.Json
{
    public interface IEncryption
    {
        public string Encrypt(string text);
        public string Decrypt(string text);
    }
}

