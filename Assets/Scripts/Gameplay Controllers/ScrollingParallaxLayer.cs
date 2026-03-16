using UnityEngine;

public class ScrollingParallaxLayer : MonoBehaviour
{
    public Transform pieceA;
    public Transform pieceB;
    public float parallaxFactor;
    
    private float pieceWidth;
    private float lastTargetX;
    private Transform target;

    private void Start()
    {
        target = FindFirstObjectByType<CamerMovement>().transform;
        lastTargetX = target.position.x;
        pieceWidth = GetWidth(pieceA.gameObject);
    }

    private void LateUpdate()
    {
        float deltaX = target.position.x - lastTargetX;
        float moveX = deltaX * parallaxFactor;

        pieceA.position += new Vector3(moveX, 0f, 0f);
        pieceB.position += new Vector3(moveX, 0f, 0f);

        if (target.position.x > lastTargetX)
        {
            if (pieceA.position.x + pieceWidth < target.position.x)
                pieceA.position = new Vector3(pieceB.position.x + pieceWidth, pieceA.position.y, pieceA.position.z);

            if (pieceB.position.x + pieceWidth < target.position.x)
                pieceB.position = new Vector3(pieceA.position.x + pieceWidth, pieceB.position.y, pieceB.position.z);
        }
        else if (target.position.x < lastTargetX)
        {
            if (pieceA.position.x > target.position.x + pieceWidth)
                pieceA.position = new Vector3(pieceB.position.x - pieceWidth, pieceA.position.y, pieceA.position.z);

            if (pieceB.position.x > target.position.x + pieceWidth)
                pieceB.position = new Vector3(pieceA.position.x - pieceWidth, pieceB.position.y, pieceB.position.z);
        }

        lastTargetX = target.position.x;
    }

    float GetWidth(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return 0f;

        Bounds bounds = renderers[0].bounds;

        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        return bounds.size.x;
    }
}