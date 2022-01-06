## SDF Baker and Ray Marching
![sdf_baker](https://github.com/douduck08/Unity-RayMarching/tree/master/images/sdf_baker.jpg)
![sdf_rendering](https://github.com/douduck08/Unity-RayMarching/tree/master/images/sdf_rendering.jpg)

### Features
SDF Baker
* Bake a single mesh info to a Texture3D asset.
* Has supported 2 methods: `CPUBruteForce` and `GPUBruteForce`.
* Method `GPUJumpFloodingAlgorithm` is in research.

Ray Marching
* Use simple Blinn-Phong lighting.
* Support 1 directional / point light, with soft shadow.
* Support up to 64 instances, 8 different sdf volume.
* Support transform of instances, include scaling.
* Not support hierarchy tranform of volumes.

### References 
#### SDF Rendering
* SIGNED DISTANCE FIELD RENDERING JOURNEY  
https://kosmonautblog.wordpress.com/2017/05/01/signed-distance-field-rendering-journey-pt-1/  
https://kosmonautblog.wordpress.com/2017/05/09/signed-distance-field-rendering-journey-pt-2/
* Ray Marching and Signed Distance Functions  
http://jamie-wong.com/2016/07/15/ray-marching-signed-distance-functions/#model-transformations
* soft shadows in raymarched SDFs  
https://iquilezles.org/www/articles/rmshadows/rmshadows.htm 

#### SDF texture generation (Brute Force)
* distance to triangle  
https://iquilezles.org/www/articles/triangledistance/triangledistance.htm
* intersectors  
https://iquilezles.org/www/articles/intersectors/intersectors.htm
* Möller-Trumbore algorithm  
https://www.scratchapixel.com/lessons/3d-basic-rendering/ray-tracing-rendering-a-triangle/moller-trumbore-ray-triangle-intersection

#### SDF texture generation (Jump Flooding)
* Jump Flooding in GPU with Applications to Voronoi Diagram and Distance Transform  
https://www.comp.nus.edu.sg/~tants/jfa/i3d06.pdf
* Fast Voronoi Diagrams and Distance Field Textures on the GPU With the Jump Flooding Algorithm  
https://blog.demofox.org/2016/02/29/fast-voronoi-diagrams-and-distance-dield-textures-on-the-gpu-with-the-jump-flooding-algorithm/
* SDFTextureGenerator  
https://github.com/cecarlsen/SDFTextureGenerator

#### Others
* The Quest for Very Wide Outlines  
https://bgolus.medium.com/the-quest-for-very-wide-outlines-ba82ed442cd9
* Distance Fields  
https://prideout.net/blog/distance_fields/