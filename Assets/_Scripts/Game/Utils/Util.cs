namespace _Scripts.Game
{
    public class Util
    {
        // Get an angle into -180 to +180 range
        public static float FixAnglePlusMinusDegrees(float ang)
        {
            while(ang >= 180.0f)
                ang -= 360.0f;
            while(ang < -180.0f)
                ang += 360.0f;
            return ang;
        }
    }
}