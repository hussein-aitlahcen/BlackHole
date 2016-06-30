using System.IO;
using System.Text;

namespace BlackHole.PayloadBuilder
{
    class Program
    {
        private const int MAX_J = 20;

        static void Main(string[] args)
        {
            var directory = Path.GetFullPath(@"..\..\..\BlackHole.Slave\bin\Debug");
            var exe = Path.Combine(directory, "BlackHole.Slave.exe");

            var payload = File.ReadAllBytes(exe);

            var output = new StringBuilder();
            output.AppendLine("#include \"stdafx.h\"");
            output.AppendLine("class Payload");
            output.AppendLine("{");
            output.AppendLine("public:");
            output.AppendLine($"\tstatic const unsigned long RawSize = {payload.Length};");
            output.AppendLine("};");
            output.AppendLine("");
            output.AppendLine("static unsigned char RawData[] = ");
            output.AppendLine("{");
            output.Append("\t\t");

            int j = 0;
            for (int i = 0; i < payload.Length; i++)
            {
                output.Append($"0x{payload[i].ToString("X2")}");
                if (i < payload.Length - 1)
                    output.Append(", ");
                j++;
                if(j == MAX_J)
                {
                    output.AppendLine();
                    output.Append("\t\t");
                    j = 0;
                }
            }
            output.AppendLine();
            output.AppendLine("};");

            File.WriteAllText(@"..\..\..\BlackHole.Loader\Payload.h", output.ToString());
        }
    }
}
