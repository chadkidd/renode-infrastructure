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

namespace Antmicro.Renode.Peripherals.USB
{
    [AllowedTranslations(AllowedTranslation.ByteToWord | AllowedTranslation.DoubleWordToWord)]
    public sealed class DA1468x_USB : IWordPeripheral, IKnownSize
    {
        public DA1468x_USB(Machine machine)
        {
            var registersMap = new Dictionary<long, WordRegister>
            {
                {(long)Registers.MainCtrl, new WordRegister(this, 0x0)
                    .WithFlag(0, name: "USBEN")
                    .WithFlag(1, name: "USB_DBG")
                    .WithReservedBits(2, 1)
                    .WithFlag(3, name: "USB_NAT")
                    .WithFlag(4, name: "LSMODE")
                    .WithReservedBits(5, 11)
                },
                 {(long)Registers.MainMask, new WordRegister(this, 0x0)
                    .WithFlag(0, name: "USB_M_WARN")
                    .WithFlag(1, name: "USB_M_ALT")
                    .WithFlag(2, name: "USB_M_TX_EV")
                    .WithFlag(3, name: "USB_M_FRAME")
                    .WithFlag(4, name: "USB_M_NAK")
                    .WithFlag(5, name: "USB_M_ULD")
                    .WithFlag(6, name: "USB_M_RX_EV")
                    .WithFlag(7, name: "USB_M_INTR")
                    .WithFlag(8, name: "USB_M_EP0_TX")
                    .WithFlag(9, name: "USB_M_EP0_RX")
                    .WithFlag(10, name: "USB_M_EP0_NAK")
                    .WithFlag(11, name: "USB_M_CH_EV")

                    .WithReservedBits(12, 4)
                },
                {(long)Registers.ChargerCtrl, new WordRegister(this, 0x0)
                    .WithFlag(0, name: "USB_CHARGE_ON")
                    .WithFlag(1, name: "IDP_SRC_ON")
                    .WithFlag(2, name: "VDP_SRC_ON")
                    .WithFlag(3, name: "VDM_SRC_ON")
                    .WithFlag(4, name: "IDP_SNK_ON")
                    .WithFlag(5, name: "IDM_SNK_ON")
                    .WithReservedBits(6, 10)
                },
                {(long)Registers.ChargerStat, new WordRegister(this, 0x0)
                    .WithFlag(0, name: "USB_DCP_DET")
                    .WithFlag(1, name: "USB_CHG_DET")
                    .WithFlag(2, name: "USB_DP_VAL")
                    .WithFlag(3, name: "USB_DM_VAL")
                    .WithFlag(4, name: "USB_DP_VAL2")
                    .WithFlag(5, name: "USB_DM_VAL2")
                    .WithReservedBits(6, 10)
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

        public long Size => 0xD8;

        private readonly WordRegisterCollection registers;

        private enum Registers
        {
            MainCtrl = 0x0,
            MainMask = 0xE,
            ChargerCtrl = 0xD4,
            ChargerStat = 0xD6,
        }
    }
}
