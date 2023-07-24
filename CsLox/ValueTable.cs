using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsLox.Compiler
{
    /// <summary>
    /// Represents a collection of values in the Lox virtual machine.
    /// </summary>
    public class ValueTable
    {
        /// <summary>
        /// Contains the data the value table wraps.
        /// </summary>
        public List<double> Values { get; set; }

        /// <summary>
        /// Initializes a new instance of the ValueTable class.
        /// </summary>
        public ValueTable()
        {
            Values = new List<double>();
        }
    }
}
