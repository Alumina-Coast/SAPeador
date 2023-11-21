using System;

namespace SAPeador.Exceptions
{
	public class SapSessionException : Exception
	{
		public SapSessionException(string message) : base(message) { }
	}
}
