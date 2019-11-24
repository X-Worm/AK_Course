using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xceed.Wpf.Toolkit;

namespace AK_Course_C_Sharp
{
    public class ASOL
    {
        public enum KeyWord
        {
            ADD = 0,
            NAND,
            LW,
            SW,
            BEQ,
            JARL,
            HALT,
            MUL
        }

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
            int num = 0;
            int addressField = 0;

            Console.Write("Input parent directory for .as and .mc files: ");
            string fileDirectory = "";
            fileDirectory = Console.ReadLine();

            Console.Write("Input name for code file: ");
            string codeFilePath = Console.ReadLine();

            try
            {
                inFileString = Path.Combine(fileDirectory, codeFilePath);
                Console.WriteLine($"File code path: {inFileString}");
                Directory.CreateDirectory(fileDirectory);

                if (!File.Exists(inFileString))
                    throw new Exception("File not exists");
            } catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            try
            {
                outFileString = fileDirectory + "\\" + "machineCode" + ".mc";
                Console.WriteLine($"Machine file path: {outFileString}");
                Directory.CreateDirectory(fileDirectory);
                var localFile = File.Create(outFileString);
                localFile.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Thread.Sleep(5000); Environment.Exit(1);
            }

            if (!File.Exists(inFileString))
            {
                Console.WriteLine($"error: file: {inFileString} not exist\n");
                Thread.Sleep(2000); Environment.Exit(1);
            }
            if (!File.Exists(outFileString))
            {
                Console.WriteLine($"error: file: {outFileString} not exists\n");
                Thread.Sleep(2000); Environment.Exit(1);
            }
            inFilePtr = new StreamReader(inFileString);
            outFilePtr = new StreamWriter(outFileString);

