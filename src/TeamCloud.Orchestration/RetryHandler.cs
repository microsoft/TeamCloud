using System;

namespace TeamCloud.Orchestration
{
    /// <summary>
    /// Interface of a retry handler to determin if other retry attemp should be made.
    /// </summary>
    public interface IRetryHandler
    {
        /// <summary>
        /// Determine if the given exception leads to another retry attemp
        /// </summary>
        /// <param name="exception">The <see cref="System.Exception"/> to evaluate</param>
        /// <returns><c>true</c> if another attemp should be executed: otherwise <c>false</c></returns>
        bool Handle(Exception exception);
    }

    /// <summary>
    /// Default implementation of <see cref="IRetryHandler"/>
    /// </summary>
    public class DefaultRetryHandler : IRetryHandler
    {
        /// <summary>
        /// Determine if the given exception leads to another retry attemp
        /// </summary>
        /// <param name="exception">The <see cref="System.Exception"/> to evaluate</param>
        /// <returns><c>true</c> if another attemp should be executed: otherwise <c>false</c></returns>
        public virtual bool Handle(Exception exception) => true;
    }
}
