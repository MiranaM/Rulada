using System.Collections.Generic;
using System.Text;

namespace PianoRoll.Util
{
    internal class Base64
    {
        #region singleton base

        private static Base64 current;
        private Base64()
        {

        }

        public static Base64 Current
        {
            get
            {
                if (current == null)
                {
                    current = new Base64();
                }
                return current;
            }
        }

        #endregion

        public string Base64EncodeInt12(int[] data)
        {
            var l = new List<string>();
            foreach (var d in data) l.Add(Base64EncodeInt12(d));
            var base64 = new StringBuilder();
            var last = "";
            var dups = 0;
            foreach (var b in l)
            {
                if (last == b)
                {
                    dups++;
                }
                else if (dups == 0)
                {
                    base64.Append(b);
                }
                else
                {
                    base64.Append('#');
                    base64.Append(dups + 1);
                    base64.Append('#');
                    dups = 0;
                    base64.Append(b);
                }

                last = b;
            }

            if (dups != 0)
            {
                base64.Append('#');
                base64.Append(dups + 1);
                base64.Append('#');
            }

            return base64.ToString();
        }

        private const string intToBase64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

        private string Base64EncodeInt12(int data)
        {
            if (data < 0) data += 4096;
            var base64 = new char[2];
            base64[0] = intToBase64[(data >> 6) & 0x003F];
            base64[1] = intToBase64[data & 0x003F];
            return new string(base64);
        }
    }
}