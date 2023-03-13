namespace Compiler
{
    public class ExceptionCompiler : Exception
    {
        private string _message;
        public ExceptionCompiler(string message)
        {
            _message = message;
        }
        public override string ToString()
        {
            return _message;
        }
    }
}
