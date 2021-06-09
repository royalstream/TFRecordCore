namespace Tensorflow
{
    /* Java reference (Apache Licence 2.0) https://github.com/tensorflow/ecosystem/blob/master/hadoop/src/main/java/org/tensorflow/hadoop/util/Crc32C.java
     * This C# file includes a line-per-line conversion of the maskedCrc32c method in the Java class mentioned above. 
     */
    internal static class Crc32C
    {
        private const uint MASK_DELTA = 0xa282ead8;

        internal static uint maskedCrc32c(byte[] data)
        {
            uint crc = Force.Crc32.Crc32CAlgorithm.Compute(data);
            return ((crc >> 15) | (crc << 17)) + MASK_DELTA;
        }
    }
}
