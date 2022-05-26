﻿using System.Text;

namespace MarlinCompiler.Common.Symbols.Kinds;

/// <summary>
/// Represents an usage of a type.
/// </summary>
public class TypeUsageSymbol : ISymbol
{
    public TypeUsageSymbol(TypeSymbol type, bool typeReferencedStatically = false)
    {
        Type                     = type;
        TypeReferencedStatically = typeReferencedStatically;
        GenericArgs              = Array.Empty<TypeUsageSymbol>();
    }

    public TypeUsageSymbol(TypeUsageSymbol copyFrom, TypeUsageSymbol[] addGenericArgs)
    {
        Type                     = copyFrom.Type;
        TypeReferencedStatically = copyFrom.TypeReferencedStatically;
        GenericArgs              = addGenericArgs;
    }

    /// <summary>
    /// The type that was instantiated.
    /// </summary>
    public TypeSymbol Type { get; set; }

    /// <summary>
    /// Whether the type was used statically or not.
    /// </summary>
    public bool TypeReferencedStatically { get; set; }

    /// <summary>
    /// The generic argument which was passed.
    /// </summary>
    public TypeUsageSymbol[] GenericArgs { get; set; }

    public string GetStringRepresentation()
    {
        if (Type is GenericParamTypeSymbol genericParam)
        {
            ClassTypeSymbol genericParamOwner = genericParam.Owner;

            for (int i = 0; i < genericParamOwner.GenericParamNames.Length; ++i)
            {
                if (genericParamOwner.GenericParamNames[i] == genericParam.Name)
                {
                    return GenericArgs[i].GetStringRepresentation();
                }
            }
        }

        if (Type is ClassTypeSymbol classTypeSymbol && classTypeSymbol.GenericParamNames.Any())
        {
            StringBuilder builder = new(classTypeSymbol.Name);
            builder.Append('<');

            for (int i = 0; i < classTypeSymbol.GenericParamNames.Length; ++i)
            {
                builder.Append(GenericArgs[i].GetStringRepresentation());

                if (i != classTypeSymbol.GenericParamNames.Length - 1)
                {
                    builder.Append(", ");
                }
            }
            
            builder.Append('>');

            return builder.ToString();
        }
        
        return Type.Name;
    }
}