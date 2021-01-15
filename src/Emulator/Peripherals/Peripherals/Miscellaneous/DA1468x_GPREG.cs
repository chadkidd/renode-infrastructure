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
                {(long)Registers.ClkFreqTrim, new WordRegister(this, 0x0)
                    .WithValueField(0, 7, name: "FINE_ADJ")
                    .WithValueField(8, 3, name: "COARSE_ADJ")
                    .WithReservedBits(11, 5)
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
                    .WithTag("DIVN_SYNC_LEVEL", 7, 1)
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
                    .WithValueField(8, 5, name: "RETAIN_RAM")
                    .WithTag("ENABLE_CLKLESS", 13, 1)
                    .WithValueField(14, 1, name: "RETAIN_CACHE")
                    .WithTag("RETAIN_ECCRAM", 15, 1)
                },
                {(long)Registers.SysCtrl, new WordRegister(this, 0x20)
                    .WithValueField(0, 3, name: "REMAP_ADR0")
                    .WithValueField(3, 2, name: "REMAP_RAMS")
                    .WithValueField(5, 1, name: "PAD_LATCH_EN")
                    .WithValueField(6, 1, name: "OTPC_RESET_REQ")
                    .WithValueField(7, 1, name: "DEBUGGER_ENABLE")
                    .WithTag("DRA_OFF", 8, 1)
                    .WithTag("TIMEOUT_DISABLE", 9, 1)
                    .WithTag("CACHERAM_MUX", 10, 1)
                    .WithTag("DEV_PHASE", 11, 1)
                    .WithTag("QSPI_INIT", 12, 1)
                    .WithTag("OTP_COPY", 13, 1)
                    .WithTag("REMAP_INTVEC", 14, 1)
                    .WithTag("SW_RESET", 15, 1)
                },
                {(long)Registers.SysStat, new WordRegister(this, 0x5C5)     //ReadOnly
                    .WithValueField(0, 1, name: "RAD_IS_DOWN")
                    .WithValueField(1, 1, name: "RAD_IS_UP")
                    .WithValueField(2, 1, name: "PER_IS_DOWN")
                    .WithValueField(3, 1, name: "PER_IS_UP")
                    .WithTag("XTAL16_SW2", 4, 1)
                    .WithValueField(5, 1, name: "DBG_IS_ACTIVE")
                    .WithValueField(6, 1, name: "XTAL16_TRIM_READY")
                    .WithValueField(7, 1, name: "XTAL16_SETTLE_READY")
                    .WithValueField(8, 1, name: "BLE_IS_DOWN")
                    .WithValueField(9, 1, name: "BLE_IS_UP")
                    .WithReservedBits(10, 6)
                },
                {(long)Registers.Clk32K, new WordRegister(this, 0x7AE)
                    .WithValueField(0, 1, name: "XTAL32K_ENABLE")
                    .WithValueField(1, 2, name: "XTAL32K_RBIAS")
                    .WithValueField(3, 4, name: "XTAL32K_CUR")
                    .WithValueField(7, 1, name: "RC32K_ENABLE")
                    .WithValueField(8, 4, name: "RC32K_TRIM")
                    .WithValueField(12, 1, name: "XTAL32K_DISABLE_AMPREG")
                    .WithReservedBits(13, 3)
                },
                {(long)Registers.Clk16M, new WordRegister(this, 0x54A0)
                    .WithValueField(0, 1, name: "RC16M_ENABLE")
                    .WithValueField(1, 4, name: "RC16M_TRIM")
                    .WithValueField(5, 3, name: "XTAL16_CUR_SET")
                    .WithTag("XTAL16_MAX_CURRENT", 8, 1)
                    .WithTag("XTAL16_EXT_CLK_ENABLE", 9, 1)
                    .WithValueField(10, 3, name: "XTAL16_AMP_TRIM")
                    .WithValueField(13, 1, name: "XTAL16_SPIKE_FLT_BYPASS")
                    .WithValueField(14, 1, name: "XTAL16_HPASS_FLT_EN")
                    .WithReservedBits(15, 1)
                },
                {(long)Registers.BandGap, new WordRegister(this, 0x0)
                    .WithValueField(0, 5, name: "BGR_TRIM")
                    .WithValueField(5, 5, name: "BGR_ITRIM")
                    .WithValueField(10, 4, name: "LDO_SLEEP_TRIM")
                    .WithValueField(14, 1, name: "LDO_SUPPLY_USE_BGREF")
                    .WithReservedBits(15, 1)
                },
                {(long)Registers.BodCtrl, new WordRegister(this, 0x700)
                    .WithTag("VDD_TRIM", 0, 2)
                    .WithTag("1V8_TRIM", 2, 2)
                    .WithTag("1V4_TRIM", 4, 2)
                    .WithTag("V33_TRIM", 6, 2)
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
                {(long)Registers.SleepTimer, new WordRegister(this, 0x0)
                    .WithValueField(0, 16, name: "SLEEP_TIMER")
                },
                {(long)Registers.XtalRdyCtrl, new WordRegister(this, 0x0)
                    .WithValueField(0, 8, name: "XTALRDY_CNT")
                    .WithReservedBits(8, 8)
                },
                {(long)Registers.XtalRdyStat, new WordRegister(this, 0x0)
                    .WithTag("XTALRDY_STAT", 0, 8)
                    .WithReservedBits(8, 8)
                },
                {(long)Registers.Xtal16MCtrl, new WordRegister(this, 0x0)
                    .WithValueField(0, 3, name: "XTAL16M_FREQ_TRIM_SW2")
                    .WithValueField(3, 1, name: "XTAL16M_AMP_REG_SIG_SEL")
                    .WithValueField(4, 2, name: "XTAL16M_TST_AON")
                    .WithValueField(6, 2, name: "XTAL16M_SH_OVERRULE")
                    .WithValueField(8, 1, name: "XTAL16M_ENABLE_ZERO")
                    .WithReservedBits(9, 7)
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
            ClkFreqTrim = 0x2,
            ClkCtrl = 0xA,
            //ClkTmr = 0xC,
            PmuCtrl = 0x10,
            SysCtrl = 0x12,
            SysStat = 0x14,
            Clk32K = 0x20,
            Clk16M = 0x22,
            BandGap = 0x28,
            BodCtrl = 0x34,
            BodCtrl2 = 0x36,
            LdoCtrl1 = 0x3A,
            LdoCtrl2 = 0x3C,
            SleepTimer = 0x3E,
            XtalRdyCtrl = 0x50,
            XtalRdyStat = 0x52,
            Xtal16MCtrl = 0x56,
        }
    }
}
