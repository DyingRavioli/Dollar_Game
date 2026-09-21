using UnityEngine;

public class MoveTowardsScript : MonoBehaviour
{
    public GameObject target;
    private float speed = 5f;

    private float lifetime = 10f;

    private bool reached;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (lifetime > 0)
        {
            lifetime -= Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
        }
        float step = speed * Time.deltaTime;
        if (!reached)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, step);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == target)
        {
            print("yes");
            reached = true;
            CutScript cutScript = target.GetComponent<CutScript>();
            if (!cutScript.oneHandTriggered)
            {
                cutScript.oneHandTriggered = true;
            }
            else
            {
                cutScript.twoHandTriggered = true;
            }
        }
    }
}
