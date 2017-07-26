using OpenTK;
using System;
using System.Collections.Generic;

namespace PlanetGenerator
{
	//public class Point
	//{
	//	public float X;
	//	public float Y;
	//	public float Z;

	//	public Point(float x,float y,float z)
	//	{
	//		X = x;
	//		Y = y;
	//		Z = z;
	//	}
	//}
	public class Poly
	{
		public Poly(Vector3 a, Vector3 b, Vector3 c)
		{
			A = a;
			B = b;
			C = c;
		}
		public Vector3 A;
		public Vector3 B;
		public Vector3 C;

        public Vector3 Normal()
        {
            //var a = new Vector3(A.X, A.Y, A.Z);
            //var b = new Vector3(B.X, B.Y, B.Z);
            //var c = new Vector3(C.X, C.Y, C.Z);
            var dir = Vector3.Cross(B - A, C - A);
            var norm = Vector3.Normalize(dir);
            return norm;
        }

    }

    public class Landscape
	{
		List<Poly> polys;
        const float r = 0.5773502691896258f; // 1/(sqrt 3)

		public Landscape ()
		{
			polys = new List<Poly> ();

			var a = new Vector3(r, r, r); 
			var b = new Vector3(r,-r,-r);
			var c = new Vector3(-r,r,-r); 
			var d = new Vector3(-r,-r,r);

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

    
        public void ZoomIn()
        {
            var newPoints = new Dictionary<Vector3, Vector3>();
            var newPolys = new List<Poly>();
            foreach (var poly in polys)
            {
                //Input: A target point p and four vertices v1, . . . , v4 of a tetrahedron,
                //each with the following information:
                //Coordinates (xi, yi, zi)
                //Seed for pseudo-random number generator si
                //	Altitude value ai

                //	Re-order vertices v1, . . . , v4 so the longest edge of the
                //	tetrahedron is between v1 and v2, i.e., such that
                //	(x1 - x2)^2 + (y1 - y2)^2 + (z1 - z2)^2
                //	is maximized.

                var ab = (poly.A - poly.B).Length;
                var bc = (poly.B - poly.C).Length;
                var ca = (poly.C - poly.A).Length;

                var a = poly.A;
                var b = poly.B;
                var c = poly.C;
                Vector3 n;
                Vector3 m = new Vector3();

                if (bc > ab && bc > ca)
                {
                    a = poly.B;
                    b = poly.C;
                    c = poly.A;
                }
                else if (ca > ab && ca > bc)
                {
                    a = poly.C;
                    b = poly.A;
                    c = poly.B;
                }

                n = ((poly.A + poly.B) / 2);
                if (newPoints.ContainsKey(n))
                {
                    m = newPoints[n];
                }
                n = ((poly.B + poly.C) / 2);
                if (newPoints.ContainsKey(n))
                {
                    m = newPoints[n];
                    a = poly.B;
                    b = poly.C;
                    c = poly.A;
                }
                n = ((poly.C + poly.A) / 2);
                if (newPoints.ContainsKey(n))
                {
                    m = newPoints[n];
                    a = poly.C;
                    b = poly.A;
                    c = poly.B;
                }
                
                //	Define new vertex vm by
                //	  (xm, ym, zm) = ((x1 + x2)/2, (y1 + y2)/2, (z1 + z2)/2)
                //		l = sqrt( (x1 - x2)^2 + (y1 - y2)^2 + (z1 - z2)^2)
                //		sm = random((s1 + s2)/2)
                //		am = (a1 + a2)/2 + offset(sm, l, a1, a2)

                if (m.Length == 0)
                {
                    n = ((a + b) / 2);
                    m = n * (1 / n.Length); // normalize point to radius 1

                    newPoints.Add(n, m);
                }
                newPolys.Add(new Poly(a, m, c));
                newPolys.Add(new Poly(m, b, c));

                //	 If p is contained in the tetrahedron defined by the four
                //		vertices vm, v1, v3 and v4, set v2 = vm. Otherwise, set
                //        v1 = vm.

                //	Repeat Until: l is small enough

                //	Return: (a1 + a2 + a3 + a4)/4
            }
            polys = newPolys;
        }    
    }
}

