

namespace Cc.Redberry.Rings.Io
{
    /// <summary>
    /// Simple math expression tokenizer
    /// </summary>
    public sealed class Tokenizer
    {
        private readonly CharacterStream stream;
        /// <summary>
        /// Create tokenizer of a given char stream
        /// </summary>
        public Tokenizer(CharacterStream stream)
        {
            this.stream = stream;
        }


        /// <summary>
        /// token type
        /// </summary>
        public enum TokenType
        {
            T_VARIABLE,
            T_PLUS,
            T_MINUS,
            T_MULTIPLY,
            T_DIVIDE,
            T_EXPONENT,
            T_SPACE,
            T_NEWLINE,
            T_BRACKET_OPEN,
            T_BRACKET_CLOSE,
            T_END
        }


        // whether there is a single char in local buffer
        private bool bufferedCharDefined;

        // local buffer with only one character
        private char bufferedChar;

        /// <summary>
        /// has more elements to consider
        /// </summary>
        private bool HasNextChar()
        {
            return bufferedCharDefined || stream.HasNext();
        }


        /// <summary>
        /// next character
        /// </summary>
        private char NextChar()
        {
            char c;
            if (bufferedCharDefined)
            {
                bufferedCharDefined = false;
                c = bufferedChar;
            }
            else
                c = stream.Next();
            CheckChar(c);
            return c;
        }


        private static void CheckChar(char c)
        {
            switch (c)
            {
                case '=':
                case '&':
                case '%':
                case '!':
                case '~':
                    throw new ArgumentException(String.Format("Illegal character %s", c));
            }
        }

        private bool first = true, last = true;

        private Token FirstToken()
        {
            first = false;
            return BRACKET_OPEN;
        }


        private Token LastToken()
        {
            if (!last)
                return END;
            last = false;
            return BRACKET_CLOSE;
        }

        /// <summary>
        /// Get the next token from stream
        /// </summary>
        public Token NextToken()
        {
            if (first)
                return FirstToken();
            if (!HasNextChar())
                return LastToken();

            // peek the first char from the buffer
            char seed = NextChar();

            // check whether this is already a token
            Token token = PrimitiveToken(seed);

            // get rid of spaces
            if (token == SPACE)
            {
                do
                {
                    if (!HasNextChar())
                        return LastToken();
                    seed = NextChar();

                    // check whether this is already a token
                    token = PrimitiveToken(seed);
                }
                while (token == SPACE);
            }

            if (token != null)
                return token;

            // <-seed char is a start of variable
            // string that contains the beginning of the variable
            string sBegin = stream.CurrentString();
            int pBegin = stream.IndexInCurrentString();
            if (!HasNextChar())
            {

                // seed char is the last
                return new Token(TokenType.T_VARIABLE, sBegin.Substring(pBegin, pBegin + 1));
            }

            char c = (char)0;
            int nSpaces = 0;
            bool needBuffer = false;
            while (HasNextChar())
            {
                c = stream.Next();
                Token t = PrimitiveToken(c);
                if (t == SPACE)
                {

                    // terminating spaces
                    ++nSpaces;
                    continue;
                }

                if (t == null)
                {
                    if (nSpaces > 0)
                        throw new ArgumentException("spaces in variable name are forbidden");
                    continue;
                }

                needBuffer = true;
                break;
            }


            // string that contains the end of the variable
            string sEnd = stream.CurrentString();
            int pEnd = stream.IndexInCurrentString() + 1; // end inclusive

            // put the next character back into the buffer
            if (needBuffer)
            {
                bufferedChar = c;
                bufferedCharDefined = true;
                pEnd -= 1;
            }

            string variable;
            if (sBegin == sEnd)
                variable = sBegin.Substring(pBegin, pEnd - nSpaces);
            else
            {
                if (nSpaces < pEnd)
                    variable = sBegin.Substring(pBegin) + sEnd.Substring(0, pEnd);
                else
                    variable = sBegin.Substring(pBegin, sBegin.Length - (pEnd - nSpaces));
            }

            return new Token(TokenType.T_VARIABLE, variable);
        }

        private static Token PrimitiveToken(char character)
        {
            switch (character)
            {
                case '+':
                    return PLUS;
                case '-':
                    return MINUS;
                case '*':
                    return MULTIPLY;
                case '/':
                    return DIVIDE;
                case '^':
                    return EXPONENT;
                case '(':
                    return BRACKET_OPEN;
                case ')':
                    return BRACKET_CLOSE;
                case '\t':
                case '\n':
                case ' ':
                    return SPACE;
                default:
                    return null;
                    break;
            }
        }

