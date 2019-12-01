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
    public class SSOL
    {
        public static int RegisterIncreasing { get; set; } = 8;

        public static List<Int64> AbsouluteAddrReg = new List<Int64>();

        public static void Exec(string machineCodePath, ref  string outFileName)
        {
            int i = 0;
            string line = "";
            State state = new State();
            state.CarryFlag = 0;

            StreamReader streamReader = null;
            try
            {
                streamReader = new StreamReader(machineCodePath);
                outFileName = Path.GetDirectoryName(machineCodePath) + "\\" + Path.GetFileNameWithoutExtension(machineCodePath) + "_report.txt";
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
                return;
            }

            for(i = 0; i < ASOL.MaxNumLabels; i++)
            {
                state.mem[i] = 0;
            }
            for(i =0; i < ASOL.RegNumbers; i++)
            {
                state.reg[i] = 0;
            }
            state.pc = 0;

            for(state.numMemory = 0; !streamReader.EndOfStream; state.numMemory++)
            {
                if (state.numMemory >= ASOL.MaxNumLabels)
                {
                    throw new Exception("exceeded memory size");
                }
                line = streamReader.ReadLine();
                if (line == "") break;
                state.mem[state.numMemory] = Int64.Parse(line);
            }

            Run(state, outFileName);
        }

        public static void PrintState(State state ,ref StreamWriter writer)
        {
            int i;
            LimitRegRange(RegisterIncreasing);

            writer.WriteLine("\n@@@\nstate:");
            writer.WriteLine($"\tpc {state.pc}");
            writer.WriteLine("\tmemory:\n");
            for(i = 0; i < state.numMemory; i++)
            {
                writer.WriteLine($"\t\tmem[{i}] - {state.mem[i]}");
            }
            if(AbsouluteAddrReg.Count != 0)
            {
                writer.WriteLine("\tmemory used as absolute value (xadd, xidiv, xsub)\n");
                foreach(var j in AbsouluteAddrReg)
                {
                    writer.WriteLine($"\t\tmem[{j}] - {state.mem[j]}");
                }
            }
            writer.WriteLine("\tregisters:\n");
            for(i = 0; i < RegisterIncreasing; i++)
            {
                writer.WriteLine($"\t\treg[{i}] - {state.reg[i]}");
            }
            writer.WriteLine($"\n\tflag state: CF: {state.CarryFlag}");

            writer.WriteLine("end state\n");
        }

        public static Int64 ConvertToNum(Int64 num)
        {
            if((num & (1 << 23)) != 0)
            {
                num -= (1 << 24);
            }
            return (num);
        }

        public static void Run(State state, string outFileName)
        {
            
            StreamWriter writer = null;
            try
            {
                writer = new StreamWriter(outFileName);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            Int64 arg0 = 0, arg1 = 0, arg2 = 0, addressField = 0;
            int instructions = 0;
            Int64 opCode = 0;
            Int64 maxMem = -1;

            if (arg0 > RegisterIncreasing || arg1 > RegisterIncreasing) RegisterIncreasing *= 2;

            for(; ; instructions++)
            {
                PrintState(state,ref writer);
                if(state.pc < 0 || state.pc >= ASOL.MaxNumLabels)
                {
                    throw new Exception("pc went out of the memory range\n");
                }

                maxMem = (state.pc > maxMem) ? state.pc : maxMem;

                // make the following code easier to read
                opCode = state.mem[(Int64)state.pc] >> 36;
                arg0  = (state.mem[state.pc] >> 30) & 63;
                arg1 = (state.mem[state.pc] >> 24) & 63;
                arg2 = (state.mem[state.pc]) & 63;

                // for beg, lw, sw
                addressField = ConvertToNum(state.mem[state.pc] & 0xFFFFFF);
                if (addressField > RegisterIncreasing) RegisterIncreasing *= 2;

                state.pc++;
                if(opCode == ASOL.Parse(ASOL.KeyWord.ADD))
                {
                    state.reg[arg2] = state.reg[arg0] + state.reg[arg1];
                }
                else if(opCode == ASOL.Parse(ASOL.KeyWord.NAND))
                {
                    state.reg[arg2] = ~(state.reg[arg0] & state.reg[arg1]);
                }
                else if(opCode == ASOL.Parse(ASOL.KeyWord.LW))
                {
                    if(state.reg[arg0] + addressField < 0 ||
                        state.reg[arg0] + addressField >= ASOL.MaxNumLabels)
                    {
                        throw new Exception("address out of bounds");
                    }
                    state.reg[arg1] = state.mem[state.reg[arg0] + addressField];
                    if(state.reg[arg0] + addressField > maxMem)
                    {
                        maxMem = state.reg[arg0] + addressField;
                    }
                }
                else if(opCode == ASOL.Parse(ASOL.KeyWord.SW))
                {
                    if(state.reg[arg0] + addressField < 0 ||
                        state.reg[arg0] + addressField >= ASOL.MaxNumLabels)
                    {
                        throw new Exception("address out of bounds");
                    }
                    state.mem[state.reg[arg0] + addressField] = state.reg[arg1];
                    if(state.reg[arg0] + addressField > maxMem)
                    {
                        maxMem = state.reg[arg0] + addressField;
                    }
                }
                else if(opCode == ASOL.Parse(ASOL.KeyWord.BEQ))
                {
                    if(state.reg[arg0] == state.reg[arg1])
                    {
                        state.pc += addressField;
                    }
                }
                else if(opCode == ASOL.Parse(ASOL.KeyWord.JARL))
                {
                    state.reg[arg1] = state.pc;
                    if (arg0 != 0)
                    {
                        state.pc = state.reg[arg0];
                    }
                    else
                        state.pc = 0;
                }
                else if(opCode == ASOL.Parse(ASOL.KeyWord.MUL))
                {
                    state.reg[arg2] = state.reg[arg0] * state.reg[arg1];
                }
                else if(opCode == ASOL.Parse(ASOL.KeyWord.HALT))
                {
                    writer.WriteLine("machine halted");
                    writer.WriteLine($"total of {instructions + 1} instructions executed");
                    writer.WriteLine("final state of machine:");
                    PrintState(state,ref writer);
                    writer.Close();
                    return;
                }
                // Додати і обміняти місцями 
                else if(opCode == ASOL.Parse(ASOL.KeyWord.XADD))
                {
                    arg2 = state.mem[state.pc-1] & 0xFFFFFF;
                    state.mem[arg2] = state.reg[arg0] + state.reg[arg1];
                    Int64 temp = state.reg[arg0];
                    state.reg[arg0] = state.reg[arg1]; state.reg[arg1] = temp;

                    AbsouluteAddrReg.Add(arg2);
                }
                // Знакове ділення і оьмін операндів місцями
                else if(opCode == ASOL.Parse(ASOL.KeyWord.XIDIV))
                {
                    arg2 = state.mem[state.pc-1] & 0xFFFFFF;
                    state.mem[arg2] = state.reg[arg0] / state.reg[arg1];
                    Int64 temp = state.reg[arg0];
                    state.reg[arg0] = state.reg[arg1]; state.reg[arg1] = temp;

                    AbsouluteAddrReg.Add(arg2);
                }
                // Віднімання і обмін операндів місцями
                else if(opCode == ASOL.Parse(ASOL.KeyWord.XSUB))
                {
                    arg2 = state.mem[state.pc-1] & 0xFFFFFF;
                    state.mem[arg2] = state.reg[arg0] - state.reg[arg1];
                    Int64 temp = state.reg[arg0];
                    state.reg[arg0] = state.reg[arg1]; state.reg[arg1] = temp;

                    AbsouluteAddrReg.Add(arg2);
                }

                // Додавання по модулю 2
                else if(opCode == ASOL.Parse(ASOL.KeyWord.XOR))
                {
                    state.reg[arg2] = state.reg[arg0] ^ state.reg[arg1];
                }
                // порівння regA == regB
                else if(opCode == ASOL.Parse(ASOL.KeyWord.CMPE))
                {
                    if (state.reg[arg1] == state.reg[arg0])
                        state.reg[arg2] = 1;
                    else
                        state.reg[arg2] = 0;
                }
                // арифметичний зсув вправо
                else if(opCode == ASOL.Parse(ASOL.KeyWord.SAR))
                {
                    state.reg[arg2] = state.reg[arg0] >> (int)state.reg[arg1];
                }
                #region with_CF
                else if(opCode == ASOL.Parse(ASOL.KeyWord.ADC))
                {
                    state.reg[arg2] = state.reg[arg1] + state.reg[arg0] + state.CarryFlag;
                }
                else if(opCode == ASOL.Parse(ASOL.KeyWord.SBB))
                {
                    state.reg[arg2] = state.reg[arg0] - state.reg[arg1] - state.CarryFlag;
                }
                else if(opCode == ASOL.Parse(ASOL.KeyWord.RCR))
                {
                    Int64 localArg = state.reg[arg0];
                    // Зсув циклічний вправо через carry flag
                    for(int i = 0; i < state.reg[arg1]; i++)
                    {
                        // перевірити молодший розряд
                        Int64 firstDigit = localArg & (Int64)1;

                        localArg = localArg >> 1;
                        if(state.CarryFlag == 1)
                        {
                            localArg = localArg | 0x800000000000;
                        }
                        state.CarryFlag = firstDigit;
                    }
                    state.reg[arg2] = localArg;
                    
                }
                #endregion
                else if(opCode == ASOL.Parse(ASOL.KeyWord.JML))
                {
                    if (state.reg[arg0] < state.reg[arg1])
                    {
                        state.pc += addressField;
                    }
                }
                else if(opCode == ASOL.Parse(ASOL.KeyWord.JMA))
                {
                    if (state.reg[arg0] > state.reg[arg1])
                    {
                        state.pc += addressField;
                    }
                }
                else if(opCode == ASOL.Parse(ASOL.KeyWord.CLCF))
                {
                    state.CarryFlag = 0;
                }
                else
                {
                    writer.WriteLine($"illegal opcode {opCode}");
                    throw new Exception($"illegal opcode {opCode}");
                    //Environment.Exit(1);
                }
            }
            writer.Close();
        }

        public static void LimitRegRange(int range)
        {
            if (range > 64) range = 64;
        }
        
    }

    public class State
    {
       public  Int64 pc;
        public Int64[] mem = new Int64[ASOL.MaxNumLabels];
        public Int64[] reg = new Int64[ASOL.RegNumbers];
        public int numMemory;
        public Int64 CarryFlag { get; set; }
    }

}
