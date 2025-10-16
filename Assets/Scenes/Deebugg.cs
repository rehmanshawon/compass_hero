using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Deebugg : MonoBehaviour
{
   public GameObject UI;
   public Button ExitButton;

   private void Start()
   {
      ExitButton.onClick.AddListener(ResetGame);
   }

   public void Update()
   {
      if (Input.GetKeyDown(KeyCode.Keypad5))
      {
         UI.SetActive(true);
      }
   }

   public void ResetGame()
   {
      SceneManager.LoadScene(0);
   }
}
