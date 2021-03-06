﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;

namespace SQLite
{
    public class SQLiteDataBase
    {
        protected String dbFileName;
        protected SQLiteConnection Connection;
        protected SQLiteCommand Command;

        public string ErrorMsg;

        public SQLiteDataBase(string FileName = "")
        {
            dbFileName = FileName;
            Connection = new SQLiteConnection();
            Command = new SQLiteCommand();
        }

        static public SQLiteDataBase Create(string FileName, string Query)
        {
            SQLiteDataBase NewBase = new SQLiteDataBase(FileName);

            if (NewBase.CreateDB(Query))
                return NewBase;
            else
                return null;
        }

        static public SQLiteDataBase Open(string FileName)
        {
            if (File.Exists(FileName))
            {
                SQLiteDataBase NewBase = new SQLiteDataBase(FileName);

                if (NewBase.OpenDB())
                    return NewBase;
                else
                    return null;
            }
            return null;
        }

        protected bool OpenDB()
        {
            if (!File.Exists(dbFileName))
            {
                ErrorMsg = "Database file not found";
                return false;
            }

            try
            {
                Connection = new SQLiteConnection("Data Source=" + dbFileName + ";Version=3;");
                Connection.Open();
                Command.Connection = Connection;
            }
            catch (SQLiteException ex)
            {
                ErrorMsg = ex.Message;
                return false;
            }
            return true;
        }

        protected bool CreateDB(string Query)
        {
            if (!File.Exists(dbFileName))
                SQLiteConnection.CreateFile(dbFileName);

            try
            {
                Connection = new SQLiteConnection("Data Source=" + dbFileName + ";Version=3;");
                Connection.Open();
                Command.Connection = Connection;

                Command.CommandText = Query;
                Command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                ErrorMsg = ex.Message;
                return false;
            }
            return true;
        }

        public DataTable ReadTable(string Query)
        {
            DataTable dTable = new DataTable();

            if (Connection.State != ConnectionState.Open)
            {
                ErrorMsg = "Open Database";
                return null;
            }

            try
            {
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(Query, Connection);
                adapter.Fill(dTable);
            }
            catch (SQLiteException ex)
            {
                ErrorMsg = ex.Message;
                return null;
            }

            return dTable;
        }

        public int GetCount(string Table, string Where = null)
        {
            DataTable dTable = new DataTable();

            if (Connection.State != ConnectionState.Open)
            {
                ErrorMsg = "Open Database";
                return 0;
            }

            try
            {
                string Query = Where == null ? "SELECT COUNT() AS 'C' FROM `" + Table + ";" : "SELECT COUNT() AS 'C' FROM `" + Table + "` WHERE " + Where + ";";
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(Query, Connection);
                adapter.Fill(dTable);
            }
            catch (SQLiteException ex)
            {
                ErrorMsg = ex.Message;
                return 0;
            }

            return Convert.ToInt32(dTable.Rows[0].ItemArray[0].ToString());
        }

