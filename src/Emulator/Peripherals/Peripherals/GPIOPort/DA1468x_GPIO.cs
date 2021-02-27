//
// Copyright (c) 2010-2020 Antmicro
//
// This file is licensed under the MIT License.
// Full license text is available in 'licenses/MIT.txt'.

using System;
using System.Linq;
using Antmicro.Renode.Core;
using Antmicro.Renode.Peripherals.Bus;
using Antmicro.Renode.Core.Structure.Registers;
using Antmicro.Renode.Logging;
using System.ComponentModel;

namespace Antmicro.Renode.Peripherals.GPIOPort
{
    public class DA1468x_GPIO : BaseGPIOPort, IProvidesRegisterCollection<WordRegisterCollection>, IWordPeripheral, IKnownSize
    {
        public DA1468x_GPIO(Machine machine) : base(machine, NumberOfPins)
        {
            Pins = new Pin[NumberOfPins];
            for(var i = 0; i < Pins.Length; i++)
            {
                Pins[i] = new Pin(this, i);
            }

            RegistersCollection = new WordRegisterCollection(this);
            DefineRegisters();
        }

        public override void Reset()
        {
            base.Reset();

            RegistersCollection.Reset();

            foreach(var pin in Pins)
            {
                pin.Reset();
            }
        }

        public ushort ReadWord(long offset)
        {
            return RegistersCollection.Read(offset);
        }

        public void WriteWord(long offset, ushort value)
        {
            RegistersCollection.Write(offset, value);
        }

        public override void OnGPIO(int number, bool value)
        {
            base.OnGPIO(number, value);
            if(CheckPinNumber(number))
            {
                PinChanged?.Invoke(Pins[number], value);
            }
        }

        public WordRegisterCollection RegistersCollection { get; }

        public long Size => 0xD2;

        public event Action<Pin, bool> PinChanged;

