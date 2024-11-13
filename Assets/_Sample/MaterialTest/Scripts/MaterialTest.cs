using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sample
{
    public class MaterialTest : MonoBehaviour
    {
        Renderer cubeRenderer;
        MaterialPropertyBlock propertyBlock;

        // Start is called before the first frame update
        void Start()
        {
            cubeRenderer = GetComponent<Renderer>();

            //cubeRenderer.material.SetColor("_BaseColor", Color.red);
            //cubeRenderer.sharedMaterial.SetColor("_BaseColor", Color.red);

            //
            propertyBlock = new MaterialPropertyBlock();

            propertyBlock.SetColor("_BaseColor", Color.red);
            cubeRenderer.SetPropertyBlock(propertyBlock);
        }
    }

}
