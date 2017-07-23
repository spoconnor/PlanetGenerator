using System;
using Sean.Shared;
using NoiseLibrary;

namespace PlanetGenerator
{
	public class Generator
	{
		private CImplicitModuleBase islandGenerator;
		private CImplicitModuleBase terrainGenerator;
		private CImplicitModuleBase biosphereGenerator;
		private Landscape worldInstance;
		private const int octaves = 1;
		private const double persistence = 0.4;

		public Generator(Landscape world, int seed)
		{
			Log.WriteInfo($"Creating Terrain Generator...");
			worldInstance = world;

			terrainGenerator = CreateTerrainGenerator();
		}

		private CImplicitModuleBase CreateTerrainGenerator()
		{
			var ground_gradient = new CImplicitGradient(x1: 0, x2: 0, y1: 0, y2: 1);

			var lowland_gradient = new CImplicitGradient(x1: 0, x2: 0, y1: 0.8, y2: 1);
			var lowland_shape_fractal = new CImplicitFractal(type: EFractalTypes.BILLOW, basistype: CImplicitBasisFunction.EBasisTypes.GRADIENT, interptype: CImplicitBasisFunction.EInterpTypes.QUINTIC, octaves: 2, freq: 1.25);
			var lowland_autocorrect = new CImplicitAutoCorrect(source: lowland_shape_fractal, low: -1, high: 1);
			var lowland_scale = new CImplicitScaleOffset(source: lowland_autocorrect, scale: 0.05, offset: 0.0);
			var lowland_cache = new CImplicitCache(lowland_scale);
			var lowland_y_scale = new CImplicitScaleDomain(source: lowland_cache, y: 0);
			var lowland_terrain = new CImplicitTranslateDomain(source: lowland_gradient, tx: 0.0, ty: lowland_y_scale, tz: 0.0);

			var mountain_gradient = new CImplicitGradient(x1: 0, x2: 0, y1: 0.7, y2: 1);
			var mountain_shape_fractal = new CImplicitFractal(type: EFractalTypes.RIDGEDMULTI, basistype: CImplicitBasisFunction.EBasisTypes.GRADIENT, interptype: CImplicitBasisFunction.EInterpTypes.QUINTIC, octaves: 4, freq: 3);
			var mountain_autocorrect = new CImplicitAutoCorrect(source: mountain_shape_fractal, low: -1, high: 1);
			var mountain_scale = new CImplicitScaleOffset(source: mountain_autocorrect, scale: 0.23, offset: 0.15);
			var mountain_cache = new CImplicitCache(mountain_scale);
			var mountain_y_scale = new CImplicitScaleDomain(source: mountain_cache, y: 0.25);
			var mountain_terrain = new CImplicitTranslateDomain(source: mountain_gradient, tx: 0.0, ty: mountain_y_scale, tz: 0.0);

			var terrain_type_fractal = new CImplicitFractal(type: EFractalTypes.FBM, basistype: CImplicitBasisFunction.EBasisTypes.GRADIENT, interptype: CImplicitBasisFunction.EInterpTypes.QUINTIC, octaves: 3, freq: 0.5);
			var terrain_autocorrect = new CImplicitAutoCorrect(source: terrain_type_fractal, low: 0, high: 1);
			var terrain_type_y_scale = new CImplicitScaleDomain(source: terrain_autocorrect, y: 0);
			var terrain_type_cache = new CImplicitCache(terrain_type_y_scale);
			var mountain_lowland_select = new CImplicitSelect(low: lowland_terrain, high: mountain_terrain, control: terrain_type_cache, threshold: 0.5, falloff: 0.2);
			var mountain_lowland_select_cache = new CImplicitCache(mountain_lowland_select);

			uint octaves = 8;
			double freq = 1.2;
			double xOffset = 5.65;
			double zOffset = 2.52;
			double scale = 0.22;
			var island_shape_fractal = new CImplicitFractal(type: EFractalTypes.MULTI,
				basistype: CImplicitBasisFunction.EBasisTypes.GRADIENT, interptype: CImplicitBasisFunction.EInterpTypes.QUINTIC, octaves: octaves, freq: freq);
			var island_autocorrect = new CImplicitAutoCorrect(source: island_shape_fractal, low: 0, high: 1);
			var island_translate = new CImplicitTranslateDomain(source: island_autocorrect, tx: (double)xOffset, ty: 1.0, tz: (double)zOffset);
			var island_offset = new CImplicitScaleOffset(source: island_translate, scale: 0.70, offset: -0.40);
			var island_scale = new CImplicitScaleDomain(source: island_offset, x: scale, y: 0, z: scale);
			var island_terrain = new CImplicitTranslateDomain(source: ground_gradient, tx: 0.0, ty: island_scale, tz: 0.0);

			var coastline_highland_lowland_select = new CImplicitTranslateDomain(source: mountain_lowland_select_cache, tx: 0.0, ty: island_terrain, tz: 0.0);

			return coastline_highland_lowland_select;
		}

		private CImplicitModuleBase CreateIslandTerrainGenerator(uint octaves, double freq, double xOffset, double zOffset, double scale)
		{
			var ground_gradient = new CImplicitGradient(x1: 0, x2: 0, y1: 0, y2: 1);
			var island_shape_fractal = new CImplicitFractal(type: EFractalTypes.MULTI, 
				basistype: CImplicitBasisFunction.EBasisTypes.GRADIENT, interptype: CImplicitBasisFunction.EInterpTypes.QUINTIC, octaves: octaves, freq: freq);
			var island_autocorrect = new CImplicitAutoCorrect(source: island_shape_fractal, low: 0, high: 1);
			var island_translate = new CImplicitTranslateDomain(source: island_autocorrect, tx: (double)xOffset, ty: 1.0, tz: (double)zOffset);
			var island_offset = new CImplicitScaleOffset(source: island_translate, scale: 0.70, offset: -0.40);
			var island_scale = new CImplicitScaleDomain(source: island_offset, x: scale, y: 0, z: scale);
			var island_terrain = new CImplicitTranslateDomain(source: ground_gradient, tx: 0.0, ty: island_scale, tz: 0.0);

			return island_terrain;
		}

	}
}