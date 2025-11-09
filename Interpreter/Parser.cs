
using System.Linq.Expressions;
using Microsoft.VisualBasic;

public class Parser
{
    private class ParseError: Exception {}

    private readonly List<Token> _tokens;
    private int _current = 0;

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    public List<Stmt> Parse()
    {
        var statements = new List<Stmt>();
        while (!IsAtEnd())
        {
            statements.Add(Declaration());
        }
        return statements;
    }

    private Stmt? Declaration()
    {
        try
        {
            if (MatchAny(TokenType.VAR)) { return VarDeclaration(); }
            return Statement();
        }
        catch (ParseError)
        {
            Synchronise();
            return null;
        }
    }

    private Stmt VarDeclaration()
    {
        var name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

        Expr initialiser = null;
        if (MatchAny(TokenType.EQUAL)) { initialiser = Expression(); }
        Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");

        return new Stmt.Var(name, initialiser);
    }
    
    private void Synchronise()
    {
        Advance();

        while (!IsAtEnd())
        {
            if (Previous().TokenType == TokenType.SEMICOLON) { return; }
            switch (Peek().TokenType)
            {
                case TokenType.CLASS:
                case TokenType.FUN:
                case TokenType.VAR:
                case TokenType.FOR:
                case TokenType.IF:
                case TokenType.WHILE:
                case TokenType.PRINT:
                case TokenType.RETURN:
                    return;
            }
            Advance();
        }
    }

    private Stmt Statement()
    {
        if (MatchAny(TokenType.PRINT)) { return PrintStatement(); }
        if (MatchAny(TokenType.LEFT_BRACE)) { return new Stmt.Block(Block()); }
        return ExpressionStatement();
    }

    private List<Stmt> Block()
    {
        var statements = new List<Stmt>();

        while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
        {
            statements.Add(Declaration());
        }

        Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
        return statements;
    }

    private Stmt PrintStatement()
    {
        var value = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' after value.");
        return new Stmt.Print(value);
    }
    
    private Stmt ExpressionStatement()
    {
        var expr = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
        return new Stmt.Expression(expr);
    }

    private Expr Expression()
    {
        return Assignment();
    }

    private Expr Assignment()
    {
        var expr = Equality();

        if (MatchAny(TokenType.EQUAL))
        {
            var equals = Previous();
            var value = Assignment();

            if (expr is Expr.Variable)
            {
                var name = ((Expr.Variable)expr).name;
                return new Expr.Assign(name, value);
            }
            Error(equals, "Invalid assignment target.");
        }
        return expr;
    }

    private Expr Equality()
    {
        var expr = Comparison();

        while (MatchAny(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
        {
            var op = Previous();
            var right = Comparison();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Comparison()
    {
        var expr = Term();

        while (MatchAny(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
        {
            var op = Previous();
            var right = Term();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Term()
    {
        var expr = Factor();

        while (MatchAny(TokenType.MINUS, TokenType.PLUS))
        {
            var op = Previous();
            var right = Factor();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Factor()
    {
        var expr = Unary();

        while (MatchAny(TokenType.SLASH, TokenType.STAR))
        {
            var op = Previous();
            var right = Unary();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Unary()
    {
        if (MatchAny(TokenType.BANG, TokenType.MINUS))
        {
            var op = Previous();
            var right = Unary();
            return new Expr.Unary(op, right);
        }

        return Primary();
    }
    
    private Expr Primary()
    {
        if (MatchAny(TokenType.FALSE)) { return new Expr.Literal(false); }
        if (MatchAny(TokenType.TRUE)) { return new Expr.Literal(true); }
        if (MatchAny(TokenType.NIL)) { return new Expr.Literal(null); }
        if (MatchAny(TokenType.NUMBER, TokenType.STRING)) { return new Expr.Literal(Previous().Literal); }
        if (MatchAny(TokenType.LEFT_PAREN))
        {
            var expr = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
            return new Expr.Grouping(expr);
        }
        if (MatchAny(TokenType.IDENTIFIER)) { return new Expr.Variable(Previous()); }

        throw Error(Peek(), "Expect expression.");
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) { return Advance(); }
        throw Error(Peek(), message);
    }

    private ParseError Error(Token token, string message)
    {
        Lox.Error(token, message);
        return new ParseError();
    }

    private bool MatchAny(params TokenType[] types)
    {
        foreach (var tokenType in types)
        {
            if (Check(tokenType))
            {
                Advance();
                return true;
            }
        }
        return false;
    }

    private Token Previous()
    {
        return _tokens[_current - 1];
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) { return false; }
        return Peek().TokenType == type;
    }

    private bool IsAtEnd()
    {
        return Peek().TokenType == TokenType.EOF;
    }

    private Token Peek()
    {
        return _tokens[_current];
    }
    
    private Token Advance()
    {
        if (!IsAtEnd()) { _current++; }
        return Previous();
    }

}