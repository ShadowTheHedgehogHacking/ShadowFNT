using System.IO;

namespace ShadowFNTTest.Assets
{
    public static class Assets
    {
        public const string STG0100EN_Original_FileName = "stg0100_EN.fnt";
        public const string STG0501EN_With_CorruptedSubtitleAddresses_FileName = "corrupted_stg0501_EN_initial_state.fnt";
        public const string STG0501EN_Without_CorruptedSubtitleAddresses_FileName = "stg0501_EN_original_to_beetle_but_in_1.4.5.fnt";

        public static byte[] STG0100EN_Original() => File.ReadAllBytes("Assets/" + STG0100EN_Original_FileName);
        public static byte[] STG0501EN_With_CorruptedSubtitleAddresses() => File.ReadAllBytes("Assets/" + STG0501EN_With_CorruptedSubtitleAddresses_FileName);
        public static byte[] STG0501EN_Without_CorruptedSubtitleAddresses() => File.ReadAllBytes("Assets/" + STG0501EN_Without_CorruptedSubtitleAddresses_FileName);


    }
}
