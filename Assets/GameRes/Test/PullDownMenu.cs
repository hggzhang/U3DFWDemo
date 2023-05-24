using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PullDownMenu : MonoBehaviour
{
    public Toggle tog;
    public GameObject content;

    private void Awake()
    {

        tog.onValueChanged.AddListener((b) => { content.SetActive(b); });
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
