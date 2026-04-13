using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.IO.Ports;
using ModbusInterface;
using System;

namespace StepperGUI.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        StepperModbusInterface _stepper;

        public string Greeting { get; } = "Welcome to Avalonia!";

        [ObservableProperty]
        List<string> _portList;

        [ObservableProperty]
        string _portName;

        [ObservableProperty]
        string _angle;

        [ObservableProperty]
        string _channel;

        public MainWindowViewModel()
        {
            _portList = [];

            var ports = SerialPort.GetPortNames();

            foreach (var port in ports) { _portList.Add(port); }
        }

        [RelayCommand]
        void InitPort()
        {
            _stepper = new StepperModbusInterface(PortName);

            _stepper.BaudRate = 38400;

            _stepper.Init();
        }

        [RelayCommand]
        void Rotate()
        {
            ushort[] data = new ushort[4];

            var bytes = BitConverter.GetBytes(float.Parse(Angle));

            data[0] = BitConverter.ToUInt16(bytes, 0);
            data[1] = BitConverter.ToUInt16(bytes, 2);

            var ch = Channel.ToString();
            data[3] = UInt16.Parse(Channel);
            _stepper.WriteRegisters(1, data);

            _stepper.WriteRegister(0, 1);

        }
    }
}
