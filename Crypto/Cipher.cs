namespace DreamBot.Crypto
{
    public abstract class Cipher
    {
        public virtual void Decrypt(byte[] buffer)
        {
            Decrypt(buffer, 0, buffer, 0, buffer.Length);
        }

        public abstract void Decrypt(byte[] src, int srcOffset, byte[] dest, int destOffset, int count);

        public void Encrypt(byte[] buffer)
        {
            Encrypt(buffer, 0, buffer, 0, buffer.Length);
        }

        public abstract void Encrypt(byte[] src, int srcOffset, byte[] dest, int destOffset, int count);
    }
}