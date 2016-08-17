﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Devart.Data.PostgreSql;
using System.Data;
using System.Configuration;



namespace Zadachki
{
    class Program 
    {
        static void Main(string[] args)
        {
            //Создайте структуру с именем student, содержащую поля: фамилия и инициалы, номер группы, успеваемость 
            //(массив из пяти элементов). Создать массив из десяти элементов такого типа, 
            //упорядочить записи по возрастанию среднего балла. Добавить возможность вывода фамилий и 
            //номеров групп студентов, имеющих оценки, равные только 4 или 5.

            //вместо массивов я использовал базу данных
            //TODO: вынести в отдельную ветку метод без подключения к бд (сделай файловый ввод)
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["MyKey"].ConnectionString;
                PgSqlConnection connection = new PgSqlConnection(connectionString);
                connection.Open(); //подключение к базе на другом хосте в СУБД Postgre
                PgSqlDataAdapter dataAdapter = new PgSqlDataAdapter("", connection);
                DataSet dataSet = new DataSet();
                dataAdapter.SelectCommand.CommandText = "SELECT list.\"Lastname\", list.initials, list.\"group\", list.grade FROM  public.list;";
                //выполнение запроса select к базе, запись ответа в dataAdapter
                dataAdapter.Fill(dataSet); //выгрузка данных из dataAdapter в структуру dataSet
                //Random rn = new Random();
                Student s = new Student();
                List<Student> ls = new List<Student>();
                //string[] x = new string[10];
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
                        s.grade = Double.Parse(row[3].ToString()); //todo: tryparse
                        ls.Add(s);
                        foreach (DataColumn column in table.Columns)
                        {
                            string encBuf;
                            encBuf = row[column].ToString();
                            encBuf = ChangeEncoding(encBuf); //смотри комментарии выше
                            Console.Write(encBuf + "\t");
                            
                        }
                        Console.Write("\n");
                    }
                }
                connection.Close();
                /*//ручной метод заполнения
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
                ls.Add(s);*/ 

                ls.Sort(CompareGrades);
                Console.WriteLine("\nСортированный вывод:");
                Console.Write("Фамилия ИО\tГруппа\tСредний балл\n");
                foreach (Student st in ls)
                {
                    Console.Out.WriteLine(st.lastname + "\t" + st.initials + "\t" + st.groupn + "\t" + st.grade);
                }
            }
            catch(Exception ex)
            {


                Console.Out.WriteLine("Не удалось выполнить программу. Текст ошибки: " + ex);
            }
            Console.In.Read();
            
            

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

    

    struct Student
    {
        public string lastname;
        public string initials;
        public string groupn;
        public double grade;
    }

    
}
