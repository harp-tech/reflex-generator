# reflex-generator
Library to automatically generate firmware, interface and documentation for harp devices



## Pre-requisites


1. Install [Visual Studio Code](https://code.visualstudio.com/)
2. Install [YAML extension](https://marketplace.visualstudio.com/items?itemName=redhat.vscode-yaml)
3. Install [Git](https://git-scm.com/download/)

## Getting started

## Using a local schema
1. Clone this repository
2. In VSCode, open the repository as your project (*e.g.* `Open Folder...`)
3. Target this repository as the schema source (`./device.json`) by adding the following line to the top of the file:

```yaml
# yaml-language-server: $schema=./device.json
```
4. Follow step 4 of the [Using a remote schema](#using-a-remote-schema) section


## Using a remote schema

1. In VSCode, open the repository as your project (*e.g.* `Open Folder...`)
2. Create a new .yml file (e.g. `device.yml`).
3. Target this repository as the schema source (for now you can use `https://raw.githubusercontent.com/harp-tech/reflex-generator/main/schema/device.json`) by adding the following line to the top of the file:

```yaml
# yaml-language-server: $schema=https://raw.githubusercontent.com/harp-tech/reflex-generator/main/schema/device.json
```

You should now be able to access the fields of the schema by pressing `Ctrl+Space` in the file.

4. Start adding the fields of the schema to your file.
   1. Add `device` and fill in the respective fields;
   2. Add `registers`, and start adding all the registers you need by filling in, at least, the obligatory fields.

## Installing the Harp Viewer Extension in VSCode

1. Download the Harp Viewer Extension VSIX file
2. Open VSCode
3. Click F1 and search for `Extensions: Install from VSIX...`
4. Once installed, you can run the extension by opening a valid `.yml` device file, and clicking `Device Preview` at the top right corner of the VSCode window.
