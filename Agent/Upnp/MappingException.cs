using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Vinchuca.Upnp
{
	[Serializable]
	public class MappingException : Exception
	{
		public int ErrorCode { get; private set; }
		public string ErrorText { get; private set; }

		internal MappingException(int errorCode, string errorText)
			: base(string.Format("Error {0}: {1}", errorCode, errorText))
		{
			ErrorCode = errorCode;
			ErrorText = errorText;
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null) throw new ArgumentNullException("info");

			ErrorCode = info.GetInt32("errorCode");
			ErrorText = info.GetString("errorText");
			base.GetObjectData(info, context);
		}
	}
}