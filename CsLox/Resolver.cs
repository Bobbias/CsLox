using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsLox
{
    /// <summary>
    /// Handles variable resolution.
    /// </summary>
    public class Resolver : Expr.IVisitor<object?>, Stmt.IVisitor<object?>
    {
        /// <summary>
        /// Indicates what kind of function context we're in when handling resolution.
        /// </summary>
        private enum FunctionType
        {
            NONE,
            FUNCTION,
            METHOD
        }

        /// <summary>
        /// Indicates what kind of class context we're in when handling resolution. Aids
        /// in detecting misuses of the <see langword="this"/> keyword, among other issues.
        /// </summary>
        private enum ClassType
        {
            NONE,
            CLASS
        }

        /// <summary>
        /// 
        /// </summary>
        private readonly Interpreter interpreter;

        /// <summary>
        /// 
        /// </summary>
        private readonly Stack<Dictionary<string, object>> scopes = new Stack<Dictionary<string, object>>();

        /// <summary>
        /// Tracks whether we're inside a function or not. This aids us in catching cases where a return statement
        /// is encountered outside of a function.
        /// </summary>
        private FunctionType currentFunction = FunctionType.NONE;

        /// <summary>
        /// Tracks whether we're in a class or not. This aids us in catching cases where a
        /// <see langword="this"/> keyword is encountered in an imporoper context.
        /// </summary>
        private ClassType currentClass = ClassType.NONE;

        /// <summary>
        /// The resolver performs variable resolution as a separate pass after parsing but before interpreting.
        /// </summary>
        /// <param name="interpreter"></param>
        public Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        /// <summary>
        /// Calls <see cref="Resolve(Stmt)"/> on each element of <paramref name="stmts"/>.
        /// </summary>
        /// <param name="stmts">A list of statements to resolve.</param>
        public void Resolve(List<Stmt> stmts)
        {
            foreach(Stmt stmt in stmts)
            {
                Resolve(stmt);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stmt"></param>
        public void Resolve(Stmt stmt)
        {
            stmt.Accept(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expr"></param>
        public void Resolve(Expr expr)
        {
            expr.Accept(this);
        }

        /// <summary>
        /// Pushes an empty scope onto <see cref="scopes"/>.
        /// </summary>
        private void BeginScope()
        {
            scopes.Push(new Dictionary<string, object>());
        }

        /// <summary>
        /// Pops the current scope off <see cref="scopes"/>.
        /// </summary>
        private void EndScope()
        {
            scopes.Pop();
        }

        /// <summary>
        /// Adds an entry to the current scope indicating that a variable name has been declared.
        /// we bind it's name to the value <see langword="false"/> to indicate it's uninitialized.
        /// </summary>
        /// <param name="name"></param>
        private void Declare(Token name)
        {
            if(scopes.Count == 0)
            {
                return;
            }
            try
            {
                scopes.Peek().Add(name.Lexeme, false);
            }
            catch(ArgumentException e)
            {
                CLI.Error(name, "Already a variable with this name in this scope.");
            }
        }

        /// <summary>
        /// Adds or updates an entry to the current scope indicating that a variable name has been
        /// initialized. We bind it's name to <see langword="true"/> to indicate it's initialized.
        /// </summary>
        /// <param name="name"></param>
        private void Define(Token name)
        {
            if (scopes.Count == 0)
            {
                return;
            }

            scopes.Peek()[name.Lexeme] = true;
        }

        /// <summary>
        /// Resolves a local variable.
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="name"></param>
        private void ResolveLocal(Expr expr, Token name)
        {
            for (int i = scopes.Count - 1; i >= 0; --i)
            {
                if (scopes.ElementAt(i).ContainsKey(name.Lexeme))
                {
                    interpreter.Resolve(expr, scopes.Count - 1 - i);
                }
            }
        }

        /// <summary>
        /// Resolves a function.
        /// </summary>
        /// <param name="function"></param>
        private void ResolveFunction(Stmt.Function function, FunctionType type)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;

            BeginScope();
            foreach(var param in function.Parameters)
            {
                Declare(param);
                Define(param);
            }
            Resolve(function.Body);
            EndScope();

            currentFunction = enclosingFunction;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expr"></param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitAssignExpr(Expr.Assign expr)
        {
            Resolve(expr.Value);
            ResolveLocal(expr, expr.Name);

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expr"></param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitBinaryExpr(Expr.Binary expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stmt"></param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitBlockStmt(Stmt.Block stmt)
        {
            BeginScope();
            Resolve(stmt.Stmts);
            EndScope();

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stmt"></param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitClassStmt(Stmt.Class stmt)
        {
            var enclosingClass = currentClass;
            currentClass = ClassType.CLASS;

            Declare(stmt.Name);
            Define(stmt.Name);

            BeginScope();
            scopes.Peek().Add("this", true);

            foreach(var method in stmt.Methods)
            {
                var declaration = FunctionType.METHOD;
                ResolveFunction(method, declaration);
            }

            EndScope();

            currentClass = enclosingClass;
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expr"></param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitCallExpr(Expr.Call expr)
        {
            Resolve(expr.Callee);

            foreach(var arg in expr.Args)
            {
                Resolve(arg);
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expr"></param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitGetExpr(Expr.Get expr)
        {
            Resolve(expr.Obj);

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stmt"></param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitExpressionStmt(Stmt.Expression stmt)
        {
            Resolve(stmt.Expr);
            
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stmt"></param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitFunctionStmt(Stmt.Function stmt)
        {
            Declare(stmt.Name);
            Define(stmt.Name);

            ResolveFunction(stmt, FunctionType.FUNCTION);

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expr"></param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitGroupingExpr(Expr.Grouping expr)
        {
            Resolve(expr.Expression);

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stmt"></param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitIfStmt(Stmt.If stmt)
        {
            Resolve(stmt.Cond);
            Resolve(stmt.ThenBranch);
            if (stmt.ElseBranch != null)
            {
                Resolve(stmt.ElseBranch);
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expr"></param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitLiteralExpr(Expr.Literal expr)
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expr"></param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitLogicalExpr(Expr.Logical expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);

            return null;
        }

        /// <summary>
        /// Resolves a <see cref="Expr.Set"/> expression.
        /// </summary>
        /// <param name="expr">The expression to resolve.</param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitSetExpr(Expr.Set expr)
        {
            Resolve(expr.Value);
            Resolve(expr.Obj);

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expr"></param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitThisExpr(Expr.This expr)
        {
            if (currentClass == ClassType.NONE)
            {
                CLI.Error(expr.Keyword, "Can't use `this` outside of a class.");
                return null;
            }

            ResolveLocal(expr, expr.Keyword);

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stmt"></param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitPrintStmt(Stmt.Print stmt)
        {
            Resolve(stmt.Expr);

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stmt"></param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitReturnStmt(Stmt.Return stmt)
        {
            if(currentFunction == FunctionType.NONE)
            {
                CLI.Error(stmt.Keyword, "Can't return from top-level code.");
            }

            if(stmt.Value != null)
            {
                Resolve(stmt.Value);
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expr"></param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitUnaryExpr(Expr.Unary expr)
        {
            Resolve(expr.Right);

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public object? VisitVariableExpr(Expr.Variable expr)
        {
            if((scopes.Count != 0) && (bool)scopes.Peek()[expr.Name.Lexeme] == false)
            {
                CLI.Error(expr.Name, "Can't read local variable in its own initializer.");
            }

            ResolveLocal(expr, expr.Name);

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stmt"></param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitVarStmt(Stmt.Var stmt)
        {
            Declare(stmt.Name);
            if (stmt.Initializer != null)
            {
                Resolve(stmt.Initializer);
            }
            Define(stmt.Name);

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stmt"></param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitWhileStmt(Stmt.While stmt)
        {
            Resolve(stmt.Cond);
            Resolve(stmt.Body);
            
            return null;
        }
    }
}
