# $title$

This repository contains the device metadata, firmware and high-level interface to the $devicename$ device. Below are simple getting started instructions for maintainers to create a new device using the automatic code generators.

## Editing device metadata

### Prerequisites

1. Install [Visual Studio Code](https://code.visualstudio.com/)
2. Install the [YAML extension](https://marketplace.visualstudio.com/items?itemName=redhat.vscode-yaml).

The `device.yml` file in the root of the project contains the $devicename$ device metadata. A complete specification of all device registers, including bit masks, group masks, and payload formats needs to be provided.

## Generating interface and firmware

### Prerequisites

1. Install [`dotnet`](https://dotnet.microsoft.com/)
2. Install `dotnet-t4`
```
dotnet tool install -g dotnet-t4
```

The `Generators` folder contains all text templates and project files required to generate both the firmware headers and the interface for the $devicename$ device. To run the text templating engine just build the project inside this folder.

```
dotnet build Generators
```
