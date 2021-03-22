﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ShieldVSExtension.Configuration
{
    public class SolutionConfiguration
    {
        public bool IsEnabled { get; set; } = true;
        public string TargetDirectory { get; set; }
        /*
         *  maximum
         *  balance
         *  optimized
         *  custom
         */
        public string ProjectPreset { get; set; } = "balance";
        [JsonProperty]
        public List<string> Protections { get; set; }
        public List<ProjectConfiguration> Projects { get; private set; } = new List<ProjectConfiguration>();
        public static async Task SaveAsync(SolutionConfiguration configuration, Stream stream)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            // ReSharper disable once MethodHasAsyncOverload
            var json = JsonConvert.SerializeObject(configuration, Formatting.None);
            var data = Encoding.UTF8.GetBytes(json);

            using (var ms = new MemoryStream())
            {
                using (var zip = new GZipStream(ms, CompressionLevel.Optimal))
                    await zip.WriteAsync(data, 0, data.Length);

                data = ms.ToArray();
            }

            await stream.WriteAsync(BitConverter.GetBytes(data.Length), 0, 4);
            await stream.WriteAsync(data, 0, data.Length);
        }
        public static void Save(SolutionConfiguration configuration, Stream stream)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            // ReSharper disable once MethodHasAsyncOverload
            var json = JsonConvert.SerializeObject(configuration, Formatting.None);
            var data = Encoding.UTF8.GetBytes(json);

            using (var ms = new MemoryStream())
            {
                using (var zip = new GZipStream(ms, CompressionLevel.Optimal))
                    zip.Write(data, 0, data.Length);

                data = ms.ToArray();
            }

            stream.Write(BitConverter.GetBytes(data.Length), 0, 4);
            stream.Write(data, 0, data.Length);
        }
        public static async Task<SolutionConfiguration> LoadAsync(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var lengthBuffer = new byte[4];
            await stream.ReadAsync(lengthBuffer, 0, 4);

            var length = BitConverter.ToInt32(lengthBuffer, 0);
            var data = new byte[length];
            await stream.ReadAsync(data, 0, length);

            using (var ms = new MemoryStream(data))
            using (var zip = new GZipStream(ms, CompressionMode.Decompress))
            {
                var ms2 = new MemoryStream();
                await zip.CopyToAsync(ms2);

                var json = Encoding.UTF8.GetString(ms2.ToArray());
                return JsonConvert.DeserializeObject<SolutionConfiguration>(json);
            }
        }
        public static SolutionConfiguration Load(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var lengthBuffer = new byte[4];
            stream.Read(lengthBuffer, 0, 4);

            var length = BitConverter.ToInt32(lengthBuffer, 0);
            var data = new byte[length];
            stream.Read(data, 0, length);

            using (var ms = new MemoryStream(data))
            using (var zip = new GZipStream(ms, CompressionMode.Decompress))
            {
                var ms2 = new MemoryStream();
                zip.CopyTo(ms2);

                var json = Encoding.UTF8.GetString(ms2.ToArray());
                return JsonConvert.DeserializeObject<SolutionConfiguration>(json);
            }
        }
    }
}
