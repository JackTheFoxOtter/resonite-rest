namespace ApiFramework.Enums
{
    public enum EditPermission : byte
    {
        None               = 0b00000000, // No permissions
        Create             = 0b00000001, // Element can be created if it doesn't yet exist
        Modify             = 0b00000010, // Element can be modified if it exists
        Delete             = 0b00000100, // Element can be deleted if it exists
        CreateModify       = 0b00000011, // Create & modify      
        CreateDelete       = 0b00000101, // Create & delete
        ModifyDelete       = 0b00000110, // Modify & delete
        CreateModifyDelete = 0b00000111, // Create, modify & delete
    }

    public static class EditPermissionExtensions
    {
        public static bool CanCreate(this EditPermission permission)
        {
            return (permission & EditPermission.Create) == EditPermission.Create;
        }

        public static bool CanModify(this EditPermission permission)
        {
            return (permission & EditPermission.Modify) == EditPermission.Modify;
        }

        public static bool CanDelete(this EditPermission permission)
        {
            return (permission & EditPermission.Delete) == EditPermission.Delete;
        }

        public static string ToFriendlyName(this EditPermission permissions)
        {
            switch (permissions)
            {
                case EditPermission.None: return "None";
                case EditPermission.Create: return "Create";
                case EditPermission.Modify: return "Modify";
                case EditPermission.Delete: return "Delete";
                case EditPermission.CreateModify: return "Create, Modify";
                case EditPermission.CreateDelete: return "Create, Delete";
                case EditPermission.ModifyDelete: return "Modify, Delete";
                case EditPermission.CreateModifyDelete: return "Create, Modify, Delete";
                default: return "Invalid Permission";
            }
        }
    }
}
