using System.Collections.Generic;
using UnityEngine;

namespace LogicLink.Plane;

public class PlaneManager
{
    public static PlaneManager Instance;

    public static readonly Vector3 PlaneScale = new(1000, 1000, 1000);
    public static readonly Vector3 PlaneRotation = new(0, 0, 90);


    LEV_Selection Selection;
    Color PlaneColor;

    public bool PlaneEnabled { get; private set; } = false;
    GameObject PlaneObject = null;
    Material PlaneMaterial = null;


    public PlaneManager(LEV_Selection selection, Color color)
    {
        Selection = selection;
        PlaneColor = color;
    }

    public void TogglePlane()
    {
        PlaneEnabled = !PlaneEnabled;

        if (!PlaneEnabled)
        {
            if (PlaneObject != null) UnityEngine.Object.Destroy(PlaneObject);
            return;
        }

        SetPlaneAtLastSelectedObject();
    }

    public void SelectionChanged()
    {
        if (!PlaneEnabled) return;
        SetPlaneAtLastSelectedObject();
    }

    public void ColorChanged(Color newColor)
    {
        PlaneColor = newColor;
        if (PlaneMaterial != null) PlaneMaterial.color = newColor;
    }

    public GameObject CreatePlaneObject()
    {
        GameObject planeContainer = new("LogicLink Plane Container");

        GameObject plane1 = CreateOneSidedPlane();
        GameObject plane2 = CreateOneSidedPlane();

        plane1.transform.SetParent(planeContainer.transform);
        plane1.transform.localEulerAngles = PlaneRotation;
        plane2.transform.SetParent(planeContainer.transform);
        plane2.transform.localEulerAngles = PlaneRotation + new Vector3(0, 0, 180);

        planeContainer.SetActive(false);

        return planeContainer;
    }

    GameObject CreateOneSidedPlane()
    {
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = "LogicLink Plane";
        plane.GetComponent<Collider>().enabled = false;

        MeshRenderer meshRenderer = plane.GetComponent<MeshRenderer>();

        if (PlaneMaterial == null) PlaneMaterial = CreatePlaneMaterial(meshRenderer.sharedMaterial);

        meshRenderer.material = PlaneMaterial;
        meshRenderer.sharedMaterial = PlaneMaterial;

        return plane;
    }

    public Material CreatePlaneMaterial(Material refrence)
    {
        Material material = new(refrence) { color = PlaneColor };
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        return material;
    }

    public void SetPlaneAtLastSelectedObject()
    {
        if (PlaneObject == null) PlaneObject = CreatePlaneObject();

        List<BlockProperties> list = Selection.list;
        if (list.Count == 0)
        {
            PlaneObject.SetActive(false);
            return;
        }

        Transform lastBlock = list[^1].transform;
        PlaneObject.SetActive(true);

        PlaneObject.transform.SetParent(lastBlock, false);
        Vector3 blockScale = lastBlock.localScale;
        PlaneObject.transform.localScale = new(
            PlaneScale.x / blockScale.x,
            PlaneScale.y / blockScale.y,
            PlaneScale.z / blockScale.z
        );
    }
}
