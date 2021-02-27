//
// Copyright (c) 2010-2019 Antmicro
//
// This file is licensed under the MIT License.
// Full license text is available in 'licenses/MIT.txt'.
//
using System;
using Antmicro.Renode.Core;
using System.Collections.Generic;
using Antmicro.Renode.Peripherals.Bus;
using Antmicro.Renode.Core.Structure.Registers;

using Antmicro.Renode.Logging;

namespace Antmicro.Renode.Peripherals.SPI
{
    [AllowedTranslations(AllowedTranslation.ByteToDoubleWord)]
    public class DA1468x_QSPI : IDoubleWordPeripheral, IKnownSize
    {
        public DA1468x_QSPI(Machine machine)
        { 
            var registersMap = new Dictionary<long, DoubleWordRegister>
            {
                {(long)Registers.CtrlBus, new DoubleWordRegister(this, 0x0)
                    .WithFlag(0, name: "QSPIC_SET_SINGLE")
                    .WithFlag(1, name: "QSPIC_SET_DUAL")
                    .WithFlag(2, name: "QSPIC_SET_QUAD")
                    .WithFlag(3, name: "QSPIC_EN_CS")
                    .WithFlag(4, name: "QSPIC_DIS_CS")
                    .WithReservedBits(5, 27)
                },
                {(long)Registers.CtrlMode, new DoubleWordRegister(this, 0x0)
                    .WithFlag(0, out autoMode, name: "QSPIC_AUTO_MD")
                    .WithFlag(1, name: "QSPIC_CLK_MD")
                    .WithFlag(2, name: "QSPIC_IO2_OEN")
                    .WithFlag(3, name: "QSPIC_IO3_OEN")
                    .WithFlag(4, name: "QSPIC_IO2_DAT")
                    .WithFlag(5, name: "QSPIC_IO3_DAT")
                    .WithFlag(6, name: "QSPIC_HRDY_MD")
                    .WithFlag(7, name: "QSPIC_RXD_NEG")
                    .WithFlag(8, name: "QSPIC_RPIPE_EN")
                    .WithValueField(9, 3, name: "QSPIC_PCLK_MD")
                    .WithFlag(12, name: "QSPIC_FORCENSEQ_EN")
                    .WithFlag(13, name: "QSPIC_USE_32BA")
                    .WithReservedBits(14, 18)
                },
                {(long)Registers.RecvData, new DoubleWordRegister(this, 0x0)
                    .WithTag("RECVDATA", 0, 32)
                },
                {(long)Registers.BurstCmdA, new DoubleWordRegister(this, 0x0)
                    .WithValueField(0, 8, name: "QSPIC_INST")
                    .WithValueField(8, 8, name: "QSPIC_INST_WB")
                    .WithValueField(16, 8, name: "QSPIC_EXT_BYTE")
                    .WithValueField(24, 2, name: "QSPIC_INST_TX_MD")
                    .WithValueField(26, 2, name: "QSPIC_ADR_TX_MD")
                    .WithValueField(28, 2, name: "QSPIC_EXT_TX_MD")
                    .WithValueField(30, 2, name: "QSPIC_DMY_TX_MD")
                },
                {(long)Registers.BurstCmdB, new DoubleWordRegister(this, 0x0)
                    .WithValueField(0, 2, name: "QSPIC_DAT_RX_MD")
                    .WithFlag(2, name: "QSPIC_EXT_BYTE_EN")
                    .WithFlag(3, name: "QSPIC_EXT_HJF_DS")
                    .WithValueField(4, 2, name: "QSPIC_DMY_NUM")
                    .WithFlag(6, name: "QSPIC_INST_MD")
                    .WithFlag(7, name: "QSPIC_WRAP_MD")
                    .WithValueField(8, 2, name: "QSPIC_WRAP_LEN")
                    .WithValueField(10, 2, name: "QSPIC_WRAP_SIZE")
                    .WithValueField(12, 3, name: "QSPIC_CS_HIGH_MIN")
                    .WithFlag(15, name: "QSPIC_DMY_FORCE")
                    .WithReservedBits(16, 16)
                },
                {(long)Registers.StatusReg, new DoubleWordRegister(this, 0x0)
                    .WithTaggedFlag("QSPIC_BUSY", 0)
                    .WithReservedBits(1, 31)
                },
                {(long)Registers.WriteData, new DoubleWordRegister(this, 0x0)
                    .WithValueField(0, 32, name: "QSPIC_WRITEDATA", mode: FieldMode.Write, writeCallback: (_, value) =>
                    {
                        sendCommand(value);
                    })
                },
                {(long)Registers.ReadData, new DoubleWordRegister(this, 0x0)
                    .WithValueField(0, 32, name: "QSPIC_READDATA", mode: FieldMode.Read, valueProviderCallback: (_) =>
                    {
                        return readData();
                    })
                },
                {(long)Registers.EraseCmdA, new DoubleWordRegister(this, 0x0)
                    .WithValueField(0, 8, name: "QSPIC_ERS_INST")
                    .WithValueField(8, 8, name: "QSPIC_WEN_INST")
                    .WithValueField(16, 8, name: "QSPIC_SUS_INST")
                    .WithValueField(24, 8, name: "QSPIC_RES_INST")
                },
                {(long)Registers.EraseCmdB, new DoubleWordRegister(this, 0x0)
                    .WithValueField(0, 2, name: "QSPIC_ERS_TX_MD")
                    .WithValueField(2, 2, name: "QSPIC_WEN_TX_MD")
                    .WithValueField(4, 2, name: "QSPIC_SUS_TX_MD")
                    .WithValueField(6, 2, name: "QSPIC_RES_TX_MD")
                    .WithValueField(8, 2, name: "QSPIC_EAD_TX_MD")
                    .WithValueField(10, 5, name: "QSPIC_ERS_CS_HI")
                    .WithReservedBits(15, 1)
                    .WithValueField(16, 4, name: "QSPIC_ERSRES_HLD")
                    .WithReservedBits(20, 4)
                    .WithValueField(24, 6, name: "QSPIC_RESSUS_DLY")
                    .WithReservedBits(30, 2)
                },
                {(long)Registers.BurstBreak, new DoubleWordRegister(this, 0x0)
                    .WithValueField(0, 16, name: "QSPIC_BRK_WRD")
                    .WithFlag(16, name: "QSPIC_BRK_EN")
                    .WithFlag(17, name: "QSPIC_BRK_SZ")
                    .WithValueField(18, 2, name: "QSPIC_BRK_TX_MD")
                    .WithFlag(20, name: "QSPIC_SEC_HF_DS")
                    .WithReservedBits(21, 11)
                },
                {(long)Registers.StatusCmd, new DoubleWordRegister(this, 0x0)
                    .WithValueField(0, 8, name: "QSPIC_RSTAT_INST")
                    .WithValueField(8, 2, name: "QSPIC_RSTAT_TX_MD")
                    .WithValueField(10, 2, name: "QSPIC_RSTAT_RX_MD")
                    .WithValueField(12, 3, name: "QSPIC_BUSY_POS")
                    .WithFlag(15, name: "QSPIC_BUSY_VAL")
                    .WithValueField(16, 6, name: "QSPIC_RESSTS_DLY")
                    .WithFlag(22, name: "QSPIC_STSDLY_SEL")
                    .WithReservedBits(23, 9)
                },
                {(long)Registers.UCodeStart, new DoubleWordRegister(this, 0x55000025)
                    .WithFlag(0, name: "CMD_VALID")
                    .WithValueField(1, 2, name: "CMD_TX_MD")
                    .WithValueField(3, 5, name: "CMD_NBYTES")
                    .WithValueField(8, 8, name: "CMD_WT_CNT_LS")
                    .WithValueField(16, 8, name: "CMD_WT_CNT_MS")
                    .WithValueField(24, 8, name: "CMD_BYTE1")
                },
                {(long)Registers.UCode1, new DoubleWordRegister(this, 0x55555555)
                    .WithValueField(0, 8, name: "CMD_BYTE2")
                    .WithValueField(8, 8, name: "CMD_BYTE3")
                    .WithValueField(16, 8, name: "CMD_BYTE4")
                    .WithValueField(24, 8, name: "CMD_BYTE5")
                },
            };
            registers = new DoubleWordRegisterCollection(this, registersMap);
            InitializeFlashRegisters();

        }

