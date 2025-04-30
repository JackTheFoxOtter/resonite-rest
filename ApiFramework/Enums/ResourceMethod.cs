namespace ApiFramework.Enums
{
    public enum ResourceMethod : byte
    {
        Query   = 0b00000001, // GET resources w/ search
        Select  = 0b00000010, // GET resoucrce by id
        Create  = 0b00000100, // POST new resource
        Replace = 0b00001000, // PUT replace existing resource fully, or create a new resource
        Update  = 0b00010000, // PATCH partially update existing resource
        Delete  = 0b00100000, // DELETE existing resource
        All     = 0b11111111
    }
}
