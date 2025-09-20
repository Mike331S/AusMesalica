using System;

namespace ProcessingModule
{
    /// <summary>
    /// Class containing logic for engineering unit conversion.
    /// </summary>
    public class EGUConverter
    {
        /// <summary>
        /// Converts the point value from raw to EGU form.
        /// </summary>
        /// <param name="scalingFactor">The scaling factor (A).</param>
        /// <param name="deviation">The deviation (B).</param>
        /// <param name="rawValue">The raw value.</param>
        /// <returns>The value in engineering units.</returns>
		public double ConvertToEGU(double scalingFactor, double deviation, ushort rawValue)
        {
            // IZMENA: Implementacija formule: EGU = A * raw + B
            return scalingFactor * rawValue + deviation;
        }

        /// <summary>
        /// Converts the point value from EGU to raw form.
        /// </summary>
        /// <param name="scalingFactor">The scaling factor (A).</param>
        /// <param name="deviation">The deviation (B).</param>
        /// <param name="eguValue">The EGU value.</param>
        /// <returns>The raw value.</returns>
		public ushort ConvertToRaw(double scalingFactor, double deviation, double eguValue)
        {
            //Implementacija formule: raw = (EGU - B) / A
            if (scalingFactor == 0)
            {
                // Izbegavamo deljenje sa nulom, mada se ne bi trebalo desiti.
                return 0;
            }

            // Vrednost mora biti pozitivan ceo broj (ushort)
            double raw = (eguValue - deviation) / scalingFactor;
            return (ushort)Math.Round(Math.Max(0, raw));
        }
    }
}