using Sirenix.OdinInspector;
using UnityEngine;

namespace Phoder1.Debugging
{
    [HideMonoScript]
    public class EventDebugLog : DebugBehaviour
    {
        public void DebugLog(string message) => Debug.Log(message);
        //public void Watch(object item)
        //    => Watch("Watched item:", item.ToString());
    }
}
