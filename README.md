# AoTBinTool [![MIT License](https://img.shields.io/badge/license-MIT-blue.svg?style=flat)](https://choosealicense.com/licenses/mit/) ![Build and release](https://github.com/kaplas80/AoTBinTool/workflows/Build%20and%20release/badge.svg)

This app can extract and create Attack on Titan BIN archives.

## Tested on

- Attack on Titan base game files (PC, VITA, PS3)
- Attack on Titan DLC files (PC, VITA)
- Attack on Titan II base game files (PC)

## Usage

### Extract files from BIN

```
./AoTBinTool extract --input path/to/archive.bin --output output/directory [--file-list path/to/file-list.txt]
```

`--file-list` parameter is optional.

### Create BIN archive

```
./AoTBinTool build --input path/to/directory --output output/file.bin [--big-endian] [--dlc]
```

`--big-endian` parameter is optional. Use it if BIN file will be used on PS3.
`--dlc` parameter is optional. Use it to save BIN in DLC format.

### Update BIN archive

```
./AoTBinTool update --input-bin path/to/original.bin --input-dir path/to/directory --output output/modified.bin [--file-list path/to/file-list.txt]
```

`--file-list` parameter is needed if new files in directory has names.
