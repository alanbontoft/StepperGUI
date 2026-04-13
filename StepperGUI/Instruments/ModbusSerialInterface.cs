using NModbus;
using NModbus.Serial;
using System.IO.Ports;

namespace ModbusInterface;

public abstract class ModbusSerialInterface :  ModbusInterfaceBase
{
    private SerialPort? _port;

    // serial port parameters
    private string _portname;
    private int _baudrate;
    private int _databits;
    private Parity _parity;
    private StopBits _stopbits;
    private int _readTimeout;
    private int _writeTimeout;

    public int BaudRate
    {
        get { return _baudrate; }
        set { _baudrate = value; }
    }

    public string PortName
    {
        get { return _portname; }
        // set { _portname = value; }
    }

    //public ushort[]? Data
    //{
    //    get
    //    {
    //        if (_data == null) return null;

    //        return (ushort[])_data.Clone();
    //    }
    //}

    public bool IsOpen
    {
        get
        {
            if (_port == null) return false;
            return _port.IsOpen;
        }
    }

    public ModbusSerialInterface() : base()
    {
        // set default slave address
        _portname = string.Empty;
        _baudrate = 9600;
        _databits = 8;
        _parity = Parity.None;
        _stopbits = StopBits.One;
        _readTimeout = 500;
        _writeTimeout = 500;
    }


    public ModbusSerialInterface(string portname) : base()
    {
        // set default port parameters
        _portname = portname;
        _baudrate = 9600;
        _databits = 8;
        _parity = Parity.None;
        _stopbits = StopBits.One;
        _readTimeout = 500;
        _writeTimeout = 500;
    }

    public ModbusSerialInterface(string portname, int baudrate = 9600, int databits = 8, Parity parity = Parity.None, StopBits stopbits = StopBits.One, int readTimeout = 500, int writeTimeout = 500) : base()
    {
        // set port parameters
        _portname = portname;
        _baudrate = baudrate;
        _databits = databits;
        _parity = parity;
        _stopbits = stopbits;
        _readTimeout = readTimeout;
        _writeTimeout = writeTimeout;
    }


    public bool Init(int readTimeout = 500)
    {
        bool result = true;

        try
        {
            resetError();

            // close any previous connection
            Close();
             
            _port = new SerialPort(_portname)   {   BaudRate = _baudrate,
                                                    DataBits = _databits,
                                                    Parity = _parity,
                                                    StopBits = _stopbits,
                                                    ReadTimeout = _readTimeout, 
                                                    WriteTimeout = _writeTimeout 
                                                };

            if (_port == null) throwEx("Error creating com port object");

            _port!.Open();

            _master = _factory!.CreateRtuMaster(_port);

            _master.Transport.ReadTimeout = readTimeout;

            if (_master == null) throwEx("Error creating modbus master");
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            result = false;
        }

        return result;
    }

    public void Close()
    {
        //// close and null SerialPort
        if (_port != null)
        {
            _port.Close();
            _port = null;
        }

        // Dispose and null ModbusMaster
        if (_master != null)
        {
            _master.Dispose();
            // _master = null;
        }
    }


    public ushort[]? ReadHoldingRegisters(ushort startAddress, ushort count)
    {
        ushort[]? result;

        try
        {
            resetError();

            var data = _master.ReadHoldingRegisters(_slaveId, startAddress, count);

            if (data == null) throwEx("Error reading Holding Registers");

            result = data;
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            result = null;
        }

        return result;
    }

    public ushort[]? ReadInputRegisters(ushort startAddress, ushort count)
    {
        ushort[]? result;

        try
        {
            resetError();

            var data = _master.ReadInputRegisters(_slaveId, startAddress, count);

            if (data == null) throwEx("Error reading Input Registers");

            result = data;
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            result = null;
        }

        return result;
    }

    public bool WriteRegisters(ushort startAddress, ushort[] data)
    {
        bool result = true;

        try
        {
            resetError();

            if (data.Length > 123) throw new ArgumentOutOfRangeException("Data Length");

            _master.WriteMultipleRegisters(_slaveId, startAddress, data);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            result = false;
        }

        return result;
    }

    public bool WriteRegister(ushort startAddress, ushort data)
    {
        bool result = true;

        try
        {
            resetError();

            _master.WriteSingleRegister(_slaveId, startAddress, data);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            result = false;
        }

        return result;
    }

