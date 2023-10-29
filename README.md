# ShadowFNT
C# library that can read/write FNT files from Shadow The Hedgehog

## Usage

### Previewing an AFS File
You can preview an AFS file by creating an instance of `AfsFileViewer`.

```csharp
using ShadowFNT.Structures;

var data = File.ReadAllBytes(afsFilePath);
if (AfsFileViewer.TryFromFile(data, out var afsViewer)) 
{
	// Do stuff.
};

string[] foundFnts = Directory.GetFiles(lastOpenDir, "*_" + localeSwitcher.Substring(localeSwitcher.Length - 2) + ".fnt", SearchOption.AllDirectories);
for (int i = 0; i < foundFnts.Length; i++) {
    byte[] readFile = File.ReadAllBytes(foundFnts[i]);
    FNT newFnt = FNT.ParseFNTFile(foundFnts[i], ref readFile, lastOpenDir);
```

### Editing an AFS File
To edit an AFS file, create an instance of `AfsArchive`.
`AfsArchive` reads all of the data from an `AfsFileViewer`, converting it into a format easier to edit for the end user.

```csharp
var data = File.ReadAllBytes(afsFilePath);
if (AfsArchive.TryFromFile(data, out var afsArchive)) 
{
	// Do stuff.
};
```

To convert the file back to bytes, use the `ToBytes` method.

```csharp
afsArchive.ToBytes();
```