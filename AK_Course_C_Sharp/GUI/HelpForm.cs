using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUI
{
    public partial class HelpForm : Form
    {
        private static List<KeyValuePair<string, string>> opCodeList = new List<KeyValuePair<string, string>>()
        {
             new KeyValuePair<string, string>("ADD", "Додає вміст регістру regA до вмісту regB, та зберігає в destReg"),
             new KeyValuePair<string, string>("NAND ", "Виконує логічне побітове І-НЕ вмісту regA з вмістом regB, та зберігає в destReg"),
             new KeyValuePair<string, string>("LW ", "Завантажує regB з пам’яті. Адреса пам’яті формується додаванням зміщення до вмісту regA"),
             new KeyValuePair<string, string>("SW ", "Зберігає вміст регістру regB в пам’ять. Адреса пам’яті формується додаванням зміщення до вмісту regA."),
             new KeyValuePair<string, string>("BEQ ", "Якщо вміст регістрів regA та regB однаковий, виконується перехід на адресу програмний лічильник(ПЛ) + 1+зміщення, в ПЛ зберігається адреса поточної тобто beq інструкції."),
             new KeyValuePair<string, string>("JALR ", "Спочатку зберігає ПЛ+1 в regB, в ПЛ адреса поточної (jalr) інструкції. Виконує перехід на адресу, яка зберігається в regA. Якщо в якості regA regB задано один і той самий регістр, то спочатку в цей регістр запишеться ПЛ+1, а потім виконається перехід до ПЛ+1."),
             new KeyValuePair<string, string>("HALT", "Збільшує значення ПЛ на 1, потім припиняє виконання, стимулятор має повідомляти, що виконано зупинку."),
             new KeyValuePair<string, string>("XADD  ", "Додає вміст регістру regA до вмісту regB, та зберігає в комірку памяті memAddr, та обмінює вміст регістрів regA regB"),
             new KeyValuePair<string, string>("XIDIV ", "Знакове ділення регістру regA на regB, результа зберігається в комірку memAddr, та обмін вмістимим регістрів regA regB"),
             new KeyValuePair<string, string>("XSUB ", "Відняти вмістиме регістра regA від regB та записати в memAddr, обмін вмістими між регістрами regA regB"),
             new KeyValuePair<string, string>("XOR ", "Додавання по модулю 2 чисел regA regB збереження результату в destReg"),
             new KeyValuePair<string, string>("CMPE ", "Порівняння чисел regA regB, якщо рівні то destReg = 1, ні — то destReg = 0"),
             new KeyValuePair<string, string>("SAR  ", "Арифметичний зсув вправо destReg=regA >> regB"),
             new KeyValuePair<string, string>("JMA ", "Беззнакове більше if (regA> regB) PC=PC+1+offSet"),
             new KeyValuePair<string, string>("JML ", "Знакове менше if (regA< regB) PC=PC+1+offSet"),
             new KeyValuePair<string, string>("ADC ", "Додавання з переносом: destReg=regA+regB+CF"),
             new KeyValuePair<string, string>("SBB ", "Віднімання з переносом: destReg=regA-regB-СF"),
             new KeyValuePair<string, string>("RCR ", "Зсунути циклічно вправо через CF destReg=regA << regB"),
             new KeyValuePair<string, string>("CLCF", "Скинути значення CF")
        };

        private readonly string helpText = "AK course work V5.\n\n-To build app press Build icon in main menu\n\n-To simulate machine code press Run icon in main menu\n\n\nHelp menu describe list of commands.";
        private readonly string repoLink = "https://github.com/X-Worm/AK_Course";

        public HelpForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            richTextBox1.Enabled = false;
            dataGridView1.DataSource = opCodeList;
            dataGridView1.AutoResizeColumns();
            dataGridView1.Columns[1].Width = 1500;
            dataGridView1.Columns[0].HeaderText = "opCode";
            dataGridView1.Columns[1].HeaderText = "description";
            richTextBox1.Text = helpText;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;
            System.Diagnostics.Process.Start(repoLink);
        }
    }
}
