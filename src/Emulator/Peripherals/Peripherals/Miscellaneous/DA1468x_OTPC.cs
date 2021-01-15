//
// Copyright (c) 2010-2020 Antmicro
//
//  This file is licensed under the MIT License.
//  Full license text is available in 'licenses/MIT.txt'.
//

using System;
using System.Collections.Generic;
using Antmicro.Renode.Core;
using Antmicro.Renode.Core.Structure.Registers;
using Antmicro.Renode.Peripherals.Bus;

namespace Antmicro.Renode.Peripherals.Miscellaneous
{
    [AllowedTranslations(AllowedTranslation.ByteToWord | AllowedTranslation.WordToDoubleWord)]
    public sealed class DA1468x_OTPC : IDoubleWordPeripheral, IKnownSize
    {
        public DA1468x_OTPC(Machine machine)
        {
            var registersMap = new Dictionary<long, DoubleWordRegister>
            {
                {(long)Registers.Mode, new DoubleWordRegister(this, 0x0)
                    .WithValueField(0, 3, name: "OTPC_MODE_MODE")
                    .WithReservedBits(3, 1)
                    .WithTag("OTPC_MODE_USE_DMA", 4, 1)
                    .WithTag("OTPC_MODE_FIFO_FLUSH", 5, 1)
                    .WithTag("OTPC_MODE_ERR_RESP_DIS", 6, 1)
                    .WithReservedBits(7, 1)
                    .WithTag("OTPC_MODE_USE_SP_ROWS", 8, 1)
                    .WithTag("OTPC_MODE_RLD_RR_REQ", 9, 1)
                    .WithReservedBits(10, 21)
                },
                
            };

            registers = new DoubleWordRegisterCollection(this, registersMap);
        }

        public uint ReadDoubleWord(long offset)
        {
            return registers.Read(offset);
        }

        public void WriteDoubleWord(long offset, uint value)
        {
            registers.Write(offset, value);
        }

        public void Reset()
        {
            registers.Reset();
        }

        public long Size => 0x30;

        private readonly DoubleWordRegisterCollection registers;


        private enum Registers
        {
            Mode = 0x0,
        }
    }
}
