using Antlr4.Runtime;

namespace AntlrTest
{
    public class RapidSharp_Antlr_Test : RapidBnf_Antlr_ParseTestBase
    {
        public string Text_RapidSharp_1000_Methods { get; set; }

        protected override void SetupTest()
        {
            base.SetupTest();

            TestFilesPath = "testfiles\\RapidSharp";

            Text_RapidSharp_1000_Methods = ReadTestFile("1000_methods.txt");
        }

        protected override void Benchmark()
        {
            Parse(Text_RapidSharp_1000_Methods);
        }

        protected override void Test()
        {
            ParseRepeated(Text_RapidSharp_1000_Methods, 100);
            ParseUntilUserInput(Text_RapidSharp_1000_Methods);
        }

        protected override Lexer CreateLexer(AntlrInputStream inputStream)
        {
            return new RSHARPLexer(inputStream);
        }

        protected override Parser CreateParser(CommonTokenStream tokenStream)
        {
            return new RSHARPParser(tokenStream);
        }

        protected override object Parse(Parser parser)
        {
            return ((RSHARPParser) parser).document();
        }
    }
}