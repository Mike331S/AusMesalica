namespace Common
{
    /// <summary>
    /// Interface representing analog point specific values.
    /// </summary>
    public interface IAnalogPoint : IPoint
    {
        /// <summary>
        /// Gets or sets the value in engineering units.
        /// </summary>
        double EguValue { get; set; }

        // IZMENA: Dodajemo propertije za skaliranje i alarme direktno na interfejs
        /// <summary>
        /// Gets the scaling factor (A).
        /// </summary>
        double ScaleFactor { get; }

        /// <summary>
        /// Gets the deviation/offset (B).
        /// </summary>
        double Deviation { get; }

        /// <summary>
        /// Gets the high alarm limit.
        /// </summary>
        double HighLimit { get; }

        /// <summary>
        /// Gets the low alarm limit.
        /// </summary>
        double LowLimit { get; }
    }
}