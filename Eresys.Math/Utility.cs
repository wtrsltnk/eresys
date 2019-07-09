using System;

namespace Eresys.Math
{
    public static class Utility
    {
        public static float[] ToFloatArray(this Matrix matrix)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException(nameof(matrix));
            }

            float[] output = new float[16];
            int k = 0;

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    output[k++] = matrix.element[i, j];
                }
            }

            return output;
        }
    }
}
