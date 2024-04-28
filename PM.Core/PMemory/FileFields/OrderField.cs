
using System.Collections.Concurrent;

namespace PM.Core.PMemory.FileFields
{
    public class OrderField : UInt16Filed
    {
        private static readonly ConcurrentDictionary<int, UInt16> _orderByInstance = new();

        public int Instance { get; }

        /// <summary>
        /// Sequential order by a instance
        /// </summary>
        /// <param name="offset">Field offset</param>
        /// <param name="instance">Instance of a determinated order</param>
        /// <param name="setValue">set value igoring instance. If the value is greather than index max value then next value fo index is updated.</param>
        public OrderField(int offset, int instance = 0, UInt16? setValue = null)
        {
            this.Offset = offset;
            this.Instance = instance;

            if (!_orderByInstance.ContainsKey(instance))
            {
                _orderByInstance[instance] = 0;
            }

            if (setValue.HasValue)
            {
                Value = setValue.Value;
                
                if (setValue.Value > _orderByInstance[instance])
                {
                    _orderByInstance[instance] = setValue.Value;
                }
            }
            else
            {
                _orderByInstance[instance]++;
                Value = _orderByInstance[instance];
            }
        }
    }
}
