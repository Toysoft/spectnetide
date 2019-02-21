using Spect.Net.Assembler.SyntaxTree.Expressions;
// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo

namespace Spect.Net.Assembler.SyntaxTree.Pragmas
{
    /// <summary>
    /// This class represents the DISP pragma
    /// </summary>
    public sealed class DispPragma : PragmaBase
    {
        /// <summary>
        /// The DISP parameter
        /// </summary>
        public ExpressionNode Expr { get; set; }
    }
}