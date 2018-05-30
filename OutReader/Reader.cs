using System;
using System.IO;

namespace LaserOutReader
{
    internal class Reader
    {
        private int _offset;
        private int _length;
        private byte[] _buffer;

        void ReadFile()
        {
            string fileName = "../../../Samples/Line-x0-y0-Line-x0-y10-w100mm-h0.tmp";
            _buffer = File.ReadAllBytes(fileName);
            Console.Write("Got {0} bytes\n", _length = _buffer.Length);
            _offset = 0;
        }

        byte Peek()
        {
            return _buffer[_offset];
        }

        byte Next()
        {
            byte b = _buffer[_offset++];
            Console.Write("{0:X2}", b);
            return b;
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
            ReadFile();

            for (byte b = 0; _offset < _buffer.Length;)
            {
                b = Next();
                if (b == 0xE3)
                {
                    b = Next();
                    switch (b)
                    {
                        case 0x02:
                            Console.Write(" <- END ");
                            Next(2);
                            break;
                        case 0x03:
                            Console.Write(" <- START ");
                            Next(2);
                            break;
                        default:
                            Console.Write("Unknown category {0:X2}", b);
                            return;
                    }
                }
                else if (b == 0xE0)
                {
                    b = Next();
                    switch (b)
                    {
                        case 0x00:
                            Next();
                            break;
                        case 0x04:
                            // something related to line position
                            Console.Write("\n  ");
                            Next(4);
                            Console.WriteLine();
                            ReadFloat("x");
                            Console.WriteLine();
                            ReadFloat("y");
                            break;
                        case 0x05:
                            Next(10);
                            break;
                        case 0x06:
                            ReadFloat();
                            break;
                        case 0x07:
                            ReadFloat("x");
                            break;
                        case 0x08:
                            ReadFloat();
                            break;
                        case 0x09:
                            ReadFloat("y");
                            break;
                        case 0x0A:
                            Next();
                            break;
                        case 0x0B:
                            Next(4);
                            break;
                        case 0x0C:
                            ReadFloat();
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
                            Console.Write("Unknown category {0:X2}", b);
                            return;
                    }
                }
                else if (b == 0xC5)
                {
                    b = Next();
                    switch (b)
                    {
                        case 0x00:
                            Next();
                            break;
                        case 0x02:
                            ReadFloat("cut speed");
                            break;
                        case 0x04:
                            ReadFloat("free speed");
                            break;
                        default:
                            Console.Write("Unknown category {0:X2}", b);
                            return;
                    }
                }
                else if (b == 0xC0)
                {
                    b = Next();
                    switch (b)
                    {
                        case 0x01:
                            Next(2);
                            break;
                        case 0x02:
                            Next(2);
                            break;
                        case 0x03:
                            Next(2);
                            break;
                        case 0x04:
                            Next(2);
                            break;
                        case 0x05:
                            Next(2);
                            break;
                        case 0x06:
                            Next(2);
                            break;
                        case 0x08:
                            Next(2);
                            break;
                        case 0x09:
                            Next(2);
                            var lb = Peek();
                            while (lb != 0xE0)
                            {
                                ReadPath();
                                lb = Peek();
                            }
                            break;
                        default:
                            Console.Write("Unknown category {0:X2}", b);
                            return;
                    }
                }
                else
                {
                    continue;
                }
                Console.WriteLine();
            }
        }

        private void ReadPath()
        {
            Console.WriteLine();
            switch (Next())
            {
                case 0x80:
                    Console.WriteLine(" <- Move to");
                    ReadFloat("x");
                    Console.WriteLine();
                    ReadFloat("y");
                    break;
                case 0x82:
                    Console.WriteLine(" <- Start laser?");
                    Console.Write("  ");
                    Next(2);
                    break;
                case 0xA0:
                    Console.WriteLine(" <- Line to");
                    ReadFloat("x");
                    Console.WriteLine();
                    ReadFloat("y");
                    break;
            }
        }

        void ReadFloat(string name = "f")
        {
            Console.Write("  " + name + ": ");
            float val = ParseFloat(Next(5));
            Console.Write(" = " + val);
        }

        private static float ParseFloat(byte[] cb)
        {
            var value = (cb[0] << 32) | (cb[1] << 24) | (cb[2] << 16) | (cb[3] << 8) | cb[4];
            // todo
            if (value == 0x01) return 0.001f;
            if (value == 0x0A) return 0.01f;
            if (value == 0x64) return 0.1f;
            if (value == 0x0768) return 1f;
            if (value == 0x4E10) return 10f;
            if (value == 0x060D20) return 100f;

            return value / 1000;
        }
    }
}