//
// Copyright (c) 2010-2018 Antmicro
// Copyright (c) 2011-2015 Realtime Embedded
//
// This file is licensed under the MIT License.
// Full license text is available in 'licenses/MIT.txt'.
//
﻿using System;
using Antmicro.Renode.Peripherals.Bus;
using Antmicro.Renode.Logging;
using Antmicro.Renode.Core;
using System.Collections.Generic;
using Antmicro.Renode.Core.Structure.Registers;

namespace Antmicro.Renode.Peripherals.DMA
{
    [AllowedTranslations(AllowedTranslation.ByteToWord | AllowedTranslation.DoubleWordToWord)]
    public sealed class DA1468x_DMA : IWordPeripheral, IKnownSize
    {
        public DA1468x_DMA(Machine machine)
        {
            var registersMap = new Dictionary<long, WordRegister>
            {
                {(long)Registers.Dma0Ctrl, new WordRegister(this, 0x0)
                    .WithFlag(0, name: "DMA_ON")
                    .WithValueField(1, 2, name: "BW")
                    .WithFlag(3, name: "IRQ_ENABLE")
                    .WithFlag(4, name: "DREQ_MODE")
                    .WithFlag(5, name: "BINC")
                    .WithFlag(6, name: "AINC")
                    .WithFlag(7, name: "CIRCULAR")
                    .WithValueField(8, 3, name: "DMA_PRIO")
                    .WithFlag(11, name: "DMA_IDLE")
                    .WithFlag(12, name: "DMA_INT")
                    .WithReservedBits(13, 3)
                },
                {(long)Registers.Dma1Ctrl, new WordRegister(this, 0x0)
                    .WithFlag(0, name: "DMA_ON")
                    .WithValueField(1, 2, name: "BW")
                    .WithFlag(3, name: "IRQ_ENABLE")
                    .WithFlag(4, name: "DREQ_MODE")
                    .WithFlag(5, name: "BINC")
                    .WithFlag(6, name: "AINC")
                    .WithFlag(7, name: "CIRCULAR")
                    .WithValueField(8, 3, name: "DMA_PRIO")
                    .WithFlag(11, name: "DMA_IDLE")
                    .WithFlag(12, name: "DMA_INT")
                    .WithReservedBits(13, 3)
                },
                {(long)Registers.Dma2Ctrl, new WordRegister(this, 0x0)
                    .WithFlag(0, name: "DMA_ON")
                    .WithValueField(1, 2, name: "BW")
                    .WithFlag(3, name: "IRQ_ENABLE")
                    .WithFlag(4, name: "DREQ_MODE")
                    .WithFlag(5, name: "BINC")
                    .WithFlag(6, name: "AINC")
                    .WithFlag(7, name: "CIRCULAR")
                    .WithValueField(8, 3, name: "DMA_PRIO")
                    .WithFlag(11, name: "DMA_IDLE")
                    .WithFlag(12, name: "DMA_INT")
                    .WithReservedBits(13, 3)
                },
                {(long)Registers.Dma3Ctrl, new WordRegister(this, 0x0)
                    .WithFlag(0, name: "DMA_ON")
                    .WithValueField(1, 2, name: "BW")
                    .WithFlag(3, name: "IRQ_ENABLE")
                    .WithFlag(4, name: "DREQ_MODE")
                    .WithFlag(5, name: "BINC")
                    .WithFlag(6, name: "AINC")
                    .WithFlag(7, name: "CIRCULAR")
                    .WithValueField(8, 3, name: "DMA_PRIO")
                    .WithFlag(11, name: "DMA_IDLE")
                    .WithFlag(12, name: "DMA_INT")
                    .WithReservedBits(13, 3)
                },
                {(long)Registers.Dma4Ctrl, new WordRegister(this, 0x0)
                    .WithFlag(0, name: "DMA_ON")
                    .WithValueField(1, 2, name: "BW")
                    .WithFlag(3, name: "IRQ_ENABLE")
                    .WithFlag(4, name: "DREQ_MODE")
                    .WithFlag(5, name: "BINC")
                    .WithFlag(6, name: "AINC")
                    .WithFlag(7, name: "CIRCULAR")
                    .WithValueField(8, 3, name: "DMA_PRIO")
                    .WithFlag(11, name: "DMA_IDLE")
                    .WithFlag(12, name: "DMA_INT")
                    .WithReservedBits(13, 3)
                },
                {(long)Registers.Dma5Ctrl, new WordRegister(this, 0x0)
                    .WithFlag(0, name: "DMA_ON")
                    .WithValueField(1, 2, name: "BW")
                    .WithFlag(3, name: "IRQ_ENABLE")
                    .WithFlag(4, name: "DREQ_MODE")
                    .WithFlag(5, name: "BINC")
                    .WithFlag(6, name: "AINC")
                    .WithFlag(7, name: "CIRCULAR")
                    .WithValueField(8, 3, name: "DMA_PRIO")
                    .WithFlag(11, name: "DMA_IDLE")
                    .WithFlag(12, name: "DMA_INT")
                    .WithReservedBits(13, 3)
                },
                {(long)Registers.Dma6Ctrl, new WordRegister(this, 0x0)
                    .WithFlag(0, name: "DMA_ON")
                    .WithValueField(1, 2, name: "BW")
                    .WithFlag(3, name: "IRQ_ENABLE")
                    .WithFlag(4, name: "DREQ_MODE")
                    .WithFlag(5, name: "BINC")
                    .WithFlag(6, name: "AINC")
                    .WithFlag(7, name: "CIRCULAR")
                    .WithValueField(8, 3, name: "DMA_PRIO")
                    .WithFlag(11, name: "DMA_IDLE")
                    .WithFlag(12, name: "DMA_INT")
                    .WithReservedBits(13, 3)
                },
                {(long)Registers.Dma7Ctrl, new WordRegister(this, 0x0)
                    .WithFlag(0, name: "DMA_ON")
                    .WithValueField(1, 2, name: "BW")
                    .WithFlag(3, name: "IRQ_ENABLE")
                    .WithFlag(4, name: "DREQ_MODE")
                    .WithFlag(5, name: "BINC")
                    .WithFlag(6, name: "AINC")
                    .WithFlag(7, name: "CIRCULAR")
                    .WithValueField(8, 3, name: "DMA_PRIO")
                    .WithFlag(11, name: "DMA_IDLE")
                    .WithFlag(12, name: "DMA_INT")
                    .WithReservedBits(13, 3)
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

        public long Size => 0x88;

        private readonly WordRegisterCollection registers;

        private enum Registers
        {
            Dma0Ctrl = 0xC,
            Dma1Ctrl = 0x1C,
            Dma2Ctrl = 0x2C,
            Dma3Ctrl = 0x3C,
            Dma4Ctrl = 0x4C,
            Dma5Ctrl = 0x5C,
            Dma6Ctrl = 0x6C,
            Dma7Ctrl = 0x7C,
        }
    }
}

