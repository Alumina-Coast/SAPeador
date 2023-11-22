using System;

namespace SAPeador.Exceptions
{
	/// <summary>
	/// Thrown if something goes wrong when working with the SAP GUI Client itself.
	/// </summary>
	public class SapApplicationException : Exception
	{
		/// <summary>
		/// Thrown if something goes wrong when working with the SAP GUI Client itself.
		/// </summary>
		/// <param name="message">Details of what went wrong.</param>
		public SapApplicationException(string message) : base(message) { }
	}
}
