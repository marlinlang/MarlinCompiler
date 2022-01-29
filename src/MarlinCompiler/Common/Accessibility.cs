namespace MarlinCompiler.Common;

[Flags]
public enum Accessibility : short
{
    PublicAccess        = 0,
    PublicModify         = 1,
    
    PrivateAccess       = 2,
    PrivateModify       = 4,
    
    InternalAccess      = 8,
    InternalModify      = 16,
    
    ProtectedAccess     = 32,
    ProtectedModify     = 64,
    
    NoModify            = 128
}