using System;

namespace Blocktrust.CredentialWorkflow.Web.Services
{
    public class WorkflowChangeTrackerService
    {
        public event Action OnChange;
        private bool _hasUnsavedChanges;

        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set
            {
                if (_hasUnsavedChanges != value)
                {
                    _hasUnsavedChanges = value;
                    OnChange?.Invoke();
                }
            }
        }

        public void ResetChanges()
        {
            HasUnsavedChanges = false;
        }
    }
}