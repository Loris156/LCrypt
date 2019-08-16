namespace LCrypt.Algorithms
{
    public class FileHeaderV1
    {
        public int Pbkdf2Iterations { get; set; }

        public byte[] Salt { get; set; }

        public byte[] Iv { get; set; }
    }
}
