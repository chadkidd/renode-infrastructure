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
                {(long)Registers.PllSysCtrl1, new WordRegister(this, 0x100)
                    .WithFlag(0, out pllEnable, name: "PLL_EN")
                    .WithFlag(1, out ldoPllEnable, name: "LDO_PLL_ENABLE")
                    .WithFlag(2, name: "LDO_PLL_VREF_HOLD")
                    .WithReservedBits(3, 5)
                    .WithValueField(8, 7, name: "PLL_R_DIV")
                    .WithReservedBits(15, 1)
                },
                {(long)Registers.PllSysCtrl2, new WordRegister(this, 0x26)
                    .WithValueField(0, 7, name: "PLL_N_DIV")
                    .WithReservedBits(7, 5)
                    .WithValueField(12, 2, name: "PLL_DEL_SEL")
                    .WithFlag(14, name: "PLL_SEL_MIN_CUR_INT")
                    .WithReservedBits(15, 1)
                },
                {(long)Registers.PllSysStatus, new WordRegister(this, 0x3)
                    .WithFlag(0, name: "PLL_LOCK_FINE", mode: FieldMode.Read, valueProviderCallback: (_) => pllEnable.Value)
                    .WithFlag(1, name: "LDO_PLL_OK", mode: FieldMode.Read, valueProviderCallback: (_) => ldoPllEnable.Value)
                    .WithReservedBits(2, 3)
                    .WithTag("PLL_BEST_MIN_CUR", 5, 6)
                    .WithTaggedFlag("PLL_CALIBR_END", 11)
                    .WithReservedBits(12, 4)
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
        private readonly IFlagRegisterField ldoPllEnable;
        private readonly IFlagRegisterField pllEnable;
        private enum Registers
        {
            SetFreeze = 0x0,
            ResetFreeze = 0x2,
            PllSysCtrl1 = 0x10,
            PllSysCtrl2 = 0x12,
            PllSysStatus = 0x16,
        }
    }
}
