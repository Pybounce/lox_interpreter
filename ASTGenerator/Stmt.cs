public abstract class Stmt
{
public abstract R Accept<R>(Visitor<R> visitor);
public interface Visitor<R>
{
public R VisitExpressionStmt(Expression stmt);
public R VisitPrintStmt(Print stmt);
public R VisitVarStmt(Var stmt);
}
public class Expression: Stmt
{
public readonly Expr expression;
public override R Accept<R>(Visitor<R> visitor)
{
return visitor.VisitExpressionStmt(this);
}
public Expression(Expr expression): base()
{
this.expression = expression;
}
}
public class Print: Stmt
{
public readonly Expr expression;
public override R Accept<R>(Visitor<R> visitor)
{
return visitor.VisitPrintStmt(this);
}
public Print(Expr expression): base()
{
this.expression = expression;
}
}
public class Var: Stmt
{
public readonly Token name;
public readonly Expr initialiser;
public override R Accept<R>(Visitor<R> visitor)
{
return visitor.VisitVarStmt(this);
}
public Var(Token name, Expr initialiser): base()
{
this.name = name;
this.initialiser = initialiser;
}
}
}
