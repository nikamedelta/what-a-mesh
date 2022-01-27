# Usage of the Plugin
## Getting Started
To initially integrate the plugin into your Unity Project, simply go to `Assets -> Import Package -> Custom Package` and select the .unitypackage file. When asked about the files you want to import specifically, select everything. It is recommended to open the TestScene in WhatAMesh/Scenes and try out the mechanics of the slice and smudge (left click to smudge, richt click to slice) on the provided objects. That way you know that everything is working properly and you have a possibility to double check your own input implementations and how the project structure should be to work successfully. 

## Smudge
### Smudge with Mouse Input
Create a Smudge Tool Component via What a Mesh -> Create Deformation Controller and select the desired mouse input method. A deformable object must have a WhatAMeshObject component attached with `smudgeable` boolean set to true. 

## Slice 
### Slice with Mouse Input
Create a Slice Tool Component via What a Mesh -> Create Deformation Controller and select the slice tool. A deformable object must have a WhatAMeshObject component attached with `sliceable` boolean set to true. 

## Define own Tools and Inputs
You can create new tools based on the MeshData class, which provides a variety of util functions and structures vertices and triangles in a more accessible way. Simply have a class inheriting from the MeshData class and write your tool-specific code. 

Own inputs of the smudge and slice tool may be structured similar to the smudge/slice controller classes and how they pre- and postprocess the tool's results. 

## Troubleshooting
### Smudge
If an provided mouse input is not working as desired, check the values auf the inner and outer radius. For example, the whole object will be moved if they are bigger than the dimensions of the object itself. 
### Performance Issues
In the case of performance issues, especially if multiple deformations have taken place, the vertex count might be too big to be handled efficiently by Unity. Experiment with the remesh passes parameter (which should be set lower) and, if using the smudge, you can increase the deformation interval. As a result the smudge will take place every few seconds/miliseconds instead of every frame. 

Other plugins may be helpful, such as a [mesh simplifier](https://github.com/Whinarn/UnityMeshSimplifier), depending on your use case. 


If any other issues or questions arise, please contact me through the [itch.io page](https://nikame.itch.io/what-a-mesh) of What a Mesh!.