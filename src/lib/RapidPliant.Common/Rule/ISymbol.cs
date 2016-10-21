using System;
using RapidPliant.Common.Util;

namespace RapidPliant.Common.Rule
{
    public interface ISymbol
    {
        SymbolType SymbolType { get; }
    }

    public abstract class Symbol : ISymbol
    {
        public Symbol(SymbolType symbolType)
        {
            SymbolType = symbolType;
        }

        public SymbolType SymbolType { get; protected set; }
    }

    public class SymbolType
    {
        private readonly int _hashCode;

        public SymbolType(Type ownerType, string name)
        {
            OwnerType = ownerType;
            Name = name;

            _hashCode = HashCode.Compute(ownerType.GetHashCode(), name.GetHashCode());
        }

        public Type OwnerType { get; private set; }

        public string Name { get; private set; }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj == this)
                return true;

            var other = obj as SymbolType;
            if (other == null)
                return false;

            if (other.OwnerType != OwnerType)
            {
                return false;
            }

            return other.Name == Name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}