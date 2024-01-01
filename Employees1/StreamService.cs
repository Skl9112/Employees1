using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Json;

namespace Employees1
{
    public class StreamService<T>
    {
        public async Task WriteToStreamAsync(Stream stream, IEnumerable<T> data, IProgress<string> progress)
        {
            foreach (var item in data)
            {
                byte[] bytes = ConvertToBytes(item);
                await stream.WriteAsync(bytes, 0, bytes.Length);

                await Task.Delay(3000);

                progress.Report($"Поток {Thread.CurrentThread.ManagedThreadId}: записано {bytes.Length} байт");
            }
        }

        public async Task CopyFromStreamAsync(Stream stream, string filename, IProgress<string> progress)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                await stream.CopyToAsync(fs);
            }

            progress.Report($"Поток {Thread.CurrentThread.ManagedThreadId}: копирование завершено");
        }

        public async Task<int> GetStatisticsAsync(string fileName, Func<T, bool> filter)
        {
            List<T> data = await ReadDataFromFileAsync(fileName);

            int count = data.Count(filter);

            return count;
        }

        private async Task<List<T>> ReadDataFromFileAsync(string fileName)
        {
            List<T> data = new List<T>();

            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                byte[] buffer = new byte[fs.Length];
                await fs.ReadAsync(buffer, 0, buffer.Length);

                for (int i = 0; i < buffer.Length; i += Marshal.SizeOf(typeof(T)))
                {
                    T item = ConvertFromBytes(buffer.Skip(i).Take(Marshal.SizeOf(typeof(T))).ToArray());
                    data.Add(item);
                }
            }

            return data;
        }

        private byte[] ConvertToBytes(T item)
        {
            string serializedItem = JsonConverter.Serialization(item);
            return System.Text.Encoding.UTF8.GetBytes(serializedItem);
        }

        private T ConvertFromBytes(byte[] bytes)
        {
            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                string serializedItem = System.Text.Encoding.UTF8.GetString(bytes);
                return JsonConvert.DeserializeObject<T>(serializedItem);
            }
        }
    }
}
