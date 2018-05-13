using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;


namespace PianoRoll.Model
{
    class Ini
    {
        enum Type
        {
            Empty,
            Value,
            ValueList,
            Values,
            ValuesList,
            KeyValues
        }

        public string Dir;
        public Dictionary<string, dynamic> data = new Dictionary<string, dynamic> { };
        public List<string> Sections = new List<string> { };

        public Ini(string INIDir)
        {
            Dir = INIDir;
            Load();
        }

        public void Load()
        {

            string[] ini = File.ReadAllLines(Dir, Encoding.GetEncoding(932));
            int i = 0;
            Regex SectionRegex = new Regex(@"\[(.*)\]");

            string section = "default";
            string value = "";
            int SectionsNumber = 0;

            while (i < ini.Length)
            {
                dynamic temp = "";
                int l = 0;
                Type type = Type.Empty;
                while (!SectionRegex.IsMatch(ini[i]))
                {
                    string[] line = ini[i].Split('=');
                    if (line.Length == 1)
                    {
                        string[] values = line[0].Split(',');
                        // value string
                        if (values.Length == 1)
                        {
                            value = values[0];
                            Console.WriteLine($"Line '{ini[i]}', type value: {value}");
                            if (type == Type.Empty)
                            {
                                temp = value;
                                type = Type.Value;
                            }
                            else if (type == Type.Value)
                            {
                                type = Type.ValueList;
                                string oldvalue = temp;
                                temp = new List<string> { };
                                temp.Add(value);
                            }
                            if (type == Type.ValueList)
                            {
                                temp.Add(value);
                            }
                        }
                        // value1,value2...valueN string
                        else
                        {
                            Console.WriteLine($"Line '{ini[i]}', type value1,value2...valueN: {String.Join(",", values)}");
                            if (type == Type.Empty)
                            {
                                temp = values;
                                type = Type.Values;
                            }
                            else if (type == Type.Values)
                            {
                                type = Type.ValuesList;
                                string oldvalues = temp;
                                temp = new List<string[]> { };
                                temp.Add(oldvalues);
                            }
                            if (type == Type.ValuesList)
                            {
                                temp.Add(values);
                            }
                        }
                    }
                    else
                    {
                        if (type == Type.Empty)
                        {
                            temp = new Dictionary<string, dynamic> { };
                        }
                        string key = line[0];
                        string[] values = line[1].Split(',');
                        // key=value string
                        if (values.Length == 1)
                        {
                            value = values[0];
                            Console.WriteLine($"Line '{ini[i]}', type key=value: {key}={value}");
                            if (type == Type.Empty)
                            {
                                type = Type.KeyValues;
                            }
                            if (type == Type.KeyValues)
                            {
                                temp[key] = value;
                            }
                        }
                        // key=value1,value2...valueN string
                        else
                        {
                            Console.WriteLine($"Line '{ini[i]}', type key=value1,value2...valueN: {key}={String.Join(",", values)}");
                            if (type == Type.Empty)
                            {
                                type = Type.KeyValues;
                            }
                            if (type == Type.KeyValues)
                            {
                                temp[key] = values;
                            }
                        }
                    }
                    if (i == ini.Length-1) break;
                    i++;
                    l++;
                }
                data[section] = temp;
                Sections.Add(section);
                section = SectionRegex.Match(ini[i]).Value;
                i++;
                SectionsNumber++;
            }
            data["SectionsNumber"] = SectionsNumber;
        }

        public void Write(string Section, string Key, string Value)
        {
            //WritePrivateProfileString(Section, Key, Value, this.Dir);
        }


        public string Read(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            //int i = GetPrivateProfileString(Section, Key, "", temp, 255, this.Dir);
            return temp.ToString();

        }
    }
}
