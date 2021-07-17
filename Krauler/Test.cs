using System;

namespace Krauler
{
    /// <summary>
    /// Test marker attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class Test : Attribute
    {
        /// <summary>
        /// Allow parallel execution of this test?
        /// </summary>
        public bool AllowParallelRun { get; }

        /// <summary>
        /// Override the name in the test report.
        /// </summary>
        public string? OverrideName { get; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="allowParallel"></param>
        /// <param name="overrideName"></param>
        public Test(bool allowParallel = false, string? overrideName = null)
        {
            this.AllowParallelRun = allowParallel;
            this.OverrideName = overrideName;
        }
    }
}
