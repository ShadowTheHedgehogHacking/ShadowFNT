using ShadowFNT;
using ShadowFNT.Structures;
using Xunit;

namespace ShadowFNTTest {
    public class Parsing {
        private const string parentDirectory = "Assets\\";
        private const string filterString = "Assets";

        [Fact]
        public void ReadFNT_Success() {
            var fileName = parentDirectory + Assets.Assets.STG0100EN_Original_FileName;
            var file = Assets.Assets.STG0100EN_Original();
            Assert.NotNull(file);
            var fnt = FNT.ParseFNTFile(fileName, ref file);
            Assert.Equal(fileName, fnt.fileName);
            Assert.Equal("", fnt.filterString);
            Assert.Equal(471, fnt.entryTable.Count);


            int expectedDataIndex = 120;
            Assert.Equal(23478, fnt.GetEntrySubtitleAddress(expectedDataIndex));
            Assert.Equal(64100, fnt.GetEntryMessageIdBranchSequence(expectedDataIndex));
            Assert.Equal(EntryType.TRIGGER_OBJECT, fnt.GetEntryEntryType(expectedDataIndex));
            Assert.Equal(172, fnt.GetEntryActiveTime(expectedDataIndex));
            Assert.Equal(1564, fnt.GetEntryAudioID(expectedDataIndex));
            Assert.Equal("This cage is protected by\nthose GUN soldiers.\0", fnt.GetEntrySubtitle(expectedDataIndex));
        }

        [Fact]
        public void ReadFNT_WithCorruptedSubtitleAddresses_And_Correct_Them() {
            // On detect FNT modified by ShadowTH Text Editor < 1.4.2

            // FNT modified with < 1.4.2 version
            var fileNameOldVersion = parentDirectory + Assets.Assets.STG0501EN_With_CorruptedSubtitleAddresses_FileName;
            var fileOldVersion = Assets.Assets.STG0501EN_With_CorruptedSubtitleAddresses();
            Assert.NotNull(fileOldVersion);
            var fntModifiedByOldVersion = FNT.ParseFNTFile(fileNameOldVersion, ref fileOldVersion, filterString);
            Assert.Equal(fileNameOldVersion, fntModifiedByOldVersion.fileName);
            Assert.Equal(filterString, fntModifiedByOldVersion.filterString);

            // FNT modified with > 1.4.2 version (1.4.5)
            var fileNameNewVersion = parentDirectory + Assets.Assets.STG0501EN_Without_CorruptedSubtitleAddresses_FileName;
            var fileNewVersion = Assets.Assets.STG0501EN_Without_CorruptedSubtitleAddresses();
            Assert.NotNull(fileNewVersion);
            var fntModifiedByNewVersion = FNT.ParseFNTFile(fileNameNewVersion, ref fileNewVersion, filterString);
            Assert.Equal(fileNameNewVersion, fntModifiedByNewVersion.fileName);
            Assert.Equal(filterString, fntModifiedByNewVersion.filterString);

            // Ensure counts match
            Assert.Equal(fntModifiedByOldVersion.entryTable.Count, fntModifiedByNewVersion.entryTable.Count);

            // Check non-corrupt entries match
            for (int i = 0; i < 390; i++) {
                Assert.Equal(fntModifiedByOldVersion.GetEntrySubtitleAddress(i), fntModifiedByNewVersion.GetEntrySubtitleAddress(i));
                Assert.Equal(fntModifiedByOldVersion.GetEntrySubtitle(i), fntModifiedByNewVersion.GetEntrySubtitle(i));
                Assert.Equal(fntModifiedByOldVersion.GetEntryMessageIdBranchSequence(i), fntModifiedByNewVersion.GetEntryMessageIdBranchSequence(i));
                Assert.Equal(fntModifiedByOldVersion.GetEntryAudioID(i), fntModifiedByNewVersion.GetEntryAudioID(i));
                Assert.Equal(fntModifiedByOldVersion.GetEntryActiveTime(i), fntModifiedByNewVersion.GetEntryActiveTime(i));
                Assert.Equal(fntModifiedByOldVersion.GetEntryEntryType(i), fntModifiedByNewVersion.GetEntryEntryType(i));
            }
            for (int i = 392; i < fntModifiedByOldVersion.entryTable.Count; i++) {
                Assert.Equal(fntModifiedByOldVersion.GetEntrySubtitleAddress(i), fntModifiedByNewVersion.GetEntrySubtitleAddress(i));
                Assert.Equal(fntModifiedByOldVersion.GetEntrySubtitle(i), fntModifiedByNewVersion.GetEntrySubtitle(i));
                Assert.Equal(fntModifiedByOldVersion.GetEntryMessageIdBranchSequence(i), fntModifiedByNewVersion.GetEntryMessageIdBranchSequence(i));
                Assert.Equal(fntModifiedByOldVersion.GetEntryAudioID(i), fntModifiedByNewVersion.GetEntryAudioID(i));
                Assert.Equal(fntModifiedByOldVersion.GetEntryActiveTime(i), fntModifiedByNewVersion.GetEntryActiveTime(i));
                Assert.Equal(fntModifiedByOldVersion.GetEntryEntryType(i), fntModifiedByNewVersion.GetEntryEntryType(i));
            }

            // entry 389, 390, 391 has corruption on modifying entry 461
            // successive address corruption, so only 390 and 391 will have mismatch
            // Check corrupt entries do not match
            for (int i = 390; i < 392; i++) {
                // expect everything to match except subtitle address
                Assert.Equal(fntModifiedByOldVersion.GetEntrySubtitle(i), fntModifiedByNewVersion.GetEntrySubtitle(i));
                Assert.Equal(fntModifiedByOldVersion.GetEntryMessageIdBranchSequence(i), fntModifiedByNewVersion.GetEntryMessageIdBranchSequence(i));
                Assert.Equal(fntModifiedByOldVersion.GetEntryAudioID(i), fntModifiedByNewVersion.GetEntryAudioID(i));
                Assert.Equal(fntModifiedByOldVersion.GetEntryActiveTime(i), fntModifiedByNewVersion.GetEntryActiveTime(i));
                Assert.Equal(fntModifiedByOldVersion.GetEntryEntryType(i), fntModifiedByNewVersion.GetEntryEntryType(i));

                Assert.NotEqual(fntModifiedByOldVersion.GetEntrySubtitleAddress(i), fntModifiedByNewVersion.GetEntrySubtitleAddress(i));
            }
            // Should fail EQ as subtitle addresses mismatch 
            Assert.False(FNT.Equals(fntModifiedByOldVersion, fntModifiedByNewVersion));

            // Recalculate all subtitle addresses
            fntModifiedByOldVersion.RecomputeAllSubtitleAddresses();

            // Corruption should be fixed
            for (int i = 0; i < fntModifiedByOldVersion.entryTable.Count; i++) {
                Assert.Equal(fntModifiedByOldVersion.GetEntrySubtitleAddress(i), fntModifiedByNewVersion.GetEntrySubtitleAddress(i));
                Assert.Equal(fntModifiedByOldVersion.GetEntrySubtitle(i), fntModifiedByNewVersion.GetEntrySubtitle(i));
            }
            Assert.True(FNT.Equals(fntModifiedByOldVersion, fntModifiedByNewVersion));
        }
    }
}
