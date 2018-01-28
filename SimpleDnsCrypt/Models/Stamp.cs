﻿using System;
using System.Text;
using Helper;
using SimpleDnsCrypt.Extensions;
using Sodium;

namespace SimpleDnsCrypt.Models
{
	public enum StampProtocolType : byte
	{
		StampProtoTypePlain = 0x00,
		StampProtoTypeDnsCrypt = 0x01,
		StampProtoTypeDoH = 0x02
	}

	public class Stamp
	{
		private const string StampPrefix = "sdns://";

		public Stamp()
		{
		}

		public Stamp(string stamp)
		{
			try
			{
				if (!stamp.StartsWith(StampPrefix)) return;
				Prefix = StampPrefix;
				var stampWithoutPrefix = stamp.Remove(0, StampPrefix.Length);
				var stampBinary = stampWithoutPrefix.FromBase64Url();
				if (stampBinary == null) return;
				const int typeDescriptionLength = 1;
				const int addressDescriptionLength = 1;
				const int publicKeyDescriptionLength = 1;
				const int providerNameDescriptionLength = 1;
				const int propertiesLength = 8;

				Type = (StampProtocolType) Enum.ToObject(typeof(StampProtocolType), stampBinary[0]);
				Properties = ArrayHelper.SubArray(stampBinary, typeDescriptionLength, propertiesLength);
				var addressLength =
					ArrayHelper.SubArray(stampBinary, typeDescriptionLength + propertiesLength, addressDescriptionLength)[0];
				Address = Encoding.UTF8.GetString(ArrayHelper.SubArray(stampBinary,
					typeDescriptionLength + propertiesLength + addressDescriptionLength, addressLength));

				//TODO: maybe use properties?
				//Workaground: IPv6
				if (Address.StartsWith("["))
				{
					Ipv6 = true;
				}

				var publicKeyLength = ArrayHelper.SubArray(stampBinary,
					typeDescriptionLength +
					propertiesLength +
					addressDescriptionLength +
					addressLength,
					publicKeyDescriptionLength)[0];
				PublicKey = Utilities.BinaryToHex(ArrayHelper.SubArray(stampBinary,
					typeDescriptionLength + propertiesLength + addressDescriptionLength + addressLength + publicKeyDescriptionLength,
					publicKeyLength));
				var providerNameLength = ArrayHelper.SubArray(stampBinary,
					typeDescriptionLength +
					propertiesLength +
					addressDescriptionLength +
					addressLength +
					publicKeyDescriptionLength +
					publicKeyLength,
					providerNameDescriptionLength)[0];
				ProviderName = Encoding.UTF8.GetString(ArrayHelper.SubArray(stampBinary,
					typeDescriptionLength +
					propertiesLength +
					addressDescriptionLength +
					addressLength +
					publicKeyDescriptionLength +
					publicKeyLength +
					providerNameDescriptionLength,
					providerNameLength));

				var props = BitConverter.ToUInt64(Properties, 0);
				switch (props)
				{
					case 0:
						NoLog = false;
						DnsSec = false;
						break;
					case 2:
						NoLog = true;
						DnsSec = false;
						break;
					case 3:
						NoLog = true;
						DnsSec = true;
						break;
				}

			}
			catch (Exception)
			{
			}
		}

		public bool NoLog { get; set; }
		public bool DnsSec { get; set; }
		public bool Ipv6 { get; set; }

		public string Prefix { get; set; }
		public StampProtocolType Type { get; set; }
		public byte[] Properties { get; set; }
		public string Address { get; set; }
		public string PublicKey { get; set; }
		public string ProviderName { get; set; }
	}
}