# ADX-ID3 

ADX-ID3 is an experimental proof of concept for adding ID3 tag support to CRI Middleware's ADX files; a common audio format used in video games over the years.

ADX-ID3 adds support for ID3v2 2.4.0, with ID3 information being stored at the beginning of the ADX file. This allows for metadata to be downloaded before playback of the file is started.

ADX-ID3 files extend, not replace the base format. As such, any ADX-ID3 can be played in a standard ADX player or game.
ADX-ID3 has been written in a single afternoon as a fun experiment.

## Supported Files

ADX-ID3 supports 'Version 3' and 'Version 4' ADX files.  
That is, files where the version field (0x12) in the file has the value `03` or `04`.  

There reportedly exists a version `05`, however I haven't seen an ADX file implement this variant of the format.  

## CLI Usage Guide

### Get Help
```
dotnet Sewer56.Adx.Id3.Tool.dll --help
dotnet Sewer56.Adx.Id3.Tool.dll view --help
dotnet Sewer56.Adx.Id3.Tool.dll copy --help
```

***Sample Output***

```
Sewer56.Adx.Id3.Tool 1.0.0
Created by Sewer56, licensed under MIT License

ERROR(S):
  No verb selected.

  view    Reads a file and prints contents of the id3 tag. Supports ADX and some
          other formats.

  copy    Copies an id3 tag from a non-ADX file to an ADX file.
```

### View ID3 Metadata of an music file (ADX or non-ADX)

```
dotnet Sewer56.Adx.Id3.Tool.dll view --source "Digital Circuit.flac" 
```

***Sample Output***

```
Title: Digital Circuit (original version)
Artist(s): 瀬上純
Album: Shadow the Hedgehog: Original Soundtrax
Year: 2006
Genre(s): Soundtrack
Track: 44
Disc No: 1
```

### Copy ID3 Metadata of a music file (ADX or non-ADX) to an ADX file

(Output file can be omitted or specifically set with `--destination`).

```
dotnet Sewer56.Adx.Id3.Tool.dll copy --source "Digital Circuit.flac" --sourceadx "Digital Circuit.adx"
```

*This would add the ID3 metadata from `Digital Circuit.flac` to `Digital Circuit.adx`

## Library Usage Guide

Some properties of the library:  
- No Heap Allocations (outside of return value).  
- No Dependencies.  
- Minimal API/Code.  
- Supports Non-Seekable Streams (Partial Web Downloads!).  

This `~9KB` .NET library is optimised for speed and size.  

### Read ID3 Tag from an ADX File

```csharp
using var fileStream = new FileStream("sample.adx", FileMode.Open);
var id3 = AdxReader.GetId3Tag(fileStream);
```

Here `id3` is a byte array containing the ID3 tag. This array is null if no tag was found.

### Write ID3 Tag to an ADX File

Takes the stream in `input`, writes new file to the stream in `output`. 

```csharp
using var input  = new FileStream("sample.adx", FileMode.Open)
using var output = new MemoryStream();
AdxWriter.WriteAdx(input, output, id3);
```

## Technical Specification

### Structure of an ADX-ID3 Header

```csharp
struct AdxHeader
{
    /* Standard ADX Header */
    short Magic;        // 8000h
    short HeaderSize;   // Pointer to raw data. 
    byte EncodingType;
    byte FrameSize;
    byte BitDepth;
    byte ChannelCount;
    int SampleRate;
    int SampleCount;
    short HighpassFreq;
    byte Version;
    byte Revision;

    /* Version 4 only stuff. */
    if(Version == 4) 
    {
        int Padding;
        for(i = 0; i < (int)Max(2, ChannelCount); i++)
            int history; // 4 bytes
    }

    /*  
        If no loop info or ID3 information contained, the header ends here.
        Remainder of ADX header below.
    */
    short AlignmentSamples;
    short LoopCount;
    for(i = 0; i < LoopCount; i++)
        Loop loop;

    /* 
        End of original ADX header. 
        Potential ID3 header is contained here. 
    */
    // ID3 id3Header;
};

struct AdxLoop
{
    short LoopNum;
    short LoopType;
    int LoopStartSample;
    int LoopStartByte;
    int LoopEndSample;
    int LoopEndByte;
};
```

