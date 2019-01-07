using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace SimpleP2P
{
    public static class Shared
    {
        public static string GetPublicIPAddress()
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                var endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint.Address.ToString();
            }
        }

        public static string Timestamp()
        {
            return DateTime.Now.ToLongTimeString();
        }

        public static void Log(string message, [CallerMemberName] string callerName = "")
        {
            Console.WriteLine($"{Timestamp()} - {callerName,-6}: {message}");
        }

        public static string ToXml(this object obj)
        {
            if (obj == null) throw new ArgumentNullException("The object to be serialized cannot be null");
            try
            {
                var serializer = new XmlSerializer(obj.GetType());
                using (var stringWriter = new StringWriter())
                {
                    serializer.Serialize(stringWriter, obj);
                    return stringWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static T FromXml<T>(this string xml)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(new StringReader(xml));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}