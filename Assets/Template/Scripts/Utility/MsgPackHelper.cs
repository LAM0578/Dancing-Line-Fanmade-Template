using System;
using System.IO;
using MsgPack;
using MsgPack.Serialization;

namespace DancingLineSample.Utility
{
	public static class MsgPackHelper
	{
		public static byte[] Serialize<T>(T obj)
		{
			var serializer = MessagePackSerializer.Get<T>();
			using (var stream = new MemoryStream())
			{
				serializer.Pack(stream, obj);
				return stream.ToArray();
			}
		}

		public static T Deserialize<T>(byte[] bytes)
		{
			var serializer = MessagePackSerializer.Get<T>();
			using (var stream = new MemoryStream(bytes))
			{
				return serializer.Unpack(stream);
			}
		}

		public static T TryDeserialize<T>(byte[] bytes)
		{
			try
			{
				return Deserialize<T>(bytes);
			}
			catch (InvalidMessagePackStreamException)
			{
				return default;
			}
		}
		
		public static T TryDeserialize<T>(byte[] bytes, T defaultValue)
		{
			try
			{
				return Deserialize<T>(bytes);
			}
			catch (InvalidMessagePackStreamException)
			{
				return defaultValue;
			}
		}

		public static T TryReadAndDeserializeFromFile<T>(string path, T defaultValue)
		{
			var bytes = FileUtility.TryReadBytesToFile(path, Serialize(defaultValue));
			return TryDeserialize<T>(bytes);
		}

		public static void SerializeToFile<T>(string path, T obj)
		{
			File.WriteAllBytes(path, Serialize(obj));
		}
	}
}
