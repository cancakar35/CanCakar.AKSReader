# CanCakar.AKSReader ![Build](https://github.com/cancakar35/CanCakar.AKSReader/actions/workflows/ci.yml/badge.svg) [![NuGet](https://img.shields.io/nuget/v/CanCakar.AKSReader.svg)](https://www.nuget.org/packages/CanCakar.AKSReader)

Cross platform integration library for AKS Elektronik card readers.

## Quick Start

Installation

```bash
dotnet add package CanCakar.AKSReader
```
Basic usage.

```csharp
using CanCakar.AKSReader;
using CanCakar.AKSReader.Enums;

using (var myReader = new Reader(IPAddress.Parse("192.168.1.139"), 1001)) // or use COM,baudrate overload for serial port
{
    await myReader.ConnectAsync();

    string? response = await myReader.SendRawCommandAsync(150, AksCommand.ReadCard);
}
```

## Known Issues
- Reader.IsConnected returns false after cancellation occured in async methods.
  
