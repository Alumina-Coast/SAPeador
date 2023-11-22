using System;

namespace SAPeador.Exceptions
{
	/// <summary>
	/// Thrown if something goes wrong when handling sessions in the SAP GUI Client.
	/// </summary>
	public class SapSessionException : Exception
	{
		/// <summary>
		/// Thrown if something goes wrong when handling sessions in the SAP GUI Client.
		/// </summary>
		/// <param name="message">Details of what went wrong.</param>
		public SapSessionException(string message) : base(message) { }
	}
}
