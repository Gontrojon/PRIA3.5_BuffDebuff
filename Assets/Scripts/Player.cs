using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    // variable que nos controla si puede o no saltar
    private bool canJump = true;
    private float speed = 5f;
    // hacemos que en el spawn tengan posicionamiento aleatorio dentro de un margen
    public override void OnNetworkSpawn()
    {
        transform.position = GetRandomPositionOnPlane();
    }

    [ServerRpc]
    void MoveServerRpc(Vector3 movement, ServerRpcParams serverRpcParams = default)
    {
        // se mueve el transform acorde al movimiento que nos manda el cliente;
        transform.position += movement;
    }

    [ServerRpc]
    void JumpServerRpc(Vector3 force, ServerRpcParams serverRpcParams = default)
    {
        // se obtiene el objeto rigidbody del IDcliente y se le añade la fuerza que salte
        NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(serverRpcParams.Receive.SenderClientId).GetComponent<Rigidbody>().AddForce(force);
    }

    // metodo que genera una posicion aleatoria dentro de unos margenes
    static Vector3 GetRandomPositionOnPlane()
    {
        return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
    }

    void Update()
    {
        // si eres owner mueve con los imputs
        if (IsOwner)
        {
            // se llama al server RPC para que efectue el movimiento dle personaje con los calculos ya hechos de speed y time.deltatime
            MoveServerRpc(new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")) * speed * Time.deltaTime);

            // se obtiene si se pulso el imput del salto y si se puede saltar
            if (Input.GetButtonDown("Jump") && canJump)
            {
                Debug.Log("Salta puto");
                // se le añade una fuerza de salto en el eje y, un valor aceptable y se manda la llamada al serverRPC para que la efectue
                JumpServerRpc(new Vector3(0, 330f, 0));
                // se pone que no pueda saltar ya que lo acaba de hacer
                canJump = false;
            }
            // si esta a la altura del suelo se le permite saltar otra vez
            if (transform.position.y <= 1.2f)
            {
                canJump = true;
            }
        }
    }
}

