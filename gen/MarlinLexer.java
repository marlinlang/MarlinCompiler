// Generated from D:/Projects/Marlin/MarlinCompiler/src\MarlinLexer.g4 by ANTLR 4.9.2
import org.antlr.v4.runtime.Lexer;
import org.antlr.v4.runtime.CharStream;
import org.antlr.v4.runtime.Token;
import org.antlr.v4.runtime.TokenStream;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.misc.*;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast"})
public class MarlinLexer extends Lexer {
	static { RuntimeMetaData.checkVersion("4.9.2", RuntimeMetaData.VERSION); }

	protected static final DFA[] _decisionToDFA;
	protected static final PredictionContextCache _sharedContextCache =
		new PredictionContextCache();
	public static final int
		WHITESPACES=1, MODULE=2, CLASS=3, STRUCT=4, RETURN=5, NEW=6, NATIVE=7, 
		NULL=8, TRUE=9, FALSE=10, PRIVATE=11, INTERNAL=12, PUBLIC=13, STATIC=14, 
		READONLY=15, SEALED=16, AT=17, DOT=18, COMMA=19, COLON=20, SEMICOLON=21, 
		QUESTION=22, LBRACE=23, RBRACE=24, LPAREN=25, RPAREN=26, LBRACKET=27, 
		RBRACKET=28, ASSIGN=29, QUOTE=30, DOUBLE_QUOTE=31, DOUBLE_COLON=32, ARROW=33, 
		IDENTIFIER=34, INTEGER=35, NORMAL_STRING=36, CHARACTER=37, DOUBLE=38;
	public static String[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static String[] modeNames = {
		"DEFAULT_MODE"
	};

	private static String[] makeRuleNames() {
		return new String[] {
			"WHITESPACES", "MODULE", "CLASS", "STRUCT", "RETURN", "NEW", "NATIVE", 
			"NULL", "TRUE", "FALSE", "PRIVATE", "INTERNAL", "PUBLIC", "STATIC", "READONLY", 
			"SEALED", "AT", "DOT", "COMMA", "COLON", "SEMICOLON", "QUESTION", "LBRACE", 
			"RBRACE", "LPAREN", "RPAREN", "LBRACKET", "RBRACKET", "ASSIGN", "QUOTE", 
			"DOUBLE_QUOTE", "DOUBLE_COLON", "ARROW", "IDENTIFIER", "INTEGER", "NORMAL_STRING", 
			"CHARACTER", "DOUBLE", "WholeNumber", "HexNumber", "DoubleNumber", "SingleLineComment", 
			"BlockComment", "Character", "EscapeSequence"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, null, "'module'", "'class'", "'struct'", "'return'", "'new'", "'native'", 
			"'null'", "'true'", "'false'", "'private'", "'internal'", "'public'", 
			"'static'", "'readonly'", "'sealed'", "'@'", "'.'", "','", "':'", "';'", 
			"'?'", "'{'", "'}'", "'('", "')'", "'['", "']'", "'='", "'''", "'\"'", 
			"'::'", "'=>'"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, "WHITESPACES", "MODULE", "CLASS", "STRUCT", "RETURN", "NEW", "NATIVE", 
			"NULL", "TRUE", "FALSE", "PRIVATE", "INTERNAL", "PUBLIC", "STATIC", "READONLY", 
			"SEALED", "AT", "DOT", "COMMA", "COLON", "SEMICOLON", "QUESTION", "LBRACE", 
			"RBRACE", "LPAREN", "RPAREN", "LBRACKET", "RBRACKET", "ASSIGN", "QUOTE", 
			"DOUBLE_QUOTE", "DOUBLE_COLON", "ARROW", "IDENTIFIER", "INTEGER", "NORMAL_STRING", 
			"CHARACTER", "DOUBLE"
		};
	}
	private static final String[] _SYMBOLIC_NAMES = makeSymbolicNames();
	public static final Vocabulary VOCABULARY = new VocabularyImpl(_LITERAL_NAMES, _SYMBOLIC_NAMES);

	/**
	 * @deprecated Use {@link #VOCABULARY} instead.
	 */
	@Deprecated
	public static final String[] tokenNames;
	static {
		tokenNames = new String[_SYMBOLIC_NAMES.length];
		for (int i = 0; i < tokenNames.length; i++) {
			tokenNames[i] = VOCABULARY.getLiteralName(i);
			if (tokenNames[i] == null) {
				tokenNames[i] = VOCABULARY.getSymbolicName(i);
			}

			if (tokenNames[i] == null) {
				tokenNames[i] = "<INVALID>";
			}
		}
	}

	@Override
	@Deprecated
	public String[] getTokenNames() {
		return tokenNames;
	}

	@Override

	public Vocabulary getVocabulary() {
		return VOCABULARY;
	}


	public MarlinLexer(CharStream input) {
		super(input);
		_interp = new LexerATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}

	@Override
	public String getGrammarFileName() { return "MarlinLexer.g4"; }

	@Override
	public String[] getRuleNames() { return ruleNames; }

	@Override
	public String getSerializedATN() { return _serializedATN; }

	@Override
	public String[] getChannelNames() { return channelNames; }

	@Override
	public String[] getModeNames() { return modeNames; }

	@Override
	public ATN getATN() { return _ATN; }

	public static final String _serializedATN =
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\2(\u0147\b\1\4\2\t"+
		"\2\4\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\4\n\t\n\4\13"+
		"\t\13\4\f\t\f\4\r\t\r\4\16\t\16\4\17\t\17\4\20\t\20\4\21\t\21\4\22\t\22"+
		"\4\23\t\23\4\24\t\24\4\25\t\25\4\26\t\26\4\27\t\27\4\30\t\30\4\31\t\31"+
		"\4\32\t\32\4\33\t\33\4\34\t\34\4\35\t\35\4\36\t\36\4\37\t\37\4 \t \4!"+
		"\t!\4\"\t\"\4#\t#\4$\t$\4%\t%\4&\t&\4\'\t\'\4(\t(\4)\t)\4*\t*\4+\t+\4"+
		",\t,\4-\t-\4.\t.\3\2\3\2\3\2\6\2a\n\2\r\2\16\2b\3\2\3\2\3\3\3\3\3\3\3"+
		"\3\3\3\3\3\3\3\3\4\3\4\3\4\3\4\3\4\3\4\3\5\3\5\3\5\3\5\3\5\3\5\3\5\3\6"+
		"\3\6\3\6\3\6\3\6\3\6\3\6\3\7\3\7\3\7\3\7\3\b\3\b\3\b\3\b\3\b\3\b\3\b\3"+
		"\t\3\t\3\t\3\t\3\t\3\n\3\n\3\n\3\n\3\n\3\13\3\13\3\13\3\13\3\13\3\13\3"+
		"\f\3\f\3\f\3\f\3\f\3\f\3\f\3\f\3\r\3\r\3\r\3\r\3\r\3\r\3\r\3\r\3\r\3\16"+
		"\3\16\3\16\3\16\3\16\3\16\3\16\3\17\3\17\3\17\3\17\3\17\3\17\3\17\3\20"+
		"\3\20\3\20\3\20\3\20\3\20\3\20\3\20\3\20\3\21\3\21\3\21\3\21\3\21\3\21"+
		"\3\21\3\22\3\22\3\23\3\23\3\24\3\24\3\25\3\25\3\26\3\26\3\27\3\27\3\30"+
		"\3\30\3\31\3\31\3\32\3\32\3\33\3\33\3\34\3\34\3\35\3\35\3\36\3\36\3\37"+
		"\3\37\3 \3 \3!\3!\3!\3\"\3\"\3\"\3#\3#\7#\u00f2\n#\f#\16#\u00f5\13#\3"+
		"$\3$\5$\u00f9\n$\3%\3%\7%\u00fd\n%\f%\16%\u0100\13%\3%\3%\3&\3&\3&\5&"+
		"\u0107\n&\3&\3&\3\'\3\'\3(\5(\u010e\n(\3(\6(\u0111\n(\r(\16(\u0112\3)"+
		"\3)\3)\3)\3)\3*\3*\3*\6*\u011d\n*\r*\16*\u011e\3*\5*\u0122\n*\3*\3*\6"+
		"*\u0126\n*\r*\16*\u0127\5*\u012a\n*\3+\3+\3+\3+\7+\u0130\n+\f+\16+\u0133"+
		"\13+\3,\3,\3,\3,\7,\u0139\n,\f,\16,\u013c\13,\3,\3,\3,\3-\3-\5-\u0143"+
		"\n-\3.\3.\3.\3\u013a\2/\3\3\5\4\7\5\t\6\13\7\r\b\17\t\21\n\23\13\25\f"+
		"\27\r\31\16\33\17\35\20\37\21!\22#\23%\24\'\25)\26+\27-\30/\31\61\32\63"+
		"\33\65\34\67\359\36;\37= ?!A\"C#E$G%I&K\'M(O\2Q\2S\2U\2W\2Y\2[\2\3\2\13"+
		"\5\2\13\f\17\17\"\"\5\2C\\aac|\6\2\62;C\\aac|\3\2$$\3\2))\4\2--//\3\2"+
		"\62;\4\2\f\f\17\17\n\2$$))^^ddhhppttvv\2\u014f\2\3\3\2\2\2\2\5\3\2\2\2"+
		"\2\7\3\2\2\2\2\t\3\2\2\2\2\13\3\2\2\2\2\r\3\2\2\2\2\17\3\2\2\2\2\21\3"+
		"\2\2\2\2\23\3\2\2\2\2\25\3\2\2\2\2\27\3\2\2\2\2\31\3\2\2\2\2\33\3\2\2"+
		"\2\2\35\3\2\2\2\2\37\3\2\2\2\2!\3\2\2\2\2#\3\2\2\2\2%\3\2\2\2\2\'\3\2"+
		"\2\2\2)\3\2\2\2\2+\3\2\2\2\2-\3\2\2\2\2/\3\2\2\2\2\61\3\2\2\2\2\63\3\2"+
		"\2\2\2\65\3\2\2\2\2\67\3\2\2\2\29\3\2\2\2\2;\3\2\2\2\2=\3\2\2\2\2?\3\2"+
		"\2\2\2A\3\2\2\2\2C\3\2\2\2\2E\3\2\2\2\2G\3\2\2\2\2I\3\2\2\2\2K\3\2\2\2"+
		"\2M\3\2\2\2\3`\3\2\2\2\5f\3\2\2\2\7m\3\2\2\2\ts\3\2\2\2\13z\3\2\2\2\r"+
		"\u0081\3\2\2\2\17\u0085\3\2\2\2\21\u008c\3\2\2\2\23\u0091\3\2\2\2\25\u0096"+
		"\3\2\2\2\27\u009c\3\2\2\2\31\u00a4\3\2\2\2\33\u00ad\3\2\2\2\35\u00b4\3"+
		"\2\2\2\37\u00bb\3\2\2\2!\u00c4\3\2\2\2#\u00cb\3\2\2\2%\u00cd\3\2\2\2\'"+
		"\u00cf\3\2\2\2)\u00d1\3\2\2\2+\u00d3\3\2\2\2-\u00d5\3\2\2\2/\u00d7\3\2"+
		"\2\2\61\u00d9\3\2\2\2\63\u00db\3\2\2\2\65\u00dd\3\2\2\2\67\u00df\3\2\2"+
		"\29\u00e1\3\2\2\2;\u00e3\3\2\2\2=\u00e5\3\2\2\2?\u00e7\3\2\2\2A\u00e9"+
		"\3\2\2\2C\u00ec\3\2\2\2E\u00ef\3\2\2\2G\u00f8\3\2\2\2I\u00fa\3\2\2\2K"+
		"\u0103\3\2\2\2M\u010a\3\2\2\2O\u010d\3\2\2\2Q\u0114\3\2\2\2S\u0129\3\2"+
		"\2\2U\u012b\3\2\2\2W\u0134\3\2\2\2Y\u0142\3\2\2\2[\u0144\3\2\2\2]a\5W"+
		",\2^a\5U+\2_a\t\2\2\2`]\3\2\2\2`^\3\2\2\2`_\3\2\2\2ab\3\2\2\2b`\3\2\2"+
		"\2bc\3\2\2\2cd\3\2\2\2de\b\2\2\2e\4\3\2\2\2fg\7o\2\2gh\7q\2\2hi\7f\2\2"+
		"ij\7w\2\2jk\7n\2\2kl\7g\2\2l\6\3\2\2\2mn\7e\2\2no\7n\2\2op\7c\2\2pq\7"+
		"u\2\2qr\7u\2\2r\b\3\2\2\2st\7u\2\2tu\7v\2\2uv\7t\2\2vw\7w\2\2wx\7e\2\2"+
		"xy\7v\2\2y\n\3\2\2\2z{\7t\2\2{|\7g\2\2|}\7v\2\2}~\7w\2\2~\177\7t\2\2\177"+
		"\u0080\7p\2\2\u0080\f\3\2\2\2\u0081\u0082\7p\2\2\u0082\u0083\7g\2\2\u0083"+
		"\u0084\7y\2\2\u0084\16\3\2\2\2\u0085\u0086\7p\2\2\u0086\u0087\7c\2\2\u0087"+
		"\u0088\7v\2\2\u0088\u0089\7k\2\2\u0089\u008a\7x\2\2\u008a\u008b\7g\2\2"+
		"\u008b\20\3\2\2\2\u008c\u008d\7p\2\2\u008d\u008e\7w\2\2\u008e\u008f\7"+
		"n\2\2\u008f\u0090\7n\2\2\u0090\22\3\2\2\2\u0091\u0092\7v\2\2\u0092\u0093"+
		"\7t\2\2\u0093\u0094\7w\2\2\u0094\u0095\7g\2\2\u0095\24\3\2\2\2\u0096\u0097"+
		"\7h\2\2\u0097\u0098\7c\2\2\u0098\u0099\7n\2\2\u0099\u009a\7u\2\2\u009a"+
		"\u009b\7g\2\2\u009b\26\3\2\2\2\u009c\u009d\7r\2\2\u009d\u009e\7t\2\2\u009e"+
		"\u009f\7k\2\2\u009f\u00a0\7x\2\2\u00a0\u00a1\7c\2\2\u00a1\u00a2\7v\2\2"+
		"\u00a2\u00a3\7g\2\2\u00a3\30\3\2\2\2\u00a4\u00a5\7k\2\2\u00a5\u00a6\7"+
		"p\2\2\u00a6\u00a7\7v\2\2\u00a7\u00a8\7g\2\2\u00a8\u00a9\7t\2\2\u00a9\u00aa"+
		"\7p\2\2\u00aa\u00ab\7c\2\2\u00ab\u00ac\7n\2\2\u00ac\32\3\2\2\2\u00ad\u00ae"+
		"\7r\2\2\u00ae\u00af\7w\2\2\u00af\u00b0\7d\2\2\u00b0\u00b1\7n\2\2\u00b1"+
		"\u00b2\7k\2\2\u00b2\u00b3\7e\2\2\u00b3\34\3\2\2\2\u00b4\u00b5\7u\2\2\u00b5"+
		"\u00b6\7v\2\2\u00b6\u00b7\7c\2\2\u00b7\u00b8\7v\2\2\u00b8\u00b9\7k\2\2"+
		"\u00b9\u00ba\7e\2\2\u00ba\36\3\2\2\2\u00bb\u00bc\7t\2\2\u00bc\u00bd\7"+
		"g\2\2\u00bd\u00be\7c\2\2\u00be\u00bf\7f\2\2\u00bf\u00c0\7q\2\2\u00c0\u00c1"+
		"\7p\2\2\u00c1\u00c2\7n\2\2\u00c2\u00c3\7{\2\2\u00c3 \3\2\2\2\u00c4\u00c5"+
		"\7u\2\2\u00c5\u00c6\7g\2\2\u00c6\u00c7\7c\2\2\u00c7\u00c8\7n\2\2\u00c8"+
		"\u00c9\7g\2\2\u00c9\u00ca\7f\2\2\u00ca\"\3\2\2\2\u00cb\u00cc\7B\2\2\u00cc"+
		"$\3\2\2\2\u00cd\u00ce\7\60\2\2\u00ce&\3\2\2\2\u00cf\u00d0\7.\2\2\u00d0"+
		"(\3\2\2\2\u00d1\u00d2\7<\2\2\u00d2*\3\2\2\2\u00d3\u00d4\7=\2\2\u00d4,"+
		"\3\2\2\2\u00d5\u00d6\7A\2\2\u00d6.\3\2\2\2\u00d7\u00d8\7}\2\2\u00d8\60"+
		"\3\2\2\2\u00d9\u00da\7\177\2\2\u00da\62\3\2\2\2\u00db\u00dc\7*\2\2\u00dc"+
		"\64\3\2\2\2\u00dd\u00de\7+\2\2\u00de\66\3\2\2\2\u00df\u00e0\7]\2\2\u00e0"+
		"8\3\2\2\2\u00e1\u00e2\7_\2\2\u00e2:\3\2\2\2\u00e3\u00e4\7?\2\2\u00e4<"+
		"\3\2\2\2\u00e5\u00e6\7)\2\2\u00e6>\3\2\2\2\u00e7\u00e8\7$\2\2\u00e8@\3"+
		"\2\2\2\u00e9\u00ea\7<\2\2\u00ea\u00eb\7<\2\2\u00ebB\3\2\2\2\u00ec\u00ed"+
		"\7?\2\2\u00ed\u00ee\7@\2\2\u00eeD\3\2\2\2\u00ef\u00f3\t\3\2\2\u00f0\u00f2"+
		"\t\4\2\2\u00f1\u00f0\3\2\2\2\u00f2\u00f5\3\2\2\2\u00f3\u00f1\3\2\2\2\u00f3"+
		"\u00f4\3\2\2\2\u00f4F\3\2\2\2\u00f5\u00f3\3\2\2\2\u00f6\u00f9\5O(\2\u00f7"+
		"\u00f9\5Q)\2\u00f8\u00f6\3\2\2\2\u00f8\u00f7\3\2\2\2\u00f9H\3\2\2\2\u00fa"+
		"\u00fe\5? \2\u00fb\u00fd\n\5\2\2\u00fc\u00fb\3\2\2\2\u00fd\u0100\3\2\2"+
		"\2\u00fe\u00fc\3\2\2\2\u00fe\u00ff\3\2\2\2\u00ff\u0101\3\2\2\2\u0100\u00fe"+
		"\3\2\2\2\u0101\u0102\5? \2\u0102J\3\2\2\2\u0103\u0106\5=\37\2\u0104\u0107"+
		"\5[.\2\u0105\u0107\n\6\2\2\u0106\u0104\3\2\2\2\u0106\u0105\3\2\2\2\u0107"+
		"\u0108\3\2\2\2\u0108\u0109\5=\37\2\u0109L\3\2\2\2\u010a\u010b\5S*\2\u010b"+
		"N\3\2\2\2\u010c\u010e\t\7\2\2\u010d\u010c\3\2\2\2\u010d\u010e\3\2\2\2"+
		"\u010e\u0110\3\2\2\2\u010f\u0111\t\b\2\2\u0110\u010f\3\2\2\2\u0111\u0112"+
		"\3\2\2\2\u0112\u0110\3\2\2\2\u0112\u0113\3\2\2\2\u0113P\3\2\2\2\u0114"+
		"\u0115\7\62\2\2\u0115\u0116\7z\2\2\u0116\u0117\3\2\2\2\u0117\u0118\5O"+
		"(\2\u0118R\3\2\2\2\u0119\u011a\5O(\2\u011a\u011c\7\60\2\2\u011b\u011d"+
		"\t\b\2\2\u011c\u011b\3\2\2\2\u011d\u011e\3\2\2\2\u011e\u011c\3\2\2\2\u011e"+
		"\u011f\3\2\2\2\u011f\u012a\3\2\2\2\u0120\u0122\t\7\2\2\u0121\u0120\3\2"+
		"\2\2\u0121\u0122\3\2\2\2\u0122\u0123\3\2\2\2\u0123\u0125\7\60\2\2\u0124"+
		"\u0126\t\b\2\2\u0125\u0124\3\2\2\2\u0126\u0127\3\2\2\2\u0127\u0125\3\2"+
		"\2\2\u0127\u0128\3\2\2\2\u0128\u012a\3\2\2\2\u0129\u0119\3\2\2\2\u0129"+
		"\u0121\3\2\2\2\u012aT\3\2\2\2\u012b\u012c\7\61\2\2\u012c\u012d\7\61\2"+
		"\2\u012d\u0131\3\2\2\2\u012e\u0130\n\t\2\2\u012f\u012e\3\2\2\2\u0130\u0133"+
		"\3\2\2\2\u0131\u012f\3\2\2\2\u0131\u0132\3\2\2\2\u0132V\3\2\2\2\u0133"+
		"\u0131\3\2\2\2\u0134\u0135\7\61\2\2\u0135\u0136\7,\2\2\u0136\u013a\3\2"+
		"\2\2\u0137\u0139\13\2\2\2\u0138\u0137\3\2\2\2\u0139\u013c\3\2\2\2\u013a"+
		"\u013b\3\2\2\2\u013a\u0138\3\2\2\2\u013b\u013d\3\2\2\2\u013c\u013a\3\2"+
		"\2\2\u013d\u013e\7,\2\2\u013e\u013f\7\61\2\2\u013fX\3\2\2\2\u0140\u0143"+
		"\5[.\2\u0141\u0143\13\2\2\2\u0142\u0140\3\2\2\2\u0142\u0141\3\2\2\2\u0143"+
		"Z\3\2\2\2\u0144\u0145\7^\2\2\u0145\u0146\t\n\2\2\u0146\\\3\2\2\2\22\2"+
		"`b\u00f3\u00f8\u00fe\u0106\u010d\u0112\u011e\u0121\u0127\u0129\u0131\u013a"+
		"\u0142\3\2\3\2";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}