﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AK_Course_C_Sharp
{
    class Program
    {
        static void Main(string[] args)
        {
            int i = Int32.MaxValue;
           
            Console.WriteLine(i);
            Console.ReadKey();
            //SSOL.Exec();
            //ASOL.Exec();
            //MainMenu();
        }

        public static void MainMenu()
        {
            int choice = 0;

            while (true)
            {
                Console.WriteLine("1: to Assembly program:");
                Console.WriteLine("2: to Simulae code:");
                Console.Write("Input chioce: ");
                try
                {
                    choice = Convert.ToInt32(Console.ReadLine());
                }catch(Exception ex)
                {
                    Console.WriteLine("Invalid choice");
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        {
                            string locl = "";
                            ASOL.Exec(@"c:\log\program.as", ref locl);
                            break;
                        }
                    case 2:
                        {
                            string local = "";
                            SSOL.Exec(@"c:\log\program.mc",ref local );
                            break;
                        }
                    default:
                        {
                            Console.WriteLine("Invalid input, repeat action.");
                            break;
                        }
                }
            }
        }
    }
}
