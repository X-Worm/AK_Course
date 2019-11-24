using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            StreamReader streamReader = new StreamReader(@"C:\log\program.mc");

            for(i = 0; i < 65536; i++)
            {
                state.mem[i] = 0;
            }
            for(i =0; i < NUMREGS; i++)
            {
                state.reg[i] = 0;
            }
            state.pc = 0;

            for(state.numMemory = 0; !streamReader.EndOfStream; state.numMemory++)
            {
                if (state.numMemory >= 65536)
                {
                    Console.WriteLine("exceeded memory size");
                    Environment.Exit(-1);
                }
                line = streamReader.ReadLine();
                state.mem[state.numMemory] = Int32.Parse(line);
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
            for(i = 0; i < NUMREGS; i++)
            {
                writer.WriteLine($"\t\treg[{i}] - {state.reg[i]}");
            }
            writer.WriteLine("end state\n");
        }

        public static int ConvertToNum(int num)
        {
            if((num & (1 << 15)) == 1)
            {
                num -= (1 << 16);
            }
            return (num);
        }

        public static void Run(State state)
        {
            StreamWriter writer = new StreamWriter(@"C:\log\report.txt");
            int arg0 = 0, arg1 = 0, arg2 = 0, addressField = 0;
            int instructions = 0;
            int opCode = 0;
            int maxMem = -1;

            for(; ; instructions++)
            {
                PrintState(state,ref writer);
                if(state.pc < 0 || state.pc >= 65536)
                {
                    Console.WriteLine("pc went out of the memory range\n");
                    Thread.Sleep(5000); Environment.Exit(1);
                }

                maxMem = (state.pc > maxMem) ? state.pc : maxMem;

                // male the following code easier to read
                opCode = state.mem[state.pc] >> 22;
                arg0  = (state.mem[state.pc] >> 19) & 0x7;
                arg1 = (state.mem[state.pc] >> 16) & 0x7;
                arg2 = (state.mem[state.pc]) & 0x7;

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
                    Environment.Exit(-1);
                }
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
       public  int pc;
        public int[] mem = new int[65536];
        public int[] reg = new int[8];
        public int numMemory;
    }

}