        public bool Execute(string Query)
        {
            if (Connection.State != ConnectionState.Open)
            {
                ErrorMsg = "Open connection with database";
                return false;
            }

            try
            {
                Command.CommandText = Query;
                Command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                ErrorMsg = ex.Message;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Выдаёт один первый из списка
        /// </summary>
        /// <param name="Field">Поле, из которого выдаётся информация</param>
        /// <param name="Table">ТаблицаПоле, из которой выдаётся информация</param>
        /// <param name="Condition">Условие</param>
        /// <returns></returns>
        protected DataRow GetOne(string Field, string Table, string Condition)
        {
            DataTable Conf = ReadTable("SELECT " + Field + " FROM `" + Table +
                "` WHERE " + Condition + " LIMIT 1");
            if (Conf.Rows.Count == 0) return null;
            return Conf.Rows[0];
        }
    }

    public class SQLiteConfig : SQLiteDataBase
    {
        public SQLiteConfig(string FileName) : base (FileName)
        {
            if (File.Exists(FileName))
            {
                OpenDB();
            }
            else
            {
                CreateDB("CREATE TABLE `config` (`id` INTEGER PRIMARY KEY AUTOINCREMENT, `name`	TEXT NOT NULL, `value` TEXT); ");
            }
        }

        public static new SQLiteConfig Open(string FileName)
        {
            return new SQLiteConfig(FileName);
        }

        //Работа с конфигом, получение значения

        /// <summary>
        /// Получает текстовый параметр из БД
        /// </summary>
        /// <param name="name">Имя параметра</param>
        /// <returns></returns>
        public string GetConfigValue(string name)
        {
            DataTable Conf = ReadTable("SELECT `value` FROM `config` WHERE `name`='" + name + "' LIMIT 1");
            if (Conf.Rows.Count == 0) return "";
            return Conf.Rows[0].ItemArray[0].ToString();
        }

        /// <summary>
        /// Получает числовой параметр из БД
        /// </summary>
        /// <param name="name">Имя параметра</param>
        /// <returns></returns>
        public int GetConfigValueInt(string name)
        {
            DataTable Conf = ReadTable("SELECT `value` FROM `config` WHERE `name`='" + name + "' LIMIT 1");
            if (Conf.Rows.Count == 0) return 0;
            return Convert.ToInt32(Conf.Rows[0].ItemArray[0].ToString());
        }

        /// <summary>
        /// Получает бинарный параметр из БД
        /// </summary>
        /// <param name="name">Имя параметра</param>
        /// <returns></returns>
        public bool GetConfigValueBool(string name)
        {
            DataTable Conf = ReadTable("SELECT `value` FROM `config` WHERE `name`='" + name + "' LIMIT 1");
            if (Conf.Rows.Count == 0) return false;
            return Conf.Rows[0].ItemArray[0].ToString() == "1";
        }

        /// <summary>
        /// Возвращает список строковых параметров с одинаковым именем
        /// </summary>
        /// <param name="name">Имя элементов списка</param>
        /// <returns></returns>
        public List<string> GetStringList(string name)
        {
            List<string> Out = new List<string>();
            DataTable Conf = ReadTable("SELECT `value` FROM `config` WHERE `name`='" + name + "';");

            foreach (DataRow Row in Conf.Rows)
            {
                Out.Add(Row.ItemArray[0].ToString());
            }
            return Out;
        }

        /// <summary>
        /// Возвращает список численных параметров с одинаковым именем
        /// </summary>
        /// <param name="name">Имя элементов списка</param>
        /// <returns></returns>
        public List<int> GetIntList(string name)
        {
            List<int> Out = new List<int>();
            DataTable Conf = ReadTable("SELECT `value` FROM `config` WHERE `name`='" + name + "';");

            foreach (DataRow Row in Conf.Rows)
            {
                Out.Add(Convert.ToInt32(Row.ItemArray[0].ToString()));
            }
            return Out;
        }



        //Работа с конфигом, установка значения

        /// <summary>
        /// Устанавливает параметр БД
        /// </summary>
        /// <param name="name">Название параметра</param>
        /// <param name="value">Строковае значение</param>
        /// <returns></returns>
        public bool SetConfigValue(string name, string value)
        {
            if (GetCount("config", "`name`='" + name + "'") > 0)
                return Execute("UPDATE `config` SET `value`='" + value + "' WHERE `name`='" + name + "';");
            else
                return Execute("INSERT INTO `config` (`name`, `value`) VALUES ('" + name + "','" + value + "');");
        }

        /// <summary>
        /// Устанавливает параметр БД
        /// </summary>
        /// <param name="name">Название параметра</param>
        /// <param name="value">Численное значение</param>
        /// <returns></returns>
        public bool SetConfigValue(string name, int value)
        {
            if (GetCount("config", "`name`='" + name + "'") > 0)
                return Execute("UPDATE `config` SET `value`='" + value.ToString() + "' WHERE `name`='" + name + "'");
            else
                return Execute("INSERT INTO `config` (`name`, `value`) VALUES ('" + name + "','" + value.ToString() + "');");
        }

        /// <summary>
        /// Устанавливает параметр БД
        /// </summary>
        /// <param name="name">Название параметра</param>
        /// <param name="value">Бинарное значение</param>
        /// <returns></returns>
        public bool SetConfigValue(string name, bool value)
        {
            string val = value ? "1" : "0";
            if (GetCount("config", "`name`='" + name + "'") > 0)
                return Execute("UPDATE `config` SET `value`='" + val + "' WHERE `name`='" + name + "'");
            else
                return Execute("INSERT INTO `config` (`name`, `value`) VALUES ('" + name + "','" + val + "');");
        }
    }

    public class SQLiteLanguage : SQLiteDataBase
    {
        private string language = "ru";

        public string Language
        {
            get
            {
                return language;
            }

            set
            {
                language = value;
            }
        }

        public SQLiteLanguage(string FileName)
        {
            dbFileName = FileName;
            Connection = new SQLiteConnection();
            Command = new SQLiteCommand();
        }

        public static new SQLiteLanguage Open(string FileName, string Lang="ru")
        {
            if (File.Exists(FileName))
            {
                SQLiteLanguage NewConf = new SQLiteLanguage(FileName)
                { Language = Lang };

                if (NewConf.OpenDB())
                    return NewConf;
                else
                    return null;
            }
            else
            {
                SQLiteLanguage NewConf = new SQLiteLanguage(FileName)
                { Language = Lang == "" ? "ru" : Lang };
                if (NewConf.CreateDB("CREATE TABLE `texts` (`id` INTEGER PRIMARY KEY AUTOINCREMENT, `class` TEXT NOT NULL, `name` TEXT NOT NULL, `text_" + Lang + "` TEXT); "))
                    return NewConf;
                else
                    return null;
            }
        }

        public void AddLanguage(string LanguageName)
        {
            this.Execute("ALTER TABLE `texts` ADD COLUMN `text_" + LanguageName + "` TEXT;");
        }

        public string GetText(string Class, string TextName)
        {
            DataTable Conf = ReadTable("SELECT `text_" + Language + "` AS 'text' FROM `texts` WHERE `class`='" + Class + "' AND `name`='" + TextName + "' LIMIT 1");
            return Conf == null ? "ERROR" : Conf.Rows[0].ItemArray[Conf.Columns.IndexOf("text")].ToString();
        }
    }
}