    public bool[]? ReadCoils(ushort startAddress, ushort count)
    {
        bool[]? result;

        try
        {
            resetError();

            var data = _master.ReadCoils(_slaveId, startAddress, count);

            if (data == null) throwEx("Error reading Coils");

            result = data;
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            result = null;
        }

        return result;
    }

    public bool WriteCoils(ushort startAddress, bool[] data)
    {
        bool result = true;

        try
        {
            resetError();

            _master.WriteMultipleCoils(_slaveId, startAddress, data);
        }
        catch (Exception ex)
        {
            Error = $"(WriteCoils) {ex.Message}";
            result = false;
        }

        return result;
    }

    public bool WriteCoil(ushort coilAddress, bool state)
    {
        bool result = true;

        try
        {
            resetError();

            _master.WriteSingleCoil(_slaveId, coilAddress, state);
        }
        catch (Exception ex)
        {
            Error = $"(WriteCoil) {ex.Message}";
            result = false;
        }

        return result;
    }

    public bool ResetCoils(ushort startAddress, ushort count)
    {
        // create bool array, all false by default
        var data = new bool[count];

        return WriteCoils(startAddress, data);
    }

    public bool SetCoils(ushort startAddress, ushort count)
    {
        var data = Enumerable.Repeat(true, count).ToArray();

        return WriteCoils(startAddress, data);
    }

    public bool ReadFloatValue(ushort startAddress, bool inputreg, out float value)
    {
        bool result = true;
        UInt32 ui32;

        value = 0F;

        try
        {
            resetError();

            var data = inputreg ? _master.ReadInputRegisters(_slaveId, startAddress, 2) : _master.ReadHoldingRegisters(_slaveId, startAddress, 2);

            if (data == null) throwEx("Error reading Input Registers");

            ui32 = (UInt32)(data![0] << 16) | data[1];

            value = BitConverter.ToSingle(BitConverter.GetBytes(ui32));
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            result = false;
        }

        return result;
    }

    public bool ReadUInt32Value(ushort startAddress, bool inputreg, out UInt32 value)
    {
        bool result = true;
        value = 0;
        string type = inputreg ? "Input" : "Holding";

        try
        {
            resetError();

            var data = inputreg ? _master.ReadInputRegisters(_slaveId, startAddress, 2) : _master.ReadHoldingRegisters(_slaveId, startAddress, 2);

            if (data == null) throwEx($"Error reading {type} Registers");

            value = (UInt32)(data![0] << 16) | data[1];
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            result = false;
        }

        return result;
    }



    public bool ReadInt32Value(ushort startAddress, bool inputreg, out Int32 value)
    {
        bool result = true;
        value = 0;
        string type = inputreg ? "Input" : "Holding";

        try
        {
            resetError();

            var data = inputreg ? _master.ReadInputRegisters(_slaveId, startAddress, 2) : _master.ReadHoldingRegisters(_slaveId, startAddress, 2);

            if (data == null) throwEx($"Error reading {type} Registers");

            value = (Int32)(data![0] << 16) | data[1];
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            result = false;
        }

        return result;
    }

    public bool ReadUInt16Value(ushort startAddress, bool inputreg, out UInt16 value)
    {
        bool result = true;
        value = 0;
        string type = inputreg ? "Input" : "Holding";

        try
        {
            resetError();

            var data = inputreg ? _master.ReadInputRegisters(_slaveId, startAddress, 1) : _master.ReadHoldingRegisters(_slaveId, startAddress, 1);

            if (data == null) throwEx($"Error reading {type} Register");

            value = (UInt16)data![0];
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            result = false;
        }

        return result;
    }

    public bool ReadInt16Value(ushort startAddress, bool inputreg, out Int16 value)
    {
        bool result = true;
        value = 0;
        string type = inputreg ? "Input" : "Holding";

        try
        {
            resetError();

            var data = inputreg ? _master.ReadInputRegisters(_slaveId, startAddress, 1) : _master.ReadHoldingRegisters(_slaveId, startAddress, 1);

            if (data == null) throwEx($"Error reading {type} Register");

            value = (Int16)data![0];
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            result = false;
        }

        return result;
    }

    protected void throwEx(string s) { throw new Exception(s); }

    protected void testConnected() { }//  if (!Connected) throwEx("Slave not connected"); }

    protected void resetError() {  Error = string.Empty; }
}
