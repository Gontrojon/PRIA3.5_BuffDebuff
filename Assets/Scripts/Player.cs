using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    // variable que nos controla si puede o no saltar
    private bool canJump = true;
    // velocidad del player
    private float speed = 5f;
    // multiplicador  de velocidad si se es bufado o debufado
    private float buffDebuffSpeed = 1;
    // variable para establecer corrutina
    private IEnumerator coroutine;
    // variable dle render para no buscarla con getcomponetn cada vez
    private Renderer render;

    private void Awake()
    {
        // se obteine el renderer
        render = GetComponent<Renderer>();
    }

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

    [ClientRpc]
    public void BuffDebuffClientRpc(float buffDebuffSpeed, float durationbuffDebuff, Color buffDebuffColor)
    {
        // se crea la corrutina y se llama
        coroutine = BuffDebuffCoroutine(buffDebuffSpeed, durationbuffDebuff, buffDebuffColor);
        StartCoroutine(coroutine);
    }

    private IEnumerator BuffDebuffCoroutine(float buffDebuffSpeed , float durationbuffDebuff, Color buffDebuffColor)
    {
        // se guarda el valor original del multiplicador de buffo/debuffo
        float originalSpeedBuffDebuff = this.buffDebuffSpeed;
        // se aplica el nuevo multiplicador
        this.buffDebuffSpeed = buffDebuffSpeed;
        // se guarda el color original del player
        Color originalColor = render.material.color;
        // se le asigna que su color es el asignado para el buffo o debuffo
        render.material.color = buffDebuffColor;
        // se duerme la corrutina durante el tiempo que va estar bufado/debufado
        yield return new WaitForSeconds(durationbuffDebuff);
        // se le devuelven sus parametros originales antes de terminar
        this.buffDebuffSpeed = originalSpeedBuffDebuff;
        render.material.color = originalColor;
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
            MoveServerRpc(new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")) * speed * buffDebuffSpeed * Time.deltaTime);

            // se obtiene si se pulso el imput del salto y si se puede saltar
            if (Input.GetButtonDown("Jump") && canJump)
            {
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

