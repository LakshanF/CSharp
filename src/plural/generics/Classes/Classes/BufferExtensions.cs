using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Classes
{
    public delegate void Printer<T>(T data);
    public static class BufferExtensions
    {
        public static void Dump1<T>(this Buffer<T> buffer, Printer<T> print)
        {
            foreach (var item in buffer)
            {
                print(item);
            }
        }

        public static void Dump2<T>(this Buffer<T> buffer, Action<T> print)
        {
            foreach (var item in buffer)
            {
                print(item);
            }
        }


        public static IEnumerable<TOut> AsEnumerable<T, TOut>(this Buffer<T> buffer)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            foreach (var item in buffer)
            {
                yield return (TOut)converter.ConvertTo(item, typeof(TOut));
            }
        }

        public static IEnumerable<TOut> Map<T, TOut>(this Buffer<T> buffer, Converter<T,TOut> converter)
        {
            return buffer.Select(i=>converter(i));
        }

    }
}