            for (address = 0; ReadAndParse(inFilePtr,ref label,ref opcode,ref arg0,ref arg1,ref arg2); address++)
            {
                // check for illegal opcode
                if (!opCodeList.Contains(opcode))
                {
                    Console.WriteLine($"error: invalid opcode: {opcode}\n");
                    Thread.Sleep(2000); Environment.Exit(1);
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
                if ((opcode != "halt" && opcode != ".fill" && opcode != "jarl" && arg2[0] == '\0')
                    || (opcode == "jarl" && arg1[0] == '\0') || (opcode == ".fill" && arg0[0] == '\0'))
                {
                    Console.WriteLine($"error: at address: {address} not enough arguments");
                    Thread.Sleep(2000); Environment.Exit(1);
                }

                if (label != "" )
                {
                    // make sure label starts with letter
                    if (!Char.IsLetter(label[0]))
                    {
                        Console.WriteLine($"error: label: {label} doesnt start with letter\n");
                        Thread.Sleep(2000); Environment.Exit(1);
                    }

                    // Make sure label consists only from letters and numbers
                    if (!label.All(item => Char.IsLetterOrDigit(item)))
                    {
                        Console.WriteLine($"error: label: {label} has character other than letters and numbers\n");
                        Thread.Sleep(2000); Environment.Exit(1);
                    }

                    // look for duplicate label
                    for(i = 0; i < numLabels; i++)
                    {
                        if(label == labelArray[i])
                        {
                            Console.WriteLine($"error: duplicate label: {label} at address: {address}");
                            Thread.Sleep(2000); Environment.Exit(1);
                        }
                    }

                    labelArray.Add(label);
                    labelAddress.Add(address);
                }
            }

            // print machine code

            // set stream
            inFilePtr = new StreamReader(inFileString);
            for(address = 0; ReadAndParse(inFilePtr,ref label,ref opcode,ref arg0,ref arg1,ref arg2); address++)
            {
                if(opcode == "add")
                {
                    num = ((Parse(KeyWord.ADD) << 22) | (Int32.Parse(arg0) << 19) | (Int32.Parse(arg1) << 16) | Int32.Parse(arg2));
                }
                else if(opcode == "nand")
                {
                    num = ((Parse(KeyWord.NAND) >> 22) | (Int32.Parse(arg0) << 19) | (Int32.Parse(arg1) << 16) | Int32.Parse(arg2));
                }
                else if(opcode == "jarl")
                {
                    num = (Parse(KeyWord.JARL) << 22) | Int32.Parse(arg0) << 19 | Int32.Parse(arg1) << 16;
                }
                else if(opcode == "halt")
                {
                    num = Parse(KeyWord.HALT) << 22;
                }
                else if(opcode == "mul")
                {
                    num = ((Parse(KeyWord.MUL) << 22) | (Int32.Parse(arg0) << 19) | (Int32.Parse(arg1) << 16) | Int32.Parse(arg2));
                }

                else if(opcode == "lw" || opcode == "sw" || opcode == "beq")
                {
                    // if arg2 is symbolic then translate into an address
                    if (arg2.All(item => Char.IsLetter(item)))
                    {
                        addressField = transalateSymbol(labelArray, labelAddress, numLabels, arg2);

                        if (opcode == "beq")
                        {
                            addressField = addressField - address - 1;
                        }
                    }
                    else addressField = Int32.Parse(arg2);

                    if(addressField < -32768 || addressField > 32767)
                    {
                        Console.WriteLine($"error: offset {addressField} out of range\n");
                        Thread.Sleep(2000); Environment.Exit(1);
                    }

                    // truncate the offset field, in case its negative
                    addressField = addressField & 0xFFFF;

                    if(opcode == "beq")
                    {
                        num = (Parse(KeyWord.BEQ) << 22) | (Int32.Parse(arg0) << 19) | (Int32.Parse(arg1) << 16) 
                            | addressField;
                    }
                    else
                    {
                        // lw or sw
                        if(opcode == "lw")
                        {
                            num = (Parse(KeyWord.LW) << 22) | (Int32.Parse(arg0) << 19) |
                                (Int32.Parse(arg1) << 16) | addressField;
                        }
                        else
                        {
                            num = (Parse(KeyWord.SW) << 22) | (Int32.Parse(arg0) << 19) |
                                (Int32.Parse(arg1) << 16) | addressField;
                        }
                    }
                }
                else if(opcode == ".fill")
                {
                    if(arg0.All(item => Char.IsLetter(item)))
                    {
                        num = transalateSymbol(labelArray, labelAddress, numLabels, arg0);
                    }
                    else
                    {
                        num = Int32.Parse(arg0);
                    }
                }
                //File.AppendAllText(outFileString, num.ToString() + "\n");
                
                outFilePtr.WriteLine(num.ToString());
            }
            //outFilePtr.WriteLine("proposal");
            outFilePtr.Close();
            
            Thread.Sleep(2000); Environment.Exit(1);

        }

        public static int Parse(KeyWord keyWord)
        {
            return (int)keyWord;
        }

        public static bool ReadAndParse(StreamReader streamReader,ref string label,ref string opcode,ref string arg0,ref string arg1,ref string arg2)
        {
            string line;
            string ptr = "";

            // check if stream is not empty
            if (streamReader.EndOfStream) return false;
            else line = streamReader.ReadLine();

            // is there a label
            var local = line.Split(' ');

            // set label
            label = (local.Length >= 1 && local[0] != null) ? local[0] : "";
            opcode = (local.Length >= 2 && local[1] != null) ? local[1] : "";
            arg0 = (local.Length >= 3 && local[2] != null) ? local[2] : "";
            arg1 = (local.Length >= 4 && local[3] != null) ? local[3] : "";
            arg2 = (local.Length >= 5 && local[4] != null) ? local[4] : "";

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
                Thread.Sleep(2000); Environment.Exit(1);
            }

            if(argNum < 0 || argNum > 7)
            {
                Console.WriteLine("error: register out of range\n");
                Thread.Sleep(2000); Environment.Exit(1);
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
                    Thread.Sleep(2000); Environment.Exit(1);
                }
            }
        }

        public static int transalateSymbol(List<string> labelArray, List<int> labelAddress, int numLabels, string symbol)
        {
            int i;
            numLabels = labelArray.Count();
            // search through address label table
            for(i = 0; i < numLabels; i++)
            {
                if (symbol == labelArray[i]) break;
            }

            if(i > numLabels)
            {
                Console.WriteLine($"error: missing label {symbol}\n");
                Thread.Sleep(2000); Environment.Exit(1);
            }

            return (labelAddress[i]);
        }
    }
}
