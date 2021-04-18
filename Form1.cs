using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lab04
{
    public partial class Form1 : Form
    {
        DirectoryInfo directoryInfo = null;
        
        List<TestInfo> filesInfoList;
        //Mutex mutex_filesInfoList = new Mutex();
        List<TestInfo> solutionsInfoList;
        //Mutex mutex_solutionsInfoList = new Mutex();
        List<TestInfo> resultInfoList;
        //Mutex mutex_resultInfoList = new Mutex();

        Mutex mutex_listBox = new Mutex();

        Thread Thread_Cheks_The_Condition;
        Thread Thread_Solutions;
        Thread Thread_Cheks_The_Result;
        public int kol;

        public Form1()
        {
            InitializeComponent();
           // MessageBox.Show("Для начала работы, попрошу ознакомиться с разделом меню Help. Во избежание в дальнейшем возможных недопониманий :)", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            kol = 1;
            directoryText.Enabled = true;
            pauseButton.Enabled = false;
            stopButton.Enabled = false;
            resumeButton.Enabled = false;
            filesInfoList = new List<TestInfo>();
            solutionsInfoList = new List<TestInfo>();
            resultInfoList = new List<TestInfo>();
        }

        //ВВОД РАСПОЛОЖЕНИЯ ПАПКИ С ТЕСТАМИ
        private void directoryText_TextChanged(object sender, EventArgs e)
        {
            startButton.Enabled = true;
            if (directoryText.Text.Length == 0)
                MessageBox.Show("Введите путь к папке, хранящей в себе тестовые файлы.\n" +
                    "Для каждого теста должен быть свой входной \"CLOSE.IN\" \n" +
                    "и свой выходной \"CLOSE.OUT\" файлы. \n" +
                    "Для каждого нового теста данные файлы должны лежать в отдельной " +
                    "поддиректории, указанной ранее папки.\n", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            try
            {
                directoryInfo = new DirectoryInfo(directoryText.Text);
            }
            catch (Exception EE)
            {
                MessageBox.Show(EE.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                directoryText.Clear();
            }
        }

        //КНОПКИ
        private void startButton_Click(object sender, EventArgs e)
        {

            startButton.Enabled = false;
            if (directoryInfo == null)
            {
                MessageBox.Show("Путь к тестам не введен", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                kol = 1;
                filesInfoList.Clear();
                solutionsInfoList.Clear();
                resultInfoList.Clear();
                listBox.Items.Clear();
                pauseButton.Enabled = true;
                stopButton.Enabled = true;
                Search_Files(directoryInfo);

                Thread_Cheks_The_Condition = new Thread(Cheks_The_Condition);
                Thread_Solutions = new Thread(Solutions);
                Thread_Cheks_The_Result = new Thread(Cheks_The_Result);

                if (filesInfoList.Count != 0)
                {
                    Thread_Cheks_The_Condition.Start();
                    Thread_Solutions.Start();
                    Thread_Cheks_The_Result.Start();
                }
                else
                    MessageBox.Show("По данному пути не найдено ни одного теста", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void stopButton_Click(object sender, EventArgs e)
        {
            if (Thread_Solutions.IsAlive)
            {
                startButton.Enabled = false;
                resumeButton.Enabled = false;
                directoryText.Enabled = false;
                try
                {
                    Thread_Solutions.Abort();
                }
                catch(Exception EE)
                {
                    MessageBox.Show(EE.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                MessageBox.Show("Поток, решающий задачу, остановлен", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
                MessageBox.Show("Поток, решающий задачу, закончил свою работу, останавливать нечего", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void pauseButton_Click(object sender, EventArgs e)
        {
           if (Thread_Solutions.IsAlive)
            {
                Thread_Solutions.Suspend();
                startButton.Enabled = false;
                resumeButton.Enabled = true;
                stopButton.Enabled = true;
                MessageBox.Show("Поток, решающий задачу, был приостановлен", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Поток, решающий задачу, завершил работу, приостанавливить его невозможно", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void resumeButton_Click(object sender, EventArgs e)
        {
            startButton.Enabled = true;
            resumeButton.Enabled = false;
            stopButton.Enabled = true;
            Thread_Solutions.Resume();
            MessageBox.Show("Поток, решающий задачу, возобновил работу", "Infformation", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        //ФУНКЦИЯ СЧИТЫВАЕТ И ПРОВЕРЯЕТ ВХОДНЫЕ ДАННЫЕ
        private void Cheks_The_Condition()
        {
            while (filesInfoList.Count != 0)
            {
               // mutex_filesInfoList.WaitOne();
                TestInfo test = filesInfoList[0];
                filesInfoList.RemoveAt(0);
                // mutex_filesInfoList.ReleaseMutex();

                //mutex_listBox.WaitOne();
                //listBox.Items.Add("\nTHREAD_CHEKS_THE_CONDITION   " + test.getNum() + "\n");
                //mutex_listBox.ReleaseMutex();

                StreamReader streamReader = new StreamReader(test.getFile_In().FullName);
                string line;
                line = streamReader.ReadLine();
                string[] s = line.Split(' ');
                test.setN(int.Parse(s[0]));
                test.setK(int.Parse(s[1]));
                if (test.getN() < 2 || test.getN() > 10000 || test.getK() < 1 || test.getK() > 10000000)
                {
                    test.setStatus(-1);
                    ShowTheTest(test);
                }
                else
                {
                    int[][] d = new int[test.getN()][];
                    for (int i = 0; i < test.getN(); i++)
                        d[i] = new int[test.getN()];
                    int j = 0;
                    while (!streamReader.EndOfStream)
                    {
                        line = streamReader.ReadLine();
                        s = line.Split(' ');
                        for (int i = 0; i < test.getN(); i++)
                        {
                            d[j][i] = int.Parse(s[i]);
                            if (d[j][i] < 0 || d[j][i] > 1000 || (i == j && d[j][i] != 0))
                            {
                                test.setStatus(-1);
                                ShowTheTest(test);
                            }
                        }
                        j++;
                    }
                    test.setD(d);
                }
                if (test.getStatus() == 0)
                {
                    test.setStatus(1);
                    streamReader.Close();
                    //mutex_solutionsInfoList.WaitOne();
                    solutionsInfoList.Add(test);
                    //mutex_solutionsInfoList.ReleaseMutex();
                }
            }
        }

        //ФУНКЦИЯ РЕШЕНИЯ
        private void Solutions()
        {
            Thread.Sleep(kol+300);
            while (solutionsInfoList.Count != 0)
            {
                //mutex_solutionsInfoList.WaitOne();
                TestInfo test = solutionsInfoList[0];
                solutionsInfoList.RemoveAt(0);
                //mutex_solutionsInfoList.ReleaseMutex();

                //mutex_listBox.WaitOne();
                //listBox.Items.Add("\nTHREAD_SOLUTIONS   " + test.getNum() + "\n");
                //mutex_listBox.ReleaseMutex();
                
                if (!Floyd_Warshall(test))
                {
                    test.setStatus(-2);
                    ShowTheTest(test);
                }
                else
                {
                    if (test.getStatus() == 1)
                    {
                        test.setStatus(2);
                        //mutex_resultInfoList.WaitOne();
                        resultInfoList.Add(test);
                        //mutex_resultInfoList.ReleaseMutex();
                    }
                }
            }
        }
        //АЛГОРИТМ РЕШЕНИЯ ПОСТАВЛЕННОЙ ЗАДАЧИ (АЛГОРИТМ ФЛОЙДА)
        static int INT_MAX = 9999;
        private static bool Floyd_Warshall(TestInfo test)
        {
            List<Trio> result = new List<Trio>();

            int[][] d = new int[test.getN()][];
            for (int i = 0; i < test.getN(); i++)
                d[i] = new int[test.getN()];
            for (int i = 0; i < test.getN(); i++)
                for (int j = 0; j < test.getN(); j++)
                    if (test.getD()[i][j] == 0 && i != j)
                        d[i][j] = INT_MAX;
                    else
                        d[i][j] = test.getD()[i][j];
            for (int k = 0; k < test.getN(); k++)
                for (int i = 0; i < test.getN(); i++)
                    for (int j = 0; j < test.getN(); j++)
                        if (d[i][k] + d[k][j] < d[i][j])
                            d[i][j] = d[i][k] + d[k][j];

            for (int i = 0; i < test.getN(); i++)
                for (int j = 0; j < test.getN(); j++)
                    if (d[i][j] != INT_MAX && i != j && d[i][j] <= test.getK())
                        result.Add(new Trio(i + 1, j + 1, d[i][j]));
            //запись в файл
            try
            {
                string path_to_result_file = test.getFile_out().FullName;
                path_to_result_file = path_to_result_file.Substring(0, path_to_result_file.LastIndexOf('\\') + 1) + "my_CLOSE.OUT";
                File.Delete(path_to_result_file);
                FileStream fs = new FileStream(path_to_result_file, FileMode.CreateNew);
                fs.Close();
                StreamWriter streamWriter = new StreamWriter(path_to_result_file, true);
                streamWriter.WriteLine(result.Count);
                for (int j = 0; j < result.Count; j++)
                    streamWriter.WriteLine(result[j].toString());
                streamWriter.Close();
            }
            catch (Exception e) { return false; }
            return true;
        }

        //ФУНКЦИЯ СВЕРКИ С ЭТАЛОННЫМ ВЫХОДНЫМ ФАЙЛОМ
        private void Cheks_The_Result()
        {
            MessageBox.Show("Пожалуйста, дождитесь конца проверки на соответствие. Всплывет окно-сообщение о завершении работы.", "Informaton", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (resultInfoList.Count == 0)
            {
                Thread.Sleep(kol * 10);
            }
            while (resultInfoList.Count != 0)
            {
                //mutex_resultInfoList.WaitOne();
                TestInfo test = resultInfoList[0];
                resultInfoList.RemoveAt(0);
                //mutex_resultInfoList.ReleaseMutex();

                //mutex_listBox.WaitOne();
                //listBox.Items.Add("\nTHREAD_CHEKS_THE_RESULT   " + test.getNum() + "\n");
                //mutex_listBox.ReleaseMutex();

                StreamReader streamR1 = new StreamReader(test.getFile_out().FullName);
                string line1;

                string path_to_result_file = test.getFile_out().FullName;
                path_to_result_file = path_to_result_file.Substring(0, path_to_result_file.LastIndexOf('\\') + 1) + "my_CLOSE.OUT";
                StreamReader streamR2 = new StreamReader(path_to_result_file);
                string line2;

                line1 = streamR1.ReadLine();
                line2 = streamR2.ReadLine();
                if (line1 == line2)
                {
                    int n = int.Parse(line1);
                    for (int i = 0; i < n; i++)
                    {
                        line1 = streamR1.ReadLine();
                        line2 = streamR2.ReadLine();
                        if (line1 != line2)
                            test.setStatus(-3);
                    }
                }
                else
                    test.setStatus(-3);

                if (test.getStatus() == 2)
                    test.setStatus(3);

                streamR1.Close();
                streamR2.Close();
                ShowTheTest(test);
              
            }
            MessageBox.Show("Работа окончена", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
     
        //МЕНЮ
        private void aboutTheAuthorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Belkina Daria, 9 group", "About the Author", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("В данной задаче решение однозначно\n" +
                "Поэтому при несовпадении с эталонным выходным файлом делается вывод о неправильности этого эталонного файла\n", "Supporting Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        //ФУНКЦИЯ ВЫВОДА ИМЕЮЩЕЙСЯ ИНФОРАМЦИИ В LISTBOX
        void ShowTheList(List<TestInfo> A)
        {
            foreach (TestInfo test in A)
            {
                listBox.Items.Add("ТЕСТ №" + test.num.ToString());
                listBox.Items.Add(test.file_in.FullName);
                listBox.Items.Add(test.file_out.FullName);

                String s = "Статус:  ";
                switch (test.status)
                {
                    case -3: s += "проверку на соответствие НЕ прошёл, скорее всего ошибка в эталонном файле\n"; break;
                    case -2: s += "возникла ошибка при решении\n"; break;
                    case -1: s += "ошибка во входном файле\n"; break;
                    case 0: s += "готов к началу работы\n"; break;
                    case 1: s += "проверен, готов к решению\n"; break;
                    case 2: s += "решен, готов к проверке на соответствие эталонному файлу\n"; break;
                    case 3: s += "проверку на соответствиее прошел, решён верно\n"; break;
                }
                listBox.Items.Add(s);
                //listBox.Items.Add(test.getN() + " " + test.getK());
                //for (int i = 0; i < test.getN(); i++)
                //{
                //    string str = "";
                //    for (int j = 0; j < test.getN(); j++)
                //    {
                //        str += test.getD()[i][j];
                //        str += " ";
                //    }
                //    listBox.Items.Add(str);
                //}
                listBox.Items.Add("\n");
            }
        }
        private void ShowTheTest(TestInfo test)
        {
            mutex_listBox.WaitOne();
            listBox.Items.Add("ТЕСТ №" + test.num.ToString());
            listBox.Items.Add(test.file_in.FullName);
            listBox.Items.Add(test.file_out.FullName);

            String s = "СТАТУС:  ";
            switch (test.status)
            {
                case -3: s += "проверку на соответствие НЕ прошёл, скорее всего ошибка в эталонном файле\n"; break;
                case -2: s += "возникла ошибка при решении\n"; break;
                case -1: s += "ошибка во входном файле\n"; break;
                case 0: s += "готов к началу работы\n"; break;
                case 1: s += "проверен, готов к решению\n"; break;
                case 2: s += "решен, готов к проверке на соответствие эталонному файлу\n"; break;
                case 3: s += "проверку на соответствиее прошел, решён верно\n"; break;
            }
            listBox.Items.Add(s);
            //listBox.Items.Add(test.getN() + " " + test.getK());
            //for (int i = 0; i < test.getN(); i++)
            //{
            //    string str = "";
            //    for (int j = 0; j < test.getN(); j++)
            //    {
            //        str += test.getD()[i][j];
            //        str += " ";
            //    }
            //    listBox.Items.Add(str);
            //}
            listBox.Items.Add("\n");
            mutex_listBox.ReleaseMutex();
        }
        
        //ФУНКЦИИ ПОИСКА ТЕСТОВЫХ ФАЙЛОВ
        void Search_Files(DirectoryInfo dirInfo)
        {
            FileInfo[] allFiles_in = null;
            FileInfo[] allFiles_out = null;
            try
            {
                allFiles_in = directoryInfo.GetFiles("*.IN", SearchOption.TopDirectoryOnly);
                allFiles_out = directoryInfo.GetFiles("*.OUT", SearchOption.TopDirectoryOnly);
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибочка вышла, скорее всего, такого пути не существует :)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (allFiles_in != null && allFiles_out != null)
                foreach (FileInfo f_in in allFiles_in)
                {
                    foreach (FileInfo f_out in allFiles_out)
                    {
                        try
                        {
                            if (f_in.Name == "CLOSE.IN" && f_out.Name == "CLOSE.OUT")
                            {
                                TestInfo new_test = new TestInfo();
                                new_test.setFile_In(f_in);
                                new_test.setFile_Out(f_out);
                                new_test.setNum(kol);
                                kol++;
                                new_test.setStatus(0);
                                filesInfoList.Add(new_test);
                            }
                        }
                        catch (Exception e) { }
                    }
                }
            DirectoryInfo[] allDirectories = null;
            try
            {
                allDirectories = directoryInfo.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
            }
            catch (Exception e) { }
            if (allDirectories != null)
                foreach (DirectoryInfo dI in allDirectories)
                {
                    try
                    {
                        HelpSearchFiles(dI);
                    }
                    catch (Exception e) { }
                }
        }

        void HelpSearchFiles(DirectoryInfo dirInfo)
        {
            FileInfo[] allFiles_in = null;
            FileInfo[] allFiles_out = null;
            try
            {
                allFiles_in = dirInfo.GetFiles("*.IN", SearchOption.TopDirectoryOnly);
                allFiles_out = dirInfo.GetFiles("*.OUT", SearchOption.TopDirectoryOnly);
            }
            catch (Exception e) { }
            if (allFiles_in != null && allFiles_out != null)
                foreach (FileInfo f_in in allFiles_in)
                {
                    foreach (FileInfo f_out in allFiles_out)
                    {
                        try
                        {
                            if (f_in.Name == "CLOSE.IN" && f_out.Name == "CLOSE.OUT")
                            {
                                TestInfo new_test = new TestInfo();
                                new_test.setFile_In(f_in);
                                new_test.setFile_Out(f_out);
                                new_test.setNum(kol);
                                kol++;
                                new_test.setStatus(0);
                                filesInfoList.Add(new_test);
                            }
                        }
                        catch (Exception e) { }
                    }
                }
            DirectoryInfo[] allDirectories = null;
            try
            {
                allDirectories = dirInfo.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
            }
            catch (Exception e) { }
            if (allDirectories != null)
                foreach (DirectoryInfo dI in allDirectories)
                {
                    try
                    {
                        HelpSearchFiles(dI);
                    }
                    catch (Exception e) { }
                }
        }

    }
    //КЛАСС КОМПОНЕНТЫ ДЛЯ ЛУЧШЕГО ХРАНЕНИЯ РЕШЕНИЯ
    class Trio
    {
        public int From_Where;
        public int Where;
        public int Price;
        public Trio()
        {
            this.From_Where = 0;
            this.Where = 0;
            this.Price = 0;
        }
        public Trio(int fW, int w, int p)
        {
            this.From_Where = fW;
            this.Where = w;
            this.Price = p;
        }
        public String toString()
        {
            return this.From_Where + " " + this.Where + " " + this.Price;
        }
        public void setFrom_Where(int f_w)
        {
            this.From_Where = f_w;
        }
        public void setWhere(int w)
        {
            this.Where = w;
        }
        public void setPrice(int p)
        {
            this.Price = p;
        }
        public int getFrom_Where()
        {
            return this.From_Where;
        }
        public int getWhere()
        {
            return this.Where;
        }
        public int getPrice()
        {
            return this.Price;
        }
    }
    //КЛАСС ДЛЯ ХРАНЕНИЯ НЕОБХОДИМОЙ ИНФОРМАЦИИ О ТЕСТЕ
    class TestInfo
    {
        public FileInfo file_in;
        public FileInfo file_out;
        public int num;
        public int status;
        public int N;
        public int K;
        public int[][] D;
        // -3 - проверку на соответствие не прошел
        // -2 - ошибка при решении
        // -1 - ошибка во входном файле
        // 0 - готов к проверке
        // 1 - проверен, готов к выполнению
        // 2 - решен, готов к проверке на соответствие эталонному файлу
        // 3 - провереку прошел
        public TestInfo()
        {
            this.file_in = null;
            this.file_out = null;
            this.num = 0;
            this.status = 0;
        }
        public TestInfo(FileInfo f_in, FileInfo f_out, int n, int s)
        {
            this.file_in = f_in;
            this.file_out = f_out;
            this.num = n;
            this.status = s;
        }
        public String toString()
        {
            String s;
            s = this.file_in.FullName + "\n" + this.file_out.FullName + "\n" +
                "Номер теста: " + this.num.ToString() + "\n" +
                "Статус: ";
            switch (this.status)
            {
                case -3: s += "проверку на соответствие не прошёл\n"; break;
                case -2: s += "возникла ошибка при решении\n"; break;
                case -1: s += "ошибка во входном файле\n"; break;
                case 0: s += "готов к началу работы\n"; break;
                case 1: s += "проверен, готов к решению\n"; break;
                case 2: s += "решен, готов к проверке на соответствие эталонному файлу\n"; break;
                case 3: s += "проверку прошел, решён верно\n"; break;
            }
            return s;
        }
        public void setN(int n)
        {
            this.N = n;
        }
        public void setK(int k)
        {
            this.K = k;
        }
        public void setD(int[][] d)
        {
            this.D = new int[this.N][];
            for (int i = 0; i < this.N; i++)
                this.D[i] = new int[this.N];
            for (int i = 0; i < this.N; i++)
                for (int j = 0; j < this.N; j++)
                    this.D[i][j] = d[i][j];
        }
        public int getN()
        {
            return this.N;
        }
        public int getK()
        {
            return this.K;
        }
        public int[][] getD()
        {
            //int[][] d = new int[this.N][];
            //for (int i = 0; i < this.N; i++)
            //    d[i] = new int[this.N];
            //for (int i = 0; i < this.N; i++)
            //    for (int j = 0; j < this.N; j++)
            //        d[i][j] = this.D[i][j];
            //return d;
            return this.D;
        }
        public void setFile_In(FileInfo fInfo)
        {
            this.file_in = fInfo;
        }
        public void setFile_Out(FileInfo fInfo)
        {
            this.file_out = fInfo;
        }
        public void setNum(int n)
        {
            this.num = n;
        }
        public void setStatus(int s)
        {
            this.status = s;
        }
        public FileInfo getFile_In()
        {
            return this.file_in;
        }
        public FileInfo getFile_out()
        {
            return this.file_out;
        }
        public int getNum()
        {
            return this.num;
        }
        public int getStatus()
        {
            return this.status;
        }
    }
}
