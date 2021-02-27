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
    [AllowedTranslations(AllowedTranslation.ByteToWord | AllowedTranslation.DoubleWordToWord)]
    public sealed class DA1468x_ANAMISC : IWordPeripheral, IKnownSize
    {
        public DA1468x_ANAMISC(Machine machine)
        {
            var registersMap = new Dictionary<long, WordRegister>
            {
                {
                (long)Registers.AnaTest, new WordRegister(this, 0x0)
                   .WithValueField(0, 4, name: "TEST_STRUCTURE")
                   .WithFlag(4, name: "ACORE_TESTBUS_EN")
                   .WithReservedBits(5, 11)
                },
                {
                (long)Registers.Ctrl1, new WordRegister(this, 0x2000)
                    .WithValueField(0, 5, name: "CHARGE_LEVEL")
                    .WithFlag(5, name: "CHARGE_ON")
                    .WithTaggedFlag("NTC_DISABLE", 6)
                    .WithTaggedFlag("NTC_LOW_DISABLE", 7)
                    .WithValueField(8, 4, name: "CHARGE_CUR")
                    .WithValueField(12, 2, name: "DIE_TEMP_SET")
                    .WithTaggedFlag("DIE_TEMP_DISABLE", 14)
                    .WithReservedBits(15, 1)
                },
                {(long)Registers.Ctrl2, new WordRegister(this, 0xF07)
                    .WithValueField(0, 4, name: "CURRENT_GAIN_TRIM")
                    .WithValueField(4, 4, name: "CHARGER_VFLOAT_ADJ")
                    .WithValueField(8, 5, name: "CURRENT_OFFSET_TRIM")
                    .WithValueField(13, 3, name: "CHARGER_TEST")
                },
                {(long)Registers.Status, new WordRegister(this, 0x0)
                    .WithFlag(0, name: "CHARGER_CC_MODE")
                    .WithTaggedFlag("CHARGER_CV_MODE", 1)
                    .WithTaggedFlag("END_OF_CHARGE", 2)
                    .WithTaggedFlag("CHARGER_BATTEMP_LOW", 3)
                    .WithTaggedFlag("CHARGER_BATTEMP_OK", 4)
                    .WithTaggedFlag("CHARGER_BATTEMP_HIGH", 5)
                    .WithTaggedFlag("CHARGER_TMODE_PROT", 6)
                    .WithReservedBits(7, 9)
                },
                {(long)Registers.SocCtrl1, new WordRegister(this, 0xD880)
                    .WithFlag(0, name: "SOC_ENABLE")
                    .WithFlag(1, name: "SOC_RESET_CHARGE")
                    .WithFlag(2, name: "SOC_RESET_AVG")
                    .WithFlag(3, name: "SOC_MUTE")
                    .WithFlag(4, name: "SOC_GPIO")
                    .WithFlag(5, name: "SOC_SIGN")
                    .WithValueField(6, 2, name: "SOC_IDAC")
                    .WithFlag(8, name: "SOC_LPF")
                    .WithValueField(9, 3, name: "SOC_CLK")
                    .WithValueField(12, 2, name: "SOC_BIAS")
                    .WithValueField(14, 2, name: "SOC_CINT")
                },
                {(long)Registers.SocCtrl2, new WordRegister(this, 0x776A)
                    .WithValueField(0, 2, name: "SOC_RVI")
                    .WithValueField(2, 3, name: "SOC_SCYCLE")
                    .WithFlag(5, name: "SOC_DCYCLE")
                    .WithValueField(6, 2, name: "SOC_ICM")
                    .WithValueField(8, 3, name: "SOC_CHOP")
                    .WithFlag(11, name: "SOC_CMIREG_ENABLE")
                    .WithValueField(12, 3, name: "SOC_MAW")
                    .WithFlag(15, name: "SOC_DYNAVG")
                },
                {(long)Registers.SocCtrl3, new WordRegister(this, 0x11)
                    .WithValueField(0, 2, name: "SOC_VSAT")
                    .WithFlag(2, name: "SOC_DYNTARG")
                    .WithFlag(3, name: "SOC_DYNHYS")
                    .WithValueField(4, 2, name: "SOC_VCMI")
                    .WithReservedBits(6, 10)
                },
                {(long)Registers.ChargeCtr1, new WordRegister(this, 0x0)
                    .WithValueField(0, 16, name: "CHARGE_CNT1", mode: FieldMode.Read)
                },
                {(long)Registers.ChargeCtr2, new WordRegister(this, 0x0)
                    .WithValueField(0, 16, name: "CHARGE_CNT2", mode: FieldMode.Read)
                },
               {(long)Registers.ChargeCtr3, new WordRegister(this, 0x0)
                    .WithValueField(0, 8, name: "CHARGE_CNT3", mode: FieldMode.Read)
                    .WithReservedBits(8, 8)
                },
                {(long)Registers.SocChargeAvg, new WordRegister(this, 0x0)
                    .WithValueField(0, 16, name: "CHARGE_AVG", mode: FieldMode.Read)
                },
               {(long)Registers.SocStatus, new WordRegister(this, 0x0)
                    .WithFlag(0, name: "SOC_INT_OVERLOAD")
                    .WithFlag(1, name: "SOC_INT_LOCKED")
                    .WithReservedBits(2, 14)
                },
                {(long)Registers.ClkRefSel, new WordRegister(this, 0x0)
                    .WithValueField(0, 2, name: "REF_CLK_SEL")
                    .WithFlag(2, name: "REF_CAL_START")
                    .WithReservedBits(3, 13)
                },
                {(long)Registers.ClkRefCnt, new WordRegister(this, 0x0)
                    .WithValueField(0, 16, name: "REF_CNT_VAL", mode: FieldMode.Read)
                },
                {(long)Registers.ClkRefValL, new WordRegister(this, 59580)
                    .WithValueField(0, 16, name: "XTAL_CNT_VAL", mode: FieldMode.Read)
                },
                {(long)Registers.ClkRefValH, new WordRegister(this, 0x0)
                    .WithValueField(0, 16, name: "XTAL_CNT_VAL", mode: FieldMode.Read)
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

        public long Size => 0x6A;

        private readonly WordRegisterCollection registers;

        private enum Registers
        {
            AnaTest = 0x2,
            Ctrl1 = 0x8,
            Ctrl2 = 0xA,
            Status = 0xC,
            SocCtrl1 = 0x40,
            SocCtrl2 = 0x42,
            SocCtrl3 = 0x44,
            ChargeCtr1 = 0x48,
            ChargeCtr2 = 0x4A,
            ChargeCtr3 = 0x4C,
            SocChargeAvg = 0x50,
            SocStatus = 0x52,
            ClkRefSel = 0x60,
            ClkRefCnt = 0x62,
            ClkRefValL = 0x64,
            ClkRefValH = 0x66,
        }
    }
}
