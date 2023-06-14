using UnityEngine;
using System;
using System.Collections;

public class BodyInitializer : MonoBehaviour 
{
    public Action<BodyInitializer> OnActivate;
    public Vector3 position;
    [SerializeField]
    private Quaternion rotation;
    [SerializeField]
    private float bodyMass;
    [SerializeField]
    private Vector3 scale;
    //private Vector3 impules;
    private bool isAdd = true;
    [System.NonSerialized]
    public Rigidbody body;
    [SerializeField]
    private bool haveBody = false;

    void Awake()
    {
        if (haveBody)
        {
            Destroy(GetComponent<Rigidbody>());
        }
    }
    void Start ()
    {
        body = GetComponent<Rigidbody>();

        //var id = UnityEngine.Random.Range(0, CueController.listPos.Count - 1);
        //transform.position = CueController.listPos[id];
        //CueController.listPos.RemoveAt(id);
        //transform.rotation = rotation;

    }
    public void Activate (Vector3 position)
    {
        this.position = position;
        isAdd = false;
    }
    public void Activate ()
    {
        position = transform.position;
        rotation = transform.rotation;
        isAdd = false;
    }
    public void DeActivate ()
    {
        if (body)
        {
            position = body.position;
            rotation = body.rotation;
            Destroy(body);
        }
    }
    public void DeActivate (Vector3 position)
    {
        if (body)
        {
            this.position = position;
            rotation = body.rotation;
            if (body)
            {
                Destroy(body);
            }
        }
    }

    void FixedUpdate ()
    {
        if (!isAdd)
        {
            isAdd = true;
            if (haveBody)
            {
                body = gameObject.AddComponent<Rigidbody>();
                body.isKinematic = true;
                transform.position = position;
               // transform.rotation = rotation;
                body.drag = 0.5f;
                body.angularDrag = 0.7f;
                body.position = position;
               // body.rotation = rotation;
                body.mass = bodyMass;
                body.isKinematic = false;
                CueController cueController = CueController.FindObjectOfType<CueController>();
                if (cueController)
                {
                    body.maxDepenetrationVelocity = cueController.ballMaxVelocity;
                }
                body.maxAngularVelocity = 350.0f;
                if(body.GetComponent<SphereCollider>())
                body.GetComponent<SphereCollider>().contactOffset = 0.01f;
                //body.AddForce(impules, ForceMode.Impulse);
                if (OnActivate != null)
                {
                    OnActivate(this);
                }
            }
        }
    }
   

    public void InitializeBodies()
    {
        body = GetComponent<Rigidbody>();
        if (body)
        {
            haveBody = true;
            bodyMass = ( body.mass);
            gameObject.isStatic = false;
        }
        else
        {
            gameObject.isStatic = true;
        }
       
        position = (transform.position);
        rotation = (transform.rotation);
    }
}
