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
    public sealed class DA1468x_CRG : IWordPeripheral, IKnownSize
    {
        public DA1468x_CRG(Machine machine)
        {
            Xtal16Interrupt = new GPIO();
            VbusRdyInterrupt = new GPIO();
            var registersMap = new Dictionary<long, WordRegister>
            {
                {(long)Registers.ClkAmba, new WordRegister(this, 0x22)
                    .WithValueField(0, 3, name: "HCLK_DIV")
                    .WithReservedBits(3, 1)
                    .WithValueField(4, 2, name: "PCLK_DIV")
                    .WithTaggedFlag("AES_CLK_ENABLE", 6)
                    .WithTaggedFlag("ECC_CLK_ENABLE", 7)
                    .WithTaggedFlag("TRNG_CLK_ENABLE", 8)
                    .WithFlag(9, name: "OTP_ENABLE")
                    .WithValueField(10, 2, name: "QSPI_DIV")
                    .WithFlag(12, name: "QSPI_ENABLE")
                    .WithReservedBits(13, 3)
                },
                {(long)Registers.ClkFreqTrim, new WordRegister(this, 0x0)
                    .WithValueField(0, 8, name: "FINE_ADJ")
                    .WithValueField(8, 3, name: "COARSE_ADJ")
                    .WithReservedBits(11, 5)
                },
                {(long)Registers.ClkRadio, new WordRegister(this, 0x0)
                    .WithValueField(0, 2, name: "RFCU_DIV")
                    .WithReservedBits(2, 1)
                    .WithFlag(3, name: "RFCU_ENABLE")
                    .WithValueField(4, 2, name: "BLE_DIV")
                    .WithFlag(6, name: "BLE_LP_RESET")
                    .WithFlag(7, name: "BLE_ENABLE")
                    .WithReservedBits(8, 8)
                },
                {(long)Registers.ClkCtrl, new WordRegister(this, 0x2001)
                    .WithEnumField<WordRegister, SysClkSel>(0, 2, name: "SYS_CLK_SEL",
                        writeCallback: (_, val) => ClockSelection = val,
                        valueProviderCallback: _ => ClockSelection)
                    .WithFlag(2, name: "XTAL16M_DISABLE", writeCallback: (_, val) =>
                    {
                        this.Log(LogLevel.Warning, "XTAL16M_DISABLE = {0}", val);
                        if(!val)
                        {
                            Xtal16Interrupt.Blink();
                        }
                    })
                    .WithTaggedFlag("XTAL32M_MODE", 3)
                    .WithTaggedFlag("USB_CLK_SRC", 4)
                    .WithTaggedFlag("PLL_DIV2", 5)
                    .WithTaggedFlag("DIVN_XTAL32M_MODE", 6)
                    .WithTaggedFlag("DIVN_SYNC_LEVEL", 7)
                    .WithValueField(8, 2, name: "CLK32K_SOURCE")
                    .WithReservedBits(10, 2)
                    .WithFlag(12, FieldMode.Read, name: "RUNNING_AT_32K", valueProviderCallback: _ => ClockSelection == SysClkSel.LowPower)
                    .WithFlag(13, FieldMode.Read, name: "RUNNING_AT_RC16M", valueProviderCallback: _ => ClockSelection == SysClkSel.RC16M)
                    .WithFlag(14, FieldMode.Read, name: "RUNNING_AT_XTAL16M", valueProviderCallback: _ => ClockSelection == SysClkSel.XTAL16M)
                    .WithFlag(15, FieldMode.Read, name: "RUNNING_AT_PLL96M", valueProviderCallback: _ => ClockSelection == SysClkSel.Pll96Mhz)
                },
                {(long)Registers.ClkTmr, new WordRegister(this, 0x0)
                    .WithValueField(0, 2, name: "TMR0_DIV")
                    .WithFlag(2, name: "TMR0_ENABLE")
                    .WithFlag(3, name: "TMR0_CLK_SEL")
                    .WithValueField(4, 2, name: "TMR1_DIV")
                    .WithFlag(6, name: "TMR1_ENABLE")
                    .WithFlag(7, name: "TMR1_CLK_SEL")
                    .WithValueField(8, 2, name: "TMR2_DIV")
                    .WithFlag(10, name: "TMR2_ENABLE")
                    .WithFlag(11, name: "TMR2_CLK_SEL")
                    .WithTaggedFlag("BREATH_ENABLE", 12)
                    .WithTaggedFlag("WAKEUPCT_ENABLE", 13)
                    .WithTaggedFlag("P06_TMR1_PWM_MODE", 14)
                    .WithReservedBits(15, 1)
                },
                {(long)Registers.PmuCtrl, new WordRegister(this, 0xF)
                    .WithFlag(0, name: "PERIPH_SLEEP", writeCallback: (_, val) => PeriphSleep = val, valueProviderCallback: _ => PeriphSleep)
                    .WithFlag(1, name: "RADIO_SLEEP", writeCallback: (_, val) => RadioSleep = val, valueProviderCallback: _ => RadioSleep)
                    .WithFlag(2, name: "BLE_SLEEP", writeCallback: (_, val) => BleSleep = val, valueProviderCallback: _ => BleSleep)
                    .WithReservedBits(3, 1)
                    .WithTaggedFlag("MAP_BANDGAP_EN", 4)
                    .WithFlag(5, name: "RESET_ON_WAKEUP")
                    .WithTag("OTP_COPY_DIV", 6, 2)
                    .WithValueField(8, 5, name: "RETAIN_RAM")
                    .WithFlag(13, name: "ENABLE_CLKLESS")
                    .WithFlag(14, name: "RETAIN_CACHE")
                    .WithTaggedFlag("RETAIN_ECCRAM", 15)
                },
                {(long)Registers.SysCtrl, new WordRegister(this, 0x20)
                    .WithValueField(0, 3, name: "REMAP_ADR0")
                    .WithValueField(3, 2, name: "REMAP_RAMS")
                    .WithFlag(5, name: "PAD_LATCH_EN")
                    .WithFlag(6, name: "OTPC_RESET_REQ")
                    .WithFlag(7, name: "DEBUGGER_ENABLE")
                    .WithTaggedFlag("DRA_OFF", 8)
                    .WithTaggedFlag("TIMEOUT_DISABLE", 9)
                    .WithTaggedFlag("CACHERAM_MUX", 10)
                    .WithTaggedFlag("DEV_PHASE", 11)
                    .WithFlag(12, name: "QSPI_INIT")
                    .WithTaggedFlag("OTP_COPY", 13)
                    .WithTaggedFlag("REMAP_INTVEC", 14)
                    .WithTaggedFlag("SW_RESET", 15)
                },
                {(long)Registers.SysStat, new WordRegister(this, 0x5C5)     //ReadOnly
                    .WithFlag(0, name: "RAD_IS_DOWN", valueProviderCallback: _ => RadioSleep == true)
                    .WithFlag(1, name: "RAD_IS_UP", valueProviderCallback: _ => RadioSleep == false)
                    .WithFlag(2, name: "PER_IS_DOWN", valueProviderCallback: _ => PeriphSleep == true)
                    .WithFlag(3, name: "PER_IS_UP", valueProviderCallback: _ => PeriphSleep == false)
                    .WithTaggedFlag("XTAL16_SW2", 4)
                    .WithFlag(5, name: "DBG_IS_ACTIVE")
                    .WithFlag(6, name: "XTAL16_TRIM_READY")
                    .WithFlag(7, name: "XTAL16_SETTLE_READY")
                    .WithFlag(8, name: "BLE_IS_DOWN", valueProviderCallback: _ => BleSleep == true)
                    .WithFlag(9, name: "BLE_IS_UP", valueProviderCallback: _ => BleSleep == false)
                    .WithReservedBits(10, 6)
                },
                {(long)Registers.Clk32K, new WordRegister(this, 0x7AE)
                    .WithFlag(0, name: "XTAL32K_ENABLE")
                    .WithValueField(1, 2, name: "XTAL32K_RBIAS")
                    .WithValueField(3, 4, name: "XTAL32K_CUR")
                    .WithFlag(7, name: "RC32K_ENABLE")
                    .WithValueField(8, 4, name: "RC32K_TRIM")
                    .WithFlag(12, name: "XTAL32K_DISABLE_AMPREG")
                    .WithReservedBits(13, 3)
                },
                {(long)Registers.Clk16M, new WordRegister(this, 0x54A0)
                    .WithFlag(0, name: "RC16M_ENABLE")
                    .WithValueField(1, 4, name: "RC16M_TRIM")
                    .WithValueField(5, 3, name: "XTAL16_CUR_SET")
                    .WithTaggedFlag("XTAL16_MAX_CURRENT", 8)
                    .WithTaggedFlag("XTAL16_EXT_CLK_ENABLE", 9)
                    .WithValueField(10, 3, name: "XTAL16_AMP_TRIM")
                    .WithFlag(13, name: "XTAL16_SPIKE_FLT_BYPASS")
                    .WithFlag(14, name: "XTAL16_HPASS_FLT_EN")
                    .WithReservedBits(15, 1)
                },
                {(long)Registers.ClkRCX20K, new WordRegister(this, 0x4C2)
                    .WithValueField(0, 4, name: "RCX20K_TRIM")
                    .WithValueField(4, 4, name: "RCX20K_NTC")
                    .WithValueField(8, 2, name: "RCX20K_BIAS")
                    .WithFlag(10, name: "RCX20K_LOWF")
                    .WithFlag(11, name: "RCX20K_ENABLE")
                    .WithReservedBits(12, 4)
                },
                {(long)Registers.BandGap, new WordRegister(this, 0x0)
                    .WithValueField(0, 5, name: "BGR_TRIM")
                    .WithValueField(5, 5, name: "BGR_ITRIM")
                    .WithValueField(10, 4, name: "LDO_SLEEP_TRIM")
                    .WithFlag(14, name: "LDO_SUPPLY_USE_BGREF")
                    .WithReservedBits(15, 1)
                },
                {(long)Registers.AnalogStatus, new WordRegister(this, 0x1820)
                    .WithFlag(0, name: "LDO_RADIO_OK", mode: FieldMode.Read)
                    .WithFlag(1, name: "COMP_VBAT_OK", mode: FieldMode.Read)
                    .WithFlag(2, name: "VBUS_AVAILABLE", mode: FieldMode.Read, valueProviderCallback: (_) => VbusAvailable)
                    .WithFlag(3, name: "NEWBAT", mode: FieldMode.Read)
                    .WithFlag(4, name: "LDO_SUPPLY_VBAT_OK", mode: FieldMode.Read)   
                    .WithFlag(5, name: "LDO_SUPPLY_USB_OK", mode: FieldMode.Read)  
                    .WithFlag(6, name: "BANDGAP_OK", mode: FieldMode.Read)
                    .WithFlag(7, name: "COMP_VDD_HIGH", mode: FieldMode.Read)
                    .WithFlag(8, name: "LCO_CORE_OK", mode: FieldMode.Read)
                    .WithFlag(9, name: "LDO_1V8_PA_OK", mode: FieldMode.Read)
                    .WithFlag(10, name: "LDO_1V8_FLASH_OK", mode: FieldMode.Read)
                    .WithFlag(11, name: "COMP_VBUS_HIGH", mode: FieldMode.Read) 
                    .WithFlag(12, name: "COMP_VBUS_LOW", mode: FieldMode.Read) 
                    .WithFlag(13, name: "COMP_V33_HIGH", mode: FieldMode.Read)
                    .WithFlag(14, name: "COMP_1V8_FLASH_HIGH", mode: FieldMode.Read)
                    .WithFlag(15, name: "COMP_1V8_PA_HIGH", mode: FieldMode.Read)
                },
                {(long)Registers.VbusIrqMask, new WordRegister(this, 0x3)
                    .WithFlag(0, name: "VBUS_IRQ_EN_FALL")
                    .WithFlag(1, name: "VBUS_IRQ_EN_RISE")
                    .WithReservedBits(2, 14)
                },
                {(long)Registers.VbusIrqClear, new WordRegister(this, 0x0)
                    .WithFlag(0, name: "VBUS_IRQ_CLEAR")
                    .WithReservedBits(1, 15)
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
                    .WithFlag(0, name: "BOD_RESET_EN")
                    .WithFlag(1, name: "BOD_VDD_EN")
                    .WithTaggedFlag("BOD_V33_EN", 2)
                    .WithTaggedFlag("BOD_1V8_PA_EN", 3)
                    .WithFlag(4, name: "BOD_1V8_FLASH_EN")
                    .WithFlag(5, name: "BOD_VBAT_EN")
                    .WithTaggedFlag("BOD_V14_EN", 6)
                    .WithReservedBits(7, 9)
                },
                {(long)Registers.LdoCtrl1, new WordRegister(this, 0xA7)
                    .WithValueField(0, 2, name: "LDO_CORE_CURLIM")
                    .WithValueField(2, 2, name: "LDO_VBAT_RET_LEVEL")
                    .WithValueField(4, 2, name: "LDO_SUPPLY_VBAT_LEVEL")
                    .WithValueField(6, 2, name: "LDO_SUPPLY_USB_LEVEL")
                    .WithValueField(8, 3, name: "LDO_CORE_SETVDD")
                    .WithValueField(11, 3, name: "LDO_RADIO_SETVDD")
                    .WithFlag(14,name: "LDO_RADIO_ENABLE")
                    .WithReservedBits(15, 1)
                },
                {(long)Registers.LdoCtrl2, new WordRegister(this, 0xF)
                    .WithFlag(0, name: "LDO_1V2_ON")
                    .WithFlag(1, name: "LDO_3V3_ON")
                    .WithFlag(2, name: "LDO_1V8_FLASH_ON")
                    .WithFlag(3, name: "LDO_1V8_PA_ON")
                    .WithFlag(4, name: "LDO_VBAT_RET_DISABLE")
                    .WithFlag(5, name: "LDO_1V8_FLASH_RET_DISABLE")
                    .WithFlag(6, name: "LDO_1V8_PA_RET_DISABLE")
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
                    .WithFlag(3, name: "XTAL16M_AMP_REG_SIG_SEL")
                    .WithValueField(4, 2, name: "XTAL16M_TST_AON")
                    .WithValueField(6, 2, name: "XTAL16M_SH_OVERRULE")
                    .WithFlag(8, name: "XTAL16M_ENABLE_ZERO")
                    .WithReservedBits(9, 7)
                },
                {(long)Registers.AonSpare, new WordRegister(this, 0x0)                   
                    .WithFlag(0, name: "OSC16_HOLD_AMP")
                    .WithTaggedFlag("OSC16_SH_DISABLE", 1)
                    .WithTaggedFlag("EN_BATSYS_RET", 2)
                    .WithTaggedFlag("EN_BUSSYS_RET", 3)
                    .WithReservedBits(4, 12)
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

        public void SetVbusRdy(bool rdy)
        {
            VbusAvailable = rdy;
            if(rdy)
            {
                VbusRdyInterrupt.Blink();
            }
        }

        public void Reset()
        {
            Xtal16Interrupt.Unset();
            VbusRdyInterrupt.Unset();
            VbusAvailable = false;
            registers.Reset();
        }

        public GPIO Xtal16Interrupt { get; }

        public GPIO VbusRdyInterrupt { get; }

        public long Size => 0x6B;

        private readonly WordRegisterCollection registers;

        private SysClkSel ClockSelection { get; set; }

        private bool BleSleep = true;
        private bool RadioSleep = true;
        private bool PeriphSleep = true;
        private bool VbusAvailable = false;

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
            ClkRadio = 0x8,
            ClkCtrl = 0xA,
            ClkTmr = 0xC,
            PmuCtrl = 0x10,
            SysCtrl = 0x12,
            SysStat = 0x14,
            Clk32K = 0x20,
            Clk16M = 0x22,
            ClkRCX20K = 0x24,
            BandGap = 0x28,
            AnalogStatus = 0x2A,
            VbusIrqMask = 0x30,
            VbusIrqClear = 0x32,
            BodCtrl = 0x34,
            BodCtrl2 = 0x36,
            LdoCtrl1 = 0x3A,
            LdoCtrl2 = 0x3C,
            SleepTimer = 0x3E,
            XtalRdyCtrl = 0x50,
            XtalRdyStat = 0x52,
            Xtal16MCtrl = 0x56,
            AonSpare = 0x64,
        }
    }
}