        private void InitializeFlashRegisters()
        {
            //These registers are for the Winbond W25Q80DV
            FlashStatus1 = new ByteRegister(this, 0x0)
                .WithFlag(0, name: "BUSY")
                .WithFlag(1, name: "WEL", mode: FieldMode.ReadToClear | FieldMode.Write)
                .WithValueField(2, 3, name: "BLOCK_PROTECT_BITS")
                .WithFlag(5, name: "TB")
                .WithFlag(6, name: "SEC")
                .WithFlag(7, name: "SRP0");
        }

        private uint readData()
        {
            uint data = 0;
            switch((FlashCommand)(spiCommand))
            {
                case FlashCommand.ReadID:
                    data = 0xffffffff;
                    break;
                case FlashCommand.ReadStatusRegister:
                    data = FlashStatus1.Read();
                    break;

                default:
                    break;
            }
            this.Log(LogLevel.Noisy, "QSPI Flash read command 0x{0:X}, data = 0x{1:X}", spiCommand, data);
            return data;
        }

        private void sendCommand(uint command)
        { 
            this.Log(LogLevel.Noisy, "QSPI Flash send command = 0x{0:X}", command);
            spiCommand = command;
            switch((FlashCommand)(command & 0xff))
            {
                case FlashCommand.ReadID:
                    break;

                case FlashCommand.ReadStatusRegister:
                    break;

                case FlashCommand.WriteEnable:
                    FlashStatus1.Write(0, (byte)FlashStatus1Bits.WEL);
                    break;

                case FlashCommand.ExitContinuousMode:
                    FlashStatus1.Reset();
                    break;

                default:
                    this.Log(LogLevel.Warning, "Unimplemented QSPI Flash command = 0x{0:X}", command);
                    break;
            }
        }

