//
// Copyright (c) 2010-2019 Antmicro
//
// This file is licensed under the MIT License.
// Full license text is available in 'licenses/MIT.txt'.
//
using System;
using Antmicro.Renode.Core;
using Antmicro.Renode.Peripherals.Bus;
using Antmicro.Renode.Core.Structure.Registers;
using Antmicro.Renode.Time;
using Antmicro.Renode.Logging;

namespace Antmicro.Renode.Peripherals.Timers
{
    public class DA1468x_Watchdog : BasicWordPeripheral, IKnownSize
    {
        public DA1468x_Watchdog(Machine machine, long frequency) : base(machine)
        {
            internalTimer = new LimitTimer(machine.ClockSource, frequency, this, "DA1468x_Watchdog", TimerLimit, direction: Direction.Descending, enabled: false, workMode: WorkMode.OneShot, eventEnabled: true);
            internalTimer.LimitReached += TimerLimitReached;
        }

        /* CKK - This file is a work in progress - nothing timer related has really been implemented yet
         * Just register placeholders for now
         */
        //TODO - implement the actual timer and reset
        public override void Reset()
        {
            base.Reset();
            internalTimer.Reset();
        }

        public long Size => 0x4;

        protected override void DefineRegisters()
        {

            Registers.Watchdog.Define(this, TimeDefault)
                .WithValueField(0, 8, name: "WDOG_VAL")
                .WithFlag(8, name: "WDOG_VAL_NEG")
                .WithValueField(9, 7, name: "WDOG_WEN")
            ;

            Registers.WatchdogCtrl.Define(this, 0)
                .WithFlag(0, name: "NMI_RST")
                .WithReservedBits(1, 15)
            ;
        }

        private void TimerLimitReached()
        {
            TriggerReset();
        }

        private void TriggerReset()
        {
            this.Log(LogLevel.Warning, "Watchdog reset triggered!");
            //machine.RequestReset();
        }

        private uint GetCurrentTimerValue()
        {
            return (uint)internalTimer.Value + triggerValue.Value;
        }


        private LimitTimer internalTimer;
        private IValueRegisterField triggerValue;

        private const uint TimerLimit = 0xFF;
        private const ushort TimeDefault = 0xFF;

        private enum Registers
        {
            Watchdog = 0x0,
            WatchdogCtrl = 0x2,
        }
    }
}
