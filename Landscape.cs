using System;
using System.Collections.Generic;

namespace PlanetGenerator
{
	public class Point
	{
		public float X;
		public float Y;
		public float Z;

		public Point(float x,float y,float z)
		{
			X = x;
			Y = y;
			Z = z;
		}
	}
	public class Poly
	{
		public Poly(Point a, Point b, Point c)
		{
			A = a;
			B = b;
			C = c;
		}
		public Point A;
		public Point B;
		public Point C;
	}

	public class Landscape
	{
		List<Poly> polys;

		public Landscape ()
		{
			polys = new List<Poly> ();

			var r = 0.707106781f; // 1/(sqrt 2)
			var a = new Point(r, r, r); 
			var b = new Point(r,-r,-r);
			var c = new Point(-r,r,-r); 
			var d = new Point(-r,-r,r);

			polys.Add(new Poly (a, b, c));
			polys.Add(new Poly (d, b, a));
			polys.Add(new Poly (a, c, d));
			polys.Add(new Poly (d, c, b));
		}

		public IEnumerable<Poly> GetPolys()
		{
			foreach (var poly in polys)
				yield return poly;
		}

	}
}

