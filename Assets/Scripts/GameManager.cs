using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // lista donde se guardaran los clientes
    private List<NetworkClient> clientsIDList = new List<NetworkClient>();
    //variable para el control de sleep de la corrutina
    private float sleepCoroutine = 20f;
    // multiplicador que aplicara para dar o quitar velocidad
    private float buffSpeed = 3.5f;
    private float debuffSpeed = 0.3f;
    // tiempo de duracion del buffo
    private float durationbuffDebuff = 10f;
    // color que se le pondra al player segun si es bufado o debufado
    private Color buffColor = Color.green;
    private Color debuffColor = Color.red;
    // control de si hay gameOver
    private bool gameOver = false;
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
        }

        GUILayout.EndArea();
    }

    void StartButtons()
    {
        if (GUILayout.Button("Host"))
        {
            NetworkManager.Singleton.StartHost();
            // se llama a la corrutina
            StartCoroutine("BuffDebuff");
        }
        if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
        if (GUILayout.Button("Server"))
        {
            NetworkManager.Singleton.StartServer();
            // se llama a la corrutina
            StartCoroutine("BuffDebuff");
        }
    }

    void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ?
            "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
            NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }

    IEnumerator BuffDebuff()
    {
        // mientras no sea gameOver la corrutina se sigue ejecutando
        while (!gameOver)
        {
            Debug.Log("empieza la espera para el siguiete buff debuff");
            // lapso de tiempo entre bufos
            yield return new WaitForSeconds(sleepCoroutine);
            // se reinicia la lista de ids por si hay mas clientes
            clientsIDList = new List<NetworkClient>();
            // se rellena la lista de clientes
            foreach (NetworkClient uid in NetworkManager.Singleton.ConnectedClientsList)
            {
                clientsIDList.Add(uid);
            }

            //randoms necesarios para el funcionamiento basico de bufar y debufar
            int random, randomclientID;

            // se genera un random de 0 o 1 para si es bufo o debuffo
            random = Random.Range(0, 2);
            // se selecciona un cliente de forma aleatoria
            randomclientID = Random.Range(0, clientsIDList.Count);
            Debug.Log("soy corrutina el random generado es: " + random);
            //depende de si es bufo o debufo se llama al clientrcp pasando las variables correspondientes
            if (random == 0)
            {

                clientsIDList[randomclientID].PlayerObject.GetComponent<Player>().BuffDebuffClientRpc(buffSpeed, durationbuffDebuff, buffColor);
            }
            else if (random == 1)
            {
                clientsIDList[randomclientID].PlayerObject.GetComponent<Player>().BuffDebuffClientRpc(debuffSpeed, durationbuffDebuff, debuffColor);
            }

        }
    }
}


