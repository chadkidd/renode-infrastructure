//
// Copyright (c) 2010-2020 Antmicro
//
//  This file is licensed under the MIT License.
//  Full license text is available in 'licenses/MIT.txt'.
//
using Antmicro.Renode.Core;
using Antmicro.Renode.Core.Structure.Registers;
using Antmicro.Renode.Time;
using Antmicro.Renode.Peripherals.Bus;
using Antmicro.Renode.Logging;

namespace Antmicro.Renode.Peripherals.Timers
{
    [AllowedTranslations(AllowedTranslation.ByteToWord)]
    public class DA14681_Timer1 : BasicWordPeripheral, IKnownSize
    {
        public DA14681_Timer1(Machine machine) : base(machine)
        {
            /*  The DA14681 timer is a 16-bit timer.
             *  
             *  Notes: Chad Kidd - 2/19/2021
             *  Still TODO if required at a later date
             *  1. Divider registers in the DA1468x_CRG -> ClkCtrl(0xA) - if set will affect the divider here. 
             *     Still Need to figure out how to pass those values between peripherals
             *  2. There are freeze registers in DA1468x_GPREG that if selected need to stop this timer
             *  3. Many of the registers and functions for Timer1 have not been implemented unless required later
             *      i.e. - OneShot / Capture / GPIO1/2 functions / PWM
             * 
             * */
            IRQ = new GPIO();
            innerTimer = new ComparingTimer(machine.ClockSource, SystemClockFrequency, this, $"compareTimer", direction: Direction.Ascending, limit: initialLimit, compare: initialLimit, eventEnabled: true);
            innerTimer.CompareReached += CompareReached;
        }

        public override void Reset()
        {
            prescaler = 0;
            innerTimer.Reset();
            reloadValue = initialLimit;
            innerTimer.Compare = initialLimit;
            interruptEnabled.Value = false;
            base.Reset();
            IRQ.Unset();
        }

        public void Pause(bool paused)
        {
            innerTimer.Enabled = !paused;
            innerTimer.EventEnabled = !paused;
            if(paused)
            {
                IRQ.Unset();
            }
        }

        public GPIO IRQ { get; }

        public long Size => 0x24;

        protected override void DefineRegisters()
        {
            Registers.Control.Define(this, 0x0)
                .WithFlag(0, name: "CAPTIM_EN", writeCallback: (_, value) =>
                {
                    innerTimer.Enabled = value;
                    this.Log(LogLevel.Noisy, "Timer enabled = {0}", innerTimer.Enabled);
                })
                .WithTaggedFlag("CAPTIM_ONESHOT_MODE_EN", 1)
                .WithFlag(2, name: "CAPTIM_COUNT_DOWN_EN", writeCallback: (_, value) =>
                {
                    if(value)
                    {
                        if(innerTimer.Direction != Direction.Descending) innerTimer.Direction = Direction.Descending;

                    }
                    else
                    {
                        if(innerTimer.Direction != Direction.Ascending) innerTimer.Direction = Direction.Ascending;
                    }
                },
                    valueProviderCallback: _ => innerTimer.Direction == Direction.Descending)
                .WithTaggedFlag("CAPTIM_IN1_EVENT_FALL_EN", 3)
                .WithTaggedFlag("CAPTIM_IN2_EVENT_FALL_EN", 4)
                .WithFlag(5, out interruptEnabled, name: "CAPTIM_IRQ_EN", writeCallback: (_, value) => IRQ.Unset())
                .WithFlag(6, out freeRunMode, name: "CAPTIM_FREE_RUN_MODE_EN")
                .WithFlag(7, name: "CAPTIM_SYS_CLK_EN", writeCallback: (_, value) =>
                {
                    if(value)
                    {
                        if(innerTimer.BaseFrequency != SystemClockFrequency) innerTimer.BaseFrequency = SystemClockFrequency;
                    }
                    else
                    {
                        if(innerTimer.BaseFrequency != LowPowerClockFrequency) innerTimer.BaseFrequency = LowPowerClockFrequency;
                    }
                    this.Log(LogLevel.Noisy, "Timer Frequency set to {0}", innerTimer.BaseFrequency);
                })
                .WithReservedBits(8, 8)
            ;
            ;
            Registers.TimerValue.Define(this, 0x0)
                .WithValueField(0, 16, name: "CAPTIM_TIMER_VAL", mode: FieldMode.Read,
                    valueProviderCallback: (_) => {
                        this.Log(LogLevel.Noisy, "Timer Value has been read {0}", innerTimer.Value);
                        return (ushort)innerTimer.Value;
                    })
            ;
            Registers.Reload.Define(this, 0x0)
                .WithValueField(0, 16, name: "CAPTIM_RELOAD",
                    valueProviderCallback: (_) => (ushort)innerTimer.Compare,
                    writeCallback: (_, value) =>
                    {
                        reloadValue = value;
                        innerTimer.Compare = reloadValue;
                        if(innerTimer.Direction == Direction.Ascending && !freeRunMode.Value)
                        {
                            innerTimer.Value = 0;
                        }
                        else if(innerTimer.Direction == Direction.Descending)
                        {
                            innerTimer.Value = reloadValue;
                        }
                        this.Log(LogLevel.Noisy, "Reload Register write: Timer is at {0}, prescaler is {1}, reloadValue is {2}, Direction is {3}",
                            innerTimer.Value, prescaler, reloadValue, innerTimer.Direction);
                    })
            ;
            Registers.Prescaler.Define(this, 0x0)
                .WithValueField(0, 16, name: "CAPTIM_PRESCALER",
                    valueProviderCallback: (_) => prescaler,
                    writeCallback: (_, value) =>
                    {
                        prescaler = (ushort)value;
                        innerTimer.Divider = value + 1;
                        this.Log(LogLevel.Noisy, "Timer Divider set to {0}", innerTimer.Divider);
                    })
            ;
            Registers.PrescalerValue.Define(this, 0x0)
                .WithValueField(0, 16, mode: FieldMode.Read, name: "CAPTIM_PRESCALER_VAL",
                    valueProviderCallback: (_) => prescaler)
            ;
        }

        private void CompareReached()
        {
            this.Log(LogLevel.Noisy, "Compare reached, setting IRQ: {0}", interruptEnabled.Value);
            IRQ.Set(interruptEnabled.Value);
        }

        private IFlagRegisterField interruptEnabled;
        private IFlagRegisterField freeRunMode;
        private ushort prescaler;

        private ComparingTimer innerTimer;
        private const uint initialLimit = 0xFFFF;
        private uint reloadValue = 0;
        private const int SystemClockFrequency = 16000000;
        private const int LowPowerClockFrequency = 32768;

        private enum Registers : long
        {
            Control = 0x0,
            TimerValue = 0x2,
            Status = 0x4,
            Reload = 0xA,
            Prescaler = 0xE,
            PrescalerValue = 0x14,
        }
    }
}