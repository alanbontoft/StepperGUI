
using System;
using System.IO.Ports;

namespace ModbusInterface;

public enum StepperHoldingRegisters
{
	HR_SLAVEADDRESS = 41,
	HR_CONFIGPIN = 42
}

public enum StepperInputRegisters
{
    IR_FLANGE_SN = 0,
    IR_EDRAWING = 7,
    IR_SENSOR_SN = 10,
    IR_PRESSURE = 24,
	IR_TEMPERATURE = 34
}

public class StepperModbusInterface : ModbusSerialInterface
{
	public StepperModbusInterface(string portname, int baudrate = 9600, int databits = 8, Parity parity = Parity.None, StopBits stopbits = StopBits.One, int readTimeout = 500, int writeTimeout = 500)
			: base(portname, baudrate, databits, parity, stopbits, readTimeout, writeTimeout)
    {
	}


    public bool ReadSerialNumber(out UInt32 value, bool flange = false)
    {
        ushort address = flange ? (ushort)StepperInputRegisters.IR_FLANGE_SN :  (ushort)StepperInputRegisters.IR_SENSOR_SN;
        return ReadUInt32Value(address, true, out value);
    }

    public bool ReadPressure(out float value)
    {
        ushort address = (ushort)StepperInputRegisters.IR_PRESSURE;
        return ReadFloatValue(address, true, out value);
    }

    public bool ReadTemperature(out float value)
    {
        ushort address = (ushort)StepperInputRegisters.IR_TEMPERATURE;
        return ReadFloatValue(address, true, out value);
    }
}

