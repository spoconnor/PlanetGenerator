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
            AB = (A + B) / 2; 
            BC = (B + C) / 2;
            CA = (C + A) / 2;
		}
        public Vector3 A, B, C;
        public Vector3 AB, BC, CA; // Mid points

        public float ABLen {  get { return (A - B).Length; } }
        public float BCLen {  get { return (B - C).Length; } }
        public float CALen {  get { return (C - A).Length; } }

        public float MaxLength { get { return Math.Max(Math.Max(ABLen, BCLen), CALen); } }
        public Vector3 AverageVertex {  get { return (A + B + C) / 3; } }

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
        SortedSet<Poly> polys;
        const float r = 0.5773502691896258f; // 1/(sqrt 3)
        Random rnd = new Random();

		public Landscape ()
		{
			polys = new SortedSet<Poly> (new PolySorter());

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

        public class PolySorter : IComparer<Poly>
        {
            public int Compare(Poly p1, Poly p2)
            {
                return (p1.MaxLength < p2.MaxLength) ? 1 : -1;
            }
        }

        public void SplitPolys()
        {
			var canSkip = new List<int>();
			var done = new List<Vector3>();
            var newPolys = new SortedSet<Poly>(new PolySorter());
			var polyArray = new Poly[polys.Count];
			polys.CopyTo (polyArray);
			for (int i = 0; i < polyArray.Length; i++)
            {
				if (canSkip.Contains(i)) continue;
				var poly = polyArray[i];
				if (i>polyArray.Length/2)
				{
					newPolys.Add(poly);
					continue;
				}

				Vector3 a, b, c;
				Vector3 ab,bc,ca;
                Vector3 m = new Vector3();

                var abDone = done.Contains(poly.AB);
                var bcDone = done.Contains(poly.BC);
                var caDone = done.Contains(poly.CA);
                if (!abDone && (poly.ABLen >= poly.BCLen || bcDone) && (poly.ABLen >= poly.CALen || caDone))
                {
                    a = poly.A; b = poly.B; c = poly.C;
					ab = poly.AB; bc = poly.BC; ca = poly.CA;
                }
                else if (!bcDone && (poly.BCLen >= poly.ABLen || abDone) && (poly.BCLen >= poly.CALen || caDone))
                {
                    a = poly.B; b = poly.C; c = poly.A;
					ab = poly.BC; bc = poly.CA; ca = poly.AB;
                }
                else if (!caDone && (poly.CALen >= poly.ABLen || abDone) && (poly.CALen >= poly.BCLen || bcDone))
                {
                    a = poly.C; b = poly.A; c = poly.B;
					ab = poly.CA; bc = poly.AB; ca = poly.BC;
                }
                else
                {
					//Console.WriteLine ("Cant split");
                    newPolys.Add(poly);
                    done.Add(poly.AB);
                    done.Add(poly.BC);
                    done.Add(poly.CA);
                    continue;
                }

                float avg = (a.Length + b.Length + c.Length) / 3;
                float h = avg + (float)(rnd.NextDouble() - 0.5) * (0.1f * (a-b).Length);
                //m = ab * (1 / ab.Length); // normalize point to radius 1
                m = ab * (h / ab.Length);

                newPolys.Add(new Poly(a, m, c));
                newPolys.Add(new Poly(m, b, c));

				done.Add (ab);
				done.Add (bc);
				done.Add (ca);

                // find other poly sharing split ab line
				for (int j = i+1; j < polyArray.Length; j++)
                {
					var poly2 = polyArray[j];
                    if (poly2.AB == ab)
                    {
                        newPolys.Add(new Poly(poly2.A, m, poly2.C));
                        newPolys.Add(new Poly(m, poly2.B, poly2.C));
                        done.Add(poly2.BC);
                        done.Add(poly2.CA);
						canSkip.Add (j);
						break;
                    }
                    else if (poly2.BC == ab)
                    {
                        newPolys.Add(new Poly(poly2.B, m, poly2.A));
                        newPolys.Add(new Poly(m, poly2.C, poly2.A));
                        done.Add(poly2.AB);
                        done.Add(poly2.CA);
						canSkip.Add (j);
						break;
                    }
                    else if (poly2.CA == ab)
                    {
                        newPolys.Add(new Poly(poly2.C, m, poly2.B));
                        newPolys.Add(new Poly(m, poly2.A, poly2.B));
                        done.Add(poly2.AB);
                        done.Add(poly2.BC);
						canSkip.Add (j);
						break;
                    }
                }
            }
            polys = newPolys;
			Console.WriteLine ($"{polys.Count} polygons");
            CreateMap();
        }    

        void CreateMap()
        {
            var p = (int)(Math.Sqrt(polys.Count));
            int resolution = 1;
            while (resolution < p) resolution *= 2;
            Console.WriteLine($"Map Resolution = {resolution}");

            var map = new Sean.Shared.Array<byte>(resolution*2, resolution);
            for (int lon = 0; lon < resolution; lon++)
            {
                var longitude = lon * 360 / resolution;
                for (int lat = 0; lat < resolution; lat++)
                {
                    var latitude = lat * 180 / resolution;
                    var poly = FindPoly(lon, lat);
                }
            }
                
        }
        Poly FindPoly(int lon, int lat)
        {
            foreach(var poly in polys)
            {
                var ALog = Math.Asin(poly.A.X / poly.A.Z);
                var ALat = Math.Acos(poly.A.Y);

            }
            return null;
        }
    }
}

