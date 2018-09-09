using System;
using System.Collections.Generic;
using System.Text;

namespace LoxInterpreter
{
    /// <summary>
    /// A token is what a scanner translate things like "var" and ";" into.
    /// It contains the raw string (lexeme) along with other things we learned while reading
    /// </summary>
    internal class Token
    {
        readonly TokenType type;
        readonly string lexeme;
        readonly Object literal;
        readonly int line;

        internal Token (TokenType type, string lexeme, Object literal, int line)
        {
            this.type = type;
            this.lexeme = lexeme;
            this.literal = literal;
            this.line = line;
        }

        public override string ToString()
        {
            return type + " " + lexeme + " " + literal;
        }
    }
}
