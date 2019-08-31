using IBApi;
using System.Collections.Generic;

namespace AmiBroker.DataSources.FT
{
    internal class ContractDetailsComparer : IComparer<ContractDetails>
    {
        internal ContractDetailsComparer()
        {
        }

        /// <summary>    
        /// Compares two DisplayContract based on member name and sort order    
        /// and return the result.   
        /// </summary>    
        /// <param name="cd1"></param>    
        /// <param name="cd2"></param>    
        /// <returns></returns>    
        public int Compare(ContractDetails cd1, ContractDetails cd2)
        {
            int result = cd1.Contract.LastTradeDateOrContractMonth.CompareTo(cd2.Contract.LastTradeDateOrContractMonth);

            return result;
        }
    }
}
