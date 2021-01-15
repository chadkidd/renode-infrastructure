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
    public sealed class DA1468x_CRG : IWordPeripheral, IKnownSize
    {
        public DA1468x_CRG(Machine machine)
        {
            var registersMap = new Dictionary<long, WordRegister>
            {
                {(long)Registers.ClkAmba, new WordRegister(this, 0x22)
                    .WithValueField(0, 3, name: "HCLK_DIV")
                    .WithReservedBits(3, 1)
                    .WithValueField(4, 2, name: "PCLK_DIV")
                    .WithTag("AES_CLK_ENABLE", 6, 1)
                    .WithTag("ECC_CLK_ENABLE", 7, 1)
                    .WithTag("TRNG_CLK_ENABLE", 8, 1)
                    .WithValueField(9, 1, name: "OTP_ENABLE")
                    .WithValueField(10, 2, name: "QSPI_DIV")
                    .WithTag("QSPI_ENABLE", 12, 1)
                    .WithReservedBits(13, 3)
                },
                {(long)Registers.ClkCtrl, new WordRegister(this, 0x2001)
                    .WithEnumField<WordRegister, SysClkSel>(0, 2, name: "SYS_CLK_SEL",
                        writeCallback: (_, val) => ClockSelection = val,
                        valueProviderCallback: _ => ClockSelection)
                    .WithValueField(2, 1, name: "XTAL16M_DISABLE")
                    .WithTag("XTAL32M_MODE", 3, 1)
                    .WithTag("USB_CLK_SRC", 4, 1)
                    .WithTag("PLL_DIV2", 5, 1)
                    .WithTag("DIVN_XTAL32M_MODE", 6, 1)
                    .WithReservedBits(7, 1)
                    .WithTag("CLK32K_SOURCE", 8, 2)
                    .WithReservedBits(10, 2)
                    .WithValueField(12, 1, FieldMode.Read, name: "RUNNING_AT_32K", valueProviderCallback: _ => { return (uint) (ClockSelection == SysClkSel.LowPower ? 1 : 0); })
                    .WithValueField(13, 1, FieldMode.Read, name: "RUNNING_AT_RC16M", valueProviderCallback: _ => { return (uint) (ClockSelection == SysClkSel.RC16M ? 1 : 0); })
                    .WithValueField(14, 1, FieldMode.Read, name: "RUNNING_AT_XTAL16M", valueProviderCallback: _ => { return (uint) (ClockSelection == SysClkSel.XTAL16M ? 1 : 0); })
                    .WithValueField(15, 1, FieldMode.Read, name: "RUNNING_AT_PLL96M", valueProviderCallback: _ => { return (uint) (ClockSelection == SysClkSel.Pll96Mhz ? 1 : 0); })
                },
                {(long)Registers.PmuCtrl, new WordRegister(this, 0xF)
                    .WithValueField(0, 1, name: "PERIPH_SLEEP")
                    .WithValueField(1, 1, name: "RADIO_SLEEP")
                    .WithValueField(2, 1, name: "BLE_SLEEP")
                    .WithReservedBits(3, 1)
                    .WithTag("MAP_BANDGAP_EN", 4, 1)
                    .WithTag("RESET_ON_WAKEUP", 5, 1)
                    .WithTag("OTP_COPY_DIV", 6, 2)
                    .WithTag("RETAIN_RAM", 8, 5)
                    .WithTag("ENABLE_CLKLESS", 13, 1)
                    .WithTag("RETAIN_CACHE", 14, 1)
                    .WithTag("RETAIN_ECCRAM", 15, 1)
                },
                {(long)Registers.SysCtrl, new WordRegister(this, 0x20)
                    .WithValueField(0, 3, name: "REMAP_ADR0")
                    .WithValueField(3, 2, name: "REMAP_RAMS")
                    .WithValueField(5, 1, name: "PAD_LATCH_EN")
                    .WithValueField(6, 1, name: "OTPC_RESET_REQ")
                    .WithValueField(7, 1, name: "DEBUGGER_ENABLE")
                    .WithReservedBits(8, 1)
                    .WithTag("TIMEOUT_DISABLE", 9, 1)
                    .WithTag("CACHERAM_MUX", 10, 1)
                    .WithTag("DEV_PHASE", 11, 1)
                    .WithTag("QSPI_INIT", 12, 1)
                    .WithTag("OTP_COPY", 13, 1)
                    .WithTag("REMAP_INTVEC", 14, 1)
                    .WithTag("SW_RESET", 15, 1)
                },
                {(long)Registers.BodCtrl, new WordRegister(this, 0x700)
                    .WithReservedBits(0, 7)
                    .WithValueField(8, 3, name: "BOD_VDD_LVL")                
                    .WithReservedBits(11, 5)
                },
                {(long)Registers.BodCtrl2, new WordRegister(this, 0x3)
                    .WithValueField(0, 1, name: "BOD_RESET_EN")
                    .WithValueField(1, 1, name: "BOD_VDD_EN")
                    .WithTag("BOD_V33_EN", 2, 1)
                    .WithTag("BOD_1V8_PA_EN", 3, 1)
                    .WithTag("BOD_1V8_FLASH_EN", 4, 1)
                    .WithValueField(5, 1, name: "BOD_VBAT_EN")
                    .WithTag("BOD_V14_EN", 6, 1)
                    .WithReservedBits(7, 9)
                },
                {(long)Registers.LdoCtrl1, new WordRegister(this, 0xA7)
                    .WithValueField(0, 2, name: "LDO_CORE_CURLIM")
                    .WithValueField(2, 2, name: "LDO_VBAT_RET_LEVEL")
                    .WithValueField(4, 2, name: "LDO_SUPPLY_VBAT_LEVEL")
                    .WithValueField(6, 2, name: "LDO_SUPPLY_USB_LEVEL")
                    .WithValueField(8, 3, name: "LDO_CORE_SETVDD")
                    .WithValueField(11, 3, name: "LDO_RADIO_SETVDD")
                    .WithValueField(14, 1, name: "LDO_RADIO_ENABLE")
                    .WithReservedBits(15, 1)
                },
                {(long)Registers.LdoCtrl2, new WordRegister(this, 0xF)
                    .WithValueField(0, 1, name: "LDO_1V2_ON")
                    .WithValueField(1, 1, name: "LDO_3V3_ON")
                    .WithValueField(2, 1, name: "LDO_1V8_FLASH_ON")
                    .WithValueField(3, 1, name: "LDO_1V8_PA_ON")
                    .WithValueField(4, 1, name: "LDO_VBAT_RET_DISABLE")
                    .WithValueField(5, 1, name: "LDO_1V8_FLASH_RET_DISABLE")
                    .WithValueField(6, 1, name: "LDO_1V8_PA_RET_DISABLE")
                    .WithReservedBits(7, 9)
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

        public long Size => 0x6B;

        private readonly WordRegisterCollection registers;

        private SysClkSel ClockSelection { get; set; }

        private enum SysClkSel
        {
            XTAL16M = 0,
            RC16M = 1,
            LowPower = 2,
            Pll96Mhz = 3
        }

        private enum Registers
        {
            ClkAmba = 0x0,
            ClkCtrl = 0xA,
            //ClkTmr = 0xC,
            PmuCtrl = 0x10,
            SysCtrl = 0x12,
            BodCtrl = 0x34,
            BodCtrl2 = 0x36,
            LdoCtrl1 = 0x3A,
            LdoCtrl2 = 0x3C,
        }
    }
}
