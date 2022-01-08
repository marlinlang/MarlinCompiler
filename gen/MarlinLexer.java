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
		READONLY=15, SEALED=16, DOT=17, COMMA=18, COLON=19, SEMICOLON=20, LBRACE=21, 
		RBRACE=22, LPAREN=23, RPAREN=24, LBRACKET=25, RBRACKET=26, ASSIGN=27, 
		DOUBLE_QUOTE=28, DOUBLE_COLON=29, ARROW=30, IDENTIFIER=31, INTEGER=32, 
		NORMAL_STRING=33, DOUBLE=34;
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
			"SEALED", "DOT", "COMMA", "COLON", "SEMICOLON", "LBRACE", "RBRACE", "LPAREN", 
			"RPAREN", "LBRACKET", "RBRACKET", "ASSIGN", "DOUBLE_QUOTE", "DOUBLE_COLON", 
			"ARROW", "IDENTIFIER", "INTEGER", "NORMAL_STRING", "DOUBLE", "WholeNumber", 
			"HexNumber", "DoubleNumber", "SingleLineComment", "BlockComment"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, null, "'module'", "'class'", "'struct'", "'return'", "'new'", "'native'", 
			"'null'", "'true'", "'false'", "'private'", "'internal'", "'public'", 
			"'static'", "'readonly'", "'sealed'", "'.'", "','", "':'", "';'", "'{'", 
			"'}'", "'('", "')'", "'['", "']'", "'='", "'\"'", "'::'", "'=>'"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, "WHITESPACES", "MODULE", "CLASS", "STRUCT", "RETURN", "NEW", "NATIVE", 
			"NULL", "TRUE", "FALSE", "PRIVATE", "INTERNAL", "PUBLIC", "STATIC", "READONLY", 
			"SEALED", "DOT", "COMMA", "COLON", "SEMICOLON", "LBRACE", "RBRACE", "LPAREN", 
			"RPAREN", "LBRACKET", "RBRACKET", "ASSIGN", "DOUBLE_QUOTE", "DOUBLE_COLON", 
			"ARROW", "IDENTIFIER", "INTEGER", "NORMAL_STRING", "DOUBLE"
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
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\2$\u0127\b\1\4\2\t"+
		"\2\4\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\4\n\t\n\4\13"+
		"\t\13\4\f\t\f\4\r\t\r\4\16\t\16\4\17\t\17\4\20\t\20\4\21\t\21\4\22\t\22"+
		"\4\23\t\23\4\24\t\24\4\25\t\25\4\26\t\26\4\27\t\27\4\30\t\30\4\31\t\31"+
		"\4\32\t\32\4\33\t\33\4\34\t\34\4\35\t\35\4\36\t\36\4\37\t\37\4 \t \4!"+
		"\t!\4\"\t\"\4#\t#\4$\t$\4%\t%\4&\t&\4\'\t\'\4(\t(\3\2\3\2\3\2\6\2U\n\2"+
		"\r\2\16\2V\3\2\3\2\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\4\3\4\3\4\3\4\3\4\3\4"+
		"\3\5\3\5\3\5\3\5\3\5\3\5\3\5\3\6\3\6\3\6\3\6\3\6\3\6\3\6\3\7\3\7\3\7\3"+
		"\7\3\b\3\b\3\b\3\b\3\b\3\b\3\b\3\t\3\t\3\t\3\t\3\t\3\n\3\n\3\n\3\n\3\n"+
		"\3\13\3\13\3\13\3\13\3\13\3\13\3\f\3\f\3\f\3\f\3\f\3\f\3\f\3\f\3\r\3\r"+
		"\3\r\3\r\3\r\3\r\3\r\3\r\3\r\3\16\3\16\3\16\3\16\3\16\3\16\3\16\3\17\3"+
		"\17\3\17\3\17\3\17\3\17\3\17\3\20\3\20\3\20\3\20\3\20\3\20\3\20\3\20\3"+
		"\20\3\21\3\21\3\21\3\21\3\21\3\21\3\21\3\22\3\22\3\23\3\23\3\24\3\24\3"+
		"\25\3\25\3\26\3\26\3\27\3\27\3\30\3\30\3\31\3\31\3\32\3\32\3\33\3\33\3"+
		"\34\3\34\3\35\3\35\3\36\3\36\3\36\3\37\3\37\3\37\3 \3 \7 \u00e0\n \f "+
		"\16 \u00e3\13 \3!\3!\5!\u00e7\n!\3\"\3\"\7\"\u00eb\n\"\f\"\16\"\u00ee"+
		"\13\"\3\"\3\"\3#\3#\3$\5$\u00f5\n$\3$\6$\u00f8\n$\r$\16$\u00f9\3%\3%\3"+
		"%\3%\3%\3&\3&\3&\6&\u0104\n&\r&\16&\u0105\3&\5&\u0109\n&\3&\3&\6&\u010d"+
		"\n&\r&\16&\u010e\5&\u0111\n&\3\'\3\'\3\'\3\'\7\'\u0117\n\'\f\'\16\'\u011a"+
		"\13\'\3(\3(\3(\3(\7(\u0120\n(\f(\16(\u0123\13(\3(\3(\3(\3\u0121\2)\3\3"+
		"\5\4\7\5\t\6\13\7\r\b\17\t\21\n\23\13\25\f\27\r\31\16\33\17\35\20\37\21"+
		"!\22#\23%\24\'\25)\26+\27-\30/\31\61\32\63\33\65\34\67\359\36;\37= ?!"+
		"A\"C#E$G\2I\2K\2M\2O\2\3\2\t\5\2\13\f\17\17\"\"\5\2C\\aac|\6\2\62;C\\"+
		"aac|\3\2$$\4\2--//\3\2\62;\4\2\f\f\17\17\2\u012f\2\3\3\2\2\2\2\5\3\2\2"+
		"\2\2\7\3\2\2\2\2\t\3\2\2\2\2\13\3\2\2\2\2\r\3\2\2\2\2\17\3\2\2\2\2\21"+
		"\3\2\2\2\2\23\3\2\2\2\2\25\3\2\2\2\2\27\3\2\2\2\2\31\3\2\2\2\2\33\3\2"+
		"\2\2\2\35\3\2\2\2\2\37\3\2\2\2\2!\3\2\2\2\2#\3\2\2\2\2%\3\2\2\2\2\'\3"+
		"\2\2\2\2)\3\2\2\2\2+\3\2\2\2\2-\3\2\2\2\2/\3\2\2\2\2\61\3\2\2\2\2\63\3"+
		"\2\2\2\2\65\3\2\2\2\2\67\3\2\2\2\29\3\2\2\2\2;\3\2\2\2\2=\3\2\2\2\2?\3"+
		"\2\2\2\2A\3\2\2\2\2C\3\2\2\2\2E\3\2\2\2\3T\3\2\2\2\5Z\3\2\2\2\7a\3\2\2"+
		"\2\tg\3\2\2\2\13n\3\2\2\2\ru\3\2\2\2\17y\3\2\2\2\21\u0080\3\2\2\2\23\u0085"+
		"\3\2\2\2\25\u008a\3\2\2\2\27\u0090\3\2\2\2\31\u0098\3\2\2\2\33\u00a1\3"+
		"\2\2\2\35\u00a8\3\2\2\2\37\u00af\3\2\2\2!\u00b8\3\2\2\2#\u00bf\3\2\2\2"+
		"%\u00c1\3\2\2\2\'\u00c3\3\2\2\2)\u00c5\3\2\2\2+\u00c7\3\2\2\2-\u00c9\3"+
		"\2\2\2/\u00cb\3\2\2\2\61\u00cd\3\2\2\2\63\u00cf\3\2\2\2\65\u00d1\3\2\2"+
		"\2\67\u00d3\3\2\2\29\u00d5\3\2\2\2;\u00d7\3\2\2\2=\u00da\3\2\2\2?\u00dd"+
		"\3\2\2\2A\u00e6\3\2\2\2C\u00e8\3\2\2\2E\u00f1\3\2\2\2G\u00f4\3\2\2\2I"+
		"\u00fb\3\2\2\2K\u0110\3\2\2\2M\u0112\3\2\2\2O\u011b\3\2\2\2QU\5O(\2RU"+
		"\5M\'\2SU\t\2\2\2TQ\3\2\2\2TR\3\2\2\2TS\3\2\2\2UV\3\2\2\2VT\3\2\2\2VW"+
		"\3\2\2\2WX\3\2\2\2XY\b\2\2\2Y\4\3\2\2\2Z[\7o\2\2[\\\7q\2\2\\]\7f\2\2]"+
		"^\7w\2\2^_\7n\2\2_`\7g\2\2`\6\3\2\2\2ab\7e\2\2bc\7n\2\2cd\7c\2\2de\7u"+
		"\2\2ef\7u\2\2f\b\3\2\2\2gh\7u\2\2hi\7v\2\2ij\7t\2\2jk\7w\2\2kl\7e\2\2"+
		"lm\7v\2\2m\n\3\2\2\2no\7t\2\2op\7g\2\2pq\7v\2\2qr\7w\2\2rs\7t\2\2st\7"+
		"p\2\2t\f\3\2\2\2uv\7p\2\2vw\7g\2\2wx\7y\2\2x\16\3\2\2\2yz\7p\2\2z{\7c"+
		"\2\2{|\7v\2\2|}\7k\2\2}~\7x\2\2~\177\7g\2\2\177\20\3\2\2\2\u0080\u0081"+
		"\7p\2\2\u0081\u0082\7w\2\2\u0082\u0083\7n\2\2\u0083\u0084\7n\2\2\u0084"+
		"\22\3\2\2\2\u0085\u0086\7v\2\2\u0086\u0087\7t\2\2\u0087\u0088\7w\2\2\u0088"+
		"\u0089\7g\2\2\u0089\24\3\2\2\2\u008a\u008b\7h\2\2\u008b\u008c\7c\2\2\u008c"+
		"\u008d\7n\2\2\u008d\u008e\7u\2\2\u008e\u008f\7g\2\2\u008f\26\3\2\2\2\u0090"+
		"\u0091\7r\2\2\u0091\u0092\7t\2\2\u0092\u0093\7k\2\2\u0093\u0094\7x\2\2"+
		"\u0094\u0095\7c\2\2\u0095\u0096\7v\2\2\u0096\u0097\7g\2\2\u0097\30\3\2"+
		"\2\2\u0098\u0099\7k\2\2\u0099\u009a\7p\2\2\u009a\u009b\7v\2\2\u009b\u009c"+
		"\7g\2\2\u009c\u009d\7t\2\2\u009d\u009e\7p\2\2\u009e\u009f\7c\2\2\u009f"+
		"\u00a0\7n\2\2\u00a0\32\3\2\2\2\u00a1\u00a2\7r\2\2\u00a2\u00a3\7w\2\2\u00a3"+
		"\u00a4\7d\2\2\u00a4\u00a5\7n\2\2\u00a5\u00a6\7k\2\2\u00a6\u00a7\7e\2\2"+
		"\u00a7\34\3\2\2\2\u00a8\u00a9\7u\2\2\u00a9\u00aa\7v\2\2\u00aa\u00ab\7"+
		"c\2\2\u00ab\u00ac\7v\2\2\u00ac\u00ad\7k\2\2\u00ad\u00ae\7e\2\2\u00ae\36"+
		"\3\2\2\2\u00af\u00b0\7t\2\2\u00b0\u00b1\7g\2\2\u00b1\u00b2\7c\2\2\u00b2"+
		"\u00b3\7f\2\2\u00b3\u00b4\7q\2\2\u00b4\u00b5\7p\2\2\u00b5\u00b6\7n\2\2"+
		"\u00b6\u00b7\7{\2\2\u00b7 \3\2\2\2\u00b8\u00b9\7u\2\2\u00b9\u00ba\7g\2"+
		"\2\u00ba\u00bb\7c\2\2\u00bb\u00bc\7n\2\2\u00bc\u00bd\7g\2\2\u00bd\u00be"+
		"\7f\2\2\u00be\"\3\2\2\2\u00bf\u00c0\7\60\2\2\u00c0$\3\2\2\2\u00c1\u00c2"+
		"\7.\2\2\u00c2&\3\2\2\2\u00c3\u00c4\7<\2\2\u00c4(\3\2\2\2\u00c5\u00c6\7"+
		"=\2\2\u00c6*\3\2\2\2\u00c7\u00c8\7}\2\2\u00c8,\3\2\2\2\u00c9\u00ca\7\177"+
		"\2\2\u00ca.\3\2\2\2\u00cb\u00cc\7*\2\2\u00cc\60\3\2\2\2\u00cd\u00ce\7"+
		"+\2\2\u00ce\62\3\2\2\2\u00cf\u00d0\7]\2\2\u00d0\64\3\2\2\2\u00d1\u00d2"+
		"\7_\2\2\u00d2\66\3\2\2\2\u00d3\u00d4\7?\2\2\u00d48\3\2\2\2\u00d5\u00d6"+
		"\7$\2\2\u00d6:\3\2\2\2\u00d7\u00d8\7<\2\2\u00d8\u00d9\7<\2\2\u00d9<\3"+
		"\2\2\2\u00da\u00db\7?\2\2\u00db\u00dc\7@\2\2\u00dc>\3\2\2\2\u00dd\u00e1"+
		"\t\3\2\2\u00de\u00e0\t\4\2\2\u00df\u00de\3\2\2\2\u00e0\u00e3\3\2\2\2\u00e1"+
		"\u00df\3\2\2\2\u00e1\u00e2\3\2\2\2\u00e2@\3\2\2\2\u00e3\u00e1\3\2\2\2"+
		"\u00e4\u00e7\5G$\2\u00e5\u00e7\5I%\2\u00e6\u00e4\3\2\2\2\u00e6\u00e5\3"+
		"\2\2\2\u00e7B\3\2\2\2\u00e8\u00ec\59\35\2\u00e9\u00eb\n\5\2\2\u00ea\u00e9"+
		"\3\2\2\2\u00eb\u00ee\3\2\2\2\u00ec\u00ea\3\2\2\2\u00ec\u00ed\3\2\2\2\u00ed"+
		"\u00ef\3\2\2\2\u00ee\u00ec\3\2\2\2\u00ef\u00f0\59\35\2\u00f0D\3\2\2\2"+
		"\u00f1\u00f2\5K&\2\u00f2F\3\2\2\2\u00f3\u00f5\t\6\2\2\u00f4\u00f3\3\2"+
		"\2\2\u00f4\u00f5\3\2\2\2\u00f5\u00f7\3\2\2\2\u00f6\u00f8\t\7\2\2\u00f7"+
		"\u00f6\3\2\2\2\u00f8\u00f9\3\2\2\2\u00f9\u00f7\3\2\2\2\u00f9\u00fa\3\2"+
		"\2\2\u00faH\3\2\2\2\u00fb\u00fc\7\62\2\2\u00fc\u00fd\7z\2\2\u00fd\u00fe"+
		"\3\2\2\2\u00fe\u00ff\5G$\2\u00ffJ\3\2\2\2\u0100\u0101\5G$\2\u0101\u0103"+
		"\7\60\2\2\u0102\u0104\t\7\2\2\u0103\u0102\3\2\2\2\u0104\u0105\3\2\2\2"+
		"\u0105\u0103\3\2\2\2\u0105\u0106\3\2\2\2\u0106\u0111\3\2\2\2\u0107\u0109"+
		"\t\6\2\2\u0108\u0107\3\2\2\2\u0108\u0109\3\2\2\2\u0109\u010a\3\2\2\2\u010a"+
		"\u010c\7\60\2\2\u010b\u010d\t\7\2\2\u010c\u010b\3\2\2\2\u010d\u010e\3"+
		"\2\2\2\u010e\u010c\3\2\2\2\u010e\u010f\3\2\2\2\u010f\u0111\3\2\2\2\u0110"+
		"\u0100\3\2\2\2\u0110\u0108\3\2\2\2\u0111L\3\2\2\2\u0112\u0113\7\61\2\2"+
		"\u0113\u0114\7\61\2\2\u0114\u0118\3\2\2\2\u0115\u0117\n\b\2\2\u0116\u0115"+
		"\3\2\2\2\u0117\u011a\3\2\2\2\u0118\u0116\3\2\2\2\u0118\u0119\3\2\2\2\u0119"+
		"N\3\2\2\2\u011a\u0118\3\2\2\2\u011b\u011c\7\61\2\2\u011c\u011d\7,\2\2"+
		"\u011d\u0121\3\2\2\2\u011e\u0120\13\2\2\2\u011f\u011e\3\2\2\2\u0120\u0123"+
		"\3\2\2\2\u0121\u0122\3\2\2\2\u0121\u011f\3\2\2\2\u0122\u0124\3\2\2\2\u0123"+
		"\u0121\3\2\2\2\u0124\u0125\7,\2\2\u0125\u0126\7\61\2\2\u0126P\3\2\2\2"+
		"\20\2TV\u00e1\u00e6\u00ec\u00f4\u00f9\u0105\u0108\u010e\u0110\u0118\u0121"+
		"\3\2\3\2";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}