using SAPFEWSELib;

namespace SAPeador
{
	public enum InteractionState
	{
		NOT_EXECUTED,
		SUCCESS,
		FAILURE,
	}

	public interface IExecutable
	{
        InteractionState GetState();
        string GetMessage();
        bool GetInterruptOnFailure();
        void SetInterruptOnFailure(bool value);
        void Execute(GuiSession session);
	}
}
