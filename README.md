# evoIsland VP

## Running the project

There are three ways to run the project:
1. In a virtual AR environment on a Desktop device (Windows 11, macOS)
2. On an iOS device with AR support
3. On an Apple Vision Pro

### Running on Desktop

1. In Unity, click File > Build Settings
2. Click "Windows, Mac, Linux", then click "Switch Platform" on the bottom-right
3. Exit the dialog, then click on the Run button

### Running on iOS

This method only works on macOS.

1. In Unity, click File > Build Settings
2. Click "iOS", then click "Switch Platform" on the bottom-right
3. Click on "Build And Run"
4. If a dialog pops up to choose a directory, create an `ios/` directory under the project directory, and select that folder
5. Allow Unity to compile assets for Xcode (may take 5 minutes)
6. Once Xcode opens, on the left sidebar, click on the Project, then under Signing & Capabilities, change the team to a Personal Team
    - You must have an Apple Developer account to perform this
7. Connect your iOS device to your laptop
8. Run the Xcode project

Any updates to the Unity editor requires re-running the steps above.