        private void DefineRegisters()
        {
            Registers.P0_Data.Define(this, 0x20)
                .WithFlags(0, 8,
                    valueProviderCallback: (id, _) => Pins[id + P0_PIN_OFFSET].Value,
                    writeCallback: (id, _, val) => Pins[id + P0_PIN_OFFSET].Value = val)
                .WithReservedBits(8, 8)
            ;
            Registers.P1_Data.Define(this, 0x60)
                .WithFlags(0, 8,
                    valueProviderCallback: (id, _) => Pins[id + P1_PIN_OFFSET].Value,
                    writeCallback: (id, _, val) => Pins[id + P1_PIN_OFFSET].Value = val)
                .WithReservedBits(8, 8)
            ;
            Registers.P2_Data.Define(this, 0x0)
                .WithFlags(0, 5,
                    valueProviderCallback: (id, _) => Pins[id + P2_PIN_OFFSET].Value,
                    writeCallback: (id, _, val) => Pins[id + P2_PIN_OFFSET].Value = val)
                .WithReservedBits(5, 11)
            ;
            Registers.P3_Data.Define(this, 0x0)
                .WithFlags(0, 8,
                    valueProviderCallback: (id, _) => Pins[id + P3_PIN_OFFSET].Value,
                    writeCallback: (id, _, val) => Pins[id + P3_PIN_OFFSET].Value = val)
                .WithReservedBits(8, 8)
            ;
            Registers.P4_Data.Define(this, 0x0)
                .WithFlags(0, 8,
                    valueProviderCallback: (id, _) => Pins[id + P4_PIN_OFFSET].Value,
                    writeCallback: (id, _, val) => Pins[id + P4_PIN_OFFSET].Value = val)
                .WithReservedBits(8, 8)
            ;
            Registers.P0_SetData.Define(this, 0x0)
                .WithFlags(0, 8,
                    valueProviderCallback: (id, _) => false,
                    writeCallback: (id, _, val) => { if(val) Pins[id + P0_PIN_OFFSET].Value = true; })
                .WithReservedBits(8, 8)
            ;
            Registers.P1_SetData.Define(this, 0x0)
                .WithFlags(0, 8,
                    valueProviderCallback: (id, _) => false,
                    writeCallback: (id, _, val) => { if(val) Pins[id + P1_PIN_OFFSET].Value = true; })
                .WithReservedBits(8, 8)
            ;
            Registers.P2_SetData.Define(this, 0x0)
                .WithFlags(0, 5,
                    valueProviderCallback: (id, _) => false,
                    writeCallback: (id, _, val) => { if(val) Pins[id + P2_PIN_OFFSET].Value = true; })
                .WithReservedBits(5, 11)
            ;
            Registers.P3_SetData.Define(this, 0x0)
                .WithFlags(0, 8,
                    valueProviderCallback: (id, _) => false,
                    writeCallback: (id, _, val) => { if(val) Pins[id + P3_PIN_OFFSET].Value = true; })
                .WithReservedBits(8, 8)
            ;
            Registers.P4_SetData.Define(this, 0x0)
                .WithFlags(0, 8,
                    valueProviderCallback: (id, _) => false,
                    writeCallback: (id, _, val) => { if(val) Pins[id + P4_PIN_OFFSET].Value = true; })
                .WithReservedBits(8, 8)
            ;
            Registers.P0_ResetData.Define(this, 0x0)
                .WithFlags(0, 8,
                    valueProviderCallback: (id, _) => false,
                    writeCallback: (id, _, val) => { if(val) Pins[id + P0_PIN_OFFSET].Value = false; })
                .WithReservedBits(8, 8)
            ;
            Registers.P1_ResetData.Define(this, 0x0)
                .WithFlags(0, 8,
                    valueProviderCallback: (id, _) => false,
                    writeCallback: (id, _, val) => { if(val) Pins[id + P1_PIN_OFFSET].Value = false; })
                .WithReservedBits(8, 8)
            ;
            Registers.P2_ResetData.Define(this, 0x0)
                .WithFlags(0, 5,
                    valueProviderCallback: (id, _) => false,
                    writeCallback: (id, _, val) => { if(val) Pins[id + P2_PIN_OFFSET].Value = false; })
                .WithReservedBits(5, 11)
            ;
            Registers.P3_ResetData.Define(this, 0x0)
                .WithFlags(0, 8,
                    valueProviderCallback: (id, _) => false,
                    writeCallback: (id, _, val) => { if(val) Pins[id + P3_PIN_OFFSET].Value = false; })
                .WithReservedBits(8, 8)
            ;
            Registers.P4_ResetData.Define(this, 0x0)
                .WithFlags(0, 8,
                    valueProviderCallback: (id, _) => false,
                    writeCallback: (id, _, val) => { if(val) Pins[id + P4_PIN_OFFSET].Value = false; })
                .WithReservedBits(8, 8)
            ;

            Registers.PXX_Mode.DefineMany(this, NumberOfPins, (register, idx) =>
            {
                register
                    .WithValueField(0, 6, name: "PID",
                        writeCallback: (_, val) => Pins[idx].PinFunction = val,
                        valueProviderCallback: _ => Pins[idx].PinFunction)
                    .WithReservedBits(6, 2)
                    .WithEnumField<WordRegister, PinConfiguration>(8, 2, name: "PUPD",
                        writeCallback: (_, val) => Pins[idx].PinConfig = val,
                        valueProviderCallback: _ => Pins[idx].PinConfig)
                    .WithFlag(10, name: "PPOD")
                    .WithReservedBits(11, 5)
                ;
            }, 2, 0x200);

            Registers.P0_PadPowerControl.Define(this, 0x0)
                .WithReservedBits(0, 6)
                .WithFlag(6, name: "P0_6_OUT_CTRL")
                .WithFlag(7, name: "P0_7_OUT_CTRL")
                .WithReservedBits(8, 8)
            ;
            Registers.P1_PadPowerControl.Define(this, 0x0)
                .WithFlag(0, name: "P1_0_OUT_CTRL")
                .WithFlag(1, name: "P1_1_OUT_CTRL")
                .WithFlag(2, name: "P1_2_OUT_CTRL")
                .WithFlag(3, name: "P1_3_OUT_CTRL")
                .WithFlag(4, name: "P1_4_OUT_CTRL")
                .WithFlag(5, name: "P1_5_OUT_CTRL")
                .WithFlag(6, name: "P1_6_OUT_CTRL")
                .WithFlag(7, name: "P1_7_OUT_CTRL")
                .WithReservedBits(8, 8)
            ;
            Registers.P2_PadPowerControl.Define(this, 0x0)
                .WithFlag(0, name: "P2_0_OUT_CTRL")
                .WithFlag(1, name: "P2_1_OUT_CTRL")
                .WithFlag(2, name: "P2_2_OUT_CTRL")
                .WithFlag(3, name: "P2_3_OUT_CTRL")
                .WithFlag(4, name: "P2_4_OUT_CTRL")
                .WithReservedBits(5, 11)
            ;
            Registers.P3_PadPowerControl.Define(this, 0x0)
                .WithFlag(0, name: "P3_0_OUT_CTRL")
                .WithFlag(1, name: "P3_1_OUT_CTRL")
                .WithFlag(2, name: "P3_2_OUT_CTRL")
                .WithFlag(3, name: "P3_3_OUT_CTRL")
                .WithFlag(4, name: "P3_4_OUT_CTRL")
                .WithFlag(5, name: "P3_5_OUT_CTRL")
                .WithFlag(6, name: "P3_6_OUT_CTRL")
                .WithFlag(7, name: "P3_7_OUT_CTRL")
                .WithReservedBits(8, 8)
            ;
            Registers.P4_PadPowerControl.Define(this, 0x0)
                .WithFlag(0, name: "P4_0_OUT_CTRL")
                .WithFlag(1, name: "P4_1_OUT_CTRL")
                .WithFlag(2, name: "P4_2_OUT_CTRL")
                .WithFlag(3, name: "P4_3_OUT_CTRL")
                .WithFlag(4, name: "P4_4_OUT_CTRL")
                .WithFlag(5, name: "P4_5_OUT_CTRL")
                .WithFlag(6, name: "P4_6_OUT_CTRL")
                .WithFlag(7, name: "P4_7_OUT_CTRL")
                .WithReservedBits(8, 8)
            ;
            Registers.GpioClkSel.Define(this, 0x0)
                .WithValueField(0, 3, name: "FUNC_CLOCK_SEL")
                .WithReservedBits(3, 13)
            ;
        }

