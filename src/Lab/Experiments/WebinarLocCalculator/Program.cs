using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace WebinarLocCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Deleting F:\\SILK.NET...");
            if (Directory.Exists("F:\\SILK.NET"))
            {
                Directory.Delete("F:\\SILK.NET", true);
            }

            Console.WriteLine("Creating F:\\SILK.NET...");
            Directory.CreateDirectory("F:\\SILK.NET");
            Console.WriteLine("Creating F:\\SILK.NET\\src...");
            Directory.CreateDirectory("F:\\SILK.NET\\src");
            Console.WriteLine("Creating F:\\SILK.NET\\src\\Lab...");
            Directory.CreateDirectory("F:\\SILK.NET\\src\\Lab");
            var randomPath = Path.Combine("F:\\SILK.NET_CLONE", Path.GetRandomFileName());
            Console.WriteLine($"Using \"{randomPath}\" as a path for a fresh Silk.NET clone...");
            Process.Start("git", $"clone https://github.com/dotnet/Silk.NET -b v2.4.0 \"{randomPath}\"")!.WaitForExit();
            Console.WriteLine("Collecting...");
            var i = Directory.GetFiles(randomPath, "*.cs", SearchOption.AllDirectories)
                .Sum(x => File.ReadAllLines(x).Length);
            var j = Directory.GetFiles(randomPath, "*.gen.cs", SearchOption.AllDirectories)
                .Sum(x => File.ReadAllLines(x).Length);
            Console.WriteLine($"Got {i} lines of C# code present in repo (excluding submodules)");
            Console.WriteLine("Initializing submodules...");
            Process.Start
            (
                new ProcessStartInfo("git", "submodule update --init --recursive") {WorkingDirectory = randomPath}
            )!.WaitForExit();
            Console.WriteLine("Enabling SilkTouch dumps...");
            var path = Path.Combine(randomPath, "src/Core/Silk.NET.SilkTouch/NativeApiGenerator.cs");
            File.WriteAllLines
            (
                path, File.ReadAllLines(path)
                    .Select
                    (
                        x => x.Trim() == "// File.WriteAllText(@\"C:\\SILK.NET\\src\\Lab\\\" + name, s);"
                            ? "File.WriteAllText(@\"F:\\SILK.NET\\src\\Lab\\\" + name, s);"
                            : x
                    )
            );
            Console.WriteLine("Building Silk.NET...");
            Process.Start
            (
                new ProcessStartInfo("dotnet", "run -p build/nuke/Silk.NET.NUKE.csproj -- Compile --force-dotnet")
                    {WorkingDirectory = randomPath}
            )!.WaitForExit();
            Console.WriteLine("Collecting...");
            var k = Directory.GetFiles("F:\\SILK.NET\\src\\Lab", "*.gen", SearchOption.AllDirectories)
                .Sum(x => File.ReadAllLines(x).Length);
            Console.WriteLine();
            Console.WriteLine("Job Summary");
            Console.WriteLine("===========");
            Console.WriteLine();
            Console.WriteLine($"Lines of code in repo\t\t{i}");
            Console.WriteLine();
            Console.WriteLine($"Manually-written code\t\t{i - j}");
            Console.WriteLine($"BuildTools generated code\t\t{j}");
            Console.WriteLine($"SilkTouch generated code\t\t{k}");
            Console.WriteLine();
            Console.WriteLine($"Total lines of code\t\t{i + k}");
        }
    }
}
