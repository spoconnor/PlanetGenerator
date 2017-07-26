using OpenTK;
using System;
using System.Collections.Generic;
using Sean.WorldGenerator;

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

		public override bool Equals(object obj)
		{
			Poly item = obj as Poly;
			if (item == null)
				return false;
			return  item.A == this.A && item.B == this.B && item.C == this.C;
		}

		public override int GetHashCode()
		{
			return CombineHashCodes(new List<int>(){ A.GetHashCode (), B.GetHashCode (), C.GetHashCode () });
		}

		// System.String.GetHashCode(): http://referencesource.microsoft.com/#mscorlib/system/string.cs,0a17bbac4851d0d4
		// System.Web.Util.StringUtil.GetStringHashCode(System.String): http://referencesource.microsoft.com/#System.Web/Util/StringUtil.cs,c97063570b4e791a
		public static int CombineHashCodes(IEnumerable<int> hashCodes)
		{
			int hash1 = (5381 << 16) + 5381;
			int hash2 = hash1;
			int i = 0;
			foreach (var hashCode in hashCodes)
			{
				if (i % 2 == 0)
					hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ hashCode;
				else
					hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ hashCode;

				++i;
			}
			return hash1 + (hash2 * 1566083941);
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
			var selected = new List<Vector3>();
			var excluded = new List<Vector3>();

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

				var AB = (poly.A + poly.B) / 2;
				var BC = (poly.B + poly.C) / 2;
				var CA = (poly.C + poly.A) / 2;

				//var ab_len = (poly.A - poly.B).Length;
				//var bc_len = (poly.B - poly.C).Length;
				//var ca_len = (poly.C - poly.A).Length;

				Vector3 ab = new Vector3 ();
				Vector3 a, b, c;
                Vector3 m = new Vector3();

				if (selected.Contains (AB)) {
					a = poly.A;
					b = poly.B;
					c = poly.C;
					ab = AB;
				} else if (selected.Contains (BC)) {
					a = poly.B;
					b = poly.C;
					c = poly.A;
					ab = BC;
				} else if (selected.Contains (CA)) {
					a = poly.C;
					b = poly.A;
					c = poly.B;
					ab = CA;
				} else {
					if (excluded.Contains (AB)) {
						a = poly.B;
						b = poly.C;
						c = poly.A;
						ab = BC;
					} else if (excluded.Contains (BC)) {
						a = poly.C;
						b = poly.A;
						c = poly.B;
						ab = CA;
					} else if (excluded.Contains (CA)) {
						a = poly.A;
						b = poly.B;
						c = poly.C;
						ab = AB;
					} else {
						a = poly.A;
						b = poly.B;
						c = poly.C;
						ab = AB;
					}
   				}

                /*
                if (bc_len > ab_len && bc_len > ca_len)
                {
                    a = poly.B;
                    b = poly.C;
                    c = poly.A;
                }
                else if (ca_len > ab_len && ca_len > bc_len)
                {
                    a = poly.C;
                    b = poly.A;
                    c = poly.B;
                }
                */
       
                //	Define new vertex vm by
                //	  (xm, ym, zm) = ((x1 + x2)/2, (y1 + y2)/2, (z1 + z2)/2)
                //		l = sqrt( (x1 - x2)^2 + (y1 - y2)^2 + (z1 - z2)^2)
                //		sm = random((s1 + s2)/2)
                //		am = (a1 + a2)/2 + offset(sm, l, a1, a2)

                //if (m.Length == 0)
                //{
                m = ab * (1 / ab.Length); // normalize point to radius 1

                //newPoints.Add(n, m);
                //}
                newPolys.Add(new Poly(a, m, c));
                newPolys.Add(new Poly(m, b, c));

				selected.Add (AB);
				excluded.Add (BC);
				excluded.Add (CA);

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

