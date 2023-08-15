using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Cinemachine;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    public Rigidbody2D rb;
    public Animator an;
    public SpriteRenderer sr;
    public PhotonView pv;
    public Text NicknameText;
    public Image HealthImage;
    public AudioClip dizzy;
    public AudioClip sword;
    public AudioClip run;

    bool isInvincible = false;
    bool isGround;
    bool isDizzy = false;
    bool isDie = false;
    AudioSource audio;

    Vector3 curPos;

    void Awake()
    {
        NicknameText.text = pv.IsMine ? PhotonNetwork.NickName : pv.Owner.NickName;
        NicknameText.color = pv.IsMine ? Color.green : Color.red;

        if(pv.IsMine)
        {
            var CM = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>();
            CM.Follow = transform;
            CM.LookAt = transform;  
        }
    }
    

    // Start is called before the first frame update
    void Start()
    {
        audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (pv.IsMine)
        {
            // 양옆 이동
            float axis = Input.GetAxisRaw("Horizontal");
            rb.velocity = new Vector2(4 * axis, rb.velocity.y);

            if (axis != 0)
            {
                an.SetBool("Walk", true);
                pv.RPC("FlipXRPC", RpcTarget.AllBuffered, axis);
                
            }
            else
            {
                an.SetBool("Walk", false);
            }

            isGround = Physics2D.OverlapCircle((Vector2)transform.position
                + new Vector2(0, -0.5f), 0.07f, 1 << LayerMask.NameToLayer("Ground"));

            an.SetBool("Jump", !isGround);

            if (Input.GetKeyDown(KeyCode.Space) && isGround)
            {
                pv.RPC("JumpRPC", RpcTarget.All);
            }

            if (Input.GetMouseButtonDown(0) && isGround)
            {
                an.SetTrigger("Attack");
                audio.PlayOneShot(sword);
                pv.RPC("AttackRPC", RpcTarget.All);
            }

            if(Input.GetMouseButtonDown(0) && !isGround)
            {
                an.SetTrigger("JumpAttack");
                audio.PlayOneShot(sword);
                pv.RPC("AttackRPC", RpcTarget.All);
            }
        }
        else if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos;

        else transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
    }

    public void Hit()
    {
        if (!isInvincible)
        {
            HealthImage.fillAmount -= 0.1f;
            if (HealthImage.fillAmount <= 0)
            {
                isDie = true;
                pv.RPC("DestroyRPC", RpcTarget.All);
                GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject.SetActive(true);
            }
            else
            {
                isInvincible = true;
                StartCoroutine(InvincibilityTimer());
                StartCoroutine(DizzyTimer());
            }
        }     
    }
    IEnumerator DizzyTimer()
    {
        isDizzy = true;
        an.SetTrigger("Dizzy");
        pv.RPC("DizzyRPC", RpcTarget.All, true);

        audio.PlayOneShot(dizzy);

        yield return new WaitForSeconds(2.0f);
        isDizzy = false;
        pv.RPC("DizzyRPC", RpcTarget.All, false);
    }
    IEnumerator InvincibilityTimer()
    {
        yield return new WaitForSeconds(1.0f); // 무적 상태 유지 시간
        isInvincible = false;
    }
    [PunRPC]
    void FlipXRPC(float axis) => sr.flipX = axis == -1;
    [PunRPC]
    void JumpRPC()
    {
        rb.velocity = Vector2.zero;
        rb.AddForce(Vector2.up * 700);
    }
    [PunRPC]
    void DestroyRPC()
    {
        isDie = true;
        an.SetTrigger("Die");
        
        Destroy(gameObject, 2f);
    }

    [PunRPC]
    void AttackRPC()
    {
        if(!pv.IsMine && !isInvincible)
        {
            // 근접 공격 로직 추가
            Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, 1.5f);

            foreach (Collider2D playerCollider in hitPlayers)
            {
                if (playerCollider.gameObject != gameObject)
                {
                    Player player = playerCollider.GetComponent<Player>();
                    if (player != null)
                    {
                        player.Hit();
                    }
                }
            }
        }
    }
    [PunRPC]
    void DizzyRPC(bool dizzyState)
    {
        isDizzy = dizzyState;

        if(isDizzy)
        {
            an.SetTrigger("Dizzy");
        }
        else
        {
            an.ResetTrigger("Dizzy");
        }
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(HealthImage.fillAmount);
            stream.SendNext(isDie);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
            HealthImage.fillAmount = (float)stream.ReceiveNext();
            isDie = (bool)stream.ReceiveNext();
        }
    }
}
