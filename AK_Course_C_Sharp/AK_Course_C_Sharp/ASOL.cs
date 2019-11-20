using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AK_Course_C_Sharp
{
    public class ASOL
    {
        public static List<string> opCodeList = new List<string>
        {
            "add", "nand", "lw", "sw", "beq", "jarl", "halt", "mul", ".fill", "sl"
        };

        public static void Exec()
        {
            string inFileString = "", outFileString = "";
            StreamReader inFilePtr;
            StreamWriter outFilePtr;

            int address;
            List<string> labelArray = new List<string>();
            List<int> labelAddress = new List<int>();

            string label = "", opcode = "", arg0 = "", arg1 = "", arg2 = "", argTmp = "";

            int i;
            int numLabels = 0;
            int num;
            int addressField;

            Console.WriteLine("Input file code path: ");
            inFileString = Console.ReadLine();

            Console.WriteLine("Input machine file path: ");
            outFileString = Console.ReadLine();

            if (!File.Exists(inFileString))
            {
                Console.WriteLine($"error: file: {inFileString} not exist\n");
                Environment.Exit(1);
            }
            if (!File.Exists(outFileString))
            {
                Console.WriteLine($"error: file: {outFileString} not exists\n");
                Environment.Exit(1);
            }
            inFilePtr = new StreamReader(inFileString);
            outFilePtr = new StreamWriter(outFileString);

            for (address = 0; ReadAndParse(inFilePtr, label, opcode, arg0, arg1, arg2); address++)
            {
                // check for illegal opcode
                if (!opCodeList.Contains(opcode))
                {
                    Console.WriteLine($"error: invalid opcode: {opcode}\n");
                    Environment.Exit(1);
                }

                // check register fields
                if (opcode == "add" || opcode == "nand" || opcode == "lw" ||
                    opcode == "sw" || opcode == "beq" || opcode == "jarl" ||
                    opcode == "mul" || opcode == "sl")
                {
                    testRegArg(arg0);
                    testRegArg(arg1);
                }
                if (opcode == "add" || opcode == "nand" || opcode == "mul" ||
                    opcode == "sl")
                {
                    testRegArg(arg2);
                }

                // checkAddressFieldId
                if(opcode == "lw" || opcode == "sw" || opcode == "beq")
                {
                    testAddrArg(arg2);
                }
                if(opcode == ".fill")
                {
                    testAddrArg(arg0);
                }

                // check for enough arguments
                if ((opcode != "halt" && opcode != ".fill" && opcode != "jarl" && arg2[0] != '\0')
                    || (opcode == "jarl" && arg1[0] == '\0') || (opcode == ".fill" && arg0[0] == '\0'))
                {
                    Console.WriteLine($"error: at address: {address} not enough arguments");
                    Environment.Exit(1);
                }

                if (label[0] != '\0')
                {
                    // make sure label starts with letter
                    if (!Char.IsLetter(label[0]))
                    {
                        Console.WriteLine($"error: label: {label} doesnt start with letter\n");
                        Environment.Exit(1);
                    }

                    // Make sure label consists only from letters and numbers
                    if (!label.All(item => Char.IsLetterOrDigit(item)))
                    {
                        Console.WriteLine($"error: label: {label} has character other than letters and numbers\n");
                        Environment.Exit(1);
                    }

                    // look for duplicate label
                    for(i = 0; i < numLabels; i++)
                    {
                        if(label == labelArray[i])
                        {
                            Console.WriteLine($"error: duplicate label: {label} at address: {address}");
                            Environment.Exit(1);
                        }
                    }

                    labelArray[numLabels] = label;
                    labelAddress[numLabels++] = address;
                }
            }

            // print machine code
            inFilePtr = new StreamReader(inFileString);
            for(address = 0; ReadAndParse(inFilePtr, label, opcode, arg0, arg1, arg2); address++)
            {

            }

        }

        public static bool ReadAndParse(StreamReader streamReader, string label, string opcode, string arg0, string arg1, string arg2)
        {
            string line;
            string ptr = "";

            // check if stream is not empty
            if (streamReader.EndOfStream) return false;
            else line = streamReader.ReadLine();

            // is there a label
            var local = line.Split(' ');

            // set label
            label = (local[0] != null) ? local[0] : "";
            opcode = (local[0] != null) ? local[0] : "";
            arg0 = (local[0] != null) ? local[0] : "";
            arg1 = (local[0] != null) ? local[0] : "";
            arg2 = (local[0] != null) ? local[0] : "";

            return true;
        }

        public static int isNumber(string str)
        {
            /* return 1 if string is a number */
            int i;
            if (Int32.TryParse(str, out var loc)) return 1;
            else return 0;
        }

        public static void testRegArg(string arg)
        {
            int num;
            char c;

            int argNum;
            bool isRight = Int32.TryParse(arg, out argNum);

            if (!isRight)
            {
                Console.WriteLine("error: Incorect arg\n");
                Environment.Exit(1);
            }

            if(argNum < 0 || argNum > 7)
            {
                Console.WriteLine("error: register out of range\n");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Test addressField argument.
        /// </summary>
        /// <param name="arg"></param>
        public static void testAddrArg(string arg)
        {
            // test numeric addressField
            int num;
            bool isRight = Int32.TryParse(arg, out num);

            if (!isRight)
            {
                // check if it consists only from letters
                var isLetterOnly = arg.All(i => Char.IsLetter(i));

                if (!isLetterOnly)
                {
                    Console.WriteLine("error: bad character in addressField\n");
                    Environment.Exit(1);
                }
            }
        }

        public static int transalateSymbol(List<string> labelArray, List<int> labelAddress, int numLabels, string symbol)
        {
            int i;

            // search through address label table
            for(i = 0; (i < numLabels) && symbol != labelArray[i]; i++)
            {

            }

            if(i > numLabels)
            {
                Console.WriteLine($"error: missing label {symbol}\n");
                Environment.Exit(1);
            }

            return (labelAddress[i]);
        }
    }
}
