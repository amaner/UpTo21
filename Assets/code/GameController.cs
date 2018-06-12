using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

	/* all coordinates are assuming use of background.png i provided
	 * and it is placed at 0,1,0 with scale 2,2,1 
	 * /

	/* coordinates of game blocks (for instantiation):
	 * 
	 * col 1: (-3.3f, 4.3f), (-3.3f, 2.1f), (-3.3f, -0.1f), (-3.3f, -2.3f)
	 * 
	 * col 2: (-1.1f, 4.3f), (-1.1f, 2.1f), (-1.1f, -0.1f), (-1.1f, -2.3f)
	 * 
	 * col 3: (1.1f, 4.3f), (1.1f, 2.1f), (1.1f, -0.1f), (-1.1f, -2.3f)
	 * 
	 * col 4: (3.3f, 4.3f), (3.3f, 2.1f), (3.3f, -0.1f), (3.3f, -2.3f)
	 * 
	 * coordinates of preview blocks:
	 * 
	 * first: (-6.5f, 0.4f), scale = (1f, 1f, 1f)
	 * 
	 * second: (-6.5f, -0.55f), scale = (0.5f, 0.5f, 0.5f)
	 * 
	 */

	/*  extents of game blocks (for detection)
	*
	* b1:  x -4.0 - -2.3, y 3.3 - 5.0
	* b2:  x -2.0 - -0.1, y 3.3 - 5.0
	* b3:  x  0.1 - 2.0,  y 3.3 - 5.0
	* b4:  x  2.2 - 3.9,  y 3.3 - 5.0
	* b5:  x -4.0 - -2.3, y 1.2 - 3.2
	* b6:  x -2.0 - -0.1, y 1.2 - 3.2
	* b7:  x  0.1 - 2.0,  y 1.2 - 3.2
	* b8:  x  2.2 - 3.9,  y 1.2 - 3.2
	* b9:  x -4.0 - -2.3, y -1.0 - 1.0
	* b10: x -2.0 - -0.1, y -1.0 - 1.0
	* b11: x  0.1 - 2.0,  y -1.0 - 1.0
	* b12: x  2.2 - 3.9,  y -1.0 - 1.0
	* b13: x -4.0 - -2.3, y -3.0 - -1.3
	* b14: x -2.0 - -0.1, y -3.0 - -1.3
	* b15: x  0.1 - 2.0,  y -3.0 - -1.3
	* b16: x  2.2 - 3.9,  y -3.0 - -1.3
	*
	*/

	/* Global Variables */

	// list of vectors of positions
	private Vector2[] blockCoords;
	// block prefabs
	public GameObject[] gameBlock;
	// currently active blocks
	// a spot containes the number of the block in it, -1 if it is empty
	private int[] blockContent;
	// array of possible adjacencies for each block
	private int[][] possAdj = new int[16][];
	// array of actual adjacencies for current block
	private int[] adjacencies = new int[4];

	// positions and scales of preview blocks
	private Vector2 largePreviewPos   = new Vector2 (-6.5f, 0.4f);
	private Vector3 largePreviewScale = new Vector3 (1f, 1f, 1f);
	private Vector2 smallPreviewPos   = new Vector2 (-6.5f, -0.55f);
	private Vector3 smallPreviewScale = new Vector3 (0.5f, 0.5f, 1f);

	// large game block scale
	private Vector3 largeGameBlockScale = new Vector3 (2f, 2f, 1f);
	// game play variables
	private int min; // for randomizer, was thinking about letting this move
	private int highestBlockNum; // highest current block number
	private float distance = 10f; // distance of camera from game board
	private int lgPrevNum, smPrevNum; // face number of large and small preview blocks
	private int blockNum, numAdj, score; // self-explanatory
	private int numClicks = 0; // number of legit clicks
	private int currentHighScore; // self-explanatory

	public Text ScoreBox; // text ui object to hold score
	public Text HighScoreBox; // text ui object to hold high score

	/* Utility Functions */

	// return a random game block number
	public int RandomBlockNum(int p, int q) {
		return Random.Range (p, q);
	}
	// create a game block with given prefab (go), position (pos), and scale
	// also will have tag locTag (used for finding and deleting object later)
	public void CreateBlock (GameObject go, Vector2 pos, Vector3 scale, string locTag) {
		go.transform.localScale = scale;
		go.tag = locTag;
		Instantiate (go, pos, Quaternion.identity);
	}
	// a set of bool functions to detect which row
	// and column a hit is in
	public bool isRow1 (Vector2 p) {
		if (p.y > 3.3 && p.y < 5.0) {
			return true;
		} else {
			return false;
		}
	}
	public bool isRow2 (Vector2 p) {
		if (p.y > 1.2 && p.y < 3.2) {
			return true;
		} else {
			return false;
		}
	}
	public bool isRow3 (Vector2 p) {
		if (p.y > -1.0 && p.y < 1.0) {
			return true;
		} else {
			return false;
		}
	}
	public bool isRow4 (Vector2 p) {
		if (p.y > -3.0 && p.y < -1.3) {
			return true;
		} else {
			return false;
		}
	}
	public bool isCol1 (Vector2 p) {
		if (p.x > -4.0 && p.x < -2.3) { // COL 1
			return true;
		} else {
			return false;
		}
	}
	public bool isCol2 (Vector2 p) {
		if (p.x > -2.0 && p.x < -0.1) {
			return true;
		} else {
			return false;
		}
	}
	public bool isCol3 (Vector3 p) {
		if (p.x > 0.1 && p.x < 2.0) {
			return true;
		} else {
			return false;
		}
	}
	public bool isCol4 (Vector2 p) {
		if (p.x > 2.2 && p.x < 3.9) {
			return true;
		} else {
			return false;
		}
	}
	// detect if click is within a game block and return the block number
	// return -1 if not within a game block
	public int BlockClicked (Vector2 p) {
		if (isRow1(p)) {
			if (isCol1 (p)) {
				return 0;
			} else if (isCol2 (p)) {
				return 1;
			} else if (isCol3 (p)) {
				return 2;
			} else if (isCol4 (p)) {
				return 3;
			} else {
				return -1;
			}
		} else if (isRow2(p)) {
			if (isCol1(p)) {
				return 4;
			} else if (isCol2(p)) {
				return 5;
			} else if (isCol3(p)) {
				return 6;
			} else if (isCol4(p)) {
				return 7;
			} else {
				return -1;
			}
		} else if (isRow3(p)) {
			if (isCol1(p)) {
				return 8;
			} else if (isCol2(p)) {
				return 9;
			} else if (isCol3(p)) {
				return 10;
			} else if (isCol4(p)){
				return 11;
			} else {
				return -1;
			}
		} else if (isRow4(p)) {
			if (isCol1(p)) {
				return 12;
			} else if (isCol2(p)) {
				return 13;
			} else if (isCol3(p)) {
				return 14;
			} else if (isCol4(p)){
				return 15;
			} else {
				return -1;
			}
		} else {
			return -1;
		}
	}
	// set center coordinates of the blocks
	public void SetCoords() {
		// instantiate array of coordinate vectors
		blockCoords = new Vector2[16];
		// row 1
		blockCoords [0] = new Vector2 (-3.3f, 4.3f);
		blockCoords [1] = new Vector2 (-1.1f, 4.3f);
		blockCoords [2] = new Vector2 (1.1f, 4.3f);
		blockCoords [3] = new Vector2 (3.3f, 4.3f);
		// row 2
		blockCoords [4] = new Vector2 (-3.3f, 2.1f);
		blockCoords [5] = new Vector2 (-1.1f, 2.1f);
		blockCoords [6] = new Vector2 (1.1f, 2.1f);
		blockCoords [7] = new Vector2 (3.3f, 2.1f);
		// row 3
		blockCoords [8] = new Vector2 (-3.3f, -0.1f);
		blockCoords [9] = new Vector2 (-1.1f, -0.1f);
		blockCoords [10] = new Vector2 (1.1f, -0.1f);
		blockCoords [11] = new Vector2 (3.3f, -0.1f);
		// row 4
		blockCoords [12] = new Vector2 (-3.3f, -2.3f);
		blockCoords [13] = new Vector2 (-1.1f, -2.3f);
		blockCoords [14] = new Vector2 (1.1f, -2.3f);
		blockCoords [15] = new Vector2 (3.3f, -2.3f);
	}
	// set indexes of adjacent blocks for each block
	// list of possible adjacencies
	public void SetPossAdj() {
		possAdj [0] = new int[3] { 1, 4, 5 };
		possAdj [1] = new int[5] { 0, 2, 4, 5, 6 };
		possAdj [2] = new int[5] { 1, 3, 5, 6, 7 };
		possAdj [3] = new int[3] { 2, 6, 7 };
		possAdj [4] = new int[5] { 0, 1, 5, 8, 9 };
		possAdj [5] = new int[8] { 0, 1, 2, 4, 6, 8, 9, 10 };
		possAdj [6] = new int[8] { 1, 2, 3, 5, 7, 9, 10, 11 };
		possAdj [7] = new int[5] { 2, 3, 6, 10, 11 };
		possAdj [8] = new int[5] { 4, 5, 9, 12, 13 };
		possAdj [9] = new int[8] { 4, 5, 6, 8, 10, 12, 13, 14 };
		possAdj [10] = new int[8] { 5, 6, 7, 9, 11, 13, 14, 15 };
		possAdj [11] = new int[5] { 6, 7, 10, 14, 15 };
		possAdj [12] = new int[3] { 8, 9, 13 };
		possAdj [13] = new int[5] { 8, 9, 10, 12, 14 };
		possAdj [14] = new int[5] { 9, 10, 11, 13, 15 };
		possAdj [15] = new int[3] { 10, 11, 14 };
	}
	// initializes the blockContent array - all values -1 at start
	public void initBlockContent() {
		blockContent = new int[16];
		for (int i = 0; i < 16; i++) {
			blockContent [i] = -1;
		}
	}
	// initialize values in adjacencies array to -1 
	// can be no more than 4 at one time
	public void initAdj() {
		for (int i = 0; i < 4; i++) {
			adjacencies [i] = -1;
		}
	}
	// destroys a game block using its blockNum to identify it
	public void RemoveBlockByNum(int n) {
		string t = "block" + n.ToString ();
		Destroy (GameObject.FindGameObjectWithTag (t));
	}
	// destroys a preview block using its tag to identify it
	// inputs = "blockNext" or "blockNextNext" only
	public void RemovePreviewBlockByTag(string t) {
		Destroy (GameObject.FindGameObjectWithTag (t));
	}
	// returns the number of adjacencies for current block
	// (returns 0 if there are none)
	public int NumAdjacencies(int bn) {
		initAdj ();
		int a = 0;
		// loop through possible adjancencies for current block
		foreach (int el in possAdj[bn]) {
			// check to see if adjacent block is equal to current block
			if (blockContent [el] == blockContent [bn]) {
				// if it is, add index to adj array
				// and increment number of adjacencies
				adjacencies [a] = el;
				a++;
			}
		}
		// return total number of adjacencies
		// will not be larger than 4 at a time
		return a;
	}
	// collapse matching adjacent blocks
	// and create a new one with face number increased by 1
	public void CollapseAdjacencies(int bn) {
		// loop through list of adjacencies
		foreach (int el in adjacencies) {
			// if the current one reflects a match
			if (el > -1) {
				// show that spot as empty in the content array
				blockContent [el] = -1;
				// and remove the block
				RemoveBlockByNum (el);
			}
		}
		// remove the active block
		RemoveBlockByNum (bn);
		// create a new one in its place
		// with face number increased by 1
		CreateBlock (gameBlock [blockContent [bn]], blockCoords [bn], largeGameBlockScale, "block" + bn.ToString ());
		// increment the number in the content array to match
		blockContent [bn]++;
		// if this is larger than the highest block number
		if (blockContent [bn] > highestBlockNum) {
			// set highest equal to new current one
			highestBlockNum = blockContent [bn];
		}
	}
	public void EndGame() {
		int numSpots = 0; // number of available spots on the board
		// loop through the board
		for (int b = 0; b < blockContent.Length; b++) {
			// if content for this spot = -1, it's empty
			if (blockContent [b] == -1) {
				// increment number of available spots
				numSpots++;	
			}
			// if any block on the board = 21, we're done
			if (blockContent [b] == 21) {

				// check current score versus high score
				// from player prefs and save if higher
				if (score > currentHighScore) {
					PlayerPrefs.SetInt ("HighScore", score);
				}
				// load restart scene
				PlayerPrefs.SetString("LastGame", "win");
				SceneManager.LoadScene (1);
			}
		}
		// if there are no available spots, we're hosed
		if (numSpots == 0) {

			// check current score versus high score
			// from player prefs and save if higher
			if (score > currentHighScore) {
				PlayerPrefs.SetInt ("HighScore", score);
			}
			// load restart scene
			PlayerPrefs.SetString("LastGame", "loss");
			SceneManager.LoadScene (1);

		}
	}


	// Use this for initialization
	void Start () {
		currentHighScore = PlayerPrefs.GetInt ("HighScore");
		HighScoreBox.text = currentHighScore.ToString ();
		score = 0;
		min = 1;
		highestBlockNum = 2;
		SetCoords ();
		initBlockContent();
		SetPossAdj();
		initAdj ();
		// create two random previews
		lgPrevNum = RandomBlockNum (min, highestBlockNum + 1);
		smPrevNum = RandomBlockNum (min, highestBlockNum + 1);
		CreateBlock (gameBlock[lgPrevNum - 1], largePreviewPos, largePreviewScale, "blockNext");
		CreateBlock (gameBlock[smPrevNum - 1], smallPreviewPos, smallPreviewScale, "blockNextNext");

	}
	// Update is called once per frame
	void Update () {
		// see if the game should end
		EndGame ();
		// if the game board is not empty and the current block
		// is an actual block...
		if (numClicks > 0 && blockNum > -1) {
			// see if there are any adjacencies to be collapsed
			numAdj = 0;
			numAdj = NumAdjacencies (blockNum);
			// if there are any
			if (numAdj > 0) {
				// add the number to the score and reflect that on the board
				score += numAdj;
				ScoreBox.text = score.ToString ();
				// collapse them
				CollapseAdjacencies (blockNum);
			}
		}
		// if someone clicks on the screen
		if (Input.GetMouseButtonDown (0)) {
			initAdj ();
			// cast a ray from the camera to the mouse click point
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			// find its coordinates in 3 space
			Vector3 loc = ray.origin + (ray.direction * distance);
			// now in 2 space
			Vector2 point = new Vector2 (loc.x, loc.y);
			// figure out where they clicked (-1 if not a block, block num if a block)
			blockNum = BlockClicked (point);
			// if it was a legit click
			if (blockNum > -1) {
				numClicks++;
				numAdj = 0;
				// place block matching that in large preview block in clicked block
				// set block as active in blockActive array
				CreateBlock (gameBlock [lgPrevNum - 1], blockCoords [blockNum], largeGameBlockScale, "block" + blockNum.ToString());
				blockContent [blockNum] = lgPrevNum;
				// delete large preview block
				RemovePreviewBlockByTag ("blockNext");
				// move small preview block up to large preview block
				lgPrevNum = smPrevNum;
				CreateBlock (gameBlock [lgPrevNum - 1], largePreviewPos, largePreviewScale, "blockNext");
				RemovePreviewBlockByTag ("blockNextNext");
				// generate new small preview block
				smPrevNum = RandomBlockNum (min, highestBlockNum + 1);
				CreateBlock (gameBlock [smPrevNum - 1], smallPreviewPos, smallPreviewScale, "blockNextNext");
				// calculate number of similar blocks adjacent to placed block
				numAdj = NumAdjacencies (blockNum);
				if (numAdj > 0) {
					// add this number to score and update it
					score += numAdj;
					ScoreBox.text = score.ToString ();
					// if there are any, then collapse them
					CollapseAdjacencies (blockNum);
				}
			} 
			// otherwise, the fn returned -1 and it's not a legit click
		}


	}
}
