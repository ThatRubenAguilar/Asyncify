using System;
using System.Diagnostics;

namespace Asyncify.Extensions
{
    class TypeNameLexer
    {
        private static readonly char[] TypeTokens = { '.', ',', '<', '>' };
        

        private int _currentIndex;
        private int _searchIndex;
        private int _length;
        private int _prevIndex;
        public string Token => TypeName.Substring(_currentIndex, 1);
        public string ProceedingIdentifier => TypeName.Substring(_prevIndex, _currentIndex - _prevIndex).Trim();
        public string TypeName { get; }
        public bool HasMoreTokens { get; private set; }

        public TypeNameLexer(string typeName, int startIndex, int length)
        {
            TypeName = typeName;
            _currentIndex = startIndex;
            _length = length;
            _prevIndex = startIndex;
            _searchIndex = startIndex;
        }

        public bool NextToken()
        {
#if DEBUG
            Debug.WriteLine(ToString());
#endif
            _prevIndex = _searchIndex;
            _currentIndex = TypeName.IndexOfAny(TypeTokens, _searchIndex, _length);
            if (_currentIndex < 0)
            {
                _searchIndex = _searchIndex + _length;
                _currentIndex = _searchIndex;
                _length = 0;
                HasMoreTokens = false;
                return false;
            }
            _searchIndex = _currentIndex + 1;
            _length = _length - (_searchIndex - _prevIndex);
            HasMoreTokens = true;
            return true;
        }

        string GetSafeToken()
        {
            if (_length <= 0)
                return "END";
            if (_searchIndex <= 0)
                return "START";
            return Token;
        }

        public void ThrowTokenError()
        {
            throw new ArgumentException($"Unexpected token '{GetSafeToken()}' at index {_currentIndex} of type syntax {TypeName}");
        }
        public void ThrowIdentifierError()
        {
            throw new ArgumentException($"Unexpected identifier '{ProceedingIdentifier}' before '{GetSafeToken()}' at index {_currentIndex} of type syntax {TypeName}");
        }

        public override string ToString()
        {
            return $"TypeName: '{TypeName}' {Environment.NewLine}HasMoreTokens: {HasMoreTokens} {Environment.NewLine}Index(Prev, Curr, Search): ({_prevIndex},{_currentIndex},{_searchIndex}) {Environment.NewLine}Length: {_length} {Environment.NewLine}Token: '{GetSafeToken()}' {Environment.NewLine}ProceedingIdentifier: {ProceedingIdentifier}";
        }
    }
}