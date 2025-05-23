# reflex-generator

Provides repository templates supporting the automatic generation of firmware and [Bonsai](https://bonsai-rx.org/) interface code for new [Harp](https://harp-tech.org/) devices. Below are simple getting started instructions for maintainers to create a new device using the automatic code generators.

## Creating a new device project

### Prerequisites

1. Install [`dotnet`](https://dotnet.microsoft.com/)
1. Install the `Harp.Templates` package.
```
dotnet new install Harp.Templates
```

Use the Harp device repository template to create a new project.

```
dotnet new harpdevice -n <project-name>
```

## Editing device metadata

### Prerequisites

1. Install [Visual Studio Code](https://code.visualstudio.com/)
2. Install the [YAML extension](https://marketplace.visualstudio.com/items?itemName=redhat.vscode-yaml).

The `device.yml` file in the root of the project contains the device metadata. A complete specification of all device registers, including bit masks, group masks, and payload formats needs to be provided.

## Generating interface and firmware

### Prerequisites

1. Install `dotnet-t4`
```
dotnet tool install -g dotnet-t4
```

The `Generators` folder contains all text templates and project files required to generate both the firmware headers and the interface for the device. To run the text templating engine just build the project inside this folder.

```
dotnet build Generators
```
