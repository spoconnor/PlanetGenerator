using System;

namespace PlanetGenerator
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Hello World!");

			var landscape = new Landscape ();
			using (GameRenderer gameRenderer = new GameRenderer(landscape))
			{
				gameRenderer.Run(30.0);
			}
		}
	}
}
