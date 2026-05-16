using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public DoorDirection doorDirection;

    public float textDistance = 1.5f;
    public float openDistance = 1.2f;
    public float closeDelay = 0.3f;

    private static List<Door> doors = new List<Door>();
    private static GameObject pressEText;

    private Animator animator;
    private Collider2D doorCollider;
    private Transform player;

    private bool isOpen = false;
    private float timer = 0f;

    void OnEnable()
    {
        doors.Add(this);
    }

    void OnDisable()
    {
        doors.Remove(this);
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        doorCollider = GetComponent<Collider2D>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        if (pressEText == null)
        {
            pressEText = GameObject.Find("PressE Text");
        }

        if (pressEText != null)
        {
            pressEText.SetActive(false);
        }

        if (doorCollider != null)
        {
            doorCollider.enabled = true;
            doorCollider.isTrigger = false;
        }
    }

    void Update()
    {
        if (player == null) return;

        Door nearestDoor = GetNearestClosedDoor(player.position);

        if (nearestDoor == this)
        {
            float distance =
                Vector2.Distance(
                    player.position,
                    transform.position
                );

            if (distance <= textDistance)
            {
                if (pressEText != null)
                {
                    pressEText.SetActive(true);
                }
            }

            if (
                distance <= openDistance &&
                Input.GetKeyDown(KeyCode.E)
            )
            {
                OpenDoor();
            }
        }

        if (isOpen)
        {
            timer += Time.deltaTime;

            if (timer >= closeDelay)
            {
                CloseDoor();
            }
        }
    }

    void LateUpdate()
    {
        if (
            player == null ||
            pressEText == null
        ) return;

        Door nearestDoor =
            GetNearestClosedDoor(player.position);

        if (nearestDoor == null)
        {
            pressEText.SetActive(false);
            return;
        }

        float distance =
            Vector2.Distance(
                player.position,
                nearestDoor.transform.position
            );

        if (distance > nearestDoor.textDistance)
        {
            pressEText.SetActive(false);
        }
    }

    Door GetNearestClosedDoor(Vector3 playerPosition)
    {
        Door nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Door door in doors)
        {
            if (door == null) continue;
            if (door.isOpen) continue;

            float distance =
                Vector2.Distance(
                    playerPosition,
                    door.transform.position
                );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = door;
            }
        }

        return nearest;
    }

    void OpenDoor()
    {
        isOpen = true;
        timer = 0f;

        if (pressEText != null)
        {
            pressEText.SetActive(false);
        }

        if (animator != null)
        {
            if (doorDirection == DoorDirection.Left)
            {
                animator.Play("LeftDoor");
            }
            else if (doorDirection == DoorDirection.Right)
            {
                animator.Play("RightDoor");
            }
            else
            {
                animator.Play("FrontDoor");
            }
        }

        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }
    }

    void CloseDoor()
    {
        isOpen = false;

        if (animator != null)
        {
            animator.Play("Stay");
        }

        if (doorCollider != null)
        {
            doorCollider.enabled = true;
            doorCollider.isTrigger = false;
        }
    }
}