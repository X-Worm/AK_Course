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
        public static int NUMREGS = 8;

        public static void Exec()
        {
            int i = 0;
            string line = "";
            State state = new State();
            state.CarryFlag = 0;

            Console.Write("Input path to machine code: ");
            string filePath = Console.ReadLine();
            StreamReader streamReader = null;
            try
            {
                streamReader = new StreamReader(filePath);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                    Console.WriteLine("exceeded memory size");
                    Environment.Exit(-1);
                }
                line = streamReader.ReadLine();
                state.mem[state.numMemory] = Int64.Parse(line);
            }

            Run(state);
        }

        public static void PrintState(State state ,ref StreamWriter writer)
        {
            int i;

            writer.WriteLine("\n@@@\nstate:");
            writer.WriteLine($"\tpc {state.pc}");
            writer.WriteLine("\tmemory:\n");
            for(i = 0; i < state.numMemory; i++)
            {
                writer.WriteLine($"\t\tmem[{i}] - {state.mem[i]}");
            }
            writer.WriteLine("\tregisters:\n");
            for(i = 0; i < ASOL.RegNumbers; i++)
            {
                writer.WriteLine($"\t\treg[{i}] - {state.reg[i]}");
            }
            writer.WriteLine("end state\n");
        }

        public static Int64 ConvertToNum(Int64 num)
        {
            if((num & (1 << 15)) == 1)
            {
                num -= (1 << 16);
            }
            return (num);
        }

        public static void Run(State state)
        {
            Console.Write("Input file for report: ");
            string filePath = Console.ReadLine();
            StreamWriter writer = null;
            try
            {
                writer = new StreamWriter(filePath);
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

            for(; ; instructions++)
            {
                PrintState(state,ref writer);
                if(state.pc < 0 || state.pc >= ASOL.MaxNumLabels)
                {
                    Console.WriteLine("pc went out of the memory range\n");
                    Thread.Sleep(5000); Environment.Exit(1);
                }

                maxMem = (state.pc > maxMem) ? state.pc : maxMem;

                // male the following code easier to read
                opCode = state.mem[(Int64)state.pc] >> 36;
                arg0  = (state.mem[state.pc] >> 30) & 63;
                arg1 = (state.mem[state.pc] >> 24) & 63;
                arg2 = (state.mem[state.pc]) & 63;

                // for beg, lw, sw
                addressField = ConvertToNum(state.mem[state.pc] & 0xFFFF);

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
                        state.reg[arg0] + addressField >= 65536)
                    {
                        Console.WriteLine("address out of bounds");
                        Thread.Sleep(2000); Environment.Exit(1);
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
                        state.reg[arg0] + addressField >= 65536)
                    {
                        Console.WriteLine("address out of bounds");
                        Thread.Sleep(2000); Environment.Exit(1);
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
                    state.reg[arg2] = state.reg[arg0] + state.reg[arg1];
                    Int64 temp = state.reg[arg0];
                    state.reg[arg0] = state.reg[arg1]; state.reg[arg1] = temp;
                }
                // Знакове ділення і оьмін операндів місцями
                else if(opCode == ASOL.Parse(ASOL.KeyWord.XIDIV))
                {
                    state.reg[arg2] = state.reg[arg0] / state.reg[arg1];
                    Int64 temp = state.reg[arg0];
                    state.reg[arg0] = state.reg[arg1]; state.reg[arg1] = temp;
                }
                // Віднімання і обмін операндів місцями
                else if(opCode == ASOL.Parse(ASOL.KeyWord.XSUB))
                {
                    state.reg[arg2] = state.reg[arg0] - state.reg[arg1];
                    Int64 temp = state.reg[arg0];
                    state.reg[arg0] = state.reg[arg1]; state.reg[arg1] = temp;
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
                    throw new NotImplementedException();
                }
                #endregion

                else
                {
                    writer.WriteLine($"illegal opcode {opCode}");
                    Console.WriteLine($"illegal opcode {opCode}");
                    //Environment.Exit(1);
                }
            }
            writer.Close();
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
