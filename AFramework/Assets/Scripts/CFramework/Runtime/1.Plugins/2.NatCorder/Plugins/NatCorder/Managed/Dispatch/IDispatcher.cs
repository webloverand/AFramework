/* 
*   NatCorder
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatCorder.Dispatch {

    using System;

    public interface IDispatcher : IDisposable {
        
        /// <summary>
        /// Dispatch a workload
        /// </summary>
        void Dispatch (Action action);
    }
}