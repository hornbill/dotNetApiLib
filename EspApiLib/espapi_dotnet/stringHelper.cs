using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hornbill
{
    internal static class stringHelper
    {
        internal static string AppendUrl(string s1, string s2)
        {
            return AppendUrl(s1, s2, true);
        }

        internal static string AppendUrl(string s1, string s2, bool appendSlash)
        {
            if (s1.Length == 0 || s2.Length == 0)
                return s1 + s2;
            StringBuilder sb = new StringBuilder();
            if (s1.EndsWith("/") && s2.StartsWith("/"))
            {
                sb.AppendFormat("{0}{1}{2}", s1, s2.Substring(1), appendSlash ? "/" : string.Empty);
            }
            else if (s1.EndsWith("/") || s2.StartsWith("/"))
            {
                sb.AppendFormat("{0}{1}{2}", s1, s2, appendSlash ? "/" : string.Empty);
            }
            else
            {
                sb.AppendFormat("{0}/{1}{2}", s1, s2, appendSlash ? "/" : string.Empty);
            }
            return sb.ToString();
        }

        internal static string PrepareForFilePath(string strFilePath)
        {
            return strFilePath.Replace('/', '\\');
        }

        internal static string PrepareForFileName(string strFileName)
        {
            StringBuilder sb = new StringBuilder(strFileName);
            sb.Replace('\\', '_');
            sb.Replace('/', '_');
            sb.Replace(':', '_');
            sb.Replace('*', '_');
            sb.Replace('?', '_');
            sb.Replace('"', '_');
            sb.Replace('\'', '_');
            sb.Replace('<', '_');
            sb.Replace('>', '_');
            return sb.ToString();
        }

        #region PrepareForXml
        internal static string PrepareForXml(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;
            return s.EncodeXmlMarkup();
        }
        #endregion

        #region EncodeXmlMarkup
        internal static string EncodeXmlMarkup(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;
            return System.Security.SecurityElement.Escape(s);
        }
        #endregion

        internal static string Base64Encode(string toEncode)
        {
            byte[] toEncodeAsBytes
                  = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
            return System.Convert.ToBase64String(toEncodeAsBytes);
        }

        internal static string Base64Decode(string encodedData)
        {
            byte[] encodedDataAsBytes
                = System.Convert.FromBase64String(encodedData);
            return System.Text.ASCIIEncoding.ASCII.GetString(encodedDataAsBytes);
        }

        internal static byte[] UTF8Encode(string toEncode)
        {
            return System.Text.UTF8Encoding.UTF8.GetBytes(toEncode);
        }

        internal static string UTF8Decode(byte[] encodedData)
        {
            return System.Text.UTF8Encoding.UTF8.GetString(encodedData);
        }


        internal static string RandomString(int size)
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            return builder.ToString();
        }

        #region GetBytes
        // Convert it to byte without any encoding
        static public byte[] GetBytes(this string s)
        {
            if (s == null)
                return null;
            if (s.Length == 0)
                return new byte[] { };

            byte[] bytes = new byte[s.Length * sizeof(char)];
            System.Buffer.BlockCopy(s.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
        #endregion
    }
}
