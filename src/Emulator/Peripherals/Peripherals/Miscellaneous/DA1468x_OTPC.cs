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
    [AllowedTranslations(AllowedTranslation.ByteToWord | AllowedTranslation.WordToDoubleWord | AllowedTranslation.ByteToDoubleWord)]
    public sealed class DA1468x_OTPC : IDoubleWordPeripheral, IKnownSize
    {
        public DA1468x_OTPC(Machine machine)
        {
            var registersMap = new Dictionary<long, DoubleWordRegister>
            {
                {(long)Registers.Mode, new DoubleWordRegister(this, 0x0)
                    .WithValueField(0, 3, name: "OTPC_MODE_MODE")
                    .WithReservedBits(3, 1)
                    .WithTaggedFlag("OTPC_MODE_USE_DMA", 4)
                    .WithTaggedFlag("OTPC_MODE_FIFO_FLUSH", 5)
                    .WithTaggedFlag("OTPC_MODE_ERR_RESP_DIS", 6)
                    .WithReservedBits(7, 1)
                    .WithTaggedFlag("OTPC_MODE_USE_SP_ROWS", 8)
                    .WithTaggedFlag("OTPC_MODE_RLD_RR_REQ", 9)
                    .WithReservedBits(10, 21)
                },
                {(long)Registers.Stat, new DoubleWordRegister(this, 0x40)
                    .WithFlag(0, name: "OTPC_STAT_PRDY",  mode: FieldMode.Read)
                    .WithFlag(1, name: "OTPC_STAT_PERR_UNC",  mode: FieldMode.Read)
                    .WithFlag(2, name: "OTPC_STAT_COR",  mode: FieldMode.Read)
                    .WithFlag(3, name: "OTPC_STAT_PZERO",  mode: FieldMode.Read)
                    .WithFlag(4, name: "OTPC_STAT_TRDY", mode: FieldMode.Read)
                    .WithFlag(5, name: "OTPC_STAT_TERROR", mode: FieldMode.Read)
                    .WithFlag(6, name: "OTPC_START_ARDY", mode: FieldMode.Read)
                    .WithFlag(7, name: "OTPC_STAT_RERROR", mode: FieldMode.WriteOneToClear | FieldMode.ReadToClear)
                    .WithValueField(8, 4, name: "OTPC_STAT_FWORDS", mode: FieldMode.Read)
                    .WithReservedBits(12, 4)
                    .WithValueField(16, 14, name: "OTPC_STAT_NWORDS", mode: FieldMode.Read)
                    .WithReservedBits(30, 2)
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
            Stat = 0x8,
        }
    }
}
