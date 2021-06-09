using System;
using System.Diagnostics;
using System.IO;

namespace Tensorflow
{
    /* Java reference (Apache Licence 2.0) https://github.com/tensorflow/ecosystem/blob/master/hadoop/src/main/java/org/tensorflow/hadoop/util/TFRecordReader.java
     * This C# class is a line-per-line conversion of the Java class mentioned above. Even variable names were kept the same.
     * The only differences are related to the peculiarities of the .Net Framework vs. the Java Library.
     */
    public class TFRecordReader : IDisposable
    {
        private readonly Stream input;
        private readonly bool crcCheck;

        public TFRecordReader(Stream input, bool crcCheck)
        {
            this.input = input;
            this.crcCheck = crcCheck;
        }

        public byte[] Read()
        {
            /* TFRecord format:
             * uint64 length
             * uint32 masked_crc32_of_length
             * byte   data[length]
             * uint32 masked_crc32_of_data
             */
            byte[] lenBytes = new byte[8];
            try
            {
                readFully(input, lenBytes);  // according to the Java implementation return null means EOF
            }
            catch
            {
                return null;
            }
            ulong len = fromInt64LE(lenBytes);

            // Verify length crc32
            byte[] lenCrc32Bytes = new byte[4];
            readFully(input, lenCrc32Bytes);
            if (crcCheck)
            {
                uint lenCrc32 = fromInt32LE(lenCrc32Bytes);
                if (lenCrc32 != Crc32C.maskedCrc32c(lenBytes))
                    throw new IOException("Length header crc32 checking failed: " + lenCrc32 + " != " + Crc32C.maskedCrc32c(lenBytes) + ", length = " + len);
            }

            if (len > UInt32.MaxValue) throw new IOException("Record size exceeds max value of UInt32: " + len);
            byte[] data = new byte[len];
            readFully(input, data);

            // Verify data crc32
            byte[] dataCrc32Bytes = new byte[4];
            readFully(input, dataCrc32Bytes);
            if (crcCheck)
            {
                uint dataCrc32 = fromInt32LE(dataCrc32Bytes);
                if (dataCrc32 != Crc32C.maskedCrc32c(data))
                    throw new IOException("Data crc32 checking failed: " + dataCrc32 + " != " + Crc32C.maskedCrc32c(data));
            }
            return data;
        }

        private static ulong fromInt64LE(byte[] data)
        {
            Debug.Assert(data.Length == 8);
            if (!BitConverter.IsLittleEndian) data = reverseCopy(data);
            return BitConverter.ToUInt64(data);
        }

        private static uint fromInt32LE(byte[] data)
        {
            Debug.Assert(data.Length == 4);
            if (!BitConverter.IsLittleEndian) data = reverseCopy(data);
            return BitConverter.ToUInt32(data);
        }

        private static void readFully(Stream inp, byte[] buffer)
        {
            int nbytes;
            for (int nread = 0; nread < buffer.Length; nread += nbytes)
            {
                nbytes = inp.Read(buffer, nread, buffer.Length - nread);
                if (nbytes < 0) throw new EndOfStreamException("End of file reached before reading fully.");
            }
        }

        // we don't want to call Array.Reverse on the function argument
        private static byte[] reverseCopy(byte[] data)
        {
            data = (byte[])data.Clone();
            Array.Reverse(data);
            return data;
        }

        public void Dispose()
        {
            input.Dispose();
        }
    }
}