        public void Reset()
        {
            InitializeFlashRegisters();
            registers.Reset();
        }
        
        public uint ReadDoubleWord(long offset)
        {
            return registers.Read(offset);
        }

        public void WriteDoubleWord(long offset, uint value)
        {
            registers.Write(offset, value);
        }

        private ByteRegister FlashStatus1;
        private IFlagRegisterField autoMode;
        private uint spiCommand;

        private readonly DoubleWordRegisterCollection registers;

        public long Size => 0x48;

        private enum Registers
        {
            CtrlBus = 0x0,
            CtrlMode = 0x4,
            RecvData = 0x8,
            BurstCmdA = 0xC,
            BurstCmdB = 0x10,
            StatusReg = 0x14,
            WriteData = 0x18,
            ReadData = 0x1C,
            EraseCmdA = 0x28,
            EraseCmdB = 0x2C,
            BurstBreak = 0x30,
            StatusCmd = 0x34,
            UCodeStart = 0x40,
            UCode1 = 0x44
        }

        private enum FlashStatus1Bits : byte
        {
            BUSY = 0x1,
            WEL = 0x2,
            BLOCK_PROTECT = 0x1C,
            TB = 0x20,
            SEC = 0x40,
            SRP0 = 0x80
        }

        private enum FlashCommand
        {
            WriteToStatusRegister = 0x01,
            PageProgram = 0x02,
            Read = 0x03,
            WriteDisable = 0x04,
            ReadStatusRegister = 0x05,
            WriteEnable = 0x06,
            FastRead = 0x0B,
            WriteToStatusRegister2 = 0x31,
            QuadPageProgram = 0x32,
            ReadStatusRegister2 = 0x35,
            BulkErase = 0x60,
            ReadID = 0x9F,
            ChipErase = 0xC7,
            SectorErase = 0xD8,
            ExitContinuousMode = 0xFF
        }
    }
}
