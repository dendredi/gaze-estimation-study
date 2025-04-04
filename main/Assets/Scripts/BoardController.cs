using UnityEngine;

public class BoardController : MonoBehaviour
{
    public GameObject board;
    public GameObject target;
    public GameObject predictor;
    
    public float moveSpeed = 0.05f;

    private Vector3 posMin, posMax;

    private BoardState state;
    
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

        state = new AlterTargetState(this);
    }

    void Update()
    {
        state.Update();
    }

    private void MoveWasd(GameObject o)
    {
        var oldPosition = o.transform.localPosition;
        
        var moveY = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime; // WS movement
        var moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime; // AD movement

        var newPosition = oldPosition + new Vector3(moveX, moveY, 0);

        // Clamp position to keep the sphere on the board surface
        newPosition.x = Mathf.Clamp(newPosition.x, posMin.x, posMax.x);
        newPosition.y = Mathf.Clamp(newPosition.y, posMin.y, posMax.y);
        newPosition.z = oldPosition.z;

        o.transform.localPosition = newPosition;
    }

    private void SetVisible(GameObject o, bool visible)
    {
        o.GetComponent<Renderer>().enabled = visible;
    }

    private abstract class BoardState
    {
        protected readonly BoardController Controller;

        protected BoardState(BoardController controller)
        {
            Controller = controller;
        }
        
        public abstract void Update();
    }

    private class AlterTargetState : BoardState
    {
        public AlterTargetState(BoardController controller) : base(controller)
        {
            Controller.SetVisible(controller.target, true);
            Controller.SetVisible(controller.predictor, false);
        }
        
        public override void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                
                Controller.state = new PredictState(Controller);
            }

            Controller.MoveWasd(Controller.target);
        }
    }

    private class PredictState : BoardState
    {
        public PredictState(BoardController controller) : base(controller)
        {
            Controller.SetVisible(controller.target, false);
            Controller.SetVisible(controller.predictor, true);
        }
        
        public override void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Controller.state = new AlterTargetState(Controller); // TODO
            }

            Controller.MoveWasd(Controller.predictor);
        }
    }
}
