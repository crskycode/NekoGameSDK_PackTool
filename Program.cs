using System;
using System.IO;
using System.Text;

namespace NekoGameSDK_PackTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (args.Length != 3)
            {
                Console.WriteLine("NekoGameSDK Pack Tool");
                Console.WriteLine("Usage:");
                Console.WriteLine("  Create PAK archive : NekoGameSDK_PackTool -c [output.arc] [root directory path]");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            var mode = args[0];
            var outputPath = Path.GetFullPath(args[1]);
            var rootPath = Path.GetFullPath(args[2]);

            if (mode == "-c")
            {
                try
                {
                    NekoPack4A.Create(outputPath, rootPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }
        }
    }
}
