using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CsLox
{
    /// <summary>
    /// The interpreter for CsLox. 
    /// <para/>
    /// This class implements the <see href="https://en.wikipedia.org/wiki/Visitor_pattern">visitor pattern</see> for expressions and statements,
    /// determining how statements and expressions are evaluated.
    /// </summary>
    public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        /// <summary>
        /// Stores the global scope.
        /// </summary>
        internal readonly LoxEnvironment globals = new LoxEnvironment();

        /// <summary>
        /// The current scope the interpreter is operating in.
        /// </summary>
        /// <remarks>
        /// Env should be initialized to <see cref="Interpreter.globals"/> but since that would require globals to be static, it is initialized to
        /// an empty environment, then assigned to globals later when the interpreter is constructed.
        /// </remarks>
        private LoxEnvironment env = new LoxEnvironment();

        /// <summary>
        /// Contains resolution information separate from the AST node.
        /// </summary>
        private readonly Dictionary<Expr, int> locals = new Dictionary<Expr, int>();

        /// <summary>
        /// Constructs an interpreter.
        /// </summary>
        public Interpreter()
        {
            globals.Define("clock", new NativeFns.Clock());
            env = globals;
        }

        /// <summary>
        /// Clears all variables in the current <see cref="LoxEnvironment"/>.
        /// <para/>
        /// This is useful for testing. Since the interpreter is a static object, it only one instance lives for the lifetime of the application.
        /// This is problematic when running tests, since it causes state to leak from one test to subsequent tests. To avoid that, this function
        /// should be called as part of the SetUp before each test to ensure that any existing state is cleaned up between tests.
        /// </summary>
        public void ClearEnvironment()
        {
            env.Clear();
        }

        /// <summary>
        /// Evaluates a given <see cref="Expr.Binary"/>. The operands are evaluated left to right.
        /// <para/>
        /// When evaluating <see langword="+"/>, both operands must be either numbers, or strings. If this condition is not met, then an exception is thrown.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns>The result of evaluating the given <see cref="Expr.Binary"/> operation as an <see langword="object"/>.</returns>
        /// <exception cref="CsLoxRuntimeException">Throws an exception if the operands to the <see langword="+"/> operation are unacceptable.</exception>
        public object VisitBinaryExpr(Expr.Binary expr)
        {
            var left = Evaluate(expr.Left);
            var right = Evaluate(expr.Right);

            switch (expr.Oper.Type)
            {
                case Token.TokenType.GREATER:
                    CheckNumberOperands(expr.Oper, left, right);
                    return (double)left > (double)right;
                case Token.TokenType.GREATER_EQUAL:
                    CheckNumberOperands(expr.Oper, left, right);
                    return (double)left >= (double)right;
                case Token.TokenType.LESS:
                    CheckNumberOperands(expr.Oper, left, right);
                    return (double)left < (double)right;
                case Token.TokenType.LESS_EQUAL:
                    CheckNumberOperands(expr.Oper, left, right);
                    return (double)left <= (double)right;
                case Token.TokenType.BANG_EQUAL:
                    return !IsEqual(left, right);
                case Token.TokenType.EQUAL_EQUAL:
                    return IsEqual(left, right);
                case Token.TokenType.MINUS:
                    CheckNumberOperands(expr.Oper, left, right);
                    return (double)left - (double)right;
                case Token.TokenType.PLUS:
                    {
                        if (left is double && right is double)
                        {
                            return (double)left + (double)right;
                        }
                        if (left is string && right is string)
                        {
                            return (string)left + (string)right;
                        }

                        throw new CsLoxRuntimeException(expr.Oper, "Operands must be two numbers or two strings.");
                    }
                case Token.TokenType.SLASH:
                    CheckNumberOperands(expr.Oper, left, right);
                    return (double)left / (double)right;
                case Token.TokenType.STAR:
                    CheckNumberOperands(expr.Oper, left, right);
                    return (double)left * (double)right;
            }

            // Should be unreachable
            return null;
        }

        /// <summary>
        /// Evaluates a function call expression.
        /// </summary>
        /// <param name="expr">the expression being called.</param>
        /// <returns>An <see langword="object"/> representing the return value of the function call.</returns>
        /// <exception cref="CsLoxRuntimeException">Throws a runtime exception if the expression being called is neither a function nor a class.</exception>
        public object VisitCallExpr(Expr.Call expr)
        {
            var callee = Evaluate(expr.Callee);

            var args = new List<object>();
            foreach (var arg in expr.Args)
            {
                args.Add(Evaluate(arg));
            }

            if(callee is not ILoxCallable)
            {
                throw new CsLoxRuntimeException(expr.Paren, "Can only call functions and classes.");
            }

            ILoxCallable fun = (ILoxCallable)callee;

            if (args.Count != fun.Arity)
            {
                throw new CsLoxRuntimeException(expr.Paren, $"Expected {fun.Arity} arguments but got {args.Count}.");
            }

            return fun.Call(this, args);
        }

        public object VisitGetExpr(Expr.Get expr)
        {
            var obj = Evaluate(expr.Obj);
            if (obj is LoxInstance)
            {
                return ((LoxInstance)obj).Get(expr.Name);
            }

            throw new CsLoxRuntimeException(expr.Name, "Only instances have properties.");
        }

        /// <summary>
        /// Evaluates a grouping (parenthesized) expression <paramref name="expr"/>.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns>An <see langword="object"/> representing the result of evaluating <paramref name="expr"/>.</returns>
        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.Expression);
        }

        /// <summary>
        /// Evaluates the literal <paramref name="expr"/> and returns the result as an <see langword="object"/>.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns>An <see langword="object"/> representing value <paramref name="expr"/>.</returns>
        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.Value!;
        }

        /// <summary>
        /// Evaluates a logical expression. Short circuits, returning the left hand side of an <see langword="or"/> expression if it is truthy, and the right side if not.
        /// </summary>
        /// <param name="expr">The logical expression to evaluate.</param>
        /// <returns>An <see langword="object"/> with the required truthiness.</returns>
        public object VisitLogicalExpr(Expr.Logical expr)
        {
            object left = Evaluate(expr.Left);

            if (expr.Oper.Type == Token.TokenType.OR)
            {
                if (IsTruthy(left)) return left;
            }
            else
            {
                if (!IsTruthy(left)) return left;
            }

            return Evaluate(expr.Right);
        }

        /// <summary>
        /// Evaluates a set expression, where a property on an instance of a class is being set to a value.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        /// <exception cref="CsLoxRuntimeException"></exception>
        public object VisitSetExpr(Expr.Set expr)
        {
            var obj = Evaluate(expr.Obj);

            if(obj is not LoxInstance)
            {
                throw new CsLoxRuntimeException(expr.Name, "Only instances have fields.");
            }

            var value = Evaluate(expr.Value);
            ((LoxInstance)obj).Set(expr.Name, value);

            return value;
        }

        /// <summary>
        /// Evaluates the unary experssion <paramref name="expr"/> and returns the result as an <see langword="object"/>.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public object VisitUnaryExpr(Expr.Unary expr)
        {
            var right = Evaluate(expr.Right);

            switch (expr.Oper.Type)
            {
                case Token.TokenType.BANG:
                    return !IsTruthy(right);
                case Token.TokenType.MINUS:
                    CheckNumberOperand(expr.Oper, right);
                    return -(double)right;


            }

            // unreachable
            return null;
        }

        /// <summary>
        /// Evaluates the variable reference <paramref name="expr"/> and returns the result as an <see langword="object"/>.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public object VisitVariableExpr(Expr.Variable expr)
        {
            return LookUpVariable(expr.Name, expr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="expr"></param>
        /// <returns></returns>
        private object LookUpVariable(Token name, Expr expr)
        {
            int distance = locals.GetValueOrDefault(expr, -1);
            if (distance != -1)
            {
                return env.GetAt(distance, name.Lexeme);
            }
            else
            {
                return globals.Get(name);
            }
        }

        /// <summary>
        /// Evaluates the Lox source in <paramref name="statements"/>.
        /// </summary>
        /// <param name="statements">A list of Lox statements to be interpreted.</param>
        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (CsLoxRuntimeException e)
            {
                CLI.RuntimeError(e);
            }
        }

        /// <summary>
        /// Evaluates the expression <paramref name="expr"/>.
        /// </summary>
        /// <param name="expr">An expression to evaluate.</param>
        /// <returns>An <see langword="object"/> containing the result of evaluating expr.</returns>
        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        /// <summary>
        /// Executes the statement <paramref name="stmt"/>.
        /// </summary>
        /// <remarks>
        /// This function calls <see cref="Stmt.Accept{T}(Stmt.IVisitor{T})"/> but throws away the resulting object, because the resulting object is guaranteed to be null.
        /// </remarks>
        /// <param name="stmt">The statement to execute.</param>
        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        /// <summary>
        /// Stores resolution information for a given expression.
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="depth"></param>
        public void Resolve(Expr expr, int depth)
        {
            locals[expr] = depth;
        }

        /// <summary>
        /// Executes the statements inside <paramref name="stmt"/> in a new environment.
        /// </summary>
        /// <param name="stmt"></param>
        /// <returns></returns>
        public object VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.Stmts, new LoxEnvironment(env));

            return null;
        }

        /// <summary>
        /// Creates an instance of <see cref="LoxClass"/> and assigns it to <see cref="Stmt.Class.Name"/>.
        /// </summary>
        /// <param name="stmt"></param>
        /// <returns></returns>
        public object VisitClassStmt(Stmt.Class stmt)
        {
            env.Define(stmt.Name.Lexeme, null);

            var methods = new Dictionary<string, LoxFunction>();
            foreach (var method in stmt.Methods)
            {
                var function = new LoxFunction(method, env);
                methods[method.Name.Lexeme] = function;
            }

            LoxClass @class = new LoxClass(stmt.Name.Lexeme, methods);

            env.Assign(stmt.Name, @class);

            return null;
        }

        /// <summary>
        /// Executes a list of statements in a new environment as supplied in <paramref name="env"/>. Restores the previous
        /// environment after execution of the block is finished.
        /// </summary>
        /// <param name="stmts"></param>
        /// <param name="env"></param>
        internal void ExecuteBlock(List<Stmt> stmts, LoxEnvironment env)
        {
            var previous = this.env;
            try
            {
                this.env = env;

                foreach (var stmt in stmts)
                {
                    Execute(stmt);
                }
            }
            finally
            {
                this.env = previous;
            }
        }

        /// <summary>
        /// Determines whether a Lox object is truthy or falsey.
        /// </summary>
        /// <param name="obj">An object to test for truthines.</param>
        /// <returns><see langword="false"/> if obj is <see langword="null"/> or <see langword="false"/>, otherwise <see langword="true"/>.</returns>
        private bool IsTruthy(object obj)
        {
            if (obj == null)
                return false;
            if (obj is bool)
                return (bool)obj;

            return true;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// <para>
        /// Unlike the reference Java implementation, <see cref="Object.Equals"/> has the same semantics as Lox; two <see langword="null"/> objects are equal.
        /// </para>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns><see langword="true"/> if the specified object is equal to the current object, otherwise <see langword="false"/>.</returns>
        private bool IsEqual(object left, object right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether <paramref name="operand"/> is a number. Lox represents all numbers as <see langword="double"/>s.
        /// <para/>
        /// Throws an exception if <paramref name="operand"/> is not a <see langword="double"/>.
        /// <seealso cref="CheckNumberOperands(Token, object, object)"/>
        /// </summary>
        /// <param name="oper">The token, to be used in the case that an error is thrown.</param>
        /// <param name="operand">The operand to be checked.</param>
        /// <exception cref="CsLoxRuntimeException"></exception>
        private void CheckNumberOperand(Token oper, object operand)
        {
            if (operand is double)
                return;

            throw new CsLoxRuntimeException(oper, "Operand must be a number.");
        }

        /// <summary>
        /// Determines whether <paramref name="left"/> and <paramref name="right"/> are both numbers. Lox represents all numbers as <see langword="double"/>s.
        /// <para/>
        /// Throws an exception if either <paramref name="left"/> or <paramref name="right"/> are not <see langword="double"/>s.
        /// <seealso cref="CheckNumberOperand(Token, object)"/>
        /// </summary>
        /// <param name="oper">The token, to be used in the case that an error is thrown.</param>
        /// <param name="left">An operand to be checked.</param>
        /// <param name="right">An operand to be checked.</param>
        /// <exception cref="CsLoxRuntimeException"></exception>
        private void CheckNumberOperands(Token oper, object left, object right)
        {
            if (left is double && right is double)
                return;

            throw new CsLoxRuntimeException(oper, "Operands must be numbers.");
        }

        /// <summary>
        /// Handles string conversion for Lox objects.
        /// <para/>
        /// Handles number conversion specially, as any number that is an integer should be represented without a decimal point.
        /// All other numbers should be represented as floating point numbers.
        /// </summary>
        /// <param name="obj">The <see langword="object"/> to be converted.</param>
        /// <returns><paramref name="obj"/>.ToString() except in the case of numbers. Represents <see langword="null"/> as <see langword="nil"/>.</returns>
        private string Stringify(object obj)
        {
            if (obj == null) return "nil";

            if (obj is double)
                return string.Format("{0:G29}", (double)obj);

            return obj.ToString() ?? "nil";
        }

        /// <summary>
        /// Evaluates <paramref name="stmt"/> as a statement.
        /// <para/>
        /// All statements return <see langword="null"/> because there is no way to define a generic with a void return type.
        /// </summary>
        /// <param name="stmt"></param>
        /// <returns><see langword="null"/>.</returns>
        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.Expr);
            
            return null;
        }

        /// <summary>
        /// Evaluates <paramref name="stmt"/>, defining the given function in the current environment.
        /// </summary>
        /// <param name="stmt"></param>
        /// <returns><see langword="null"/>.</returns>
        public object VisitFunctionStmt(Stmt.Function stmt)
        {
            var function = new LoxFunction(stmt, env);
            env.Define(stmt.Name.Lexeme, function);
            
            return null;
        }

        /// <summary>
        /// Evaluates <paramref name="stmt"/>.
        /// <para/>
        /// All statements return <see langword="null"/> because there is no way to define a generic with a void return type.
        /// </summary>
        /// <param name="stmt"></param>
        /// <returns><see langword="null"/>.</returns>
        public object VisitIfStmt(Stmt.If stmt)
        {
            if (IsTruthy(Evaluate(stmt.Cond)))
                Execute(stmt.ThenBranch);
            else if (stmt.ElseBranch != null)
                Execute(stmt.ElseBranch);

            return null;
        }

        /// <summary>
        /// Evaluates <paramref name="stmt"/> and prints the result.
        /// </summary>
        /// <param name="stmt"></param>
        /// <returns><see langword="null"/>.</returns>
        public object VisitPrintStmt(Stmt.Print stmt)
        {
            var value = Evaluate(stmt.Expr);
            Console.WriteLine(Stringify(value));

            return null;
        }

        /// <summary>
        /// Evaluates a return statement. It does this by evaluating the expression supplied, if any, and then throwing a
        /// <see cref="Return"/> exception to escape the current call stack and bubble the return value up to the function
        /// call site.
        /// </summary>
        /// <param name="stmt"></param>
        /// <returns>Never.</returns>
        /// <exception cref="Return">Throws a custom exception.</exception>
        [DoesNotReturn]
        public object VisitReturnStmt(Stmt.Return stmt)
        {
            object value = null;
            if (stmt.Value != null)
            {
                value = Evaluate(stmt.Value);
            }

            throw new Return(value);
        }

        /// <summary>
        /// Evaluates <paramref name="stmt"/> and defines the variable named by it.
        /// </summary>
        /// <param name="stmt"></param>
        /// <returns><see langword="null"/>.</returns>
        public object VisitVarStmt(Stmt.Var stmt)
        {
            object value = null;
            if (stmt.Initializer != null)
            {
                value = Evaluate(stmt.Initializer);
            }

            env.Define(stmt.Name.Lexeme, value);

            return null;
        }

        /// <summary>
        /// Evaluates <paramref name="stmt"/>.
        /// </summary>
        /// <param name="stmt"></param>
        /// <returns><see langword="null"/>.</returns>
        public object VisitWhileStmt(Stmt.While stmt)
        {
            while (IsTruthy(Evaluate(stmt.Cond)))
            {
                Execute(stmt.Body);
            }

            return null;
        }

        /// <summary>
        /// Evaluates <paramref name="expr"/>.
        /// </summary>
        /// <param name="expr">The assignment expression to evaluate.</param>
        /// <returns>the value assigned to <see cref="Expr.Assign.Name"/>.</returns>
        public object VisitAssignExpr(Expr.Assign expr)
        {
            var value = Evaluate(expr.Value);

            int distance = locals[expr];

            if (distance != -1)
                env.AssignAt(distance, expr.Name, value);
            else
                globals.Assign(expr.Name, value);

            return value;
        }
    }
}
