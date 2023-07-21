using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CsLox
{
    /// <summary>
    /// Handles an intermediate phase, after lexing and parsing, but before any code is evaluated by the interpreter.
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
            INITIALIZER,
            METHOD
        }

        /// <summary>
        /// Indicates what kind of class context we're in when handling resolution. Aids
        /// in detecting misuses of the <see langword="this"/> and <see langword="super"/>
        /// keywords, among other things.
        /// </summary>
        private enum ClassType
        {
            NONE,
            CLASS,
            SUBCLASS
        }

        /// <summary>
        /// The Lox interpreter. Resolution passes some information about variables to the interpreter during the resolution
        /// phase.
        /// </summary>
        private readonly Interpreter interpreter;

        /// <summary>
        /// The stack of variable scopes. At any point during the program, some number of scopes will be visible and populated.
        /// Because Lox is lexically scoped, scopes can be determined before evaluating any code. This is used to track which
        /// variables have been declared or defined.
        /// </summary>
        private readonly Stack<Dictionary<string, object>> scopes = new();

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
        /// Helper which manages multiple dispatch to call the correct visitor method for resolving Lox expressions.
        /// </summary>
        /// <param name="stmt">The statement to resolve.</param>
        public void Resolve(Stmt stmt)
        {
            stmt.Accept(this);
        }

        /// <summary>
        /// Helper which manages multiple dispatch to call the correct visitor method for resolving Lox expressions.
        /// </summary>
        /// <param name="expr">The expression to resolve.</param>
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
        /// <param name="name">The name of the variable to be marked as declared byt not defined.</param>
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
            catch(ArgumentException)
            {
                CLI.Error(name, "Already a variable with this name in this scope.");
            }
        }

        /// <summary>
        /// Adds or updates an entry to the current scope indicating that a variable name has been
        /// initialized. We bind it's name to <see langword="true"/> to indicate it's initialized.
        /// </summary>
        /// <param name="name">The name of the variable to be marked as defined.</param>
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
        /// <remarks>
        /// Checks the current scope first, iterating in reverse until the global scope is reached.
        /// </remarks>
        /// <param name="expr">The expression containing the local variable.</param>
        /// <param name="name">The name of the local variable.</param>
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
        /// Resolves <see cref="Stmt.Function"/>s, creating a new scope and adding all parameters to it, as well as tracking the function type.
        /// </summary>
        /// <param name="function">The function to resolve.</param>
        /// <param name="type">The type of the function to resolve.</param>
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
        /// Resolves <see cref="Expr.Assign"/> expressions.
        /// </summary>
        /// <param name="expr">The expression to resolve.</param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitAssignExpr(Expr.Assign expr)
        {
            Resolve(expr.Value);
            ResolveLocal(expr, expr.Name);

            return null;
        }

        /// <summary>
        /// Resolves <see cref="Expr.Binary"/> expressions by resolving the left hand expression and then the right hand expression.
        /// </summary>
        /// <param name="expr">The expression to resolve.</param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitBinaryExpr(Expr.Binary expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);

            return null;
        }

        /// <summary>
        /// Resolves <see cref="Stmt.Block"/> statements by creating a new scope and resolving the statements contained within.
        /// </summary>
        /// <param name="stmt">The statement to resolve.</param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitBlockStmt(Stmt.Block stmt)
        {
            BeginScope();
            Resolve(stmt.Stmts);
            EndScope();

            return null;
        }

        /// <summary>
        /// Resolves variables in <see cref="Stmt.Class"/> statements (class declarations).
        /// This includes implicitly defining <see langword="this"/> and <see langword="super"/>.
        /// Checks for errors such as self-inheritance.
        /// </summary>
        /// <param name="stmt">The statement to resolve.</param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitClassStmt(Stmt.Class stmt)
        {
            var enclosingClass = currentClass;
            currentClass = ClassType.CLASS;

            Declare(stmt.Name);
            Define(stmt.Name);

            if (stmt.Superclass != null &&
                stmt.Name.Lexeme.Equals(stmt.Superclass.Name.Lexeme))
            {
                CLI.Error(stmt.Superclass.Name, "A class can't inherit from itself.");
            }

            if(stmt.Superclass != null)
            {
                currentClass = ClassType.SUBCLASS;
                Resolve(stmt.Superclass);
            }

            if (stmt.Superclass != null)
            {
                BeginScope();

                scopes.Peek().Add("super", true);
            }

            BeginScope();
            scopes.Peek().Add("this", true);

            foreach(var method in stmt.Methods)
            {
                var declaration = FunctionType.METHOD;
                if (method.Name.Lexeme.Equals("init"))
                {
                    declaration = FunctionType.INITIALIZER;
                }

                ResolveFunction(method, declaration);
            }

            EndScope();

            if (stmt.Superclass != null)
            {
                EndScope();
            }

            currentClass = enclosingClass;
            return null;
        }

        /// <summary>
        /// Resolves <see cref="Expr.Call"/> expressions.
        /// </summary>
        /// <param name="expr">The expression to resolve.</param>
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
        /// Resolves <see cref="Expr.Get"/> expressions (that is, expressions which accesses a property of a class).
        /// </summary>
        /// <param name="expr">The expression to resolve.</param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitGetExpr(Expr.Get expr)
        {
            Resolve(expr.Obj);

            return null;
        }

        /// <summary>
        /// Resolves <see cref="Stmt.Expression"/> statements.
        /// </summary>
        /// <param name="stmt">The statement to be resolved.</param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitExpressionStmt(Stmt.Expression stmt)
        {
            Resolve(stmt.Expr);
            
            return null;
        }

        /// <summary>
        /// Resolves <see cref="Stmt.Function"/> statements.
        /// </summary>
        /// <param name="stmt">The statement to be resolved.</param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitFunctionStmt(Stmt.Function stmt)
        {
            Declare(stmt.Name);
            Define(stmt.Name);

            ResolveFunction(stmt, FunctionType.FUNCTION);

            return null;
        }

        /// <summary>
        /// Resolves <see cref="Expr.Grouping"/> expressions.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitGroupingExpr(Expr.Grouping expr)
        {
            Resolve(expr.Expression);

            return null;
        }

        /// <summary>
        /// Resolves <see cref="Stmt.If"/> statements. 
        /// </summary>
        /// <param name="stmt">The statement to resolve.</param>
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
        /// Resolves <see cref="Expr.Literal"/> expressions.
        /// </summary>
        /// <remarks>
        /// This function currently does nothing except return null.
        /// </remarks>
        /// <param name="expr"></param>
        /// <returns><see langword="null"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object? VisitLiteralExpr(Expr.Literal expr)
        {
            return null;
        }

        /// <summary>
        /// Resolves <see cref="Expr.Logical"/> expressions (<see langword="and"/> and <see langword="or"/>).
        /// </summary>
        /// <param name="expr">The expression to resolve.</param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitLogicalExpr(Expr.Logical expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);

            return null;
        }

        /// <summary>
        /// Resolves <see cref="Expr.Set"/> expressions.
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
        /// Resolves <see cref="Expr.Super"/> expressions. Checks whether a super expression is allowed in the current context.
        /// </summary>
        /// <remarks>
        /// If a super expression is ecncountered at the top level (ie. outside of a class), or in a class with no superclass,
        /// this will trigger an error.
        /// </remarks>
        /// <param name="expr">The expression to resolve.</param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitSuperExpr(Expr.Super expr)
        {
            if(currentClass == ClassType.NONE)
            {
                CLI.Error(expr.Keyword, "Can't use `super` outside of a class.");
            }
            else if (currentClass != ClassType.SUBCLASS)
            {
                CLI.Error(expr.Keyword, "Can't use `super` in a class with no superclass.");
            }

            ResolveLocal(expr, expr.Keyword);

            return null;
        }

        /// <summary>
        /// Resolves <see cref="Expr.This"/> expressions. Checks whether a this expression is allowed in the current context.
        /// </summary>
        /// <remarks>
        /// If this is encountered at the top level (ie. outside of a method context) this will trigger an error.
        /// </remarks>
        /// <param name="expr">the expression to resolve.</param>
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
        /// Resolves <see cref="Stmt.Print"/> statements.
        /// </summary>
        /// <param name="stmt">The statement to resolve.</param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitPrintStmt(Stmt.Print stmt)
        {
            Resolve(stmt.Expr);

            return null;
        }

        /// <summary>
        /// Resolves <see cref="Stmt.Return"/> statements. Checks whether a return statement is allowed in the current context.
        /// </summary>
        /// <remarks>
        /// If a return statement is encountered at the top level (ie. outside of a function context), this will trigger an error.
        /// If a return statement is provided a return value, and is encountered in an initializer (class constructor), this will trigger an error.
        /// </remarks>
        /// <param name="stmt">The statement to resolve.</param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitReturnStmt(Stmt.Return stmt)
        {
            if(currentFunction == FunctionType.NONE)
            {
                CLI.Error(stmt.Keyword, "Can't return from top-level code.");
            }

            if(stmt.Value != null)
            {
                if(currentFunction == FunctionType.INITIALIZER)
                {
                    CLI.Error(stmt.Keyword, "Can't return a value from an initializer.");
                }

                Resolve(stmt.Value);
            }

            return null;
        }

        /// <summary>
        /// Resolves <see cref="Expr.Unary"/> expressions.
        /// </summary>
        /// <param name="expr">The expression to resolve.</param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitUnaryExpr(Expr.Unary expr)
        {
            Resolve(expr.Right);

            return null;
        }

        /// <summary>
        /// Resolves <see cref="Expr.Variable"/> expressions, or references to variables.
        /// </summary>
        /// <param name="expr">the expression to resolve.</param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitVariableExpr(Expr.Variable expr)
        {
            if ((scopes.Count != 0) && !(bool)scopes.Peek()[expr.Name.Lexeme])
            {
                CLI.Error(expr.Name, "Can't read local variable in its own initializer.");
            }

            ResolveLocal(expr, expr.Name);

            return null;
        }

        /// <summary>
        /// Resolves <see cref="Stmt.Var"/> statements by declaring the variable, resolving the initializer expression, and marking the variable as defined after.
        /// </summary>
        /// <param name="stmt">The statement to resolve.</param>
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
        /// Resolves <see cref="Stmt.While"/> statements, resolving variables in the condition, and then the body.
        /// </summary>
        /// <param name="stmt">The statement to resolve.</param>
        /// <returns><see langword="null"/>.</returns>
        public object? VisitWhileStmt(Stmt.While stmt)
        {
            Resolve(stmt.Cond);
            Resolve(stmt.Body);
            
            return null;
        }
    }
}
