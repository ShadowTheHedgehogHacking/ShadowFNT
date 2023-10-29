using ShadowFNT;
using ShadowFNT.Structures;
using System.Collections.Generic;
using Xunit;

namespace ShadowFNTTest {
    public class Modifying {
        private const string parentDirectory = "Assets\\";
        private const string filterString = "Assets";


        [Fact]
        public void ModifyExistingEntries() {
            var fileName = parentDirectory + Assets.Assets.STG0100EN_Original_FileName;
            var file = Assets.Assets.STG0100EN_Original();
            Assert.NotNull(file);
            var fnt = FNT.ParseFNTFile(fileName, ref file, filterString);
            Assert.Equal(fileName, fnt.fileName);
            Assert.Equal(filterString, fnt.filterString);
            Assert.Equal(471, fnt.entryTable.Count);
            Assert.Equal(Assets.Assets.STG0100EN_Original_FileName, fnt.ToString());

            int expectedDataIndex = 120;
            Assert.Equal(23478, fnt.GetEntrySubtitleAddress(expectedDataIndex));
            Assert.Equal(64100, fnt.GetEntryMessageIdBranchSequence(expectedDataIndex));
            Assert.Equal(EntryType.TRIGGER_OBJECT, fnt.GetEntryEntryType(expectedDataIndex));
            Assert.Equal(172, fnt.GetEntryActiveTime(expectedDataIndex));
            Assert.Equal(1564, fnt.GetEntryAudioID(expectedDataIndex));
            Assert.Equal("This cage is protected by\nthose GUN soldiers.\0", fnt.GetEntrySubtitle(expectedDataIndex));

            // TODO: Write ModifyExistingEntries Test
        }

        [Fact]
        public void AddNewEntry() {
            var fileName = parentDirectory + Assets.Assets.STG0100EN_Original_FileName;
            var file = Assets.Assets.STG0100EN_Original();
            Assert.NotNull(file);
            var fnt = FNT.ParseFNTFile(fileName, ref file, filterString);
            Assert.Equal(fileName, fnt.fileName);
            Assert.Equal(filterString, fnt.filterString);
            Assert.Equal(471, fnt.entryTable.Count);

            // TODO: Write AddNewEntry Test
        }

        [Fact]
        public void DeleteEntry() {
            var fileName = parentDirectory + Assets.Assets.STG0100EN_Original_FileName;
            var file = Assets.Assets.STG0100EN_Original();
            Assert.NotNull(file);
            var fnt = FNT.ParseFNTFile(fileName, ref file, filterString);
            Assert.Equal(fileName, fnt.fileName);
            Assert.Equal(filterString, fnt.filterString);
            Assert.Equal(471, fnt.entryTable.Count);

            int deletionIndex = 120;
            Assert.Equal(23478, fnt.GetEntrySubtitleAddress(deletionIndex));
            Assert.Equal(64100, fnt.GetEntryMessageIdBranchSequence(deletionIndex));
            Assert.Equal(EntryType.TRIGGER_OBJECT, fnt.GetEntryEntryType(deletionIndex));
            Assert.Equal(172, fnt.GetEntryActiveTime(deletionIndex));
            Assert.Equal(1564, fnt.GetEntryAudioID(deletionIndex));
            Assert.Equal("This cage is protected by\nthose GUN soldiers.\0", fnt.GetEntrySubtitle(deletionIndex));

            TableEntry priorEntryBeforeDelete = fnt.entryTable[deletionIndex - 1];
            TableEntry deletedEntry = fnt.entryTable[deletionIndex];
            TableEntry nextEntryBeforeDelete = fnt.entryTable[deletionIndex + 1];

            fnt.DeleteEntry(deletionIndex);
            Assert.Equal(470, fnt.entryTable.Count);

            // Check expected shift of data

            TableEntry priorEntryAfterDelete = fnt.entryTable[deletionIndex - 1];
            Assert.NotEqual(priorEntryBeforeDelete.subtitleAddress, priorEntryAfterDelete.subtitleAddress);
            Assert.Equal(priorEntryBeforeDelete.subtitle, priorEntryAfterDelete.subtitle);
            Assert.Equal(priorEntryBeforeDelete.messageIdBranchSequence, priorEntryAfterDelete.messageIdBranchSequence);
            Assert.Equal(priorEntryBeforeDelete.audioId, priorEntryAfterDelete.audioId);
            Assert.Equal(priorEntryBeforeDelete.subtitleActiveTime, priorEntryAfterDelete.subtitleActiveTime);
            Assert.Equal(priorEntryBeforeDelete.entryType, priorEntryAfterDelete.entryType);

            TableEntry deletedIndexAfterDelete = fnt.entryTable[deletionIndex];
            Assert.NotEqual(nextEntryBeforeDelete.subtitleAddress, deletedIndexAfterDelete.subtitleAddress);
            Assert.Equal(nextEntryBeforeDelete.subtitle, deletedIndexAfterDelete.subtitle);
            Assert.Equal(nextEntryBeforeDelete.messageIdBranchSequence, deletedIndexAfterDelete.messageIdBranchSequence);
            Assert.Equal(nextEntryBeforeDelete.audioId, deletedIndexAfterDelete.audioId);
            Assert.Equal(nextEntryBeforeDelete.subtitleActiveTime, deletedIndexAfterDelete.subtitleActiveTime);
            Assert.Equal(nextEntryBeforeDelete.entryType, deletedIndexAfterDelete.entryType);

            TableEntry nextEntryAfterDelete = fnt.entryTable[deletionIndex + 1];
            Assert.NotEqual(nextEntryBeforeDelete.subtitleAddress, nextEntryAfterDelete.subtitleAddress);
            Assert.NotEqual(nextEntryBeforeDelete.subtitle, nextEntryAfterDelete.subtitle);
            Assert.NotEqual(nextEntryBeforeDelete.messageIdBranchSequence, nextEntryAfterDelete.messageIdBranchSequence);
            Assert.NotEqual(nextEntryBeforeDelete.audioId, nextEntryAfterDelete.audioId);
            Assert.NotEqual(nextEntryBeforeDelete.subtitleActiveTime, nextEntryAfterDelete.subtitleActiveTime);

            // Check data is deleted
            for (int i = 0; i < 470; i++) {
                Assert.NotEqual(deletedEntry.subtitleAddress, fnt.GetEntrySubtitleAddress(i));
                Assert.NotEqual(deletedEntry.subtitle, fnt.GetEntrySubtitle(i));
                Assert.NotEqual(deletedEntry.messageIdBranchSequence, fnt.GetEntryMessageIdBranchSequence(i));
                Assert.NotEqual(deletedEntry.audioId, fnt.GetEntryAudioID(i));
            }

            // Compare compute vs delete result
            var copiedTable = new List<TableEntry>();
            for (int i = 0; i < fnt.entryTable.Count; i++) {
                var entry = fnt.entryTable[i];
                copiedTable.Add(entry);
            }
            fnt.RecomputeAllSubtitleAddresses();
            Assert.Equal(fnt.entryTable, copiedTable);

            // ensure deep copy
            fnt.entryTable[3] = deletedIndexAfterDelete;
            Assert.NotEqual(fnt.entryTable, copiedTable);
        }
    }
}
