using NModbus;
using System;

namespace ModbusInterface;

public abstract class ModbusInterfaceBase
{
    protected ModbusFactory? _factory;

    protected IModbusMaster _master;

    protected byte _slaveId;

    protected ushort[] _data;

    public string Error { get; protected set; }


    protected ModbusInterfaceBase()
    {
        _factory = new();

        _slaveId = 1;

        Error = string.Empty;
    }

    public byte SlaveId
    {
        get { return _slaveId; }

        set
        {
            if (value > 247) throw new ArgumentOutOfRangeException("SlaveId");

            _slaveId = value;
        }
    }
}
