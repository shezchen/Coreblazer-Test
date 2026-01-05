using UnityEngine;

namespace Architecture
{
    public class UIRoot : MonoBehaviour
    {
        public void ClearRoot()
        {
            foreach (Transform t in transform)
            {
                Destroy(t.gameObject);
            }
        }
    }
}