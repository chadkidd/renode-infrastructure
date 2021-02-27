//
// Copyright (c) 2010-2019 Antmicro
//
// This file is licensed under the MIT License.
// Full license text is available in 'licenses/MIT.txt'.
//
using System;
using Antmicro.Renode.Core;
using Antmicro.Renode.Core.Structure.Registers;

namespace Antmicro.Renode.Peripherals.Analog
{
    public class DA1468x_ADC : BasicWordPeripheral, IKnownSize
    {
        public DA1468x_ADC(Machine machine, ushort initialResult) : base(machine)
        {
            adcResult = initialResult;
        }

        /* Chad Kidd - Basic functionality implemented to get initialization to pass
         *
         * TODO - may want to add ability to simulate conversions and activate a real interrupt
         * 
         */

        public override void Reset()
        {
            base.Reset();
        }

        public void SetResult(ushort result)
        {
            adcResult = result;
        }

        public long Size => 0xD;

        protected override void DefineRegisters()
        {

            Registers.Control.Define(this, 0x0)
                .WithFlag(0, name: "GP_ADC_EN")
                .WithFlag(1, name: "GP_ADC_START",
                    writeCallback: (_, value) => interrupt.Value = value,
                    valueProviderCallback: (_) => false)
                .WithFlag(2, name: "GP_ADC_CONT")
                .WithFlag(3, name: "GP_ADC_CLK_SEL")
                .WithFlag(4, out interrupt, name: "GP_ADC_INT", mode: FieldMode.Read)
                .WithFlag(5, name: "GP_ADC_MINT")
                .WithFlag(6, name: "GP_ADC_SE")
                .WithFlag(7, name: "GP_ADC_MUTE")
                .WithValueField(8, 5, name: "GP_ADC_SEL", writeCallback: (_, value) =>
                {
                    adcSelect = value;
                    if(value == 14)
                    {
                        adcResult = 0xC000;
                    }
                })
                .WithFlag(13, name: "GP_ADC_SIGN")
                .WithFlag(14, name: "GP_ADC_CHOP")
                .WithFlag(15, name: "GP_ADC_LDO_ZERO")
            ;
            Registers.Control2.Define(this, 0x0)
                .WithFlag(0, name: "GP_ADC_ATTN3X")
                .WithFlag(1, name: "GP_ADC_IDYN")
                .WithFlag(2, name: "GP_ADC_I20U")
                .WithFlag(3, name: "GP_ADC_DMA_EN")
                .WithReservedBits(4, 1)
                .WithValueField(5, 3, name: "GP_ADC_CONV_NRS")
                .WithValueField(8, 4, name: "GP_ADC_SMPL_TIME")
                .WithValueField(12, 4, name: "GP_ADC_STORE_DEL")
            ;
            Registers.Control3.Define(this, 0x40)
                .WithValueField(0, 8, name: "GP_ADC_EN_DEL")
                .WithValueField(8, 8, name: "GP_ADC_INTERVAL")
            ;
            Registers.OffsetPositive.Define(this, 0x200)
                .WithValueField(0, 10, name: "GP_ADC_OFFP")
                .WithReservedBits(10, 6)
            ;
            Registers.OffsetNegative.Define(this, 0x200)
                .WithValueField(0, 10, name: "GP_ADC_OFFN")
                .WithReservedBits(10, 6)
            ;
            Registers.ClearInterrupt.Define(this, 0x0)
                .WithValueField(0, 16, name: "GP_ADC_CLR_INT", writeCallback: (_, value) => interrupt.Value = false)
            ;
            Registers.Result.Define(this, 0x0)
                .WithValueField(0, 16, name: "GP_ADC_VAL", mode: FieldMode.Read, valueProviderCallback: (_) => adcResult)
            ;
        }

        private IFlagRegisterField interrupt;
        private ushort adcResult;
        private uint adcSelect = 0;

        private enum Registers
        {
            Control = 0x0,
            Control2 = 0x2,
            Control3 = 0x4,
            OffsetPositive = 0x6,
            OffsetNegative = 0x8,
            ClearInterrupt = 0xA,
            Result = 0xC
        }
    }
}
