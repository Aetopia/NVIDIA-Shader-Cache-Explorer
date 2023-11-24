# NVIDIA Shader Cache Explorer
A tool to manage shader cache generated by a NVIDIA GPU driver.

## Usage
1. Download the latest release from [GitHub Releases](https://github.com/Aetopia/NVIDIA-Shader-Cache-Explorer/releases/latest).
2. The UI should look like this:
    ![image](https://github.com/Aetopia/NVIDIA-Shader-Cache-Explorer/assets/41850963/bfb02b4c-85ba-4b04-b979-62694d6c5844)
    - Use the <kbd>⟳ Refresh</kbd> to refresh the queried shader cache list.
    - Use the <kbd>🗑️ Delete</kbd> to delete the shader cache of selected applications.
    - Use the checkboxes to select which shader caches should be deleted.

## Build
1. Download and install the .NET SDK and .NET Framework 4.8.1 Developer Pack from:<br>https://dotnet.microsoft.com/en-us/download/visual-studio-sdks
2. Run the following command:

    ```cmd
    dotnet build Halo-Infinite-Settings-Editor-NET\Halo-Infinite-Settings-Editor-NET.csproj --configuration Release
    ```
