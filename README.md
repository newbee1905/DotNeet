# DotNeet

## Description

A cli tool to download, search and read manga built with C#

## Build

```bash
dotnet build --configuration Release
```

## Run

```bash
./bin/Release/netcoreapp6.0/DotNeet <commands> <args>
```

## Help

```
Description:

Usage:
  DotNeet [command] [options]

Options:
  --version       Show version information
  -?, -h, --help  Show help and usage information

Commands:
  providers              List all supoprted providers
  search <manga-name>    Search Manga to read.
  download <manga-name>  Download Manga to read.
  read <manga-name>      Read Manga to read.
```
