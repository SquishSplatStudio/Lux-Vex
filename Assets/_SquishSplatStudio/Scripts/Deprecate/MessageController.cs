using UnityEngine;
using UnityEngine.UI;

namespace SquishSplatStudio
{
    public class MessageController : MonoBehaviour
    {
        Text message;
        // Start is called before the first frame update
        void Start()
        {
            message = this.GetComponent<Text>();
            message.enabled = false;
        }

        public void SetMessage(GameObject gameObject)
        {
            message.text = "You picked up an item!!";
            message.enabled = true;
            Invoke("TurnOff", 2);
        }

        void TurnOff()
        {
            message.enabled = false;
        }
    }

}