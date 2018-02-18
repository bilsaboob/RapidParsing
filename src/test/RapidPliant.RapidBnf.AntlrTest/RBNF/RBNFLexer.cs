//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.7.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from C:\Users\bilsa\AppData\Roaming\RapidEditor\Extensions\Rapid_Extensions_Antlr\code_gen\201802\18\05\51\21_266\RBNFLexer.g4 by ANTLR 4.7.1

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

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.7.1")]
[System.CLSCompliant(false)]
public partial class RBNFLexer : Lexer {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		S_LP=1, S_RP=2, S_ASTE=3, S_DOT=4, S_SLASH=5, S_SEMI=6, S_QM=7, S_LB=8, 
		S_RB=9, S_OR=10, S_PLUS=11, S_EQ=12, IMPORT=13, IDENTIFIER=14, STRING=15, 
		NUMBER=16, COLOR=17, WS=18, SINGLE_LINE_DOC_COMMENT=19, DELIMITED_DOC_COMMENT=20, 
		SINGLE_LINE_COMMENT=21, DELIMITED_COMMENT=22, ERRCHAR=23;
	public const int
		COMMENTS_CHANNEL=2, WS_CHANNEL=3;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN", "COMMENTS_CHANNEL", "WS_CHANNEL"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"S_LP", "S_RP", "S_ASTE", "S_DOT", "S_SLASH", "S_SEMI", "S_QM", "S_LB", 
		"S_RB", "S_OR", "S_PLUS", "S_EQ", "IMPORT", "IDENTIFIER", "STRING", "ESC", 
		"UNICODE", "HEX", "NUMBER", "INT", "EXP", "COLOR", "WS", "SINGLE_LINE_DOC_COMMENT", 
		"DELIMITED_DOC_COMMENT", "SINGLE_LINE_COMMENT", "DELIMITED_COMMENT", "InputCharacter", 
		"NewLineCharacter", "ERRCHAR"
	};


	public RBNFLexer(ICharStream input)
	: this(input, Console.Out, Console.Error) { }

	public RBNFLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
	: base(input, output, errorOutput)
	{
		Interpreter = new LexerATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	private static readonly string[] _LiteralNames = {
		null, "'('", "')'", "'*'", "'.'", "'/'", "';'", "'?'", "'['", "']'", "'|'", 
		"'+'", "'='", "'import'"
	};
	private static readonly string[] _SymbolicNames = {
		null, "S_LP", "S_RP", "S_ASTE", "S_DOT", "S_SLASH", "S_SEMI", "S_QM", 
		"S_LB", "S_RB", "S_OR", "S_PLUS", "S_EQ", "IMPORT", "IDENTIFIER", "STRING", 
		"NUMBER", "COLOR", "WS", "SINGLE_LINE_DOC_COMMENT", "DELIMITED_DOC_COMMENT", 
		"SINGLE_LINE_COMMENT", "DELIMITED_COMMENT", "ERRCHAR"
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

	public override string GrammarFileName { get { return "RBNFLexer.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ChannelNames { get { return channelNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override string SerializedAtn { get { return new string(_serializedATN); } }

	static RBNFLexer() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}
	private static char[] _serializedATN = {
		'\x3', '\x608B', '\xA72A', '\x8133', '\xB9ED', '\x417C', '\x3BE7', '\x7786', 
		'\x5964', '\x2', '\x19', '\xE4', '\b', '\x1', '\x4', '\x2', '\t', '\x2', 
		'\x4', '\x3', '\t', '\x3', '\x4', '\x4', '\t', '\x4', '\x4', '\x5', '\t', 
		'\x5', '\x4', '\x6', '\t', '\x6', '\x4', '\a', '\t', '\a', '\x4', '\b', 
		'\t', '\b', '\x4', '\t', '\t', '\t', '\x4', '\n', '\t', '\n', '\x4', '\v', 
		'\t', '\v', '\x4', '\f', '\t', '\f', '\x4', '\r', '\t', '\r', '\x4', '\xE', 
		'\t', '\xE', '\x4', '\xF', '\t', '\xF', '\x4', '\x10', '\t', '\x10', '\x4', 
		'\x11', '\t', '\x11', '\x4', '\x12', '\t', '\x12', '\x4', '\x13', '\t', 
		'\x13', '\x4', '\x14', '\t', '\x14', '\x4', '\x15', '\t', '\x15', '\x4', 
		'\x16', '\t', '\x16', '\x4', '\x17', '\t', '\x17', '\x4', '\x18', '\t', 
		'\x18', '\x4', '\x19', '\t', '\x19', '\x4', '\x1A', '\t', '\x1A', '\x4', 
		'\x1B', '\t', '\x1B', '\x4', '\x1C', '\t', '\x1C', '\x4', '\x1D', '\t', 
		'\x1D', '\x4', '\x1E', '\t', '\x1E', '\x4', '\x1F', '\t', '\x1F', '\x3', 
		'\x2', '\x3', '\x2', '\x3', '\x3', '\x3', '\x3', '\x3', '\x4', '\x3', 
		'\x4', '\x3', '\x5', '\x3', '\x5', '\x3', '\x6', '\x3', '\x6', '\x3', 
		'\a', '\x3', '\a', '\x3', '\b', '\x3', '\b', '\x3', '\t', '\x3', '\t', 
		'\x3', '\n', '\x3', '\n', '\x3', '\v', '\x3', '\v', '\x3', '\f', '\x3', 
		'\f', '\x3', '\r', '\x3', '\r', '\x3', '\xE', '\x3', '\xE', '\x3', '\xE', 
		'\x3', '\xE', '\x3', '\xE', '\x3', '\xE', '\x3', '\xE', '\x3', '\xF', 
		'\x3', '\xF', '\a', '\xF', '\x61', '\n', '\xF', '\f', '\xF', '\xE', '\xF', 
		'\x64', '\v', '\xF', '\x3', '\x10', '\x3', '\x10', '\x3', '\x10', '\a', 
		'\x10', 'i', '\n', '\x10', '\f', '\x10', '\xE', '\x10', 'l', '\v', '\x10', 
		'\x3', '\x10', '\x3', '\x10', '\x3', '\x11', '\x3', '\x11', '\x3', '\x11', 
		'\x5', '\x11', 's', '\n', '\x11', '\x3', '\x12', '\x3', '\x12', '\x3', 
		'\x12', '\x3', '\x12', '\x3', '\x12', '\x3', '\x12', '\x3', '\x13', '\x3', 
		'\x13', '\x3', '\x14', '\x5', '\x14', '~', '\n', '\x14', '\x3', '\x14', 
		'\x3', '\x14', '\x3', '\x14', '\x6', '\x14', '\x83', '\n', '\x14', '\r', 
		'\x14', '\xE', '\x14', '\x84', '\x5', '\x14', '\x87', '\n', '\x14', '\x3', 
		'\x14', '\x5', '\x14', '\x8A', '\n', '\x14', '\x3', '\x15', '\x3', '\x15', 
		'\x3', '\x15', '\a', '\x15', '\x8F', '\n', '\x15', '\f', '\x15', '\xE', 
		'\x15', '\x92', '\v', '\x15', '\x5', '\x15', '\x94', '\n', '\x15', '\x3', 
		'\x16', '\x3', '\x16', '\x5', '\x16', '\x98', '\n', '\x16', '\x3', '\x16', 
		'\x3', '\x16', '\x3', '\x17', '\x3', '\x17', '\x6', '\x17', '\x9E', '\n', 
		'\x17', '\r', '\x17', '\xE', '\x17', '\x9F', '\x3', '\x18', '\x6', '\x18', 
		'\xA3', '\n', '\x18', '\r', '\x18', '\xE', '\x18', '\xA4', '\x3', '\x18', 
		'\x3', '\x18', '\x3', '\x19', '\x3', '\x19', '\x3', '\x19', '\x3', '\x19', 
		'\x3', '\x19', '\a', '\x19', '\xAE', '\n', '\x19', '\f', '\x19', '\xE', 
		'\x19', '\xB1', '\v', '\x19', '\x3', '\x19', '\x3', '\x19', '\x3', '\x1A', 
		'\x3', '\x1A', '\x3', '\x1A', '\x3', '\x1A', '\x3', '\x1A', '\a', '\x1A', 
		'\xBA', '\n', '\x1A', '\f', '\x1A', '\xE', '\x1A', '\xBD', '\v', '\x1A', 
		'\x3', '\x1A', '\x3', '\x1A', '\x3', '\x1A', '\x3', '\x1A', '\x3', '\x1A', 
		'\x3', '\x1B', '\x3', '\x1B', '\x3', '\x1B', '\x3', '\x1B', '\a', '\x1B', 
		'\xC8', '\n', '\x1B', '\f', '\x1B', '\xE', '\x1B', '\xCB', '\v', '\x1B', 
		'\x3', '\x1B', '\x3', '\x1B', '\x3', '\x1C', '\x3', '\x1C', '\x3', '\x1C', 
		'\x3', '\x1C', '\a', '\x1C', '\xD3', '\n', '\x1C', '\f', '\x1C', '\xE', 
		'\x1C', '\xD6', '\v', '\x1C', '\x3', '\x1C', '\x3', '\x1C', '\x3', '\x1C', 
		'\x3', '\x1C', '\x3', '\x1C', '\x3', '\x1D', '\x3', '\x1D', '\x3', '\x1E', 
		'\x3', '\x1E', '\x3', '\x1F', '\x3', '\x1F', '\x3', '\x1F', '\x3', '\x1F', 
		'\x4', '\xBB', '\xD4', '\x2', ' ', '\x3', '\x3', '\x5', '\x4', '\a', '\x5', 
		'\t', '\x6', '\v', '\a', '\r', '\b', '\xF', '\t', '\x11', '\n', '\x13', 
		'\v', '\x15', '\f', '\x17', '\r', '\x19', '\xE', '\x1B', '\xF', '\x1D', 
		'\x10', '\x1F', '\x11', '!', '\x2', '#', '\x2', '%', '\x2', '\'', '\x12', 
		')', '\x2', '+', '\x2', '-', '\x13', '/', '\x14', '\x31', '\x15', '\x33', 
		'\x16', '\x35', '\x17', '\x37', '\x18', '\x39', '\x2', ';', '\x2', '=', 
		'\x19', '\x3', '\x2', '\r', '\x6', '\x2', '&', '&', '\x42', '\\', '\x61', 
		'\x61', '\x63', '|', '\a', '\x2', '/', '/', '\x32', ';', '\x43', '\\', 
		'\x61', '\x61', '\x63', '|', '\x4', '\x2', '$', '$', '^', '^', '\n', '\x2', 
		'$', '$', '\x31', '\x31', '^', '^', '\x64', '\x64', 'h', 'h', 'p', 'p', 
		't', 't', 'v', 'v', '\x5', '\x2', '\x32', ';', '\x43', 'H', '\x63', 'h', 
		'\x3', '\x2', '\x32', ';', '\x3', '\x2', '\x33', ';', '\x4', '\x2', 'G', 
		'G', 'g', 'g', '\x4', '\x2', '-', '-', '/', '/', '\x5', '\x2', '\v', '\f', 
		'\xF', '\xF', '\"', '\"', '\x6', '\x2', '\f', '\f', '\xF', '\xF', '\x87', 
		'\x87', '\x202A', '\x202B', '\x2', '\xED', '\x2', '\x3', '\x3', '\x2', 
		'\x2', '\x2', '\x2', '\x5', '\x3', '\x2', '\x2', '\x2', '\x2', '\a', '\x3', 
		'\x2', '\x2', '\x2', '\x2', '\t', '\x3', '\x2', '\x2', '\x2', '\x2', '\v', 
		'\x3', '\x2', '\x2', '\x2', '\x2', '\r', '\x3', '\x2', '\x2', '\x2', '\x2', 
		'\xF', '\x3', '\x2', '\x2', '\x2', '\x2', '\x11', '\x3', '\x2', '\x2', 
		'\x2', '\x2', '\x13', '\x3', '\x2', '\x2', '\x2', '\x2', '\x15', '\x3', 
		'\x2', '\x2', '\x2', '\x2', '\x17', '\x3', '\x2', '\x2', '\x2', '\x2', 
		'\x19', '\x3', '\x2', '\x2', '\x2', '\x2', '\x1B', '\x3', '\x2', '\x2', 
		'\x2', '\x2', '\x1D', '\x3', '\x2', '\x2', '\x2', '\x2', '\x1F', '\x3', 
		'\x2', '\x2', '\x2', '\x2', '\'', '\x3', '\x2', '\x2', '\x2', '\x2', '-', 
		'\x3', '\x2', '\x2', '\x2', '\x2', '/', '\x3', '\x2', '\x2', '\x2', '\x2', 
		'\x31', '\x3', '\x2', '\x2', '\x2', '\x2', '\x33', '\x3', '\x2', '\x2', 
		'\x2', '\x2', '\x35', '\x3', '\x2', '\x2', '\x2', '\x2', '\x37', '\x3', 
		'\x2', '\x2', '\x2', '\x2', '=', '\x3', '\x2', '\x2', '\x2', '\x3', '?', 
		'\x3', '\x2', '\x2', '\x2', '\x5', '\x41', '\x3', '\x2', '\x2', '\x2', 
		'\a', '\x43', '\x3', '\x2', '\x2', '\x2', '\t', '\x45', '\x3', '\x2', 
		'\x2', '\x2', '\v', 'G', '\x3', '\x2', '\x2', '\x2', '\r', 'I', '\x3', 
		'\x2', '\x2', '\x2', '\xF', 'K', '\x3', '\x2', '\x2', '\x2', '\x11', 'M', 
		'\x3', '\x2', '\x2', '\x2', '\x13', 'O', '\x3', '\x2', '\x2', '\x2', '\x15', 
		'Q', '\x3', '\x2', '\x2', '\x2', '\x17', 'S', '\x3', '\x2', '\x2', '\x2', 
		'\x19', 'U', '\x3', '\x2', '\x2', '\x2', '\x1B', 'W', '\x3', '\x2', '\x2', 
		'\x2', '\x1D', '^', '\x3', '\x2', '\x2', '\x2', '\x1F', '\x65', '\x3', 
		'\x2', '\x2', '\x2', '!', 'o', '\x3', '\x2', '\x2', '\x2', '#', 't', '\x3', 
		'\x2', '\x2', '\x2', '%', 'z', '\x3', '\x2', '\x2', '\x2', '\'', '}', 
		'\x3', '\x2', '\x2', '\x2', ')', '\x93', '\x3', '\x2', '\x2', '\x2', '+', 
		'\x95', '\x3', '\x2', '\x2', '\x2', '-', '\x9B', '\x3', '\x2', '\x2', 
		'\x2', '/', '\xA2', '\x3', '\x2', '\x2', '\x2', '\x31', '\xA8', '\x3', 
		'\x2', '\x2', '\x2', '\x33', '\xB4', '\x3', '\x2', '\x2', '\x2', '\x35', 
		'\xC3', '\x3', '\x2', '\x2', '\x2', '\x37', '\xCE', '\x3', '\x2', '\x2', 
		'\x2', '\x39', '\xDC', '\x3', '\x2', '\x2', '\x2', ';', '\xDE', '\x3', 
		'\x2', '\x2', '\x2', '=', '\xE0', '\x3', '\x2', '\x2', '\x2', '?', '@', 
		'\a', '*', '\x2', '\x2', '@', '\x4', '\x3', '\x2', '\x2', '\x2', '\x41', 
		'\x42', '\a', '+', '\x2', '\x2', '\x42', '\x6', '\x3', '\x2', '\x2', '\x2', 
		'\x43', '\x44', '\a', ',', '\x2', '\x2', '\x44', '\b', '\x3', '\x2', '\x2', 
		'\x2', '\x45', '\x46', '\a', '\x30', '\x2', '\x2', '\x46', '\n', '\x3', 
		'\x2', '\x2', '\x2', 'G', 'H', '\a', '\x31', '\x2', '\x2', 'H', '\f', 
		'\x3', '\x2', '\x2', '\x2', 'I', 'J', '\a', '=', '\x2', '\x2', 'J', '\xE', 
		'\x3', '\x2', '\x2', '\x2', 'K', 'L', '\a', '\x41', '\x2', '\x2', 'L', 
		'\x10', '\x3', '\x2', '\x2', '\x2', 'M', 'N', '\a', ']', '\x2', '\x2', 
		'N', '\x12', '\x3', '\x2', '\x2', '\x2', 'O', 'P', '\a', '_', '\x2', '\x2', 
		'P', '\x14', '\x3', '\x2', '\x2', '\x2', 'Q', 'R', '\a', '~', '\x2', '\x2', 
		'R', '\x16', '\x3', '\x2', '\x2', '\x2', 'S', 'T', '\a', '-', '\x2', '\x2', 
		'T', '\x18', '\x3', '\x2', '\x2', '\x2', 'U', 'V', '\a', '?', '\x2', '\x2', 
		'V', '\x1A', '\x3', '\x2', '\x2', '\x2', 'W', 'X', '\a', 'k', '\x2', '\x2', 
		'X', 'Y', '\a', 'o', '\x2', '\x2', 'Y', 'Z', '\a', 'r', '\x2', '\x2', 
		'Z', '[', '\a', 'q', '\x2', '\x2', '[', '\\', '\a', 't', '\x2', '\x2', 
		'\\', ']', '\a', 'v', '\x2', '\x2', ']', '\x1C', '\x3', '\x2', '\x2', 
		'\x2', '^', '\x62', '\t', '\x2', '\x2', '\x2', '_', '\x61', '\t', '\x3', 
		'\x2', '\x2', '`', '_', '\x3', '\x2', '\x2', '\x2', '\x61', '\x64', '\x3', 
		'\x2', '\x2', '\x2', '\x62', '`', '\x3', '\x2', '\x2', '\x2', '\x62', 
		'\x63', '\x3', '\x2', '\x2', '\x2', '\x63', '\x1E', '\x3', '\x2', '\x2', 
		'\x2', '\x64', '\x62', '\x3', '\x2', '\x2', '\x2', '\x65', 'j', '\a', 
		'$', '\x2', '\x2', '\x66', 'i', '\x5', '!', '\x11', '\x2', 'g', 'i', '\n', 
		'\x4', '\x2', '\x2', 'h', '\x66', '\x3', '\x2', '\x2', '\x2', 'h', 'g', 
		'\x3', '\x2', '\x2', '\x2', 'i', 'l', '\x3', '\x2', '\x2', '\x2', 'j', 
		'h', '\x3', '\x2', '\x2', '\x2', 'j', 'k', '\x3', '\x2', '\x2', '\x2', 
		'k', 'm', '\x3', '\x2', '\x2', '\x2', 'l', 'j', '\x3', '\x2', '\x2', '\x2', 
		'm', 'n', '\a', '$', '\x2', '\x2', 'n', ' ', '\x3', '\x2', '\x2', '\x2', 
		'o', 'r', '\a', '^', '\x2', '\x2', 'p', 's', '\t', '\x5', '\x2', '\x2', 
		'q', 's', '\x5', '#', '\x12', '\x2', 'r', 'p', '\x3', '\x2', '\x2', '\x2', 
		'r', 'q', '\x3', '\x2', '\x2', '\x2', 's', '\"', '\x3', '\x2', '\x2', 
		'\x2', 't', 'u', '\a', 'w', '\x2', '\x2', 'u', 'v', '\x5', '%', '\x13', 
		'\x2', 'v', 'w', '\x5', '%', '\x13', '\x2', 'w', 'x', '\x5', '%', '\x13', 
		'\x2', 'x', 'y', '\x5', '%', '\x13', '\x2', 'y', '$', '\x3', '\x2', '\x2', 
		'\x2', 'z', '{', '\t', '\x6', '\x2', '\x2', '{', '&', '\x3', '\x2', '\x2', 
		'\x2', '|', '~', '\a', '/', '\x2', '\x2', '}', '|', '\x3', '\x2', '\x2', 
		'\x2', '}', '~', '\x3', '\x2', '\x2', '\x2', '~', '\x7F', '\x3', '\x2', 
		'\x2', '\x2', '\x7F', '\x86', '\x5', ')', '\x15', '\x2', '\x80', '\x82', 
		'\a', '\x30', '\x2', '\x2', '\x81', '\x83', '\t', '\a', '\x2', '\x2', 
		'\x82', '\x81', '\x3', '\x2', '\x2', '\x2', '\x83', '\x84', '\x3', '\x2', 
		'\x2', '\x2', '\x84', '\x82', '\x3', '\x2', '\x2', '\x2', '\x84', '\x85', 
		'\x3', '\x2', '\x2', '\x2', '\x85', '\x87', '\x3', '\x2', '\x2', '\x2', 
		'\x86', '\x80', '\x3', '\x2', '\x2', '\x2', '\x86', '\x87', '\x3', '\x2', 
		'\x2', '\x2', '\x87', '\x89', '\x3', '\x2', '\x2', '\x2', '\x88', '\x8A', 
		'\x5', '+', '\x16', '\x2', '\x89', '\x88', '\x3', '\x2', '\x2', '\x2', 
		'\x89', '\x8A', '\x3', '\x2', '\x2', '\x2', '\x8A', '(', '\x3', '\x2', 
		'\x2', '\x2', '\x8B', '\x94', '\a', '\x32', '\x2', '\x2', '\x8C', '\x90', 
		'\t', '\b', '\x2', '\x2', '\x8D', '\x8F', '\t', '\a', '\x2', '\x2', '\x8E', 
		'\x8D', '\x3', '\x2', '\x2', '\x2', '\x8F', '\x92', '\x3', '\x2', '\x2', 
		'\x2', '\x90', '\x8E', '\x3', '\x2', '\x2', '\x2', '\x90', '\x91', '\x3', 
		'\x2', '\x2', '\x2', '\x91', '\x94', '\x3', '\x2', '\x2', '\x2', '\x92', 
		'\x90', '\x3', '\x2', '\x2', '\x2', '\x93', '\x8B', '\x3', '\x2', '\x2', 
		'\x2', '\x93', '\x8C', '\x3', '\x2', '\x2', '\x2', '\x94', '*', '\x3', 
		'\x2', '\x2', '\x2', '\x95', '\x97', '\t', '\t', '\x2', '\x2', '\x96', 
		'\x98', '\t', '\n', '\x2', '\x2', '\x97', '\x96', '\x3', '\x2', '\x2', 
		'\x2', '\x97', '\x98', '\x3', '\x2', '\x2', '\x2', '\x98', '\x99', '\x3', 
		'\x2', '\x2', '\x2', '\x99', '\x9A', '\x5', ')', '\x15', '\x2', '\x9A', 
		',', '\x3', '\x2', '\x2', '\x2', '\x9B', '\x9D', '\a', '%', '\x2', '\x2', 
		'\x9C', '\x9E', '\x5', '%', '\x13', '\x2', '\x9D', '\x9C', '\x3', '\x2', 
		'\x2', '\x2', '\x9E', '\x9F', '\x3', '\x2', '\x2', '\x2', '\x9F', '\x9D', 
		'\x3', '\x2', '\x2', '\x2', '\x9F', '\xA0', '\x3', '\x2', '\x2', '\x2', 
		'\xA0', '.', '\x3', '\x2', '\x2', '\x2', '\xA1', '\xA3', '\t', '\v', '\x2', 
		'\x2', '\xA2', '\xA1', '\x3', '\x2', '\x2', '\x2', '\xA3', '\xA4', '\x3', 
		'\x2', '\x2', '\x2', '\xA4', '\xA2', '\x3', '\x2', '\x2', '\x2', '\xA4', 
		'\xA5', '\x3', '\x2', '\x2', '\x2', '\xA5', '\xA6', '\x3', '\x2', '\x2', 
		'\x2', '\xA6', '\xA7', '\b', '\x18', '\x2', '\x2', '\xA7', '\x30', '\x3', 
		'\x2', '\x2', '\x2', '\xA8', '\xA9', '\a', '\x31', '\x2', '\x2', '\xA9', 
		'\xAA', '\a', '\x31', '\x2', '\x2', '\xAA', '\xAB', '\a', '\x31', '\x2', 
		'\x2', '\xAB', '\xAF', '\x3', '\x2', '\x2', '\x2', '\xAC', '\xAE', '\x5', 
		'\x39', '\x1D', '\x2', '\xAD', '\xAC', '\x3', '\x2', '\x2', '\x2', '\xAE', 
		'\xB1', '\x3', '\x2', '\x2', '\x2', '\xAF', '\xAD', '\x3', '\x2', '\x2', 
		'\x2', '\xAF', '\xB0', '\x3', '\x2', '\x2', '\x2', '\xB0', '\xB2', '\x3', 
		'\x2', '\x2', '\x2', '\xB1', '\xAF', '\x3', '\x2', '\x2', '\x2', '\xB2', 
		'\xB3', '\b', '\x19', '\x3', '\x2', '\xB3', '\x32', '\x3', '\x2', '\x2', 
		'\x2', '\xB4', '\xB5', '\a', '\x31', '\x2', '\x2', '\xB5', '\xB6', '\a', 
		',', '\x2', '\x2', '\xB6', '\xB7', '\a', ',', '\x2', '\x2', '\xB7', '\xBB', 
		'\x3', '\x2', '\x2', '\x2', '\xB8', '\xBA', '\v', '\x2', '\x2', '\x2', 
		'\xB9', '\xB8', '\x3', '\x2', '\x2', '\x2', '\xBA', '\xBD', '\x3', '\x2', 
		'\x2', '\x2', '\xBB', '\xBC', '\x3', '\x2', '\x2', '\x2', '\xBB', '\xB9', 
		'\x3', '\x2', '\x2', '\x2', '\xBC', '\xBE', '\x3', '\x2', '\x2', '\x2', 
		'\xBD', '\xBB', '\x3', '\x2', '\x2', '\x2', '\xBE', '\xBF', '\a', ',', 
		'\x2', '\x2', '\xBF', '\xC0', '\a', '\x31', '\x2', '\x2', '\xC0', '\xC1', 
		'\x3', '\x2', '\x2', '\x2', '\xC1', '\xC2', '\b', '\x1A', '\x3', '\x2', 
		'\xC2', '\x34', '\x3', '\x2', '\x2', '\x2', '\xC3', '\xC4', '\a', '\x31', 
		'\x2', '\x2', '\xC4', '\xC5', '\a', '\x31', '\x2', '\x2', '\xC5', '\xC9', 
		'\x3', '\x2', '\x2', '\x2', '\xC6', '\xC8', '\x5', '\x39', '\x1D', '\x2', 
		'\xC7', '\xC6', '\x3', '\x2', '\x2', '\x2', '\xC8', '\xCB', '\x3', '\x2', 
		'\x2', '\x2', '\xC9', '\xC7', '\x3', '\x2', '\x2', '\x2', '\xC9', '\xCA', 
		'\x3', '\x2', '\x2', '\x2', '\xCA', '\xCC', '\x3', '\x2', '\x2', '\x2', 
		'\xCB', '\xC9', '\x3', '\x2', '\x2', '\x2', '\xCC', '\xCD', '\b', '\x1B', 
		'\x3', '\x2', '\xCD', '\x36', '\x3', '\x2', '\x2', '\x2', '\xCE', '\xCF', 
		'\a', '\x31', '\x2', '\x2', '\xCF', '\xD0', '\a', ',', '\x2', '\x2', '\xD0', 
		'\xD4', '\x3', '\x2', '\x2', '\x2', '\xD1', '\xD3', '\v', '\x2', '\x2', 
		'\x2', '\xD2', '\xD1', '\x3', '\x2', '\x2', '\x2', '\xD3', '\xD6', '\x3', 
		'\x2', '\x2', '\x2', '\xD4', '\xD5', '\x3', '\x2', '\x2', '\x2', '\xD4', 
		'\xD2', '\x3', '\x2', '\x2', '\x2', '\xD5', '\xD7', '\x3', '\x2', '\x2', 
		'\x2', '\xD6', '\xD4', '\x3', '\x2', '\x2', '\x2', '\xD7', '\xD8', '\a', 
		',', '\x2', '\x2', '\xD8', '\xD9', '\a', '\x31', '\x2', '\x2', '\xD9', 
		'\xDA', '\x3', '\x2', '\x2', '\x2', '\xDA', '\xDB', '\b', '\x1C', '\x3', 
		'\x2', '\xDB', '\x38', '\x3', '\x2', '\x2', '\x2', '\xDC', '\xDD', '\n', 
		'\f', '\x2', '\x2', '\xDD', ':', '\x3', '\x2', '\x2', '\x2', '\xDE', '\xDF', 
		'\t', '\f', '\x2', '\x2', '\xDF', '<', '\x3', '\x2', '\x2', '\x2', '\xE0', 
		'\xE1', '\v', '\x2', '\x2', '\x2', '\xE1', '\xE2', '\x3', '\x2', '\x2', 
		'\x2', '\xE2', '\xE3', '\b', '\x1F', '\x4', '\x2', '\xE3', '>', '\x3', 
		'\x2', '\x2', '\x2', '\x14', '\x2', '\x62', 'h', 'j', 'r', '}', '\x84', 
		'\x86', '\x89', '\x90', '\x93', '\x97', '\x9F', '\xA4', '\xAF', '\xBB', 
		'\xC9', '\xD4', '\x5', '\b', '\x2', '\x2', '\x2', '\x4', '\x2', '\x2', 
		'\x3', '\x2',
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
