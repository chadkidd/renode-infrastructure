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
    [AllowedTranslations(AllowedTranslation.ByteToWord)]
    public sealed class DA1468x_GPREG : IWordPeripheral, IKnownSize
    {
        public DA1468x_GPREG(Machine machine)
        {
            var registersMap = new Dictionary<long, WordRegister>
            {
                {(long)Registers.SetFreeze, new WordRegister(this, 0x0)
                    .WithFlag(0, name: "FRZ_WKUPTIM")
                    .WithFlag(1, name: "FRZ_SWTIM0")
                    .WithFlag(2, name: "FRZ_BLETIM")
                    .WithFlag(3, name: "FRZ_WDOG")
                    .WithFlag(4, name: "FRZ_USB")
                    .WithFlag(5, name: "FRZ_DMA")
                    .WithFlag(6, name: "FRZ_SWTIM1")
                    .WithFlag(7, name: "FRZ_SWTIM2")
                    .WithReservedBits(8, 8)
                },
                {(long)Registers.ResetFreeze, new WordRegister(this, 0x0)
                    .WithFlag(0, name: "FRZ_WKUPTIM")
                    .WithFlag(1, name: "FRZ_SWTIM0")
                    .WithFlag(2, name: "FRZ_BLETIM")
                    .WithFlag(3, name: "FRZ_WDOG")
                    .WithFlag(4, name: "FRZ_USB")
                    .WithFlag(5, name: "FRZ_DMA")
                    .WithFlag(6, name: "FRZ_SWTIM1")
                    .WithFlag(7, name: "FRZ_SWTIM2")
                    .WithReservedBits(8, 8)
                },

            };

            registers = new WordRegisterCollection(this, registersMap);
        }

        public ushort ReadWord(long offset)
        {
            return registers.Read(offset);
        }

        public void WriteWord(long offset, ushort value)
        {
            registers.Write(offset, value);
        }

        public void Reset()
        {
            registers.Reset();
        }

        public long Size => 0x18;

        private readonly WordRegisterCollection registers;

        private enum Registers
        {
            SetFreeze = 0x0,
            ResetFreeze = 0x2,
        }
    }
}
