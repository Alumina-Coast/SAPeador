using SAPFEWSELib;
using System;

namespace SAPeador
{
	public class StartTransactionExecutable : IExecutable
	{
        private InteractionState state = InteractionState.NOT_EXECUTED;
        private string message = string.Empty;
        private bool interruptOnFailure;
        public string PreviousText { get; private set; } = string.Empty;
        public string TransactionCode { get; set; }

        public StartTransactionExecutable(string transactionCode, bool interruptOnFailure = false)
        {
            TransactionCode = transactionCode;
            this.interruptOnFailure = interruptOnFailure;
        }

        public InteractionState GetState()
        {
            return state;
        }

        private void SetState(InteractionState value)
        {
            state = value;
        }

        public string GetMessage()
        {
            return message;
        }

        private void SetMessage(string value)
        {
            message = value;
        }

        public bool GetInterruptOnFailure()
        {
            return interruptOnFailure;
        }

        public void SetInterruptOnFailure(bool value)
        {
            interruptOnFailure = value;
        }

		void IExecutable.Execute(GuiSession session)
        {
            SetState(InteractionState.FAILURE);
            if (string.IsNullOrWhiteSpace(TransactionCode))
            {
                SetMessage($"No item path given.");
                return;
            }
            try
            {
                PreviousText = session.Info.Transaction;
                session.StartTransaction(TransactionCode);
                if (session.Info.Transaction.ToUpper() == TransactionCode.ToUpper())
                {
                    SetMessage($"Transaction {TransactionCode} started");
                } else
                {
                    SetMessage($"Could not start transaction {TransactionCode}.");
                    return;
                }
            }
            catch (Exception e)
            {
                SetMessage($"Failed to start transaction {TransactionCode}. Error: {e.Message}");
                return;
            }
            SetState(InteractionState.SUCCESS);
		}
	}
}
