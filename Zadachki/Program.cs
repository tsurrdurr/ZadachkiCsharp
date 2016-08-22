using System;
using System.Collections.Generic;
using System.Text;
using Devart.Data.PostgreSql;
using System.Data;
using System.Configuration;



namespace Zadachki
{
    class Program 
    {
        enum RunModes : byte { db = 1, file = 2, manual = 3, quit = 4 };
        static void Main(string[] args)
        {
            //вид исходной задачи:
            //Создайте структуру с именем student, содержащую поля: фамилия и инициалы, номер группы, успеваемость 
            //(массив из пяти элементов). Создать массив из десяти элементов такого типа, 
            //упорядочить записи по возрастанию среднего балла. Добавить возможность вывода фамилий и 
            //номеров групп студентов, имеющих оценки, равные только 4 или 5.

            //вместо массивов я использовал базу данных

            bool running = true;
            while (running)
            {
                Console.Write("Выберите режим работы:\n1. чтение из БД\n2. чтение из файла\n3. ввод из кода\n4. завершение работы\nВаш выбор:");
                byte modeApp = 0;
                while (!byte.TryParse(Console.ReadLine(), out modeApp) || 0 == modeApp || modeApp > 4)
                {
                    Console.WriteLine("Неверный ввод");
                }
                try
                {
                    RunModes rm = (RunModes)modeApp;
                    switch (rm)
                    {
                        case RunModes.db: runDBmode(); break;
                        case RunModes.file: runFilemode(); break;
                        case RunModes.manual: runBadmode(); break;
                        case RunModes.quit: running = false; break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Не удалось выполнить программу. Текст ошибки: " + ex);
                }
            }
        }

        static void runDBmode()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DBstringKey"].ConnectionString;
            using (PgSqlConnection connection = new PgSqlConnection(connectionString))
            {
                connection.Open(); //подключение к базе на другом хосте в СУБД Postgre
                PgSqlDataAdapter dataAdapter = new PgSqlDataAdapter("", connection);
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand.CommandText = "SELECT list.\"Lastname\", list.initials, list.\"group\", list.grade FROM  public.list;";
                //TODO: test @"SELECT list.\"Lastname\", list.initials, list.\"group\", list.grade FROM  public.list;";
                //выполнение запроса select к базе, запись ответа в dataAdapter
                dataAdapter.Fill(dataSet); //выгрузка данных из dataAdapter в структуру dataSet
                Student s = new Student();
                List<Student> ls = new List<Student>();
                Console.WriteLine("Сырой вывод:");
                Console.Write("Фамилия ИО\tГруппа\tСредний балл\n");

                foreach (DataTable table in dataSet.Tables)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        //создание объекта со значениями полей из базы
                        s.lastname = ChangeEncoding(row[0].ToString());
                        s.initials = ChangeEncoding(row[1].ToString());
                        //TODO: разобраться с этим ужасным кодом
                        //найти способ читать сразу в win1251 из бд
                        s.groupn = row[2].ToString();
                        s.grade = Double.Parse(row[3].ToString());
                        ls.Add(s);
                        foreach (DataColumn column in table.Columns)
                        {
                            string encBuf;
                            encBuf = row[column].ToString();
                            encBuf = ChangeEncoding(encBuf);
                            Console.Write(encBuf + "\t");

                        }
                        Console.Write("\n");
                    }
                }
                connection.Close();
                sortAndShowList(ls);
            }
        }

        static void sortAndShowList(List<Student> ls) //вывод содержимого списка в консоль в виде таблицы
        {
            ls.Sort(CompareGrades);
            Console.WriteLine("\nСортированный вывод:");
            Console.Write("Фамилия ИО\tГруппа\tСредний балл\n");
            foreach (Student st in ls)
            {
                Console.WriteLine(st.lastname + "\t" + st.initials + "\t" + st.groupn + "\t" + st.grade);
            }
            Console.Write("\n");
        }

        static void runBadmode()
        {
            //ручной метод заполнения
            //имеет ГСЧ для демонстрации корректной работы сортировщика
            Random rn = new Random();
            Student s = new Student();
            List<Student> ls = new List<Student>();
            s.lastname = "Иванов";
            s.initials = "И.И.";
            s.groupn = "12";
            s.grade = 2 + rn.NextDouble() * 3;
            ls.Add(s);
            s.lastname = "Петров";
            s.initials = "П.П.";
            s.groupn = "11";
            s.grade = 2 + rn.NextDouble() * 3;
            ls.Add(s);
            s.lastname = "Сидоров";
            s.initials = "С.С.";
            s.groupn = "6";
            s.grade = 2 + rn.NextDouble() * 3;
            ls.Add(s);
            sortAndShowList(ls);
        }

        static void runFilemode()
        {
            try
            {
                Student s = new Student();
                List<Student> ls = new List<Student>();
                string appSettings = ConfigurationManager.AppSettings["FilepathKey"];
                string[] lines = System.IO.File.ReadAllLines(appSettings);

                foreach (string line in lines)
                {
                    if (!(line[0] == '#'))
                    {
                        string[] data = line.Split('\t');
                        s.lastname = data[0].ToString();
                        s.initials = data[1].ToString();
                        s.groupn = data[2].ToString();
                        s.grade = Double.Parse(data[3].ToString());
                        ls.Add(s);
                    }
                }
                sortAndShowList(ls);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Не удалось выполнить программу. Текст ошибки: " + ex);
            }
        }

        private static int CompareGrades(Student x, Student y) //метод сравнения двух объектов Student
        {
            if (x.grade > y.grade)
            {
                return 1;
            }
            if (x.grade < y.grade)
            {
                return -1;
            }
            
                return 0;
            
        }



        private static string ChangeEncoding(string input) //метод смены кодировки строки
        {
       
            Encoding utf = Encoding.UTF8;
            Encoding win = Encoding.GetEncoding(1251);
            //Encoding cons = Encoding.GetEncoding(866);
            byte[] utfArr = win.GetBytes(input);
            byte[] winArr = Encoding.Convert(utf, win, utfArr);
            string winLine = win.GetString(winArr);
            return winLine;
            
        }

    }

    
}
