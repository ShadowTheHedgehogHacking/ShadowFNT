using System.IO;

namespace ShadowFNTTest.Assets
{
    public static class Assets
    {
        public static byte[] CleanSTG0100EN() => File.ReadAllBytes("Assets/stg0100_EN.fnt");
    }
}
