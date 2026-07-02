# evoIsland VP

[![Unity](https://img.shields.io/badge/Unity-2022.3.62f2-black?logo=unity)](https://unity.com/)
[![Platform](https://img.shields.io/badge/Platform-Apple%20Vision%20Pro-black?logo=apple)](https://www.apple.com/apple-vision-pro/)
[![XR](https://img.shields.io/badge/XR-PolySpatial%20%7C%20OpenXR%20%7C%20ARKit-5c6bc0)](https://docs.unity3d.com/)
[![Project Type](https://img.shields.io/badge/Project-Research%20Prototype-2e7d32)](#about)
[![Collaboration](https://img.shields.io/badge/Development-Group%20Project-1565c0)](#team-and-context)

An Apple Vision Pro research project built in Unity that lets users place evolving tile-based elements onto real-world surfaces detected in their room.

This project was developed for school as a group project, and it became one of the most valuable learning experiences in my early XR development work. It helped me build practical experience with Unity, Apple Vision Pro deployment, spatial interaction design, surface detection, raycasting, meshes, layers, and collaborative GitHub workflows.

## Table of Contents
- [About](#about)
- [What Makes This Project Strong Portfolio Work](#what-makes-this-project-strong-portfolio-work)
- [My Contributions and Learning](#my-contributions-and-learning)
- [Tech Stack](#tech-stack)
- [Team and Context](#team-and-context)
- [Running the Project](#running-the-project)

## About

`evoIsland VP` is a mixed reality prototype created to explore how digital systems can be placed into a user's physical environment using Apple Vision Pro. The core concept was to show simple tile evolution mechanics by allowing users to place tiles on detected real-world surfaces around the room.

The project combines spatial computing concepts with game-like interaction. Users interact with planes detected from the environment, then place tile-based content onto those surfaces to create and grow a spatial layout in mixed reality.

## What Makes This Project Strong Portfolio Work

- Built for **Apple Vision Pro** using **Unity** and the visionOS/XR toolchain.
- Developed as a **research-focused prototype**, not just a tutorial or template project.
- Implemented spatial interactions involving **surface detection**, **raycasting**, **layers**, **meshes**, and **GameObject spawning** on detected planes.
- Worked across multiple runtime environments, including **desktop simulation**, **mobile AR**, and **Apple Vision Pro deployment**.
- Strengthened real-world engineering habits through **team collaboration** and **GitHub-based version control**.

## My Contributions and Learning

This project was especially important for my growth because it taught me how to move from general programming into XR-focused development.

Key things I learned and applied:

- **Unity development workflow**: scene management, platform switching, build settings, and testing across multiple targets.
- **Apple Vision Pro development**: working with Unity's visionOS ecosystem and understanding how to prepare projects for headset deployment.
- **Surface detection and spatial computing principles**: detecting surfaces in the environment and designing interactions around those surfaces.
- **Raycasting and interaction logic**: using raycasts, layers, and meshes to determine where objects should be placed in 3D space.
- **Grid and tile systems**: creating and expanding a grid of tiles/textiles placed on surfaces to support the evolution concept.
- **GameObject spawning on planes**: spawning content on top of generated surface planes in a way that aligned with the room layout.
- **Version control in a team setting**: using GitHub more seriously in a collaborative project and improving my workflow through pair collaboration.

## Tech Stack

- **Engine:** Unity `2022.3.62f2`
- **Target device:** Apple Vision Pro
- **XR packages:** PolySpatial, visionOS XR, OpenXR, ARKit
- **Language:** C#
- **Version control:** GitHub

## Team and Context

This was a **group project** created for school as a **research project**. Beyond the final prototype itself, one of the biggest takeaways was learning how to build technical work collaboratively: dividing tasks, integrating features, and using version control more effectively in a shared codebase.

## Running the Project

There are four ways to run the project:

1. In a virtual AR environment on a desktop device (`Windows 11` or `macOS`)
2. In a virtual HMD environment on a desktop device (`Windows 11` or `macOS`)
3. On an iOS device with AR support
4. On an Apple Vision Pro

### Run on Desktop with AR simulation

1. In Unity, click `File > Build Settings`
2. Select `Windows, Mac, Linux`, then click `Switch Platform`
3. Ensure that only `ARScene` is added to the scenes to build
4. Close the dialog
5. Open `ARScene`
6. Click the Play button

### Run on Desktop with HMD simulation

1. In Unity, click `File > Build Settings`
2. Select `Windows, Mac, Linux`, then click `Switch Platform`
3. Ensure that only `VPScene` is added to the scenes to build
4. Close the dialog
5. Open `VPScene`
6. Click the Play button

### Run on iOS

This method only works on `macOS`.

1. In Unity, click `File > Build Settings`
2. Select `iOS`, then click `Switch Platform`
3. Ensure that only `ARScene` is added to the scenes to build
4. Click `Build And Run`
5. If Unity prompts you to choose a directory, create an `ios/` folder in the project root and select it
6. Let Unity compile the Xcode project and assets
7. In Xcode, open the project settings and update `Signing & Capabilities` to use your Personal Team
8. Connect your iOS device to your laptop
9. Run the Xcode project

You must have an Apple Developer account to complete signing for device deployment.

Any updates made in Unity will require rebuilding and rerunning the iOS project.

### Run on Apple Vision Pro

To deploy to Apple Vision Pro, use the visionOS/Unity build workflow from the same Unity project and open the generated Xcode project for device deployment. The same Apple signing requirements apply.
