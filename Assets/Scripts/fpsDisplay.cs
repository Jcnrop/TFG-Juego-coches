using UnityEngine;
using TMPro;
using Photon.Pun;
public class fpsDisplay : MonoBehaviour
{
    public TextMeshProUGUI FpsText;
    private float pollingTime = 0.25f;
    private float time;
    private int frameCount;
    void Update()
    {
        time += Time.deltaTime;
        frameCount++;
        if (time >= pollingTime)
        {
            int frameRate = Mathf.RoundToInt(frameCount / time);
            FpsText.text = frameRate.ToString() + " FPS";
            time -= pollingTime;
            frameCount = 0;

            //Muestra el ping si estamos en online
            if (PhotonNetwork.InRoom)
            {
                FpsText.text += "<br>Ping: " + PhotonNetwork.GetPing();
            }
        }
    }
}
    