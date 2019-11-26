using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace AK_Course_C_Sharp
{

    public class ASOL
    {
        /// <summary>
        /// max program size
        /// </summary>
        public static int MaxNumLabels = 16777216;

        /// <summary>
        /// number of registers
        /// </summary>
        public static int RegNumbers = 64;

        /// <summary>
        /// length of bus
        /// </summary>
        public static int BusLength = 48;

        /// <summary>
        /// min and max address field
        /// </summary>
        public static int MinAddressField = -8388608, MaxAddressField = 8388607;

        /// <summary>
        /// key word
        /// </summary>
        public enum KeyWord
        {
            ADD = 0,
            NAND = 1,
            LW = 2,
            SW = 3,
            BEQ = 4,
            JARL = 5,
            HALT = 6,
            MUL = 7,
            XADD = 8,
            XIDIV = 9,
            XSUB = 10,
            XOR = 11,
            CMPE = 12,
            SAR = 13,
            JMA = 14,
            JML = 15,
            ADC = 16,
            SBB = 17,
            RCR = 18
        }

        public static List<string> opCodeList = new List<string>
        {
            "add", "nand", "lw", "sw", "beq", "jarl", "halt", "mul", ".fill", "sl", "xadd", "xidiv", "xsub", "xor", "cmpe", "sar", "jma", "jml", "adc", "sbb", "rcr"
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
            BigInteger num = 0;
            BigInteger addressField = 0;

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
                    return;
                }

                // check register fields
                List<string> testArg0Arg1 = new List<string>
                {
                    "add", "nand", "lw", "sw", "beq", "jarl", "mul", "sl", "xadd",
                    "xidiv", "xsub", "xor", "cmpe", "sar", "jma", "jml" ,"adc", "sbb", "rcr"
                };
                if (testArg0Arg1.Contains(opcode))
                {
                    testRegArg(arg0);
                    testRegArg(arg1);
                }
                List<string> testArg2 = new List<string>
                {
                    "add", "nand", "mul", "sl", "xadd",
                    "xidiv", "xsub", "xor", "cmpe", "sar" ,"adc", "sbb", "rcr"
                };
                if (testArg2.Contains(opcode))
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
                if ((opcode != "halt" && opcode != ".fill" && opcode != "jarl" 
                    &&  opcode != "jma" && opcode != "jml" && arg2 == "")
                    || (opcode == "jarl" && arg1 == "") || (opcode == ".fill" && arg0 == ""))
                {
                    Console.WriteLine($"error: at address: {address} not enough arguments");
                    return;
                }

                if (label != "" )
                {
                    // make sure label starts with letter
                    if (!Char.IsLetter(label[0]))
                    {
                        Console.WriteLine($"error: label: {label} doesnt start with letter\n");
                        return;
                    }

                    // Make sure label consists only from letters and numbers
                    if (!label.All(item => Char.IsLetterOrDigit(item)))
                    {
                        Console.WriteLine($"error: label: {label} has character other than letters and numbers\n");
                        return;
                    }

                    // look for duplicate label
                    for(i = 0; i < numLabels; i++)
                    {
                        if(label == labelArray[i])
                        {
                            Console.WriteLine($"error: duplicate label: {label} at address: {address}");
                            return;
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
                    num = ((Parse(KeyWord.ADD) << 36) | (Int64.Parse(arg0) << 30) | (Int64.Parse(arg1) << 24) | Int64.Parse(arg2));
                }
                else if(opcode == "nand")
                {
                    num = ((Parse(KeyWord.NAND) << 36) | (Int64.Parse(arg0) << 30) | (Int64.Parse(arg1) << 24) | Int64.Parse(arg2));
                }
                else if (opcode == "xadd")
                {
                    num = ((Parse(KeyWord.XADD) << 36) | (Int64.Parse(arg0) << 30) | (Int64.Parse(arg1) << 24) | Int64.Parse(arg2));
                }
                else if (opcode == "xidiv")
                {
                    num = ((Parse(KeyWord.XIDIV) << 36) | (Int64.Parse(arg0) << 30) | (Int64.Parse(arg1) << 24) | Int64.Parse(arg2));
                }
                else if (opcode == "xsub")
                {
                    num = ((Parse(KeyWord.XSUB) << 36) | (Int64.Parse(arg0) << 30) | (Int64.Parse(arg1) << 24) | Int64.Parse(arg2));
                }
                else if (opcode == "xor")
                {
                    num = ((Parse(KeyWord.XOR) << 36) | (Int64.Parse(arg0) << 30) | (Int64.Parse(arg1) << 24) | Int64.Parse(arg2));
                }
                else if (opcode == "cmpe")
                {
                    num = ((Parse(KeyWord.CMPE) << 36) | (Int64.Parse(arg0) << 30) | (Int64.Parse(arg1) << 24) | Int64.Parse(arg2));
                }
                else if (opcode == "sar")
                {
                    num = ((Parse(KeyWord.SAR) << 36) | (Int64.Parse(arg0) << 30) | (Int64.Parse(arg1) << 24) | Int64.Parse(arg2));
                }
                else if (opcode == "adc")
                {
                    num = ((Parse(KeyWord.ADC) << 36) | (Int64.Parse(arg0) << 30) | (Int64.Parse(arg1) << 24) | Int64.Parse(arg2));
                }
                else if (opcode == "sbb")
                {
                    num = ((Parse(KeyWord.SBB) << 36) | (Int64.Parse(arg0) << 30) | (Int64.Parse(arg1) << 24) | Int64.Parse(arg2));
                }
                else if (opcode == "rcr")
                {
                    num = ((Parse(KeyWord.RCR) << 36) | (Int64.Parse(arg0) << 30) | (Int64.Parse(arg1) << 24) | Int64.Parse(arg2));
                }
                else if(opcode == "jarl")
                {
                    num = (Parse(KeyWord.JARL) << 36) | Int64.Parse(arg0) << 30 | Int64.Parse(arg1) << 24;
                }
                else if(opcode == "halt")
                {
                   BigInteger loc = Parse(KeyWord.HALT);
                   num = (loc << 36);
                }
                else if(opcode == "mul")
                {
                    num = ((Parse(KeyWord.MUL) << 36) | (Int64.Parse(arg0) << 30) | (Int64.Parse(arg1) << 24) | Int64.Parse(arg2));
                }

                else if(opcode == "lw" || opcode == "sw" || opcode == "beq" || opcode == "jma" || opcode == "jml")
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
                    else addressField = Int64.Parse(arg2);

                    if(addressField < MinAddressField || addressField > MaxAddressField)
                    {
                        Console.WriteLine($"error: offset {addressField} out of range\n");
                        return;
                    }

                    // truncate the offset field, in case its negative
                    addressField = addressField & 0xFFFF;

                    if(opcode == "beq")
                    {
                        num = (Parse(KeyWord.BEQ) << 36) | (Int64.Parse(arg0) << 30) | (Int64.Parse(arg1) << 24) 
                            | addressField;
                    }
                    else
                    {
                        // lw or sw
                        if(opcode == "lw")
                        {
                            num = (Parse(KeyWord.LW) << 36) | (Int64.Parse(arg0) << 30) |
                                (Int64.Parse(arg1) << 24) | addressField;
                        }
                        else
                        {
                            num = (Parse(KeyWord.SW) << 36) | (Int64.Parse(arg0) << 30) |
                                (Int64.Parse(arg1) << 24) | addressField;
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
                        num = Int64.Parse(arg0);
                    }
                }
                //File.AppendAllText(outFileString, num.ToString() + "\n");
                
                outFilePtr.WriteLine(num.ToString());
            }
            //outFilePtr.WriteLine("proposal");
            outFilePtr.Close();

            return;

        }

        public static Int64 Parse(KeyWord keyWord)
        {
            return (Int64)keyWord;
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
                throw new Exception("error: Incorect arg\n");
            }

            if(argNum < 0 || argNum >= RegNumbers)
            {
                Console.WriteLine("error: register out of range\n");
                throw new Exception("error: register out of range\n");
            }
        }

        /// <summary>
        /// Test addressField argument.
        /// </summary>
        /// <param name="arg"></param>
        public static void testAddrArg(string arg)
        {
            // test numeric addressField
            Int64 num;
            bool isRight = Int64.TryParse(arg, out num);

            if (!isRight)
            {
                // check if it consists only from letters
                var isLetterOnly = arg.All(i => Char.IsLetter(i));

                if (!isLetterOnly)
                {
                    Console.WriteLine("error: bad character in addressField\n");
                    return;
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
                throw new Exception($"error: missing label {symbol}\n");
            }

            return (labelAddress[i]);
        }
    }
}
