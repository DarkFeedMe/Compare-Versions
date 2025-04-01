# Compare Versions

`Compare_Versions` is a C# application designed to create and compare file reports within a specified directory. It generates a report of files, including their sizes and SHA-256 hashes, and compares them against an existing report to identify changes.

## Features

- **Report Generation**: Creates a report of all files in a directory, excluding the executable itself.
- **File Comparison**: Compares the current state of files against a previously generated report to detect size and hash mismatches, new files, and missing files.
- **Detailed Output**: Provides a comprehensive summary of matched files, mismatches, and discrepancies.

## Usage

1. **Run the Application**: Execute the program. It will automatically check for an existing report file named `CompareVersions.txt` in the application directory.
2. **Generate Report**: If no report is found, it generates a new report file containing the relative path, size, and hash of each file.
3. **Compare Report**: If a report exists, it compares the current files against the report and outputs a detailed comparison.

## How It Works

- The application calculates the SHA-256 hash for each file and records it along with the file size.
- It excludes the executable file from the report to prevent self-inclusion.
- The comparison process identifies:
  - **Matched Files**: Files that have the same size and hash as recorded in the report.
  - **Size Mismatches**: Files with differing sizes compared to the report.
  - **Hash Mismatches**: Files with the same size but different hashes.
  - **New Files**: Files present in the directory but not in the report.
  - **Missing Files**: Files listed in the report but not found in the directory.

## Error Handling

- The application includes error handling to manage exceptions during file operations and hash computations, providing informative error messages.

## Requirements

- .NET Framework 4.8.1
