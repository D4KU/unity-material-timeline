<div align="center">

![](https://github.com/D4KU/unity-material-timeline/blob/master/Media%7E/MaterialTimeline.gif)

</div>

This is a *Unity Timeline* extension to animate and blend material properties.
It consists of two custom tracks:

| Track          | Description |
| -------------- | ----------- |
| Material Track | Change properties of a material directly, changing it everywhere in the scene.            |
| Renderer Track | Overwrite a selection of a renderer's material slots, changing only one specific object.* |

\* *Material Property Blocks* are used, so instancing isn't broken.

# Features

|                                    | Material Track | Renderer Track |
| ---------------------------------- | -------------- | -------------- |
| Layers (a.k.a. Override Tracks)    | &check;        | &check;        |
| Clip extrapolation                 | &check;        | &check;        |
| Set/Blend Float/Range/Color/Vector | &check;        | &check;        |
| Set/Blend* Texture2D/RenderTexture | &check;        | &check;        |
| Set/Blend Texture Tiling/Offset    | &check;        | &check;        |
| Set Texture3D                      | &check;        | &check;        |
| Blend Texture3D                    | &cross;        | &cross;        |
| Set CubeMap                        | &check;        | &check;        |
| Blend CubeMap                      | &cross;        | &cross;        |
| Overwrite with entire Material**   | &check;        | &cross;        |

All blending can be done between two clips, or with the original value set
in the material.

\* See [Use texture blending](#use-texture-blending) for how to activate this
feature.<br/>
\** Just uses `Material.Lerp` internally, so it's not able to blend textures.

# Installation

In your project folder, simply add this to the dependencies inside `Packages/manifest.json`:

`"com.d4ku.material-timeline": "https://github.com/D4KU/unity-material-timeline.git"`

Alternatively, you can:
* Clone this repository
* In Unity, go to `Window` > `Package Manager` > `+` > `Add Package from disk`
* Select `package.json` at the root of the package folder

## Use texture blending

This package ships a shader to blend textures, named *TextureBlend*. To tell
Unity to include it in builds, even if no scene has a dependency to it, add it
to the list of always included shaders under *ProjectSettings* > *Graphics*.
Without this shader the package functions normally, but textures are switched
instead of blended.
