# Eraware_Dnn_Templates
Dnn templates

Available in the Visual Studio Marketplace at
https://marketplace.visualstudio.com/items?itemName=DanielValadas.eraware-dnn-templates

Currently this templates package only has a module template but it will probably grow into multiple different different types of DNN extensions.
In order for these templates to work, you will need Visual Studio 2019 (free edition is fine) and you must ensure you have the "Node.js development" workload installed, if you don't and you get an error opening the module.Web project, please use the visual studio installer to add it.

## Stecil SPA Module Template
A WebAPI/WebComponents module template.
- The frontend uses [stencil.js](https://stenciljs.com/) to build standards compliant pure web components. The code enforces using [TypeScript](https://www.typescriptlang.org/) like Angular does and uses jsx like react does. JSX is not part of React but because it as been made popular together with React, you will see some React references that are only namespaces, this template does not use React. Stencil.js is a built-time only compiler that helps quickly and simply create stardards compliant WebComponents that can be used with any framework or without a framework. It also uses a subset of DNN specific WebComponents [dnn-elements](https://www.npmjs.com/package/@eraware/dnn-elements) that should make their way directly into DNN10 soon. Because TypeScript is enforced, a build will automatically document each component.
- The data layer uses [Entity Framework](https://www.entityframeworktutorial.net/) using a generic repository pattern and [Effort](https://entityframework-effort.net/) for making testing easier. What is great is that using the code-first approach to date, you never need to write an SQL script anymore, but this has currently 2 drawbacks:
    1. You still need an sql script for deleting tables upon uninstallation.
    2. This template currently does not support an object qualifier.
- The controllers use [Swagger](https://swagger.io/) annotations to self-document them and also automatically generate matching typescript clients upon build.
- [DocFX](https://dotnet.github.io/docfx/) is used to keep documentation in sync with the code and also provida a base for general documentation.
- [XUnit](https://xunit.net/) is used to manage Unit Test and Integration Tests and the module starts with 100% Unit Test coverage.
