using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IntXLib;


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
            RCR = 18,
            CLCF = 19
        }

        public static List<string> opCodeList = new List<string>
        {
            "add", "nand", "lw", "sw", "beq", "jarl", "halt", "mul", ".fill", "sl", "xadd", "xidiv", "xsub", "xor", "cmpe", "sar", "jma", "jml", "adc", "sbb", "rcr"
        };

        public static void Exec(string codePath, ref string outFileName)
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



            try
            {
                inFileString = codePath;

                if (!File.Exists(inFileString))
                    throw new Exception("File not exists");
            } catch(Exception ex)
            {
                throw new Exception(ex.Message);
                return;
            }

            try
            {
                string fileName = Path.GetFileNameWithoutExtension(codePath);
                string fileDirectory = Path.GetDirectoryName(codePath);
                outFileString = fileDirectory + "\\" + fileName + ".mc";
                var localFile = File.Create(outFileString);
                localFile.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
                return;
            }

            if (!File.Exists(inFileString))
            {
               throw new Exception($"error: file: {inFileString} not exist\n");
                return;
            }
            if (!File.Exists(outFileString))
            {
                throw new Exception($"error: file: {outFileString} not exists\n");
                return;
            }
            inFilePtr = new StreamReader(inFileString);
            outFilePtr = new StreamWriter(outFileString);

            try
            {
                for (address = 0; ReadAndParse(inFilePtr, ref label, ref opcode, ref arg0, ref arg1, ref arg2); address++)
                {
                    if (label == "" && opcode == "" && arg0 == "" && arg1 == "" && arg2 == "")
                        break;

                    // check for illegal opcode
                    if (!opCodeList.Contains(opcode))
                    {
                        throw new Exception($"error: invalid opcode: {opcode}");
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
                    "add", "nand", "mul", "sl", 
                    "xor", "cmpe", "sar" ,"adc", "sbb", "rcr"
                };
                    if (testArg2.Contains(opcode))
                    {
                        testRegArg(arg2);
                    }

                    // checkAddressFieldId
                    if (opcode == "lw" || opcode == "sw" || opcode == "beq")
                    {
                        testAddrArg(arg2);
                    }
                    if (opcode == ".fill")
                    {
                        testAddrArg(arg0);
                    }
                    if(opcode == "xadd" || opcode == "xidiv" || opcode == "xsub")
                    {
                        Int64 localArg2 = Convert.ToInt64(arg2);
                        if (localArg2 > MaxNumLabels)
                        {
                            throw new Exception($"overflow memmory address: {localArg2}");
                        }
                    }

                    // check for enough arguments
                    if ((opcode != "halt" && opcode != ".fill" && opcode != "jarl"
                        && opcode != "jma" && opcode != "jml" && arg2 == "")
                        || (opcode == "jarl" && arg1 == "") || (opcode == ".fill" && arg0 == ""))
                    {
                        throw new Exception($"error: at address: {address} not enough arguments");
                        return;
                    }

                    if (label != "")
                    {
                        // make sure label starts with letter
                        if (!Char.IsLetter(label[0]))
                        {
                            throw new Exception($"error: label: {label} doesnt start with letter\n");
                            return;
                        }

                        // Make sure label consists only from letters and numbers
                        if (!label.All(item => Char.IsLetterOrDigit(item)))
                        {
                            throw new Exception($"error: label: {label} has character other than letters and numbers\n");
                            return;
                        }

                        // look for duplicate label
                        for (i = 0; i < numLabels; i++)
                        {
                            if (label == labelArray[i])
                            {
                                throw new Exception($"error: duplicate label: {label} at address: {address}");
                                return;
                            }
                        }
                        // see if there are too many labels
                        if (numLabels >= MaxNumLabels)
                        {
                            throw new Exception("error: too many labels");
                            ;
                        }

                        labelArray.Add(label);
                        labelAddress.Add(address);
                    }
                }

                // print machine code

                // set stream
                inFilePtr.Dispose(); inFilePtr.Close();
                inFilePtr = new StreamReader(inFileString);
                for (address = 0; ReadAndParse(inFilePtr, ref label, ref opcode, ref arg0, ref arg1, ref arg2); address++)
                {
                    if (opcode == "add")
                    {
                        num = ((Parse(KeyWord.ADD) << 36) | (Int64.Parse(arg0) << 30) | (Int64.Parse(arg1) << 24) | Int64.Parse(arg2));
                    }
                    else if (opcode == "nand")
                    {
                        num = ((Parse(KeyWord.NAND) << 36) | (Int64.Parse(arg0) << 30) | (Int64.Parse(arg1) << 24) | Int64.Parse(arg2));
                    }
                    #region AbsoluteAddr
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
                    #endregion
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
                    else if (opcode == "jarl")
                    {
                        num = (Parse(KeyWord.JARL) << 36) | Int64.Parse(arg0) << 30 | Int64.Parse(arg1) << 24;
                    }
                    else if (opcode == "halt")
                    {
                        BigInteger loc = Parse(KeyWord.HALT);
                        num = (loc << 36);
                    }
                    else if (opcode == "mul")
                    {
                        num = ((Parse(KeyWord.MUL) << 36) | (Int64.Parse(arg0) << 30) | (Int64.Parse(arg1) << 24) | Int64.Parse(arg2));
                    }
                    else if (opcode == "clcf")
                    {
                        num = (Parse(KeyWord.CLCF) << 36);
                    }

                    else if (opcode == "lw" || opcode == "sw" || opcode == "beq" || opcode == "jma" || opcode == "jml")
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

                        if (addressField < MinAddressField || addressField > MaxAddressField)
                        {
                            throw new Exception($"error: offset {addressField} out of range\n");
                            return;
                        }

                        // truncate the offset field, in case its negative
                        addressField = addressField & 0xFFFFFF;

                        if (opcode == "beq")
                        {
                            num = (Parse(KeyWord.BEQ) << 36) | (Int64.Parse(arg0) << 30) | (Int64.Parse(arg1) << 24)
                                | addressField;
                        }
                        else if (opcode == "lw" || opcode == "sw")
                        {
                            // lw or sw
                            if (opcode == "lw")
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
                        else
                        {
                            // jma or jml
                            if (opcode == "jma")
                            {
                                num = (Parse(KeyWord.JMA) << 36) | (Int64.Parse(arg0) << 30) |
                                    (Int64.Parse(arg1) << 24) | addressField;
                            }
                            else if (opcode == "jml")
                            {
                                // it jml
                                num = (Parse(KeyWord.JML) << 36) | (Int64.Parse(arg0) << 30) |
                                    (Int64.Parse(arg1) << 24) | addressField;
                            }
                        }
                    }
                    else if (opcode == ".fill")
                    {
                        if (arg0.All(item => Char.IsLetter(item)))
                        {
                            num = transalateSymbol(labelArray, labelAddress, numLabels, arg0);
                        }
                        else
                        {
                            num = Int64.Parse(arg0);
                        }
                    }
                    else if(opcode == "")
                    {
                        continue;
                    }
                    //File.AppendAllText(outFileString, num.ToString() + "\n");

                    outFilePtr.WriteLine(num.ToString());
                }
                //outFilePtr.WriteLine("proposal");
                outFilePtr.Dispose();
                outFilePtr.Close();
                outFileName = outFileString;
                inFilePtr.Dispose(); inFilePtr.Close();
                return;
            }
            catch(Exception ex)
            {
                inFilePtr.Dispose(); inFilePtr.Close();
                outFilePtr.Dispose(); outFilePtr.Close();
                throw ex;
            }

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
                
                throw new Exception("error: Incorect arg\n");
            }

            if(argNum < 0 || argNum >= RegNumbers)
            {
                
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
                    throw new Exception("error: bad character in addressField\n");
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
                
                throw new Exception($"error: missing label {symbol}\n");
            }

            return (labelAddress[i]);
        }
    }
}
