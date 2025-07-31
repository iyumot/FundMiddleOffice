// See https://aka.ms/new-console-template for more information



using System.IO.Compression;

//#if DEBUG
//var di = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\..\Templates"));
//#else
//var di = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "..\\Templates"));
//#endif

var di = new DirectoryInfo(Directory.GetCurrentDirectory());
Console.WriteLine($"Start {di.FullName}");

while (di.Parent?.Name != "FMO")
{
    di = di.Parent ?? throw new DirectoryNotFoundException("Templates directory not found.");
}
di = new DirectoryInfo(Path.Combine(di.FullName, @"..\Templates"));


var folders = di.GetDirectories();
foreach (var fd in folders)
{
    Console.WriteLine($"Work On {fd.FullName}");

    var tf = fd.GetFiles("tpl.dll", SearchOption.AllDirectories).FirstOrDefault();
    if (tf is null)
    { 
        Console.WriteLine($"     未找到tpl.dll");
        continue;
    }
    using var fs = new FileStream(Path.Combine(di.FullName.Replace("Templates", "TPL"), $"{fd.Name}.zip"), FileMode.Create);
    using ZipArchive zipArchive = new ZipArchive(fs, ZipArchiveMode.Create); 
    zipArchive.CreateEntryFromFile(tf.FullName, tf.Name, CompressionLevel.Optimal);

    foreach (var e in tf.Directory!.GetFiles("*.xlsx"))
    { 
        zipArchive.CreateEntryFromFile(e.FullName, e.Name, CompressionLevel.Optimal); 
    }


    Console.WriteLine($"     Success");
}




