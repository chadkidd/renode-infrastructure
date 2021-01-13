//
// Copyright (c) 2010-2020 Antmicro
//
// This file is licensed under the MIT License.
// Full license text is available in 'licenses/MIT.txt'.
//
using Antmicro.Renode.Core;
using Antmicro.Renode.Core.Structure.Registers;
using Antmicro.Renode.Logging;
using Antmicro.Renode.Peripherals.SPI;
using Antmicro.Renode.Peripherals.Sensor;
using Antmicro.Renode.Utilities;

namespace Antmicro.Renode.Peripherals.Sensors
{
    public class LIS3DSH_Accelerometer : ISPIPeripheral, IGPIOReceiver, IProvidesRegisterCollection<ByteRegisterCollection>, ISensor
    {
        public LIS3DSH_Accelerometer()
        {
            RegistersCollection = new ByteRegisterCollection(this);
            DefineRegisters();
        }

        public byte Transmit(byte data)
        {
            byte result = 0;

            switch(state)
            {
                case State.Idle:
                {
                    selectedRegister = (Registers)BitHelper.GetValue(data, 0, 7);
                    var isRead = BitHelper.IsBitSet(data, 7);

                    this.NoisyLog("Decoded register {0} (0x{0:X}) and isRead bit as {1}", selectedRegister, isRead);
                    state = isRead
                        ? State.Reading
                        : State.Writing;

                    break;
                }

                case State.Reading:
                    this.NoisyLog("Reading register {0} (0x{0:X})", selectedRegister);
                    result = RegistersCollection.Read((long)selectedRegister);
                    break;

                case State.Writing:
                    this.NoisyLog("Writing 0x{0:X} to register {1} (0x{1:X})", data, selectedRegister);
                    RegistersCollection.Write((long)selectedRegister, data);
                    UpdateDataAvailable();
                    break;

                default:
                    this.Log(LogLevel.Error, "Received byte in an unexpected state: {0}", state);
                    break;
            }

            this.NoisyLog("Received byte 0x{0:X}, returning 0x{1:X}", data, result);
            return result;
        }

        void ISPIPeripheral.FinishTransmission()
        {
            this.NoisyLog("Finishing transmission, going idle");
            state = State.Idle;
        }

        public void OnGPIO(int number, bool value)
        {
            this.NoisyLog("OnGPIO -> got signal on pin {0} with value {1}", number, value);
            if(number != 0)
            {
                this.Log(LogLevel.Warning, "This model supports only CS on pin 0, but got signal on pin {0} with value {1}", number, value);
                return;
            }

            // value is the negated CS
            if(chipSelected && value)
            {
                ((ISPIPeripheral)this).FinishTransmission();
            }
            chipSelected = !value;
        }

        public void Reset()
        {
            RegistersCollection.Reset();
            chipSelected = false;
            selectedRegister = 0x0;
            state = State.Idle;
        }

        private void UpdateDataAvailable()
        {
            if(dataRate.Value == 0)
            {
                this.Log(LogLevel.Debug, "Power-down mode is set");
                xyzDataAvailable.Value = false;
                xDataAvailable.Value = false;
                yDataAvailable.Value = false;
                zDataAvailable.Value = false;
            }
            else
            {
                this.Log(LogLevel.Noisy, "XYZ_DataAvailable, X={0}, Y={1}, Z={2}", xAxisEnable.Value, yAxisEnable.Value, zAxisEnable.Value);
                if(xAxisEnable.Value && yAxisEnable.Value && zAxisEnable.Value)
                {
                    xyzDataAvailable.Value = true;
                    xDataAvailable.Value = false;
                    yDataAvailable.Value = false;
                    zDataAvailable.Value = false;
                }
                else
                {
                    xyzDataAvailable.Value = false;
                    xDataAvailable.Value = xAxisEnable.Value;
                    yDataAvailable.Value = yAxisEnable.Value;
                    zDataAvailable.Value = zAxisEnable.Value;
                }
            }
        }

        public decimal AccelerationX
        {
            get => accelerationX;
            set
            {
                if(!IsAccelerationOutOfRange(value))
                {
                    accelerationX = value;
                    this.Log(LogLevel.Noisy, "AccelerationX set to {0}", accelerationX);
                }
            }
        }

        public decimal AccelerationY
        {
            get => accelerationY;
            set
            {
                if(!IsAccelerationOutOfRange(value))
                {
                    accelerationY = value;
                    this.Log(LogLevel.Noisy, "AccelerationY set to {0}", accelerationY);
                }
            }
        }

        public decimal AccelerationZ
        {
            get => accelerationZ;
            set
            {
                if(!IsAccelerationOutOfRange(value))
                {
                    accelerationZ = value;
                    this.Log(LogLevel.Noisy, "AccelerationZ set to {0}", accelerationZ);
                }
            }
        }

        public ushort Sensitivity
        {
            get => GetSensitivity();
        }

        public bool XaxisEnable
        {
            get => xAxisEnable.Value;
            set
            {
                accelerationX = 0;
                xAxisEnable.Value = value;
                this.Log(LogLevel.Noisy, "X axis enable set to {0}", value);
            }
        }

        public bool YaxisEnable
        {
            get => yAxisEnable.Value;
            set
            {
                accelerationY = 0;
                yAxisEnable.Value = value;
                this.Log(LogLevel.Noisy, "Y axis enable set to {0}", value);
            }
        }

        public bool ZaxisEnable
        {
            get => zAxisEnable.Value;
            set
            {
                accelerationZ = 0;
                zAxisEnable.Value = value;
                this.Log(LogLevel.Noisy, "Z axis enable set to {0}", value);
            }
        }

