using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class colisionPared : MonoBehaviour
{
    private int id_pared;

    private int numGolpes = 0;
    private int maxNumGolpes = 2;

    private float tiempoDelay = 0.05f;
    private float tiempoActualDelay = 0f;
    private bool activarDelay = false;

    public int Id_pared { get => id_pared; set => id_pared = value; }

    public void aumentarNumGolpes()
    {

        /*if (activarDelay)
        {
            tiempoActualDelay += Time.deltaTime;

            if (tiempoActualDelay >= tiempoDelay)
            {
                activarDelay = false;
                tiempoActualDelay = 0;
            }
        }
        else*/
        if(!activarDelay)
        {
            numGolpes++;

            float cambioColor = 1 - (((float)numGolpes) / ((float)maxNumGolpes));
            gameObject.GetComponent<MeshRenderer>().material.color = new Color(1f, cambioColor, cambioColor, 1);

            

            if (numGolpes >= maxNumGolpes + 1)
            {
                Destroy(this.gameObject);
            }

            activarDelay = true;
            StartCoroutine("startDelay");
        }

    }

    IEnumerator startDelay()
    {
        yield return new WaitForSeconds(tiempoDelay);
        activarDelay = false;
    }
}
