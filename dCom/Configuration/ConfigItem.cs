using Common;
using System;
using System.Collections.Generic;

namespace dCom.Configuration
{
    internal class ConfigItem : IConfigItem
    {
        #region Fields

        private PointType registryType;
        private ushort numberOfRegisters;
        private ushort startAddress;
        private ushort decimalSeparatorPlace;
        private ushort minValue;
        private ushort maxValue;
        private ushort defaultValue;
        private string processingType;
        private string description;
        private int acquisitionInterval;
        private double scalingFactor;
        private double deviation;
        private double egu_max;
        private double egu_min;
        private ushort abnormalValue;
        private double highLimit;
        private double lowLimit;
        private int secondsPassedSinceLastPoll;

        #endregion Fields

        #region Properties

        public PointType RegistryType { get => registryType; set => registryType = value; }
        public ushort NumberOfRegisters { get => numberOfRegisters; set => numberOfRegisters = value; }
        public ushort StartAddress { get => startAddress; set => startAddress = value; }
        public ushort DecimalSeparatorPlace { get => decimalSeparatorPlace; set => decimalSeparatorPlace = value; }
        public ushort MinValue { get => minValue; set => minValue = value; }
        public ushort MaxValue { get => maxValue; set => maxValue = value; }
        public ushort DefaultValue { get => defaultValue; set => defaultValue = value; }
        public string ProcessingType { get => processingType; set => processingType = value; }
        public string Description { get => description; set => description = value; }
        public int AcquisitionInterval { get => acquisitionInterval; set => acquisitionInterval = value; }
        public double ScaleFactor { get => scalingFactor; set => scalingFactor = value; }
        public double Deviation { get => deviation; set => deviation = value; }
        public double EGU_Max { get => egu_max; set => egu_max = value; }
        public double EGU_Min { get => egu_min; set => egu_min = value; }
        public ushort AbnormalValue { get => abnormalValue; set => abnormalValue = value; }
        public double HighLimit { get => highLimit; set => highLimit = value; }
        public double LowLimit { get => lowLimit; set => lowLimit = value; }
        public int SecondsPassedSinceLastPoll { get => secondsPassedSinceLastPoll; set => secondsPassedSinceLastPoll = value; }

        #endregion Properties

        public ConfigItem(List<string> configurationParameters)
        {
            // --- Postojeci kod za parsiranje ---
            RegistryType = GetRegistryType(configurationParameters[0]);
            int temp;
            Int32.TryParse(configurationParameters[1], out temp);
            NumberOfRegisters = (ushort)temp;
            Int32.TryParse(configurationParameters[2], out temp);
            StartAddress = (ushort)temp;
            Int32.TryParse(configurationParameters[3], out temp);
            DecimalSeparatorPlace = (ushort)temp;
            Int32.TryParse(configurationParameters[4], out temp);
            MinValue = (ushort)temp;
            Int32.TryParse(configurationParameters[5], out temp);
            MaxValue = (ushort)temp;
            Int32.TryParse(configurationParameters[6], out temp);
            DefaultValue = (ushort)temp;
            ProcessingType = configurationParameters[7];
            Description = configurationParameters[8].TrimStart('@');
            if (configurationParameters[9].Equals("#"))
            {
                AcquisitionInterval = 1;
            }
            else
            {
                Int32.TryParse(configurationParameters[9], out temp);
                AcquisitionInterval = temp;
            }

            // Postavljanje podrazumevanih vrednosti
            ScaleFactor = 1; // A = 1
            Deviation = 0;   // B = 0

            // Provera tipa tacke pre parsiranja specificnih parametara
            if (RegistryType == PointType.DIGITAL_OUTPUT || RegistryType == PointType.DIGITAL_INPUT)
            {
                // Digitalne tacke imaju samo AbnormalValue (indeks 10)
                if (configurationParameters.Count > 10 && ushort.TryParse(configurationParameters[10], out ushort abnormal))
                {
                    AbnormalValue = abnormal;
                }
            }
            else if (RegistryType == PointType.ANALOG_OUTPUT || RegistryType == PointType.ANALOG_INPUT)
            {
                // Analogne tacke imaju A, B, HighAlarm, LowAlarm (indeksi 10, 11, 12, 13)
                if (configurationParameters.Count > 10 && double.TryParse(configurationParameters[10], out double scaleFactor))
                {
                    ScaleFactor = scaleFactor;
                }

                if (configurationParameters.Count > 11 && double.TryParse(configurationParameters[11], out double deviation))
                {
                    Deviation = deviation;
                }

                if (configurationParameters.Count > 12 && double.TryParse(configurationParameters[12], out double highLimit))
                {
                    HighLimit = highLimit;
                }

                if (configurationParameters.Count > 13 && double.TryParse(configurationParameters[13], out double lowLimit))
                {
                    LowLimit = lowLimit;
                }
            }
        }

        private PointType GetRegistryType(string registryTypeName)
        {
            PointType registryType;
            switch (registryTypeName)
            {
                case "DO_REG":
                    registryType = PointType.DIGITAL_OUTPUT;
                    break;

                case "DI_REG":
                    registryType = PointType.DIGITAL_INPUT;
                    break;

                case "IN_REG":
                    registryType = PointType.ANALOG_INPUT;
                    break;

                case "HR_INT":
                    registryType = PointType.ANALOG_OUTPUT;
                    break;

                default:
                    registryType = PointType.HR_LONG;
                    break;
            }
            return registryType;
        }
    }
}