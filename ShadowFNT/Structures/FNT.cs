using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShadowFNT.Structures {

    /*
    .fnt file
    Everything is little endian.

    fnt {
        Header : Number of entries | int
        list of TableEntry
        list of UTF-16 Strings (Note: for convenience and eq(), sticking Strings inside TableEntry, but will be written per original file)
    }
    */

    public struct FNT {
        public string fileName;
        public string filterString;
        public List<TableEntry> entryTable;
        private const int ENTRY_SIZE = 20;

        public FNT(string fileName, ref byte[] file) {
            this = ParseFNTFile(fileName, ref file);
            this.fileName = fileName;
        }

        /// <summary>
        /// Parses a .fnt file
        /// </summary>
        /// <param name="fileName">Full name (not SafeName) retrieved via a FilePicker</param>
        /// <param name="file">Bytes of file to parse</param>
        /// <param name="filterString">String to remove when ToString() is called</param>
        /// <returns>FNT</returns>
        public static FNT ParseFNTFile(string fileName, ref byte[] file, string filterString = "") {
            FNT fnt = new FNT();

            fnt.fileName = fileName;
            fnt.filterString = filterString;

            int numberOfEntries = BitConverter.ToInt32(file, 0);
            int positionIndex = 4; // position of byte to read

            fnt.entryTable = new List<TableEntry>();

            // Length of single table entry
            int subtitleTableEntryStructSize = 0x14;

            // read table entries
            for (int i = 0; i < numberOfEntries; i++) {
                TableEntry entry = new TableEntry {
                    subtitleAddress = BitConverter.ToInt32(file, positionIndex),
                    messageIdBranchSequence = BitConverter.ToInt32(file, positionIndex + 4),
                    entryType = (EntryType)BitConverter.ToInt32(file, positionIndex + 8),
                    subtitleActiveTime = BitConverter.ToInt32(file, positionIndex + 12),
                    audioId = BitConverter.ToInt32(file, positionIndex + 16)
                };

                fnt.entryTable.Add(entry);
                positionIndex += subtitleTableEntryStructSize;
            }

            // read UTF-16 strings
            for (int i = 0; i < numberOfEntries; i++) {
                int subtitleLength;
                string subtitle;
                if (i == numberOfEntries - 1) {
                    // if last subtitleTable entry, size is originalFilesize - entry index
                    // however the original .fnt files sometimes have junk strings at the end
                    // end at first "\0"
                    subtitleLength = file.Length - fnt.entryTable[i].subtitleAddress;
                    subtitle = Encoding.Unicode.GetString(file, positionIndex, subtitleLength).Split("\0")[0];
                } else {
                    // otherwise calculate based on next entry in list
                    subtitleLength = fnt.entryTable[i + 1].subtitleAddress - fnt.entryTable[i].subtitleAddress;
                    subtitle = Encoding.Unicode.GetString(file, positionIndex, subtitleLength);
                }
                fnt.UpdateEntrySubtitle(i, subtitle);
                positionIndex += subtitleLength;
            }

            return fnt;
        }

        /// <summary>
        /// Returns a .fnt built from the FNT
        /// </summary>
        public List<byte> BuildFNTFile() {
            List<byte> fntFile = new List<byte>();

            // write header
            BitConverter.GetBytes(entryTable.Count).ToList().ForEach(b => { fntFile.Add(b); });

            // write table entries
            for (int i = 0; i < entryTable.Count; i++) {
                
                BitConverter.GetBytes(entryTable[i].subtitleAddress).ToList().ForEach(b => { fntFile.Add(b); });
                BitConverter.GetBytes(entryTable[i].messageIdBranchSequence).ToList().ForEach(b => { fntFile.Add(b); });
                BitConverter.GetBytes((int)entryTable[i].entryType).ToList().ForEach(b => { fntFile.Add(b); });
                BitConverter.GetBytes(entryTable[i].subtitleActiveTime).ToList().ForEach(b => { fntFile.Add(b); });
                BitConverter.GetBytes(entryTable[i].audioId).ToList().ForEach(b => { fntFile.Add(b); });
            }

            // write UTF-16 entries
            for (int i = 0; i < entryTable.Count; i++)
                Encoding.Unicode.GetBytes(entryTable[i].subtitle).ToList().ForEach(b => { fntFile.Add(b); });

            // add 4 null bytes to satisfy in-game parser's constraint to close file of any size
            fntFile.Add(0x00);
            fntFile.Add(0x00);
            fntFile.Add(0x00);
            fntFile.Add(0x00);

            return fntFile;
        }

        public override string ToString() {
            if (filterString != "")
                return fileName.Split(filterString + '\\')[1];
            return fileName;
        }

        public override bool Equals(object obj) {
            FNT compareFnt;
            try {
                compareFnt = (FNT)obj;
            } catch (InvalidCastException) {
                // on DarkMode/LightMode change eq will be called on MS.Internal.NamedObject, handle gracefully
                return false;
            }
            
            for (int i = 0; i < entryTable.Count; i++) {
                if (entryTable[i].subtitle != compareFnt.entryTable[i].subtitle)
                    return false;
                if (entryTable[i].subtitleAddress != compareFnt.entryTable[i].subtitleAddress)
                    return false;
                if (entryTable[i].messageIdBranchSequence != compareFnt.entryTable[i].messageIdBranchSequence)
                    return false;
                if (entryTable[i].entryType != compareFnt.entryTable[i].entryType)
                    return false;
                if (entryTable[i].subtitleActiveTime != compareFnt.entryTable[i].subtitleActiveTime)
                    return false;
                if (entryTable[i].audioId != compareFnt.entryTable[i].audioId)
                    return false;
            }
            return true;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        /// <summary>
        /// Getter for subtitleAddress of passed in entry index
        /// </summary>
        /// <param name="tableEntryIndex">Index of entry to get from</param>
        /// <returns>int</returns>
        public int GetEntrySubtitleAddress(int tableEntryIndex) {
            return entryTable[tableEntryIndex].subtitleAddress;
        }

        /// <summary>
        /// Getter for messageIdBranchSequence of passed in entry index
        /// </summary>
        /// <param name="tableEntryIndex">Index of entry to get from</param>
        /// <returns>int</returns>
        public int GetEntryMessageIdBranchSequence(int tableEntryIndex) {
            return entryTable[tableEntryIndex].messageIdBranchSequence;
        }

        /// <summary>
        /// Update a tableEntry's messageIdBranchSequence
        /// </summary>
        /// <param name="tableEntryIndex">Index of entry to update</param>
        /// <param name="messageIdBranchSequence">Updated messageIdBranchSequence value</param>
        public void UpdateEntryMessageIdBranchSequence(int tableEntryIndex, int messageIdBranchSequence) {
            TableEntry updatedEntry = entryTable[tableEntryIndex];
            updatedEntry.messageIdBranchSequence = messageIdBranchSequence;
            entryTable[tableEntryIndex] = updatedEntry;
        }

        /// <summary>
        /// Getter for EntryType of passed in entry index
        /// </summary>
        /// <param name="tableEntryIndex">Index of entry to get from</param>
        /// <returns>EntryType</returns>
        public EntryType GetEntryEntryType(int tableEntryIndex) {
            return entryTable[tableEntryIndex].entryType;
        }

        /// <summary>
        /// Update a tableEntry's EntryType
        /// </summary>
        /// <param name="tableEntryIndex">Index of entry to update</param>
        /// <param name="enumIndex">Updated EntryType value</param>
        public void UpdateEntryEntryType(int tableEntryIndex, int enumIndex) {
            TableEntry updatedEntry = entryTable[tableEntryIndex];
            EntryType temp = EntryType.BACKGROUND_VOICE; //stub entry to get enum values
            updatedEntry.entryType = (EntryType)(Enum.GetValues(temp.GetType())).GetValue(enumIndex);
            entryTable[tableEntryIndex] = updatedEntry;
        }

        /// <summary>
        /// Getter for subtitleActiveTime of passed in subtitleEntry index
        /// </summary>
        /// <param name="tableEntryIndex">Index of entry to get subtitleActiveTime</param>
        /// <returns>int</returns>
        public int GetEntryActiveTime(int tableEntryIndex) {
            return entryTable[tableEntryIndex].subtitleActiveTime;
        }

        /// <summary>
        /// Update a tableEntry's activeTime
        /// </summary>
        /// <param name="tableEntryIndex">Index of entry to update</param>
        /// <param name="activeTime">Updated activeTime value</param>
        public void UpdateEntryActiveTime(int tableEntryIndex, int activeTime) {
            TableEntry updatedEntry = entryTable[tableEntryIndex];
            updatedEntry.subtitleActiveTime = activeTime;
            entryTable[tableEntryIndex] = updatedEntry;
        }

        /// <summary>
        /// Getter for AudioID of an entry
        /// </summary>
        /// <param name="tableEntryIndex">Index of entry to get from</param>
        /// <returns>int</returns>
        public int GetEntryAudioID(int tableEntryIndex) {
            return entryTable[tableEntryIndex].audioId;
        }

        /// <summary>
        /// Sets the AudioID of an entry
        /// </summary>
        /// <param name="tableEntryIndex">Index of entry to update</param>
        /// <param name="audioId">AudioID to play (index in the AFS)</param>
        public void UpdateEntryAudioID(int tableEntryIndex, int audioId) {
            TableEntry updatedEntry = entryTable[tableEntryIndex];
            updatedEntry.audioId = audioId;
            entryTable[tableEntryIndex] = updatedEntry;
        }

        /// <summary>
        /// Get the subtitle string of an entry
        /// </summary>
        /// <param name="tableEntryIndex"></param>
        /// <returns>String</returns>
        public string GetEntrySubtitle(int tableEntryIndex) {
            return entryTable[tableEntryIndex].subtitle;
        }

        /// <summary>
        /// Update an entry's subtitle
        /// Performs a safe expand/shrink of all succeeding entries
        /// </summary>
        /// <param name="tableEntryIndex">Index of entry to update</param>
        /// <param name="updatedText">string of new text to display</param>
        public void UpdateEntrySubtitle(int tableEntryIndex, string updatedText) {
            updatedText = updatedText.Replace("\r\n", "\n");
            updatedText = updatedText.Replace("\0", "");
            updatedText += '\0';
            TableEntry entry = entryTable[tableEntryIndex];
            int characterSizeDifference = 0;

            if (entry.subtitle != null) {
                characterSizeDifference = updatedText.Length - entry.subtitle.Length;
            }

            entry.subtitle = updatedText;
            entryTable[tableEntryIndex] = entry;

            // Update TableEntry of all succeeding elements to account for length change
            if (characterSizeDifference != 0) {
                for (int i = tableEntryIndex + 1; i < entryTable.Count; i++) {
                    TableEntry succeedingEntry = entryTable[i];
                    succeedingEntry.subtitleAddress = entryTable[i].subtitleAddress + characterSizeDifference * 2;
                    entryTable[i] = succeedingEntry;
                }
            }
        }

        /// <summary>
        /// Create a new entry and add it based on the MessageIdBranchSequence while shifting all successor positions
        /// </summary>
        /// <param name="newEntryMessageIdBranchSequence">MessageIdBranchSequence of new entry to determine where to insert in the table</param>
        /// <returns>-1 = did not add, 0 = success</returns>
        public int InsertNewEntry(int newEntryMessageIdBranchSequence) {
            int successor = -1;
            for (int i = 0; i < entryTable.Count; i++) {
                if (newEntryMessageIdBranchSequence < entryTable[i].messageIdBranchSequence) {
                    successor = i;
                    break;
                } else if (newEntryMessageIdBranchSequence == entryTable[i].messageIdBranchSequence) {
                    break;
                }
            }
            if (successor == -1)
                return -1;
            TableEntry newEntry = new TableEntry {
                subtitleAddress = entryTable[successor].subtitleAddress,
                messageIdBranchSequence = newEntryMessageIdBranchSequence,
                entryType = EntryType.TRIGGER_OBJECT,
                subtitleActiveTime = 0,
                audioId = -1,
                subtitle = "\0"
            };
            entryTable.Insert(successor, newEntry);
            //correct subtitleAddress for all succeeding entries: add 2
            for (int i = successor+1; i<entryTable.Count; i++) {
                TableEntry succeedingEntry = entryTable[i];
                succeedingEntry.subtitleAddress = entryTable[i].subtitleAddress + 2;
                entryTable[i] = succeedingEntry;
            }

            //grow an entry size for all
            for (int i = 0; i < entryTable.Count; i++) {
                 TableEntry entry = entryTable[i];
                 entry.subtitleAddress += ENTRY_SIZE;
                 entryTable[i] = entry;
             }
            return 0;
        }

        public void DeleteEntry(int index) {
            int chardiff = entryTable[index].subtitle.Length * 2; //successor's address - original entry is difference
            entryTable.RemoveAt(index);
            //correct subtitleAddress for all succeeding entries (start at self since successor occupies old location)
            for (int i = index; i < entryTable.Count; i++) {
                TableEntry succeedingEntry = entryTable[i];
                succeedingEntry.subtitleAddress -= chardiff;
                entryTable[i] = succeedingEntry;
            }

           //shrink an entry size for all
            for (int i = 0; i < entryTable.Count; i++) {
                TableEntry entry = entryTable[i];
                entry.subtitleAddress -= ENTRY_SIZE;
                entryTable[i] = entry;
            }
        }

        /// <summary>
        /// Recomputes subtitleAddress for all entries in the entryTable.
        /// </summary>
        /// 
        public void RecomputeAllSubtitleAddresses()
        {
            // Calculate entry size + header to determine entry 0 without assuming source of truth
            int addressStart = 0;
            addressStart += 4; // add 4 for entryCount (4 bytes)
            addressStart += ENTRY_SIZE * entryTable.Count; // add a slot for every entry
            
            if (addressStart != entryTable[0].subtitleAddress)
            {
                TableEntry entry0 = entryTable[0];
                entry0.subtitleAddress = addressStart;
                entryTable[0] = entry0;
            }
            
            for (int i = 1; i < entryTable.Count; i++)
            {
                TableEntry predecessor = entryTable[i - 1];
                TableEntry entry = entryTable[i];
                entry.subtitleAddress = predecessor.subtitleAddress + (predecessor.subtitle.Length * 2);
                entryTable[i] = entry;
            }
        }
    }
}