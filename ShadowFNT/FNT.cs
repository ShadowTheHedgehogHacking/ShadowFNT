using ShadowFNT.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShadowFNT {

    /*
    .FNT file
    Everything is little endian.

    FNT {
        Header : Number of entries | int
        list of TableEntry
        list of UTF-16 Strings
    }
    */
    public struct FNT {
        public string fileName;
        public string filterString;
        private List<TableEntry> entryTable;
        private const int ENTRY_SIZE = 20;

        public FNT(string fileName, ref byte[] file) {
            this = ParseFNTFile(fileName, ref file);
            this.fileName = fileName;
        }

        /// <summary>
        /// Parses a .FNT file
        /// </summary>
        /// <param name="fileName">Full name (not SafeName) of the FNT file.</param>
        /// <param name="file">Bytes of FNT to parse.</param>
        /// <param name="filterString">String to remove when ToString() is called.</param>
        /// <returns>FNT instance from the provided data.</returns>
        public static FNT ParseFNTFile(string fileName, ref byte[] file, string filterString = "") {
            FNT fnt = new FNT {
                fileName = fileName,
                filterString = filterString
            };

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
                    subtitle = Encoding.Unicode.GetString(file, positionIndex, subtitleLength).Split('\0')[0];
                } else {
                    // otherwise calculate based on next entry in list
                    subtitleLength = fnt.entryTable[i + 1].subtitleAddress - fnt.entryTable[i].subtitleAddress;
                    subtitle = Encoding.Unicode.GetString(file, positionIndex, subtitleLength);
                }
                fnt.SetEntrySubtitle(i, subtitle);
                positionIndex += subtitleLength;
            }

            return fnt;
        }

        /// <summary>
        /// Returns a byte[] of the FNT instance.
        /// It is recommended to first call <see cref="RecomputeAllSubtitleAddresses"/> if lots of changes were made to the FNT instance.
        /// </summary>
        /// <returns>A byte array containing the serialized FNT.</returns>
        public byte[] ToBytes() {
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

            return fntFile.ToArray();
        }

        /// <summary>
        /// Returns the fileName of the FNT; Removing filterString if it exists.
        /// </summary>
        /// <returns>
        /// If a filterString was specified in the creation of the FNT, this method trims the file name by removing
        /// the filterString and its leading directory separator from the fileName before returning it.
        /// If no filterString was specified in creation, the absolute fileName is returned.
        /// </returns>
        public override string ToString() {
            if (filterString != "") {
                int index = fileName.IndexOf(filterString + "\\");
                return index >= 0 ? fileName.Substring(index + (filterString + "\\").Length) : fileName;
            }
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
            unchecked {
                int hash = 17; // Choose a prime number as the initial hash code
                for (int i = 0; i < entryTable.Count; i++) {
                    hash = hash * 23 + entryTable[i].subtitle.GetHashCode();
                    hash = hash * 23 + entryTable[i].subtitleAddress.GetHashCode();
                    hash = hash * 23 + entryTable[i].messageIdBranchSequence.GetHashCode();
                    hash = hash * 23 + entryTable[i].entryType.GetHashCode();
                    hash = hash * 23 + entryTable[i].subtitleActiveTime.GetHashCode();
                    hash = hash * 23 + entryTable[i].audioId.GetHashCode();
                }
                return hash;
            }
        }

        /// <summary>
        /// Retrieves total number of entries in the FNT.
        /// </summary>
        /// <returns>Number of entries in the entryTable.</returns>
        public int GetEntryTableCount() { return entryTable.Count; }

        /// <summary>
        /// Get the entryTable from the FNT.
        /// Treat this as a Read-Only getter.
        /// You should avoid using this method unless you absolutely need it.
        /// Instead use the various getter/setters for modifying the FNT.
        /// </summary>
        /// <returns>The full entryTable list from the FNT.</returns>
        public List<TableEntry> GetEntryTable() {
            return entryTable;
        }

        /// <summary>
        /// Retrieves the full TableEntry struct at the specified index.
        /// </summary>
        /// <param name="tableEntryIndex">The index of the entry.</param>
        /// <returns>TableEntry at the specified index.</returns>
        public TableEntry GetTableEntry(int tableEntryIndex) {
            return entryTable[tableEntryIndex];
        }

        /// <summary>
        /// Retrieves the index of the specified TableEntry.
        /// </summary>
        /// <param name="tableEntry">TableEntry to find the index of.</param>
        /// <returns>Index of the specified TableEntry.</returns>
        public int GetIndexOfTableEntry(TableEntry tableEntry) {
            return entryTable.IndexOf(tableEntry);
        }

        /// <summary>
        /// Retrieves subtitleAddress at the specified index.
        /// </summary>
        /// <param name="tableEntryIndex">The index of the entry.</param>
        /// <returns>subtitleAddress at the specified index.</returns>
        public int GetEntrySubtitleAddress(int tableEntryIndex) {
            return entryTable[tableEntryIndex].subtitleAddress;
        }

        /// <summary>
        /// Retrieves messageIdBranchSequence at the specified index.
        /// </summary>
        /// <param name="tableEntryIndex">The index of the entry.</param>
        /// <returns>messageIdBranchSequence at the specified index<./returns>
        public int GetEntryMessageIdBranchSequence(int tableEntryIndex) {
            return entryTable[tableEntryIndex].messageIdBranchSequence;
        }

        /// <summary>
        /// Updates messageIdBranchSequence at the specified index.
        /// </summary>
        /// <param name="tableEntryIndex">The index of the entry.</param>
        /// <param name="messageIdBranchSequence">New messageIdBranchSequence.</param>
        public void SetEntryMessageIdBranchSequence(int tableEntryIndex, int messageIdBranchSequence) {
            TableEntry updatedEntry = entryTable[tableEntryIndex];
            updatedEntry.messageIdBranchSequence = messageIdBranchSequence;
            entryTable[tableEntryIndex] = updatedEntry;
        }

        /// <summary>
        /// Retrieves EntryType at the specified index.
        /// </summary>
        /// <param name="tableEntryIndex">The index of the entry.</param>
        /// <returns>EntryType at the specified index.</returns>
        public EntryType GetEntryEntryType(int tableEntryIndex) {
            return entryTable[tableEntryIndex].entryType;
        }

        /// <summary>
        /// Updates entryType at the specified index.
        /// </summary>
        /// <param name="tableEntryIndex">The index of the entry.</param>
        /// <param name="entryTypeValue">New entryType value as an int.</param>
        public void SetEntryEntryType(int tableEntryIndex, int entryTypeValue) {
            TableEntry updatedEntry = entryTable[tableEntryIndex];
            EntryType temp = EntryType.BACKGROUND_VOICE; //stub entry to get enum values
            updatedEntry.entryType = (EntryType)Enum.GetValues(temp.GetType()).GetValue(entryTypeValue);
            entryTable[tableEntryIndex] = updatedEntry;
        }

        /// <summary>
        /// Updates entryType at the specified index.
        /// </summary>
        /// <param name="tableEntryIndex">The index of the entry.</param>
        /// <param name="enum">New entryType.</param>
        public void SetEntryEntryType(int tableEntryIndex, EntryType entryType) {
            TableEntry updatedEntry = entryTable[tableEntryIndex];
            updatedEntry.entryType = entryType;
            entryTable[tableEntryIndex] = updatedEntry;
        }

        /// <summary>
        /// Retrieves subtitleActiveTime at the specified index.
        /// </summary>
        /// <param name="tableEntryIndex">The index of the entry.</param>
        /// <returns>subtitleActiveTime at the specified index.</returns>
        public int GetEntrySubtitleActiveTime(int tableEntryIndex) {
            return entryTable[tableEntryIndex].subtitleActiveTime;
        }

        /// <summary>
        /// Updates subtitleActiveTime at the specified index.
        /// </summary>
        /// <param name="tableEntryIndex">The index of the entry.</param>
        /// <param name="subtitleActiveTime">New subtitleActiveTime.</param>
        public void SetEntrySubtitleActiveTime(int tableEntryIndex, int subtitleActiveTime) {
            TableEntry updatedEntry = entryTable[tableEntryIndex];
            updatedEntry.subtitleActiveTime = subtitleActiveTime;
            entryTable[tableEntryIndex] = updatedEntry;
        }

        /// <summary>
        /// Retrieves audioId at the specified index.
        /// </summary>
        /// <param name="tableEntryIndex">The index of the entry.</param>
        /// <returns>audioId at the specified index.</returns>
        public int GetEntryAudioId(int tableEntryIndex) {
            return entryTable[tableEntryIndex].audioId;
        }

        /// <summary>
        /// Updates audioId at the specified index.
        /// </summary>
        /// <param name="tableEntryIndex">The index of the entry.</param>
        /// <param name="audioId">New audioId.</param>
        public void SetEntryAudioId(int tableEntryIndex, int audioId) {
            TableEntry updatedEntry = entryTable[tableEntryIndex];
            updatedEntry.audioId = audioId;
            entryTable[tableEntryIndex] = updatedEntry;
        }

        /// <summary>
        /// Retrieves subtitle at the specified index.
        /// </summary>
        /// <param name="tableEntryIndex">The index of the entry.</param>
        /// <returns>subtitle at the specified index.</returns>
        public string GetEntrySubtitle(int tableEntryIndex) {
            return entryTable[tableEntryIndex].subtitle;
        }

        /// <summary>
        /// Updates subtitle at the specified index.
        /// Shifts the subtitleAddress of all succeeding entries.
        /// </summary>
        /// <param name="tableEntryIndex">The index of the entry.</param>
        /// <param name="subtitle">New subtitle.</param>
        public void SetEntrySubtitle(int tableEntryIndex, string subtitle) {
            subtitle = subtitle.Replace("\r\n", "\n");
            subtitle = subtitle.Replace("\0", "");
            subtitle += '\0';
            TableEntry entry = entryTable[tableEntryIndex];
            int characterSizeDifference = 0;

            if (entry.subtitle != null) {
                characterSizeDifference = subtitle.Length - entry.subtitle.Length;
            }

            entry.subtitle = subtitle;
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
        /// Creates a new entry in the entryTable and shifts the subtitleAddress of all succeeding entries.
        /// </summary>
        /// <param name="newEntryMessageIdBranchSequence">MessageIdBranchSequence of the new entry. Used to determine where the entry will be inserted in the table.</param>
        /// <returns>true if successful; false if an error occurs.</returns>
        public bool InsertEntry(int newEntryMessageIdBranchSequence) {
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
                return false;
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
            for (int i = successor + 1; i < entryTable.Count; i++) {
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
            return true;
        }

        /// <summary>
        /// Deletes an entry and shifts the subtitleAddress of all succeeding entries.
        /// </summary>
        /// <param name="tableEntryIndex">The index of the entry.</param>
        public void DeleteEntry(int tableEntryIndex) {
            int chardiff = entryTable[tableEntryIndex].subtitle.Length * 2; //successor's address - original entry is difference
            entryTable.RemoveAt(tableEntryIndex);
            //correct subtitleAddress for all succeeding entries (start at self since successor occupies old location)
            for (int i = tableEntryIndex; i < entryTable.Count; i++) {
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
        /// Recommended to be called before exporting using <see cref="ToBytes"/>.
        /// </summary>
        public void RecomputeAllSubtitleAddresses() {
            // Calculate entry size + header to determine entry 0 without assuming source of truth
            int addressStart = 0;
            addressStart += 4; // add 4 for entryCount (4 bytes)
            addressStart += ENTRY_SIZE * entryTable.Count; // add a slot for every entry

            if (addressStart != entryTable[0].subtitleAddress) {
                TableEntry entry0 = entryTable[0];
                entry0.subtitleAddress = addressStart;
                entryTable[0] = entry0;
            }

            for (int i = 1; i < entryTable.Count; i++) {
                TableEntry predecessor = entryTable[i - 1];
                TableEntry entry = entryTable[i];
                entry.subtitleAddress = predecessor.subtitleAddress + predecessor.subtitle.Length * 2;
                entryTable[i] = entry;
            }
        }
    }
}