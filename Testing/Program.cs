using System;
using System.IO;
using Tensorflow;
using Tensorflow.Hadoop.Util;

class Program
{
    static void Main(string[] args)
    {
        var example = new Example();
        example.Features = new Features();
        var feat1 = new Feature();
        feat1.Int64List = new Int64List();
        feat1.Int64List.Value.Add(1L);
        feat1.Int64List.Value.Add(2L);
        example.Features.Feature.Add("feat", feat1);
        var feat2 = new Feature();
        feat2.FloatList = new FloatList();
        feat2.FloatList.Value.Add(0.1f);
        feat2.FloatList.Value.Add(0.2f);
        example.Features.Feature.Add("feat2", feat2);

        // Write
        Console.WriteLine("Written to file:");
        Console.WriteLine(example);
        using (var tw = new TFRecordWriter(File.Create("testing.tfrecord"))) tw.Write(example);

        // Read
        byte[] readbytes;
        using (var tr = new TFRecordReader(File.OpenRead("testing.tfrecord"), true)) readbytes = tr.Read();
        var readexample = Example.Parser.ParseFrom(readbytes);
        Console.WriteLine("Read from file:");
        Console.WriteLine(readexample);
    }
}
