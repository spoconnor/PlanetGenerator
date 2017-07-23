using System;

namespace PlanetGenerator
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Hello World!");

			var landscape = new Landscape ();
			using (TestGameWindow test = new TestGameWindow())
			{
				test.Run ();
			}
			/*
			using (GameRenderer gameRenderer = new GameRenderer(landscape))
			{
				gameRenderer.Run(30.0);
			}*/
		}

		/*
		Input: A target point p and four vertices v1, . . . , v4 of a tetrahedron,
		each with the following information:

		Coordinates (xi, yi, zi)
		Seed for pseudo-random number generator si
			Altitude value ai

			Repeat:

			1 Re-order vertices v1, . . . , v4 so the longest edge of the
			tetrahedron is between v1 and v2, i.e., such that
			(x1 - x2)^2 + (y1 - y2)^2 + (z1 - z2)^2
			is maximized.

			2 Define new vertex vm by

			(xm, ym, zm) = ((x1 + x2)/2, (y1 + y2)/2, (z1 + z2)/2)
				l = sqrt( (x1 - x2)^2 + (y1 - y2)^2 + (z1 - z2)^2)
				sm = random((s1 + s2)/2)
				am = (a1 + a2)/2 + offset(sm, l, a1, a2)

				3 If p is contained in the tetrahedron defined by the four
				vertices vm, v1, v3 and v4, set v2 = vm. Otherwise, set
		        v1 = vm.

			Until: l is small enough
			Return: (a1 + a2 + a3 + a4)/4
            */

	}
}