        public ByteRegisterCollection RegistersCollection { get; }

        private void DefineRegisters()
        {
            Registers.WhoAmI.Define(this, 0x3F);  //RO

            Registers.Control4.Define(this, 0x43) //RW
                .WithFlag(0, out xAxisEnable, name: "X_AXIS_ENABLE")
                .WithFlag(1, out yAxisEnable, name: "Y_AXIS_ENABLE")
                .WithFlag(2, out zAxisEnable, name: "Z_AXIS_ENABLE")
                .WithTaggedFlag("BDU", 3)
                .WithValueField(4, 4, out dataRate, name: "OUTPUT_DATA_RATE");

            Registers.Control5.Define(this, 0x00) //RW
                .WithTaggedFlag("SIM", 0)
                .WithTag("ST", 1, 2)
                .WithValueField(3, 3, out fullScale, name: "FSCALE")
                .WithTag("BW", 6, 2);

            Registers.StatusReg.Define(this, 0x08) //RO
                .WithFlag(0, out xDataAvailable, FieldMode.Read, name: "X_DATA_AVAILABLE")
                .WithFlag(1, out yDataAvailable, FieldMode.Read, name: "Y_DATA_AVAILABLE")
                .WithFlag(2, out zDataAvailable, FieldMode.Read, name: "Z_DATA_AVAILABLE")
                .WithFlag(3, out xyzDataAvailable, FieldMode.Read, name: "ZYX_DATA_AVAILABLE")
                .WithTaggedFlag("X_DATA_OVERRUN", 4)
                .WithTaggedFlag("Y_DATA_OVERRUN", 5)
                .WithTaggedFlag("Z_DATA_OVERRUN", 6)
                .WithTaggedFlag("ZYX_DATA_OVERRUN", 7);

            Registers.DataOutXL.Define(this)
                .WithValueField(0, 8, FieldMode.Read, name: "X_ACCEL_DATA[7:0]", valueProviderCallback: _ => Convert(AccelerationX, upperByte: false));

            Registers.DataOutXH.Define(this)
                .WithValueField(0, 8, FieldMode.Read, name: "X_ACCEL_DATA[15:8]", valueProviderCallback: _ => Convert(AccelerationX, upperByte: true));

            Registers.DataOutYL.Define(this)
                .WithValueField(0, 8, FieldMode.Read, name: "Y_ACCEL_DATA[7:0]", valueProviderCallback: _ => Convert(AccelerationY, upperByte: false));

            Registers.DataOutYH.Define(this)
                .WithValueField(0, 8, FieldMode.Read, name: "Y_ACCEL_DATA[15:8]", valueProviderCallback: _ => Convert(AccelerationY, upperByte: true));

            Registers.DataOutZL.Define(this)
                .WithValueField(0, 8, FieldMode.Read, name: "Z_ACCEL_DATA[7:0]", valueProviderCallback: _ => Convert(AccelerationZ, upperByte: false));

            Registers.DataOutZH.Define(this)
                .WithValueField(0, 8, FieldMode.Read, name: "Z_ACCEL_DATA[15:8]", valueProviderCallback: _ => Convert(AccelerationZ, upperByte: true));

        }

        private ushort GetSensitivity()
        {
            ushort sensitivity = 0; // [mg/LSB]
            switch(fullScale.Value)
            {
                case 0:
                    sensitivity = 2;
                    break;
                case 1:
                    sensitivity = 4;
                    break;
                case 2:
                    sensitivity = 6;
                    break;
                case 3:
                    sensitivity = 8;
                    break;
                case 4:
                    sensitivity = 16;
                    break;
                default:
                    this.Log(LogLevel.Warning, "Unsupported value of sensor sensitivity.");
                    break;
            }
            return sensitivity;
        }

        private bool IsAccelerationOutOfRange(decimal acceleration)
        {
            // This range protects from the overflow of the short variables in the 'Convert' function.
            if(acceleration < MinAcceleration || acceleration > MaxAcceleration)
            {
                this.Log(LogLevel.Warning, "Acceleration is out of range, use value from the range <{0};{1}>",
                                            MinAcceleration, MaxAcceleration);
                return true;
            }
            return false;
        }

        private byte Convert(decimal value, bool upperByte)
        {
            decimal convertedValue = (decimal)(((short)value << 14) / (GetSensitivity() * GravitationalConst));
            short convertedValueAsShort = (short)convertedValue;
            return upperByte ? (byte)(convertedValueAsShort >> 8) : (byte)convertedValueAsShort;
        }

        private IValueRegisterField dataRate;
        private IValueRegisterField fullScale;
        private IFlagRegisterField xyzDataAvailable;
        private IFlagRegisterField xAxisEnable, yAxisEnable, zAxisEnable;
        private IFlagRegisterField xDataAvailable, yDataAvailable, zDataAvailable;

        private decimal accelerationX;
        private decimal accelerationY;
        private decimal accelerationZ;

        private const decimal MinAcceleration = -19.0m;
        private const decimal MaxAcceleration = 19.0m;
        private const decimal GravitationalConst = 9.806650m; // [m/s^2]

        private bool chipSelected;
        private Registers selectedRegister;
        private State state;

        private enum State
        {
            Idle,
            Reading,
            Writing
        }

        private enum Registers : byte
        {
            // Reserved: 0x00 - 0x0B
            WhoAmI = 0x0F,
            Control4 = 0x20,
            Control5 = 0x24,
            StatusReg = 0x27,
            DataOutXL = 0x28,
            DataOutXH = 0x29,
            DataOutYL = 0x2A,
            DataOutYH = 0x2B,
            DataOutZL = 0x2C,
            DataOutZH = 0x2D,  
        }
    }
}
