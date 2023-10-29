# ShadowFNT
C# library that can read/write FNT files from Shadow The Hedgehog

## Usage
Reads the data from a FNT file, converting it into a format easier to edit for the end user.


### Reading and Writing FNT Files
```csharp
using ShadowFNT

byte[] readFile = File.ReadAllBytes(fntFilePath);
FNT fnt = FNT.ParseFNTFile(fntFilePath, ref readFile);
```

To convert the file back to bytes, use the `BuildFNTFile` method.

```csharp
byte[] bytesForWriting = fnt.BuildFNTFile();
```

### Methods for Manipulating FNT Content

```csharp
GetEntrySubtitleAddress
GetEntryMessageIdBranchSequence
SetEntryMessageIdBranchSequence
GetEntryEntryType
SetEntryEntryType
GetEntryActiveTime
SetEntryActiveTime
GetEntryAudioID
SetEntryAudioID
GetEntrySubtitle
SetEntrySubtitle
InsertNewEntry
DeleteEntry
RecomputeAllSubtitleAddresses
```