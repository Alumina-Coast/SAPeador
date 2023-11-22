using SAPFEWSELib;

namespace SAPeador
{
	public enum InteractionState
	{
		NOT_EXECUTED,
		SUCCESS,
		FAILURE,
	}

	/// <summary>
	/// Defines an interface for executable actions within the SAP GUI Client.
	/// These actions can perform operations such as reading or setting text, selecting checkboxes, starting transactions, pressing buttons, etc.
	/// The class <see cref="SapOperator"/> executes these actions on the corresponding SAP session.
	/// </summary>
	public interface IExecutable
	{
        InteractionState GetState();
		/// <summary>
		/// Retrieves a message providing details about the action's execution. This could include success messages, error details, or other relevant information.
		/// </summary>
		/// <returns>A string containing the message.</returns>
		string GetMessage();
        bool GetInterruptOnFailure();
        void SetInterruptOnFailure(bool value);
		void Execute(GuiSession session);
	}
}
