using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class LineRendererController : MonoBehaviour
{
    [SerializeField] List<LineRenderer> lineRenderers = new List<LineRenderer>();

    public void SetPosition(Transform startPos, Transform endPos) //For Lightning mode (fireMode = 4)
    {
        if (lineRenderers.Count > 0)
        {
            foreach (LineRenderer lineRenderer in lineRenderers)
            {
                if(lineRenderer.positionCount >= 2)
                {
                    lineRenderer.SetPosition(0, startPos.position);
                    lineRenderer.SetPosition(1, endPos.position);
                }
                else
                {
                    Debug.Log("LineRenderer does not have enough positions (need 2) to set start and end points.");
                }
            }
           
        }
        else
        {
            Debug.Log("No LineRenderers in List.");
        }
    }
}
