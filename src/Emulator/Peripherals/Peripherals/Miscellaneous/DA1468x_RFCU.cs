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
    [AllowedTranslations(AllowedTranslation.ByteToWord | AllowedTranslation.WordToDoubleWord | AllowedTranslation.ByteToDoubleWord)]
    public sealed class DA1468x_RFCU : IDoubleWordPeripheral, IKnownSize
    {
        public DA1468x_RFCU(Machine machine)
        {
            var registersMap = new Dictionary<long, DoubleWordRegister>
            {
                {(long)Registers.RfCalCtrl, new DoubleWordRegister(this, 0x0)
                    .WithFlag(0, name: "SO_CAL",
                        valueProviderCallback: (_) => {
                            startOfCal = !startOfCal;
                            return !startOfCal;  
                        },
                        writeCallback: (_, value) => {
                            startOfCal = value;
                        })
                    .WithFlag(1, name: "EO_CAL")
                    .WithTaggedFlag("MGAIN_CAL_DIS", 2)
                    .WithTaggedFlag("IFF_CAL_DIS", 3)
                    .WithTaggedFlag("DC_OFFSET_CAL_DIS", 4)
                    .WithTaggedFlag("VCO_CAL_DIS", 5)
                    .WithReservedBits(6, 10)
                },
                
            };

            registers = new DoubleWordRegisterCollection(this, registersMap);
        }

        public uint ReadDoubleWord(long offset)
        {
            return registers.Read(offset);
        }

        public void WriteDoubleWord(long offset, uint value)
        {
            registers.Write(offset, value);
        }

        public void Reset()
        {
            registers.Reset();
        }

        public long Size => 0xF0;

        private readonly DoubleWordRegisterCollection registers;

        private bool startOfCal;

        private enum Registers
        {
            RfCalCtrl= 0x50,
        }
    }
}
