using System;
using System.IO;
using System.Text;

namespace LaserOutReader
{
    internal class Reader
    {
        private int _offset;
        private int _length;
        private byte[] _buffer;
        private TextWriter _outStream;

        void ProcessFiles()
        {
            var path = "../../../Samples/";
            var files = Directory.GetFiles(path, "*.out", SearchOption.TopDirectoryOnly);
            foreach(var file in files)
            {
                var filename = path + Path.GetFileNameWithoutExtension(file) + ".txt";
                using (_outStream = new StreamWriter(File.Create(filename))) { 
                    ReadFile(file);
                    _outStream.Flush();
                    _outStream.Close();
                }
            }
        }

        void ReadFile(string fileName)
        {
            _buffer = File.ReadAllBytes(fileName);
            _outStream.WriteLine("Got {0} bytes\n", _length = _buffer.Length);
            _offset = 0;

            for (; _offset < _buffer.Length;)
            {
                _outStream.Write(_offset + ":\t");
                ReadChunk();
                _outStream.WriteLine();
            }
        }

        byte Peek(int offset = 0)
        {
            return (byte)(_buffer[_offset + offset] ^ 0x63);
        }

        byte Read()
        {
            return (byte)(_buffer[_offset++] ^ 0x63);
        }

        byte Next()
        {
            byte b = Read();
            _outStream.Write("{0:X2} ", b);
            return b;
        }

        byte[] Read(int length)
        {
            var bytes = new byte[length];
            for (var i = 0; i < length; i++)
                bytes[i] = Read();

            return bytes;
        }

        byte[] Next(int length)
        {
            var bytes = new byte[length];
            for (var i = 0; i < length; i++)
                bytes[i] = Next();

            return bytes;
        }

        public Reader()
        {
            ProcessFiles();
            Console.WriteLine("Done");
        }

