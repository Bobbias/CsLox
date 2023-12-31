﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsLox
{
    /// <summary>
    /// A LoxEnvironment represents a given scope while interpreting Lox source.
    /// </summary>
    public class LoxEnvironment
    {
        /// <summary>
        /// The enclosing environment if one exists, otherwise <see langword="null"/>.
        /// </summary>
        public LoxEnvironment? Enclosing { get; }

        /// <summary>
        /// A <see cref="Dictionary{TKey, TValue}"/> of <see langword="string"/> to <see langword="object"/> containing definitions in the environment.
        /// </summary>
        private Dictionary<string, object?> Values { get; } = new Dictionary<string, object?>();

        /// <summary>
        /// Constructs a top-level LoxEnvironment with a <see langword="null"/> <see cref="Enclosing"/>.
        /// </summary>
        public LoxEnvironment()
        {
            Enclosing = null;
        }

        /// <summary>
        /// Constructs a LoxEnvironment with a given <paramref name="enclosing"/> LoxEnvironment.
        /// </summary>
        /// <param name="enclosing"></param>
        public LoxEnvironment(LoxEnvironment enclosing)
        {
            Enclosing = enclosing;
        }

        /// <summary>
        /// Clears the dictionary.
        /// <para/>
        /// This is useful when running tests, as typically the root environment does not get cleared between calls to <see cref="CLI.RunFile(FileInfo)"/>.
        /// </summary>
        public void Clear()
        {
            Values.Clear();
        }

        /// <summary>
        /// Adds a variable definition with the provided name and value to the current environment.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The value assigned to the variable.</param>
        public void Define(string name, object? value)
        {
            Values.Add(name, value);
        }

        /// <summary>
        /// Retrieves a previously defined variable by name.
        /// </summary>
        /// <param name="name">The name of the variable in question.</param>
        /// <returns>An <see langword="object"/> containing the value of the variable.</returns>
        /// <exception cref="CsLoxRuntimeException">Throws an exception if the given variable is not defined on either this environment or an enclosing one.</exception>
        public object? Get(Token name)
        {
            if (Values.TryGetValue(name.Lexeme, out object? value)) return value;

            if (Enclosing != null)
            {
                return Enclosing.Get(name);
            }

            throw new CsLoxRuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
        }

        /// <summary>
        /// Retrieves a previously defined variable by <paramref name="name"/>, from the ancestor level indicated by <paramref name="distance"/>.
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public object? GetAt(int distance, string name)
        {
            return Ancestor(distance)!.Values[name];
        }

        /// <summary>
        /// Assigns <paramref name="value"/> to a variable by <paramref name="name"/> at an ancestor level specified by <paramref name="distance"/>.
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AssignAt(int distance, Token name, object? value)
        {
            Ancestor(distance)!.Values[name.Lexeme] = value;
        }

        /// <summary>
        /// Gets an ancestor of the current environment, traversing <paramref name="distance"/> levels up the heirarchy.
        /// </summary>
        /// <param name="distance"></param>
        /// <returns>A <see cref="LoxEnvironment"/> <paramref name="distance"/> levels up the heirarchy.</returns>
        public LoxEnvironment? Ancestor(int distance)
        {
            var environment = this;
            for (int i = 0; i < distance; ++i)
            {
                environment = environment!.Enclosing;
            }

            return environment;
        }

        /// <summary>
        /// Reassigns an existing variable to a new value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <exception cref="CsLoxRuntimeException">Throws an exception if the given variable is not defined on either this environment or an enclosing one.</exception>
        public void Assign(Token name, object? value)
        {
            if (Values.ContainsKey(name.Lexeme))
            {
                Values[name.Lexeme] = value;
                return;
            }

            if (Enclosing != null)
            {
                Enclosing.Assign(name, value);
                return;
            }

            throw new CsLoxRuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
        }
    }
}
