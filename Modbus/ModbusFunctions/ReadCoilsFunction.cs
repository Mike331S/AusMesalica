using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read coil functions/requests.
    /// </summary>
    public class ReadCoilsFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadCoilsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
		public ReadCoilsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc/>
        public override byte[] PackRequest()
        {
            ModbusReadCommandParameters mrcp = this.CommandParameters as ModbusReadCommandParameters;
            byte[] request = new byte[12];

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mrcp.TransactionId)), 0, request, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mrcp.ProtocolId)), 0, request, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mrcp.Length)), 0, request, 4, 2);
            request[6] = mrcp.UnitId;
            request[7] = mrcp.FunctionCode;
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mrcp.StartAddress)), 0, request, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mrcp.Quantity)), 0, request, 10, 2);

            return request;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            var retVal = new Dictionary<Tuple<PointType, ushort>, ushort>();
            ModbusReadCommandParameters mrcp = this.CommandParameters as ModbusReadCommandParameters;

            if (response[7] == (mrcp.FunctionCode + 0x80))
            {
                HandeException(response[8]);
            }
            else
            {
                int byteCount = response[8];
                for (int i = 0; i < byteCount; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        ushort address = (ushort)(mrcp.StartAddress + i * 8 + j);
                        ushort value = (ushort)((response[9 + i] >> j) & 1);
                        retVal.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, address), value);
                    }
                }
            }
            return retVal;
        }
    }
}