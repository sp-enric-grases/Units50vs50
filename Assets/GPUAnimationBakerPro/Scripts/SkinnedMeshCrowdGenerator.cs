using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is use to instantiate the gameObjects with the skinned mesh renderer component to perform animation
/// </summary>
public class SkinnedMeshCrowdGenerator : MonoBehaviour
{
    [System.Serializable]
    public class ValueRange
    {
        public float minValue = 1.0f;
        public float maxValue = 1.0f;
    }

    /// <summary>
    /// the rectangle crowd shape
    /// </summary>
    [System.Serializable]
    public class RectangleArea
    {
        public float width = 10.0f;
        public float length = 20.0f;
        public float height = 20.0f;
    }

    /// <summary>
    /// the cycle crowd shape
    /// </summary>
    [System.Serializable]
    public class CycleArea
    {
        //customize model forward axis 
        public enum ModelForwardAxis
        {
            TransformForward,
            TransformBackward,
            TransformUpward,
            TransformDownward,
            TransformRight,
            TransformLeft,
        }

        //the cycle crowd area inner hollow radius
        public float innerHollowRadius = 7.0f;
        //the cycle crowd area radius
        public float radius = 10.0f;
        //customize model forward axis 
        public ModelForwardAxis modelForwardAxis;
    }

    //the crowd shape type this generator will created
    public enum CrowdShapeType
    {
        Rectangle,
        Cycle
    }

    //the prefab you want to instantiate
    public GameObject animatePrefab;
    //the animation play speed's range of each individual in crowd.
    public ValueRange randomSpeedRange;
    //this field will make the individuals in the crowd have different play offset,in order to make them look different.
    public float randomPlayNoise;
    //the crowd shape type this generator will created,there are two types of shape: the Cycle and the Rectangle.
    public CrowdShapeType crowdShapeType;
    //the rectangle crowd shape
    public RectangleArea generateInCuboidArea;
    //the cycle crowd shape
    public CycleArea generateCycleArea;
    //the num of individual this generator will created
    public int generateNum = 100;
    //whether the individuals in crowd will cast shadow or not
    public bool castShadow;
    //whether the individual in crowd will receive shadow or not
    public bool receiveShadow;

    /// <summary>
    /// Init
    /// </summary>
    void Start()
    {
        this.Generate();
    }

    /// <summary>
    /// Generate the crowd of animatePrefab
    /// </summary>
    public void Generate()
    {
        for (int i = 0;i< generateNum; i++)
        {
            GameObject goClone = null;
            if (crowdShapeType == CrowdShapeType.Rectangle)
            {
                Vector3 pos = RectangleRandomPos(this.generateInCuboidArea);
                Quaternion rot = this.transform.rotation * animatePrefab.transform.rotation;
                goClone = Instantiate(animatePrefab, pos,rot);
                goClone.transform.SetParent(this.transform);
            }
            else if (crowdShapeType == CrowdShapeType.Cycle)
            {
                Vector3 pos = CycleRandomPos(this.generateCycleArea);
                Vector3 lookDir = this.transform.position - pos;
                Quaternion needRot = Quaternion.identity;

                if (this.generateCycleArea.modelForwardAxis == CycleArea.ModelForwardAxis.TransformForward)
                {
                    needRot = Quaternion.FromToRotation(animatePrefab.transform.forward, lookDir);
                }
                else if (this.generateCycleArea.modelForwardAxis == CycleArea.ModelForwardAxis.TransformBackward)
                {
                    needRot = Quaternion.FromToRotation(-animatePrefab.transform.forward, lookDir);
                }
                else if (this.generateCycleArea.modelForwardAxis == CycleArea.ModelForwardAxis.TransformUpward)
                {
                    needRot = Quaternion.FromToRotation(animatePrefab.transform.up, lookDir);
                }
                else if (this.generateCycleArea.modelForwardAxis == CycleArea.ModelForwardAxis.TransformDownward)
                {
                    needRot = Quaternion.FromToRotation(-animatePrefab.transform.up, lookDir);
                }
                else if (this.generateCycleArea.modelForwardAxis == CycleArea.ModelForwardAxis.TransformLeft)
                {
                    needRot = Quaternion.FromToRotation(-animatePrefab.transform.right, lookDir);
                }
                else if (this.generateCycleArea.modelForwardAxis == CycleArea.ModelForwardAxis.TransformRight)
                {
                    needRot = Quaternion.FromToRotation(animatePrefab.transform.right, lookDir);
                }

                Quaternion rot = this.transform.rotation * needRot * animatePrefab.transform.rotation;
                goClone = Instantiate(animatePrefab, pos, rot);
                goClone.transform.SetParent(this.transform);
            }

            Renderer[] renderers = goClone.GetComponentsInChildren<Renderer>();

            for (int j = 0;j<renderers.Length;j++)
            {
                if (this.castShadow == true)
                {
                    //enable shadow effect of each individual
                    renderers[j].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                }
                else
                {
                    //disable shadow effect of each individual
                    renderers[j].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
               
                renderers[j].receiveShadows = this.receiveShadow;
            }

            Animator animator = goClone.GetComponent<Animator>();
            Animation animation = goClone.GetComponent<Animation>();
            float aniSpeed = Random.Range(randomSpeedRange.minValue, randomSpeedRange.maxValue);

            if (animator != null)
            {
                animator.speed = aniSpeed;

                animator.SetFloat("Offset", Random.Range(0.0f, 1.0f) * randomPlayNoise);
            }
            
            if (animation != null)
            {
                foreach (AnimationState state in animation)
                {
                    state.speed = aniSpeed;
                    state.time = Random.Range(0.0f, state.length) * randomPlayNoise;
                }
            }

        }

      
    }

    /// <summary>
    /// Get a random position within the rectangle shape area
    /// </summary>
    /// <param name="rectangleShape"></param>
    /// <returns></returns>
    private Vector3 RectangleRandomPos(RectangleArea rectangleShape)
    {
        Vector3 centerPos = this.transform.position;
        Matrix4x4 localToWorldMatrix = this.transform.localToWorldMatrix;
        float halfWidth = rectangleShape.width / 2.0f;
        float halfLength = rectangleShape.length / 2.0f;
        float halfHeight = rectangleShape.height / 2.0f;
        float posX = Random.Range(-halfWidth, halfWidth);
        float posY = Random.Range(-halfHeight, halfHeight);
        float posZ = Random.Range(-halfLength, halfLength);
        Vector3 localPos = new Vector3(posX, posY, posZ);
        Vector3 worldPos = localToWorldMatrix.MultiplyPoint(localPos);
        return worldPos;
    }

    /// <summary>
    /// Get a random position within the cycle shape area
    /// </summary>
    /// <param name="cycleArea"></param>
    /// <returns></returns>
    private Vector3 CycleRandomPos(CycleArea cycleArea)
    {
        Matrix4x4 localToWorldMatrix = this.transform.localToWorldMatrix;
        float posX = Random.Range(-1.0f, 1.0f);
        float posY = 0.0f;
        float posZ = Random.Range(-1.0f, 1.0f);
        Vector3 randomDir = new Vector3(posX, posY, posZ);
        randomDir = Vector3.Normalize(randomDir);
        float radius = Random.Range(cycleArea.innerHollowRadius, cycleArea.radius);
        Vector3 localPos = randomDir * radius;
        Vector3 worldPos = localToWorldMatrix.MultiplyPoint(localPos);
        return worldPos;
    }

}
