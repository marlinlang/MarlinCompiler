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
		SINGLE_LINE_COMMENT=1, BLOCK_COMMENT=2, MODULE=3, CLASS=4, STRUCT=5, RETURN=6, 
		NEW=7, NATIVE=8, NULL=9, TRUE=10, FALSE=11, PRIVATE=12, INTERNAL=13, PUBLIC=14, 
		STATIC=15, READONLY=16, SEALED=17, DOT=18, COMMA=19, COLON=20, SEMICOLON=21, 
		LBRACE=22, RBRACE=23, LPAREN=24, RPAREN=25, LBRACKET=26, RBRACKET=27, 
		ASSIGN=28, DOUBLE_QUOTE=29, DOUBLE_COLON=30, ARROW=31, IDENTIFIER=32, 
		INTEGER=33, NORMAL_STRING=34, DOUBLE=35, WHITESPACES=36;
	public static final int
		COMMENTS_CHANNEL=2;
	public static String[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN", "COMMENTS_CHANNEL"
	};

	public static String[] modeNames = {
		"DEFAULT_MODE"
	};

	private static String[] makeRuleNames() {
		return new String[] {
			"SINGLE_LINE_COMMENT", "BLOCK_COMMENT", "MODULE", "CLASS", "STRUCT", 
			"RETURN", "NEW", "NATIVE", "NULL", "TRUE", "FALSE", "PRIVATE", "INTERNAL", 
			"PUBLIC", "STATIC", "READONLY", "SEALED", "DOT", "COMMA", "COLON", "SEMICOLON", 
			"LBRACE", "RBRACE", "LPAREN", "RPAREN", "LBRACKET", "RBRACKET", "ASSIGN", 
			"DOUBLE_QUOTE", "DOUBLE_COLON", "ARROW", "IDENTIFIER", "INTEGER", "NORMAL_STRING", 
			"DOUBLE", "WholeNumber", "HexNumber", "DoubleNumber", "WHITESPACES"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, null, null, "'module'", "'class'", "'struct'", "'return'", "'new'", 
			"'native'", "'null'", "'true'", "'false'", "'private'", "'internal'", 
			"'public'", "'static'", "'readonly'", "'sealed'", "'.'", "','", "':'", 
			"';'", "'{'", "'}'", "'('", "')'", "'['", "']'", "'='", "'\"'", "'::'", 
			"'=>'"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, "SINGLE_LINE_COMMENT", "BLOCK_COMMENT", "MODULE", "CLASS", "STRUCT", 
			"RETURN", "NEW", "NATIVE", "NULL", "TRUE", "FALSE", "PRIVATE", "INTERNAL", 
			"PUBLIC", "STATIC", "READONLY", "SEALED", "DOT", "COMMA", "COLON", "SEMICOLON", 
			"LBRACE", "RBRACE", "LPAREN", "RPAREN", "LBRACKET", "RBRACKET", "ASSIGN", 
			"DOUBLE_QUOTE", "DOUBLE_COLON", "ARROW", "IDENTIFIER", "INTEGER", "NORMAL_STRING", 
			"DOUBLE", "WHITESPACES"
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
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\2&\u0129\b\1\4\2\t"+
		"\2\4\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\4\n\t\n\4\13"+
		"\t\13\4\f\t\f\4\r\t\r\4\16\t\16\4\17\t\17\4\20\t\20\4\21\t\21\4\22\t\22"+
		"\4\23\t\23\4\24\t\24\4\25\t\25\4\26\t\26\4\27\t\27\4\30\t\30\4\31\t\31"+
		"\4\32\t\32\4\33\t\33\4\34\t\34\4\35\t\35\4\36\t\36\4\37\t\37\4 \t \4!"+
		"\t!\4\"\t\"\4#\t#\4$\t$\4%\t%\4&\t&\4\'\t\'\4(\t(\3\2\3\2\3\2\3\2\7\2"+
		"V\n\2\f\2\16\2Y\13\2\3\2\3\2\3\3\3\3\3\3\3\3\7\3a\n\3\f\3\16\3d\13\3\3"+
		"\3\3\3\3\3\3\3\3\3\3\4\3\4\3\4\3\4\3\4\3\4\3\4\3\5\3\5\3\5\3\5\3\5\3\5"+
		"\3\6\3\6\3\6\3\6\3\6\3\6\3\6\3\7\3\7\3\7\3\7\3\7\3\7\3\7\3\b\3\b\3\b\3"+
		"\b\3\t\3\t\3\t\3\t\3\t\3\t\3\t\3\n\3\n\3\n\3\n\3\n\3\13\3\13\3\13\3\13"+
		"\3\13\3\f\3\f\3\f\3\f\3\f\3\f\3\r\3\r\3\r\3\r\3\r\3\r\3\r\3\r\3\16\3\16"+
		"\3\16\3\16\3\16\3\16\3\16\3\16\3\16\3\17\3\17\3\17\3\17\3\17\3\17\3\17"+
		"\3\20\3\20\3\20\3\20\3\20\3\20\3\20\3\21\3\21\3\21\3\21\3\21\3\21\3\21"+
		"\3\21\3\21\3\22\3\22\3\22\3\22\3\22\3\22\3\22\3\23\3\23\3\24\3\24\3\25"+
		"\3\25\3\26\3\26\3\27\3\27\3\30\3\30\3\31\3\31\3\32\3\32\3\33\3\33\3\34"+
		"\3\34\3\35\3\35\3\36\3\36\3\37\3\37\3\37\3 \3 \3 \3!\3!\7!\u00f0\n!\f"+
		"!\16!\u00f3\13!\3\"\3\"\5\"\u00f7\n\"\3#\3#\7#\u00fb\n#\f#\16#\u00fe\13"+
		"#\3#\3#\3$\3$\3%\5%\u0105\n%\3%\6%\u0108\n%\r%\16%\u0109\3&\3&\3&\3&\3"+
		"&\3\'\3\'\3\'\6\'\u0114\n\'\r\'\16\'\u0115\3\'\5\'\u0119\n\'\3\'\3\'\6"+
		"\'\u011d\n\'\r\'\16\'\u011e\5\'\u0121\n\'\3(\6(\u0124\n(\r(\16(\u0125"+
		"\3(\3(\4Wb\2)\3\3\5\4\7\5\t\6\13\7\r\b\17\t\21\n\23\13\25\f\27\r\31\16"+
		"\33\17\35\20\37\21!\22#\23%\24\'\25)\26+\27-\30/\31\61\32\63\33\65\34"+
		"\67\359\36;\37= ?!A\"C#E$G%I\2K\2M\2O&\3\2\b\5\2C\\aac|\6\2\62;C\\aac"+
		"|\3\2$$\4\2--//\3\2\62;\5\2\13\f\17\17\"\"\2\u0131\2\3\3\2\2\2\2\5\3\2"+
		"\2\2\2\7\3\2\2\2\2\t\3\2\2\2\2\13\3\2\2\2\2\r\3\2\2\2\2\17\3\2\2\2\2\21"+
		"\3\2\2\2\2\23\3\2\2\2\2\25\3\2\2\2\2\27\3\2\2\2\2\31\3\2\2\2\2\33\3\2"+
		"\2\2\2\35\3\2\2\2\2\37\3\2\2\2\2!\3\2\2\2\2#\3\2\2\2\2%\3\2\2\2\2\'\3"+
		"\2\2\2\2)\3\2\2\2\2+\3\2\2\2\2-\3\2\2\2\2/\3\2\2\2\2\61\3\2\2\2\2\63\3"+
		"\2\2\2\2\65\3\2\2\2\2\67\3\2\2\2\29\3\2\2\2\2;\3\2\2\2\2=\3\2\2\2\2?\3"+
		"\2\2\2\2A\3\2\2\2\2C\3\2\2\2\2E\3\2\2\2\2G\3\2\2\2\2O\3\2\2\2\3Q\3\2\2"+
		"\2\5\\\3\2\2\2\7j\3\2\2\2\tq\3\2\2\2\13w\3\2\2\2\r~\3\2\2\2\17\u0085\3"+
		"\2\2\2\21\u0089\3\2\2\2\23\u0090\3\2\2\2\25\u0095\3\2\2\2\27\u009a\3\2"+
		"\2\2\31\u00a0\3\2\2\2\33\u00a8\3\2\2\2\35\u00b1\3\2\2\2\37\u00b8\3\2\2"+
		"\2!\u00bf\3\2\2\2#\u00c8\3\2\2\2%\u00cf\3\2\2\2\'\u00d1\3\2\2\2)\u00d3"+
		"\3\2\2\2+\u00d5\3\2\2\2-\u00d7\3\2\2\2/\u00d9\3\2\2\2\61\u00db\3\2\2\2"+
		"\63\u00dd\3\2\2\2\65\u00df\3\2\2\2\67\u00e1\3\2\2\29\u00e3\3\2\2\2;\u00e5"+
		"\3\2\2\2=\u00e7\3\2\2\2?\u00ea\3\2\2\2A\u00ed\3\2\2\2C\u00f6\3\2\2\2E"+
		"\u00f8\3\2\2\2G\u0101\3\2\2\2I\u0104\3\2\2\2K\u010b\3\2\2\2M\u0120\3\2"+
		"\2\2O\u0123\3\2\2\2QR\7\61\2\2RS\7\61\2\2SW\3\2\2\2TV\13\2\2\2UT\3\2\2"+
		"\2VY\3\2\2\2WX\3\2\2\2WU\3\2\2\2XZ\3\2\2\2YW\3\2\2\2Z[\b\2\2\2[\4\3\2"+
		"\2\2\\]\7\61\2\2]^\7,\2\2^b\3\2\2\2_a\13\2\2\2`_\3\2\2\2ad\3\2\2\2bc\3"+
		"\2\2\2b`\3\2\2\2ce\3\2\2\2db\3\2\2\2ef\7,\2\2fg\7\61\2\2gh\3\2\2\2hi\b"+
		"\3\2\2i\6\3\2\2\2jk\7o\2\2kl\7q\2\2lm\7f\2\2mn\7w\2\2no\7n\2\2op\7g\2"+
		"\2p\b\3\2\2\2qr\7e\2\2rs\7n\2\2st\7c\2\2tu\7u\2\2uv\7u\2\2v\n\3\2\2\2"+
		"wx\7u\2\2xy\7v\2\2yz\7t\2\2z{\7w\2\2{|\7e\2\2|}\7v\2\2}\f\3\2\2\2~\177"+
		"\7t\2\2\177\u0080\7g\2\2\u0080\u0081\7v\2\2\u0081\u0082\7w\2\2\u0082\u0083"+
		"\7t\2\2\u0083\u0084\7p\2\2\u0084\16\3\2\2\2\u0085\u0086\7p\2\2\u0086\u0087"+
		"\7g\2\2\u0087\u0088\7y\2\2\u0088\20\3\2\2\2\u0089\u008a\7p\2\2\u008a\u008b"+
		"\7c\2\2\u008b\u008c\7v\2\2\u008c\u008d\7k\2\2\u008d\u008e\7x\2\2\u008e"+
		"\u008f\7g\2\2\u008f\22\3\2\2\2\u0090\u0091\7p\2\2\u0091\u0092\7w\2\2\u0092"+
		"\u0093\7n\2\2\u0093\u0094\7n\2\2\u0094\24\3\2\2\2\u0095\u0096\7v\2\2\u0096"+
		"\u0097\7t\2\2\u0097\u0098\7w\2\2\u0098\u0099\7g\2\2\u0099\26\3\2\2\2\u009a"+
		"\u009b\7h\2\2\u009b\u009c\7c\2\2\u009c\u009d\7n\2\2\u009d\u009e\7u\2\2"+
		"\u009e\u009f\7g\2\2\u009f\30\3\2\2\2\u00a0\u00a1\7r\2\2\u00a1\u00a2\7"+
		"t\2\2\u00a2\u00a3\7k\2\2\u00a3\u00a4\7x\2\2\u00a4\u00a5\7c\2\2\u00a5\u00a6"+
		"\7v\2\2\u00a6\u00a7\7g\2\2\u00a7\32\3\2\2\2\u00a8\u00a9\7k\2\2\u00a9\u00aa"+
		"\7p\2\2\u00aa\u00ab\7v\2\2\u00ab\u00ac\7g\2\2\u00ac\u00ad\7t\2\2\u00ad"+
		"\u00ae\7p\2\2\u00ae\u00af\7c\2\2\u00af\u00b0\7n\2\2\u00b0\34\3\2\2\2\u00b1"+
		"\u00b2\7r\2\2\u00b2\u00b3\7w\2\2\u00b3\u00b4\7d\2\2\u00b4\u00b5\7n\2\2"+
		"\u00b5\u00b6\7k\2\2\u00b6\u00b7\7e\2\2\u00b7\36\3\2\2\2\u00b8\u00b9\7"+
		"u\2\2\u00b9\u00ba\7v\2\2\u00ba\u00bb\7c\2\2\u00bb\u00bc\7v\2\2\u00bc\u00bd"+
		"\7k\2\2\u00bd\u00be\7e\2\2\u00be \3\2\2\2\u00bf\u00c0\7t\2\2\u00c0\u00c1"+
		"\7g\2\2\u00c1\u00c2\7c\2\2\u00c2\u00c3\7f\2\2\u00c3\u00c4\7q\2\2\u00c4"+
		"\u00c5\7p\2\2\u00c5\u00c6\7n\2\2\u00c6\u00c7\7{\2\2\u00c7\"\3\2\2\2\u00c8"+
		"\u00c9\7u\2\2\u00c9\u00ca\7g\2\2\u00ca\u00cb\7c\2\2\u00cb\u00cc\7n\2\2"+
		"\u00cc\u00cd\7g\2\2\u00cd\u00ce\7f\2\2\u00ce$\3\2\2\2\u00cf\u00d0\7\60"+
		"\2\2\u00d0&\3\2\2\2\u00d1\u00d2\7.\2\2\u00d2(\3\2\2\2\u00d3\u00d4\7<\2"+
		"\2\u00d4*\3\2\2\2\u00d5\u00d6\7=\2\2\u00d6,\3\2\2\2\u00d7\u00d8\7}\2\2"+
		"\u00d8.\3\2\2\2\u00d9\u00da\7\177\2\2\u00da\60\3\2\2\2\u00db\u00dc\7*"+
		"\2\2\u00dc\62\3\2\2\2\u00dd\u00de\7+\2\2\u00de\64\3\2\2\2\u00df\u00e0"+
		"\7]\2\2\u00e0\66\3\2\2\2\u00e1\u00e2\7_\2\2\u00e28\3\2\2\2\u00e3\u00e4"+
		"\7?\2\2\u00e4:\3\2\2\2\u00e5\u00e6\7$\2\2\u00e6<\3\2\2\2\u00e7\u00e8\7"+
		"<\2\2\u00e8\u00e9\7<\2\2\u00e9>\3\2\2\2\u00ea\u00eb\7?\2\2\u00eb\u00ec"+
		"\7@\2\2\u00ec@\3\2\2\2\u00ed\u00f1\t\2\2\2\u00ee\u00f0\t\3\2\2\u00ef\u00ee"+
		"\3\2\2\2\u00f0\u00f3\3\2\2\2\u00f1\u00ef\3\2\2\2\u00f1\u00f2\3\2\2\2\u00f2"+
		"B\3\2\2\2\u00f3\u00f1\3\2\2\2\u00f4\u00f7\5I%\2\u00f5\u00f7\5K&\2\u00f6"+
		"\u00f4\3\2\2\2\u00f6\u00f5\3\2\2\2\u00f7D\3\2\2\2\u00f8\u00fc\5;\36\2"+
		"\u00f9\u00fb\n\4\2\2\u00fa\u00f9\3\2\2\2\u00fb\u00fe\3\2\2\2\u00fc\u00fa"+
		"\3\2\2\2\u00fc\u00fd\3\2\2\2\u00fd\u00ff\3\2\2\2\u00fe\u00fc\3\2\2\2\u00ff"+
		"\u0100\5;\36\2\u0100F\3\2\2\2\u0101\u0102\5M\'\2\u0102H\3\2\2\2\u0103"+
		"\u0105\t\5\2\2\u0104\u0103\3\2\2\2\u0104\u0105\3\2\2\2\u0105\u0107\3\2"+
		"\2\2\u0106\u0108\t\6\2\2\u0107\u0106\3\2\2\2\u0108\u0109\3\2\2\2\u0109"+
		"\u0107\3\2\2\2\u0109\u010a\3\2\2\2\u010aJ\3\2\2\2\u010b\u010c\7\62\2\2"+
		"\u010c\u010d\7z\2\2\u010d\u010e\3\2\2\2\u010e\u010f\5I%\2\u010fL\3\2\2"+
		"\2\u0110\u0111\5I%\2\u0111\u0113\7\60\2\2\u0112\u0114\t\6\2\2\u0113\u0112"+
		"\3\2\2\2\u0114\u0115\3\2\2\2\u0115\u0113\3\2\2\2\u0115\u0116\3\2\2\2\u0116"+
		"\u0121\3\2\2\2\u0117\u0119\t\5\2\2\u0118\u0117\3\2\2\2\u0118\u0119\3\2"+
		"\2\2\u0119\u011a\3\2\2\2\u011a\u011c\7\60\2\2\u011b\u011d\t\6\2\2\u011c"+
		"\u011b\3\2\2\2\u011d\u011e\3\2\2\2\u011e\u011c\3\2\2\2\u011e\u011f\3\2"+
		"\2\2\u011f\u0121\3\2\2\2\u0120\u0110\3\2\2\2\u0120\u0118\3\2\2\2\u0121"+
		"N\3\2\2\2\u0122\u0124\t\7\2\2\u0123\u0122\3\2\2\2\u0124\u0125\3\2\2\2"+
		"\u0125\u0123\3\2\2\2\u0125\u0126\3\2\2\2\u0126\u0127\3\2\2\2\u0127\u0128"+
		"\b(\3\2\u0128P\3\2\2\2\17\2Wb\u00f1\u00f6\u00fc\u0104\u0109\u0115\u0118"+
		"\u011e\u0120\u0125\4\2\4\2\2\3\2";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}