        void ReadChunk()
        {
            byte b = Next();
            switch (b)
            {
                case 0x00:
                    Next(8);
                    break;
                case 0xE2:
                    if (Next() == 0x01)
                    {
                        _outStream.Write(" File name (or a part of it): " + Encoding.UTF8.GetString(Next(9)));
                    }
                    else
                    {
                        var a = Next();
                        var b2 = Next();
                        var c = Next();
                        var d = Next();
                        //TODO combine numbers
                        _outStream.Write(" Payloadsize: " + BitConverter.ToNumber(a, b2));
                        var payloadSize = (int)BitConverter.ToNumber(c, d);
                        _outStream.Write(" + " + payloadSize);
                    }
                    break;
                case 0xE3:
                    b = Next();
                    switch (b)
                    {
                        case 0x01:
                            _outStream.Write(" ? ");
                            Next();
                            Next();
                            PrintFloat("?");
                            break;
                        case 0x02:
                            _outStream.Write(" <- END ");
                            Next(2);
                            break;
                        case 0x03:
                            _outStream.Write(" <- START ");
                            Next(2);
                            break;
                        default:
                            _outStream.Write("Unknown category {0:X2}", b);
                            return;
                    }
                    break;

                case 0xE0:

                    b = Next();
                    switch (b)
                    {
                        case 0x00:
                            //Next();
                            break;
                        case 0x04:
                            // something related to line position

                            Next(4);
                            PrintFloat(" x");
                            _outStream.Write(" ");
                            PrintFloat(" y");
                            break;
                        case 0x05:
                            Next(10);
                            break;
                        case 0x06:
                            PrintFloat();
                            break;
                        case 0x07:
                            PrintFloat("x");
                            break;
                        case 0x08:
                            PrintFloat();
                            break;
                        case 0x09:
                            PrintFloat("y");
                            break;
                        case 0x0A:
                            Next();
                            break;
                        case 0x0B:
                            Next();
                            break;
                        case 0x0C:
                            PrintFloat();
                            break;
                        case 0x0E:
                            Next();
                            break;
                        case 0x11:
                            Next(8);
                            break;
                        case 0x12:
                            Next(68);
                            break;
                        default:
                            _outStream.Write("Unknown category {0:X2}", b);
                            return;
                    }
                    break;
                case 0xC5:

                    b = Next();
                    switch (b)
                    {
                        case 0x00:
                            Next();
                            break;
                        case 0x02:
                            PrintFloat("cut speed");
                            break;
                        case 0x04:
                            PrintFloat("free speed");
                            break;
                        default:
                            _outStream.Write("Unknown category {0:X2}", b);
                            return;
                    }
                    break;
                case 0xC0:

                    b = Next();
                    switch (b)
                    {
                        case 0x00:
                            Next(2);
                            break;
                        case 0x01:
                            _outStream.Write(" Corner power1: ");
                            _outStream.Write(ReadPercentage().ToString("n2") + "%");
                            break;
                        case 0x02:
                            _outStream.Write(" Work power1: ");
                            _outStream.Write(ReadPercentage().ToString("n2") + "%");
                            break;
                        case 0x03:
                            _outStream.Write(" Work power2: ");
                            _outStream.Write(ReadPercentage().ToString("n2") + "%");
                            break;
                        case 0x04:
                            _outStream.Write("{0}", BitConverter.ToNumber(Next(), Next()));
                            break;
                        case 0x05:
                            Next(2);
                            break;
                        case 0x06:
                            Next(2);
                            break;
                        case 0x07:
                            Next(2);
                            break;
                        case 0x08:
                            _outStream.Write(" Corner power2: ");
                            _outStream.Write(ReadPercentage().ToString("n2") + "%");
                            break;
                        case 0x09:
                            var a3 = Next();
                            var b3 = Next();
                            _outStream.Write("{0}", BitConverter.ToNumber(a3, b3));
                            break;
                        case 0x10:
                            PrintFloat(" Point mode, delay");
                            break;
                        case 0x11:
                            PrintFloat();
                            break;
                        default:
                            _outStream.Write("Unknown category {0:X2}", b);
                            return;
                    }
                    break;
                    
                case 0xC1:
                    Next(2);
                    break;
                case 0xC2:
                    Next(2);
                    break;
                case 0xCD:

                    b = Next();
                    switch (b)
                    {
                        case 0x00:
                            Next(2);
                            break;
                        case 0x01:
                            Next(1);
                            break;
                    }
                    break;
                case 0xD0:
                    break;
                case 0x80:
                    _outStream.Write(" Move to: ");
                    PrintFloat(" x");
                    _outStream.Write(" ");
                    PrintFloat(" y");
                    break;
                case 0x81:
                    _outStream.Write(" Carve? ");
                    Next(4);
                    break;
                case 0x82:
                    _outStream.Write(" Start laser? ");
                    Next(2);
                    break;
                case 0xA0:
                    _outStream.Write(" Line to: ");
                    PrintFloat(" x");
                    _outStream.Write(" ");
                    PrintFloat(" y");
                    break;
                case 0xA1:
                    _outStream.Write(" Short line to: ");
                    var a1 = Next();
                    var b1 = Next();
                    var c1 = Next();
                    var d1 = Next();
                    _outStream.Write("[{0},{1}],", BitConverter.ToNumber(a1, b1), BitConverter.ToNumber(c1, d1));
                    break;
                case 0xA2:
                    // Used when drawing square. Probably have some sort of direction. TODO rotate the sqare and see what happens 
                    _outStream.Write(" Horizontal line? {0}", BitConverter.ToNumber(Next(), Next()));
                    break;
                case 0xA3:
                    // Used when drawing square. Probably have some sort of direction. TODO rotate the sqare and see what happens 
                    _outStream.Write(" Vertical line? {0}", BitConverter.ToNumber(Next(), Next()));
                    break;
                default:
                    _outStream.Write("Unknown category {0:X2}", b);
                    return; 
            }
        }


        private float ReadPercentage() { return BitConverter.ToPercentage(Next(), Next()); }


        void PrintFloat(string name = "f")
        {
            _outStream.Write("{0}:{1:n2}", name, BitConverter.ToFloat(Next(5)));
        }
    }

    public static class BitConverter
    {
        public static float ToNumber(byte a, byte b)
        {
            if (a == 0)
                return b;

            int v = (b - a);
            if (a > 64) return v - (0x7F * (0x7F - a));
            if (a >= 1) return a * 0x7F + v;

            return v;
        }

        public static float ToFloat(byte[] cb)
        {
            int value =
                  (cb[0] << 7 * 4)
                + (cb[1] << 7 * 3)
                + (cb[2] << 7 * 2)
                + (cb[3] << 7)
                + cb[4];

            return value / 1000f;
        }

        public static float ToPercentage(byte a, byte b)
        {
            int l = (a * 0x7f) + b;
            int f = 0x7f << 7;
            float v = (l * 10000f) / f;
            var g = v + 0.5f;
            return ((int)g) / 100f;
        }
    }
}
 