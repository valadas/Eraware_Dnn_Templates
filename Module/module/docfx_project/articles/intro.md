# Getting Started

Welcome to you new module template!

## First build

-   **Before you start**

    This module template makes a few assumptions about your environment, make sure you have the following installed:
    - Latest version of Visual Studio, and updates (Free community edition is fine)
    - Latest version of .NET Core SDK (LST)
    - A test site that uses the latest version of DNN (you can target older versions but out-of-the-box this template is set to the latest)
    - The solution and the project are expected to be in the same folder.
      If you move the project to a different folder, you will need to update the paths in the `Build.cs` file.


-   **Package your module**

    Build tasks are placed in the launch profiles menu in Visual Studio,
    click the debug button with the **Package** profile selected.
    This will fire up a console application to run the module build and will
    create a Dnn extension package in the website under the `install\modules` folder.

    Don't worry about opening up that folder, see next step.
    
    > [!NOTE]
    > If for some reason you do not see the `Package` target, go to the Solution Explorer and ensure that the build project is set as the startup project for the solution.

    > [!NOTE]
    > The build script is simply a console application in the <em>build</em> project, it uses <a href="https://nuke.build/" target="_blank">Nuke</a> to help with utilities.
                You can later customize that process simply by editing the <em>Build.cs</em> file.
    ![Package your module](../images/Package.gif)

-   **Install your module**

    Now we need to install the module in Dnn to see it in action. Simply log in as a SuperUser (host) and navigate to Extensions / Available Extensions to install your new module.

    Then create a test page and put the module on it.

    > [!NOTE]
    > The module javascript fires up on page load, so after dropping your module you need to refresh the page to see it work.
    ![Install module](../images/install-module.gif)

Congratulations, you have a working module!

Now let's make it a git repository. [Learn How](./git.md)
