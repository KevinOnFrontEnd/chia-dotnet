﻿using System;
using System.Diagnostics;
using System.IO;
using System.Dynamic;
using System.Collections.Generic;

using YamlDotNet.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace chia.dotnet
{
    /// <summary>
    /// Represents the chia config yaml file and its contents. Used to find the uri and ssl certs needed to connect via a <see cref="System.Net.WebSockets.WebSocket"/>
    /// </summary>
    public sealed class Config
    {
        internal Config(dynamic config)
        {
            Contents = config;
        }

        /// <summary>
        /// The contents of the config yaml
        /// </summary>
        public dynamic Contents { get; private set; }

        /// <summary>
        /// Creates an <see cref="EndpointInfo"/> from the named service section
        /// </summary>
        /// <param name="serviceName">The setion name in the config file. Use 'daemon' for the root config that include 'sel_fhostname'; i.e. the local daemon</param>
        /// <returns>An <see cref="EndpointInfo"/> that can be used to connect to the given service's RPC interface</returns>
        public EndpointInfo GetEndpoint(string serviceName)
        {
            UriBuilder builder = new("wss", "");
            dynamic ssl;

            if (serviceName == "ui")
            {
                dynamic section = Contents.ui;
                builder.Host = section.daemon_host;
                builder.Port = Convert.ToInt32(section.daemon_port);
                ssl = section.daemon_ssl;
            }
            else
            {
                // all other service names resolve to the info at the root of the yaml config
                builder.Host = Contents.self_hostname;
                builder.Port = Convert.ToInt32(Contents.daemon_port);
                ssl = Contents.daemon_ssl;
            }

            return new EndpointInfo
            {
                Uri = builder.Uri,
                CertPath = Path.Combine(DefaultRootPath, ssl.private_crt),
                KeyPath = Path.Combine(DefaultRootPath, ssl.private_key)
            };
        }

        /// <summary>
        /// Opens a chia config yaml file
        /// </summary>
        /// <param name="fullPath">The full filesystem path to the config file</param>
        /// <returns>A <see cref="Config"/> instance</returns>
        public static Config Open(string fullPath)
        {
            Debug.Assert(!string.IsNullOrEmpty(fullPath));

            using var input = new StreamReader(fullPath);
            var deserializer = new DeserializerBuilder()
                .WithTagMapping("tag:yaml.org,2002:set", typeof(YamlSet<object>))
                .Build();
            var yamlObject = deserializer.Deserialize(input);
            var serializer = new SerializerBuilder()
                .JsonCompatible()
                .Build();

            var json = serializer.Serialize(yamlObject);
            dynamic config = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());

            return new Config(config);
        }

        /// <summary>
        /// The OS specific default location of the chia config file
        /// </summary>
        public static string DefaultRootPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".chia", "mainnet");

        /// <summary>
        /// Opens the <see cref="Config"/> from <see cref="DefaultRootPath"/> plus 'config' and 'config.yaml'
        /// </summary>
        /// <returns></returns>
        public static Config Open()
        {
            return Open(Path.Combine(DefaultRootPath, "config", "config.yaml"));
        }

        // sigh... YAML
        // https://stackoverflow.com/questions/32757084/yamldotnet-how-to-handle-set
        private class YamlSet<T> : HashSet<T>, IDictionary<T, object>
        {
            void IDictionary<T, object>.Add(T key, object value)
            {
                _ = Add(key);
            }

            bool IDictionary<T, object>.ContainsKey(T key)
            {
                throw new NotImplementedException();
            }

            ICollection<T> IDictionary<T, object>.Keys => throw new NotImplementedException();

            bool IDictionary<T, object>.Remove(T key)
            {
                throw new NotImplementedException();
            }

            bool IDictionary<T, object>.TryGetValue(T key, out object value)
            {
                throw new NotImplementedException();
            }

            ICollection<object> IDictionary<T, object>.Values => throw new NotImplementedException();

            object IDictionary<T, object>.this[T key] { get => throw new NotImplementedException(); set => _ = Add(key); }

            void ICollection<KeyValuePair<T, object>>.Add(KeyValuePair<T, object> item)
            {
                throw new NotImplementedException();
            }

            void ICollection<KeyValuePair<T, object>>.Clear()
            {
                throw new NotImplementedException();
            }

            bool ICollection<KeyValuePair<T, object>>.Contains(KeyValuePair<T, object> item)
            {
                throw new NotImplementedException();
            }

            void ICollection<KeyValuePair<T, object>>.CopyTo(KeyValuePair<T, object>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            int ICollection<KeyValuePair<T, object>>.Count => base.Count;

            bool ICollection<KeyValuePair<T, object>>.IsReadOnly => throw new NotImplementedException();

            bool ICollection<KeyValuePair<T, object>>.Remove(KeyValuePair<T, object> item)
            {
                throw new NotImplementedException();
            }

            IEnumerator<KeyValuePair<T, object>> IEnumerable<KeyValuePair<T, object>>.GetEnumerator()
            {
                IDictionary<T, object> dict = new Dictionary<T, object>();
                var keys = new T[base.Count];
                base.CopyTo(keys);
                foreach (var k in keys)
                {
                    dict.Add(k, null);
                }
                return dict.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return base.GetEnumerator();
            }
        }
    }
}
