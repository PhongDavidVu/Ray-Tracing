using System;
using System.Collections.Generic;
namespace RayTracer
{
    /// <summary>
    /// Class to represent a ray traced scene, including the objects,
    /// light sources, and associated rendering logic.
    /// </summary>
    public class Scene
    {
        private SceneOptions options;
        private ISet<SceneEntity> entities;
        private ISet<PointLight> lights;

        private double _imagePlaneHeight;
        private double _imagePlaneWidth;
        
        /// <summary>
        /// Construct a new scene with provided options.
        /// </summary>
        /// <param name="options">Options data</param>
        public Scene(SceneOptions options = new SceneOptions())
        {
            this.options = options;
            this.entities = new HashSet<SceneEntity>();
            this.lights = new HashSet<PointLight>();
        }

        /// <summary>
        /// Add an entity to the scene that should be rendered.
        /// </summary>
        /// <param name="entity">Entity object</param>
        public void AddEntity(SceneEntity entity)
        {
            this.entities.Add(entity);
        }

         

        /// <summary>
        /// Add a point light to the scene that should be computed.
        /// </summary>
        /// <param name="light">Light structure</param>
        public void AddPointLight(PointLight light)
        {
            this.lights.Add(light);
        }

        /// <summary>
        /// Render the scene to an output image. This is where the bulk
        /// of your ray tracing logic should go... though you may wish to
        /// break it down into multiple functions as it gets more complex!
        /// </summary>
        /// <param name="outputImage">Image to store render output</param>
        /// External Source for AA
        /// https://raytracing.github.io/books/RayTracingInOneWeekend.html
        public void Render(Image outputImage)
        {
            Color actualColor = new Color(0,0,0);
        
            ComputeWorldImageBounds(outputImage);
            
            for(var y = 0; y< outputImage.Height;y++)
            for(var x = 0; x<outputImage.Width; x++)
            {
                var ray = PixelRay(x,y,outputImage);
                if (options.AAMultiplier ==1) actualColor = findColor(ray,0);
               
                else
                {
                    for (int x1 = 1; x1<= options.AAMultiplier; x1++)
                    for (int y1 = 1; y1<= options.AAMultiplier; y1++)
                    {
                        double nX,nY;
                        if (x1 % 2 == 0) nX =1;
                        else nX = -1;
                        if(y1 % 2 == 0) nY =1;
                        else nY = -1;

                        double xJiggle = (x1 * nX) / (2*options.AAMultiplier*outputImage.Width);
                        double yJiggle = (y1 * nY) / (2*options.AAMultiplier*outputImage.Height);
                        Vector3 xy = NormalizedImageToWorldCoord(x,y,outputImage);
                        Ray newRay = new Ray(new Vector3(0f,0f,0f), new Vector3(ray.Direction.X+xJiggle, ray.Direction.Y+yJiggle,1));
                        actualColor += findColor(newRay,0);
                    }
                    actualColor = actualColor / (Math.Pow(options.AAMultiplier,2));
                }
                outputImage.SetPixel(x, y, actualColor);

            }
           
            
        }
        /// External Source Reference for refractive and reflective function
        /// https://www.scratchapixel.com/lessons/3d-basic-rendering/introduction-to-shading/reflection-refraction-fresnel
        public Color  findColor(Ray ray, int recurse)
        {
            double nearest = double.PositiveInfinity;

            Color pColor = new Color(0,0,0);
            SceneEntity actualEntity = null;
            RayHit actualHit = null;
            double rr = 0;

            bool isShadow = false;
            
            if (recurse >= 4) return new Color(0,0,0);
            foreach (SceneEntity entity in this.entities)
            {
                RayHit hit = entity.Intersect(ray);
                if (hit != null && (hit.Position-ray.Origin).LengthSq() < nearest && (hit.Position-ray.Origin).LengthSq() > 0)
                {    
                    
                    actualHit = hit;
                    actualEntity = entity;
                    nearest = (hit.Position - ray.Origin).LengthSq();
                }
            }
            if (actualHit == null) return pColor;
            
            if (actualEntity.Material.Type is Material.MaterialType.Diffuse)
            {

                foreach (PointLight light in this.lights)
                {
                    isShadow = false;
                    Vector3 Ldis = light.Position - actualHit.Position;
                    Vector3 Lv = Ldis.Normalized();
                    Ray shadowRay = new Ray (actualHit.Position+(actualHit.Normal*0.001),Lv);
                    foreach (SceneEntity entity1 in this.entities) 
                    {
                        RayHit shadowHit = entity1.Intersect(shadowRay);  
                        if (shadowHit!= null && (Ldis.LengthSq()>(shadowHit.Position-actualHit.Position).LengthSq())) {
                            isShadow = true;                        
                        }
                        
                    }
                    if(!isShadow) pColor += actualEntity.Material.Color*light.Color*Math.Max(0,(actualHit.Normal.Dot((light.Position-actualHit.Position).Normalized())));    
                        
                }
                return pColor;
            }
            if (actualEntity.Material.Type is Material.MaterialType.Reflective)
            {
                Vector3 nOrigin;
                if (actualHit.Incident.Dot(actualHit.Normal) < 0)  nOrigin = actualHit.Position + (actualHit.Normal*0.001);
                else nOrigin = actualHit.Position - (actualHit.Normal*0.001);
                Ray refVec = new Ray (nOrigin,(actualHit.Incident - 2*actualHit.Incident.Dot(actualHit.Normal)*actualHit.Normal).Normalized());
                pColor = findColor(refVec,recurse+1);
                return pColor;
            }
            if (actualEntity.Material.Type is Material.MaterialType.Refractive)
            {
                Color refracColor = new Color(0f,0f,0f);

                double cos = Math.Clamp(actualHit.Incident.Dot(actualHit.Normal), -1 ,1);
                Vector3 normal = actualHit.Normal;
                double etai = 1, etat = actualEntity.Material.RefractiveIndex;
                // Outside => cos need to be positive
                if (cos < 0 ) cos = -cos;

                // Inside
                else 
                {
                    normal = -actualHit.Normal;
                    etai = actualEntity.Material.RefractiveIndex;
                    etat = 1;
                }
                double eta = etai/etat;
                double k = 1 - Math.Pow(eta,2)*(1-Math.Pow(cos,2));
                
                double sin = etai/ (etat) * Math.Sqrt(Math.Max(0,1-Math.Pow(cos,2)));
                if (sin>= 1) rr = 1;
                else 
                {
                    double cost = Math.Sqrt(Math.Max(0,1-sin*sin));
                    double rs = ((etat * Math.Abs(cos)) - (etai * Math.Abs(cos))) /  ((etat * Math.Abs(cos)) + (etai * Math.Abs(cos)));
                    double rp = ((etai * Math.Abs(cos))-(etat *Math.Abs(cos))) / ((etai * Math.Abs(cos))+(etat *Math.Abs(cos)));
                    rr = (rs * rs + rp * rp) / 2;

                }
                if (rr < 1) {
                    Vector3 nOrigin;
                    if (actualHit.Incident.Dot(actualHit.Normal) < 0)  nOrigin = actualHit.Position - (actualHit.Normal*0.001);
                    else nOrigin = actualHit.Position + (actualHit.Normal*0.001);
                    Ray reFracRay = new Ray (nOrigin, (eta * actualHit.Incident + (eta * Math.Abs(cos) - Math.Sqrt(k)) * normal).Normalized());
                    refracColor = findColor (reFracRay,recurse+1);
                   
                }
                // Fresnel part
                Vector3 nOrigin1;
                if (actualHit.Incident.Dot(actualHit.Normal) < 0)  nOrigin1 = actualHit.Position + (actualHit.Normal*0.001);
                else nOrigin1 = actualHit.Position - (actualHit.Normal*0.001);
                Ray refVec = new Ray (nOrigin1, (actualHit.Incident - 2*actualHit.Incident.Dot(actualHit.Normal)*actualHit.Normal).Normalized());
                Color reflColor = findColor(refVec,recurse+1);
                pColor = reflColor * (rr) + (refracColor * (1-rr));
                return pColor;

            }
            
            return pColor;
        }

        private Ray PixelRay(double x, double y, Image image)
    {
        var normX = (x + 0.5f) / image.Width;
        var normY = (y + 0.5f) / image.Height;

        var worldPixelCoord = NormalizedImageToWorldCoord(normX, normY, image);

        return new Ray (new Vector3(0f,0f,0f),worldPixelCoord.Normalized());
    }

    private Vector3 NormalizedImageToWorldCoord(double x, double y, Image image)
    {
        return new Vector3(
            this._imagePlaneWidth * (x - 0.5f),
            this._imagePlaneHeight * (0.5f - y),
            1f); 
    }
      private void ComputeWorldImageBounds(Image image)
    {
        var fov = 60f;
        var aspectRatio = (float)image.Width / image.Height;
        var width = Math.Tan(fov/2*Math.PI/180) * 2f;

        this._imagePlaneWidth = width;
        this._imagePlaneHeight = this._imagePlaneWidth / aspectRatio;
    }

    }
}
