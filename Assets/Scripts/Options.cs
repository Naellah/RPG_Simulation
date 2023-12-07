using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{

    public int numberOfKnightsRed;
    public int numberOfKnightsBlue;
    public int numberOfArchersRed;
    public int numberOfArchersBlue;



    public void readStringInputRed(string s)
    {
        numberOfKnightsRed = int.Parse(s);
    }

    public void readStringInputBlue(string s)
    {
        numberOfKnightsBlue = int.Parse(s);
    }

    public void readStringInputRedArcher(string s)
    {
        numberOfArchersRed = int.Parse(s);
    }

    public void readStringInputBlueArcher(string s)
    {
        numberOfArchersBlue = int.Parse(s);
    }





    public void StartGame()
    {
        PlayerPrefs.SetInt("BlueKnightCount", numberOfKnightsBlue);
        PlayerPrefs.SetInt("RedKnightCount", numberOfKnightsRed);
       

        // Chargez la scène de jeu
        SceneManager.LoadSceneAsync("Game");
    }


}