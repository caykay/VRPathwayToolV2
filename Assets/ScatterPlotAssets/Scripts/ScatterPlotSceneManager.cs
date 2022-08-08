﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


public class ScatterPlotSceneManager : MonoBehaviour
{

    public List<SAxis> sceneAxes { get; internal set; }

    public HashSet<int> axisIds { get; set; }

    public DataBinding.DataObject dataObject;

    public class OnAxisAddedEvent : UnityEvent<SAxis> { }
    public OnAxisAddedEvent OnAxisAdded = new OnAxisAddedEvent();

    public bool generateAll;

    [SerializeField]
    GameObject axisPrefab;

    [Header("Data Source")]

    [SerializeField]
    TextAsset sourceData;

    [SerializeField]
    DataObjectMetadata metadata;

    public GameObject headCamera;


    static ScatterPlotSceneManager _instance;
    public static ScatterPlotSceneManager Instance
    {
        get { return _instance ?? (_instance = FindObjectOfType<ScatterPlotSceneManager>()); }
    }

    private void Awake()
    {
        sceneAxes = new List<SAxis>();
        axisIds = new HashSet<int>();
        dataObject = new DataBinding.DataObject(sourceData.text, metadata);
    }

    void Start()
    {
        

        // setup default visual settings

        VisualisationAttributes.Instance.sizes = Enumerable.Range(0, ScatterPlotSceneManager.Instance.dataObject.DataPoints).Select(_ => 1f).ToArray();

        List<float> categories = ScatterPlotSceneManager.Instance.dataObject.getNumberOfCategories(VisualisationAttributes.Instance.ColoredAttribute);
        int nbCategories = categories.Count;
        Color[] palette = Colors.generateColorPalette(nbCategories);

        Dictionary<float, Color> indexCategoryToColor = new Dictionary<float, Color>();
        for (int i = 0; i < categories.Count; i++)
        {
            indexCategoryToColor.Add(categories[i], palette[i]);
        }

        VisualisationAttributes.Instance.colors = Colors.mapColorPalette(ScatterPlotSceneManager.Instance.dataObject.getDimension(VisualisationAttributes.Instance.ColoredAttribute), indexCategoryToColor);

        // create the axis

        for (int i = 0; i < dataObject.Identifiers.Length; ++i)
        {
            // displays all axes on the scene if true
            if (generateAll)
            {
                Vector3 v = new Vector3(1.352134f - (i % 7) * 0.35f, 1.506231f - (i / 7) / 2f, 0f);// -0.4875801f);
                GameObject obj = (GameObject)Instantiate(axisPrefab);
                obj.transform.position = v;
                SAxis axis = obj.GetComponent<SAxis>();
                axis.Init(dataObject, i, true);
                axis.InitOrigin(v, obj.transform.rotation);
                axis.tag = "Axis";

                AddAxis(axis);
            }
            
        }

    }


    public int assignNodeId(string name)
    {
        foreach(var item in dataObject.Identifiers.Select((name, index) => (name, index)))
        {
            if (name == item.name) return item.index;
        }
        return -1;
    }


    //generates the 2 axis graph for a specific node
    public bool SpawnGraph(Transform node, Vector3 location, int spID)
    {
        /*GameObject graph = new GameObject();
        graph.name = node.name + " Graph";
        graph = Instantiate(graph, node);
        GameObject obj = (GameObject)Instantiate(axisPrefab, graph.transform);*/

        /*
                if (sceneAxes.Count >= 1)
                {
                    GameObject axisClone = sceneAxes.Last().Clone();
                    //axisClone.transform.rotation = obj.transform.rotation;
                    axisClone.transform.position = location;
                    //axisClone.transform.localScale = axis.transform.localScale;

                    AddAxis(axisClone.GetComponent<SAxis>());
                    return axisClone;
                }
                else {
                    GameObject obj = (GameObject)Instantiate(axisPrefab);
                    obj.transform.position = location;
                    SAxis axis = obj.GetComponent<SAxis>();
                    axis.Init(dataObject, 2, false);
                    axis.InitOrigin(location, obj.transform.rotation);
                    //axis.transform.localScale = new Vector3(axis.transform.localScale.x * 2, axis.transform.localScale.y * 2, axis.transform.localScale.z * 2);
                    axis.tag = "Axis";
                    AddAxis(axis);
                    return axis.gameObject;
                }*/

        //int id = 2;  // to be changed when dataset attribute is relevant


        if (axisIds.Contains(1)) // if axis control axis already exists
        {
            GenerateCloneAxis(1, location, Quaternion.Euler(0, 0, 90), true);

        }
        else {
            GenerateAxis(1, location, Quaternion.Euler(0, 0, 90), true);
        }

        if (axisIds.Contains(spID))
        {
            GenerateCloneAxis(spID, location, Quaternion.Euler(0, 180, 0), false);
        }
        else
        {
            GenerateAxis(spID, location, Quaternion.Euler(0, 180, 0), false);
        }

        return true;
    }

