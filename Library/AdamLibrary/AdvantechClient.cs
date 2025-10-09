using RJCP.IO.Ports;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdamLibrary
{
    public class AdvantechClient : IDisposable
    {
        private SerialPortStream _stream;

            public string PortName
            {
                get
                {
                    return _stream.PortName;
                }
                set
                {
                    _stream.PortName = value;
                }
            }

            public int BaudRate
            {
                get
                {
                    return _stream.BaudRate;
                }
                set
                {
                    _stream.BaudRate = value;
                }
            }

            public int DataBits
            {
                get
                {
                    return _stream.DataBits;
                }
                set
                {
                    _stream.DataBits = value;
                }
            }

            public StopBits StopBits
            {
                get
                {
                    return _stream.StopBits;
                }
                set
                {
                    _stream.StopBits = value;
                }
            }

            public Parity Parity
            {
                get
                {
                    return _stream.Parity;
                }
                set
                {
                    _stream.Parity = value;
                }
            }

            public Handshake Handshake
            {
                get
                {
                    return _stream.Handshake;
                }
                set
                {
                    _stream.Handshake = value;
                }
            }

            public AdvantechClient()
            {
                _stream = new SerialPortStream();
                var defaultPortName = SerialPortStream.GetPortNames().FirstOrDefault();
                if (defaultPortName != null)
                    _stream.PortName = defaultPortName;
                BaudRate = 9600;
                Parity = Parity.None;
                StopBits = StopBits.One;
                Handshake = Handshake.None;
            }

            public void Open()
            {
                _stream.Open();
            }

            public bool IsOpen => _stream.IsOpen;

            public void Close()
            {
                _stream.Close();
            }

            public void Send(string cmd)
            {
                _stream.Write(cmd + "\r");
                _stream.Flush();
            }

            public string Recv()
            {
                _stream.ReadTimeout = 1000;
                var resp = _stream.ReadTo("\r");
                if (resp.Length < 1)
                    throw new FormatException($"Response too short to be valid ({resp.Length} < 1).");
                return resp;
            }

            public string SendRecv(string cmd)
            {
                Send(cmd);
                return Recv();
            }


            public double ReadAI(byte unitAdr, int channel)
            {
                var sb = new StringBuilder();
                sb.Append("#");
                sb.Append(unitAdr.ToString("X2"));
                sb.Append(channel);

                string resp = SendRecv(sb.ToString());
                var reader = new StringReader(resp);

                char startChar = (char)reader.Read();
                if (startChar != '>')
                    throw new FormatException($"Response has invalid start character ('{startChar}' != '>').");

                if (resp.Length % 7 != 1)
                    throw new FormatException($"Response data is invalid length ({resp.Length}).");

                if (!reader.TryReadDouble(7, out double value))
                    throw new FormatException($"Response data is not valid number.");

                return value;
            }

            public void WriteAO(byte unitAdr, int channel, double value)
            {
                var sb = new StringBuilder();
                sb.Append("#");
                sb.Append(unitAdr.ToString("X2"));
                sb.Append("C");
                sb.Append(channel);
                sb.AppendDouble(value);

                string resp = SendRecv(sb.ToString());
                var reader = new StringReader(resp);

                char startChar = (char)reader.Read();
                if (startChar != '!')
                    throw new FormatException($"Response has invalid start character ('{startChar}' != '!').");

                if (resp.Length < 3)
                    throw new FormatException($"Response is too short ('{resp.Length} < 3).");

                int recvUnitAdr = reader.ReadInt(2, System.Globalization.NumberStyles.HexNumber);
                if (recvUnitAdr != unitAdr)
                    throw new FormatException($"Response from incorrect address ('{recvUnitAdr} != {unitAdr}).");
            }

            public double ReadAO(byte unitAdr, int channel)
            {
                var sb = new StringBuilder();
                sb.Append("$");
                sb.Append(unitAdr.ToString("X2"));
                sb.Append("6C");
                sb.Append(channel);

                string resp = SendRecv(sb.ToString());
                var reader = new StringReader(resp);

                char startChar = (char)reader.Read();
                if (startChar != '!')
                    throw new FormatException($"Response has invalid start character ('{startChar}' != '!').");

                if (resp.Length != 10)
                    throw new FormatException($"Response data is invalid length ({resp.Length} != 10).");

                int recvUnitAdr = reader.ReadInt(2, System.Globalization.NumberStyles.HexNumber);
                if (recvUnitAdr != unitAdr)
                    throw new FormatException($"Response from incorrect address ('{recvUnitAdr} != {unitAdr}).");

                if (!reader.TryReadDouble(7, out double value))
                    throw new FormatException($"Response data is not valid number.");

                return value;
            }

            public void WriteDO(byte unitAdr, int channel, bool state)
            {
                var sb = new StringBuilder();
                sb.Append("#");
                sb.Append(unitAdr.ToString("X2"));
                sb.Append("1");
                sb.Append(channel);
                sb.Append("0");
                sb.Append(state ? "1" : "0");

                string resp = SendRecv(sb.ToString());
                var reader = new StringReader(resp);

                char startChar = (char)reader.Read();
                if (startChar != '>')
                    throw new FormatException($"Response has invalid start character ('{startChar}' != '>').");
            }

            public bool ReadDO(byte unitAdr, int channel)
            {
                var sb = new StringBuilder();
                sb.Append("$");
                sb.Append(unitAdr.ToString("X2"));
                sb.Append(6);

                string resp = SendRecv(sb.ToString());
                var reader = new StringReader(resp);

                char startChar = (char)reader.Read();
                if (startChar != '!')
                    throw new FormatException($"Response has invalid start character ('{startChar}' != '!').");

                if (resp.Length != 7)
                    throw new FormatException($"Response is wrong length ({resp.Length} != 7).");

                if (!reader.TryReadInt(2, out int value, System.Globalization.NumberStyles.HexNumber))
                    throw new FormatException($"Response is not value hex number.");

                return (value & (1 << channel)) != 0;

            }

            public string ReadModuleType(byte unitAdr)
            {
                var sb = new StringBuilder();
                sb.Append("$");
                sb.Append(unitAdr.ToString("X2"));
                sb.Append("M");

                string resp = SendRecv(sb.ToString());
                var reader = new StringReader(resp);

                char startChar = (char)reader.Read();
                if (startChar != '!')
                    throw new FormatException($"Response has invalid start character ('{startChar}' != '!').");

                if (resp.Length < 7)
                    throw new FormatException($"Response data is too short ({resp.Length} < 7).");

                int recvUnitAdr = reader.ReadInt(2, System.Globalization.NumberStyles.HexNumber);
                if (recvUnitAdr != unitAdr)
                    throw new FormatException($"Response from incorrect address ('{recvUnitAdr} != {unitAdr}).");

                return reader.ReadToEnd();
            }

            public void Dispose()
            {
                if (!_stream.IsDisposed)
                    _stream.Dispose();
            }
        }
    }
