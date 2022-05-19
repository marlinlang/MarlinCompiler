namespace MarlinCompiler.Common;

public enum GetAccessibility : short
{
    Public    = 4,
    Internal  = 3,
    Private   = 2,
    Protected = 1
}

public enum SetAccessibility : short
{
    Public    = 4,
    Internal  = 3,
    Private   = 2,
    Protected = 1,
    NoModify  = 0
}