using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Utility.Helper
{
    public class Snowflake
    {
        private static readonly long Twepoch = 1288834974657L;
        private const long WorkerIdBits = 5L;
        private const long DatacenterIdBits = 5L;
        private const long MaxWorkerId = -1L ^ (-1L << (int)WorkerIdBits);
        private const long MaxDatacenterId = -1L ^ (-1L << (int)DatacenterIdBits);
        private const long SequenceBits = 12L;
        private const long WorkerIdShift = SequenceBits;
        private const long DatacenterIdShift = SequenceBits + WorkerIdBits;
        private const long TimestampLeftShift = SequenceBits + WorkerIdBits + DatacenterIdBits;
        private const long SequenceMask = -1L ^ (-1L << (int)SequenceBits);

        private static long _lastTimestamp = -1L;
        private static long _sequence = 0L;
        private static long _workerId;
        private static long _datacenterId;
        private static bool _isInitialized = false;

        private static readonly object SyncRoot = new object();

        /// <summary>
        /// 生成下一个唯一 ID。
        /// </summary>
        /// <param name="workerId">工作机器 ID。</param>
        /// <param name="datacenterId">数据中心 ID。</param>
        /// <returns>生成的唯一 ID。</returns>
        public static long NextId(long workerId = 1, long datacenterId = 1)
        {
            lock (SyncRoot)
            {
                if (!_isInitialized)
                {
                    Initialize(workerId, datacenterId);
                    _isInitialized = true;
                }

                var timestamp = TimeGen();

                if (timestamp < _lastTimestamp)
                {
                    throw new InvalidOperationException($"时钟倒退。拒绝生成 ID，直到 {_lastTimestamp - timestamp} 毫秒之后");
                }

                if (_lastTimestamp == timestamp)
                {
                    _sequence = (_sequence + 1) & SequenceMask;
                    if (_sequence == 0)
                    {
                        timestamp = TilNextMillis(_lastTimestamp);
                    }
                }
                else
                {
                    _sequence = 0L;
                }

                _lastTimestamp = timestamp;

                return ((timestamp - Twepoch) << (int)TimestampLeftShift) |
                       (_datacenterId << (int)DatacenterIdShift) |
                       (_workerId << (int)WorkerIdShift) |
                       _sequence;
            }
        }

        private static void Initialize(long workerId, long datacenterId)
        {
            if (workerId > MaxWorkerId || workerId < 0)
            {
                throw new ArgumentException($"工作机器 ID 不能大于 {MaxWorkerId} 或小于 0");
            }

            if (datacenterId > MaxDatacenterId || datacenterId < 0)
            {
                throw new ArgumentException($"数据中心 ID 不能大于 {MaxDatacenterId} 或小于 0");
            }

            _workerId = workerId;
            _datacenterId = datacenterId;
        }

        private static long TilNextMillis(long lastTimestamp)
        {
            var timestamp = TimeGen();
            while (timestamp <= lastTimestamp)
            {
                timestamp = TimeGen();
            }
            return timestamp;
        }

        private static long TimeGen()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
