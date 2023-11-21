using System;

namespace SAPeador.Exceptions
{
	public class SapApplicationException : Exception
	{
		public SapApplicationException(string message) : base(message) { }
	}
}
