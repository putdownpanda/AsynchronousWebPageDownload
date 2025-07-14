Asynchronous Web Page Download (.NET 8)

A simple, extensible console application that downloads web pages asynchronously with retry support, logging, and configurable settings.
Features

    Async download of one or multiple web pages

    Retry logic (default: 3 attempts, 10 seconds delay)

    Console logging and status reporting

    Command-line input or batch file of URLs

    Configurable via appsettings.json

    Unit tested with xUnit and Moq

Requirements

    .NET 8 SDK

Configuration

appsettings.json:

{
  "RetryCount": "3",
  "RetryDelaySeconds": "10",
  "DefaultOutputDirectory": "DownloadedPages"
}

Usage
Single URL

dotnet run -- "https://example.com"

File with Multiple URLs

dotnet run -- "urls.txt"

Each line in urls.txt should contain one valid URL.
Optional Output Directory

dotnet run -- "https://example.com" "C:\\MyWebPages"

Running Tests

cd AsynchronousWebPageDownload.Tests
dotnet test

Output

Saved files are named using sanitized URLs (e.g., example.com.html) and saved in the specified or default output directory.