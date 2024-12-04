namespace Rings.io;


public sealed class Tokenizer {
    private readonly CharacterStream stream;

    /**
     * Create tokenizer of a given char stream
     */
    public Tokenizer(CharacterStream stream) {
        this.stream = stream;
    }

    /** token type */
    public enum TokenType {
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

    /** has more elements to consider */
    private bool hasNextChar() {
        return bufferedCharDefined || stream.hasNext();
    }

    /** next character */
    private char nextChar() {
        char c;
        if (bufferedCharDefined) {
            bufferedCharDefined = false;
            c = bufferedChar;
        } else
            c = stream.next();

        checkChar(c);
        return c;
    }

    private static void checkChar(char c) {
        switch (c) {
            case '=':
            case '&':
            case '%':
            case '!':
            case '~':
                throw new ArgumentException(string.Format("Illegal character %s", c));
        }
    }

    // envelope tokens wit ( )
    private bool first = true, last = true;

    private Token firstToken() {
        first = false;
        return BRACKET_OPEN;
    }

    private Token lastToken() {
        if (!last)
            return END;
        last = false;
        return BRACKET_CLOSE;
    }

    /**
     * Get the next token from stream
     */
    public Token nextToken() {
        if (first)
            return firstToken();

        if (!hasNextChar())
            return lastToken();

        // peek the first char from the buffer
        char seed = nextChar();
        // check whether this is already a token
        Token token = primitiveToken(seed);

        // get rid of spaces
        if (token == SPACE) {
            do {
                if (!hasNextChar())
                    return lastToken();

                seed = nextChar();
                // check whether this is already a token
                token = primitiveToken(seed);
            } while (token == SPACE);
        }

        if (token != null)
            return token;

        // <-seed char is a start of variable

        // string that contains the beginning of the variable
        String sBegin = stream.currentString();
        int pBegin = stream.indexInCurrentString();

        if (!hasNextChar()) {
            // seed char is the last
            return new Token(TokenType.T_VARIABLE, sBegin.Substring(pBegin, pBegin + 1));
        }

        char c = '0';
        int nSpaces = 0;
        bool needBuffer = false;
        while (hasNextChar()) {
            c = stream.next();
            Token t = primitiveToken(c);
            if (t == SPACE) {// terminating spaces
                ++nSpaces;
                continue;
            }
            if (t == null) {
                if (nSpaces > 0)
                    throw new ArgumentException("spaces in variable name are forbidden");
                continue;
            }
            needBuffer = true;
            break;
        }

        // string that contains the end of the variable
        String sEnd = stream.currentString();
        int pEnd = stream.indexInCurrentString() + 1; // end inclusive

        // put the next character back into the buffer
        if (needBuffer) {
            bufferedChar = c;
            bufferedCharDefined = true;
            pEnd -= 1;
        }

        string variable;
        if (sBegin == sEnd)
            variable = sBegin.Substring(pBegin, pEnd - nSpaces);
        else {
            if (nSpaces < pEnd)
                variable = sBegin.Substring(pBegin) + sEnd.Substring(0, pEnd);
            else
                variable = sBegin.Substring(pBegin, sBegin.Length - (pEnd - nSpaces));
        }

        return new Token(TokenType.T_VARIABLE, variable);
    }

    private static Token primitiveToken(char character) {
        switch (character) {
            case '+': return PLUS;
            case '-': return MINUS;
            case '*': return MULTIPLY;
            case '/': return DIVIDE;
            case '^': return EXPONENT;
            case '(': return BRACKET_OPEN;
            case ')': return BRACKET_CLOSE;
            case '\t':
            case '\n':
            case ' ': return SPACE;
            default: return null;
        }
    }

    public static readonly Token
            END = new Token(TokenType.T_END, ""),
            PLUS = new Token(TokenType.T_PLUS, "+"),
            MINUS = new Token(TokenType.T_MINUS, "-"),
            MULTIPLY = new Token(TokenType.T_MULTIPLY, "*"),
            DIVIDE = new Token(TokenType.T_DIVIDE, "/"),
            EXPONENT = new Token(TokenType.T_EXPONENT, "^"),
            BRACKET_OPEN = new Token(TokenType.T_BRACKET_OPEN, "("),
            BRACKET_CLOSE = new Token(TokenType.T_BRACKET_CLOSE, ")"),
            SPACE = new Token(TokenType.T_SPACE, " ");

    /** Simple token */
    public sealed class Token {
        public readonly TokenType tokenType;
        public readonly String content;

        public Token(TokenType tokenType, String content) {
            this.tokenType = tokenType;
            this.content = content;
        }

        public string toString() {
            return tokenType + "(" + content + ")";
        }
    }

    /** Stream of chars. Implementations are not synchronized and doesn't support concurrent iteration. */
    public interface CharacterStream {
        bool hasNext();

        char next();

        String currentString();

        int indexInCurrentString();
        
        bool seek(char c) {
            while (hasNext()) {
                char n = next();
                if (n == c)
                    return true;
            }
            return false;
        }
    }

    public class ConcatStreams : CharacterStream {
        readonly List<CharacterStream> streams;
        int currentStream;

        public ConcatStreams(List<CharacterStream> streams) : this(streams, 0){
        }

        public ConcatStreams(List<CharacterStream> streams, int currentStream) {
            this.streams = streams;
            this.currentStream = currentStream;
        }

        public bool hasNext() {
            int cs = currentStream;
            for (; cs < streams.Count; ++cs)
                if (streams[cs].hasNext())
                    return true;
            return false;
        }

        public char next() {
            int cs = currentStream;
            for (; cs < streams.Count; ++cs)
                if (streams[cs].hasNext()) {
                    currentStream = cs;
                    return streams[cs].next();
                }
            throw new ArgumentException("no more elements in this stream");
        }

        public String currentString() {
            return streams[currentStream].currentString();
        }

        public int indexInCurrentString() {
            return streams[currentStream].indexInCurrentString();
        }
    }

    public static CharacterStream concat(CharacterStream a, CharacterStream b) {
        List<CharacterStream> streams = new ();
        streams.Add(a);
        streams.Add(b);
        return new ConcatStreams(streams);
    }

    public class MKCharacterStream : CharacterStream
    {
        int index = 0;
        int currentIndex = 0;
        private string str;
        private char? terminateChar;

        public MKCharacterStream(string str, char? terminateChar)
        {
            this.str = str;
            this.terminateChar = terminateChar;
        }
        
        public bool hasNext() {
            return index < str.Length
                   && (terminateChar is null || str[index] != terminateChar);
        }

        public char next() {
            currentIndex = index;
            return str[index++];
        }

        public String currentString() {
            return str;
        }

        public int indexInCurrentString() {
            return currentIndex;
        }
    }

    public static CharacterStream mkCharacterStream(string str, char? terminateChar)
    {
        return new MKCharacterStream(str, terminateChar);
    }

    public static Tokenizer mkTokenizer(string str, char terminateChar) {
        return new Tokenizer(mkCharacterStream(str, terminateChar));
    }

    public static Tokenizer mkTokenizer(string str) {
        return new Tokenizer(mkCharacterStream(str, null));
    }
}
