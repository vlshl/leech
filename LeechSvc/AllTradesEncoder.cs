using System;
using System.Linq;

namespace LeechSvc
{
    /// <summary>
    /// Кодировщик информации по сделкам.
    /// Кодирует информацию по сделке в поток байтов.
    /// Используется для одного фин. инструмента.
    /// </summary>
    public class AllTradesEncoder
    {
        private int _decimals;
        private uint _curSecond;
        private int _curPrice;
        private byte[] _buffer;
        private byte _buffer_count;
        private long _t0;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="decimals">Кол-во знаков после запятой в цене фин. инструмента</param>
        public AllTradesEncoder(int decimals)
	    {
		    _decimals = decimals;
		    _curSecond = 0;
		    _curPrice = 0;
            _buffer = new byte[32];
            _t0 = new DateTime(2000, 1, 1).Ticks;
        }

        /// <summary>
        /// Кодирует информацию о сделке в массив байт
        /// </summary>
        /// <param name="ts">Дата и время сделки</param>
        /// <param name="price">Цена</param>
        /// <param name="lots">Кол-во лотов</param>
        /// <returns>Массив байт с кодированной информацией</returns>
        public byte[] AddTick(DateTime ts, decimal price, int lots)
        {
            long s = (ts.Ticks - _t0) / TimeSpan.TicksPerSecond;
            if (s < 0) s = 0; if (s > uint.MaxValue) s = uint.MaxValue;
            uint second = (uint)s;

            _buffer_count = 0;
            uint diffSec = (uint)(second - _curSecond);
            EncodeTime(second, diffSec);
            _curSecond = second;

            int k = 1;
            switch (_decimals)
            {
                case 0: k = 1; break;
                case 1: k = 10; break;
                case 2: k = 100; break;
                case 3: k = 1000; break;
                case 4: k = 10000; break;
                case 5: k = 100000; break;
                case 6: k = 1000000; break;
                case 7: k = 10000000; break;
                case 8: k = 100000000; break;
                case 9: k = 1000000000; break;
                default: break;
            }

            int p = (int)Math.Round(price * k);
            int dp = p - _curPrice;
            EncodePrice(p, dp);
            _curPrice = p;

            EncodeLots(lots);

            return _buffer.Take(_buffer_count).ToArray();
        }

        private void EncodePrice(int p, int dp)
        {
            if (dp >= -127 && dp <= 127)
            {
                byte c;
                if (dp >= 0)
                {
                    c = (byte)dp;
                }
                else
                {
                    c = (byte)(-dp - 1 + 0x80);
                }
                _buffer[_buffer_count++] = c;
            }
            else
            {
                _buffer[_buffer_count++] = 0xff;
                var bytes = BitConverter.GetBytes(p);
                _buffer[_buffer_count++] = bytes[0];
                _buffer[_buffer_count++] = bytes[1];
                _buffer[_buffer_count++] = bytes[2];
                _buffer[_buffer_count++] = bytes[3];
            }
        }

        private void EncodeTime(uint s, uint ds)
        {
            if (ds <= 254)
            {
                _buffer[_buffer_count++] = (byte)ds;
            }
            else
            {
                _buffer[_buffer_count++] = 0xff;
                var bytes = BitConverter.GetBytes(s);
                _buffer[_buffer_count++] = bytes[0];
                _buffer[_buffer_count++] = bytes[1];
                _buffer[_buffer_count++] = bytes[2];
                _buffer[_buffer_count++] = bytes[3];
            }
        }

        private void EncodeLots(int lots)
        {
            if (lots >= 0 && lots <= 254)
            {
                _buffer[_buffer_count++] = (byte)lots;
            }
            else
            {
                _buffer[_buffer_count++] = 0xff;
                var bytes = BitConverter.GetBytes(lots);
                _buffer[_buffer_count++] = bytes[0];
                _buffer[_buffer_count++] = bytes[1];
                _buffer[_buffer_count++] = bytes[2];
                _buffer[_buffer_count++] = bytes[3];
            }
        }
    }
}