   void GenerateAxis(int id, Vector3 location, Quaternion rotation, bool horizantal)
    {
        // id 1 is control attribute

        // Vertical Axis
        GameObject obj = (GameObject)Instantiate(axisPrefab);
        float h = obj.GetComponent<BoxCollider>().size.y * obj.transform.localScale.y;

        if (horizantal) location = new Vector3(location.x - h / 2, location.y - h / 2, location.z);
        obj.transform.position = location;
        obj.transform.rotation = rotation;
        // Horizontal Axis
        SAxis axis = obj.GetComponent<SAxis>();
        axis.Init(dataObject, id, false);  // id = 1 i.e. control attribute
        
        axis.InitOrigin(location, rotation);
        axis.tag = "Axis";
        AddAxis(axis);
        ScaleAxisVisualizations(axis);
    }

    /*
     * Generates a clone of an existing axis (To be removed)
     */
    void GenerateCloneAxis(int id, Vector3 location, Quaternion rotation, bool horizontal)
    {

        SAxis hAxisClone = sceneAxes.ElementAt(id).Clone().GetComponent<SAxis>();
        hAxisClone.transform.rotation = rotation;

        float h = hAxisClone.GetComponent<BoxCollider>().size.y * hAxisClone.transform.localScale.y;
        if (horizontal)
        {
            hAxisClone.transform.position = new Vector3(location.x - h / 2, location.y - h / 2, location.z);

        }
        else {
            hAxisClone.transform.position = location;
        }
        AddAxis(hAxisClone);
        ScaleAxisVisualizations(hAxisClone);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreateSPLOMS();
        }
    }
    /*
     * Doubles the scale of the visualizations attached to an axis
     */
    void ScaleAxisVisualizations(SAxis axis) {
        foreach (Visualization v in axis.GetComponent<SAxis>().correspondingVisualizations())
        {
            v.transform.localScale = new Vector3(v.transform.localScale.x * 2, v.transform.localScale.y * 2, v.transform.localScale.z * 2);
        }
    }

    /*
     * Keeps track of the axes loaded to the scene
     */
    public void AddAxis(SAxis axis)
    {
        sceneAxes.Add(axis);
        OnAxisAdded.Invoke(axis);
        //Debug.Log(sceneAxes.Count());
    }


    /*
     * Create Scatter Plot Matrices
     */
    void CreateSPLOMS()
    {
        print("creating the splom");
        SAxis[] axes = (SAxis[])GameObject.FindObjectsOfType(typeof(SAxis));

        GameObject a = axes[0].gameObject;// GameObject.Find("axis horesepower");
        a.transform.position = new Vector3(0f, 1.383f, 0.388f);

        Quaternion qt = new Quaternion();
        qt.eulerAngles = new Vector3(-90f, 180f, 0f);
        a.transform.rotation = qt;

        GameObject b = axes[1].gameObject;
        b.transform.position = new Vector3(0f, 1.506231f, 0.2461f);

        GameObject d = axes[2].gameObject;
        d.transform.position = new Vector3(0.1485f, 1.4145f, 0.2747f);
        qt.eulerAngles = new Vector3(0f, 180f, 90f);
        d.transform.rotation = qt;
    }

    void CreateLSPLOM()
    {
        Quaternion qt = new Quaternion();
        qt.eulerAngles = new Vector3(0f, 180f, 90f);

        GameObject a = GameObject.Find("axis horesepower");
        a.transform.position = new Vector3(0.1018f, 1.369f, -1.3629f);
        a.transform.rotation = qt;

        GameObject b = GameObject.Find("axis weight");
        b.transform.position = new Vector3(-0.04786599f, 1.506231f, -1.356f);

        GameObject c = GameObject.Find("axis mpg");
        c.transform.position = new Vector3(-0.045f, 1.768f, -1.357f);

        GameObject d = GameObject.Find("axis name");
        d.transform.position = new Vector3(-0.047f, 2.03f, -1.354f);

        qt.eulerAngles = new Vector3(0f, 180f, 90f);

        GameObject e = GameObject.Find("axis displacement");
        e.transform.position = new Vector3(0.37f, 1.378f, -1.37f);
        e.transform.rotation = qt;

    }

    void CreateSPLOMCenter()
    {
        Quaternion qt = new Quaternion();
        qt.eulerAngles = new Vector3(0f, 180f, -90f);

        GameObject c = GameObject.Find("axis mpg");
        c.transform.position = new Vector3(1.3173f, 1.7632f, -0.941f);

        GameObject d = GameObject.Find("axis weight");

        d.transform.position = new Vector3(1.173f, 1.6389f, -0.9362f);
        d.transform.rotation = qt;

        //        Quaternion qt = new Quaternion();

        GameObject e = GameObject.Find("axis displacement");

        e.transform.position = new Vector3(0.8942f, 1.64f, -0.938f);
        e.transform.rotation = qt;
    }

    void CreateSPLOMSWithU()
    {
        SAxis[] axes = (SAxis[])GameObject.FindObjectsOfType(typeof(SAxis));
        for (int i = 2; i < 8; ++i)
        {
            axes[i].transform.Translate(i % 2 * (axes[i].transform.localScale.x * 10f), i % 2 * (-axes[i].transform.localScale.x * 6.5f), 1f);
            axes[i].transform.Rotate(0f, 0f, i % 2 * (90f));
        }

        GameObject a = GameObject.Find("axis mpg");
        a.transform.position = new Vector3(0.236f, 1.506231f, -1.486f);
    }
}
