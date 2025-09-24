using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write coil functions/requests.
    /// </summary>
    public class WriteSingleCoilFunction : ModbusFunction
    {
        public WriteSingleCoilFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
        }

        public override byte[] PackRequest()
        {
            ModbusWriteCommandParameters mwcp = this.CommandParameters as ModbusWriteCommandParameters;
            byte[] request = new byte[12];

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mwcp.TransactionId)), 0, request, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mwcp.ProtocolId)), 0, request, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mwcp.Length)), 0, request, 4, 2);
            request[6] = mwcp.UnitId;
            request[7] = mwcp.FunctionCode;
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)mwcp.OutputAddress)), 0, request, 8, 2);

            // ISPRAVKA: Vrednost za ON mora biti 0xFF00, a za OFF 0x0000
            ushort valueToSend = (mwcp.Value != 0) ? (ushort)0xFF00 : (ushort)0x0000;
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)valueToSend)), 0, request, 10, 2);

            return request;
        }

        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            var retVal = new Dictionary<Tuple<PointType, ushort>, ushort>();
            ModbusWriteCommandParameters mwcp = this.CommandParameters as ModbusWriteCommandParameters;

            if (response[7] == (mwcp.FunctionCode + 0x80))
            {
                HandeException(response[8]);
            }
            else
            {
                ushort address = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(response, 8));
                ushort valueResponse = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(response, 10));

                // Vracamo 1 ili 0 da bi nasa aplikacija to razumela
                ushort normalizedValue = (valueResponse == 0xFF00) ? (ushort)1 : (ushort)0;
                retVal.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, address), normalizedValue);
            }
            return retVal;
        }
    }
}