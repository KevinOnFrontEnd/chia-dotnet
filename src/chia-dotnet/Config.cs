﻿using System;
using System.Diagnostics;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Collections.Generic;

using YamlDotNet.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace chia.dotnet
{
    public sealed class Config
    {
        private dynamic _config;

        internal Config([NotNull] dynamic config)
        {
            _config = config;
        }

        public EndpointInfo GetEndpoint(string name)
        {
			IDictionary<string, object> d = _config;
			dynamic section = d[name];
			UriBuilder builder = new("wss", section.daemon_host, Convert.ToInt32(section.daemon_port));

			dynamic ssl = section.daemon_ssl;

            return new EndpointInfo
			{
				Uri = builder.Uri,
				CertPath = Path.Combine(DefaultRoot, ssl.private_crt),
				KeyPath = Path.Combine(DefaultRoot, ssl.private_key)
			};
        }

        [return: NotNull]
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

		public static string DefaultRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".chia", "mainnet");

		[return: NotNull]
        public static Config Open()
        {
            var homedir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Open(Path.Combine(DefaultRoot, "config", "config.yaml"));
        }

		// sigh... YAML
		// https://stackoverflow.com/questions/32757084/yamldotnet-how-to-handle-set
		private class YamlSet<T> : HashSet<T>, IDictionary<T, object>
		{
			void IDictionary<T, object>.Add(T key, object value)
			{
				Add(key);
			}

			bool IDictionary<T, object>.ContainsKey(T key)
			{
				throw new NotImplementedException();
			}

			ICollection<T> IDictionary<T, object>.Keys
			{
				get { throw new NotImplementedException(); }
			}

			bool IDictionary<T, object>.Remove(T key)
			{
				throw new NotImplementedException();
			}

			bool IDictionary<T, object>.TryGetValue(T key, out object value)
			{
				throw new NotImplementedException();
			}

			ICollection<object> IDictionary<T, object>.Values
			{
				get { throw new NotImplementedException(); }
			}

			object IDictionary<T, object>.this[T key]
			{
				get
				{
					throw new NotImplementedException();
				}
				set
				{
					Add(key);
				}
			}

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

			int ICollection<KeyValuePair<T, object>>.Count
			{
				get { return base.Count; }
			}

			bool ICollection<KeyValuePair<T, object>>.IsReadOnly
			{
				get { throw new NotImplementedException(); }
			}

			bool ICollection<KeyValuePair<T, object>>.Remove(KeyValuePair<T, object> item)
			{
				throw new NotImplementedException();
			}

			IEnumerator<KeyValuePair<T, object>> IEnumerable<KeyValuePair<T, object>>.GetEnumerator()
			{
				IDictionary<T, object> dict = new Dictionary<T, object>();
				T[] keys = new T[base.Count];
				base.CopyTo(keys);
				foreach (T k in keys)
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
