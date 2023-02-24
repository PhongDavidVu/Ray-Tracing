using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a triangle in a scene represented by three vertices.
    /// </summay>
    public class Triangle : SceneEntity
    {
        private Vector3 v0, v1, v2;
        private Material material;

        /// <summary>
        /// Construct a triangle object given three vertices.
        /// </summary>
        /// <param name="v0">First vertex position</param>
        /// <param name="v1">Second vertex position</param>
        /// <param name="v2">Third vertex position</param>
        /// <param name="material">Material assigned to the triangle</param>
        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Material material)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the triangle, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        /// external source:
        /// https://www.scratchapixel.com/lessons/3d-basic-rendering/ray-tracing-rendering-a-triangle/ray-triangle-intersection-geometric-solution
        public  RayHit Intersect(Ray ray)
        {
            Vector3 a1 = v1 - v0;
            Vector3 a2 = v2 - v0;

            Vector3 n = (a1.Cross(a2));
            double area = n.Length();
            
            double ndotray = n.Dot(ray.Direction);
            if (Math.Abs(ndotray) < float.Epsilon) return null;

            double d = -n.Dot(v0);

            double t = -(n.Dot(ray.Origin)+ d) / ndotray;

            if (t <= 0 ) return null;
            Vector3 intPoint = ray.Origin + ray.Direction*t;
            Vector3  arb;

            Vector3 edge0 = v1-v0;
            Vector3 p0 = intPoint - v0;
            arb = edge0.Cross(p0);
            if (n.Dot(arb) < 0) return null;

            Vector3 edge1 = v2-v1;
            Vector3 p1 = intPoint - v1;
            arb = edge1.Cross(p1);
            if (n.Dot(arb) < 0) return null;

            Vector3 edge2 = v0 - v2;
            Vector3 p2 = intPoint - v2;
            arb = edge2.Cross(p2);
            if (n.Dot(arb) < 0) return null;

            return new RayHit(intPoint,n.Normalized(), ray.Direction,this.material);


        }

        /// <summary>
        /// The material of the triangle.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
