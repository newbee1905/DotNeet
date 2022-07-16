# DotNeet

## Description

A cli tool to download, search and read manga built with C#

## Dependencies

Please install the developer packages of webkit2gtk and libgtk on your machine.

**NOTE**

Currently, the program is not compilable on MacOS

### Debian and Ubuntu based

```bash
sudo apt install -y libwebkit2gtk-4.0-dev libgtk-3-dev
```

## Build

```bash
dotnet build --configuration Release
```

## Run

```bash
./bin/Release/net6.0/DotNeet [command] [options]
```

## Help

- `search` → return the URL of that chapter
- `download` → download all images of that chapter
- `read` → open all the images inside a WebView

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

## Credits

### Providers

- [Comickfun's api](https://api.comick.fun/docs/static/index.html)
- [Battwo](battwo.com/)
