using UnityEngine;

public class Door : MonoBehaviour
{
    public Sprite upSprite;
    public Sprite downSprite;
    public Sprite leftSprite;
    public Sprite rightSprite;

    public DoorDirection doorDirection;

    private SpriteRenderer sr;
    private Transform player;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (doorDirection == DoorDirection.Left)
        {
            sr.sprite = leftSprite;
        }
        else if (doorDirection == DoorDirection.Right)
        {
            sr.sprite = rightSprite;
        }
    }

    void Update()
    {
        if (player == null) return;

        if (doorDirection == DoorDirection.Left ||
            doorDirection == DoorDirection.Right)
        {
            return;
        }

        Vector2 dir = player.position - transform.position;

        if (dir.y > 0)
        {
            sr.sprite = downSprite;
        }
        else
        {
            sr.sprite = upSprite;
        }
    }
}