### How to Extend Loop-less ADX File(s)

Some ADX files have no loops, which means that the ADX headers end prematurely.  
This happens (as seen above) when the header has insufficient space to support a loop header/section.  

In order to support loop-less files are converted to have 1 loop, which is zero'd out and thus ignored by the native player. 

As such, `AdxHeader->LoopCount` is set to `1` and `Loop` struct is zero'd out. This is done in order to retain support for 3rd party tools working with ADXes that assume a loop count of 1.

### Where to Find the ID3 Header

The ID3 header is located after the standard ADX header and before the raw audio data.  
Its location varies with ADX version.  

```csharp
// pAdx is a pointer to start of an ADX file.
// Return Value: 
//      -1 indicates no id3 tag.
//      >0 indicates possible start of id3 tag.
// Note: This is pseudocode, real file uses Big Endian and you'll probably need to account for that.
int GetId3Offset(AdxHeader* pAdx) 
{
    var version = pAdx->Version;                        // Version = 0x12 [byte]
    byte* versionHeader = &(pAdx->VersionHeaderStart);  // 0x14: Version specific data.
    int versionHeaderOffset = versionHeader - pAdx;

    if (version == 3) 
    {
        // No loop information (and no ID3 tag)
        if (versionHeaderOffset + sizeof(AdxLoop) >= pAdx->HeaderSize)
            return -1;

        var loopCount = pAdx->LoopCountv3;           // loopCount = 0x16 [short]
        var loopBytes = loopCount * sizeof(AdxLoop); // loopCount * 20

        return offsetOf(pAdx->Loopv3) + loopBytes;   // loopv3 = [0x18] Offset to first loop item.
    }

    if (version == 4) 
    {
        // channelCount = 0x07 [byte]
        var historySize  = Math.Max(pAdx->ChannelCount * 4, 8); // Size of extra sample history.
        var postHistoryOffset = 4 + historySize; // End of v4 non-looping header. +4 for unknown padding.

        // No loop information (and no ID3 tag)
        if (versionHeaderOffset + postHistoryOffset + sizeof(AdxLoop) >= pAdx->HeaderSize)
            return -1;

        // Skip to loop count.
        var loopCountOffset = postHistoryOffset + 2;         // Padding + History + AlignmentSamples
        var loopCount       = *(short*)(versionHeader + loopCountOffset);
        var loopBytes       = loopCount * sizeof(AdxLoop);   // loopCount * 20

        return versionHeaderOffset + loopCountOffset + sizeof(short) + loopBytes;
    }

    // Not Supported
    return -1;
}
```

The following code defines how to get the offset of a potential ID3 header.

## Limitations

ID3 Header can have max size of ~32700 bytes; due to a format limitation.  
In practice, this limit is never reached as images in ADX-ID3 use [Attached Pictures](https://id3.org/id3v2.3.0#Attached_picture).



## Future Improvements

Here is a list of items to do in the future.

- Support files with `AINF` and `CINF` headers. [Very Rare]
- Support files with version `05`.

Normally I would initially add support for these items, but I couldn't find any sample ADX files to test with in a timely manner (2 hours of searching).

## Special Thanks

- Header alignment code for ADX adapted from [VGAudio](https://github.com/Thealexbarney/VGAudio) by Alex Barney.

- Music by <a href="/users/sergequadrado-24990007/?tab=audio&amp;utm_source=link-attribution&amp;utm_medium=referral&amp;utm_campaign=audio&amp;utm_content=13407">SergeQuadrado</a> from <a href="https://pixabay.com/music/?utm_source=link-attribution&amp;utm_medium=referral&amp;utm_campaign=music&amp;utm_content=13407">Pixabay</a>