using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;

using renjuzhihui.shiyu.barcode;

namespace console_example.AIMHanXinCode.Net.CSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            string input_data_file = "";
            string output_image_file = "hanxincode.png";
            int version = 1;
            int ECL = 2;

            int ii;

            if ((args.Length < 1) || (Array.IndexOf<string>(args, "-h") >= 0) || (Array.IndexOf<string>(args, "--help") >= 0))
            {
                print_help_message();

                return;
            }

            input_data_file = args[0];

            //parse input parameters
            for (ii = 1;ii < args.Length;++ii)
            {
                if (args[ii].StartsWith("--out="))
                {
                    output_image_file = args[ii].Substring("--out=".Length);
                }
                else if (args[ii].StartsWith("--ecl="))
                {
                    if (!int.TryParse(args[ii].Substring("--ecl=".Length), out ECL))
                    {
                        ECL = 2;
                    }
                }
                else if (args[ii].StartsWith("--version="))
                {
                    if (!int.TryParse(args[ii].Substring("--version=".Length), out version))
                    {
                        version = 1;
                    }
                }
            }
            
            //check input file exists
            if (!File.Exists(input_data_file))
            {
                Console.WriteLine("The input file \"" + input_data_file + "\" does not exist.");

                return;
            }
            //and output is a support image file type
            FileInfo outInfo = new FileInfo(output_image_file);
            switch (outInfo.Extension.ToLower())
            {
                case ".bmp":
                case "jpg":
                case ".jpeg":
                case ".png":
                case ".tif":
                case ".tiff":
                    break;
                default:
                    Console.WriteLine("The output file \"" + output_image_file + "\" is not a supported image file.");
                    return;
            }

            //Read input Data
            string data = File.ReadAllText(input_data_file);

            //Han Xin encode
            byte[,] symbol_matrix = HanXinCode.EncodeFromCommonData(data, ref version, ref ECL);

            if (null == symbol_matrix)
            {
                Console.WriteLine("The encoding process of Han Xin failed!");

                return;
            }

            Console.WriteLine("The encoding process of Han Xin succeed:");
            Console.WriteLine("\tversion={0:D}", version);
            Console.WriteLine("\tECL={0:D}", ECL);

            //BarcodeTools to bitmap
            Bitmap bitmap_result = BarcodeTools.barcode_bitmap(symbol_matrix);
            if (null == bitmap_result)
            {
                Console.WriteLine("The image construction process failed!");

                return;
            }

            bitmap_result.Save(output_image_file);

            Console.WriteLine("Succeed.");
        }

        static string GetApplicationName()
        {
            string app_name = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            Uri app_uri = new Uri(app_name);

            return app_uri.Segments[app_uri.Segments.Length - 1];
        }

        

        static void print_help_message()
        {
            Console.WriteLine(GetApplicationName() + " \"input text file path\"" + " --out=\"output image file path\"" + " [--ecl=1 to 4]" + " [--version=1 to 84]");
            Console.WriteLine("\t" + "input text file path : the input text file, whose data will be encoded in Han Xin Code symbol.");
            Console.WriteLine("\t" + "--out=\"output image file path\" : the output Han Xin image file, it can be png, bmp, jpg, jpeg.");
            Console.WriteLine("\t" + "[--ecl=1 to 4] : optional parameter to set the user chosen error correction level of Han Xin Code, L1 to L4.");
            Console.WriteLine("\t" + "[--version=1 to 84] : optional parameter to set the user chosen version of Han Xin Code, 1 to 84.");
        }
    }
}
