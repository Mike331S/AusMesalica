using Common;

namespace ProcessingModule
{
    /// <summary>
    /// Class containing logic for alarm processing.
    /// </summary>
    public class AlarmProcessor
    {
        /// <summary>
        /// Processes the alarm for analog point.
        /// </summary>
        /// <param name="point">The analog point to check.</param>
        /// <returns>The alarm indication.</returns>
		public AlarmType GetAlarmForAnalogPoint(IAnalogPoint point)
        {
            // IZMENA: Logika za High/Low alarme
            if (point.EguValue > point.HighLimit)
            {
                return AlarmType.HIGH_ALARM;
            }

            if (point.EguValue < point.LowLimit)
            {
                return AlarmType.LOW_ALARM;
            }

            return AlarmType.NO_ALARM;
        }

        /// <summary>
        /// Processes the alarm for digital point.
        /// </summary>
        /// <param name="point">The digital point to check.</param>
        /// <returns>The alarm indication.</returns>
		public AlarmType GetAlarmForDigitalPoint(IDigitalPoint point)
        {
            //Logika za Abnormal alarm
            if (point.RawValue == point.AbnormalValue)
            {
                return AlarmType.ABNORMAL_VALUE;
            }

            return AlarmType.NO_ALARM;
        }
    }
}