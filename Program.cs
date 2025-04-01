using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Compare_Versions
{
    internal class Program
    {
        static void Main()
        {
            try
            {
                string appFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string reportFilePath = Path.Combine(appFolder, "CompareVersions.txt");
                string appFileName = Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);

                if (!File.Exists(reportFilePath))
                {
                    CreateReport(appFolder, reportFilePath, appFileName);
                    Console.WriteLine($"Report file '{Path.GetFileName(reportFilePath)}' created successfully.");
                }
                else
                {
                    CompareReportWithFolder(appFolder, reportFilePath, appFileName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            Console.WriteLine("Process completed. Press any key to exit.");
            Console.ReadKey();
        }
        static void CreateReport(string folderPath, string reportFilePath, string excludeFileName)
        {
            try
            {
                var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
                using (var writer = new StreamWriter(reportFilePath))
                {
                    foreach (var filePath in files)
                    {
                        string relativePath = GetRelativePath(folderPath, filePath).ToLower();
                        string fileName = Path.GetFileName(filePath);

                        if (!fileName.Equals(excludeFileName, StringComparison.OrdinalIgnoreCase))
                        {
                            long fileSize = new FileInfo(filePath).Length;
                            string fileHash = ComputeFileHash(filePath);
                            writer.WriteLine($"{relativePath}|{fileSize}|{fileHash}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating report: {ex.Message}");
                throw;
            }
        }

        static void CompareReportWithFolder(string folderPath, string reportFilePath, string excludeFileName)
        {
            try
            {
                var reportEntries = new Dictionary<string, (long Size, string Hash)>();
                using (var reader = new StreamReader(reportFilePath))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var parts = line.Split('|');
                        if (parts.Length == 3)
                            reportEntries[parts[0]] = (long.Parse(parts[1]), parts[2]);
                    }
                }

                var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
                int matchedCount = 0, sizeMismatchCount = 0, hashMismatchCount = 0, foundInFolderNotReportCount = 0, foundInReportNotFolderCount = 0;

                Console.WriteLine("=== Full File Comparison Report ===\n");

                foreach (var filePath in files)
                {
                    string relativePath = GetRelativePath(folderPath, filePath);
                    string lowerRelativePath = relativePath.ToLower();
                    string fileName = Path.GetFileName(filePath);

                    if (fileName.Equals("CompareVersions.txt", StringComparison.OrdinalIgnoreCase)) continue;

                    if (!fileName.Equals(excludeFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        long fileSize = new FileInfo(filePath).Length;

                        if (reportEntries.ContainsKey(lowerRelativePath))
                        {
                            var (Size, Hash) = reportEntries[lowerRelativePath];
                            if (Size != fileSize)
                            {
                                Console.WriteLine($"File: {relativePath}\n  Status: Size Mismatch\n  Report Size: {Size} bytes\n  Current Size: {fileSize} bytes");
                                sizeMismatchCount++;
                            }
                            else
                            {
                                string fileHash = ComputeFileHash(filePath);
                                if (Hash != fileHash)
                                {
                                    Console.WriteLine($"File: {relativePath}\n  Status: Hash Mismatch\n  Size: {fileSize} bytes\n  Report Hash: {Hash}\n  Current Hash: {fileHash}");
                                    hashMismatchCount++;
                                }
                                else
                                {
                                    matchedCount++;
                                }
                            }
                            reportEntries.Remove(lowerRelativePath);
                        }
                        else
                        {
                            Console.WriteLine($"File: {relativePath}\n  Status: New File (Found in folder but not in report)\n  Size: {fileSize} bytes");
                            foundInFolderNotReportCount++;
                        }
                    }
                }

                foreach (var missingFile in reportEntries.Keys)
                {
                    Console.WriteLine($"File: {missingFile}\n  Status: Missing (Found in report but not in folder)\n  Report Size: {reportEntries[missingFile].Size} bytes\n  Report Hash: {reportEntries[missingFile].Hash}\n");
                    foundInReportNotFolderCount++;
                }

                Console.WriteLine("=== Summary ===");
                Console.WriteLine($"Total Matched Files: {matchedCount}");
                Console.WriteLine($"Total Size Mismatches: {sizeMismatchCount}");
                Console.WriteLine($"Total Hash Mismatches: {hashMismatchCount}");
                Console.WriteLine($"Total New Files (Found in folder but not in report): {foundInFolderNotReportCount}");
                Console.WriteLine($"Total Missing Files (Found in report but not in folder): {foundInReportNotFolderCount}");
                Console.WriteLine("\n=== End of Report ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error comparing report: {ex.Message}");
                throw;
            }
        }

        static string ComputeFileHash(string filePath)
        {
            try
            {
                using (var sha256 = SHA256.Create())
                using (var stream = File.OpenRead(filePath))
                {
                    var hashBytes = sha256.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error computing file hash: {ex.Message}");
                throw;
            }
        }

        static string GetRelativePath(string baseFolder, string filePath)
        {
            var baseUri = new Uri(baseFolder + Path.DirectorySeparatorChar);
            var fileUri = new Uri(filePath);
            return Uri.UnescapeDataString(baseUri.MakeRelativeUri(fileUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
