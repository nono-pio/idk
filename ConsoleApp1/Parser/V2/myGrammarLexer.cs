//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.13.2
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from myGrammar.g4 by ANTLR 4.13.2

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.13.2")]
[System.CLSCompliant(false)]
public partial class myGrammarLexer : Lexer {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		T__0=1, WS=2, DIGIT=3, CHAR=4, CARET=5, COMMA=6, UNDERSCORE=7, FRAC_CMD=8, 
		SQRT_CMD=9, DERIVATIVE=10, ADD=11, SUB=12, MUL=13, CDOT=14, TIMES=15, 
		DIV=16, L_PAREN=17, R_PAREN=18, L_BRACE=19, R_BRACE=20, L_BRACKET=21, 
		R_BRACKET=22, BAR=23, L_FLOOR=24, R_FLOOR=25, L_CEIL=26, R_CEIL=27, FAC=28, 
		FUNC_NAME=29, FUNC_NAME_LATEX=30, LATEX_CMD=31;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"T__0", "WS", "DIGIT", "CHAR", "CARET", "COMMA", "UNDERSCORE", "FRAC_CMD", 
		"SQRT_CMD", "DERIVATIVE", "ADD", "SUB", "MUL", "CDOT", "TIMES", "DIV", 
		"L_PAREN", "R_PAREN", "L_BRACE", "R_BRACE", "L_BRACKET", "R_BRACKET", 
		"BAR", "L_FLOOR", "R_FLOOR", "L_CEIL", "R_CEIL", "FAC", "FUNC_NAME", "FUNC_NAME_LATEX", 
		"LATEX_CMD"
	};


	public myGrammarLexer(ICharStream input)
	: this(input, Console.Out, Console.Error) { }

	public myGrammarLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
	: base(input, output, errorOutput)
	{
		Interpreter = new LexerATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	private static readonly string[] _LiteralNames = {
		null, "'.'", null, null, null, "'^'", "','", "'_'", "'\\frac'", "'\\sqrt'", 
		"'''", "'+'", "'-'", "'*'", "'\\cdot'", "'\\times'", "'/'", null, null, 
		"'{'", "'}'", "'['", "']'", "'|'", "'\\lfloor'", "'\\rfloor'", "'\\lceil'", 
		"'\\rceil'", "'!'"
	};
	private static readonly string[] _SymbolicNames = {
		null, null, "WS", "DIGIT", "CHAR", "CARET", "COMMA", "UNDERSCORE", "FRAC_CMD", 
		"SQRT_CMD", "DERIVATIVE", "ADD", "SUB", "MUL", "CDOT", "TIMES", "DIV", 
		"L_PAREN", "R_PAREN", "L_BRACE", "R_BRACE", "L_BRACKET", "R_BRACKET", 
		"BAR", "L_FLOOR", "R_FLOOR", "L_CEIL", "R_CEIL", "FAC", "FUNC_NAME", "FUNC_NAME_LATEX", 
		"LATEX_CMD"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "myGrammar.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ChannelNames { get { return channelNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override int[] SerializedAtn { get { return _serializedATN; } }

	static myGrammarLexer() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}
	private static int[] _serializedATN = {
		4,0,31,234,6,-1,2,0,7,0,2,1,7,1,2,2,7,2,2,3,7,3,2,4,7,4,2,5,7,5,2,6,7,
		6,2,7,7,7,2,8,7,8,2,9,7,9,2,10,7,10,2,11,7,11,2,12,7,12,2,13,7,13,2,14,
		7,14,2,15,7,15,2,16,7,16,2,17,7,17,2,18,7,18,2,19,7,19,2,20,7,20,2,21,
		7,21,2,22,7,22,2,23,7,23,2,24,7,24,2,25,7,25,2,26,7,26,2,27,7,27,2,28,
		7,28,2,29,7,29,2,30,7,30,1,0,1,0,1,1,4,1,67,8,1,11,1,12,1,68,1,1,1,1,1,
		2,1,2,1,3,1,3,1,4,1,4,1,5,1,5,1,6,1,6,1,7,1,7,1,7,1,7,1,7,1,7,1,8,1,8,
		1,8,1,8,1,8,1,8,1,9,1,9,1,10,1,10,1,11,1,11,1,12,1,12,1,13,1,13,1,13,1,
		13,1,13,1,13,1,14,1,14,1,14,1,14,1,14,1,14,1,14,1,15,1,15,1,16,1,16,1,
		16,1,16,1,16,1,16,1,16,3,16,125,8,16,1,17,1,17,1,17,1,17,1,17,1,17,1,17,
		1,17,3,17,135,8,17,1,18,1,18,1,19,1,19,1,20,1,20,1,21,1,21,1,22,1,22,1,
		23,1,23,1,23,1,23,1,23,1,23,1,23,1,23,1,24,1,24,1,24,1,24,1,24,1,24,1,
		24,1,24,1,25,1,25,1,25,1,25,1,25,1,25,1,25,1,26,1,26,1,26,1,26,1,26,1,
		26,1,26,1,27,1,27,1,28,1,28,1,28,1,28,1,28,1,28,1,28,1,28,1,28,1,28,1,
		28,1,28,1,28,1,28,1,28,1,28,1,28,1,28,1,28,1,28,3,28,199,8,28,1,29,1,29,
		1,29,1,29,1,29,1,29,1,29,1,29,1,29,1,29,1,29,1,29,1,29,1,29,1,29,1,29,
		1,29,1,29,1,29,1,29,1,29,1,29,1,29,1,29,1,29,1,29,3,29,227,8,29,1,30,1,
		30,4,30,231,8,30,11,30,12,30,232,0,0,31,1,1,3,2,5,3,7,4,9,5,11,6,13,7,
		15,8,17,9,19,10,21,11,23,12,25,13,27,14,29,15,31,16,33,17,35,18,37,19,
		39,20,41,21,43,22,45,23,47,24,49,25,51,26,53,27,55,28,57,29,59,30,61,31,
		1,0,3,3,0,9,10,13,13,32,32,1,0,48,57,2,0,65,90,97,122,250,0,1,1,0,0,0,
		0,3,1,0,0,0,0,5,1,0,0,0,0,7,1,0,0,0,0,9,1,0,0,0,0,11,1,0,0,0,0,13,1,0,
		0,0,0,15,1,0,0,0,0,17,1,0,0,0,0,19,1,0,0,0,0,21,1,0,0,0,0,23,1,0,0,0,0,
		25,1,0,0,0,0,27,1,0,0,0,0,29,1,0,0,0,0,31,1,0,0,0,0,33,1,0,0,0,0,35,1,
		0,0,0,0,37,1,0,0,0,0,39,1,0,0,0,0,41,1,0,0,0,0,43,1,0,0,0,0,45,1,0,0,0,
		0,47,1,0,0,0,0,49,1,0,0,0,0,51,1,0,0,0,0,53,1,0,0,0,0,55,1,0,0,0,0,57,
		1,0,0,0,0,59,1,0,0,0,0,61,1,0,0,0,1,63,1,0,0,0,3,66,1,0,0,0,5,72,1,0,0,
		0,7,74,1,0,0,0,9,76,1,0,0,0,11,78,1,0,0,0,13,80,1,0,0,0,15,82,1,0,0,0,
		17,88,1,0,0,0,19,94,1,0,0,0,21,96,1,0,0,0,23,98,1,0,0,0,25,100,1,0,0,0,
		27,102,1,0,0,0,29,108,1,0,0,0,31,115,1,0,0,0,33,124,1,0,0,0,35,134,1,0,
		0,0,37,136,1,0,0,0,39,138,1,0,0,0,41,140,1,0,0,0,43,142,1,0,0,0,45,144,
		1,0,0,0,47,146,1,0,0,0,49,154,1,0,0,0,51,162,1,0,0,0,53,169,1,0,0,0,55,
		176,1,0,0,0,57,198,1,0,0,0,59,226,1,0,0,0,61,228,1,0,0,0,63,64,5,46,0,
		0,64,2,1,0,0,0,65,67,7,0,0,0,66,65,1,0,0,0,67,68,1,0,0,0,68,66,1,0,0,0,
		68,69,1,0,0,0,69,70,1,0,0,0,70,71,6,1,0,0,71,4,1,0,0,0,72,73,7,1,0,0,73,
		6,1,0,0,0,74,75,7,2,0,0,75,8,1,0,0,0,76,77,5,94,0,0,77,10,1,0,0,0,78,79,
		5,44,0,0,79,12,1,0,0,0,80,81,5,95,0,0,81,14,1,0,0,0,82,83,5,92,0,0,83,
		84,5,102,0,0,84,85,5,114,0,0,85,86,5,97,0,0,86,87,5,99,0,0,87,16,1,0,0,
		0,88,89,5,92,0,0,89,90,5,115,0,0,90,91,5,113,0,0,91,92,5,114,0,0,92,93,
		5,116,0,0,93,18,1,0,0,0,94,95,5,39,0,0,95,20,1,0,0,0,96,97,5,43,0,0,97,
		22,1,0,0,0,98,99,5,45,0,0,99,24,1,0,0,0,100,101,5,42,0,0,101,26,1,0,0,
		0,102,103,5,92,0,0,103,104,5,99,0,0,104,105,5,100,0,0,105,106,5,111,0,
		0,106,107,5,116,0,0,107,28,1,0,0,0,108,109,5,92,0,0,109,110,5,116,0,0,
		110,111,5,105,0,0,111,112,5,109,0,0,112,113,5,101,0,0,113,114,5,115,0,
		0,114,30,1,0,0,0,115,116,5,47,0,0,116,32,1,0,0,0,117,125,5,40,0,0,118,
		119,5,92,0,0,119,120,5,108,0,0,120,121,5,101,0,0,121,122,5,102,0,0,122,
		123,5,116,0,0,123,125,5,40,0,0,124,117,1,0,0,0,124,118,1,0,0,0,125,34,
		1,0,0,0,126,135,5,41,0,0,127,128,5,92,0,0,128,129,5,114,0,0,129,130,5,
		105,0,0,130,131,5,103,0,0,131,132,5,104,0,0,132,133,5,116,0,0,133,135,
		5,41,0,0,134,126,1,0,0,0,134,127,1,0,0,0,135,36,1,0,0,0,136,137,5,123,
		0,0,137,38,1,0,0,0,138,139,5,125,0,0,139,40,1,0,0,0,140,141,5,91,0,0,141,
		42,1,0,0,0,142,143,5,93,0,0,143,44,1,0,0,0,144,145,5,124,0,0,145,46,1,
		0,0,0,146,147,5,92,0,0,147,148,5,108,0,0,148,149,5,102,0,0,149,150,5,108,
		0,0,150,151,5,111,0,0,151,152,5,111,0,0,152,153,5,114,0,0,153,48,1,0,0,
		0,154,155,5,92,0,0,155,156,5,114,0,0,156,157,5,102,0,0,157,158,5,108,0,
		0,158,159,5,111,0,0,159,160,5,111,0,0,160,161,5,114,0,0,161,50,1,0,0,0,
		162,163,5,92,0,0,163,164,5,108,0,0,164,165,5,99,0,0,165,166,5,101,0,0,
		166,167,5,105,0,0,167,168,5,108,0,0,168,52,1,0,0,0,169,170,5,92,0,0,170,
		171,5,114,0,0,171,172,5,99,0,0,172,173,5,101,0,0,173,174,5,105,0,0,174,
		175,5,108,0,0,175,54,1,0,0,0,176,177,5,33,0,0,177,56,1,0,0,0,178,199,3,
		59,29,0,179,180,5,92,0,0,180,199,3,59,29,0,181,182,5,97,0,0,182,183,5,
		98,0,0,183,199,5,115,0,0,184,185,5,102,0,0,185,186,5,108,0,0,186,187,5,
		111,0,0,187,188,5,111,0,0,188,199,5,114,0,0,189,190,5,99,0,0,190,191,5,
		101,0,0,191,192,5,105,0,0,192,199,5,108,0,0,193,194,5,114,0,0,194,195,
		5,111,0,0,195,196,5,117,0,0,196,197,5,110,0,0,197,199,5,100,0,0,198,178,
		1,0,0,0,198,179,1,0,0,0,198,181,1,0,0,0,198,184,1,0,0,0,198,189,1,0,0,
		0,198,193,1,0,0,0,199,58,1,0,0,0,200,201,5,115,0,0,201,202,5,105,0,0,202,
		227,5,110,0,0,203,204,5,99,0,0,204,205,5,111,0,0,205,227,5,115,0,0,206,
		207,5,116,0,0,207,208,5,97,0,0,208,227,5,110,0,0,209,210,5,99,0,0,210,
		211,5,111,0,0,211,227,5,116,0,0,212,213,5,115,0,0,213,214,5,101,0,0,214,
		227,5,99,0,0,215,216,5,99,0,0,216,217,5,115,0,0,217,227,5,99,0,0,218,219,
		5,108,0,0,219,220,5,111,0,0,220,227,5,103,0,0,221,222,5,108,0,0,222,227,
		5,110,0,0,223,224,5,101,0,0,224,225,5,120,0,0,225,227,5,112,0,0,226,200,
		1,0,0,0,226,203,1,0,0,0,226,206,1,0,0,0,226,209,1,0,0,0,226,212,1,0,0,
		0,226,215,1,0,0,0,226,218,1,0,0,0,226,221,1,0,0,0,226,223,1,0,0,0,227,
		60,1,0,0,0,228,230,5,92,0,0,229,231,7,2,0,0,230,229,1,0,0,0,231,232,1,
		0,0,0,232,230,1,0,0,0,232,233,1,0,0,0,233,62,1,0,0,0,7,0,68,124,134,198,
		226,232,1,6,0,0
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
