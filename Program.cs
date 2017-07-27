using System;

namespace PlanetGenerator
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Hello World!");
			Console.WriteLine ("Press '1' to split polygons");

			var landscape = new Landscape ();
			using (GameRenderer gameRenderer = new GameRenderer(landscape))
			{
				gameRenderer.Run(30.0);
			}
		}
	}
}
