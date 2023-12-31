﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsLox
{
    /// <summary>
    /// Reresents a class in Lox.
    /// <para/>
    /// In Lox, classes are single inheritance. This means a class may only inherit from one parent.
    /// </summary>
    public class LoxClass: ILoxCallable
    {
        /// <summary>
        /// The name of the class.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// This class' superclass.
        /// </summary>
        public readonly LoxClass? superclass;

        /// <inheritdoc/>
        public int Arity {
            get
            {
                var initializer = FindMethod("init");
                
                if (initializer == null)
                    return 0;
                
                return initializer.Arity;
            }
        }

        /// <summary>
        /// Contains the methods for each instance of this class.
        /// </summary>
        private readonly Dictionary<string, LoxFunction> methods;

        /// <summary>
        /// Constructs a LoxClass with the given name.
        /// </summary>
        /// <param name="name">The name of the class.</param>
        /// <param name="methods">The methods of the class.</param>
        public LoxClass(string name, LoxClass? superclass, Dictionary<string, LoxFunction> methods)
        {
            Name = name;
            this.methods = methods;
            this.superclass = superclass;
        }

        /// <summary>
        /// Looks up a method by name, and returns it if present.
        /// </summary>
        /// <param name="name">The method name to search for.</param>
        /// <returns>A <see cref="LoxFunction"/> or <see langword="null"/>.</returns>
        public LoxFunction? FindMethod(string name)
        {
            if (methods.TryGetValue(name, out LoxFunction? method)) return method;

            if (superclass != null)
            {
                return superclass.FindMethod(name);
            }

            return null;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="interpreter">A copy of the interpreter, which may be needed in some cases.</param>
        /// <param name="args">Arguments to the call.</param>
        /// <returns>A new <see cref="LoxInstance"/> of this class.</returns>
        public object Call(Interpreter interpreter, List<object?> args)
        {
            var instance = new LoxInstance(this);
            var initializer = FindMethod("init");
            initializer?
                .Bind(instance)
                .Call(interpreter, args);

            return instance;
        }
    }
}
