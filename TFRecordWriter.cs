using System;
using System.IO;
using Google.Protobuf;

namespace Tensorflow.Hadoop.Util
{
    /* Java reference (Apache Licence 2.0) https://github.com/tensorflow/ecosystem/blob/master/hadoop/src/main/java/org/tensorflow/hadoop/util/TFRecordWriter.java
     * This C# class is a line-per-line conversion of the Java class mentioned above. Even variable names were kept the same.
     * The only differences are related to the peculiarities of the .Net Framework vs. the Java Library.
     */
    public class TFRecordWriter : IDisposable
    {
        private readonly Stream output;

        public TFRecordWriter(Stream output)
        {
            this.output = output;
        }

        public void Write(byte[] record, int offset, int length)
        {
            /* TFRecord format:
             * uint64 length
             * uint32 masked_crc32_of_length
             * byte   data[length]
             * uint32 masked_crc32_of_data
             */
            byte[] len = toInt64LE((ulong)record.Length);
            output_Write(len);
            output_Write(toInt32LE(Crc32C.maskedCrc32c(len)));
            output.Write(record, offset, length);
            output_Write(toInt32LE(Crc32C.maskedCrc32c(record)));
        }

        public void Write(byte[] record)
        {
            Write(record, 0, record.Length);
        }

        private static byte[] toInt64LE(ulong data)
        {
            byte[] buff = BitConverter.GetBytes(data);
            if (!BitConverter.IsLittleEndian) Array.Reverse(buff);
            return buff;
        }

        private static byte[] toInt32LE(uint data)
        {
            byte[] buff = BitConverter.GetBytes(data);
            if (!BitConverter.IsLittleEndian) Array.Reverse(buff);
            return buff;
        }

        // convenience method, not present in the original Java class
        public void Write(IMessage data)
        {
            Write(data.ToByteArray());
        }

        // Stream.Write(byte[]) does not exist and Stream.Write(ReadOnlySpan<byte>) makes an unnecessary copy
        private void output_Write(byte[] buff)
        {
            output.Write(buff, 0, buff.Length);
        }

        public void Dispose()
        {
            this.output.Dispose();
        }
    }
}