        public Pin[] Pins { get; }

        private const int NumberOfPins = 37;

        public class Pin
        {
            public Pin(DA1468x_GPIO parent, int id)
            {
                this.Parent = parent;
                this.Id = id;
            }

            public void Reset()
            {
                Parent.Connections[Id].Set(false);
                PinConfig = PinConfiguration.InputPullDown;
            }

            public bool Value
            {
                get
                {
                    Parent.NoisyLog("Reading pin {0} value = {1}", Id, Parent.State[Id]);
                    return Parent.State[Id];
                }

                set
                {
                    if(PinConfig != PinConfiguration.Output)
                    {
                        if(value)
                        {
                            Parent.Log(LogLevel.Warning, "Trying to write pin #{0} that is not configured as output", Id);
                        }
                        return;
                    }

                    Parent.NoisyLog("Setting pin {0} to {1}", Id, value);
                    Parent.Connections[Id].Set(value);
                    //Parent.PinChanged(this, value);       //TODO - uncomment if/when GPIO interrupts implemented
                }
            }

            public PinConfiguration PinConfig { get; set; }

            public uint PinFunction = 0;

            public DA1468x_GPIO Parent { get; }
            public int Id { get; }
        }

        [DefaultValue(InputPullDown)]
        public enum PinConfiguration
        {
            Input = 0,
            InputPullUp = 1,
            InputPullDown = 2,
            Output = 3
        }

        private const uint P0_PIN_OFFSET = 0;
        private const uint P1_PIN_OFFSET = 8;
        private const uint P2_PIN_OFFSET = 16;
        private const uint P3_PIN_OFFSET = 21;
        private const uint P4_PIN_OFFSET = 29;

        private enum Registers
        {
            P0_Data = 0x0,
            P1_Data = 0x2,
            P2_Data = 0x4,
            P3_Data = 0x6,
            P4_Data = 0x8,
            P0_SetData = 0xA,
            P1_SetData = 0xC,
            P2_SetData = 0xE,
            P3_SetData = 0x10,
            P4_SetData = 0x12,
            P0_ResetData = 0x14,
            P1_ResetData = 0x16,
            P2_ResetData = 0x18,
            P3_ResetData = 0x1A,
            P4_ResetData = 0x1C,
            // There are 37 mode registers starting at 0x1E
            PXX_Mode = 0x1E,
            P0_PadPowerControl = 0xC0,
            P1_PadPowerControl = 0xC2,
            P2_PadPowerControl = 0xC4,
            P3_PadPowerControl = 0xC6,
            P4_PadPowerControl = 0xC8,
            GpioClkSel = 0xD0,
        }
    }
}
