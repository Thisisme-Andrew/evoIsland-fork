# evoIsland VP

## Running the project

There are three ways to run the project:
1. In a virtual AR environment on a Desktop device (Windows 11, macOS)
2. In a virtual HMD environment on a Desktop device (Windows 11, macOS)
3. On an iOS device with AR support
4. On an Apple Vision Pro

### Running on Desktop with AR simulation

1. In Unity, click File > Build Settings
2. Click "Windows, Mac, Linux", then click "Switch Platform" on the bottom-right
3. Ensure that only "ARScene" is added to the scenes to build
4. Exit the dialog
5. Open the ARScene file
6. Click on the Run button

### Running on Desktop with HMD simultion

1. In Unity, click File > Build Settings
2. Click "Windows, Mac, Linux", then click "Switch Platform" on the bottom-right
3. Ensure that only "VRScene" is added to the scenes to build
4. Exit the dialog
5. Open the VPScene file
6. Click on the Run button

### Running on iOS

This method only works on macOS.

1. In Unity, click File > Build Settings
2. Click "iOS", then click "Switch Platform" on the bottom-right
4. Ensure that only "ARScene" is added to the scenes to build
5. Click on "Build And Run"
6. If a dialog pops up to choose a directory, create an `ios/` directory under the project directory, and select that folder
7. Allow Unity to compile assets for Xcode (may take 5 minutes)
8. Once Xcode opens, on the left sidebar, click on the Project, then under Signing & Capabilities, change the team to a Personal Team
    - You must have an Apple Developer account to perform this
9. Connect your iOS device to your laptop
10. Run the Xcode project

Any updates to the Unity editor requires re-running the steps above.
