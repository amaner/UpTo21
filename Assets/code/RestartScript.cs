using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RestartScript : MonoBehaviour {

	public Button RestartBtn;
	public Text MessageBox;
	private string lastGameResult;

	// Use this for initialization
	void Start () {
		lastGameResult = PlayerPrefs.GetString ("LastGame");
		if (lastGameResult == "win") {
			MessageBox.text = "You Win!!!!";
		} else if (lastGameResult == "loss") {
			MessageBox.text = "You Lose!!!";
		} else {
			MessageBox.text = "Hi there!!!";
		}
		RestartBtn.onClick.AddListener (RestartGame);
	}

	void RestartGame() {
		SceneManager.LoadScene (0);
	}

}
