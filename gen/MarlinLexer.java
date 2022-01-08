// Generated from D:/Marlin/MarlinCompiler/src\MarlinLexer.g4 by ANTLR 4.9.2
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
		LBRACE=22, RBRACE=23, LPAREN=24, RPAREN=25, LBRACKET=26, RBRACKET=27, 
		ASSIGN=28, QUOTE=29, DOUBLE_QUOTE=30, DOUBLE_COLON=31, ARROW=32, IDENTIFIER=33, 
		INTEGER=34, NORMAL_STRING=35, CHARACTER=36, DOUBLE=37;
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
			"SEALED", "AT", "DOT", "COMMA", "COLON", "SEMICOLON", "LBRACE", "RBRACE", 
			"LPAREN", "RPAREN", "LBRACKET", "RBRACKET", "ASSIGN", "QUOTE", "DOUBLE_QUOTE", 
			"DOUBLE_COLON", "ARROW", "IDENTIFIER", "INTEGER", "NORMAL_STRING", "CHARACTER", 
			"DOUBLE", "WholeNumber", "HexNumber", "DoubleNumber", "SingleLineComment", 
			"BlockComment", "Character", "EscapeSequence"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, null, "'module'", "'class'", "'struct'", "'return'", "'new'", "'native'", 
			"'null'", "'true'", "'false'", "'private'", "'internal'", "'public'", 
			"'static'", "'readonly'", "'sealed'", "'@'", "'.'", "','", "':'", "';'", 
			"'{'", "'}'", "'('", "')'", "'['", "']'", "'='", "'''", "'\"'", "'::'", 
			"'=>'"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, "WHITESPACES", "MODULE", "CLASS", "STRUCT", "RETURN", "NEW", "NATIVE", 
			"NULL", "TRUE", "FALSE", "PRIVATE", "INTERNAL", "PUBLIC", "STATIC", "READONLY", 
			"SEALED", "AT", "DOT", "COMMA", "COLON", "SEMICOLON", "LBRACE", "RBRACE", 
			"LPAREN", "RPAREN", "LBRACKET", "RBRACKET", "ASSIGN", "QUOTE", "DOUBLE_QUOTE", 
			"DOUBLE_COLON", "ARROW", "IDENTIFIER", "INTEGER", "NORMAL_STRING", "CHARACTER", 
			"DOUBLE"
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
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\2\'\u0140\b\1\4\2\t"+
		"\2\4\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\4\n\t\n\4\13"+
		"\t\13\4\f\t\f\4\r\t\r\4\16\t\16\4\17\t\17\4\20\t\20\4\21\t\21\4\22\t\22"+
		"\4\23\t\23\4\24\t\24\4\25\t\25\4\26\t\26\4\27\t\27\4\30\t\30\4\31\t\31"+
		"\4\32\t\32\4\33\t\33\4\34\t\34\4\35\t\35\4\36\t\36\4\37\t\37\4 \t \4!"+
		"\t!\4\"\t\"\4#\t#\4$\t$\4%\t%\4&\t&\4\'\t\'\4(\t(\4)\t)\4*\t*\4+\t+\4"+
		",\t,\4-\t-\3\2\3\2\3\2\6\2_\n\2\r\2\16\2`\3\2\3\2\3\3\3\3\3\3\3\3\3\3"+
		"\3\3\3\3\3\4\3\4\3\4\3\4\3\4\3\4\3\5\3\5\3\5\3\5\3\5\3\5\3\5\3\6\3\6\3"+
		"\6\3\6\3\6\3\6\3\6\3\7\3\7\3\7\3\7\3\b\3\b\3\b\3\b\3\b\3\b\3\b\3\t\3\t"+
		"\3\t\3\t\3\t\3\n\3\n\3\n\3\n\3\n\3\13\3\13\3\13\3\13\3\13\3\13\3\f\3\f"+
		"\3\f\3\f\3\f\3\f\3\f\3\f\3\r\3\r\3\r\3\r\3\r\3\r\3\r\3\r\3\r\3\16\3\16"+
		"\3\16\3\16\3\16\3\16\3\16\3\17\3\17\3\17\3\17\3\17\3\17\3\17\3\20\3\20"+
		"\3\20\3\20\3\20\3\20\3\20\3\20\3\20\3\21\3\21\3\21\3\21\3\21\3\21\3\21"+
		"\3\22\3\22\3\23\3\23\3\24\3\24\3\25\3\25\3\26\3\26\3\27\3\27\3\30\3\30"+
		"\3\31\3\31\3\32\3\32\3\33\3\33\3\34\3\34\3\35\3\35\3\36\3\36\3\37\3\37"+
		"\3 \3 \3 \3!\3!\3!\3\"\3\"\7\"\u00ee\n\"\f\"\16\"\u00f1\13\"\3#\3#\5#"+
		"\u00f5\n#\3$\3$\7$\u00f9\n$\f$\16$\u00fc\13$\3$\3$\3%\3%\3%\3%\3&\3&\3"+
		"\'\5\'\u0107\n\'\3\'\6\'\u010a\n\'\r\'\16\'\u010b\3(\3(\3(\3(\3(\3)\3"+
		")\3)\6)\u0116\n)\r)\16)\u0117\3)\5)\u011b\n)\3)\3)\6)\u011f\n)\r)\16)"+
		"\u0120\5)\u0123\n)\3*\3*\3*\3*\7*\u0129\n*\f*\16*\u012c\13*\3+\3+\3+\3"+
		"+\7+\u0132\n+\f+\16+\u0135\13+\3+\3+\3+\3,\3,\5,\u013c\n,\3-\3-\3-\3\u0133"+
		"\2.\3\3\5\4\7\5\t\6\13\7\r\b\17\t\21\n\23\13\25\f\27\r\31\16\33\17\35"+
		"\20\37\21!\22#\23%\24\'\25)\26+\27-\30/\31\61\32\63\33\65\34\67\359\36"+
		";\37= ?!A\"C#E$G%I&K\'M\2O\2Q\2S\2U\2W\2Y\2\3\2\n\5\2\13\f\17\17\"\"\5"+
		"\2C\\aac|\6\2\62;C\\aac|\4\2--//\3\2\62;\4\2\f\f\17\17\6\2\f\f\17\17)"+
		")^^\n\2$$))^^ddhhppttvv\2\u0147\2\3\3\2\2\2\2\5\3\2\2\2\2\7\3\2\2\2\2"+
		"\t\3\2\2\2\2\13\3\2\2\2\2\r\3\2\2\2\2\17\3\2\2\2\2\21\3\2\2\2\2\23\3\2"+
		"\2\2\2\25\3\2\2\2\2\27\3\2\2\2\2\31\3\2\2\2\2\33\3\2\2\2\2\35\3\2\2\2"+
		"\2\37\3\2\2\2\2!\3\2\2\2\2#\3\2\2\2\2%\3\2\2\2\2\'\3\2\2\2\2)\3\2\2\2"+
		"\2+\3\2\2\2\2-\3\2\2\2\2/\3\2\2\2\2\61\3\2\2\2\2\63\3\2\2\2\2\65\3\2\2"+
		"\2\2\67\3\2\2\2\29\3\2\2\2\2;\3\2\2\2\2=\3\2\2\2\2?\3\2\2\2\2A\3\2\2\2"+
		"\2C\3\2\2\2\2E\3\2\2\2\2G\3\2\2\2\2I\3\2\2\2\2K\3\2\2\2\3^\3\2\2\2\5d"+
		"\3\2\2\2\7k\3\2\2\2\tq\3\2\2\2\13x\3\2\2\2\r\177\3\2\2\2\17\u0083\3\2"+
		"\2\2\21\u008a\3\2\2\2\23\u008f\3\2\2\2\25\u0094\3\2\2\2\27\u009a\3\2\2"+
		"\2\31\u00a2\3\2\2\2\33\u00ab\3\2\2\2\35\u00b2\3\2\2\2\37\u00b9\3\2\2\2"+
		"!\u00c2\3\2\2\2#\u00c9\3\2\2\2%\u00cb\3\2\2\2\'\u00cd\3\2\2\2)\u00cf\3"+
		"\2\2\2+\u00d1\3\2\2\2-\u00d3\3\2\2\2/\u00d5\3\2\2\2\61\u00d7\3\2\2\2\63"+
		"\u00d9\3\2\2\2\65\u00db\3\2\2\2\67\u00dd\3\2\2\29\u00df\3\2\2\2;\u00e1"+
		"\3\2\2\2=\u00e3\3\2\2\2?\u00e5\3\2\2\2A\u00e8\3\2\2\2C\u00eb\3\2\2\2E"+
		"\u00f4\3\2\2\2G\u00f6\3\2\2\2I\u00ff\3\2\2\2K\u0103\3\2\2\2M\u0106\3\2"+
		"\2\2O\u010d\3\2\2\2Q\u0122\3\2\2\2S\u0124\3\2\2\2U\u012d\3\2\2\2W\u013b"+
		"\3\2\2\2Y\u013d\3\2\2\2[_\5U+\2\\_\5S*\2]_\t\2\2\2^[\3\2\2\2^\\\3\2\2"+
		"\2^]\3\2\2\2_`\3\2\2\2`^\3\2\2\2`a\3\2\2\2ab\3\2\2\2bc\b\2\2\2c\4\3\2"+
		"\2\2de\7o\2\2ef\7q\2\2fg\7f\2\2gh\7w\2\2hi\7n\2\2ij\7g\2\2j\6\3\2\2\2"+
		"kl\7e\2\2lm\7n\2\2mn\7c\2\2no\7u\2\2op\7u\2\2p\b\3\2\2\2qr\7u\2\2rs\7"+
		"v\2\2st\7t\2\2tu\7w\2\2uv\7e\2\2vw\7v\2\2w\n\3\2\2\2xy\7t\2\2yz\7g\2\2"+
		"z{\7v\2\2{|\7w\2\2|}\7t\2\2}~\7p\2\2~\f\3\2\2\2\177\u0080\7p\2\2\u0080"+
		"\u0081\7g\2\2\u0081\u0082\7y\2\2\u0082\16\3\2\2\2\u0083\u0084\7p\2\2\u0084"+
		"\u0085\7c\2\2\u0085\u0086\7v\2\2\u0086\u0087\7k\2\2\u0087\u0088\7x\2\2"+
		"\u0088\u0089\7g\2\2\u0089\20\3\2\2\2\u008a\u008b\7p\2\2\u008b\u008c\7"+
		"w\2\2\u008c\u008d\7n\2\2\u008d\u008e\7n\2\2\u008e\22\3\2\2\2\u008f\u0090"+
		"\7v\2\2\u0090\u0091\7t\2\2\u0091\u0092\7w\2\2\u0092\u0093\7g\2\2\u0093"+
		"\24\3\2\2\2\u0094\u0095\7h\2\2\u0095\u0096\7c\2\2\u0096\u0097\7n\2\2\u0097"+
		"\u0098\7u\2\2\u0098\u0099\7g\2\2\u0099\26\3\2\2\2\u009a\u009b\7r\2\2\u009b"+
		"\u009c\7t\2\2\u009c\u009d\7k\2\2\u009d\u009e\7x\2\2\u009e\u009f\7c\2\2"+
		"\u009f\u00a0\7v\2\2\u00a0\u00a1\7g\2\2\u00a1\30\3\2\2\2\u00a2\u00a3\7"+
		"k\2\2\u00a3\u00a4\7p\2\2\u00a4\u00a5\7v\2\2\u00a5\u00a6\7g\2\2\u00a6\u00a7"+
		"\7t\2\2\u00a7\u00a8\7p\2\2\u00a8\u00a9\7c\2\2\u00a9\u00aa\7n\2\2\u00aa"+
		"\32\3\2\2\2\u00ab\u00ac\7r\2\2\u00ac\u00ad\7w\2\2\u00ad\u00ae\7d\2\2\u00ae"+
		"\u00af\7n\2\2\u00af\u00b0\7k\2\2\u00b0\u00b1\7e\2\2\u00b1\34\3\2\2\2\u00b2"+
		"\u00b3\7u\2\2\u00b3\u00b4\7v\2\2\u00b4\u00b5\7c\2\2\u00b5\u00b6\7v\2\2"+
		"\u00b6\u00b7\7k\2\2\u00b7\u00b8\7e\2\2\u00b8\36\3\2\2\2\u00b9\u00ba\7"+
		"t\2\2\u00ba\u00bb\7g\2\2\u00bb\u00bc\7c\2\2\u00bc\u00bd\7f\2\2\u00bd\u00be"+
		"\7q\2\2\u00be\u00bf\7p\2\2\u00bf\u00c0\7n\2\2\u00c0\u00c1\7{\2\2\u00c1"+
		" \3\2\2\2\u00c2\u00c3\7u\2\2\u00c3\u00c4\7g\2\2\u00c4\u00c5\7c\2\2\u00c5"+
		"\u00c6\7n\2\2\u00c6\u00c7\7g\2\2\u00c7\u00c8\7f\2\2\u00c8\"\3\2\2\2\u00c9"+
		"\u00ca\7B\2\2\u00ca$\3\2\2\2\u00cb\u00cc\7\60\2\2\u00cc&\3\2\2\2\u00cd"+
		"\u00ce\7.\2\2\u00ce(\3\2\2\2\u00cf\u00d0\7<\2\2\u00d0*\3\2\2\2\u00d1\u00d2"+
		"\7=\2\2\u00d2,\3\2\2\2\u00d3\u00d4\7}\2\2\u00d4.\3\2\2\2\u00d5\u00d6\7"+
		"\177\2\2\u00d6\60\3\2\2\2\u00d7\u00d8\7*\2\2\u00d8\62\3\2\2\2\u00d9\u00da"+
		"\7+\2\2\u00da\64\3\2\2\2\u00db\u00dc\7]\2\2\u00dc\66\3\2\2\2\u00dd\u00de"+
		"\7_\2\2\u00de8\3\2\2\2\u00df\u00e0\7?\2\2\u00e0:\3\2\2\2\u00e1\u00e2\7"+
		")\2\2\u00e2<\3\2\2\2\u00e3\u00e4\7$\2\2\u00e4>\3\2\2\2\u00e5\u00e6\7<"+
		"\2\2\u00e6\u00e7\7<\2\2\u00e7@\3\2\2\2\u00e8\u00e9\7?\2\2\u00e9\u00ea"+
		"\7@\2\2\u00eaB\3\2\2\2\u00eb\u00ef\t\3\2\2\u00ec\u00ee\t\4\2\2\u00ed\u00ec"+
		"\3\2\2\2\u00ee\u00f1\3\2\2\2\u00ef\u00ed\3\2\2\2\u00ef\u00f0\3\2\2\2\u00f0"+
		"D\3\2\2\2\u00f1\u00ef\3\2\2\2\u00f2\u00f5\5M\'\2\u00f3\u00f5\5O(\2\u00f4"+
		"\u00f2\3\2\2\2\u00f4\u00f3\3\2\2\2\u00f5F\3\2\2\2\u00f6\u00fa\5=\37\2"+
		"\u00f7\u00f9\5W,\2\u00f8\u00f7\3\2\2\2\u00f9\u00fc\3\2\2\2\u00fa\u00f8"+
		"\3\2\2\2\u00fa\u00fb\3\2\2\2\u00fb\u00fd\3\2\2\2\u00fc\u00fa\3\2\2\2\u00fd"+
		"\u00fe\5=\37\2\u00feH\3\2\2\2\u00ff\u0100\5;\36\2\u0100\u0101\5W,\2\u0101"+
		"\u0102\5;\36\2\u0102J\3\2\2\2\u0103\u0104\5Q)\2\u0104L\3\2\2\2\u0105\u0107"+
		"\t\5\2\2\u0106\u0105\3\2\2\2\u0106\u0107\3\2\2\2\u0107\u0109\3\2\2\2\u0108"+
		"\u010a\t\6\2\2\u0109\u0108\3\2\2\2\u010a\u010b\3\2\2\2\u010b\u0109\3\2"+
		"\2\2\u010b\u010c\3\2\2\2\u010cN\3\2\2\2\u010d\u010e\7\62\2\2\u010e\u010f"+
		"\7z\2\2\u010f\u0110\3\2\2\2\u0110\u0111\5M\'\2\u0111P\3\2\2\2\u0112\u0113"+
		"\5M\'\2\u0113\u0115\7\60\2\2\u0114\u0116\t\6\2\2\u0115\u0114\3\2\2\2\u0116"+
		"\u0117\3\2\2\2\u0117\u0115\3\2\2\2\u0117\u0118\3\2\2\2\u0118\u0123\3\2"+
		"\2\2\u0119\u011b\t\5\2\2\u011a\u0119\3\2\2\2\u011a\u011b\3\2\2\2\u011b"+
		"\u011c\3\2\2\2\u011c\u011e\7\60\2\2\u011d\u011f\t\6\2\2\u011e\u011d\3"+
		"\2\2\2\u011f\u0120\3\2\2\2\u0120\u011e\3\2\2\2\u0120\u0121\3\2\2\2\u0121"+
		"\u0123\3\2\2\2\u0122\u0112\3\2\2\2\u0122\u011a\3\2\2\2\u0123R\3\2\2\2"+
		"\u0124\u0125\7\61\2\2\u0125\u0126\7\61\2\2\u0126\u012a\3\2\2\2\u0127\u0129"+
		"\n\7\2\2\u0128\u0127\3\2\2\2\u0129\u012c\3\2\2\2\u012a\u0128\3\2\2\2\u012a"+
		"\u012b\3\2\2\2\u012bT\3\2\2\2\u012c\u012a\3\2\2\2\u012d\u012e\7\61\2\2"+
		"\u012e\u012f\7,\2\2\u012f\u0133\3\2\2\2\u0130\u0132\13\2\2\2\u0131\u0130"+
		"\3\2\2\2\u0132\u0135\3\2\2\2\u0133\u0134\3\2\2\2\u0133\u0131\3\2\2\2\u0134"+
		"\u0136\3\2\2\2\u0135\u0133\3\2\2\2\u0136\u0137\7,\2\2\u0137\u0138\7\61"+
		"\2\2\u0138V\3\2\2\2\u0139\u013c\n\b\2\2\u013a\u013c\5Y-\2\u013b\u0139"+
		"\3\2\2\2\u013b\u013a\3\2\2\2\u013cX\3\2\2\2\u013d\u013e\7^\2\2\u013e\u013f"+
		"\t\t\2\2\u013fZ\3\2\2\2\21\2^`\u00ef\u00f4\u00fa\u0106\u010b\u0117\u011a"+
		"\u0120\u0122\u012a\u0133\u013b\3\2\3\2";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}