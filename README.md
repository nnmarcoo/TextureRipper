# <img src="TextureRipper/Images/iconorig.png" alt="Icon" width="15px" /> TextureRipper

![GitHub](https://img.shields.io/badge/license-MIT-green)
![GitHub release (latest by date)](https://img.shields.io/github/v/release/nnmarcoo/TextureRipper)
![GitHub issues](https://img.shields.io/github/issues/nnmarcoo/TextureRipper)
![GitHub pull requests](https://img.shields.io/github/issues-pr/nnmarcoo/TextureRipper)

TextureRipper is a tool written in C# using WPF that allows you to select a plane of an image and fix the perspective to face the camera. The purpose of this tool is to rip textures from images that are otherwise unobtainable.

## Features

- Select a plane in an image and define four corner points.
- Automatically correct the perspective of the selected plane to face the camera.
- Export the corrected image with the fixed perspective.
- User-friendly graphical user interface built with WPF.

## Screenshots

<details>
  <summary>Click to expand</summary>
  
  ![Steep Sign](TextureRipper/Images/steepsign.png)
</details>

## Usage

- Drag/drop or select an image to work with
- Left click to place/drag a point
- Right click to pan the image
- Scroll to zoom
- Tab to cycle the selected point
- Arrow keys or WASD to shift selected pixel
- Shift + arrow keys or WASD to shift selected quad
- R to rotate the output