        public static readonly Token END = new Token(TokenType.T_END, ""), PLUS = new Token(TokenType.T_PLUS, "+"), MINUS = new Token(TokenType.T_MINUS, "-"), MULTIPLY = new Token(TokenType.T_MULTIPLY, "*"), DIVIDE = new Token(TokenType.T_DIVIDE, "/"), EXPONENT = new Token(TokenType.T_EXPONENT, "^"), BRACKET_OPEN = new Token(TokenType.T_BRACKET_OPEN, "("), BRACKET_CLOSE = new Token(TokenType.T_BRACKET_CLOSE, ")"), SPACE = new Token(TokenType.T_SPACE, " ");

        /// <summary>
        /// Simple token
        /// </summary>
        public sealed class Token
        {
            public readonly TokenType tokenType;
            public readonly string content;

            public Token(TokenType tokenType, string content)
            {
                this.tokenType = tokenType;
                this.content = content;
            }

            public string ToString()
            {
                return tokenType + "(" + content + ")";
            }
        }


        /// <summary>
        /// Stream of chars. Implementations are not synchronized and doesn't support concurrent iteration.
        /// </summary>
        public interface CharacterStream
        {

            bool HasNext();
 
            /// <summary>
            /// next char from this stream
            /// </summary>
            char Next();

            /// <summary>
            /// string containing current char
            /// </summary>
            string CurrentString();

            /// <summary>
            /// index of char in string
            /// </summary>
            int IndexInCurrentString();

            /// <summary>
            /// skip all chars preceding the specified char and place caret to the first char after the specified one
            /// </summary>
            /// <returns>false if stream finished without specified char and true otherwise</returns>
            bool Seek(char c)
            {
                while (HasNext())
                {
                    char n = Next();
                    if (n == c)
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        /// skip all chars preceding the specified char and place caret to the first char after the specified one
        /// </summary>
        /// <returns>false if stream finished without specified char and true otherwise</returns>
        private sealed class ConcatStreams : CharacterStream
        {
            readonly IList<CharacterStream> streams;
            int currentStream;
            public ConcatStreams(IList<CharacterStream> streams) : this(streams, 0)
            {
            }

            public ConcatStreams(IList<CharacterStream> streams, int currentStream)
            {
                this.streams = streams;
                this.currentStream = currentStream;
            }

            public bool HasNext()
            {
                int cs = currentStream;
                for (; cs < streams.Count; ++cs)
                    if (streams[cs].HasNext())
                        return true;
                return false;
            }

            public char Next()
            {
                int cs = currentStream;
                for (; cs < streams.Count; ++cs)
                    if (streams[cs].HasNext())
                    {
                        currentStream = cs;
                        return streams[cs].Next();
                    }

                throw new ArgumentException("no more elements in this stream");
            }

            public string CurrentString()
            {
                return streams[currentStream].CurrentString();
            }

            public int IndexInCurrentString()
            {
                return streams[currentStream].IndexInCurrentString();
            }
        }

     
        /// <summary>
        /// Concat char streams
        /// </summary>
        public static CharacterStream Concat(CharacterStream a, CharacterStream b)
        {
            List<CharacterStream> streams = new List<CharacterStream>();
            streams.Add(a);
            streams.Add(b);
            return new ConcatStreams(streams);
        }

        /// <summary>
        /// Create character stream from string
        /// </summary>
        /// <param name="terminateChar">if a non-null value specified, stream will terminate on the last char preceding the {@code terminateChar}</param>
        public static CharacterStream MkCharacterStream(string @string, char? terminateChar)
        {
            return new AnonymousCharacterStream(@string, terminateChar);
        }

        private sealed class AnonymousCharacterStream : CharacterStream
        {
            public AnonymousCharacterStream(string @string, char? terminateChar)
            {
                this.@string = @string;
                this.terminateChar = terminateChar;
            }
            
            private readonly string @string;
            private readonly char? terminateChar;
            
            int index = 0;
            int currentIndex = 0;
            public bool HasNext()
            {
                return index < @string.Length && (terminateChar is null || @string[index] != terminateChar);
            }

            public char Next()
            {
                currentIndex = index;
                return @string[index++];
            }

            public string CurrentString()
            {
                return @string;
            }

            public int IndexInCurrentString()
            {
                return currentIndex;
            }
        }

        /// <summary>
        /// Create string tokenizer
        /// </summary>
        /// <param name="terminateChar">if a non-null value specified, stream will terminate on the last char preceding the {@code terminateChar}</param>
        public static Tokenizer MkTokenizer(string @string, char terminateChar)
        {
            return new Tokenizer(MkCharacterStream(@string, terminateChar));
        }

        /// <summary>
        /// Create string tokenizer
        /// </summary>
        public static Tokenizer MkTokenizer(string @string)
        {
            return new Tokenizer(MkCharacterStream(@string, null));
        }
    }
}