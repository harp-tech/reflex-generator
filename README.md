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


## How to install the firmware and interface generator

1. Install Bonsai in your system. You can find the instructions [here](https://bonsai-rx.org/docs/getting-started/installation/).
2. install DotNet >6 in your system. You can find the instructions [here](https://dotnet.microsoft.com/download/dotnet/6.0).
3. Open a command-line and run `dotnet new --install Harp.Templates` to install `Harp.Templates` package that contains the template. (Or install it from a local package: `dotnet new --install ./local/path/to/Harp.Templates.nupkg`).
4. Confirm the package has been installed by running `dotnet new --list`.
5. To create a new project, run `dotnet new harpdevice -n <project-name>`. We strongly advise you to follow the convention `Harp.Device` for `<project-name>`.
6. Open the project folder in VSCode. (`cd project-name` & `code .`).
7. The project template should create a file with instructions (`README.md`) to generate the Bonsai interface and Device firmware.
8. Install the code generator: ```dotnet tool install -g dotnet-t4```
9. Go into the folder `Generators` and run `dotnet build`.
10. Confirm that a solution has been created inside `Firmware` and `Interface` folders. Importantly, each time you run `build`, it will override any automatically generated code currently present in the folders.
11. Simply replace the relevant `.yml` files necessary to generate the firmware and try to build the solution.
