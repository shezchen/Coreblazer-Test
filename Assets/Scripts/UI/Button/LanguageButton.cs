using Architecture.Language;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace Architecture
{
    public class LanguageButton : MonoBehaviour,ISelectHandler
    {
        [SerializeField] private GameLanguageType languageType;
        
        [Inject] private EventBus _eventBus;
        
        public void OnSelect(BaseEventData eventData)
        {
            _eventBus.Publish<LanguageChangeEvent>(new LanguageChangeEvent(languageType));
        }
    }
}