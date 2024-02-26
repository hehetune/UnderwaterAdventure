
using UnityEngine;

namespace _Scripts.Game
{
    public class Bounds2D
    {
        public float				x0,y0,x1,y1;
        public Bounds2D(Vector3 centre, Vector3 size)
        {
            x0 = centre.x - (size.x*0.5f);
            y0 = centre.y - (size.y*0.5f);
            x1 = x0 + size.x;
            y1 = y0 + size.y;
        }
    }
}