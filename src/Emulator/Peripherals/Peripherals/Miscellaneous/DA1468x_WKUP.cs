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
using Antmicro.Renode.Logging;

namespace Antmicro.Renode.Peripherals.Miscellaneous
{
    [AllowedTranslations(AllowedTranslation.ByteToWord | AllowedTranslation.DoubleWordToWord)]
    public sealed class DA1468x_WKUP : IWordPeripheral, IKnownSize
    {
        public DA1468x_WKUP(Machine machine)
        {
            IRQ = new GPIO();
            var registersMap = new Dictionary<long, WordRegister>
            {
                {(long)Registers.Ctrl, new WordRegister(this, 0x0)
                    .WithValueField(0, 5, name: "WKUP_DEB_VALUE")
                    .WithTaggedFlag("WKUP_SFT_KEYHIT", 6)
                    .WithFlag(7, name: "WKUP_ENABLE_IRQ", writeCallback: (_, value) => irqEnable = value)
                    .WithReservedBits(8, 8)
                },
                {(long)Registers.Compare, new WordRegister(this, 0x0)
                    .WithValueField(0, 8, name: "COMPARE")
                    .WithReservedBits(8, 8)
                },
                {(long)Registers.ResetIrq, new WordRegister(this, 0x0)
                    .WithValueField(0, 16, name: "WKUP_IRQ_RESET", mode: FieldMode.Write, writeCallback: (_, value) =>
                    {
                        eventsCounter = 0;
                        irqEnable = false;
                    })
                },
                {(long)Registers.Counter, new WordRegister(this, 0x0)
                    .WithValueField(0, 8, name: "EVENT_VALUE", mode: FieldMode.Read, valueProviderCallback: (_) => eventsCounter)
                    .WithReservedBits(8, 8)
                },
                {(long)Registers.ResetCounter, new WordRegister(this, 0x0)
                    .WithValueField(0, 15, name: "WKUP_CNTR_RST", mode: FieldMode.Write, writeCallback: (_, value) =>
                    {
                        eventsCounter = 0;
                    })
                },
                {(long)Registers.SelectP0, new WordRegister(this, 0x0)
                    .WithValueField(0, 8, name: "WKUP_SELECT_P0")
                    .WithReservedBits(8, 8)
                },
                {(long)Registers.SelectP1, new WordRegister(this, 0x0)
                    .WithValueField(0, 8, name: "WKUP_SELECT_P1")
                    .WithReservedBits(8, 8)
                },
                {(long)Registers.SelectP2, new WordRegister(this, 0x0)
                    .WithValueField(0, 8, name: "WKUP_SELECT_P2")
                    .WithReservedBits(8, 8)
                },
                {(long)Registers.SelectP3, new WordRegister(this, 0x0)
                    .WithValueField(0, 8, name: "WKUP_SELECT_P3")
                    .WithReservedBits(8, 8)
                },
                {(long)Registers.SelectP4, new WordRegister(this, 0x0)
                    .WithValueField(0, 8, name: "WKUP_SELECT_P4")
                    .WithReservedBits(8, 8)
                },
                {(long)Registers.PolarityP0, new WordRegister(this, 0x0)
                    .WithValueField(0, 8, name: "WKUP_POL_P0")
                    .WithReservedBits(8, 8)
                },
                {(long)Registers.PolarityP1, new WordRegister(this, 0x0)
                    .WithValueField(0, 8, name: "WKUP_POL_P1")
                    .WithReservedBits(8, 8)
                },
                {(long)Registers.PolarityP2, new WordRegister(this, 0x0)
                    .WithValueField(0, 8, name: "WKUP_POL_P2")
                    .WithReservedBits(8, 8)
                },
                {(long)Registers.PolarityP3, new WordRegister(this, 0x0)
                    .WithValueField(0, 8, name: "WKUP_POL_P3")
                    .WithReservedBits(8, 8)
                },
                {(long)Registers.PolarityP4, new WordRegister(this, 0x0)
                    .WithValueField(0, 8, name: "WKUP_POL_P4")
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
            IRQ.Unset();
            registers.Reset();
        }

        public GPIO IRQ { get; }

        public long Size => 0x1E;

        private readonly WordRegisterCollection registers;
        private bool irqEnable;
        private uint eventsCounter = 0;

        private enum Registers
        {
            Ctrl = 0x0,
            Compare = 0x2,
            ResetIrq = 0x4,
            Counter = 0x6,
            ResetCounter = 0x8,
            SelectP0 = 0xA,
            SelectP1 = 0xC,
            SelectP2 = 0xE,
            SelectP3 = 0x10,
            SelectP4 = 0x12,
            PolarityP0 = 0x14,
            PolarityP1 = 0x16,
            PolarityP2 = 0x18,
            PolarityP3 = 0x1A,
            PolarityP4 = 0x1C,
        }
    }
}
