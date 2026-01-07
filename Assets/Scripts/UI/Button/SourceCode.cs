using UnityEngine;

namespace UI.Button
{
    public class SourceCode : MonoBehaviour
    {
        public void OnSourceCodeClicked()
        {
            Application.OpenURL("https://github.com/shezchen/Coreblazer-Test");
        }
    }
}