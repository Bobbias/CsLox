﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsLox
{
    /// <summary>
    /// Represents an instance of a class in the Lox interpreter.
    /// </summary>
    public class LoxInstance
    {
        /// <summary>
        /// The class definition this instance is based on.
        /// </summary>
        private LoxClass @class;

        /// <summary>
        /// Contains all properties of the class.
        /// </summary>
        private readonly Dictionary<string, object> fields = new Dictionary<string, object>();

        /// <summary>
        /// Creates a runtime instance of the given <see cref="LoxClass"/>.
        /// </summary>
        /// <param name="class"></param>
        public LoxInstance(LoxClass @class)
        {
            this.@class = @class;
        }

        /// <summary>
        /// Gets a property of the class.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>An <see langword="object"/> or <see langword="null"/>.</returns>
        /// <exception cref="CsLoxRuntimeException">If there is no defined value.</exception>
        public object? Get(Token name)
        {
            if (fields.TryGetValue(name.Lexeme, out object? value)) return value;

            throw new CsLoxRuntimeException(name, $"Undefined property '{name.Lexeme}'.");
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{@class.Name} instance";
        }
    }
}