﻿using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Karonda.ModbusTcp.Entity;
using Karonda.ModbusTcp.Entity.Function;
using Karonda.ModbusTcp.Entity.Function.Request;
using Karonda.ModbusTcp.Entity.Function.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Karonda.ModbusTcp.Handler
{
    public class ModbusDecoder : ByteToMessageDecoder
    {
        private bool isServerMode;
        private readonly short MaxFunctionCode = 0x80;

        public ModbusDecoder(bool isServerMode)
        {
            this.isServerMode = isServerMode;
        }
        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            //Transaction Identifier + Protocol Identifier + Length + Unit Identifier + Function Code
            if (input.Capacity < 2 + 2 + 2 + 1 + 1)
            {
                return;
            }

            ModbusHeader header = new ModbusHeader(input);
            short functionCode = input.ReadByte();
            ModbusFunction function = null;

            switch ((ModbusCommand)functionCode)
            {
                case ModbusCommand.ReadHoldingRegisters:
                    if (isServerMode) function = new ReadHoldingRegistersRequest();
                    else function = new ReadHoldingRegistersResponse();
                    break;
            }


            if (functionCode >= MaxFunctionCode)
            {
                function = new ExceptionFunction(functionCode);
            }
            else if(function == null)
            {
                function = new ExceptionFunction(functionCode, 0x01);
            }

            function.Decode(input);
            ModbusFrame frame = new ModbusFrame(header, function);

            output.Add(frame);
        }
    }
}
