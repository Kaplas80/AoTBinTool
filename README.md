# AoTBinTool [![MIT License](https://img.shields.io/badge/license-MIT-blue.svg?style=flat)](https://choosealicense.com/licenses/mit/) ![Build and release](https://github.com/kaplas80/AoTBinTool/workflows/Build%20and%20release/badge.svg)

This app can extract and create Attack on Titan BIN archives.

It is compatible with:

- Attack on Titan (PC)
- Attack on Titan (PS3)
- Attack on Titan II (PC)

## Usage

### Extract files from BIN

```
./AoTBinTool extract --input path/to/archive.bin --output output/directory [--file-list path/to/file-list.txt]
```

`--file-list` parameter is optional.

### Create BIN archive

```
./AoTBinTool build --input path/to/directory --output output/file.bin [--big-endian]
```

`--big-endian` parameter is optional. Use it if BIN file is for PS3.
