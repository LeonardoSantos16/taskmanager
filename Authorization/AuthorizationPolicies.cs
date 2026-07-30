namespace taskmanager.Authorization
{
    public static class AuthorizationPolicies
    {
        /// <summary>Caller must have any role (Owner, Editor or Viewer) on the project.</summary>
        public const string ProjectMember = "ProjectMember";

        /// <summary>Caller must be Owner or Editor on the project.</summary>
        public const string ProjectEditorOrOwner = "ProjectEditorOrOwner";
    }
}
