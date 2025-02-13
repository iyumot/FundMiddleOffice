﻿namespace FMO.Models;

using System.Diagnostics.CodeAnalysis;
using System.IO;


public class FileStorageInfo
{

    public FID Id { get; set; }

    public string? Path { get; set; }

    public required string Name { get; set; }


    public DateTime Time { get; set; }

    public string? Hash { get; set; }

    public FileStorageInfo()
    {
    }


    [SetsRequiredMembers]
    public FileStorageInfo(string name)
    {
        Name = name;
        Id = IdGenerator.GetNextId(nameof(FileStorageInfo));
    }

    [SetsRequiredMembers]
    public FileStorageInfo(string file, string hash, DateTime last)
    {
        Name = System.IO.Path.GetFileName(file);
        Hash = hash;
        Path = System.IO.Path.GetRelativePath(Directory.GetCurrentDirectory(), file);
        Time = last;
        Id = IdGenerator.GetNextId(nameof(FileStorageInfo));
    }
}



public class FileVersion
{
    public DateTime Time { get; set; }

    public required string Hash { get; set; }

    public required string Path { get; set; }
}

public class VersionedFileInfo
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public List<FileVersion> Files { get; set; } = new();

}