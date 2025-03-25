using UnityEngine;

public class BoardController : MonoBehaviour
{
    public GameObject board;
    public GameObject target;
    
    public float moveSpeed = 0.05f;

    private Vector3 posMin, posMax;
    
    void Start()
    {
        if (target == null || board == null)
        {
            Debug.LogError("Target is not assigned!");
            return;
        }
        
        // Get board size
        var boardPosition = board.transform.localPosition;
        var boardScale = board.transform.localScale;
        
        var targetRadius = target.GetComponent<SphereCollider>().radius;

        // Calculate movement boundaries (assuming board is flat on XZ plane)
        posMin = boardPosition - boardScale / 2 + (Vector3.one * targetRadius);
        posMax = boardPosition + boardScale / 2 - (Vector3.one * targetRadius);
    }

    // Update is called once per frame
    void Update()
    {
        var oldPosition = target.transform.localPosition;
        
        float moveY = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime; // WS movement
        float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime; // AD movement

        Vector3 newPosition = oldPosition + new Vector3(moveX, moveY, 0);

        // Clamp position to keep the sphere on the board surface
        newPosition.x = Mathf.Clamp(newPosition.x, posMin.x, posMax.x);
        newPosition.y = Mathf.Clamp(newPosition.y, posMin.y, posMax.y);
        newPosition.z = oldPosition.z;

        target.transform.localPosition = newPosition;
    }
}
