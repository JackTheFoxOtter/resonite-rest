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
}
