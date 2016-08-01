using UnityEngine;
using System.Collections;

namespace UnityStandardAssets.Utility
{
    public static class Arithmetic
    {

        public static float Normalize( float value, float min, float max )
        {
            float result = Mathf.Clamp(((value - min) / (max - min)), min, max);
            return result;
        }

        public static float Normalize(int value, int min, int max)
        {
            int result = Mathf.Clamp(((value - min) / (max - min)), min, max);
            return result;
        }

        public static float PercentToRange(float value, float range, bool viaOrigin)
        {
            float scale = (viaOrigin) ? 2 : 1;
            return value * ((scale * range)) - range;
        }

        public static int PercentToRange(int value, int range, bool viaOrigin)
        {
            int scale = (viaOrigin) ? 2 : 1;
            return value * ((scale * range)) - range;
        }
    }
}
