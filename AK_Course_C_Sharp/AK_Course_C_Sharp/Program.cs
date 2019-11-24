﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AK_Course_C_Sharp
{
    class Program
    {
        static void Main(string[] args)
        {
            //SSOL.Exec();
            //ASOL.Exec();
            MainMenu();
        }

        public static void MainMenu()
        {
            int choice = 0;

            while (true)
            {
                Console.WriteLine("1: to Assembly program:");
                Console.WriteLine("2: to Simulae code:");
                Console.Write("Input chioce: ");
                choice = Convert.ToInt32( Console.ReadLine());

                switch (choice)
                {
                    case 1:
                        {
                            ASOL.Exec();
                            break;
                        }
                    case 2:
                        {
                            SSOL.Exec();
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
