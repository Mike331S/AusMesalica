using System;

namespace Common
{
    /// <summary>
    /// Interface representing a point.
    /// </summary>
    public interface IPoint
    {
        /// <summary>
        /// Gets the point identifier.
        /// </summary>
		int PointId { get; }

        // NOVO: Dodajemo Modbus adresu u osnovni interfejs
        /// <summary>
        /// Gets the address of the point.
        /// </summary>
        ushort Address { get; }

        /// <summary>
        /// Gets or sets the raw value.
        /// </summary>
		ushort RawValue { get; set; }

        /// <summary>
        /// Gets or sets the alarm state.
        /// </summary>
        AlarmType Alarm { get; set; }

        /// <summary>
        /// Gets or sets the configuration item of the point.
        /// </summary>
        IConfigItem ConfigItem { get; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        DateTime Timestamp { get; set; }
    }
}