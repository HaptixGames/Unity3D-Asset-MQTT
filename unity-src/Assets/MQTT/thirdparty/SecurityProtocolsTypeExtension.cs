namespace System.Net
{
	using System.Security.Authentication;
	public static class SecurityProtocolTypeExtension
	{
		public const SecurityProtocolType Tls_12 = (SecurityProtocolType) SslProtocolsExtension.Tls_12;
		public const SecurityProtocolType Tls_11 = (SecurityProtocolType) SslProtocolsExtension.Tls_11;
		public const SecurityProtocolType SystemDefault = (SecurityProtocolType) 0;
	}
}