# ShadowFNT
C# .NET Standard 2.0 library that can read/write FNT files from Shadow The Hedgehog.

## Usage

### Reading and Writing FNT Files

Reads the data from a FNT file, converting it into a format easier to edit for the end user.

```csharp
using ShadowFNT

byte[] readFile = File.ReadAllBytes(fntFilePath);
FNT fnt = FNT.ParseFNTFile(fntFilePath, ref readFile);
```

To convert the file back to bytes, use the `ToBytes` method.

```csharp
byte[] bytesForWriting = fnt.ToBytes();
```

### Methods for Manipulating FNT Content

Most methods will take in a `tableEntryIndex` for getting/setting.

Below are the method names without their signatures.

```
GetEntryTable - Backwards compatibility accessor, not recommended to use this method, instead use the below methods.

GetEntryTableCount - Get the total number of entries in a FNT
GetIndexOfTableEntry - Get the index of a TableEntry

RecomputeAllSubtitleAddresses - Method to automatically compute the subtitle addresses and update all entries. Recommended to call this before exporting back to bytes using ToBytes();

-- These methods are for operating on specific entries --
GetEntrySubtitleAddress
GetEntryMessageIdBranchSequence
SetEntryMessageIdBranchSequence
GetEntryEntryType
SetEntryEntryType
GetEntrySubtitleActiveTime
SetEntrySubtitleActiveTime
GetEntryAudioId
SetEntryAudioId
GetEntrySubtitle
SetEntrySubtitle

-- These methods are for adding/deleting entries from a FNT
InsertEntry
DeleteEntry
```

## Real Usage in Projects

This library is used primarily in [ShadowTHTextEditor](https://github.com/ShadowTheHedgehogHacking/ShadowTHTextEditor). This project uses the library to its full potential. There are examples of reading/writing/deleting/removing/randomization of FNT data.

[HeroesPowerPlant](https://github.com/igorseabra4/HeroesPowerPlant) uses this library for displaying subtitle associations in Shadow the Hedgehog SET data, and for retrieving AudioID for previewing AFS references.