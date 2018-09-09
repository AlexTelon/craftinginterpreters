using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LoxInterpreter
{
    class Scanner
    {
        private readonly string source;
        private readonly List<Token> tokens = new List<Token>();

        private int start = 0; // offset to first char in current lexeme being scanned
        private int current = 0; // the char we are currently considering
        private int line = 1;

        /// <summary>
        /// The current Lexeme always starts at start and ends at current
        /// </summary>
        private string currentLexeme => source.Substring(start, current - start);

        private static readonly Dictionary<string, TokenType> ReservedKeywords = new Dictionary<string, TokenType>()
        {
            {"and", TokenType.AND},
            {"class", TokenType.CLASS},
            {"else", TokenType.ELSE},
            {"false", TokenType.FALSE},
            {"for", TokenType.FOR},
            {"fun", TokenType.FUN},
            {"if", TokenType.IF},
            {"nil", TokenType.NIL},
            {"or", TokenType.OR},
            {"print", TokenType.PRINT},
            {"return", TokenType.RETURN},
            {"super", TokenType.SUPER},
            {"this", TokenType.THIS},
            {"true", TokenType.TRUE},
            {"var", TokenType.VAR},
            {"while", TokenType.WHILE}
        };

        internal Scanner(string source)
        {
            this.source = source;
        }

        internal List<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                // We are at the beginning of the next lexeme.
                start = current;
                ScanToken();
            }

            tokens.Add(new Token(TokenType.EOF, "", null, line));
            return tokens;
        }

        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                // Single char tokens.
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;
                case '!': AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
                case '=': AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
                case '<': AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
                case '>': AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;
                case '/':
                    if (Match('/'))
                    {
                        // A comment goes until the end of the line.
                        // Use peek so we dont consume the newline token since we want it to go to its switch-case below
                        while (Peek() != '\n' && !IsAtEnd()) Advance();
                        // Dont add the comment as token.
                    }
                    else
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;

                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace.
                    break;

                case '\n':
                    line++;
                    break;

                case '"': String(); break;

                default:
                    if (IsDigit(c))
                    {
                        Number();
                    } else if (IsAlphabetical(c))
                    {
                        Identifier();
                    }
                    Lox.Error(line, "Unexpected character.");
                    break;
            }
        }

        private void Identifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            // See if the identifier is a reserved word.
            var success = ReservedKeywords.TryGetValue(currentLexeme, out TokenType type);
            // if not then its just a normal identifier
            if (!success) type = TokenType.IDENTIFIER;
            AddToken(type);
        }

        /// <summary>
        /// Checks for a match and consumes the char if a match is found.
        /// </summary>
        /// <param name="expected"></param>
        /// <returns></returns>
        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (source[current] != expected) return false;

            current++;
            return true;
        }

        /// <summary>
        /// Peek at the upcomming char.
        /// </summary>
        /// <returns></returns>
        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return source[current];
        }

        /// <summary>
        /// Peek ahead 2 steps.
        /// </summary>
        /// <returns></returns>
        private char PeekNext()
        {
            current++;
            var peekNext = Peek();
            current--;
            return peekNext;
        }

        private bool IsAlphabetical(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                   c == '_';
        }

        private bool IsAlphaNumeric(char c) => IsAlphabetical(c) || IsDigit(c);


        private bool IsDigit(char c) => c >= '0' && c <= '9';

        private void Number()
        {
            while (IsDigit(Peek())) Advance();

            // Look for a fractional part.
            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                // Consume the "."
                Advance();

                while (IsDigit(Peek())) Advance();
            }

            AddToken(TokenType.NUMBER, Double.Parse(currentLexeme, CultureInfo.InvariantCulture));
        }


        private void String()
        {
            // Consume until end.
            while (Peek() != '"' && !IsAtEnd())
            {
                // Handle multiline strings
                if (Peek() == '\n') line++;
                Advance();
            }

            // Unterminated string.
            if (IsAtEnd())
            {
                Lox.Error(line, "Unterminated string.");
                return;
            }

            // The closing ".
            Advance();

            // Trim the surrounding quotes.
            string value = source.Substring(start + 1, current - start - 2);
            AddToken(TokenType.STRING, value);
        }


        private bool IsAtEnd() => current >= source.Length;
        private char Advance() => source[++current - 1];

        private void AddToken(TokenType type) => AddToken(type, null);

        private void AddToken(TokenType type, Object literal) => tokens.Add(new Token(type, currentLexeme, literal, line));

    }
}
