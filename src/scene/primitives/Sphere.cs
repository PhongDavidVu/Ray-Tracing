using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent an (infinite) plane in a scene.
    /// </summary>
    public class Sphere : SceneEntity
    {
        private Vector3 center;
        private double radius;
        private Material material;

        /// <summary>
        /// Construct a sphere given its center point and a radius.
        /// </summary>
        /// <param name="center">Center of the sphere</param>
        /// <param name="radius">Radius of the spher</param>
        /// <param name="material">Material assigned to the sphere</param>
        public Sphere(Vector3 center, double radius, Material material)
        {
            this.center = center;
            this.radius = radius;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the sphere, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        /// external source:
        /// https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-sphere-intersection
        public RayHit Intersect(Ray ray)
        {
            Vector3 V = ray.Origin - center; 
            double a = ray.Direction.Dot(ray.Direction);
            double b = 2* V.Dot(ray.Direction);
            double c =  V.Dot(V) - radius*radius;
            double discriminant = b*b - 4*a*c;
            double t;
            if (discriminant < 0 ) return null;
            
            double t0 = (-b - Math.Sqrt(discriminant));
            double t1 =  (-b + Math.Sqrt(discriminant));
            if (t0 >0 && t0<t1) t = t0;
            else t = t1;
            t = t/(2*a);
            if (t<=0) return null;
            
            return new RayHit(ray.Origin+ray.Direction*t,((ray.Origin+ray.Direction*t)-center).Normalized(), ray.Direction, material);
              
        }
      

        /// <summary>
        /// The material of the sphere.
        /// </summary>
        public Material Material { get { return this.material; } }
        public Vector3 Center {get {return this.center; }}
    }

}
