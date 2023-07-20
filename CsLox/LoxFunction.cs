using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsLox
{
    public class LoxFunction : ILoxCallable
    {
        /// <summary>
        /// The declaration for the function.
        /// </summary>
        private readonly Stmt.Function declaration;

        /// <summary>
        /// The number of arguments the function takes.
        /// </summary>
        public int Arity { get ; private set; }

        /// <summary>
        /// Provides a closure containing the function's environment.
        /// </summary>
        private readonly LoxEnvironment closure;

        private readonly bool isInitializer;

        /// <summary>
        /// Constructs a Function object.
        /// </summary>
        /// <param name="decl">The declaration for the function.</param>
        public LoxFunction(Stmt.Function decl, LoxEnvironment closure, bool isInitializer)
        {
            declaration = decl;
            this.closure = closure;
            this.isInitializer = isInitializer;
        }

        /// <summary>
        /// Binds an instance to the method.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>A <see cref="LoxFunction"/> where <see langword="this"/> is bound to the given instance.</returns>
        public LoxFunction Bind(LoxInstance instance)
        {
            var env = new LoxEnvironment(closure);
            env.Define("this", instance);

            return new LoxFunction(declaration, env, isInitializer);
        }

        /// <summary>
        /// Calls the function with the given arguments.
        /// </summary>
        /// <param name="interpreter">The interpreter instance to execute the function call with.</param>
        /// <param name="args">The arguments with which to call the function.</param>
        /// <returns>Either an <see langword="object"/> containing the return value of the function call, or <see langword="null"/> if none.</returns>
        public object Call(Interpreter interpreter, List<object> args)
        {
            var environment = new LoxEnvironment(closure);

            for(int i = 0; i < args.Count; i++)
            {
                environment.Define(declaration.Parameters[i].Lexeme, args[i]);
            }
            
            try
            {
                interpreter.ExecuteBlock(declaration.Body, environment);
            }
            catch (Return retValue)
            {
                if (isInitializer) return closure.GetAt(0, "this");
                return retValue.Value;
            }

            if (isInitializer) return closure.GetAt(0, "this");

            return null;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"<fn {declaration.Name.Lexeme}>";
        }
    }
}
