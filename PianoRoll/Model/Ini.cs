using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace PianoRoll.Model
{
    internal class Ini
    {
        private enum Type
        {
            Empty,
            Value,
            ValueList,
            Values,
            ValuesList,
            KeyValues
        }

        public string Dir;
        public Dictionary<string, dynamic> data = new Dictionary<string, dynamic>();
        public List<string> Sections = new List<string>();

        public Ini(string INIDir)
        {
            Dir = INIDir;
            Load();
        }

        public void Load()
        {
            var ini = File.ReadAllLines(Dir, Encoding.GetEncoding(932));
            var i = 0;
            var SectionRegex = new Regex(@"\[(.*)\]");

            var section = "default";
            var value = "";
            var SectionsNumber = 0;

            while (i < ini.Length)
            {
                dynamic temp = "";
                var l = 0;
                var type = Type.Empty;
                while (!SectionRegex.IsMatch(ini[i]))
                {
                    var line = ini[i].Split('=');
                    if (line.Length == 1)
                    {
                        var values = line[0].Split(',');
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
                                temp = new List<string>();
                                temp.Add(value);
                            }

                            if (type == Type.ValueList) temp.Add(value);
                        }
                        // value1,value2...valueN string
                        else
                        {
                            Console.WriteLine(
                                $"Line '{ini[i]}', type value1,value2...valueN: {string.Join(",", values)}");
                            if (type == Type.Empty)
                            {
                                temp = values;
                                type = Type.Values;
                            }
                            else if (type == Type.Values)
                            {
                                type = Type.ValuesList;
                                string oldvalues = temp;
                                temp = new List<string[]>();
                                temp.Add(oldvalues);
                            }

                            if (type == Type.ValuesList) temp.Add(values);
                        }
                    }
                    else
                    {
                        if (type == Type.Empty) temp = new Dictionary<string, dynamic>();

                        var key = line[0];
                        var values = line[1].Split(',');
                        // key=value string
                        if (values.Length == 1)
                        {
                            value = values[0];
                            Console.WriteLine($"Line '{ini[i]}', type key=value: {key}={value}");
                            if (type == Type.Empty) type = Type.KeyValues;

                            if (type == Type.KeyValues) temp[key] = value;
                        }
                        // key=value1,value2...valueN string
                        else
                        {
                            Console.WriteLine(
                                $"Line '{ini[i]}', type key=value1,value2...valueN: {key}={string.Join(",", values)}");
                            if (type == Type.Empty) type = Type.KeyValues;

                            if (type == Type.KeyValues) temp[key] = values;
                        }
                    }

                    if (i == ini.Length - 1) break;
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
            var temp = new StringBuilder(255);
            //int i = GetPrivateProfileString(Section, Key, "", temp, 255, this.Dir);
            return temp.ToString();
        }
    }
}